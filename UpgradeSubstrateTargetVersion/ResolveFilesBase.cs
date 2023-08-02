using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace UpgradeSubstrateTargetVersion
{
    public class ResolveFilesBase
    {
        protected List<Task<FileMatchResult>> tasks = new();
        protected SubstrateScanUtil scan;

        public ResolveFilesBase(SubstrateScanUtil scanUtil)
        {
            this.scan = scanUtil;
        }

        protected void ResolveFilesTask(IEnumerable<string> paths, Func<string, Task<FileMatchResult>> func)
        {
            foreach (var path in paths)
            {
                this.tasks.Add(Task.Run(async () =>
                {
                    return await func(path);
                }));
            }
        }

        public void VerifyUnResolvedFile()
        {
            Parallel.ForEach(scan.AllFiles(), (file) =>
            {
                try
                {
                    var content = File.ReadAllText(file);
                    if (content.Contains("Microsoft.M365.Core.EventLog") && !content.Contains("System.Diagnostics.EventLog"))
                    {
                        LogUtil.AddNotResolvedFile(file);
                    }
                }
                catch (Exception e)
                {
                    LogUtil.AddExceptionFile(e, file);
                }
            });
        }

        protected void ValidateXmlFile(string path)
        {
            try
            {
                using (XmlReader reader = XmlReader.Create(path))
                {
                    while (reader.Read())
                    {
                    }
                }
            }
            catch (Exception)
            {
                LogUtil.AddXmlStructErrorFiles(path);
            }
        }

        public async Task WaitDone()
        {
            await Task.WhenAll(tasks);
            var results = tasks.Where(a => a.Result != null).Select(a => a.Result);
            LogUtil.RecordResult(results);
        }


        protected string Pattern(string pattern)
        {
            return pattern.ToRegexString();
        }
    }
}
