using LeoDB.Engine;

namespace LeoDB.Runtime;

internal class LeoRuntime
{
    public static void Generate(ILeoDatabase dataBase, ILeoEngine engine, BsonMapper mapper)
    {
        Console.WriteLine("Leo");
        engine.IsSettings = true;

        // Registrar las tablas almacenadas del sistema.
        engine.RegisterStoredSystemCollections(new SystemStoreCollection("$intelligence"));
        engine.RegisterStoredSystemCollections(new SystemStoreCollection("$indexes"));
        engine.RegisterStoredSystemCollections(new SystemStoreCollection("$users"));
        engine.RegisterStoredSystemCollections(new SystemStoreCollection("$permissions"));
        engine.RegisterStoredSystemCollections(new SystemStoreCollection("$permissions_user"));

        // Manejar las colecciones almacenadas.
        engine.ManageStoreCollection();

        foreach (var collection in engine.GetCollectionNames())
        {

            // Obtener los campos.
            var collectionDb = dataBase.GetCollection(collection);

            // Obtener los campos de la colección.
            var reader = engine.Query(collection, new Query());

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
        engine.Authorize();

        engine.IsSettings = false;
    }


}