namespace LeoDB.Engine;

/// <summary>
/// Implement a simple system collection with input data only (to use Output must inherit this class)
/// </summary>
internal class SystemCollection : SystemBaseCollection
{

    public SystemCollection(string name) : base(name)
    {
    }

    public SystemCollection(string name, Func<IEnumerable<BsonDocument>> input) : base(name, input)
    {
    }

    /// <summary>
    /// Get input data source factory
    /// </summary>
    public override IEnumerable<BsonDocument> Input(BsonValue options) => _input();
}