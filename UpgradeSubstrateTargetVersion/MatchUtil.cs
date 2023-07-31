
namespace UpgradeSubstrateTargetVersion
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    public class MatchUtil
    {
        private string _where = null;
       
        public string Where
        {
            get
            {
                return string.IsNullOrEmpty(_where) ? When : _where;
            }
        }

        public int WhereOffSet { get; private set; }

        public string When { get; private set; }

        public string WhenNot { get; private set; }

        public string Path { get; private set; }

        public List<string> Values { get; private set; } = new();

        private static ConcurrentDictionary<string, Dictionary<string, List<string>>> cache = new();

        public static Dictionary<string, List<string>> GetValues(string path)
        {
            string pattern = @"<FilePath\s+path=""([^""]+)""\s*/>";

            Dictionary<string, List<string>> result = new();

            var lines = File.ReadAllLines(path);

            string key = null;
            foreach (var item in lines)
            {
                if (string.IsNullOrEmpty(item))
                {
                    continue;
                }

                Match match = Regex.Match(item, pattern);

                if (match.Success)
                {
                    string pathValue = match.Groups[1].Value;
                    key = pathValue;
                    result.Add(key, new());
                }
                else if (key != null)
                {
                    result[key].Add(item);
                }
            }

            return result;

        }

        public static List<string> GetInsertValuesByPath(string dataFile, string path = null)
        {
            path = path.Replace("\\", "/");
            lock (cache)
            {
                Dictionary<string, List<string>> keyValues = null;
                if (cache.ContainsKey(dataFile))
                {
                    keyValues = cache[dataFile];
                }
                else
                {
                    keyValues = GetValues(dataFile);
                    cache[dataFile] = keyValues;
                }

                if (path == null)
                {
                    return keyValues.ContainsKey("*") ? keyValues["*"] : null;
                }

                foreach (var item in keyValues.Keys)
                {
                    if (path.EndsWith(item, StringComparison.OrdinalIgnoreCase))
                    {
                        return keyValues[item];
                    }
                }
                return null;
            }
        }

        public MatchUtil()
        {
        }

        public MatchUtil(string when, string whenNot, string where, string path, List<string> values)
        {
            this._where = where;
            this.When = when;
            this.WhenNot = whenNot;
            this.Path = path;
            this.Values = values;
        }

        public static List<MatchUtil> GetMatchs(string path)
        {
            var lines = File.ReadAllLines(path);

            List<MatchUtil> matchUtils = new List<MatchUtil>();

            MatchUtil matchUtil = new();
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (line.Contains("WHEN:"))
                {
                    matchUtil = new MatchUtil();
                    matchUtils.Add(matchUtil);
                    matchUtil.When = line.Replace("WHEN:", "").Trim();
                }
                else if (line.Contains("NOT:"))
                {
                    matchUtil.WhenNot = line.Replace("WHENNOT:", "").Trim();
                }
                else if (line.Contains("WHERE:"))
                {
                    matchUtil._where = line.Replace("WHERE:", "").Trim();
                }
                else if (line.Contains("PATH:"))
                {
                    matchUtil.Path = line.Replace("PATH:", "").Trim();
                }
                else if (line.Contains("WHEREOFFSET:"))
                {
                    int.TryParse(line.Replace("WHEREOFFSET:", "").Trim(), out int offset);
                    matchUtil.WhereOffSet = offset;
                }
                else if (line.Contains("VALUES:"))
                {
                    continue;
                }
                else if (!string.IsNullOrEmpty(line))
                {
                    matchUtil.Values.Add(line);
                }
            }

            return matchUtils;
        }
    }
}
