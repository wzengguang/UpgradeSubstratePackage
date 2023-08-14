namespace UpgradeSubstrateTargetVersion
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    public class LogUtil
    {
        public static string LogPath = "../../../../Logs/";

        private static ConcurrentBag<string> NotResolvedFiles = new();
        private static ConcurrentBag<string> ExceptionFiles = new();
        private static ConcurrentBag<string> UnNornalFiles = new();
        private static ConcurrentBag<string> XmlStructErrorFiles = new();
        private static ConcurrentBag<FileMatchResult> TaskResults = new();

        public static void AddNotResolvedFile(string path)
        {
            NotResolvedFiles.Add(path);
        }

        public static void AddUnNornalFile(string path)
        {
            UnNornalFiles.Add(path);
        }
        public static void AddXmlStructErrorFiles(string path)
        {
            XmlStructErrorFiles.Add(path);
        }

        public static void AddExceptionFile(Exception exception, string path)
        {
            ExceptionFiles.Add(exception.GetType().Name + " :: " + path);
        }

        public static void RecordResult(IEnumerable<FileMatchResult> results)
        {
            TaskResults = new(results);
        }

        public static void SaveLogFile()
        {
            File.WriteAllLines(nameof(NotResolvedFiles), NotResolvedFiles.OrderBy(a => Path.GetExtension(a)));

            File.WriteAllLines(nameof(UnNornalFiles), UnNornalFiles.OrderBy(a => a));
            File.WriteAllLines(nameof(XmlStructErrorFiles), XmlStructErrorFiles.OrderBy(a => Path.GetExtension(a)));

            List<string> LogIgnoreFile = MatchParam.Load("Match/logIgnore.xml").First().Path;

            var result = TaskResults.Where(a => !a.Path.ContainIgnoreCase(LogIgnoreFile));

            var taskResult = result.Where(a => a.EventLogCount == a.PortableCount).OrderBy(a => Path.GetExtension(a.Path))
                .Select(a => $"[EventLog:{a.EventLogCount};Portable:{a.PortableCount}]" + a.Path);
            File.WriteAllLines(LogPath + "Portable-equal-EventLog", taskResult);

            var taskResult1 = result.Where(a => string.IsNullOrEmpty(a.Skip) && a.PerseusCount > 0 && a.DiagnosticsCount > 0).OrderBy(a => Path.GetExtension(a.Path))
                .Select(a => $"[Perseus:{a.PerseusCount};Diagnostics:{a.DiagnosticsCount}]" + a.Path);
            File.WriteAllLines(LogPath + "Perseus-Diagnostics-not-zero", taskResult1);

            var taskResult2 = result.Where(a => string.IsNullOrEmpty(a.Skip) && a.EventLogCount > a.PortableCount).OrderBy(a => Path.GetExtension(a.Path))
                .Select(a => $"[EventLog:{a.EventLogCount};Portable:{a.PortableCount}]" + a.Path);
            File.WriteAllLines(LogPath + "Portable-less-EventLog", taskResult2);

            var taskResult3 = result.Where(a => string.IsNullOrEmpty(a.Skip) && a.EventLogCount < a.PortableCount).OrderBy(a => Path.GetExtension(a.Path))
                .Select(a => $"[EventLog:{a.EventLogCount};Portable:{a.PortableCount}]" + a.Path);
            File.WriteAllLines(LogPath + "Portable-than-EventLog", taskResult3);

            var taskResult4 = result.Where(a => string.IsNullOrEmpty(a.Skip) && a.EventLogCount > a.DiagnosticsCount).OrderBy(a => Path.GetExtension(a.Path))
                .Select(a => $"[Eventlog:{a.EventLogCount};Diagnostics:{a.DiagnosticsCount}]" + a.Path);
            File.WriteAllLines(LogPath + "Diagnostics-less-Eventlog", taskResult4);

            var taskResult5 = result.Where(a => string.IsNullOrEmpty(a.Skip) && a.EventLogCount < a.DiagnosticsCount).OrderBy(a => Path.GetExtension(a.Path))
                .Select(a => $"[Eventlog:{a.EventLogCount};Diagnostics:{a.DiagnosticsCount}]" + a.Path);
            File.WriteAllLines(LogPath + "Diagnostics-than-Eventlog", taskResult5);
        }
    }
}
