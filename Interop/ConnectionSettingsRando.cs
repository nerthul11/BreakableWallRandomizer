using BreakableWallRandomizer.Manager;
using ConnectionSettingsRando;

namespace BreakableWallRandomizer.Interop
{
    internal static class CSR_Interop
    {
        public static void Hook()
        {
            CSR.Register(
            BreakableWallRandomizer.Instance.GetName(),
            () => BWR_Manager.Settings,
            s => SettingsRandomizer.CopyTo(s, BWR_Manager.Settings));
        }
    }
}