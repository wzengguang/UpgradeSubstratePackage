
namespace UpgradeSubstrateTargetVersion
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public partial class ResolveFilesOfEventLog : ResolveFilesBase
    {
        public void ResolveWixprojFiles()
        {
            ResolveFilesTask(scan.WixprojFiles, async (path) =>
            {
                FileMatch fileUtils = await FileMatch.ReadFileAsync(path, false);

                if (path.Contains("Test\\AutomationSetup\\MSIs\\Exchange_Test", StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                string when = @"\s*-dPkgMicrosoft_M365_Core_EventLog";
                string whenNot = "-dPkgSystem_Diagnostics_EventLog";
                string[] inserts =
                {
                    "-dPkgSystem_Diagnostics_EventLog=$(PkgSystem_Diagnostics_EventLog)",
                    "-dPkgMicrosoft_M365_Core_Portable_EventLog=$(PkgMicrosoft_M365_Core_Portable_EventLog)"
                    };
                fileUtils.InsertWhenAndNot(whenNot, when, inserts);

                string when2 = @"\s*\<QCustomInput Include=""\$\(PkgSystem_Diagnostics_EventLog\)""\>";
                string whenNot2 = "\"\\$\\(PkgSystem_Diagnostics_EventLog\\)\"";
                string[] inserts2 =
                {
                    "<QCustomInput Include=\"$(PkgSystem_Diagnostics_EventLog)\">",
                    "  <Visible>false</Visible>",
                    "</QCustomInput>",
                    "<QCustomInput Include=\"$(PkgMicrosoft_M365_Core_Portable_EventLog)\">",
                    "  <Visible>false</Visible>",
                    "</QCustomInput>"
                    };
                fileUtils.InsertWhenAndNot(whenNot2, when2, inserts2);

                return await fileUtils.SaveResult();
            });
        }

        public void ResolveWxsFiles()
        {
            List<string> skips = new List<string>
            {
            };

            ResolveFilesTask(scan.WxsFiles, async (path) =>
            {
                FileMatch fileMatch = await FileMatch.ReadFileAsync(path);
                if (skips.Exists(a => path.Contains(a, StringComparison.OrdinalIgnoreCase)))
                {
                    fileMatch.SkipFile = "skip";
                    return await fileMatch.SaveResult();
                }

                var matchparams = MatchParam.Load("match/wxs.xml");
                foreach (var item in matchparams)
                {
                    fileMatch.Match(item);
                }

                //string when = Pattern(@"<File Vital=""yes"" Source=""$(var.PkgMicrosoft_M365_Core_EventLog)\lib\net462\Microsoft.M365.Core.EventLog.dll"" />");
                //string whenNot = @"Microsoft\.M365\.Core\.Portable\.EventLog\.dll";
                //string fileName = Path.GetFileNameWithoutExtension(path).Replace('.', '_');
                //string[] inserts =
                //    {
                //    $"<File Id=\"{fileName}_System_Diagnostics_EventLog\" Source=\"$(var.PkgSystem_Diagnostics_EventLog)\\lib\\net461\\System.Diagnostics.EventLog.dll\" />",
                //    $"<File Id=\"{fileName}_Microsoft_M365_Core_Portable_EventLog\" Source=\"$(var.PkgMicrosoft_M365_Core_Portable_EventLog)\\lib\\netstandard2.0\\Microsoft.M365.Core.Portable.EventLog.dll\" />"
                //    };

                if (path.Contains("dev\\Hygiene\\MSIs\\WstSetup\\msi\\ManagedClient.wxs", StringComparison.OrdinalIgnoreCase))
                {
                    //when = @"\s*\<Component Id=""Microsoft\.M365\.Core\.EventLog\.dll""";
                    //whenNot = @"Microsoft\.M365\.Core\.Portable\.EventLog\.dll";
                    //inserts =
                    //{
                    //    "",
                    //    @"<Component Id=""System.Diagnostics.EventLog.dll"" Guid=""CDBD702E-71F2-4A77-BFF1-240A002F25B2"" Directory=""INSTALLDIR"">",
                    //    @"  <File Id=""System.Diagnostics.EventLog.dll"" Name=""System.Diagnostics.EventLog.dll"" KeyPath=""yes"" Compressed=""yes"" Source=""$(var.PkgSystem_Diagnostics_EventLog)\lib\net461\System.Diagnostics.EventLog.dll"" />",
                    //    @"</Component>",
                    //    "",
                    //    @"<Component Id=""Microsoft.M365.Core.Portable.EventLog.dll"" Guid=""D3E09636-6C39-4C9C-ACE8-F3A813BF9C65"" Directory=""INSTALLDIR"">",
                    //    @"  <File Id=""Microsoft.M365.Core.Portable.EventLog.dll"" Name=""Microsoft.M365.Core.Portable.EventLog.dll"" KeyPath=""yes"" Compressed=""yes""  Source=""$(var.PkgMicrosoft_M365_Core_Portable_EventLog)\lib\netstandard2.0\Microsoft.M365.Core.Portable.EventLog.dll"" />",
                    //    @"</Component>"
                    //};

                    //fileMatch.InsertWhenAndNot(whenNot, when, inserts);
                }
                else
                {
                    //string fileName = Path.GetFileNameWithoutExtension(path).Replace('.', '_');

                    //string when = @"\s*\<File Id=""\w+"" Source=""\$\(var\.PkgMicrosoft_M365_Core_EventLog\)\\lib\\";
                    //string whenNot = "var\\.PkgSystem_Diagnostics_EventLog";
                    //string[] inserts =
                    //{
                    //$"<File Id=\"{fileName}_System_Diagnostics_EventLog\" Source=\"$(var.PkgSystem_Diagnostics_EventLog)\\lib\\net461\\System.Diagnostics.EventLog.dll\" />",
                    //$"<File Id=\"{fileName}_Microsoft_M365_Core_Portable_EventLog\" Source=\"$(var.PkgMicrosoft_M365_Core_Portable_EventLog)\\lib\\netstandard2.0\\Microsoft.M365.Core.Portable.EventLog.dll\" />"
                    //};
                    //fileMatch.InsertWhenAndNot(whenNot, when, inserts);
                }

                return await fileMatch.SaveResult();
            });
        }

        public void ResolveConfigFiles()
        {
            ResolveFilesTask(scan.ConfigFiles, async (path) =>
            {
                FileMatch fileUtils = await FileMatch.ReadFileAsync(path, false);
                this.ResolveConfigFilesDependentAssembly(fileUtils);
                this.ResolveConfigFilesAssembly(fileUtils);

                return await fileUtils.SaveResult();
            });
        }

        private void ResolveConfigFilesDependentAssembly(FileMatch fileUtils)
        {
            // codeBase: Microsoft.M365.Core.Portable.EventLog
            string when = @"\s*\<dependentAssembly\>\s+\<assemblyIdentity name=""Microsoft\.M365\.Core\.EventLog""\s+publicKeyToken=""[A-Za-z0-9]+""\s+culture=""neutral""\s+\/\>\s+\<codeBase";
            string whenNot = @"\<assemblyIdentity name=""Microsoft\.M365\.Core\.Portable\.EventLog""\s+publicKeyToken=""[A-Za-z0-9]+""\s+culture=""neutral""\s+\/\>\s+\<codeBase version=""18.0.0.0""";
            string[] inserts =
            {
                        "<dependentAssembly>",
                        "  <assemblyIdentity name=\"Microsoft.M365.Core.Portable.EventLog\" publicKeyToken=\"5a24b4a52c5686bd\" culture=\"neutral\" />",
                        "  <codeBase version=\"18.0.0.0\" href=\"file:///%ExchangeInstallDir%bin\\Microsoft.M365.Core.Portable.EventLog.dll\" />",
                        "</dependentAssembly>"
                    };
            fileUtils.InsertWhenAndNot(whenNot, when, inserts);

            string when2 = @"\s*\<dependentAssembly\>\s+\<assemblyIdentity name=""Microsoft\.M365\.Core\.EventLog""\s+publicKeyToken=""[A-Za-z0-9]+""\s+culture=""neutral""\s+\/\>\s+\<bindingRedirect";
            string whenNot2 = @"\<assemblyIdentity name=""Microsoft\.M365\.Core\.Portable\.EventLog""\s+publicKeyToken=""[A-Za-z0-9]+""\s+culture=""neutral""\s+\/\>\s+\<bindingRedirect";
            string[] inserts2 = new string[]
             {
                            "<dependentAssembly>",
                            "  <assemblyIdentity name=\"Microsoft.M365.Core.Portable.EventLog\" publicKeyToken=\"5a24b4a52c5686bd\" culture=\"neutral\" />",
                            "  <bindingRedirect oldVersion=\"0.0.0.0-18.0.0.0\" newVersion=\"18.0.0.0\" />",
                            "</dependentAssembly>"
                     };
            fileUtils.InsertWhenAndNot(whenNot2, when2, inserts2);


            //if (fileUtils.Path.Contains("dev\\common\\src\\common\\SharedBindingRedirects.config"))
            //{
            //    string whenNot01 = @"\<assemblyIdentity name=""Microsoft\.M365\.Core\.Portable\.EventLog""\s+publicKeyToken=""[A-Za-z0-9]+""\s+culture=""neutral""\s+\/\>\s+\<bindingRedirect";
            //    string[] inserts01 =
            //    {
            //                "<dependentAssembly>",
            //                "  <assemblyIdentity name=\"Microsoft.M365.Core.Portable.EventLog\" publicKeyToken=\"5a24b4a52c5686bd\" culture=\"neutral\" />",
            //                "  <bindingRedirect oldVersion=\"0.0.0.0-18.0.0.0\" newVersion=\"18.0.0.0\" />",
            //                "</dependentAssembly>"
            //            };
            //    fileUtils.InsertWhenAndNot(whenNot01, when, inserts01);
            //}

            // bindingRedirect: System.Diagnostics.EventLog, 可能有些文件有了。
            string when3 = @"\s*\<dependentAssembly\>\s+\<assemblyIdentity name=""System\.Diagnostics\.EventLog""\s+publicKeyToken=""[A-Za-z0-9]+""\s+culture=""neutral""\s+\/\>\s+\<bindingRedirect";
            string whenNot3 = @"\<assemblyIdentity name=""System\.Diagnostics\.EventLog""";
            string[] inserts3 =
            {
                        "<dependentAssembly>",
                        "  <assemblyIdentity name=\"System.Diagnostics.EventLog\" publicKeyToken=\"5a24b4a52c5686bd\" culture=\"neutral\" />",
                        "  <bindingRedirect oldVersion=\"0.0.0.0-4.0.2.0\" newVersion=\"4.0.2.0\" />",
                        "</dependentAssembly>"
                    };
            fileUtils.InsertWhenAndNot(whenNot3, when3, inserts3);
        }

        private void ResolveConfigFilesAssembly(FileMatch fileUtils)
        {
            string when = @"\s+\<add assembly=""Microsoft\.M365\.Core\.EventLog";
            string whenNot = @"\<add assembly=""Microsoft\.M365\.Core\.Portable\.EventLog";
            string[] inserts =
                {
                "<add assembly=\"Microsoft.M365.Core.Portable.EventLog, Version=18.0.0.0, Culture=neutral, publicKeyToken=5a24b4a52c5686bd\" />",
                };

            fileUtils.InsertWhenAndNot(whenNot, when, inserts);

            string whenNot2 = @"\<add assembly=""System\.Diagnostics\.EventLog";
            string[] inserts2 =
                {
                "<add assembly=\"System.Diagnostics.EventLog, Version=18.0.0.0, Culture=neutral, publicKeyToken=5a24b4a52c5686bd\" />",
                };

            fileUtils.InsertWhenAndNot(whenNot2, when, inserts2);
        }

        public void ResolveXmlDropFiles()
        {
            this.ResolveFilesTask(scan.XmlDropFiles, async (path) =>
            {
                string when = @"\s+\<FILENUPKG filename=""Microsoft\.M365\.Core\.EventLog\.dll""";
                string whenNot = @"<FILENUPKG filename=""Microsoft\.M365\.Core\.Portable\.EventLog\.dll""";
                var inserts = MatchUtil.GetInsertValuesByPath("Data/EventLogXmldrop.xml", path);
                if (inserts != null)
                {
                    FileMatch fileUtils = await FileMatch.ReadFileAsync(path, false);
                    fileUtils.InsertWhenAndNot(whenNot, when, inserts);
                    return await fileUtils.SaveResult();
                }
                return null;
            });
        }

        public void ResolveCsprojFiles()
        {
            List<MatchUtil> matches = MatchUtil.GetMatchs("Data/csproj");
            List<string> exclude = new() {
                "Dev\\Clients\\src\\common",
                "dev\\services\\src\\EwsSerializersGeneratorPostProcessing",
                "dev\\common\\src\\BinPlaceForPackages\\BinPlaceForPackages",
                "dev\\admin\\src\\Reports\\Server\\Extensions",
                "dev\\admin\\src\\ecp\\ControlPanel",
                "Broker\\Service",
                "dev\\admin\\src\\ReportingWebService\\Service",
                "Dev\\Cafe\\src\\FootPrint",
                "Dev\\Common\\src\\DnsOpticsCollector",
                "Dev\\Search\\Src\\Service",
                "dev\\Networking\\src\\NetMan\\NetworkManager",
                "Dev\\Filtering\\src\\platform\\Management\\ADConnector\\Impl",
                "src\\TrainingRecord\\TrainingRecordManager",
                "src\\CacheConvergence\\Diagnostics",
                "Dev\\Networking\\src\\ReTiNA\\BranchConnectOpticsLogger",
                "Dev\\Data\\src\\ThrottlingService\\Service",
                "Dev\\Networking\\src\\PLAT\\Microsoft.Office.Datacenter.Connectivity.PLAT.Shared",
                "Dev\\Cafe\\src\\AnycastDnsOnCafe\\DnsHelperService",
                "Dev\\Services\\src\\OAB",
                "Dev\\MailboxAssistants\\src\\Service",
                "Dev\\MailboxAssistants\\src\\AssistantInfra\\src",
                "Dev\\MapiMT\\src\\RpcHttpModules",
                "Dev\\Directory\\src\\CacheService",
                "Dev\\RcaService\\src\\Service",
                "Test\\Performance\\SRC\\EDS",
                "Dev\\Networking\\src\\ReTiNA\\SinkPlugin",
                "Test\\Transport\\src\\PoisonMsg\\NonBVT\\Component\\Tests",
                "Dev\\Clients\\src\\security",
                "Dev\\Cafe\\src\\Diagnostics",
                "Test\\Transport\\src\\smtp\\ZeroBox",
                "test\\tools\\src\\EdgeManagement2",
                "Dev\\Networking\\src\\PLAT\\Microsoft.Office.Datacenter.Connectivity.PLAT.PLATClient",
                "Dev\\SharedCache\\src\\Caches",
                "Dev\\Networking\\src\\PLAT\\Microsoft.Office.Datacenter.Connectivity.PLAT.PLATServer",
                "Test\\Transport\\src\\transport\\ZeroBox",
                "Test\\Tools\\src\\TenantMonitoring",
                "Dev\\Networking\\src\\ReTiNA\\LensHostService",
                "Dev\\SharedCache\\src\\Server",
                "Dev\\MessageSecurity\\src\\Service",
                "Dev\\Directory\\src\\TopologyService\\Service",
                "Dev\\Cafe\\src\\SmokeTestHeaderModule",
                "dev\\cafe\\src\\RoutingService\\Server",
                "Test\\ExpoFramework\\src\\storage\\NonBVT",
                "Test\\MailboxTransport\\src\\Categorizer\\Extensibility\\Agents",
                "Dev\\Configuration\\src\\CertificateAuthentication",
                "Dev\\Configuration\\src\\RemotePowershellBackendCmdletProxy",
                "Dev\\Configuration\\src\\DiagnosticsModules",
                "Dev\\Configuration\\src\\FailFast",
                "dev\\cafe\\src\\HttpProxy",
                "Test\\Transport\\src\\transport\\SafetyNet\\Component",
                "dev\\clients\\src\\Owa2\\Server",
                "Test\\Tools\\src\\EdgeAutoInfra",
                "Test\\Transport\\src\\TransportSmoke",
                "Test\\Filtering\\src\\platform\\AutomatedTests\\E15Automation\\Reporting",
                "Test\\Infoworker\\src\\Shared\\Common\\Management",
                "Dev\\Clients\\src\\owa\\bin",
                "Dev\\E4E\\src\\Server",
                "Test\\BCM\\src\\inboxrule\\NonBVT",
                "Test\\Search\\Src\\Common",
                "Test\\Transport\\src\\BackPressure\\Component",
                "Test\\Directory\\src\\TopologyService\\ZeroBox",
                "Test\\Transport\\src\\Storage\\ZeroBox",
                "test\\infoworker\\src\\Shared\\ConsolidatedBinary",
                "test\\Search\\src\\BigFunnel",
                "test\\Search\\src\\Core",
                "test\\infoworker\\src\\Shared\\Components\\ELC",
                "test\\infoworker\\src\\NonBVT\\OOF",
                "test\\infoworker\\src\\NonBVT\\Availability",
                "sources\\Test\\Transport\\src\\smtp\\SmtpBlobs",
                "sources\\Dev\\Data\\src\\dsapi\\Api",
                "sources\\Test\\Management\\src\\Management\\ProvisioningSOPsTests"};

            this.ResolveFilesTask(scan.CsprojFiles, async (path) =>
            {
                if (exclude.Exists(e => path.Contains(e, StringComparison.OrdinalIgnoreCase)))
                {
                    return null;
                }

                FileMatchResult result = null;
                FileMatch fileUtils = await FileMatch.ReadFileAsync(path, false);

                // case:
                string when = @"\s*""\$\(PkgMicrosoft_M365_Core_EventLog\)\\lib\\net462\\Microsoft\.M365\.Core\.EventLog\.dll"" \^";
                string whenNot = @"Microsoft\.M365\.Core\.Portable\.EventLog";
                string[] inserts =
                    {
                    @"""$(PkgMicrosoft_M365_Core_Portable_EventLog)\lib\netstandard2.0\Microsoft.M365.Core.Portable.EventLog.dll"" ^"
                };
                fileUtils.InsertWhenAndNot(whenNot, when, inserts);

                // case:
                string when2 = @"\s*""\$\(PkgMicrosoft_M365_Core_EventLog\)\\lib\\net462\\Microsoft\.M365\.Core\.EventLog\.dll"" \^";
                string whenNot2 = @"System\.Diagnostics\.EventLog\.dll";
                string[] inserts2 =
                    {
                    @"""$(PkgSystem_Diagnostics_EventLog)\lib\net461\System.Diagnostics.EventLog.dll"" ^"
                };
                fileUtils.InsertWhenAndNot(whenNot2, when2, inserts2);

                // case:
                string when3 = @"\s*<QCustomInput\s+Include=""\$\(PkgMicrosoft_M365_Core_EventLog\)""\>";
                string whenNot3 = @"\<QCustomInput Include=""\$\(PkgMicrosoft_M365_Core_Portable_EventLog\)""\>";
                string[] inserts3 =
                    {
                    @"<QCustomInput Include=""$(PkgMicrosoft_M365_Core_Portable_EventLog)"">",
                    @"  <Visible>false</Visible>",
                    @"</QCustomInput>"
                };
                fileUtils.InsertWhenAndNot(whenNot3, when3, inserts3);

                // case:
                string when4 = @"\s*<QCustomInput\s+Include=""\$\(PkgMicrosoft_M365_Core_EventLog\)""\>";
                string whenNot4 = @"\<QCustomInput Include=""\$\(PkgSystem_Diagnostics_EventLog\)""\>";
                string[] inserts4 =
                    {
                    @"<QCustomInput Include=""$(PkgSystem_Diagnostics_EventLog)"">",
                    @"  <Visible>false</Visible>",
                    @"</QCustomInput>"
                };
                fileUtils.InsertWhenAndNot(whenNot4, when4, inserts4);

                // case:
                string when5 = @"\s*\<Reference Include=""Microsoft.M365.Core.EventLog.dll""\>\s*\<HintPath\>\$\(PkgMicrosoft_M365_Core_EventLog\)\\lib\\net462\\Microsoft\.M365\.Core\.EventLog\.dll";
                string whenNot5 = @"\<HintPath\>\$\(PkgSystem_Diagnostics_EventLog\)\\lib\\";
                string[] inserts5 =
                    {
                    @"<Reference Include=""System.Diagnostics.EventLog.dll"">",
                    @"  <HintPath>$(PkgSystem_Diagnostics_EventLog)\lib\net461\System.Diagnostics.EventLog.dll</HintPath>",
                    @"</Reference>"
                };
                fileUtils.InsertWhenAndNot(whenNot5, when5, inserts5);

                // case:
                string when6 = @"\s*\<Reference Include=""Microsoft.M365.Core.EventLog.dll""\>\s*\<HintPath\>\$\(PkgMicrosoft_M365_Core_EventLog\)\\lib\\net462\\Microsoft\.M365\.Core\.EventLog\.dll";
                string whenNot6 = @"\<HintPath\>\$\(PkgMicrosoft_M365_Core_Portable_EventLog\)\\lib\\";
                string[] inserts6 =
                    {
                    @"<Reference Include=""Microsoft.M365.Core.Portable.EventLog.dll"">",
                    @"  <HintPath>$(PkgMicrosoft_M365_Core_Portable_EventLog)\lib\netstandard2.0\Microsoft.M365.Core.Portable.EventLog.dll</HintPath>",
                    @"</Reference>"
                };
                fileUtils.InsertWhenAndNot(whenNot6, when6, inserts6);

                string when7 = @" \$\(PkgMicrosoft_M365_Core_EventLog\)\\lib\\net462\\Microsoft\.M365\.Core\.EventLog\.dll ";
                string whenNot7 = @" \$\(PkgSystem_Diagnostics_EventLog\)\\lib\\net461\\System\.Diagnostics\.EventLog\.dll ";
                string inserts7 = @" $(PkgSystem_Diagnostics_EventLog)\lib\net461\System.Diagnostics.EventLog.dll";
                fileUtils.InsertWhenAndNotNoFixSpaces(whenNot7, when7, inserts7);

                string when8 = @" \$\(PkgMicrosoft_M365_Core_EventLog\)\\lib\\net462\\Microsoft\.M365\.Core\.EventLog\.dll ";
                string whenNot8 = @" \$\(PkgMicrosoft_M365_Core_Portable_EventLog\)\\lib\\netstandard2\.0\\Microsoft\.M365\.Core\.Portable\.EventLog\.dll ";
                string inserts8 = @" $(PkgMicrosoft_M365_Core_Portable_EventLog)\lib\netstandard2.0\Microsoft.M365.Core.Portable.EventLog.dll";
                fileUtils.InsertWhenAndNotNoFixSpaces(whenNot8, when8, inserts8);

                string when9 = @"\s+\<Copy SourceFiles=""\$\(PkgMicrosoft_M365_Core_EventLog\)\\lib\\net462\\Microsoft\.M365\.Core\.EventLog\.dll""";
                string whenNot9 = @"\<Copy SourceFiles=""\$\(PkgMicrosoft_M365_Core_Portable_EventLog\)\\lib\\";
                var inserts9 = new string[] { @"<Copy SourceFiles=""$(PkgMicrosoft_M365_Core_Portable_EventLog)\lib\netstandard2.0\Microsoft.M365.Core.Portable.EventLog.dll"" DestinationFolder=""$(DISTRIB_PRIVATE_BIN_PATH)"" SkipUnchangedFiles=""true"" />" };
                fileUtils.InsertWhenAndNot(whenNot9, when9, inserts9);

                string when10 = @"\s+\<Copy SourceFiles=""\$\(PkgMicrosoft_M365_Core_EventLog\)\\lib\\net462\\Microsoft\.M365\.Core\.EventLog\.dll""";
                string whenNot10 = @"\<Copy SourceFiles=""\$\(PkgSystem_Diagnostics_EventLog\)\\lib\\";
                var inserts10 = new string[] { @"<Copy SourceFiles=""$(PkgSystem_Diagnostics_EventLog)\lib\net461\System.Diagnostics.EventLog.dll"" DestinationFolder=""$(DISTRIB_PRIVATE_BIN_PATH)"" SkipUnchangedFiles=""true"" />" };
                fileUtils.InsertWhenAndNot(whenNot10, when10, inserts10);

                string when11 = @"\s+\<Delete Files=""\$\(DISTRIB_PRIVATE_BIN_PATH\)\\Microsoft\.M365\.Core\.EventLog\.dll""\s+\/\>";
                string whenNot11 = @"\<Delete Files=""\$\(DISTRIB_PRIVATE_BIN_PATH\)\\System\.Diagnostics\.EventLog\.dll"" \/\>";
                var inserts11 = new string[] { @"<Delete Files=""$(DISTRIB_PRIVATE_BIN_PATH)\System.Diagnostics.EventLog.dll"" />" };
                fileUtils.InsertWhenAndNot(whenNot11, when11, inserts11);

                string when12 = @"\s+\<Delete Files=""\$\(DISTRIB_PRIVATE_BIN_PATH\)\\Microsoft\.M365\.Core\.EventLog\.dll""\s+\/\>";
                string whenNot12 = @"\<Delete Files=""\$\(DISTRIB_PRIVATE_BIN_PATH\)\\Microsoft\.M365\.Core\.Portable\.EventLog\.dll"" \/\>";
                var inserts12 = new string[] { @"<Delete Files=""$(DISTRIB_PRIVATE_BIN_PATH)\Microsoft.M365.Core.Portable.EventLog.dll"" />" };
                fileUtils.InsertWhenAndNot(whenNot12, when12, inserts12);

                string when14 = @"\s*\<Reference Include=""\$\(PkgMicrosoft_M365_Core_EventLog\)\\lib\\net462\\Microsoft\.M365\.Core\.EventLog\.dll"" AllowedUnnecessary=";
                string whenNot14 = @"\<Reference Include=""\$\(PkgMicrosoft_M365_Core_Portable_EventLog\)\\lib\\netstandard2\.0\\Microsoft\.M365\.Core\.Portable\.EventLog\.dll""";
                var inserts14 = new string[] { @"<Reference Include=""$(PkgMicrosoft_M365_Core_Portable_EventLog)\lib\netstandard2.0\Microsoft.M365.Core.Portable.EventLog.dll"" AllowedUnnecessary=""true"" />" };
                fileUtils.InsertWhenAndNot(whenNot14, when14, inserts14);

                string when15 = @"\s*\<Reference Include=""\$\(PkgMicrosoft_M365_Core_EventLog\)\\lib\\net462\\Microsoft\.M365\.Core\.EventLog\.dll"" AllowedUnnecessary=";
                string whenNot15 = @"\<Reference Include=""\$\(PkgSystem_Diagnostics_EventLog\)\\lib\\net461\\System\.Diagnostics\.EventLog\.dll""";
                var inserts15 = new string[] { @"<Reference Include=""$(PkgSystem_Diagnostics_EventLog)\lib\net461\System.Diagnostics.EventLog.dll"" AllowedUnnecessary=""true"" />" };
                fileUtils.InsertWhenAndNot(whenNot15, when15, inserts15);

                string when141 = @"\s*\<Reference Include=""\$\(PkgMicrosoft_M365_Core_EventLog\)\\lib\\net462\\Microsoft\.M365\.Core\.EventLog\.dll""\s*\/\>";
                string whenNot141 = @"Microsoft\.M365\.Core\.Portable\.EventLog";
                var inserts141 = new string[] { @"<Reference Include=""$(PkgMicrosoft_M365_Core_Portable_EventLog)\lib\netstandard2.0\Microsoft.M365.Core.Portable.EventLog.dll"" />" };
                fileUtils.InsertWhenAndNotByGroup(when141, whenNot141, when141, inserts141);
                string whenNot142 = @"System\.Diagnostics\.EventLog";
                var inserts142 = new string[] { @"<Reference Include=""$(PkgSystem_Diagnostics_EventLog)\lib\net461\System.Diagnostics.EventLog.dll"" />" };
                fileUtils.InsertWhenAndNotByGroup(when141, whenNot142, when141, inserts142);

                string when143 = @"\s*\<Reference Include=""\$\(PkgMicrosoft_M365_Core_EventLog\)\\lib\\net462\\Microsoft\.M365\.Core\.EventLog\.dll"" \/\>";
                string whenNot143 = @"Microsoft\.M365\.Core\.Portable\.EventLog";
                var inserts143 = new string[] { @"<Reference Include=""$(PkgMicrosoft_M365_Core_Portable_EventLog)\lib\netstandard2.0\Microsoft.M365.Core.Portable.EventLog.dll"" />" };
                fileUtils.InsertWhenAndNotByGroup(when143, whenNot143, when143, inserts143);
                string whenNot144 = @"System\.Diagnostics\.EventLog";
                var inserts144 = new string[] { @"<Reference Include=""$(PkgSystem_Diagnostics_EventLog)\lib\net461\System.Diagnostics.EventLog.dll"" />" };
                fileUtils.InsertWhenAndNotByGroup(when143, whenNot144, when143, inserts144);

                string when16 = @"\s+\<AssemblyRef Include=""\$\(PkgMicrosoft_M365_Core_EventLog\)\\lib\\net462\\Microsoft\.M365\.Core\.EventLog\.dll"" \/\>";
                string whenNot16 = @"PkgSystem_Diagnostics_EventLog";
                var inserts16 = new string[] { @"<AssemblyRef Include=""$(PkgSystem_Diagnostics_EventLog)\lib\net461\System.Diagnostics.EventLog.dll"" />" };
                fileUtils.InsertWhenAndNotByGroup(when16, whenNot16, when16, inserts16);

                string when17 = @"\s+\<AssemblyRef Include=""\$\(PkgMicrosoft_M365_Core_EventLog\)\\lib\\net462\\Microsoft\.M365\.Core\.EventLog\.dll"" \/\>";
                string whenNot17 = @"PkgMicrosoft_M365_Core_Portable_EventLog";
                var inserts17 = new string[] { @"<AssemblyRef Include=""$(PkgMicrosoft_M365_Core_Portable_EventLog)\lib\netstandard2.0\Microsoft.M365.Core.Portable.EventLog.dll"" />" };
                fileUtils.InsertWhenAndNotByGroup(when17, whenNot17, when17, inserts17);

                string when18 = @"\s+\<SandBoxDependencies Include=""\$\(PkgMicrosoft_M365_Core_EventLog\)\\lib\\net462\\Microsoft\.M365\.Core\.EventLog\.dll"" \/\>";
                string whenNot18 = @"PkgSystem_Diagnostics_EventLog";
                var inserts18 = new string[] { @"<SandBoxDependencies Include=""$(PkgSystem_Diagnostics_EventLog)\lib\net461\System.Diagnostics.EventLog.dll"" />" };
                fileUtils.InsertWhenAndNotByGroup(when18, whenNot18, when18, inserts18);

                string when19 = @"\s+\<SandBoxDependencies Include=""\$\(PkgMicrosoft_M365_Core_EventLog\)\\lib\\net462\\Microsoft\.M365\.Core\.EventLog\.dll"" \/\>";
                string whenNot19 = @"PkgMicrosoft_M365_Core_Portable_EventLog";
                var inserts19 = new string[] { @"<SandBoxDependencies Include=""$(PkgMicrosoft_M365_Core_Portable_EventLog)\lib\netstandard2.0\Microsoft.M365.Core.Portable.EventLog.dll"" />" };
                fileUtils.InsertWhenAndNotByGroup(when19, whenNot19, when19, inserts19);

                string when20 = @"\s+\<Reference Include=""Microsoft.M365.Core.EventLog"" AllowedUnnecessary=""true""\>\s*\<HintPath\>\$\(PkgMicrosoft_M365_Core_EventLog\)\\lib\\";
                string whenNot20 = @"PkgSystem_Diagnostics_EventLog";
                var inserts20 = new string[] {
                    @"<Reference Include=""System.Diagnostics.EventLog"" AllowedUnnecessary=""true"">",
                    @"  <HintPath>$(PkgSystem_Diagnostics_EventLog)\lib\net461\System.Diagnostics.EventLog.dll</HintPath>",
                    @"</Reference>"
                };
                fileUtils.InsertWhenAndNotByGroup(when20, whenNot20, when20, inserts20);
                string whenNot201 = @"PkgMicrosoft_M365_Core_Portable_EventLog";
                var inserts201 = new string[] {
                    @"<Reference Include=""Microsoft.M365.Core.Portable.EventLog"" AllowedUnnecessary=""true"">",
                    @"  <HintPath>$(PkgMicrosoft_M365_Core_Portable_EventLog)\lib\netstandard2.0\Microsoft.M365.Core.Portable.EventLog.dll</HintPath>",
                    @"</Reference>"
                };
                fileUtils.InsertWhenAndNotByGroup(when20, whenNot201, when20, inserts201);

                string when21 = @"\s+\<Reference Include=""Microsoft.M365.Core.EventLog""\s*\>\s*\<HintPath\>\$\(PkgMicrosoft_M365_Core_EventLog\)\\lib\\";
                string whenNot21 = @"System\.Diagnostics\.EventLog";
                var inserts21 = new string[] {
                    @"<Reference Include=""System.Diagnostics.EventLog"">",
                    @"  <HintPath>$(PkgSystem_Diagnostics_EventLog)\lib\net461\System.Diagnostics.EventLog.dll</HintPath>",
                    @"</Reference>"
                };
                fileUtils.InsertWhenAndNotByGroup(when21, whenNot21, when21, inserts21);
                string whenNot211 = @"Microsoft\.M365\.Core\.Portable\.EventLog";
                var inserts211 = new string[] {
                    @"<Reference Include=""Microsoft.M365.Core.Portable.EventLog"">",
                    @"  <HintPath>$(PkgMicrosoft_M365_Core_Portable_EventLog)\lib\netstandard2.0\Microsoft.M365.Core.Portable.EventLog.dll</HintPath>",
                    @"</Reference>"
                };
                fileUtils.InsertWhenAndNotByGroup(when20, whenNot211, when20, inserts211);

                string when2111 = @"\s+\<Reference Include=""Microsoft\.M365\.Core\.EventLog\.dll""\s*\>\s*\<HintPath\>\$\(PkgMicrosoft_M365_Core_EventLog\)\\lib\\";
                fileUtils.InsertWhenAndNotByGroup(when2111, whenNot21, when2111, inserts21);
                fileUtils.InsertWhenAndNotByGroup(when2111, whenNot211, when2111, inserts211);

                string when22 = @"\s*\<PowerShellWrapperGenLookupOnly Include=""\$\(PkgMicrosoft_M365_Core_EventLog\)\\lib\\net462\\Microsoft\.M365\.Core\.EventLog\.dll""";
                string whenNot22 = @"\<PowerShellWrapperGenLookupOnly Include=""\$\(PkgMicrosoft_M365_Core_Portable_EventLog\)\\lib\\netstandard2\.0\\Microsoft\.M365\.Core\.Portable\.EventLog\.dll""";
                var inserts22 = new string[] { @"<PowerShellWrapperGenLookupOnly Include=""$(PkgMicrosoft_M365_Core_Portable_EventLog)\lib\netstandard2.0\Microsoft.M365.Core.Portable.EventLog.dll"" />" };
                fileUtils.InsertWhenAndNotByGroup(when22, whenNot22, when22, inserts22);
                string whenNot221 = @"\<PowerShellWrapperGenLookupOnly Include=""\$\(PkgSystem_Diagnostics_EventLog\)\\lib\\net461\\System\.Diagnostics\.EventLog\.dll""";
                var inserts221 = new string[] { @"<PowerShellWrapperGenLookupOnly Include=""$(PkgSystem_Diagnostics_EventLog)\lib\net461\System.Diagnostics.EventLog.dll"" />" };
                fileUtils.InsertWhenAndNotByGroup(when22, whenNot221, when22, inserts221);

                if (
                 path.Contains("services\\src\\EwsSerializersGeneratorPostProcessing\\EwsSerializersGeneratorPostProcessing.csproj", StringComparison.OrdinalIgnoreCase) ||
                 path.Contains("sources\\test\\tools\\src\\Titan\\Internal.Exchange.Test.TitanCommon.csproj", StringComparison.OrdinalIgnoreCase)
               )
                {
                    string when13 = @"\s+\<PackageReference Include=""Microsoft\.M365\.Core\.EventLog"" GeneratePathProperty=""true"" \/\>";
                    string whenNot13 = @"\s+\<PackageReference Include=""Microsoft\.M365\.Core\.Portable\.EventLog"" GeneratePathProperty=""true"" \/\>";
                    var inserts13 = new string[] {
                        @"<PackageReference Include=""System.Diagnostics.EventLog"" GeneratePathProperty=""true"" />",
                        "<PackageReference Include=\"Microsoft.M365.Core.Portable.EventLog\" GeneratePathProperty=\"true\" />"
                    };
                    fileUtils.InsertWhenAndNot(whenNot13, when13, inserts13);
                }

                bool sdk = fileUtils.Content.Contains("Sdk=\"Microsoft.NET.Sdk", StringComparison.OrdinalIgnoreCase);

                if (!sdk)
                {
                    fileUtils.InsertWhenAndNotByGroup(matches);
                }
                else
                {
                    fileUtils.SkipFile = "SDK";
                }

                await OtherCsproj(fileUtils, sdk);

                result = await fileUtils.SaveResult();
                return result;
            });
        }

        private async Task OtherCsproj(FileMatch fileMatch, bool isSDK)
        {
            string WhenPerseus = @"\s*\<Import Project=""\$\(BranchTargetsPath\)\\Test\\Perseus\\Perseus\.DataTypes\.targets"" \/\>";
            string[] PerseusInserts =
                {
                    "<Reference Include=\"$(PkgOSS_Build_ExchangeTestStudio)\\ExchangeTestStudio\\Content\\Perseus.DataTypes.dll\" />",
                    "<Reference Include=\"$(PkgOSS_Build_ExchangeTestStudio)\\ExchangeTestStudio\\Content\\TcUtils.dll\" />"
                };
            if (fileMatch.IsMatchAll(WhenPerseus,
                @"\<Reference Include=""\$\(PkgMicrosoft_M365_Core_EventLog\)\\lib\\",
                @"\<Reference Include=""\$\(PkgMicrosoft_M365_Core_Portable_EventLog\)\\lib\\")
                && !isSDK)
            {
                string[] when = { @"\s*\<Reference Include=""\$\(PkgMicrosoft_M365_Core_EventLog\)\\lib\\",
                @"\s*\<Reference Include=""Microsoft\.M365\.Core\.EventLog""\>"};
                string whenNot = @"Perseus.DataTypes.dll";
                fileMatch.InsertWhenNot(whenNot, when, PerseusInserts);
                fileMatch.Replace(WhenPerseus, "");
            }

            if (fileMatch.Path.Contains("\\sources\\dev\\common\\src\\BinPlaceForPackages", StringComparison.OrdinalIgnoreCase))
            {
                string when = @"\s*\<PackageReference Include=""Microsoft\.M365\.Core\.EventLog""";
                string whenNot = @"\<PackageReference Include=""Microsoft\.M365\.Core\.Portable\.EventLog""";
                string[] inserts =
                    {
                    "<PackageReference Include=\"Microsoft.M365.Core.Portable.EventLog\" GeneratePathProperty=\"true\" />"
                };

                fileMatch.InsertWhenAndNotByGroup(when, whenNot, when, inserts);

                string when2 = @"\s*\<BinPlaceNetCore Include=""\$\(PkgMicrosoft_M365_Core_EventLog\)\\lib\\";
                string whenNot2 = @"PkgMicrosoft_M365_Core_Portable_EventLog";
                string[] inserts2 =
                    {
                    "<BinPlaceNetCore Include=\"$(PkgMicrosoft_M365_Core_Portable_EventLog)\\lib\\netstandard2.0\\Microsoft.M365.Core.Portable.EventLog.dll\" />"
                };

                fileMatch.InsertWhenAndNotByGroup(when2, whenNot2, when2, inserts2);
            }
            else if (fileMatch.Path.Contains("Test\\MExAgents\\src\\UnifiedGroupAgent\\UnitTests", StringComparison.OrdinalIgnoreCase))
            {
                string when = @"\s*\<PackageReference Include=""Microsoft.NET.Test.Sdk""";
                string whenNot = @"\<PackageReference Include=""System.Diagnostics.EventLog""";
                string[] inserts =
                    {
                    "<PackageReference Include=\"System.Diagnostics.EventLog\" NoWarn=\"NU1605\" />"
                };
                fileMatch.InsertWhenNot(whenNot, when, inserts);
                fileMatch.Replace(WhenPerseus, "");
            }
            else if (fileMatch.Path.Contains("test\\Hygiene\\src\\ThreatIntel\\AirUnitTests", StringComparison.OrdinalIgnoreCase))
            {
                string pc = Path.Combine(AppSettings.RootPath, "sources\\test\\hygiene\\src\\threatintel\\airunittests\\teamsmessageentitymodeltests.cs");
                FileMatch filec = await FileMatch.ReadFileAsync(pc);
                filec.Replace(@"\s*using Perseus.DataTypes;", "");
                await filec.SaveResult();

                string when = @"\s*\<Reference Include=""\$\(PkgMicrosoft_M365_Core_EventLog\)\\lib\\";
                string whenNot = @"Perseus.DataTypes.dll";
                fileMatch.InsertWhenNot(whenNot, when, PerseusInserts);
                fileMatch.Replace(WhenPerseus, "");
            }
            else if (fileMatch.Path.Contains("dev\\Hygiene\\src\\Webstore\\MSICustomActions\\MSICustomActions.csproj", StringComparison.OrdinalIgnoreCase))
            {
                string when = @"\s*""\$\(PkgMicrosoft_M365_Core_DiagnosticsLog\)\\lib\\";
                string whenNot = @"Microsoft\.M365\.Core\.Portable\.EventLog";
                string[] inserts =
                    {
                    @"""$(PkgSystem_Diagnostics_EventLog)\lib\net461\System.Diagnostics.EventLog.dll"" ^",
                    @"""$(PkgMicrosoft_M365_Core_Portable_EventLog)\lib\netstandard2.0\Microsoft.M365.Core.Portable.EventLog.dll"" ^"
                };
                fileMatch.InsertWhenAndNot(whenNot, when, inserts);
            }
            else if (fileMatch.Path.Contains("dev\\Hygiene\\src\\Webstore\\Client\\", StringComparison.OrdinalIgnoreCase))
            {
                string when = @"\s*\<PackageReference Include=""System\.Diagnostics\.EventLog"" \/\>";
                string whenNot = @"Microsoft\.M365\.Core\.Portable\.EventLog";
                string[] inserts =
                    {
                    "<PackageReference Include=\"Microsoft.M365.Core.Portable.EventLog\" />"
                };
                fileMatch.InsertWhenNot(whenNot, when, inserts);
            }
            else if (fileMatch.Path.Contains("sources\\Test\\Hygiene\\src\\SmartScreen\\SpamEngineExtensions\\FunctionalTests", StringComparison.OrdinalIgnoreCase)
                || fileMatch.Path.Contains("sources\\Test\\Hygiene\\src\\DataInsights\\Common\\UnitTests", StringComparison.OrdinalIgnoreCase)
                || fileMatch.Path.Contains("sources\\Test\\EDiscovery\\src\\TaskDistributionSystem", StringComparison.OrdinalIgnoreCase)
                || fileMatch.Path.Contains("sources\\Test\\Hygiene\\src\\SmartLinks\\UnitTests\\SpamEngineExtensions", StringComparison.OrdinalIgnoreCase)
                || fileMatch.Path.Contains("sources\\Test\\Hygiene\\src\\MailTagRetrieval\\UnitTests", StringComparison.OrdinalIgnoreCase)
                || fileMatch.Path.Contains("sources\\Test\\OfficeGraph\\Src\\SecondaryCopyQuotaManagement\\Zerobox", StringComparison.OrdinalIgnoreCase)
                || fileMatch.Path.Contains("sources\\Test\\Services\\src\\RestUnitTests\\MailboxItemUnitTests", StringComparison.OrdinalIgnoreCase)
                || fileMatch.Path.Contains("sources\\Test\\Hygiene\\src\\SmartScreen\\SpamEngineExtensions\\Unittests", StringComparison.OrdinalIgnoreCase))
            {
                string[] when = { @"\s*\<Reference Include=""\$\(PkgMicrosoft_M365_Core_EventLog\)\\lib\\",
                @"\s*\<Reference Include=""Microsoft\.M365\.Core\.EventLog""\>"};
                string whenNot = @"Perseus.DataTypes.dll";
                string[] inserts =
                    {
                    "<Reference Include=\"$(PkgOSS_Build_ExchangeTestStudio)\\ExchangeTestStudio\\Content\\Perseus.DataTypes.dll\" />",
                    "<Reference Include=\"$(PkgOSS_Build_ExchangeTestStudio)\\ExchangeTestStudio\\Content\\TcUtils.dll\" />"
                };
                fileMatch.InsertWhenNot(whenNot, when, inserts);
                fileMatch.Replace(WhenPerseus, "");
            }
        }

        public void ResolveNoprojFiles()
        {
            this.ResolveFilesTask(scan.NoprojFiles, async (path) =>
            {
                FileMatch fileUtils = await FileMatch.ReadFileAsync(path, false);
                string when = @"\s*\<Copy .*Microsoft\.M365\.Core\.EventLog\.dll";
                string whenNot = @"\<Copy .*System\.Diagnostics\.EventLog\.dll";
                string[] inserts =
                {
                    "<Copy SourceFiles=\"$(PkgMicrosoft_M365_Core_Portable_EventLog)\\lib\\netstandard2.0\\Microsoft.M365.Core.Portable.EventLog.dll\" DestinationFolder=\"$(BinDir)\" />",
                    "<Copy SourceFiles=\"$(PkgSystem_Diagnostics_EventLog)\\lib\\net461\\System.Diagnostics.EventLog.dll\" DestinationFolder=\"$(BinDir)\" />"
                };
                fileUtils.InsertWhenAndNot(whenNot, when, inserts);

                string when2 = @"\s*\<QCustomInput .*Microsoft\.M365\.Core\.EventLog\.dll";
                string whenNot2 = @"\<QCustomInput .*System\.Diagnostics\.EventLog\.dll";
                string[] inserts2 =
                {
                    "<QCustomInput Include=\"$(PkgMicrosoft_M365_Core_Portable_EventLog)\\lib\\netstandard2.0\\Microsoft.M365.Core.Portable.EventLog.dll\">",
                    "  <Visible>false</Visible>",
                    "</QCustomInput>",
                    "<QCustomInput Include=\"$(PkgSystem_Diagnostics_EventLog)\\lib\\net461\\System.Diagnostics.EventLog.dll\">",
                    "  <Visible>false</Visible>",
                    "</QCustomInput>"
                };
                fileUtils.InsertWhenAndNot(whenNot2, when2, inserts2);

                string when3 = @"\s*\<SandBoxDependencies .*Microsoft\.M365\.Core\.EventLog\.dll";
                string whenNot3 = @"\<SandBoxDependencies .*System\.Diagnostics\.EventLog\.dll";
                string[] inserts3 =
                {
                    "<SandBoxDependencies Include=\"$(PkgMicrosoft_M365_Core_Portable_EventLog)\\lib\\netstandard2.0\\Microsoft.M365.Core.Portable.EventLog.dll\" />",
                    "<SandBoxDependencies Include=\"$(PkgSystem_Diagnostics_EventLog)\\lib\\net461\\System.Diagnostics.EventLog.dll\" />"
                };
                fileUtils.InsertWhenAndNot(whenNot3, when3, inserts3);

                string when4 = @"\s*\<NonDistribSchemaValidatorReferencesFileNames .*Microsoft\.M365\.Core\.EventLog\.dll";
                string whenNot4 = @"\<NonDistribSchemaValidatorReferencesFileNames .*System\.Diagnostics\.EventLog\.dll";
                string[] inserts4 =
                {
                    "<NonDistribSchemaValidatorReferencesFileNames Include=\"$(PkgMicrosoft_M365_Core_Portable_EventLog)\\lib\\netstandard2.0\\Microsoft.M365.Core.Portable.EventLog.dll\" />",
                    "<NonDistribSchemaValidatorReferencesFileNames Include=\"$(PkgSystem_Diagnostics_EventLog)\\lib\\net461\\System.Diagnostics.EventLog.dll\" />"
                };
                fileUtils.InsertWhenAndNot(whenNot4, when4, inserts4);

                return await fileUtils.SaveResult();
            });
        }

        public void ResolveNupkgProjFiles()
        {
            this.ResolveFilesTask(scan.NupkgProjFiles, async (path) =>
            {
                string when = @"\s*\<Content .*Microsoft\.M365\.Core\.EventLog\.dll";
                string whenNot = @"\<Content .*System\.Diagnostics\.EventLog\.dll";
                string[] inserts =
                {
                    "<Content Include=\"$(PkgMicrosoft_M365_Core_Portable_EventLog)\\lib\\netstandard2.0\\Microsoft.M365.Core.Portable.EventLog.dll\">",
                    "  <Link>OwsSchemaValidator\\lib\\%(Filename)%(Extension)</Link>",
                    "</Content>",
                    "<Content Include=\"$(PkgSystem_Diagnostics_EventLog)\\lib\\net461\\System.Diagnostics.EventLog.dll\">",
                    "  <Link>exchangetypedll\\System.Diagnostics.EventLog.dll</Link>",
                    "</Content>"
                };

                if (path.EndsWith("OwaServer.nupkg.proj"))
                {
                    inserts[1] = "  <Link>lib\\1.0\\%(Filename)%(Extension)</Link>";
                    inserts[4] = "  <Link>lib\\1.0\\%(Filename)%(Extension)</Link>";
                }
                else if (path.EndsWith("TorusClientExchangeDependency.nupkg.proj"))
                {
                    inserts[1] = "  <Link>exchangetypedll\\Microsoft.M365.Core.Portable.EventLog.dll</Link>";
                    inserts[4] = "  <Link>exchangetypedll\\System.Diagnostics.EventLog.dll</Link>";
                }
                else if (path.EndsWith("Microsoft.Exchange.Compliance.KqlUtilities.nupkg.proj"))
                {
                    inserts[1] = "  <Link>lib\\net47\\%(Filename)%(Extension)</Link>";
                    inserts[4] = "  <Link>lib\\net47\\%(Filename)%(Extension)</Link>";
                }
                else if (path.EndsWith("Microsoft.Exchange.AntiSpam.SpamEngine.SmartScreenExtensibility.nupkg.proj"))
                {
                    inserts[1] = "  <Link>lib\\%(Filename)%(Extension)</Link>";
                    inserts[4] = "  <Link>lib\\%(Filename)%(Extension)</Link>";
                }
                else if (path.EndsWith("ODL.nupkg.proj"))
                {
                    inserts[1] = "  <Link>Core\\OfficeDataLoaderV2\\%(Filename)%(Extension)</Link>";
                    inserts[4] = "  <Link>Core\\OfficeDataLoaderV2\\%(Filename)%(Extension)</Link>";
                }
                else if (path.EndsWith("OrchestratorHost.nupkg.proj"))
                {
                    inserts[1] = "  <Link>Dropbox\\%(Filename)%(Extension)</Link>";
                    inserts[4] = "  <Link>Dropbox\\%(Filename)%(Extension)</Link>";
                }
                else if (path.EndsWith("NetworkManager.nupkg.proj"))
                {
                    inserts[1] = "  <Link>lib\\Microsoft.M365.Core.Portable.EventLog\\Microsoft.M365.Core.Portable.EventLog.dll</Link>";
                    inserts[4] = "  <Link>lib\\System.Diagnostics.EventLog\\System.Diagnostics.EventLog.dll</Link>";
                }
                else if (path.EndsWith("NetworkManager.nupkg.proj"))
                {
                    inserts[1] = "  <Link>exchangetypedll\\Microsoft.M365.Core.Portable.EventLog.dll</Link>";
                    inserts[4] = "  <Link>exchangetypedll\\System.Diagnostics.EventLog.dll</Link>";
                }
                else if (path.EndsWith("Microsoft.Exchange.AntiSpam.Common.Core.nupkg.proj"))
                {
                    inserts[1] = "  <Link>lib\\net472\\%(Filename)%(Extension)</Link>";
                    inserts[4] = "  <Link>lib\\net472\\%(Filename)%(Extension)</Link>";
                }
                else
                {
                    inserts = null;
                }

                FileMatch fileUtils = await FileMatch.ReadFileAsync(path, false);
                fileUtils.InsertWhenAndNot(whenNot, when, inserts);
                return await fileUtils.SaveResult();
            });
        }

        public void ResolveNuspecFiles()
        {
            this.ResolveFilesTask(scan.NuspecFiles, async (path) =>
            {
                FileMatch fileUtils = await FileMatch.ReadFileAsync(path, false);

                string when = @"\s*\<VariantConfigurationDependencies .*Microsoft\.M365\.Core\.EventLog\.dll";
                string whenNot = @"\<VariantConfigurationDependencies .*Microsoft.M365.Core.Portable.EventLog.dll";
                string[] inserts =
                {
                    "<VariantConfigurationDependencies Include=\"$(PkgMicrosoft_M365_Core_Portable_EventLog)\\lib\\netstandard2.0\\Microsoft.M365.Core.Portable.EventLog.dll\" />"
                };
                fileUtils.InsertWhenAndNot(whenNot, when, inserts);

                string when2 = @"\s*\<PackageReference .*Microsoft\.M365\.Core\.EventLog";
                string whenNot2 = @"\<PackageReference .*Microsoft.M365.Core.Portable.EventLog";
                string[] inserts2 =
                {
                    "<PackageReference Include=\"Microsoft.M365.Core.Portable.EventLog\" GeneratePathProperty=\"true\"/>"
                };
                fileUtils.InsertWhenAndNot(whenNot2, when2, inserts2);

                return await fileUtils.SaveResult();
            });
        }

        public void ResolveTargetsFiles()
        {
            this.ResolveFilesTask(scan.TargetsFiles, async (path) =>
            {
                FileMatch fileUtils = await FileMatch.ReadFileAsync(path, false);

                string when = @"\s*\<PackageReference .*Microsoft\.M365\.Core\.EventLog";
                string whenNot = @"\<PackageReference .*Microsoft.M365.Core.Portable.EventLog";
                string[] inserts =
                {
                    "<PackageReference Include=\"Microsoft.M365.Core.Portable.EventLog\" GeneratePathProperty=\"true\"/>"
                };
                fileUtils.InsertWhenAndNotByGroup(when, whenNot, when, inserts);

                string when2 = @"\s*\<VariantConfigurationDependencies .*Microsoft\.M365\.Core\.EventLog\.dll";
                string whenNot2 = @"\<VariantConfigurationDependencies .*Microsoft.M365.Core.Portable.EventLog.dll";
                string[] inserts2 =
                {
                    "<VariantConfigurationDependencies Include=\"$(PkgMicrosoft_M365_Core_Portable_EventLog)\\lib\\netstandard2.0\\Microsoft.M365.Core.Portable.EventLog.dll\" />"
                };
                fileUtils.InsertWhenAndNotByGroup(when2, whenNot2, when2, inserts2);

                return await fileUtils.SaveResult();
            });
        }

        public void ResolveBatFiles()
        {
            this.ResolveFilesTask(scan.BatFiles, async (path) =>
            {
                FileMatch fileUtils = await FileMatch.ReadFileAsync(path, false);

                string when = @"\s*Microsoft.M365.Core.EventLog.dll\^";
                string whenNot = @"Microsoft.M365.Core.Portable.EventLog.dll\^";
                string[] inserts =
                {
                    "Microsoft.M365.Core.Portable.EventLog.dll^",
                    "System.Diagnostics.EventLog.dll^"
                };

                fileUtils.InsertWhenAndNot(whenNot, when, inserts);

                return await fileUtils.SaveResult();
            });
        }

        public void ResolvePs1Files()
        {
            this.ResolveFilesTask(scan.Ps1Files, async (path) =>
            {
                FileMatch fileUtils = await FileMatch.ReadFileAsync(path, false);

                string when = @"\s*""Microsoft.M365.Core.EventLog.dll""";
                string whenNot = @"""Microsoft.M365.Core.Portable.EventLog.dll""";
                string[] inserts =
                {
                    "\"System.Diagnostics.EventLog.dll\",",
                    "\"Microsoft.M365.Core.Portable.EventLog.dll\","
                };

                fileUtils.InsertWhenAndNot(whenNot, when, inserts);

                string when2 = @"\s*""Bin\\Microsoft.M365.Core.EventLog.dll""";
                string whenNot2 = @"""Bin\\Microsoft.M365.Core.Portable.EventLog.dll""";
                string[] inserts2 =
                {
                    "\"Bin\\System.Diagnostics.EventLog.dll\",",
                    "\"Bin\\Microsoft.M365.Core.Portable.EventLog.dll\","
                };

                fileUtils.InsertWhenAndNot(whenNot2, when2, inserts2);

                return await fileUtils.SaveResult();
            });
        }
    }
}