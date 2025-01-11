using System;

namespace LeoDB;

/// <summary>
/// Indicar que la propiedad no se persistirá en la serialización.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class CollectionFieldIgnoreAttribute : Attribute
{
}