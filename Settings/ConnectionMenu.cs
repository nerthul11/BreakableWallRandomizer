using MenuChanger;
using MenuChanger.MenuElements;
using MenuChanger.MenuPanels;
using MenuChanger.Extensions;
using RandomizerMod.Menu;
using static RandomizerMod.Localization;
using BreakableWallRandomizer.Manager;

namespace BreakableWallRandomizer.Settings
{
    public class ConnectionMenu
    {
        internal MenuPage WallRandoPage;
        internal MenuElementFactory<BWR_Settings> wallMEF;
        internal VerticalItemPanel wallVIP;

        internal SmallButton OpenWallRandoSettings;

        internal static ConnectionMenu Instance { get; private set; }
        private ConnectionMenu(MenuPage landingPage)
        {
            WallRandoPage = new MenuPage("BreakableWallSettings", landingPage);
            wallMEF = new(WallRandoPage, BWR_Manager.Settings);
            wallVIP = new(WallRandoPage, new(0, 350), 75f, true, wallMEF.Elements);

            foreach (IValueElement e in wallMEF.Elements)
            {
                e.SelfChanged += obj => SetTopLevelButtonColor();
            }

            OpenWallRandoSettings = new(landingPage, Localize("Breakable Walls"));
            OpenWallRandoSettings.AddHideAndShowEvent(landingPage, WallRandoPage);

            SetTopLevelButtonColor();
        }

        public static void OnExitMenu()
        {
            Instance = null; 
        }

        public static void Hook()
        {
            RandomizerMenuAPI.AddMenuPage(ConstructMenu, HandleButton);
            MenuChangerMod.OnExitMainMenu += OnExitMenu;
        }

        private static bool HandleButton(MenuPage landingPage, out SmallButton button)
        {
            button = Instance.OpenWallRandoSettings;
            return true;
        }

        private void SetTopLevelButtonColor()
        {
            if (OpenWallRandoSettings != null)
            {
                OpenWallRandoSettings.Text.color = BWR_Manager.Settings.Enabled ? Colors.TRUE_COLOR : Colors.DEFAULT_COLOR;
            }
        }

        private static void ConstructMenu(MenuPage landingPage) => Instance = new(landingPage);

        public void Disable() => wallMEF.ElementLookup[nameof(BWR_Settings.Enabled)].SetValue(false);
        public void Apply(BWR_Settings settings) => wallMEF.SetMenuValues(settings);
    } 
}
