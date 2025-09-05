namespace LeoDB.Engine;

/// <summary>
/// Implementa una colección de sistema simple solo con datos de entrada (para usar Output debe heredar esta clase)
/// </summary>
internal abstract class SystemBaseCollection
{
    // Nombre de la colección del sistema (debe comenzar con $)
    protected readonly string _name;

    // Fábrica de fuente de datos de entrada
    protected readonly Func<IEnumerable<BsonDocument>> _input = null;

    public SystemBaseCollection(string name)
    {
        if (!name.StartsWith("$")) 
            throw new ArgumentException("System collection name must starts with $");
        _name = name;
    }

    public SystemBaseCollection(string name, Func<IEnumerable<BsonDocument>> input) : this(name)
    {
        _input = input;
    }

    /// <summary>
    /// Get system collection name (must starts with $)
    /// </summary>
    public string Name => _name;

    /// <summary>
    /// Get input data source factory
    /// </summary>
    public abstract IEnumerable<BsonDocument> Input(BsonValue options);

    /// <summary>
    /// Get output data source factory (must implement in inherit class)
    /// </summary>
    public virtual int Output(IEnumerable<BsonDocument> source, BsonValue options) => throw new LeoException(0, $"{_name} do not support as output collection");

    /// <summary>
    /// Static helper to read options arg as plain value or as document fields
    /// </summary>
    protected static BsonValue GetOption(BsonValue options, string key)
    {
        return GetOption(options, key, null);
    }

    /// <summary>
    /// Static helper to read options arg as plain value or as document fields
    /// </summary>
    protected static BsonValue GetOption(BsonValue options, string key, BsonValue defaultValue)
    {
        if (options != null && options.IsDocument)
        {
            if (options.AsDocument.TryGetValue(key, out var value))
            {
                if (defaultValue == null || value.Type == defaultValue.Type)
                {
                    return value;
                }
                else
                {
                    throw new LeoException(0, $"Parameter `{key}` expect {defaultValue.Type} value type");
                }
            }
            else
            {
                return defaultValue;
            }
        }
        else
        {
            return defaultValue == null ? options : defaultValue;
        }
    }
}