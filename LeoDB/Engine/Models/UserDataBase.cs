namespace LeoDB.Engine.Models;

internal class UserDataBase
{
    [CollectionId(true)]
    public int Id { get; set; }
    [CollectionUnique]
    public string Name { get; set; }
    public string Password { get; set; }
}