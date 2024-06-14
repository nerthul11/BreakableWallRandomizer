using RandoSettingsManager;
using RandoSettingsManager.SettingsManagement;
using RandoSettingsManager.SettingsManagement.Versioning;
using BreakableWallRandomizer.Manager;
using BreakableWallRandomizer.Settings;

namespace BreakableWallRandomizer.Interop
{
    internal static class RSM_Interop
    {
        public static void Hook()
        {
            RandoSettingsManagerMod.Instance.RegisterConnection(new AccessSettingsProxy());
        }
    }

    internal class AccessSettingsProxy : RandoSettingsProxy<BWR_Settings, string>
    {
        public override string ModKey => BreakableWallRandomizer.Instance.GetName();

        public override VersioningPolicy<string> VersioningPolicy { get; }
            = new EqualityVersioningPolicy<string>(BreakableWallRandomizer.Instance.GetVersion());

        public override void ReceiveSettings(BWR_Settings settings)
        {
            if (settings != null)
            {
                ConnectionMenu.Instance!.Apply(settings);
            }
            else
            {
                ConnectionMenu.Instance!.Disable();
            }
        }

        public override bool TryProvideSettings(out BWR_Settings settings)
        {
            settings = BWR_Manager.Settings;
            return settings.Enabled;
        }
    }
}