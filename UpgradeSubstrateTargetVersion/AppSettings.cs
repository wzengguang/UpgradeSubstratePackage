namespace UpgradeSubstrateTargetVersion
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    internal class AppSettings
    {
        public static string RootPath = "C:\\O365\\SB\\src";

        public static string EventLog = @"Microsoft\.M365\.Core\.EventLog";

        public static string DiagnosticsEventLog = @"System\.Diagnostics\.EventLog";

        public static string Portable = @"Microsoft\.M365\.Core\.Portable\.EventLog";
        public static string Perseus = @"Perseus\.DataTypes\.targets";
    }
}
