using System.Reflection;

namespace LeoDB
{
    internal class DateTimeResolver : ITypeResolver
    {
        public string ResolveMethod(MethodInfo method)
        {
            switch (method.Name)
            {
                // instance methods
                case "AddYears": return "DATEADD('y', @0, #)";
                case "AddMonths": return "DATEADD('M', @0, #)";
                case "AddDays": return "DATEADD('d', @0, #)";
                case "AddHours": return "DATEADD('h', @0, #)";
                case "AddMinutes": return "DATEADD('m', @0, #)";
                case "AddSeconds": return "DATEADD('s', @0, #)";
                case "ToString":
                    var pars = method.GetParameters();
                    if (pars.Length == 0) return "STRING(#)";
                    else if (pars.Length == 1 && pars[0].ParameterType == typeof(string)) return "FORMAT(#, @0)";
                    break;

                case "ToUniversalTime": return "TO_UTC(#)";
                // static methods
                case "Parse": return "DATETIME(@0)";
                case "Equals": return "# = @0";
            };

            return null;
        }

        public string ResolveMember(MemberInfo member)
        {
            return member.Name switch
            {
                // static properties
                "Now" => "NOW()",
                "UtcNow" => "NOW_UTC()",
                "Today" => "TODAY()",
                // instance properties
                "Year" => "YEAR(#)",
                "Month" => "MONTH(#)",
                "Day" => "DAY(#)",
                "Hour" => "HOUR(#)",
                "Minute" => "MINUTE(#)",
                "Second" => "SECOND(#)",
                "Date" => "DATETIME(YEAR(#), MONTH(#), DAY(#))",
                "ToLocalTime" => "TO_LOCAL(#)",
                "ToUniversalTime" => "TO_UTC(#)",
                _ => null,
            };
        }

        public string ResolveCtor(ConstructorInfo ctor)
        {
            var pars = ctor.GetParameters();

            if (pars.Length == 3)
            {
                // int year, int month, int day
                if (pars[0].ParameterType == typeof(int) && pars[1].ParameterType == typeof(int) && pars[2].ParameterType == typeof(int))
                {
                    return "DATETIME(@0, @1, @2)";
                }
            }

            return null;
        }
    }
}