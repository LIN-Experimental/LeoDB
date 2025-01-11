using System;

namespace LeoDB;

/// <summary>
/// Indicar que la propiedad no se persistirá en la serialización.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class CollectionFieldIgnoreAttribute : Attribute
{
}