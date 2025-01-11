using System;

namespace LeoDB;

/// <summary>
/// Indica que el campo no se persiste dentro de este documento, sino que es una referencia para otro documento (DbRef)
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class CollectionReferenceAttribute : Attribute
{
    public string Collection { get; set; }

    public CollectionReferenceAttribute(string collection)
    {
        this.Collection = collection;
    }

    public CollectionReferenceAttribute()
    {
        this.Collection = null;
    }
}