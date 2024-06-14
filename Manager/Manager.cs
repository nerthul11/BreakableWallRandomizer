using BreakableWallRandomizer.Settings;
using Newtonsoft.Json;
using RandomizerMod.Logging;

namespace BreakableWallRandomizer.Manager
{
    internal static class BWR_Manager
    {
        public static BWR_Settings Settings => BreakableWallRandomizer.Instance.GS;
        public static void Hook()
        {
            LogicHandler.Hook();
            ItemHandler.Hook();
            ConnectionMenu.Hook();
            SettingsLog.AfterLogSettings += AddFileSettings;
        }

        private static void AddFileSettings(LogArguments args, System.IO.TextWriter tw)
        {
            // Log settings into the settings file
            tw.WriteLine("Breakable Wall Randomizer Settings:");
            using JsonTextWriter jtw = new(tw) { CloseOutput = false };
            RandomizerMod.RandomizerData.JsonUtil._js.Serialize(jtw, Settings);
            tw.WriteLine();
        }
    }
    
    
}