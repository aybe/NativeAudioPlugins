using System;
using System.Text.RegularExpressions;

namespace Wipeout
{
    public static class TypeExtensions
    {
        private const RegexOptions GetNiceNameRegexOptions = RegexOptions.Compiled | RegexOptions.Singleline;

        private static readonly Regex GetNiceNameRegex1 = new(@"`\d\[", GetNiceNameRegexOptions);

        private static readonly Regex GetNiceNameRegex2 = new(@"\]", GetNiceNameRegexOptions);

        private static readonly Regex GetNiceNameRegex3 = new(@"\w+\.", GetNiceNameRegexOptions);

        public static string GetNiceName(this Type type, bool full = false)
        {
            var name = type.ToString();

            name = GetNiceNameRegex1.Replace(name, "<");

            name = GetNiceNameRegex2.Replace(name, ">");

            if (full)
            {
                return name;
            }

            name = GetNiceNameRegex3.Replace(name, string.Empty);

            return name;
        }
    }
}