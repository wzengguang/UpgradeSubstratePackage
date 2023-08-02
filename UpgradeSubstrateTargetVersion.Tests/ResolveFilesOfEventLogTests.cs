using Microsoft.VisualStudio.TestTools.UnitTesting;
using UpgradeSubstrateTargetVersion;

namespace UpgradeSubstrateTargetVersion.Tests
{
    [TestClass]
    public class ResolveFilesOfEventLogTests
    {
        private SubstrateScanUtil scan;
        private ResolveFilesOfEventLog resolve;

        [TestInitialize]
        public async Task TestInitialize()
        {
            this.scan = await SubstrateScanUtil.Scan();
            resolve = new ResolveFilesOfEventLog(this.scan);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            LogUtil.SaveLogFile();
        }

        [TestMethod]
        public async Task ResolveTest()
        {
            await ResolveFilesOfEventLog.Resolve(this.scan);
        }

        [TestMethod]
        public async Task ResolveNoprojFilesTest()
        {
            resolve.ResolveNoprojFiles();
            await resolve.WaitDone();
        }

        [TestMethod]
        public async Task ResolveNupkgProjFilesTest()
        {
            resolve.ResolveNupkgProjFiles();
            await resolve.WaitDone();
        }

        [TestMethod]
        public async Task ResolveTargetsFilesTest()
        {
            resolve.ResolveTargetsFiles();
            await resolve.WaitDone();
        }

        [TestMethod]
        public async Task ResolveBatFilesTest()
        {
            resolve.ResolveBatFiles();
            await resolve.WaitDone();
        }

        [TestMethod]
        public async Task ResolvePs1FilesTest()
        {
            resolve.ResolvePs1Files();
            await resolve.WaitDone();
        }

        [TestMethod]
        public async Task ResolveXmlDropFilesTest()
        {
            resolve.ResolveXmlDropFiles();
            await resolve.WaitDone();
        }

        [TestMethod]
        public async Task ResolveConfigFilesTest()
        {
            resolve.ResolveConfigFiles();
            await resolve.WaitDone();
        }

        [TestMethod]
        public async Task ResolveWixprojFilesTest()
        {
            resolve.ResolveWixprojFiles();
            await resolve.WaitDone();
        }

        [TestMethod]
        public async Task ResolveWxsFilesTest()
        {
            resolve.ResolveWxsFiles();
            await resolve.WaitDone();
        }

        [TestMethod]
        public async Task ResolveCsprojFilesTest()
        {
            resolve.ResolveCsprojFiles();
            await resolve.WaitDone();
        }

        [TestMethod]
        public async Task ResolveCsFileTest()
        {
            resolve.ResolveCsFile();
            await resolve.WaitDone();
        }

        [TestMethod]
        public async Task ResolveCsvFileTest()
        {
            resolve.ResolveCsvFile();
            await resolve.WaitDone();
        }

        [TestMethod]
        public async Task ResolveVcxprojFileTest()
        {
            resolve.ResolveVcxprojFile();
            await resolve.WaitDone();
        }

        [TestMethod]
        public async Task ResolvePackagespropsFileTest()
        {
            resolve.ResolvePackagespropsFile();
            resolve.ResolveBuildCorextConfig();
            resolve.ResolveCorextConfig();
            resolve.ResolveVcxprojFile();
            resolve.ResolveCsFile();
            resolve.ResolveCsvFile();
            await resolve.WaitDone();
        }
    }
}