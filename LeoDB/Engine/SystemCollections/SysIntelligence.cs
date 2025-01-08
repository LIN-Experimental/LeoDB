using System.Collections.Generic;

namespace LeoDB.Engine;

public partial class LeoEngine
{
    private IEnumerable<BsonDocument> SysIntelligence(BsonMapper mapper)
    {
        // get any transaction from current thread ID
        var transaction = _monitor.GetThreadTransaction();

        var ss = new LiteCollection<SysIntelligence>("$intelligence", BsonAutoId.Guid, this, mapper);

        var ssa = ss.FindAll().GetEnumerator();




        foreach (var collection in _header.GetCollections())
        {
            yield return new BsonDocument
            {
                ["collection"] = collection.Key,
                ["name"] = collection.Key,
                ["message"] = ss.FindOne(t => t.collection == collection.Key)?.message
            };
        }
    }

}

file class SysIntelligence
{
    public string collection { get; set; }
    public string name { get; set; }
    public string message { get; set; }
}