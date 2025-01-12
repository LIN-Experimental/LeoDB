using LeoDB.Engine.Models;

namespace LeoDB.Engine;

public partial class LeoEngine
{

    public List<int> UserPermissions { get; set; } = [];

    internal bool IsSettings = false;

    /// <summary>
    /// Validar la autorización general.
    /// </summary>
    internal void Authorize()
    {
        // Validar si hay usuarios.
        var query = Query("$users", new()).ToList();

        int userId = 0;
        if (query.Count <= 0)
        {
            // Modelo.
            var user = new UserDataBase()
            {
                Name = _settings.ContextUser,
                Password = _settings.UserPassword
            };

            // Crear el usuario.
            userId = _settings.Database.GetCollection<UserDataBase>("$users").Insert(user);

            // Insertar los roles.
            for (int i = 1; i <= 6; i++)
            {
                var rol = new PermissionUserDataBase()
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    PermissionId = i
                };

                Insert("$permissions_user", [_settings.Database.Mapper.ToDocument(rol)], BsonAutoId.Guid);
                UserPermissions.Add(i);
            }
            return;
        }

        // Si hay usuarios, validar si el usuario existe y la contraseña es correcta.
        var exist = query.Where(t => t["Name"] == _settings.ContextUser && t["Password"] == _settings.UserPassword).FirstOrDefault();

        if (exist is null)
        {
            throw LeoException.InvalidUserPassword(_settings.ContextUser);
        }

        // Cargar los permisos.
        var sss = Query("$permissions_user", new()).ToList();
        var userPermissions = Query("$permissions_user", new()).ToList().Where(t => t["UserId"] == exist["_id"]);

        UserPermissions = userPermissions.Select(t => (int)t["PermissionId"].RawValue).ToList();
    }


    internal bool IsAuthorize(int action)
    {
        return IsSettings || UserPermissions.Contains(action);
    }

}