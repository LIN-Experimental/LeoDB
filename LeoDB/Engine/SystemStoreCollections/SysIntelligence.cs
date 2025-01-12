﻿namespace LeoDB.Engine;

public partial class LeoEngine
{
    private void SysIntelligence(string name)
    {
        // Colección es unica.
        foreach (var collection in _header.GetCollections().ToList())
        {
            // Validar si existe en la tabla.
            var exist = _settings.Database.GetCollection<SysIntelligence>(name)
                                          .Exists(x => x.collection == collection.Key);

            // Si no existe, se crea el registro.
            if (!exist && !collection.Key.StartsWith("$"))
            {
                _settings.Database.GetCollection<SysIntelligence>(name).Insert(new SysIntelligence
                {
                    collection = collection.Key,
                    message = string.Empty
                });
            }
        }
    }

}

internal class SysIntelligence
{
    public int Id { get; set; }
    [CollectionUnique]
    public string collection { get; set; }
    public string message { get; set; }
}