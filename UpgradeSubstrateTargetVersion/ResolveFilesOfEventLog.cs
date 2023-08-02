
namespace UpgradeSubstrateTargetVersion
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public partial class ResolveFilesOfEventLog : ResolveFilesBase
    {
        public ResolveFilesOfEventLog(SubstrateScanUtil scanUtil) : base(scanUtil)
        {
        }

        public static async Task Resolve(SubstrateScanUtil scan)
        {
            ResolveFilesOfEventLog resolve = new(scan);
            resolve.ResolveCorextConfig();
            resolve.ResolveBuildCorextConfig();
            resolve.ResolveVcxprojFile();
            resolve.ResolveCsFile();
            resolve.ResolveCsvFile();
            // resolve.ResolveHFile();
            // resolve.ResolveWsfFile();
            resolve.ResolvePackagespropsFile();

            #region ResolveFilesOfEventLogFromScan
            resolve.ResolveWixprojFiles();
            resolve.ResolveWxsFiles();
            resolve.ResolveConfigFiles();
            resolve.ResolveXmlDropFiles();
            resolve.ResolveNoprojFiles();
            resolve.ResolveNuspecFiles();
            resolve.ResolveBatFiles();
            resolve.ResolveNupkgProjFiles();
            resolve.ResolveTargetsFiles();
            resolve.ResolvePs1Files();
            resolve.ResolveCsprojFiles();
            #endregion

            await resolve.WaitDone();

            resolve.VerifyUnResolvedFile();
        }

        public void ResolveCorextConfig()
        {
            string[] paths = { ".corext/corext.config" };
            ResolveFilesTask(paths, async (path) =>
            {
                FileMatch fileUtils = await FileMatch.ReadFileAsync(path);

                fileUtils.Replace("id=\"Microsoft.M365.Core.EventLog\" version=\"1.2.3\"", "id=\"Microsoft.M365.Core.EventLog\" version=\"1.2.4\"");
                fileUtils.Replace("id=\"Microsoft.M365.Core.DiagnosticsLog\" version=\"4.6.1\"", "id=\"Microsoft.M365.Core.DiagnosticsLog\" version=\"4.6.2\"");

                return await fileUtils.SaveResult();
            });
        }

        public void ResolveBuildCorextConfig()
        {
            string[] paths = { "build/corext/corext.config" };
            string[] portableValue = {
                "<package id=\"Microsoft.M365.Core.Portable.EventLog\" version=\"0.3.0\" rekeyForCloudSign=\"true\" producingBranch=\"Root\" />",
                "<!--This comment exists to prevent Git merge conflicts.  Do not delete it when editing this file.-->"
            };
            string whennot = @"id=""Microsoft\.M365\.Core\.Portable\.EventLog""";
            string where = @"\s*\<package\s+id=""Microsoft\.M365\.Core\.Portable\.Registry";

            ResolveFilesTask(paths, async (path) =>
            {
                FileMatch fileUtils = await FileMatch.ReadFileAsync(path);
                fileUtils.InsertWhenNot(whennot, where, portableValue);
                return await fileUtils.SaveResult();
            });
        }

        public void ResolveVcxprojFile()
        {
            string[] paths = { "sources/dev/cluster/src/ReplicaVSSWriter/Microsoft.Exchange.Cluster.ReplicaVSSWriter.vcxproj" };
            ResolveFilesTask(paths, async (path) =>
            {
                FileMatch fileUtils = await FileMatch.ReadFileAsync(path);
                string when = @"\s*\<QCustomInput .*Microsoft\.M365.Core\.EventLog\.dll";
                string whenNot = @"\<QCustomInput .*System\.Diagnostics\.EventLog\.dll";
                string[] inserts =
                    {
                    "<QCustomInput Include=\"$(PkgMicrosoft_M365_Core_Portable_EventLog)\\lib\\netstandard2.0\\Microsoft.M365.Core.Portable.EventLog.dll\">",
                    "  <Visible>false</Visible>",
                    "</QCustomInput>",
                    "<QCustomInput Include=\"$(PkgSystem_Diagnostics_EventLog)\\lib\\net461\\System.Diagnostics.EventLog.dll\">",
                    "  <Visible>false</Visible>",
                    "</QCustomInput>"
                };

                fileUtils.InsertWhenAndNotByGroup(when, whenNot, when, inserts);
                return await fileUtils.SaveResult();
            });
        }

        public void ResolveCsvFile()
        {
            string[] paths = { "sources/dev/ServiceManagement/nupkg/TorusClientExchangeDependency/exchangetypedlls.csv" };
            ResolveFilesTask(paths, async (path) =>
            {
                FileMatch fileUtils = await FileMatch.ReadFileAsync(path);
                string when = @"\s*Setup\\ServerRoles\\Common\\Microsoft\.M365.Core\.EventLog\.dll";
                string whenNot = @"Setup\\ServerRoles\\Common\\System\.Diagnostics\.EventLog\.dll";
                string[] inserts =
                    {
                    "Setup\\ServerRoles\\Common\\Microsoft.M365.Core.Portable.EventLog.dll",
                    "Setup\\ServerRoles\\Common\\System.Diagnostics.EventLog.dll"
                };

                fileUtils.InsertWhenAndNotByGroup(when, whenNot, when, inserts);
                return await fileUtils.SaveResult();
            });
        }

        public void ResolveCsFile()
        {
            string[] paths ={
                "sources/test/mapimt/src/Performance/PerformanceHelper.cs" ,
                "sources/dev/Setup/src/setup/BootstrapperCommon/setupchecksFileConstant.cs" ,
                "sources/test/ese/src/WSTF/core/TestFlightBase.cs"};
            ResolveFilesTask(paths, async (path) =>
            {
                FileMatch fileUtils = await FileMatch.ReadFileAsync(path);
                string when = @"\s*""Microsoft\.M365\.Core\.EventLog\.dll"",";
                string whenNot = @"""System\.Diagnostics\.EventLog\.dll"",";
                string[] inserts =
                    {
                    "\"Microsoft.M365.Core.Portable.EventLog.dll\",",
                    "\"System.Diagnostics.EventLog.dll\","
                };

                fileUtils.InsertWhenAndNotByGroup(when, whenNot, when, inserts);
                return await fileUtils.SaveResult();
            });
        }

        public void ResolvePackagespropsFile()
        {
            string[] paths = { "Packages.props" };
            ResolveFilesTask(paths, async (path) =>
            {
                FileMatch fileUtils = await FileMatch.ReadFileAsync(path);
                string pattern = @"\<PackageVersion Include\=""Microsoft\.M365\.Core\.EventLog"" Version=.+\>";
                string replacement = "<PackageVersion Include=\"Microsoft.M365.Core.EventLog\" Version=\"1.2.4\" />";
                fileUtils.Replace(pattern, replacement);

                string pattern2 = @"(<PackageVersion Include=""Microsoft\.M365\.Core\.DiagnosticsLog"" Version="")(\d+\.\d+\.\d+)";
                string replacement2 = "${1}4.6.2";
                fileUtils.Replace(pattern2, replacement2);

                string[] portableValue = {
                "<PackageVersion Include=\"Microsoft.M365.Core.Portable.EventLog\" Version=\"0.3.0\" />",
                "<!--This comment exists to prevent Git merge conflicts.  Do not delete it when editing this file.-->"
                };

                string whennot = @"Include=""Microsoft\.M365\.Core\.Portable\.EventLog""";
                string where = @"\s*\<PackageVersion\s+Include=""Microsoft\.M365\.Core\.Portable\.Registry";
                fileUtils.InsertWhenNot(whennot, where, portableValue);

                return await fileUtils.SaveResult();
            });
        }

        //public void ResolveHFile()
        //{
        //    string[] paths = { "sources/dev/cluster/src/ReplicaVSSWriter/interop.h" };
        //    ResolveFilesTask(paths, async (path) =>
        //    {
        //        FileMatch fileUtils = await FileMatch.ReadFileAsync(path);
        //        return await fileUtils.SaveResult();
        //    });
        //}

        //public void ResolveWsfFile()
        //{
        //    string[] paths = { "sources/test/ese/src/scripts/unittests.wsf" };
        //    ResolveFilesTask(paths, async (path) =>
        //    {
        //        FileMatch fileUtils = await FileMatch.ReadFileAsync(path);
        //        return await fileUtils.SaveResult();
        //    });
        //}
    }
}