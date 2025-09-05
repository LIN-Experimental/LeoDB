using System.Reflection;
using System.Threading.Tasks;
using static LeoDB.Constants;

namespace LeoDB.Engine
{
    public partial class LeoEngine
    {
        private IEnumerable<BsonDocument> SysDatabase()
        {
            var version = typeof(LeoEngine).GetTypeInfo().Assembly.GetName().Version;

            yield return new BsonDocument
            {
                ["name"] = _disk.GetName(FileOrigin.Data),
                ["encrypted"] = _settings.Password != null,
                ["readOnly"] = _settings.ReadOnly,

                ["lastPageID"] = (int)_header.LastPageID,
                ["freeEmptyPageID"] = (int)_header.FreeEmptyPageList,

                ["creationTime"] = _header.CreationTime,

                ["dataFileSize"] = (int)_disk.GetFileLength(FileOrigin.Data),
                ["logFileSize"] = (int)_disk.GetFileLength(FileOrigin.Log),

                ["currentReadVersion"] = _walIndex.CurrentReadVersion,
                ["lastTransactionID"] = _walIndex.LastTransactionID,
                ["engine"] = $"LeoDB-ce-v{version.Major}.{version.Minor}.{version.Build}",

                ["pragmas"] = new BsonDocument(_header.Pragmas.Pragmas.ToDictionary(x => x.Name, x => x.Get())),

                ["cache"] = new BsonDocument
                {
                    ["extendSegments"] = _disk.Cache.ExtendSegments,
                    ["extendPages"] = _disk.Cache.ExtendPages,
                    ["freePages"] = _disk.Cache.FreePages,
                    ["readablePages"] = _disk.Cache.GetPages().Count,
                    ["writablePages"] = _disk.Cache.WritablePages,
                    ["pagesInUse"] = _disk.Cache.PagesInUse,
                },

                ["transactions"] = new BsonDocument
                {
                    ["open"] = _monitor.Transactions.Count,
                    ["maxOpenTransactions"] = MAX_OPEN_TRANSACTIONS,
                    ["initialTransactionSize"] = _monitor.InitialSize,
                    ["availableSize"] = _monitor.FreePages
                }

            };
        }

        //private async Task<IEnumerable<BsonDocument>> SysDatabaseAPI()
        //{
        //    var cliente = new HttpClient() { BaseAddress = new Uri("https://jsonplaceholder.typicode.com/todos") };

        //    var ss = await cliente.GetAsync("");


        //    List<BsonDocument> docs = ToBsonDocuments(await ss.Content.ReadAsStringAsync());

        //    return docs;
        //}



        //public static List<BsonDocument> ToBsonDocuments(string json)
        //{
        //    using var jsonDoc = JsonDocument.Parse(json);

        //    if (jsonDoc.RootElement.ValueKind != JsonValueKind.Array)
        //        throw new ArgumentException("El JSON debe ser un array en la raíz");

        //    var list = new List<BsonDocument>();

        //    foreach (var element in jsonDoc.RootElement.EnumerateArray())
        //    {
        //        list.Add(JsonToBsonDocument(element));
        //    }

        //    return list;
        //}

        //private static BsonDocument JsonToBsonDocument(JsonElement obj)
        //{
        //    var doc = new BsonDocument();

        //    foreach (var prop in obj.EnumerateObject())
        //    {
        //        doc[prop.Name] = ConvertJsonElement(prop.Value);
        //    }

        //    return doc;
        //}

        //private static BsonValue ConvertJsonElement(JsonElement element)
        //{
        //    return element.ValueKind switch
        //    {
        //        JsonValueKind.Object => JsonToBsonDocument(element),
        //        JsonValueKind.Array => new BsonArray(element.EnumerateArray().Select(ConvertJsonElement)),
        //        JsonValueKind.String => element.GetString(),
        //        JsonValueKind.Number => element.TryGetInt64(out var l) ? new BsonValue(l) : new BsonValue(element.GetDouble()),
        //        JsonValueKind.Null => BsonValue.Null,
        //        _ => BsonValue.Null
        //    };
        //}













    }
}