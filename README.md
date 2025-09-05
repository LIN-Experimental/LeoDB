# LeoDB - Base de datos NoSQL preparada para todo.

LeoDB es una base de datos .NET NoSQL, rápida y ligera.
Funciona tanto embebida en aplicaciones locales como en modo servidor vía REST, lista para usarse en la nube.

- Almacén de documentos NoSQL sin servidor
- API simple, similar a MongoDB
- Código 100% C# para .NET Core.
- A prueba de hilos
- ACID con soporte completo de transacciones
- Recuperación de datos tras un fallo de escritura (archivo de registro WAL)
- Cifrado de archivos de datos utilizando criptografía DES (AES)
- Mapea tus clases POCO a `BsonDocument` usando atributos o API de mapeo fluido
- Almacenar archivos y datos de flujo (como GridFS en MongoDB)
- Almacenamiento de archivos de datos individuales (como SQLite)
- Indexar campos de documentos para búsquedas rápidas
- Soporte LINQ para consultas
- Comandos similares a SQL para acceder/transformar datos

## Nueva v1

- Nuevo motor de almacenamiento
- Sin bloqueos para operaciones de lectura (múltiples lectores)
- Bloqueos de escritura por colección (múltiples escritores)
- Colecciones internas/sistema 
- Nueva sintaxis similar a SQL
- Nuevo motor de consulta (admite proyección, ordenación, filtrado y consulta)
- Carga parcial de documentos (nivel raíz)

## ¿Como usarlo?

A quick example for storing and searching documents:

```C#
// Create your POCO class
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }
}

// Open database (or create if doesn't exist)
using(var db = new LeoDatabase(@"Data.db"))
{
    // Get customer collection
    var col = db.GetCollection<User>("users");

    // Create your new user instance
    var user = new User
    { 
        Name = "John Doe", 
        Age = 39
    };

    // Create unique index in Name field
    col.EnsureIndex(x => x.Name, true);

    // Insert new customer document (Id will be auto-incremented)
    col.Insert(user);

    // Update a document inside a collection
    user.Name = "Joana Doe";

    col.Update(customer);

    // Use LINQ to query documents (with no index)
    var results = col.Find(x => x.Age > 20);
}
```


## ¿Dónde utilizarlo?

- Escritorio/pequeñas aplicaciones locales
- Formato de archivo de aplicaciones
- Pequeños sitios web/aplicaciones
- Una base de datos **por cuenta/usuario** almacén de datos
- Puedes usar LeoDB en LIN Cloud (Servicio de LeoDB en la nube) para almacenar datos en la nube.


## License

[MIT](http://opensource.org/licenses/MIT)
