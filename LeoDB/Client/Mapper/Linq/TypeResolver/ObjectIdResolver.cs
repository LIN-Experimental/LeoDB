using System.Reflection;

namespace LeoDB
{
    internal class ObjectIdResolver : ITypeResolver
    {
        public string ResolveMethod(MethodInfo method)
        {
            switch (method.Name)
            {
                // instance methods
                case "ToString": return "STRING(#)";
                case "Equals": return "# = @0";
            };

            return null;
        }

        public string ResolveMember(MemberInfo member)
        {
            return member.Name switch
            {
                // static properties
                "Empty" => "OBJECTID('000000000000000000000000')",
                // instance properties
                "CreationTime" => "OID_CREATIONTIME(#)",
                _ => null,
            };
        }

        public string ResolveCtor(ConstructorInfo ctor)
        {
            var pars = ctor.GetParameters();

            if (pars.Length == 1)
            {
                // string value
                if (pars[0].ParameterType == typeof(string))
                {
                    return "OBJECTID(@0)";
                }
            }

            return null;
        }
    }
}