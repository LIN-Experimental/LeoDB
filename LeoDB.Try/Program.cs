var database = new LeoDB.LeoDatabase("datw2.db");

var users = database.GetCollection<User>("users");
//users.Insert(new User() { Name = "John Doe", Age = 25 });

var ia = database.GetCollection<Intelligence>("$intelligence");

//ia.Insert(new Intelligence() { collection = "users", name = "users", message = "El campo alias debe ser un apodo del campo Name" });

var ss2 = users.FindAll();

var sss = new Dictionary<string, object>();

foreach (var index in ia.FindAll())
{
    Console.WriteLine(index.name + " - " + index.message);
}

users.EnsureIndex(x => x.Name, true);

foreach (var user in users.Find(t => t.Age > 25))
{
    Console.WriteLine(user.Name);
}

foreach (var user in users.FindAll())
{
    Console.WriteLine(user.Name);
}


internal class User
{
    public string Name { get; set; }
    public string Alias { get; set; }
    public int Age { get; set; }
}

internal class Intelligence
{
    public string collection { get; set; }
    public string name { get; set; }
    public string message { get; set; }
}