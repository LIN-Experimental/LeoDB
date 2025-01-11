using System.Reflection;

namespace LeoDB
{
    internal class RegexResolver : ITypeResolver
    {
        public string ResolveMethod(MethodInfo method)
        {
            return method.Name switch
            {
                "Split" => "SPLIT(@0, @1, true)",
                "IsMatch" => "IS_MATCH(@0, @1)",
                _ => null,
            };
        }

        public string ResolveMember(MemberInfo member) => null;
        public string ResolveCtor(ConstructorInfo ctor) => null;
    }
}