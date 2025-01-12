using LeoDB;

var database = new LeoDB.LeoDatabase("pruebaz1.db");

var users = database.GetCollection<User2>("users");

//users.EnsureIndex(x => x.Age, true);

users.Insert(new User2() { Alias = "John Doe", Age = 21 });



foreach (var user in users.FindAll().ToList())
{
    Console.WriteLine(user.id + " | " + user.Alias);
}


internal class User2
{
    [CollectionId()]
    public int id { get; set; }
    public string Alias { get; set; }
    public int Age { get; set; }
}