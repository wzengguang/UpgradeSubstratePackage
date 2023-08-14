
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
                    fileMatch.Insert(item);
                }
                return await fileMatch.SaveResult();
            });
        }

        public void ResolveConfigFiles()
        {
            ResolveFilesTask(scan.ConfigFiles, async (path) =>
            {
                FileMatch fileMatch = await FileMatch.ReadFileAsync(path);

                var matchparams = MatchParam.Load("match/config.xml");
                foreach (var item in matchparams)
                {
                    fileMatch.Insert(item);
                }
                return await fileMatch.SaveResult();
            });
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
            List<string> exclude = MatchParam.Load("match/csproj.exclude.xml").FirstOrDefault().Path;
            List<MatchUtil> matches = MatchUtil.GetMatchs("Data/csproj");
            List<MatchParam> inserts = MatchParam.Load("match/csproj.insert.xml");
            List<MatchParam> replace = MatchParam.Load("match/csproj.replace.xml");
            List<MatchParam> excludeRmoves = MatchParam.Load("match/csproj.exclude.remove.xml");

            this.ResolveFilesTask(scan.CsprojFiles, async (path) =>
            {
                //if (!path.Contains("Internal.Exchange.Management.ForwardSync.Service.UnitTests.csproj"))
                //{
                //    return null;
                //}

                FileMatch fileMatch = await FileMatch.ReadFileAsync(path, false);
                if (fileMatch.IsSDKProject)
                {
                    return null;
                }

                if (path.ContainIgnoreCase(exclude))
                {
                    fileMatch.SkipFile = "Portable";
                    fileMatch.Replace(excludeRmoves);
                }
                else
                {
                    fileMatch.Insert(inserts);
                    fileMatch.Replace(replace);
                }

                return await fileMatch.SaveResult();
            });
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