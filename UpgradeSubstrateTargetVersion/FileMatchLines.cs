
namespace UpgradeSubstrateTargetVersion
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    public class FileMatchLines
    {
        public int NeedModifiedCount { get; private set; }

        public int ThisModifiedCount { get; private set; }

        public int MatchedCount { get; private set; }

        public string Path { get; private set; }

        public List<string> Lines { private set; get; }

        public List<MatchUtil> MatchUtils { set; get; }

        public FileMatchLines(string path, List<string> list)
        {
            this.Path = path;
            this.Lines = list;
        }

        public void ReplaceLine(string match, string replace)
        {
            int index = this.Lines.FindIndex(str => Regex.IsMatch(str, match, RegexOptions.IgnoreCase));
            if (index != -1)
            {
                string newLine = Regex.Replace(this.Lines[index], match, replace);
                this.Lines[index] = newLine;
                this.ThisModifiedCount++;
            }
        }

        public bool InsertWhenNot(string whenNotMatch, string whereMatch, IList<string> values)
        {
            bool match = !this.Lines.Exists(str => Regex.IsMatch(str, whenNotMatch, RegexOptions.IgnoreCase));
            if (match)
            {
                int index = this.Lines.FindIndex(str => Regex.IsMatch(str, whereMatch, RegexOptions.IgnoreCase));

                if (index != -1)
                {
                    for (int i = values.Count - 1; i >= 0; i--)
                    {
                        this.Lines.Insert(index, values[i]);
                    }

                    this.ThisModifiedCount++;
                }
            }

            return match;
        }

        public bool InsertWhenAndNot(string whenMatch, string whenNot, string whereMatch, IList<string> values, int insertOffSet = 0)
        {
            if (values == null || values.Count == 0)
            {
                return false;
            }

            bool match = this.Lines.Exists(str => Regex.IsMatch(str, whenMatch, RegexOptions.IgnoreCase));

            if (match && !this.Lines.Exists(str => Regex.IsMatch(str, whenNot, RegexOptions.IgnoreCase)))
            {
                int insertIndex = this.Lines.FindIndex(str => Regex.IsMatch(str, whereMatch, RegexOptions.IgnoreCase));

                if (insertIndex != -1)
                {
                    insertIndex = insertIndex + insertOffSet;

                    string spaces = new string(this.Lines[insertIndex].TakeWhile(c => char.IsWhiteSpace(c)).ToArray());

                    for (int i = values.Count - 1; i >= 0; i--)
                    {
                        this.Lines.Insert(insertIndex, spaces + values[i]);
                    }

                    this.ThisModifiedCount++;
                }
            }

            return match;
        }

        public void InsertWhenAndNotByGroup(List<MatchUtil> MatchUtils)
        {
            this.MatchUtils = MatchUtils;
            foreach (var match in this.MatchUtils)
            {
                if (Regex.IsMatch(this.Path, match.Path, RegexOptions.IgnoreCase))
                {
                    this.InsertWhenAndNotByGroup(match.When, match.WhenNot, match.Where, match.Values, match.WhereOffSet);
                }
            }
        }

        public void InsertWhenAndNotByGroup(string whenMatch, string whenNot, string whereMatch, IList<string> values, int insertOffSet = 0)
        {
            if (values == null || values.Count == 0)
            {
                return;
            }

            var groups = FindAllGroup(Lines);

            foreach (var group in groups)
            {
                bool match = this.Lines.Exists(whenMatch, group.Item1, group.Item2);
                if (match && !this.Lines.Exists(whenNot, group.Item1, group.Item2))
                {
                    int insertIndex = this.Lines.FindIndex(group.Item1, group.Item2 - group.Item1, str => Regex.IsMatch(str, whereMatch, RegexOptions.IgnoreCase));

                    if (insertIndex != -1)
                    {
                        insertIndex = insertIndex + insertOffSet;

                        string spaces = new string(this.Lines[insertIndex].TakeWhile(c => char.IsWhiteSpace(c)).ToArray());

                        for (int i = values.Count - 1; i >= 0; i--)
                        {
                            this.Lines.Insert(insertIndex, spaces + values[i]);
                        }

                        this.ThisModifiedCount++;
                    }
                }
            }
        }

        public async Task<bool> SaveAsync()
        {
            if (this.ThisModifiedCount > 0)
            {
                await File.WriteAllLinesAsync(this.Path, this.Lines);
            }

            if (this.MatchUtils != null && this.MatchUtils.Count > 0)
            {
                foreach (var item in this.MatchUtils)
                {
                    this.MatchedCount += this.Lines.Where(str => Regex.IsMatch(str, item.When, RegexOptions.IgnoreCase)).Count();
                    this.NeedModifiedCount += this.Lines.Where(str => Regex.IsMatch(str, item.WhenNot, RegexOptions.IgnoreCase)).Count();
                }
            }
            else
            {
                MatchedCount = this.Lines.Where(str => Regex.IsMatch(str, AppSettings.EventLog, RegexOptions.IgnoreCase)).Count();
                NeedModifiedCount = this.Lines.Where(str => Regex.IsMatch(str, AppSettings.DiagnosticsEventLog, RegexOptions.IgnoreCase)).Count();
            }

            return this.ThisModifiedCount > 0;
        }

        public async Task<FileMatchResult> SaveResult()
        {
            await SaveAsync();
            return FileMatchResult.CreateFileMatchResult(this);
        }

        public static async Task<FileMatchLines> ReadAllLinesAsync(string relativePath, bool isRelativePath = true)
        {
            string path = isRelativePath ? System.IO.Path.Combine(AppSettings.RootPath, relativePath) : relativePath;

            string[] strings = await File.ReadAllLinesAsync(path);

            return new FileMatchLines(path, strings.ToList());
        }

        public static List<(int, int)> FindAllGroup(List<string> list)
        {
            List<(int, int)> result = new();

            int groupStart = 0;
            int groupEnd = 0;

            for (int i = 0; i < list.Count; i++)
            {
                if (Regex.IsMatch(list[i], @"^\s*\<ItemGroup", RegexOptions.IgnoreCase))
                {
                    groupStart = i;
                }
                else if (Regex.IsMatch(list[i], @"^\s*\<\/ItemGroup", RegexOptions.IgnoreCase))
                {
                    groupEnd = i;
                    result.Insert(0, (groupStart, groupEnd));
                }
            }

            return result;
        }
    }
}
