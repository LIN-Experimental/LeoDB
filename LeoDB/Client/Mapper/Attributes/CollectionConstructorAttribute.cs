namespace LeoDB;

/// <summary>
/// Indica qué método constructor se utilizará en esta entidad.
/// </summary>
[AttributeUsage(AttributeTargets.Constructor)]
public class CollectionConstructorAttribute : Attribute
{
}