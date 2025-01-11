using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace LeoDB
{
    internal static class TypeInfoExtensions
    {
        public static bool IsAnonymousType(this Type type)
        {
            bool isAnonymousType =
                type.FullName.Contains("AnonymousType") &&
                type.GetTypeInfo().GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Any();

            return isAnonymousType;
        }

        public static bool IsEnumerable(this Type type)
        {
            return
                type != typeof(String) &&
                typeof(IEnumerable).IsAssignableFrom(type);
        }
    }
}
