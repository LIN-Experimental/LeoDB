#if NET10_0_OR_GREATER

using Infidex;
using Infidex.Api;
using Infidex.Core;
#endif
using LeoDB.Utils.Extensions;

using static LeoDB.Constants;

namespace LeoDB.Engine;

/// <summary>
/// Class that execute QueryPlan returing results
/// </summary>
internal class QueryExecutor
{
    private readonly LeoEngine _engine;
    private readonly EngineState _state;
    private readonly TransactionMonitor _monitor;
    private readonly SortDisk _sortDisk;
    private readonly DiskService _disk;
    private readonly EnginePragmas _pragmas;
    private readonly CursorInfo _cursor;
    private readonly string _collection;
    private readonly Query _query;
    private readonly IEnumerable<BsonDocument> _source;

    public QueryExecutor(
        LeoEngine engine,
        EngineState state,
        TransactionMonitor monitor,
        SortDisk sortDisk,
        DiskService disk,
        EnginePragmas pragmas,
        string collection,
        Query query,
        IEnumerable<BsonDocument> source)
    {
        _engine = engine;
        _state = state;
        _monitor = monitor;
        _sortDisk = sortDisk;
        _disk = disk;
        _pragmas = pragmas;
        _collection = collection;
        _query = query;

        _cursor = new CursorInfo(collection, query);

        LOG(_query.ToSQL(_collection).Replace(Environment.NewLine, " "), "QUERY");

        // source will be != null when query will run over external data source, like system collections or files (not user collection)
        _source = source;
    }

    public BsonDataReader ExecuteQuery()
    {
        if (_query.Into == null)
        {
            return this.ExecuteQuery(_query.ExplainPlan);
        }
        else
        {
            return this.ExecuteQueryInto(_query.Into, _query.IntoAutoId);
        }
    }

    /// <summary>
    /// Run query definition into engine. Execute optimization to get query planner
    /// </summary>
    internal BsonDataReader ExecuteQuery(bool executionPlan)
    {
        // get current transaction (if contains a explicit transaction) or a query-only transaction
        var transaction = _monitor.GetTransaction(true, true, out var isNew);

        transaction.OpenCursors.Add(_cursor);

        var enumerable = RunQuery();

        // Validar si es por busqueda binaria.
        if (_query.IsVectorial && !string.IsNullOrWhiteSpace(_query.VectorialValue))
        {
#if NET10_0_OR_GREATER
            var engine = SearchEngine.CreateDefault();
            var documents = enumerable.Select((r, i) => CreateInfidexDocument(i, r)).ToList();

            engine.IndexDocuments(documents);
            enumerable = SearchVectorial(engine, new Infidex.Api.Query(_query.VectorialValue), enumerable);
#endif
        }

        enumerable = enumerable.OnDispose(() => transaction.OpenCursors.Remove(_cursor));

        if (isNew)
        {
            enumerable = enumerable.OnDispose(() => _monitor.ReleaseTransaction(transaction));
        }

        // return new BsonDataReader with IEnumerable source
        return new BsonDataReader(enumerable, _collection, _state);

        IEnumerable<BsonDocument> RunQuery()
        {
            var snapshot = transaction.CreateSnapshot(_query.ForUpdate ? LockMode.Write : LockMode.Read, _collection, false);

            // no collection, no documents
            if (snapshot.CollectionPage == null && _source == null)
            {
                // if query use Source (*) need runs with empty data source
                if (_query.Select.UseSource)
                {
                    yield return _query.Select.ExecuteScalar(_pragmas.Collation).AsDocument;
                }

                yield break;
            }

            // execute optimization before run query (will fill missing _query properties instance)
            var optimizer = new QueryOptimization(snapshot, _query, _source, _pragmas.Collation);

            var queryPlan = optimizer.ProcessQuery();

            var plan = queryPlan.GetExecutionPlan();

            // if execution is just to get explan plan, return as single document result
            if (executionPlan)
            {
                yield return queryPlan.GetExecutionPlan();
                yield break;
            }

            // get node list from query - distinct by dataBlock (avoid duplicate)
            var nodes = queryPlan.Index.Run(snapshot.CollectionPage, new IndexService(snapshot, _pragmas.Collation, _disk.MAX_ITEMS_COUNT));

            // get current query pipe: normal or groupby pipe
            var pipe = queryPlan.GetPipe(transaction, snapshot, _sortDisk, _pragmas, _disk.MAX_ITEMS_COUNT);

            // start cursor elapsed timer which stops on dispose
            using var _ = _cursor.Elapsed.StartDisposable();

            using (var enumerator = pipe.Pipe(nodes, queryPlan).GetEnumerator())
            {
                var read = false;

                try
                {
                    read = enumerator.MoveNext();
                }
                catch (Exception ex)
                {
                    _state.Handle(ex);
                    throw;
                }

                while (read)
                {
                    _cursor.Fetched++;
                    _cursor.Elapsed.Stop();

                    yield return enumerator.Current;

                    if (transaction.State != TransactionState.Active) throw new LeoException(0, $"There is no more active transaction for this cursor: {_cursor.Query.ToSQL(_cursor.Collection)}");

                    _cursor.Elapsed.Start();

                    try
                    {
                        read = enumerator.MoveNext();
                    }
                    catch (Exception ex)
                    {
                        _state.Handle(ex);
                        throw;
                    }
                }
            }
        }
        ;
    }

