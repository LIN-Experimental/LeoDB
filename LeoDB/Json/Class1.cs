       // BsonValue, BsonDocument, BsonArray, BsonExpression
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Text;

namespace LeoDB.Json
{

    public class InsertReponse
    {
        public int InsertedCount { get; set; }
        public IEnumerable<BsonDocument> InsertedIds { get; set; }
    }


    public static class HttpClientNewtonsoftJsonExtensions
    {
        public static Task<HttpResponseMessage> PostAsJsonAsync<T>(
            this HttpClient client,
            string requestUri,
            T value,
            JsonSerializerSettings settings,
            CancellationToken cancellationToken = default)
        {
            var json = JsonConvert.SerializeObject(value, settings);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            return client.PostAsync(requestUri, content, cancellationToken);
        }

        public static Task<HttpResponseMessage> PutAsJsonAsync<T>(
            this HttpClient client,
            string requestUri,
            T value,
            JsonSerializerSettings settings,
            CancellationToken cancellationToken = default)
        {
            var json = JsonConvert.SerializeObject(value, settings);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            return client.PutAsync(requestUri, content, cancellationToken);
        }

        public static async Task<T> ReadFromJsonAsync<T>(
            this HttpContent content,
            JsonSerializerSettings settings,
            CancellationToken cancellationToken = default)
        {
            // HttpContent no expone ReadAsStringAsync con token; simplificamos
            var json = await content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<T>(json, settings);
        }
    }
    public class BsonValueJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(BsonValue)
                || objectType == typeof(BsonDocument)
                || objectType == typeof(BsonArray);
        }

        public override object ReadJson(Newtonsoft.Json.JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            var token = JToken.Load(reader);
            var bson = Parse(token);

            if (objectType == typeof(BsonDocument)) return bson.AsDocument;
            if (objectType == typeof(BsonArray)) return bson.AsArray;
            return bson; // BsonValue
        }

        private static BsonValue Parse(JToken t)
        {
            switch (t.Type)
            {
                case JTokenType.Null:
                    return BsonValue.Null;

                case JTokenType.String:
                    return t.Value<string>();

                case JTokenType.Integer:
                    {
                        var l = t.Value<long>();
                        if (l >= int.MinValue && l <= int.MaxValue) return (int)l;
                        return l;
                    }

                case JTokenType.Float:
                    return t.Value<double>();

                case JTokenType.Boolean:
                    return t.Value<bool>();

                case JTokenType.Array:
                    {
                        var arr = new BsonArray();
                        foreach (var item in (JArray)t)
                            arr.Add(Parse(item));
                        return arr;
                    }

                case JTokenType.Object:
                    {

                        var obj = (JObject)t;

                        // Soporte {"$oid":"..."} -> LiteDB.ObjectId
                        if (obj.Count == 1 && obj.TryGetValue("$oid", out var oidTok) && oidTok.Type == JTokenType.String)
                        {
                            var s = oidTok.Value<string>();
                            if (TryParseLiteDbObjectId(s, out var oid))
                                return new BsonValue(oid);
                        }

                        var dict = new Dictionary<string, BsonValue>(StringComparer.Ordinal);
                        foreach (var p in obj.Properties())
                            dict[p.Name] = Parse(p.Value);
                        return new BsonDocument(dict);
                    }

                default:
                    throw new NotSupportedException($"Unsupported JSON token {t.Type}");
            }
        }

        // LiteDB no tiene TryParse; usamos el ctor y atrapamos formato inválido.
        private static bool TryParseLiteDbObjectId(string s, out ObjectId oid)
        {
            try { oid = new ObjectId(s); return true; }
            catch { oid = default; return false; }
        }

        public override void WriteJson(Newtonsoft.Json.JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        {
            // Acepta BsonValue, BsonDocument o BsonArray
            if (value is BsonDocument doc)
            {
                writer.WriteStartObject();
                foreach (var kv in doc) // IEnumerable<KeyValuePair<string,BsonValue>>
                {
                    writer.WritePropertyName(kv.Key);
                    WriteJson(writer, kv.Value, serializer);
                }
                writer.WriteEndObject();
                return;
            }

            if (value is BsonArray arr)
            {
                writer.WriteStartArray();
                foreach (var sv in arr.AsArray) 
                    WriteJson(writer, sv, serializer);
                writer.WriteEndArray();
                return;
            }

            var v = value as BsonValue ?? BsonValue.Null;

            switch (v.Type)
            {
                case BsonType.Null: writer.WriteNull(); break;
                case BsonType.String: writer.WriteValue(v.AsString); break;
                case BsonType.Int32: writer.WriteValue(v.AsInt32); break;
                case BsonType.Int64: writer.WriteValue(v.AsInt64); break;
                case BsonType.Double: writer.WriteValue(v.AsDouble); break;
                case BsonType.Boolean: writer.WriteValue(v.AsBoolean); break;
                case BsonType.DateTime: writer.WriteValue(v.AsDateTime.ToString("o")); break;

                case BsonType.Document:
                    writer.WriteStartObject();
                    foreach (var kv in v.AsDocument)
                    {
                        writer.WritePropertyName(kv.Key);
                        WriteJson(writer, kv.Value, serializer);
                    }
                    writer.WriteEndObject();
                    break;

                case BsonType.Array:
                    writer.WriteStartArray();
                    foreach (var item in v.AsArray)
                        WriteJson(writer, item, serializer);
                    writer.WriteEndArray();
                    break;

                case BsonType.ObjectId:
                    writer.WriteStartObject();
                    writer.WritePropertyName("$oid");
                    writer.WriteValue(v.AsObjectId.ToString());
                    writer.WriteEndObject();
                    break;

                default:
                    throw new NotSupportedException($"BsonType {v.Type} not supported");
            }
        }

        
    }

    public class BsonExpressionJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(BsonExpression);

        public override object ReadJson(Newtonsoft.Json.JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            if (reader.TokenType != JsonToken.String)
                throw new JsonSerializationException($"Expected string for BsonExpression, got {reader.TokenType}");

            var expr = (string)reader.Value;
            return expr == null ? null : BsonExpression.Create(expr);
        }

        public override void WriteJson(Newtonsoft.Json.JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        {
            var expr = (BsonExpression)value;
            writer.WriteValue(expr?.Source);
        }
    }

    public static class LeoJsonSettings
    {
        public static readonly JsonSerializerSettings Default = new JsonSerializerSettings
        {
            Formatting = Formatting.None,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters =
            {
                new BsonValueJsonConverter(),
                new BsonExpressionJsonConverter()
            },
            DateParseHandling = DateParseHandling.None
        };
    }
}
