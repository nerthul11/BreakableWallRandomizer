using BreakableWallRandomizer.Settings;
using BreakableWallRandomizer.Manager;
using BreakableWallRandomizer.Interop;
using Modding;
using System;

namespace BreakableWallRandomizer
{
    public class BreakableWallRandomizer : Mod, IGlobalSettings<BWR_Settings>
    {
        new public string GetName() => "Breakable Wall Randomizer";
        public override string GetVersion() => "4.0.0.0";
        public BWR_Settings GS { get; set; } = new();
        private static BreakableWallRandomizer _instance;
        public BreakableWallRandomizer() : base()
        {
            _instance = this;
        }
        internal static BreakableWallRandomizer Instance
        {
            get
            {
                if (_instance == null)
                {
                    throw new InvalidOperationException($"{nameof(BreakableWallRandomizer)} was never initialized");
                }
                return _instance;
            }
        }
        public override void Initialize()
        {
            Log("Initializing...");
            BWR_Manager.Hook();

            if (ModHooks.GetMod("FStatsMod") is Mod)
                FStats_Interop.Hook();

            if (ModHooks.GetMod("Randomizer 4") is Mod)
            { 
                if (ModHooks.GetMod("MoreLocations") is Mod)
                    MoreLocations_Interop.Hook();
                
                if (ModHooks.GetMod("RandoSettingsManager") is Mod)
                    RSM_Interop.Hook();
            }

            CondensedSpoilerLogger.AddCategory("Grub Walls", () => BWR_Manager.Settings.Enabled, 
                [
                    "Dive_Floor-Basin_Grub",
                    "Wall-Catacombs_Grub",
                    "Wall-Crossroads_Grub",
                    "Wall-Deepnest_Mimics",
                    "Wall-Deepnest_Springs_Grub",
                    "Wall-Edge_Camp_Grub",
                    "Wall-Peak_Mimic",
                    "Wall-Waterways_Grub"
                ]
            );

            CondensedSpoilerLogger.AddCategory("Access Walls", () => BWR_Manager.Settings.Enabled, 
                [
                    "Dive_Floor-Flukemarm",
                    "Dive_Floor-Peak_Entrance",
                    "Plank-Colo_Shortcut",
                    "Plank-Edge_Tram_Exit",
                    "Plank-Hive_Exit",
                    "Plank-Nailsmith",
                    "Wall-Hidden_Station",
                    "Wall-Junk_Pit_Entrance",
                    "Wall-Lower_Hive_Entrance",
                    "Wall-Path_of_Pain_Entrance",
                    "Wall-Pleasure_House",
                    "Wall-Shade_Soul_Shortcut",
                    "Wall-Weaver's_Den_Entrance"
                ]
            );
            Log("Initialized.");
        }
        public void OnLoadGlobal(BWR_Settings s) => GS = s;
        public BWR_Settings OnSaveGlobal() => GS;
    }
}