    /// <summary>
    /// Execute query and insert result into another collection. Support external collections
    /// </summary>
    internal BsonDataReader ExecuteQueryInto(string into, BsonAutoId autoId)
    {
        IEnumerable<BsonDocument> GetResultset()
        {
            using (var reader = this.ExecuteQuery(false))
            {
                while (reader.Read())
                {
                    yield return reader.Current.AsDocument;
                }
            }
        }

        int result;

        // if collection starts with $ it's system collection
        if (into.StartsWith("$"))
        {
            SqlParser.ParseCollection(new Tokenizer(into), out var name, out var options);

            var sys = _engine.GetSystemCollection(name);

            result = sys.Output(GetResultset(), options);
        }
        // otherwise insert as normal collection
        else
        {
            result = _engine.Insert(into, GetResultset(), autoId);
        }

        return new BsonDataReader(result);
    }

#if NET10_0_OR_GREATER

    /// <summary>
    /// Buscar de forma vectorial.
    /// </summary>
    /// <param name="engine">Engine.</param>
    /// <param name="query">Filtrado por Infidex.</param>
    /// <param name="values">Valores.</param>
    private static List<BsonDocument> SearchVectorial(SearchEngine engine, Infidex.Api.Query query, IEnumerable<BsonDocument> values)
    {
        // Valores.
        List<BsonDocument> yield = [];

        // Realizar busqueda.
        var result = engine.Search(query);

        // Recorrer resultados.
        foreach (var hit in result.Records)
        {
            Document document = engine.GetDocument(hit.DocumentId);

            if (document is null)
                continue;

            // Id del documento en LeoDB.
            var id = document.Fields.GetField("_id")?.Value.ToString();

            // Obtener el valor.
            var val = values.Where(t => t.RawValue["_id"].RawValue.ToString() == id).FirstOrDefault();

            if (val is null)
                continue;

            yield.Add(val);
        }

        return yield;
    }

    /// <summary>
    /// Crear documento de Infidex.
    /// </summary>
    private static Document CreateInfidexDocument(long id, BsonDocument document)
    {
        DocumentFields documentFields = new();

        foreach (var e in document.GetElements())
        {
            documentFields.AddField(new Field()
            {
                Name = e.Key,
                Value = e.Value.RawValue.ToString(),
                Weight = Weight.High
            });
        }
        return new Document(id, documentFields);
    }

#endif

}