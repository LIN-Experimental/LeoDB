namespace LeoDB.Engine;

/// <summary>
/// Implementa una colección del sistema almacenada.
/// </summary>
internal class SystemStoreCollection(string name) : SystemBaseCollection(name)
{

    /// <summary>
    /// Get input data source factory
    /// </summary>
    public override IEnumerable<BsonDocument> Input(BsonValue options) => null;

}