using LeoDB.Engine;

namespace LeoDB.Runtime;

internal class LeoRuntime
{
    public static void Generate(LeoEngine leoEngine)
    {

        // Registrar las tablas almacenadas del sistema.
        leoEngine.RegisterStoredSystemCollections(new SystemSavedCollection("$intelligence"));
        leoEngine.RegisterStoredSystemCollections(new SystemSavedCollection("$indexes"));

        // Manejar las colecciones almacenadas.
        leoEngine.ManageStoreCollection();

        foreach (var collection in leoEngine.GetCollectionNames())
        {
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

                // armar el predicado
                var predicate = Query.And(
                    Query.EQ("field", key),
                    Query.EQ("collection", collection)
                );

                // envolver en un Query
                var query = new Query { Select = predicate };

                // ejecutar con el engine
                var readeddr = leoEngine.Query("$indexes", query).FirstOrDefault();

                if (readeddr is not null && readeddr["field_collection"].RawValue is  bool bl && bl)
                {
                    leoEngine.EnsureIndex(collection, key, key, true, false);
                }
            }
        }
    }
}