using System.Reflection;

namespace LeoDB
{
    internal class NullableResolver : ITypeResolver
    {
        public string ResolveMethod(MethodInfo method)
        {
            return null;
        }

        public string ResolveMember(MemberInfo member)
        {
            return member.Name switch
            {
                "HasValue" => "(IS_NULL(#) = false)",
                "Value" => "#",
                _ => null,
            };
        }

        public string ResolveCtor(ConstructorInfo ctor) => null;
    }
}
