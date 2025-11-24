namespace LeoDB.Engine;

public partial class LeoEngine
{
    /// <summary>
    /// Manejar y crear las colecciones del sistema almacenadas.
    /// </summary>
    public void ManageStoreCollection()
    {
        // Colecciones del sistema almacenadas.
        string[] collections = ["$indexes"];

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
            }
        }
    }
}