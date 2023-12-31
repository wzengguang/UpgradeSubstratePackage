﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace UpgradeSubstrateTargetVersion
{
    public class FileMatchResult
    {
        public int TotalModifiedCount { get; private set; }

        public string Path { get; private set; }

        public int MatchedCount { get; private set; }

        public int ModifiedCount { get; private set; }

        public int EventLogCount { get; private set; }

        public int PortableCount { get; private set; }

        public int DiagnosticsCount { get; private set; }
        public int PerseusCount { get; private set; }

        public string Skip { get; private set; }

        public static FileMatchResult CreateFileMatchResult(FileMatch fileUtil)
        {
            int eventLogCount = Regex.Matches(fileUtil.Content, AppSettings.EventLog, RegexOptions.IgnoreCase).Count;
            int perseusCount = Regex.Matches(fileUtil.Content, AppSettings.Perseus, RegexOptions.IgnoreCase).Count;
            int portableCount = Regex.Matches(fileUtil.Content, AppSettings.Portable, RegexOptions.IgnoreCase).Count;
            int diagnosticsCount = Regex.Matches(fileUtil.Content, AppSettings.DiagnosticsEventLog, RegexOptions.IgnoreCase).Count;
            int matchedCount = 0, needModifiedCount = 0;
            if (fileUtil.MatchUtils != null && fileUtil.MatchUtils.Count > 0)
            {
                foreach (var item in fileUtil.MatchUtils)
                {
                    matchedCount += Regex.Matches(fileUtil.Content, item.When, RegexOptions.IgnoreCase).Count;
                    needModifiedCount += Regex.Matches(fileUtil.Content, item.WhenNot, RegexOptions.IgnoreCase).Count;
                }
            }

            return matchedCount > 0 || eventLogCount > 0 ? new FileMatchResult
            {
                DiagnosticsCount = diagnosticsCount,
                Path = fileUtil.Path,
                MatchedCount = matchedCount,
                ModifiedCount = fileUtil.ModifiedCount,
                TotalModifiedCount = needModifiedCount,
                PortableCount = portableCount,
                EventLogCount = eventLogCount,
                Skip = fileUtil.SkipFile,
                PerseusCount = perseusCount
            } : null;
        }
    }
}
