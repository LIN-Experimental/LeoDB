using LeoDB.Engine;

namespace LeoDB.Runtime;

internal class LeoRuntime
{
    public static void Generate(ILeoDatabase dataBase, ILeoEngine engine, BsonMapper mapper)
    {

        if (engine is not LeoEngine leoEngine)
        {
            return;
        }
        leoEngine.IsSettings = true;

        // Registrar las tablas almacenadas del sistema.
        leoEngine.RegisterStoredSystemCollections(new SystemStoreCollection("$intelligence"));
        leoEngine.RegisterStoredSystemCollections(new SystemStoreCollection("$indexes"));
        leoEngine.RegisterStoredSystemCollections(new SystemStoreCollection("$users"));
        leoEngine.RegisterStoredSystemCollections(new SystemStoreCollection("$permissions"));
        leoEngine.RegisterStoredSystemCollections(new SystemStoreCollection("$permissions_user"));

        // Manejar las colecciones almacenadas.
        leoEngine.ManageStoreCollection();

        foreach (var collection in leoEngine.GetCollectionNames())
        {

            // Obtener los campos.
            var collectionDb = dataBase.GetCollection(collection);

            // Obtener los campos de la colección.
            var reader = leoEngine.Query(collection, new Query());

            if (reader.Current is not BsonDocument bsDoc)
                continue;

            var entity = new EntityMapper();

            // Agregar la entidad.
            foreach (var key in bsDoc.Keys)
            {
                entity.Members.Add(new MemberMapper
                {
                    FieldName = key,
                    MemberName = key
                });

                // Validar si hay un registro de que es único.
                var result = dataBase.GetCollection<SysIndex>("$indexes").FindOne(t => t.field == key && t.collection == collection);

                if (result is null)
                {
                    continue;
                }

                engine.EnsureIndex(collection, key, key, true, false);
            }
        }



        // Validar la autorización.
        leoEngine.Authorize();

        leoEngine.IsSettings = false;
    }


}