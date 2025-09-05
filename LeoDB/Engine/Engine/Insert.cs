using static LeoDB.Constants;

namespace LeoDB.Engine;

public partial class LeoEngine
{
    /// <summary>
    /// Insert all documents in collection. If document has no _id, use AutoId generation.
    /// </summary>
    public int Insert(string collection, IEnumerable<BsonDocument> docs, BsonAutoId autoId)
    {
        if (collection.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(collection));

        if (docs == null)
            throw new ArgumentNullException(nameof(docs));

        return this.AutoTransaction(transaction =>
        {
            var snapshot = transaction.CreateSnapshot(LockMode.Write, collection, true);
            var count = 0;
            var indexer = new IndexService(snapshot, _header.Pragmas.Collation, _disk.MAX_ITEMS_COUNT);
            var data = new DataService(snapshot, _disk.MAX_ITEMS_COUNT);

            LOG($"insert `{collection}`", "COMMAND");

            foreach (var doc in docs)
            {
                _state.Validate();

                transaction.Safepoint();

                this.InsertDocument(snapshot, doc, autoId, indexer, data);

                count++;
            }

            return count;
        });
    }

    /// <summary>
    /// Internal implementation of insert a document
    /// </summary>
    /// <summary>
    /// Internal implementation of insert a document
    /// </summary>
    private void InsertDocument(Snapshot snapshot, BsonDocument doc, BsonAutoId autoId, IndexService indexer, DataService data)
    {
        // if no _id, use AutoId
        if (!doc.TryGetValue("_id", out var id))
        {
            doc["_id"] = id =
                autoId == BsonAutoId.ObjectId ? new BsonValue(ObjectId.NewObjectId()) :
                autoId == BsonAutoId.Guid ? new BsonValue(Guid.NewGuid()) :
                this.GetSequence(snapshot, autoId);
        }
        else if (id.IsNumber)
        {
            // update memory sequence of numeric _id
            this.SetSequence(snapshot, id);
        }

        // test if _id is a valid type
        if (id.IsNull || id.IsMinValue || id.IsMaxValue)
        {
            throw LeoException.InvalidDataType("_id", id);
        }

        // Ejecutar pipeline.
        _settings.PipelineRuntime?.ExecuteOnInsert(doc);

        // storage in data pages - returns dataBlock address
        var dataBlock = data.Insert(doc);

        IndexNode last = null;
        var indexes = snapshot.CollectionPage.GetCollectionIndexes();
        var collation = _header.Pragmas.Collation;

        // for each index, insert new IndexNode
        foreach (var index in indexes)
        {
            // for each index, get all keys (supports multi-key) - gets distinct values only
            // if index are unique, get single key only
            var keys = index.BsonExpr.GetIndexKeys(doc, collation);

            // do a loop with all keys (multi-key supported)
            foreach (var key in keys)
            {
                // insert node
                var node = indexer.AddNode(index, key, dataBlock, last);

                last = node;
            }
        }
    }

    private static string GetJsonType(BsonValue value)
    {
        return value.Type switch
        {
            BsonType.String => "string",
            BsonType.Int32 or BsonType.Int64 => "integer",
            BsonType.Double => "number",
            BsonType.Boolean => "boolean",
            BsonType.Array => "array",
            BsonType.Document => "object",
            _ => "string"
        };
    }
}