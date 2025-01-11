using LeoDB.Utils;
using System.Collections.Concurrent;
using static LeoDB.Constants;

namespace LeoDB.Engine;

/// <summary>
/// Una clase pública que se encarga de todo el acceso a la estructura de datos del motor - es la implementación básica de una base de datos NoSql.
/// Está aislada de la solución completa - funciona sólo a bajo nivel (no linq, no poco... sólo objetos BSON)
/// [ThreadSafe]
/// </summary>
public partial class LeoEngine : ILeoEngine
{

    private LockService _locker;

    private DiskService _disk;

    private WalIndexService _walIndex;

    private HeaderPage _header;

    private TransactionMonitor _monitor;

    private SortDisk _sortDisk;

    private EngineState _state;

    /// <summary>
    /// immutable settings
    /// </summary>
    private readonly EngineSettings _settings;

    /// <summary>
    /// All system read-only collections for get metadata database information
    /// </summary>
    private Dictionary<string, SystemBaseCollection> _systemCollections { get; set; }

    /// <summary>
    /// Sequence cache for collections last ID (for int/long numbers only)
    /// </summary>
    private ConcurrentDictionary<string, long> _sequences;

    /// <summary>
    /// Initialize LeoEngine using initial engine settings
    /// </summary>
    public LeoEngine(EngineSettings settings)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        this.Open();
    }

    #region Open & Close

    public void OpenPersonalizeDatabase(BsonMapper mapper)
    {
        // Tabla de IA.
        SysIntelligence();
    }

    internal bool Open()
    {
        LOG($"start initializing{(_settings.ReadOnly ? " (readonly)" : "")}", "ENGINE");

        _systemCollections = new Dictionary<string, SystemBaseCollection>(StringComparer.OrdinalIgnoreCase);
        _sequences = new ConcurrentDictionary<string, long>(StringComparer.OrdinalIgnoreCase);

        try
        {
            // initialize engine state 
            _state = new EngineState(this, _settings);

            // before initilize, try if must be upgrade
            if (_settings.Upgrade) this.TryUpgrade();

            // initialize disk service (will create database if needed)
            _disk = new DiskService(_settings, _state, MEMORY_SEGMENT_SIZES);

            // read page with no cache ref (has a own PageBuffer) - do not Release() support
            var buffer = _disk.ReadFull(FileOrigin.Data).First();

            // if first byte are 1 this datafile are encrypted but has do defined password to open
            if (buffer[0] == 1) throw new LeoException(0, "This data file is encrypted and needs a password to open");

            // read header database page
            _header = new HeaderPage(buffer);

            // if database is set to invalid state, need rebuild
            if (buffer[HeaderPage.P_INVALID_DATAFILE_STATE] != 0 && _settings.AutoRebuild)
            {
                // dispose disk access to rebuild process
                _disk.Dispose();
                _disk = null;

                // rebuild database, create -backup file and include _rebuild_errors collection
                this.Recovery(_header.Pragmas.Collation);

                // re-initialize disk service
                _disk = new DiskService(_settings, _state, MEMORY_SEGMENT_SIZES);

                // read buffer header page again
                buffer = _disk.ReadFull(FileOrigin.Data).First();

                _header = new HeaderPage(buffer);
            }

            // test for same collation
            if (_settings.Collation != null && _settings.Collation.ToString() != _header.Pragmas.Collation.ToString())
            {
                throw new LeoException(0, $"Datafile collation '{_header.Pragmas.Collation}' is different from engine settings. Use Rebuild database to change collation.");
            }

            // initialize locker service
            _locker = new LockService(_header.Pragmas);

            // initialize wal-index service
            _walIndex = new WalIndexService(_disk, _locker);

            // if exists log file, restore wal index references (can update full _header instance)
            if (_disk.GetFileLength(FileOrigin.Log) > 0)
            {
                _walIndex.RestoreIndex(ref _header);
            }

            // initialize sort temp disk
            _sortDisk = new SortDisk(_settings.CreateTempFactory(), CONTAINER_SORT_SIZE, _header.Pragmas);

            // initialize transaction monitor as last service
            _monitor = new TransactionMonitor(_header, _locker, _disk, _walIndex, this);

            // register system collections
            this.InitializeSystemCollections();

            _systemCollections.Add("$intelligence", new SystemSavedCollection("$intelligence"));

            LOG("initialization completed", "ENGINE");

            return true;
        }
        catch (Exception ex)
        {
            LOG(ex.Message, "ERROR");

            this.Close(ex);
            throw;
        }
    }


    /// <summary>
    /// Normal close process:
    /// - Stop any new transaction
    /// - Stop operation loops over database (throw in SafePoint)
    /// - Wait for writer queue
    /// - Close disks
    /// - Clean variables
    /// </summary>
    internal List<Exception> Close()
    {
        if (_state.Disposed) return new List<Exception>();

        _state.Disposed = true;

        var tc = new TryCatch();

        // stop running all transactions
        tc.Catch(() => _monitor?.Dispose());

        if (_header?.Pragmas.Checkpoint > 0)
        {
            // do a soft checkpoint (only if exclusive lock is possible)
            tc.Catch(() => _walIndex?.TryCheckpoint());
        }

        // close all disk streams (and delete log if empty)
        tc.Catch(() => _disk?.Dispose());

        // delete sort temp file
        tc.Catch(() => _sortDisk?.Dispose());

        // dispose lockers
        tc.Catch(() => _locker?.Dispose());

        return tc.Exceptions;
    }

    /// <summary>
    /// Exception close database:
    /// - Stop diskQueue
    /// - Stop any disk read/write (dispose)
    /// - Dispose sort disk
    /// - Dispose locker
    /// - Checks Exception type for INVALID_DATAFILE_STATE to auto rebuild on open
    /// </summary>
    internal List<Exception> Close(Exception ex)
    {
        if (_state.Disposed) return new List<Exception>();

        _state.Disposed = true;

        var tc = new TryCatch(ex);

        tc.Catch(() => _monitor?.Dispose());

        // close disks streams
        tc.Catch(() => _disk?.Dispose());

        // close sort disk service
        tc.Catch(() => _sortDisk?.Dispose());

        // close engine lock service
        tc.Catch(() => _locker?.Dispose());

        if (tc.InvalidDatafileState)
        {
            // mark byte = 1 in HeaderPage.P_INVALID_DATAFILE_STATE - will open in auto-rebuild
            // this method will throw no errors
            tc.Catch(() => _disk.MarkAsInvalidState());
        }

        return tc.Exceptions;
    }

    #endregion



    /// <summary>
    /// Run checkpoint command to copy log file into data file
    /// </summary>
    public int Checkpoint() => _walIndex.Checkpoint();

    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        this.Close();
    }
}