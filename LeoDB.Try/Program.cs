var database = new LeoDB.LeoDatabase("datpopkup2245.db");

var users = database.GetCollection<User>("users");
//users.Insert(new User() { Name = "John Doe", Age = 25 });

var ia = database.GetCollection<Intelligence>("$intelligence");
var mono = database.GetCollection<Mono>("mono");

//mono.Insert(new Mono() { Name = "Chango" });

//ia.Upsert(new Intelligence() { collection = "users", message = "El campo alias debe ser un apodo del campo Name" });

var ss2 = users.FindAll();

var sss = new Dictionary<string, object>();

foreach (var index in ia.FindAll())
{
    Console.WriteLine(index.collection + " - " + index.message);
}

users.EnsureIndex(x => x.Name, true);

users.Insert(new User() { Name = "Alexander", Age = 25, Alias = string.Empty });

foreach (var user in users.FindAll().ToList())
{
    Console.WriteLine(user.Name + " | " + user.Alias);
}


internal class User
{
    public string Name { get; set; }
    public string Alias { get; set; }
    public int Age { get; set; }
}

internal class Mono
{
    public string Name { get; set; }
}


internal class Intelligence
{
    public int id { get; set; }
    public string collection { get; set; }
    public string message { get; set; }
}