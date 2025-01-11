namespace LeoDB.Engine;

public partial class LeoEngine
{

    internal bool ExistSystemCollection(string name)
    {
        // Si es una tabla del sistema calculada.
        return _systemCollections.TryGetValue(name, out _);
    }


    /// <summary>
    /// Get registered system collection
    /// </summary>
    internal SystemBaseCollection GetSystemCollection(string name)
    {
        // Si es una tabla del sistema calculada.
        if (_systemCollections.TryGetValue(name, out var sys))
        {
            return sys;
        }

        throw new LeoException(0, $"System collection '{name}' are not registered as system collection");
    }

    /// <summary>
    /// Register a new system collection that can be used in query for input/output data
    /// Collection name must starts with $
    /// </summary>
    internal void RegisterSystemCollection(SystemCollection systemCollection)
    {
        if (systemCollection == null)
            throw new ArgumentNullException(nameof(systemCollection));

        _systemCollections[systemCollection.Name] = systemCollection;
    }

    /// <summary>
    /// Register a new system collection that can be used in query for input data
    /// Collection name must starts with $
    /// </summary>
    internal void RegisterSystemCollection(string collectionName, Func<IEnumerable<BsonDocument>> factory)
    {
        if (collectionName.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(collectionName));
        if (factory == null)
            throw new ArgumentNullException(nameof(factory));

        _systemCollections[collectionName] = new SystemCollection(collectionName, factory);
    }
}