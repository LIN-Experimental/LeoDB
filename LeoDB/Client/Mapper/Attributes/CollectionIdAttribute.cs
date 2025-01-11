namespace LeoDB;

/// <summary>
/// Indica que la propiedad se utilizará como BsonDocument Id.
/// </summary>
public class CollectionIdAttribute : Attribute
{
    public bool AutoId { get; private set; }

    public CollectionIdAttribute()
    {
        this.AutoId = true;
    }

    public CollectionIdAttribute(bool autoId)
    {
        this.AutoId = autoId;
    }
}