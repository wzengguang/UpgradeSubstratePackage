namespace UpgradeSubstrateTargetVersion
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    public class SubstrateScanUtil
    {
        public ConcurrentBag<string> WxsFiles { get; private set; } = new();
        public ConcurrentBag<string> WixprojFiles { get; private set; } = new();
        public ConcurrentBag<string> ConfigFiles { get; private set; } = new();
        public ConcurrentBag<string> XmlDropFiles { get; private set; } = new();
        public ConcurrentBag<string> NoprojFiles { get; private set; } = new();
        public ConcurrentBag<string> CsprojFiles { get; private set; } = new();
        public ConcurrentBag<string> NupkgProjFiles { get; private set; } = new();
        public ConcurrentBag<string> NuspecFiles { get; private set; } = new();
        public ConcurrentBag<string> TargetsFiles { get; private set; } = new();
        public ConcurrentBag<string> BatFiles { get; private set; } = new();
        public ConcurrentBag<string> Ps1Files { get; private set; } = new();

        public static async Task<SubstrateScanUtil> Scan()
        {
            var scan = new SubstrateScanUtil();

            var sourcePath = Path.Combine(AppSettings.RootPath);

            await scan.ScanFiles(sourcePath, scan.FindSpecifiedFile);

            return scan;
        }

        public ConcurrentBag<string> AllFiles()
        {
            var files = this.WxsFiles.Union(this.WixprojFiles)
                .Union(this.ConfigFiles)
                .Union(this.XmlDropFiles)
                .Union(this.NoprojFiles)
                .Union(this.NupkgProjFiles)
                .Union(this.NuspecFiles)
                .Union(this.TargetsFiles)
                .Union(this.BatFiles)
                .Union(this.CsprojFiles)
                .Union(this.Ps1Files).ToList();

            return new ConcurrentBag<string>(files);
        }

        private void FindSpecifiedFile(string path)
        {
            if (path.EndsWith(".wxs", StringComparison.OrdinalIgnoreCase))
            {
                this.WxsFiles.Add(path);
            }
            else if (path.EndsWith(".wixproj", StringComparison.OrdinalIgnoreCase))
            {
                this.WixprojFiles.Add(path);
            }
            else if (path.EndsWith(".config", StringComparison.OrdinalIgnoreCase))
            {
                this.ConfigFiles.Add(path);
            }
            else if (path.EndsWith("xmldrop.xml", StringComparison.OrdinalIgnoreCase))
            {
                this.XmlDropFiles.Add(path);
            }
            else if (path.EndsWith(".csproj"))
            {
                this.CsprojFiles.Add(path);
            }
            else if (path.EndsWith(".noproj"))
            {
                this.NoprojFiles.Add(path);
            }
            else if (path.EndsWith(".nupkg.proj"))
            {
                this.NupkgProjFiles.Add(path);
            }
            else if (path.EndsWith(".nuspec"))
            {
                this.NuspecFiles.Add(path);
            }
            else if (path.EndsWith(".targets"))
            {
                this.TargetsFiles.Add(path);
            }
            else if (path.EndsWith(".bat"))
            {
                this.BatFiles.Add(path);
            }
            else if (path.EndsWith(".ps1"))
            {
                this.Ps1Files.Add(path);
            }
        }

        private async Task ScanFiles(string folder, Action<string> action)
        {
            var fs = Directory.GetFiles(folder);
            foreach (string fileInfo in fs)
            {
                action(fileInfo);
            }

            List<Task> tasks = new List<Task>();
            var dirs = Directory.GetDirectories(folder);
            foreach (var dir in dirs)
            {
                tasks.Add(Task.Run(() => ScanFiles(dir, action)));
            }

            await Task.WhenAll(tasks);
        }
    }
}
