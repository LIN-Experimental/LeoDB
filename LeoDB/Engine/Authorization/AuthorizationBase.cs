using LeoDB.Engine.Models;

namespace LeoDB.Engine;

public partial class LeoEngine
{

    public List<int> UserPermissions { get; set; } = [];

   public bool IsSettings { get; set; } = false;

    internal bool UsePermissions = true;

    /// <summary>
    /// Validar la autorización general.
    /// </summary>
    public void Authorize()
    {
        // Validar si hay usuarios.
        var query = Query("$users", new()).ToList();

        if (query.Count <= 0 && string.IsNullOrWhiteSpace(_settings.ContextUser))
        {
            UsePermissions = false;
            return;
        }

        UsePermissions = true;

        if (query.Count <= 0)
        {
            // Modelo.
            var user = new UserDataBase()
            {
                Name = _settings.ContextUser,
                Password = _settings.UserPassword
            };

            // Crear el usuario.
            int userId = _settings.Database.GetCollection<UserDataBase>("$users").Insert(user);

            // Insertar los roles.
            List<BsonDocument> documents = [];

            for (int i = 1; i <= 6; i++)
            {
                var rol = new PermissionUserDataBase()
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    PermissionId = i
                };

                documents.Add(_settings.Database.Mapper.ToDocument(rol));
                UserPermissions.Add(i);
            }

            Insert("$permissions_user", documents, BsonAutoId.Guid);
            return;
        }

        // Si hay usuarios, validar si el usuario existe y la contraseña es correcta.
        var exist = query.Where(t => t["Name"] == _settings.ContextUser && t["Password"] == _settings.UserPassword).FirstOrDefault() 
                    ?? throw LeoException.InvalidUserPassword(_settings.ContextUser);

        // Cargar los permisos.
        var userPermissions = Query("$permissions_user", new()).ToList().Where(t => t["UserId"] == exist["_id"]);

        UserPermissions = userPermissions.Select(t => (int)t["PermissionId"].RawValue).ToList();
    }


   public bool IsAuthorize(int action)
    {
        return !UsePermissions || IsSettings || UserPermissions.Contains(action);
    }

}