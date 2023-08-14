using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace UpgradeSubstrateTargetVersion
{
    public static class ListExtensions
    {
        public static bool Exists(this List<string> strings, string match, int startIndex, int endIndex)
        {
            for (int i = startIndex; i < endIndex; i++)
            {
                if (Regex.IsMatch(strings[i], match, RegexOptions.IgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool ContainIgnoreCase(this string value, List<String> strings)
        {
            return strings.Exists(e => value.Contains(e, StringComparison.OrdinalIgnoreCase));
        }

        public static bool EqualIgnoreCase(this string value, string other)
        {
            return value.Equals(other, StringComparison.OrdinalIgnoreCase);
        }
    }
}
