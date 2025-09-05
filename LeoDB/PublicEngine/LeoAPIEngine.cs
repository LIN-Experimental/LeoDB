using LeoDB.Engine;
using LeoDB.Json;                 // <-- tus converters + extensions + settings
// using System.Text.Json;        // ❌ ya no lo necesitas aquí
// using System.Net.Http.Json;    // ❌ ya no lo necesitas aquí

namespace LeoDB.PublicEngine;

public class LeoApiEngine : ILeoEngine
{
    private readonly HttpClient _http;
    private EngineState _state;
    private readonly EngineSettings _settings;

    public LeoApiEngine(HttpClient httpClient, EngineSettings settings)
    {
        _http = httpClient;
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        this.Open();
    }

    // =======================
    // Helpers de manejo HTTP
    // =======================

    private void ThrowIfBadResponse4000(HttpResponseMessage response)
    {
        if ((int)response.StatusCode == 400)
        {
            BadResponse bad = null;

            try
            {
                // Newtonsoft + tus converters
                bad = response.Content.ReadFromJsonAsync<BadResponse>(LeoJsonSettings.Default).Result;
            }
            catch
            {
                // Si no se puede deserializar, sigue el flujo de excepción genérica
            }

            var lll = response.Content.ReadAsStringAsync().Result;
            var msg =  "Error de negocio (400) recibido desde el servidor.";
            throw new LeoException(0, msg);
        }
    }

    private T ReadOrThrow<T>(HttpResponseMessage response)
    {
        ThrowIfBadResponse4000(response);
        response.EnsureSuccessStatusCode();

        // Newtonsoft + tus converters
        return response.Content.ReadFromJsonAsync<T>(LeoJsonSettings.Default).Result!;
    }

    // =======================
    // Métodos públicos
    // =======================

    public int Checkpoint()
        => ReadOrThrow<int>(_http.PostAsJsonAsync("engine/checkpoint", new { }, LeoJsonSettings.Default).Result);

    public long Rebuild(RebuildOptions options)
        => ReadOrThrow<long>(_http.PostAsJsonAsync("engine/rebuild", options, LeoJsonSettings.Default).Result);

    public bool BeginTrans()
        => ReadOrThrow<bool>(_http.PostAsJsonAsync("engine/beginTrans", new { }, LeoJsonSettings.Default).Result);

    public bool Commit()
        => ReadOrThrow<bool>(_http.PostAsJsonAsync("engine/commit", new { }, LeoJsonSettings.Default).Result);

    public bool Rollback()
        => ReadOrThrow<bool>(_http.PostAsJsonAsync("engine/rollback", new { }, LeoJsonSettings.Default).Result);

    public IBsonDataReader Query(string collection, Query query)
    {
        var response = _http.PostAsJsonAsync(
            $"collections/{collection}/query",
            query,
            LeoJsonSettings.Default).Result;

        ThrowIfBadResponse4000(response);
        response.EnsureSuccessStatusCode();

        var docs = response.Content.ReadFromJsonAsync<List<BsonDocument>>(LeoJsonSettings.Default).Result!;
        return new BsonDataReader(_state, docs, collection);
    }

    public int Insert(string collection, IEnumerable<BsonDocument> docs, BsonAutoId autoId)
    {
        var response = _http.PostAsJsonAsync(
            $"collections/{collection}/insert?autoId={autoId}",
            docs,
            LeoJsonSettings.Default).Result;

        var result = ReadOrThrow<InsertReponse>(response);

        int i = 0;
        foreach (var doc in docs)
        {
            doc["_id"] = result.InsertedIds.ElementAt(i)["_id"];
            i++;
        }

        return result.InsertedCount;
    }

    public int Update(string collection, IEnumerable<BsonDocument> docs)
        => ReadOrThrow<int>(_http.PutAsJsonAsync($"collections/{collection}/update", docs, LeoJsonSettings.Default).Result);

    public int UpdateMany(string collection, BsonExpression transform, BsonExpression predicate)
        => ReadOrThrow<int>(_http.PostAsJsonAsync($"collections/{collection}/updateMany", new { transform, predicate }, LeoJsonSettings.Default).Result);

    public int InsertOrUpdate(string collection, IEnumerable<BsonDocument> docs, BsonAutoId autoId)
        => ReadOrThrow<int>(_http.PostAsJsonAsync($"collections/{collection}/upsert?autoId={autoId}", docs, LeoJsonSettings.Default).Result);

    public int Delete(string collection, IEnumerable<BsonValue> ids)
        => ReadOrThrow<int>(_http.PostAsJsonAsync($"collections/{collection}/delete", ids, LeoJsonSettings.Default).Result);

    public int DeleteMany(string collection, BsonExpression predicate)
        => ReadOrThrow<int>(_http.PostAsJsonAsync($"collections/{collection}/deleteMany", predicate, LeoJsonSettings.Default).Result);

    public bool DropCollection(string name)
        => ReadOrThrow<bool>(_http.DeleteAsync($"collections/{name}").Result);

    public bool RenameCollection(string name, string newName)
        => ReadOrThrow<bool>(_http.PostAsJsonAsync($"collections/{name}/rename", newName, LeoJsonSettings.Default).Result);

    public bool EnsureIndex(string collection, string name, BsonExpression expression, bool unique, bool save = true)
        => ReadOrThrow<bool>(_http.PostAsJsonAsync($"collections/{collection}/indexes", new { name, expression, unique, save }, LeoJsonSettings.Default).Result);

    public bool DropIndex(string collection, string name)
        => ReadOrThrow<bool>(_http.DeleteAsync($"collections/{name}/indexes/{name}").Result);

    public BsonValue Pragma(string name)
        => ReadOrThrow<BsonValue>(_http.GetAsync($"engine/pragma/{name}").Result);

    public bool Pragma(string name, BsonValue value)
        => ReadOrThrow<bool>(_http.PostAsJsonAsync($"engine/pragma/{name}", value, LeoJsonSettings.Default).Result);

    public void Dispose() => _http?.Dispose();

    internal bool Open()
    {
        _state = new EngineState(this, _settings);
        return true;
    }

    public void CloseEngine(Exception exception) => _state.Disposed = true;
}
