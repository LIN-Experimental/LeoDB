using LeoDB;

//var database = new LeoDB.LeoDatabase("filename=ss42.db;connection=DIRECT");
var database = new LeoDB.LeoDatabase("filename=ss42.db;connection=REST");

var models = database.GetCollection<Movie>("movies");

if (models.Count() <= 0)
{
    models.Insert(
        new Movie()
        {
            Name = "Inception",
            Description = "Un ladrón especializado en infiltrarse en los sueños debe realizar una última misión: implantar una idea en la mente de un poderoso empresario."
        });

    models.Insert(
        new Movie()
        {
            Name = "Interstellar",
            Description = "Un grupo de exploradores viaja a través de un agujero de gusano en busca de un nuevo hogar para la humanidad."
        });

    models.Insert(
        new Movie()
        {
            Name = "El Señor de los Anillos: La Comunidad del Anillo",
            Description = "Un joven hobbit hereda un anillo con un poder oscuro y comienza un viaje épico para destruirlo antes de que caiga en manos del enemigo."
        });

    models.Insert(
        new Movie()
        {
            Name = "Gladiador",
            Description = "Un general romano traicionado busca venganza mientras lucha como gladiador en la arena del Imperio."
        });

    models.Insert(
        new Movie()
        {
            Name = "El Origen del Planeta de los Simios",
            Description = "Un experimento científico da lugar a una raza de simios con inteligencia avanzada que comienza a desafiar la supremacía humana."
        });

    models.Insert(
        new Movie()
        {
            Name = "La La Land",
            Description = "Dos artistas que buscan cumplir sus sueños en Los Ángeles se enamoran mientras persiguen sus metas personales."
        });

    models.Insert(
        new Movie()
        {
            Name = "El Pianista",
            Description = "La historia real de un músico judío-polaco que lucha por sobrevivir durante la Segunda Guerra Mundial."
        });

    models.Insert(
        new Movie()
        {
            Name = "Parásitos",
            Description = "Una familia pobre se infiltra en la vida de una familia rica, desencadenando una serie de eventos inesperados."
        });


}


var query = Query.All();

query.VectorialValue = "Buscar";

var movies = models.FindVectorial(query);

var lola = movies.ToList();
















Console.ReadLine();

return;


//var models = database.GetCollection<User2>("users");
//var numbers = database.GetCollection<Numbers>("nums");

//var readsr = database.Execute("SELECT  Alias FROM users");

//var lista = models.FindAll().ToList();

//foreach (var user in lista)
//{
//    Console.WriteLine(user.Alias);
//}

////numbers.EnsureIndex(t => t.Numero, true);

////numbers.Insert(new Numbers() { Numero = "123" });
////numbers.Insert(new Numbers() { Numero = "1235" });
////numbers.Insert(new Numbers() { Numero = "1235" });


//var xx = models.DeleteAll();

//database.BeginTransaction();

//models.Insert(new User2() { Alias = "Primero", Age = 222 });
//models.Insert(new User2() { Alias = "Ultimo Dos               -", Age = 222 });
//models.Insert(new User2() { Alias = "Ultimo TREEW", Age = 222 });

//database.Commit();

//var readr = database.Execute("SELECT TRIM(Alias) as ali, Alias FROM users").ToArray();


//Console.ReadLine();

//var readr3 = database.Execute("RENAME COLLECTION users to fresas");

//var readsr2 = database.Execute("SELECT * FROM fresas");

//var rwwweadr = database.Execute("RENAME COLLECTION users to $indexes");


//var vectResult = models.FindVectorial(Query.All());


//Console.ReadLine();
//var xx = database.Execute("select * from $api");







//var users = database.GetCollection<User2>("users");


////users.EnsureIndex(x => x.Age, true);

//users.Insert(new User2() { Alias = "John Doe", Age = 21 });



//foreach (var user in users.FindAll().ToList())
//{
//    Console.WriteLine(user.id + " | " + user.Alias);
//}


internal class Numbers
{
    [CollectionUnique()]
    public string Numero { get; set; }
}


internal class User2
{
    [CollectionId()]
    public int id { get; set; }
    public string Alias { get; set; }
    public int Age { get; set; }
}


internal class Movie
{
    [CollectionId()]
    public int id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}




internal class Api
{
    public string title { get; set; }
}