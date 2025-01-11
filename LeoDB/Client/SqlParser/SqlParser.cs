using LeoDB.Engine;
using static LeoDB.Constants;

namespace LeoDB;

/// <summary>
/// Internal class to parse and execute sql-like commands
/// </summary>
internal partial class SqlParser
{

    private readonly ILeoEngine _engine;
    private readonly Tokenizer _tokenizer;
    private readonly BsonDocument _parameters;
    private readonly Lazy<Collation> _collation;

    public SqlParser(ILeoEngine engine, Tokenizer tokenizer, BsonDocument parameters)
    {
        _engine = engine;
        _tokenizer = tokenizer;
        _parameters = parameters ?? new BsonDocument();
        _collation = new Lazy<Collation>(() => new Collation(_engine.Pragma(Pragmas.COLLATION)));
    }

    public IBsonDataReader Execute()
    {
        var ahead = _tokenizer.LookAhead().Expect(TokenType.Word);

        LOG($"executing `{ahead.Value.ToUpper()}`", "SQL");

        return ahead.Value.ToUpper() switch
        {
            "SELECT" or "EXPLAIN" => this.ParseSelect(),
            "INSERT" => this.ParseInsert(),
            "DELETE" => this.ParseDelete(),
            "UPDATE" => this.ParseUpdate(),
            "DROP" => this.ParseDrop(),
            "RENAME" => this.ParseRename(),
            "CREATE" => this.ParseCreate(),
            "CHECKPOINT" => this.ParseCheckpoint(),
            "REBUILD" => this.ParseRebuild(),
            "BEGIN" => this.ParseBegin(),
            "ROLLBACK" => this.ParseRollback(),
            "COMMIT" => this.ParseCommit(),
            "PRAGMA" => this.ParsePragma(),
            _ => throw LeoException.UnexpectedToken(ahead),
        };
    }
}