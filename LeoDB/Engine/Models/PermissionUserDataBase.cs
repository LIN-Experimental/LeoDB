namespace LeoDB.Engine.Models;

internal class PermissionUserDataBase
{
    [CollectionId]
    public Guid Id { get; set; }
    public int UserId { get; set; }
    public int PermissionId { get; set; }
}