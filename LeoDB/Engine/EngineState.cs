using System;
using System.IO;

using static LeoDB.Constants;

namespace LeoDB.Engine;

internal class EngineState
{
    public bool Disposed = false;
    private Exception _exception;
    private readonly LeoEngine _engine; // can be null for unit tests
    private readonly EngineSettings _settings;

#if DEBUG
    public Action<PageBuffer> SimulateDiskReadFail = null;
    public Action<PageBuffer> SimulateDiskWriteFail = null;
#endif

    public EngineState(LeoEngine engine, EngineSettings settings)
    {
        _engine = engine;
        _settings = settings;
    }

    public void Validate()
    {
        if (this.Disposed) throw _exception ?? LeoException.EngineDisposed();
    }

    public bool Handle(Exception ex)
    {
        LOG(ex.Message, "ERROR");

        if (ex is IOException ||
            (ex is LeoException lex && lex.ErrorCode == LeoException.INVALID_DATAFILE_STATE))
        {
            _exception = ex;

            _engine?.Close(ex);

            return false;
        }

        return true;
    }

    public BsonValue ReadTransform(string collection, BsonValue value)
    {
        if (_settings?.ReadTransform is null) return value;

        return _settings.ReadTransform(collection, value);
    }
}