using System;
using System.Text;

namespace Wipeout
{
    public static class TypeExtensions
    {
        public static string GetRealTypeName(this Type t)
        {
            if (!t.IsGenericType)
                return t.Name;

            var sb = new StringBuilder();
            sb.Append(t.Name[..t.Name.IndexOf('`')]);
            sb.Append('<');
            var appendComma = false;
            foreach (var arg in t.GetGenericArguments())
            {
                if (appendComma) sb.Append(',');
                sb.Append(GetRealTypeName(arg));
                appendComma = true;
            }

            sb.Append('>');
            return sb.ToString();
        }
    }
}