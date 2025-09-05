using LeoDB;

var database = new LeoDB.LeoDatabase("filename=ss42.db;connection=DIRECT");
var models = database.GetCollection<User2>("users");
var numbers = database.GetCollection<Numbers>("nums");

var lista = models.FindAll().ToList();

foreach (var user in lista)
{
    Console.WriteLine(user.Alias);
}

//numbers.EnsureIndex(t => t.Numero, true);

//numbers.Insert(new Numbers() { Numero = "123" });
//numbers.Insert(new Numbers() { Numero = "1235" });
//numbers.Insert(new Numbers() { Numero = "1235" });


var xx = models.DeleteAll();

database.BeginTransaction();

models.Insert(new User2() {Alias = "Primero", Age = 222 });
models.Insert(new User2() { Alias = "Ultimo Dos               -", Age = 222 });
models.Insert(new User2() { Alias = "Ultimo TREEW", Age = 222 });

database.Commit();

var readr = database.Execute("SELECT TRIM(Alias) as ali, Alias FROM users").ToArray();


Console.ReadLine();

var readr3 = database.Execute("RENAME COLLECTION users to fresas");

var readsr = database.Execute("SELECT SUM(*.Age) FROM users");
var readsr2 = database.Execute("SELECT * FROM fresas");

var rwwweadr = database.Execute("RENAME COLLECTION users to $indexes");



Console.ReadLine();
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


internal class Api
{
    public string title { get; set; }
}