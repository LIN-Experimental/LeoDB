namespace LeoDB.Engine;

public partial class LeoEngine
{

    /// <summary>
    /// Manejar y crear las colecciones del sistema almacenadas.
    /// </summary>
    public void ManageStoreCollection()
    {

        // Colecciones del sistema almacenadas.
        string[] collections = ["$intelligence", "$indexes", "$users", "$permissions", "$permissions_user"];

        foreach (var collection in collections)
        {
            switch (collection)
            {
                case "$intelligence":
                    SysIntelligence(collection);
                    break;
                case "$indexes":
                    SysIndexes(collection);
                    break;
                case "$users":
                    SysUsers(collection);
                    break;
                case "$permissions":
                    SysPermissions(collection);
                    break;
                case "$permissions_user":
                    SysPermissionsUser(collection);
                    break;
            }
        }
    }
}