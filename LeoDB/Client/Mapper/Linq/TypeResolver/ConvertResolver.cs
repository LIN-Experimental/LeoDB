using System.Reflection;

namespace LeoDB
{
    internal class ConvertResolver : ITypeResolver
    {
        public string ResolveMethod(MethodInfo method)
        {
            return method.Name switch
            {
                "ToInt32" => "INT32(@0)",
                "ToInt64" => "INT64(@0)",
                "ToDouble" => "DOUBLE(@0)",
                "ToDecimal" => "DECIMAL(@0)",
                "ToDateTime" => "DATE(@0)",
                "FromBase64String" => "BINARY(@0)",
                "ToBoolean" => "BOOL(@0)",
                "ToString" => "STRING(@0)",
                _ => null,
            };
        }

        public string ResolveMember(MemberInfo member) => null;
        public string ResolveCtor(ConstructorInfo ctor) => null;
    }
}