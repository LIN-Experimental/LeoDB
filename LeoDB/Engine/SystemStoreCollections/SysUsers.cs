using LeoDB.Engine.Models;

namespace LeoDB.Engine;

public partial class LeoEngine
{

    const int PER_INSERT = 1;
    const int PER_READ = 2;
    const int PER_UPDATE = 3;
    const int PER_DELETE = 4;
    const int PER_STRUCTURE = 5;
    const int PER_TRANSACTION = 6;

    private void SysUsers(string name)
    {
        _settings.Database.GetCollection<UserDataBase>(name);
    }

    private void SysPermissions(string name)
    {
        // Colección es unica.
        _settings.Database.GetCollection<PermissionDataBase>(name);

        Dictionary<int, string> permissions = new Dictionary<int, string>()
        {
            { 1, "insert" },
            { 2, "read" },
            { 3, "update" },
            { 4, "delete" },
            { 5, "structure" },
            { 6, "transaction" }
        };

        foreach (var item in permissions)
        {
            // Validar si existe en la tabla.
            var exist = _settings.Database.GetCollection<PermissionDataBase>(name)
                                          .Exists(x => x.Name == item.Value);

            // Si no existe, se crea el registro.
            if (!exist)
            {
                _settings.Database.GetCollection<PermissionDataBase>(name).Insert(new PermissionDataBase
                {
                    Id = item.Key,
                    Name = item.Value
                });
            }
        }
    }

    private void SysPermissionsUser(string name)
    {
        // Colección es unica.
        _settings.Database.GetCollection<PermissionUserDataBase>(name);
    }
}