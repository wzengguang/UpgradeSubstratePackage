namespace UpgradeSubstrateTargetVersion
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            SubstrateScanUtil scan = await SubstrateScanUtil.Scan();

            await ResolveFilesOfEventLog.Resolve(scan);
            LogUtil.SaveLogFile();
        }
    }
}