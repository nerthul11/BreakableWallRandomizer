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
        public override string GetVersion() => "3.0.2.3";
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
            Log("Initialized.");
        }
        public void OnLoadGlobal(BWR_Settings s) => GS = s;
        public BWR_Settings OnSaveGlobal() => GS;
    }
}
