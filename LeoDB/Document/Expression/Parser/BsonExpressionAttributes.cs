using System;

namespace LeoDB
{
    /// <summary>
    /// When a method are decorated with this attribute means that this method are not immutable
    /// </summary>
    internal class VolatileAttribute : Attribute
    {
    }
}
