using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace UpgradeSubstrateTargetVersion
{
    public static class RegexUtil
    {
        public static string ToRegexString(this string pattern)
        {
            pattern = Regex.Replace(pattern, @"\r\n\s+", @"\s+");

            var chars = new List<char>(pattern.ToCharArray());

            int i = 0;

            char[] regexChars = new char[] { '\\', '{', '}', '[', ']', '(', ')', '^', '$', '.', '|', '?', '*', '+', '-' };

            while (i < chars.Count - 2)
            {
                if (i < chars.Count - 3 && chars[i] == '\\' && chars[i + 1] == 's' && (chars[i + 2] == '*' || chars[i + 2] == '+'))
                {
                    i += 3;
                    continue;
                }

                if (regexChars.Contains(chars[i]))
                {
                    chars.Insert(i, '\\');
                    i += 2;
                }
                else if (chars[i] == ' ')
                {
                    while (chars[i] == ' ')
                    {
                        chars.RemoveAt(i);
                    }
                    chars.Insert(i, '+');
                    chars.Insert(i, 's');
                    chars.Insert(i, '\\');
                    i += 3;
                }
                else
                {
                    i++;
                }
            }

            return string.Concat(chars);
        }
    }
}
