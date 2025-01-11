namespace LeoDB;

/// <summary>
/// Asigna un nombre a esta propiedad en la colección.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class CollectionFieldAttribute : Attribute
{
    /// <summary>
    /// Nombre del campo.
    /// </summary>
    public string Name { get; set; }

    public CollectionFieldAttribute(string name)
    {
        this.Name = name;
    }

    public CollectionFieldAttribute()
    {
    }
}