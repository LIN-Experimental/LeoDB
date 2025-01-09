﻿namespace LeoDB.Engine;

public partial class LeoEngine
{
    private void SysIntelligence()
    {
        const string collectionName = "$intelligence";

        // Colección es unica.
        _settings.Database.GetCollection<SysIntelligence>(collectionName).EnsureIndex(x => x.collection, true);

        foreach (var collection in _header.GetCollections())
        {
            // Validar si existe en la tabla.
            var exist = _settings.Database.GetCollection<SysIntelligence>(collectionName)
                                          .Exists(x => x.collection == collection.Key);

            // Si no existe, se crea el registro.
            if (!exist && !collection.Key.StartsWith("$"))
            {
                _settings.Database.GetCollection<SysIntelligence>(collectionName).Insert(new SysIntelligence
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
    public string collection { get; set; }
    public string message { get; set; }
}