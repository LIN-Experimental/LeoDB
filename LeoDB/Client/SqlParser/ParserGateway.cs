namespace LeoDB;

internal partial class SqlParser
{

    private BsonDataReader ParseCreate()
    {
        _tokenizer.ReadToken().Expect("CREATE");

        var ahead = _tokenizer.LookAhead().Expect(TokenType.Word);

        return ahead.Value.ToUpper() switch
        {
            "USER" => this.ParseCreateUser(),
            _ => this.ParseCreateIndex()
        };
    }

}