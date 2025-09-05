namespace LeoDB;

internal partial class SqlParser
{
    /// <summary>
    /// CREATE USER {name} WITH {password}
    /// </summary>
    private BsonDataReader ParseCreateUser()
    {
        _tokenizer.ReadToken().Expect("CREATE");
        _tokenizer.ReadToken().Expect("USER");

        // nombre de usuario
        var username = _tokenizer.ReadToken().Expect(TokenType.Word).Value;

        _tokenizer.ReadToken().Expect("WITH");

        // password puede ser un string literal o número
        var pwdToken = _tokenizer.ReadToken();
        string password = pwdToken.Value;

        // terminar con ; o EOF
        _tokenizer.ReadToken().Expect(TokenType.EOF, TokenType.SemiColon);

        // Guardar en colección de sistema $users
        var doc = new BsonDocument
        {
            ["_id"] = username,
            ["password"] = password
        };

        // Usa InsertOrUpdate para evitar duplicados
        var result = _engine.InsertOrUpdate("$users", new[] { doc }, BsonAutoId.ObjectId);

        return new BsonDataReader(result);
    }
}
