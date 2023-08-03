namespace UpgradeSubstrateTargetVersion
{
    using System.Text.RegularExpressions;

    public static class RegexUtil
    {
        public static string ToRegexString(this string pattern, bool noprefixSpace = false, bool nopostfixspace = false)
        {
            string matchSpace = @"\s*";

            pattern = Regex.Replace(pattern, @"\r\n\s+", matchSpace);

            if (noprefixSpace)
            {
                if (pattern.StartsWith(matchSpace))
                {
                    pattern = pattern.Substring(3);
                }
            }
            else
            {
                if (!pattern.StartsWith(matchSpace))
                {
                    pattern = matchSpace + pattern;
                }
            }

            if (nopostfixspace)
            {
                if (pattern.EndsWith(matchSpace))
                {
                    pattern = pattern.Substring(0, pattern.Length - 4);
                }
            }
            else
            {
                if (!pattern.EndsWith(matchSpace))
                {
                    pattern = pattern + matchSpace;
                }
            }

            var chars = new List<char>(pattern.ToCharArray());

            int i = 0;

            char[] regexChars = new char[] { '\\', '{', '}', '[', ']', '(', ')', '^', '$', '.', '|', '?', '*', '+', '-' };

            while (i < chars.Count - 2)
            {
                if (i <= chars.Count - 3 && chars[i] == '\\' && (chars[i + 1] == 's' || chars[i + 1] == 'w') && (chars[i + 2] == '*' || chars[i + 2] == '+'))
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
