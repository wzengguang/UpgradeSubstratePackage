namespace UpgradeSubstrateTargetVersion
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    public class LogUtil
    {
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

            var taskResult = TaskResults.Where(a => a.EventLogCount == a.PortableCount).OrderBy(a => Path.GetExtension(a.Path))
                .Select(a => $"[E:{a.EventLogCount};P:{a.PortableCount}]" + a.Path);
            File.WriteAllLines(nameof(TaskResults) + 1, taskResult);

            var taskResult2 = TaskResults.Where(a => string.IsNullOrEmpty(a.Skip) && a.EventLogCount > a.PortableCount).OrderBy(a => Path.GetExtension(a.Path))
                .Select(a => $"[E:{a.EventLogCount};P:{a.PortableCount}]" + a.Path);
            File.WriteAllLines("LessPortableCount", taskResult2);

            var taskResult3 = TaskResults.Where(a => string.IsNullOrEmpty(a.Skip) && a.EventLogCount < a.PortableCount).OrderBy(a => Path.GetExtension(a.Path))
                .Select(a => $"[E:{a.EventLogCount};P:{a.PortableCount}]" + a.Path);
            File.WriteAllLines("MorePortableCount", taskResult3);

            var taskResult4 = TaskResults.Where(a => string.IsNullOrEmpty(a.Skip) && a.EventLogCount > a.DiagnosticsCount).OrderBy(a => Path.GetExtension(a.Path))
                .Select(a => $"[E:{a.EventLogCount};D:{a.DiagnosticsCount}]" + a.Path);
            File.WriteAllLines("LessDiagnosticsCount", taskResult4);

            var taskResult5 = TaskResults.Where(a => string.IsNullOrEmpty(a.Skip) && a.EventLogCount < a.DiagnosticsCount).OrderBy(a => Path.GetExtension(a.Path))
                .Select(a => $"[E:{a.EventLogCount};D:{a.DiagnosticsCount}]" + a.Path);
            File.WriteAllLines("MoreDiagnosticsCount", taskResult5);
        }
    }
}
