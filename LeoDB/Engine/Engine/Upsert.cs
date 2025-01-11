using static LeoDB.Constants;

namespace LeoDB.Engine;

public partial class LeoEngine
{
    /// <summary>
    /// Implementa el comando upsert a los documentos de una colección. Llama update a todos los documentos,
    /// luego se intenta insertar cualquier documento no actualizado.
    /// Esto tendrá el efecto secundario de lanzar si se intentan insertar elementos duplicados.
    /// </summary>
    public int InsertOrUpdate(string collection, IEnumerable<BsonDocument> docs, BsonAutoId autoId)
    {
        if (collection.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(collection));
        if (docs == null) throw new ArgumentNullException(nameof(docs));

        return this.AutoTransaction(transaction =>
        {
            var snapshot = transaction.CreateSnapshot(LockMode.Write, collection, true);
            var collectionPage = snapshot.CollectionPage;
            var indexer = new IndexService(snapshot, _header.Pragmas.Collation, _disk.MAX_ITEMS_COUNT);
            var data = new DataService(snapshot, _disk.MAX_ITEMS_COUNT);
            var count = 0;

            LOG($"upsert `{collection}`", "COMMAND");

            foreach (var doc in docs)
            {
                _state.Validate();

                transaction.Safepoint();

                // first try update document (if exists _id), if not found, do insert
                if (doc["_id"] == BsonValue.Null || this.UpdateDocument(snapshot, collectionPage, doc, indexer, data) == false)
                {
                    this.InsertDocument(snapshot, doc, autoId, indexer, data);
                    count++;
                }
            }

            // returns how many document was inserted
            return count;
        });
    }
}