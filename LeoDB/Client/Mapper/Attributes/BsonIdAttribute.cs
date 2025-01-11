using System;

namespace LeoDB;

/// <summary>
/// Indica que la propiedad se utilizará como BsonDocument Id.
/// </summary>
public class BsonIdAttribute : Attribute
{
    public bool AutoId { get; private set; }

    public BsonIdAttribute()
    {
        this.AutoId = true;
    }

    public BsonIdAttribute(bool autoId)
    {
        this.AutoId = autoId;
    }
}