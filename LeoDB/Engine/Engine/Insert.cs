using LIN.Access.OpenIA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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

            // Validar si es desde el Engine.
            var information = _settings.Database.GetCollection<SysIntelligence>("$intelligence").FindOne(t => t.collection == collection);

            foreach (var doc in docs)
            {
                _state.Validate();

                // Ejecutar IA.
                if (!string.IsNullOrWhiteSpace(information?.message ?? null))
                {
                    LIN.Access.OpenIA.IAModelBuilder model = new();
                    model.Schema = GenerateSchema(doc);
                    model.Load(Message.FromSystem($"Eres una IA integrada en una base de datos: {information.message}"));
                    model.Load(Message.FromUser($"El documento JSON es: {doc}"));

                    var ss = model.Reply();
                    ss.Wait();
                    ReplaceValuesWithJson(doc, ss.Result.Content);
                }

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

        // storage in data pages - returns dataBlock address
        var dataBlock = data.Insert(doc);

        IndexNode last = null;

        // for each index, insert new IndexNode
        foreach (var index in snapshot.CollectionPage.GetCollectionIndexes())
        {
            // for each index, get all keys (supports multi-key) - gets distinct values only
            // if index are unique, get single key only
            var keys = index.BsonExpr.GetIndexKeys(doc, _header.Pragmas.Collation);

            // do a loop with all keys (multi-key supported)
            foreach (var key in keys)
            {
                // insert node
                var node = indexer.AddNode(index, key, dataBlock, last);

                last = node;
            }
        }
    }








    public static string GenerateSchema(BsonDocument bsonDocument)
    {
        var properties = new Dictionary<string, object>();

        foreach (var element in bsonDocument)
        {
            string propertyType = GetJsonType(element.Value);

            if (!string.IsNullOrEmpty(propertyType))
            {
                properties[element.Key] = new
                {
                    type = propertyType,
                    description = $"Auto-generated schema for {element.Key}"
                };
            }
        }

        var schema = new
        {
            type = "json_schema",
            json_schema = new
            {
                name = "LIN_IA",
                strict = true,
                schema = new
                {
                    type = "object",
                    properties = properties,
                    required = bsonDocument.Select(t => t.Key),
                    additionalProperties = false
                }
            }
        };

        return System.Text.Json.JsonSerializer.Serialize(schema, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
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


    public static BsonDocument ReplaceValuesWithJson(BsonDocument bsonDocument, string json)
    {
        var jsonData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(json);

        foreach (var key in jsonData.Keys)
        {
            if (bsonDocument.ContainsKey(key))
            {

                if (jsonData[key] is System.Text.Json.JsonElement je)
                {
                    bsonDocument[key] = ConvertToBsonValue(je);
                }
                else
                {
                    bsonDocument[key] = new BsonValue(bsonDocument[key].Type, jsonData[key]);
                }


            }
        }

        return bsonDocument;
    }

    private static BsonValue ConvertToBsonValue(System.Text.Json.JsonElement element)
    {
        return element.ValueKind switch
        {
            System.Text.Json.JsonValueKind.String => new BsonValue(element.GetString()),
            System.Text.Json.JsonValueKind.Number => element.TryGetInt32(out var intValue) ? new BsonValue(intValue) : new BsonValue(element.GetDouble()),
            System.Text.Json.JsonValueKind.True => new BsonValue(true),
            System.Text.Json.JsonValueKind.False => new BsonValue(false),
            System.Text.Json.JsonValueKind.Array => new BsonArray(element.EnumerateArray().Select(ConvertToBsonValue)),
            System.Text.Json.JsonValueKind.Object => new BsonDocument(element.EnumerateObject().ToDictionary(p => p.Name, p => ConvertToBsonValue(p.Value))),
            _ => new BsonValue()
        };
    }
}