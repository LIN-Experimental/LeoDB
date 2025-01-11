using System.Reflection;

namespace LeoDB
{
    internal class GuidResolver : ITypeResolver
    {
        public string ResolveMethod(MethodInfo method)
        {
            return method.Name switch
            {
                // instance methods
                "ToString" => "STRING(#)",
                // static methods
                "NewGuid" => "GUID()",
                "Parse" => "GUID(@0)",
                "TryParse" => throw new NotSupportedException("There is no TryParse translate. Use Guid.Parse()"),
                "Equals" => "# = @0",
                _ => null,
            };
        }

        public string ResolveMember(MemberInfo member)
        {
            return member.Name switch
            {
                // static properties
                "Empty" => "GUID('00000000-0000-0000-0000-000000000000')",
                _ => null,
            };
        }

        public string ResolveCtor(ConstructorInfo ctor)
        {
            var pars = ctor.GetParameters();

            if (pars.Length == 1)
            {
                // string s
                if (pars[0].ParameterType == typeof(string))
                {
                    return "GUID(@0)";
                }
            }

            return null;
        }
    }
}