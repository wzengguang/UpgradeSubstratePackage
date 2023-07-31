using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace UpgradeSubstrateTargetVersion.Tests
{
    [TestClass]
    public class RegexMatchTest
    {
        [TestMethod]
        public void Noproj()
        {
            string when = @"\<Copy .*Microsoft\.M365\.Core\.EventLog\.dll";

            string str = "<Copy SourceFiles=\"$(PkgMicrosoft_M365_Core_EventLog)\\lib\\net462\\Microsoft.M365.Core.EventLog.dll\" DestinationFolder=\"$(BinDir)\" />";

            bool resullt = Regex.IsMatch(str, when, RegexOptions.IgnoreCase);

            Assert.IsTrue(resullt);
        }

        [TestMethod]
        public void P()
        {
            string source = "<PackageReference Update=\"Microsoft.M365.Core.EventLog\" Version=\"1.2.3\" />";
            string pattern = @"\<PackageReference Update=""Microsoft\.M365\.Core\.EventLog"" Version=.+\>";
            string replacement = "<PackageReference Update=\"Microsoft.M365.Core.EventLog\" Version=\"1.2.4\" />";
            bool resullt = Regex.IsMatch(source, pattern, RegexOptions.IgnoreCase);

            var s = Regex.Replace(source, pattern, replacement);
        }

        [TestMethod]
        public void NotMatch()
        {
            string str = "esfsfes";
            string pattern = "^(?!.*(?:CertificateAuthentication|SmokeTestHeaderModule|RemotePowershellBackendCmdletProxy|FailFast|NonBVT|DiagnosticsModules|src\\\\Categorizer\\\\Extensibility\\\\Agents|MessageSecurity\\\\src\\\\Service|Directory\\\\src\\\\TopologyService\\\\Service)).*";
            var m1 = Regex.IsMatch(str, pattern, RegexOptions.IgnoreCase);
            Assert.IsTrue(m1);
            string text = "Some text without the forbidden string smokeTestHeaderModule.";

            bool m = Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase);

            Assert.IsFalse(m);
        }

        [TestMethod]
        public void Replace()
        {
            string orgin = "<PackageReference Update=\"Microsoft.M365.Core.EventLog\" Version=\"1.2.3\" />";
            string pattern = @"(\<PackageReference Update=""Microsoft\.M365\.Core\.EventLog"" Version="")(\d+\.\d+\.\d+)";
            string replacement = @"${1}1.2.2";
            var match = Regex.Match(orgin, pattern, RegexOptions.IgnoreCase);
            var result = Regex.Replace(orgin, pattern, replacement, RegexOptions.IgnoreCase);
        }

        [TestMethod]
        public async Task Match()
        {
            List<MatchUtil> matches = MatchUtil.GetMatchs("Data/csproj");

            FileMatch fileUtils = await FileMatch.ReadFileAsync("sources\\test\\Hygiene\\src\\RemoteWorkflow\\UnitTests\\Internal.Exchange.Hygiene.RemoteWorkflow.UnitTests.csproj");
            fileUtils.InsertWhenAndNotByGroup(matches);
            await fileUtils.SaveResult();
        }
    }
}
