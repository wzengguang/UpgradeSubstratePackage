
namespace UpgradeSubstrateTargetVersion
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Xml;
    using System.Xml.Linq;

    public class FileMatch
    {
        public string SkipFile { get; set; } = string.Empty;

        private XDocument xDoc;

        public int ModifiedCount { get; private set; }

        public string Path { get; private set; }

        public string Content { private set; get; }


        public static async Task<FileMatch> ReadFileAsync(string relativePath, bool isRelativePath = true)
        {
            string path = isRelativePath ? System.IO.Path.Combine(AppSettings.RootPath, relativePath) : relativePath;
            string strings = await File.ReadAllTextAsync(path);
            return new FileMatch(path, strings);
        }

        public List<MatchUtil> MatchUtils { set; get; }

        public FileMatch(string path, string txt)
        {
            this.Path = path;
            this.Content = txt;
            this.xDoc = XDocument.Parse(txt);
        }

        public void Replace(string match, string replace)
        {
            bool isMatch = Regex.IsMatch(Content, match, RegexOptions.IgnoreCase);
            if (isMatch)
            {
                string newContent = Regex.Replace(Content, match, replace, RegexOptions.IgnoreCase);
                Content = UpdateContent(newContent, replace);
            }
        }

        public bool IsMatchAll(params string[] whenMatchs)
        {
            foreach (var item in whenMatchs)
            {
                if (!Regex.IsMatch(Content, item, RegexOptions.IgnoreCase))
                {
                    return false;
                }
            }
            return true;
        }

        public bool IsMatchAny(params string[] whenMatchs)
        {
            var match = FirstMatch(whenMatchs);
            return match.Item1 == null ? false : true;
        }

        public (string, Match) FirstMatch(params string[] whenMatchs)
        {
            foreach (var item in whenMatchs)
            {
                var match = Regex.Match(Content, item, RegexOptions.IgnoreCase);

                if (match.Success)
                {
                    return (item, match);
                }
            }
            return (null, null);
        }

        private bool PreMatch(MatchParam matchParam)
        {
            if (matchParam.PathNot.Count != 0 && matchParam.PathNot.Exists(a => Path.Contains(a, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            if (matchParam.Path.Count != 0 && !matchParam.Path.Exists(a => Path.Contains(a, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            if (matchParam.IsWhenAll && !IsMatchAll(matchParam.When.ToArray()))
            {
                return false;
            }

            if (!matchParam.IsWhenAll && !IsMatchAny(matchParam.When.ToArray()))
            {
                return false;
            }

            if (matchParam.WhenNot.Count != 0)
            {
                foreach (var item in matchParam.WhenNot)
                {
                    if (Regex.IsMatch(Content, item, RegexOptions.IgnoreCase))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public void Replace(List<MatchParam> matchParams)
        {
            foreach (var matchParam in matchParams)
            {
                if (PreMatch(matchParam))
                {
                    string whereRegex = matchParam.When.First();
                    Match where = Regex.Match(Content, whereRegex, RegexOptions.IgnoreCase);
                    if (where.Success)
                    {
                        var vString = matchParam.value.Count == 0 ? "" : FormateSpaces(matchParam.value, where);

                        if (!string.IsNullOrEmpty(vString) && Regex.IsMatch(Content, vString.ToRegexString(), RegexOptions.IgnoreCase))
                        {
                            return;
                        }

                        string newContent = Regex.Replace(Content, whereRegex, m => vString, RegexOptions.IgnoreCase);
                        if (newContent.Contains(vString))
                        {
                            this.ModifiedCount++;
                            Content = newContent;
                        }
                    }
                }
            }
        }

        public void Insert(List<MatchParam> matchParams)
        {
            foreach (var item in matchParams)
            {
                Insert(item);
            }
        }

        public void Insert(MatchParam matchParam)
        {
            if (PreMatch(matchParam))
            {
                var match = FirstMatch(matchParam.When.ToArray());
                string whereStr = string.IsNullOrEmpty(matchParam.Where) ? match.Item1 : matchParam.Where;
                Match where = Regex.Match(Content, whereStr, RegexOptions.IgnoreCase);
                if (where.Success)
                {
                    var vString = FormateSpaces(matchParam.value, where);

                    if (Regex.IsMatch(Content, vString.ToRegexString(), RegexOptions.IgnoreCase))
                    {
                        return;
                    }

                    string newContent = Regex.Replace(Content, whereStr, m => vString + m.Value, RegexOptions.IgnoreCase);
                    if (newContent.Contains(vString))
                    {
                        this.ModifiedCount++;
                        Content = newContent;
                    }
                }
            }
        }

        public void InsertWhenNot(string whenNotMatch, IEnumerable<string> anyWhereMatchs, IList<string> values)
        {
            var match = Regex.Match(Content, whenNotMatch, RegexOptions.IgnoreCase);

            if (!match.Success)
            {
                foreach (var item in anyWhereMatchs)
                {
                    Match where = Regex.Match(Content, item, RegexOptions.IgnoreCase);
                    if (where.Success)
                    {
                        var vString = FormateSpaces(values, where);
                        string newContent = Regex.Replace(Content, item, m => vString + m.Value, RegexOptions.IgnoreCase);
                        Content = UpdateContent(newContent, vString, whenNotMatch);
                        return;
                    }
                }
            }
        }

        public void InsertWhenNot(string whenNotMatch, string whereMatch, IList<string> values)
        {
            var match = Regex.Match(Content, whenNotMatch, RegexOptions.IgnoreCase);

            if (!match.Success)
            {
                var where = Regex.Match(Content, whereMatch, RegexOptions.IgnoreCase);
                if (where.Success)
                {
                    var vString = FormateSpaces(values, where);
                    string newContent = Regex.Replace(Content, whereMatch, m => vString + m.Value, RegexOptions.IgnoreCase);
                    Content = UpdateContent(newContent, vString, whenNotMatch);
                }
            }
        }

        public void InsertWhenAndNot(string whenNotMatch, string whenMatch, IList<string> values)
        {
            var matchNot = Regex.Match(Content, whenNotMatch, RegexOptions.IgnoreCase);
            var matchWhen = Regex.Match(Content, whenMatch, RegexOptions.IgnoreCase);

            if (!matchNot.Success && matchWhen.Success)
            {
                var vString = FormateSpaces(values, matchWhen);
                string newContent = Regex.Replace(Content, whenMatch, m => vString + m.Value, RegexOptions.IgnoreCase);
                Content = UpdateContent(newContent, vString, whenNotMatch);
            }
        }

        public void InsertWhenAndNotNoFixSpaces(string whenNotMatch, string whenMatch, string value)
        {
            var matchNot = Regex.Match(Content, whenNotMatch, RegexOptions.IgnoreCase);
            var matchWhen = Regex.Match(Content, whenMatch, RegexOptions.IgnoreCase);

            if (!matchNot.Success && matchWhen.Success)
            {
                string newContent = Regex.Replace(Content, whenMatch, m => value + m.Value, RegexOptions.IgnoreCase);
                Content = UpdateContent(newContent, value, whenNotMatch);
            }
        }

        public void InsertWhenAndNotByGroup(MatchUtil MatchUtil)
        {
            InsertWhenAndNotByGroup(new List<MatchUtil> { MatchUtil });
        }

        public void InsertWhenAndNotByGroup(string whenMatch, string whenNot, string whereMatch, IList<string> values)
        {
            MatchUtil matchUtil = new MatchUtil(whenMatch, whenNot, whereMatch, ".*", values.ToList());

            InsertWhenAndNotByGroup(new List<MatchUtil> { matchUtil });
        }

        public void InsertWhenAndNotByGroup(List<MatchUtil> MatchUtils)
        {
            this.MatchUtils = MatchUtils;
            foreach (var match in this.MatchUtils)
            {
                if (Regex.IsMatch(this.Path, match.Path, RegexOptions.IgnoreCase))
                {
                    this.InsertWhenAndNotByGroupInternal(match.When, match.WhenNot, match.Where, match.Values);
                }
            }
        }

        public async Task<FileMatchResult> SaveResult()
        {
            if (this.ModifiedCount > 0)
            {
                await File.WriteAllTextAsync(this.Path, this.Content);
            }
            return FileMatchResult.CreateFileMatchResult(this);
        }

        private void InsertWhenAndNotByGroupInternal(string whenMatch, string whenNot, string whereMatch, IList<string> values)
        {
            if (values == null || values.Count == 0)
            {
                return;
            }

            var groups = Content.Split(@"<ItemGroup");

            for (int i = 0; i < groups.Length; i++)
            {
                string group = groups[i];

                Match matchWhen = Regex.Match(group, whenMatch, RegexOptions.IgnoreCase);
                if (matchWhen.Success && !Regex.IsMatch(group, whenNot, RegexOptions.IgnoreCase))
                {
                    var vString = FormateSpaces(values, matchWhen);
                    group = Regex.Replace(group, whereMatch, m => vString + m.Value, RegexOptions.IgnoreCase);
                    groups[i] = UpdateContent(group, vString, whenNot);
                }
            }

            Content = string.Join("<ItemGroup", groups);
        }

        private static string FormateSpaces(IList<string> values, Match matchWhen)
        {
            var copyedValues = new List<string>(values);
            string spaces = new string(matchWhen.Value.Replace(Environment.NewLine, "").TakeWhile(c => char.IsWhiteSpace(c)).ToArray());
            for (int i = copyedValues.Count - 1; i >= 0; i--)
            {
                copyedValues[i] = spaces + copyedValues[i];
            }

            return Environment.NewLine + string.Join(Environment.NewLine, copyedValues);
        }

        private string UpdateContent(string newContent, string value, string whenNotMatch = null)
        {
            bool isWhenNotMatch = whenNotMatch == null ? true : Regex.IsMatch(newContent, whenNotMatch, RegexOptions.IgnoreCase);
            if (isWhenNotMatch && newContent.Contains(value))
            {
                this.ModifiedCount++;
                return newContent;
            }

            return Content;
        }
    }
}
