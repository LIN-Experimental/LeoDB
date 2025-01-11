using System.Text;

namespace LeoDB.Engine;

internal class CollectionService
{
    private readonly HeaderPage _header;
    private readonly DiskService _disk;
    private readonly Snapshot _snapshot;
    private readonly TransactionPages _transPages;
    private readonly ILeoEngine _engine;

    public CollectionService(HeaderPage header, DiskService disk, Snapshot snapshot, TransactionPages transPages, ILeoEngine engine)
    {
        _header = header;
        _disk = disk;
        _snapshot = snapshot;
        _transPages = transPages;
        _engine = engine;
    }

    /// <summary>
    /// Check collection name if is valid (and fit on header)
    /// Throw correct message error if not valid name or not fit on header page
    /// </summary>
    public static void CheckName(string name, HeaderPage header, bool isFromSystem = false)
    {
        if (Encoding.UTF8.GetByteCount(name) > header.GetAvailableCollectionSpace())
            throw LeoException.InvalidCollectionName(name, "There is no space in header this collection name");

        if (!name.IsWord())
            throw LeoException.InvalidCollectionName(name, "Use only [a-Z$_]");

        if (name.StartsWith("$") && !isFromSystem)
            throw LeoException.InvalidCollectionName(name, "Collection can't starts with `$` (reserved for system collections)");
    }

    /// <summary>
    /// Get collection page instance (or create a new one). Returns true if a new collection was created
    /// </summary>
    public bool Get(string name, bool addIfNotExists, ref CollectionPage collectionPage)
    {
        // get collection pageID from header
        var pageID = _header.GetCollectionPageID(name);

        if (pageID != uint.MaxValue)
        {
            collectionPage = _snapshot.GetPage<CollectionPage>(pageID);
            return false;
        }
        else if (addIfNotExists)
        {
            this.Add(name, ref collectionPage);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Add a new collection. Check if name the not exists. Create only in transaction page - will update header only in commit
    /// </summary>
    private void Add(string name, ref CollectionPage collectionPage)
    {

        bool fromSystem = false;
        if (_engine is LeoEngine leo && name.StartsWith("$"))
        {
            fromSystem = leo.ExistSystemCollection(name);
        }

        // checks for collection name/size
        CheckName(name, _header, fromSystem);

        // create new collection page
        collectionPage = _snapshot.NewPage<CollectionPage>();
        var pageID = collectionPage.PageID;

        // insert collection name/pageID in header only in commit operation
        _transPages.Commit += (h) => h.InsertCollection(name, pageID);

        // create first index (_id pk) (must pass collectionPage because snapshot contains null in CollectionPage prop)
        var indexer = new IndexService(_snapshot, _header.Pragmas.Collation, _disk.MAX_ITEMS_COUNT);

        indexer.CreateIndex("_id", "$._id", true);
    }
}