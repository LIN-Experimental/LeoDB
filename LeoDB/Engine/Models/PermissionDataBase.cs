namespace LeoDB.Engine.Models;

internal class PermissionDataBase
{
    [CollectionId(true)]
    public int Id { get; set; }
    public string Name { get; set; }
}