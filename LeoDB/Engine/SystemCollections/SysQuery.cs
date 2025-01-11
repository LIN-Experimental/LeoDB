namespace LeoDB.Engine
{
    /// <summary>
    /// This class implement $query experimental system function to run sub-queries. It's experimental only - possible not be present in final release
    /// </summary>
    internal class SysQuery : SystemCollection
    {
        private readonly ILeoEngine _engine;

        public SysQuery(ILeoEngine engine) : base("$query")
        {
            _engine = engine;
        }

        public override IEnumerable<BsonDocument> Input(BsonValue options)
        {
            var query = options?.AsString ?? throw new LeoException(0, $"Collection $query(sql) requires `sql` string parameter");

            var sql = new SqlParser(_engine, new Tokenizer(query), null);

            using (var reader = sql.Execute())
            {
                while (reader.Read())
                {
                    var value = reader.Current;

                    yield return value.IsDocument ? value.AsDocument : new BsonDocument { ["expr"] = value };
                }
            }
        }
    }
}