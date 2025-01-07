using System.Collections.Generic;

namespace LeoDB.Engine;

public partial class LiteEngine
{
    private IEnumerable<BsonDocument> SysIntelligence()
    {
        // get any transaction from current thread ID
        var transaction = _monitor.GetThreadTransaction();

        foreach (var collection in _header.GetCollections())
        {
            yield return new BsonDocument
            {
                ["collection"] = collection.Key,
                ["name"] = collection.Key,
                ["message"] = string.Empty
            };
        }
    }
}