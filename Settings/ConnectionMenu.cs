using MenuChanger;
using MenuChanger.MenuElements;
using MenuChanger.MenuPanels;
using MenuChanger.Extensions;
using RandomizerMod.Menu;
using static RandomizerMod.Localization;
using BreakableWallRandomizer.Manager;
using UnityEngine;
using System;

namespace BreakableWallRandomizer.Settings
{
    public class ConnectionMenu
    {
        internal MenuPage settingsPage;
        internal MenuElementFactory<BWR_Settings> wallMEF;
        private SmallButton shopPageButton;
        internal MenuElementFactory<MylaShopSettings> shopMEF;
        internal VerticalItemPanel wallVIP;
        internal SmallButton settingsButton;
        internal static ConnectionMenu Instance { get; private set; }
        private ConnectionMenu(MenuPage landingPage)
        {
            settingsPage = new MenuPage("BreakableWallSettings", landingPage);
            wallMEF = new(settingsPage, BWR_Manager.Settings);
            wallVIP = new(settingsPage, new(0, 350), 70f, true, wallMEF.Elements);
            
            foreach (IValueElement e in wallMEF.Elements)
            {
                e.SelfChanged += obj => SetButtonColor(settingsButton, () => BWR_Manager.Settings.Enabled);
            }
            shopPageButton = new(settingsPage, "Myla Shop");
            shopPageButton.AddHideAndShowEvent(DisplayShop());
            wallVIP.Add(shopPageButton);
            
            settingsButton = new(landingPage, Localize("Breakable Walls"));
            settingsButton.AddHideAndShowEvent(landingPage, settingsPage);
            SetButtonColor(settingsButton, () => BWR_Manager.Settings.Enabled);

            wallVIP.ResetNavigation();
            wallVIP.SymSetNeighbor(Neighbor.Up, settingsPage.backButton);
            wallVIP.SymSetNeighbor(Neighbor.Down, settingsPage.backButton);
        }

        private MenuPage DisplayShop()
        {
            MenuPage shopPage = new("Myla Shop", settingsPage);
            MenuLabel header = new(shopPage, "Myla Shop");
            shopMEF = new(shopPage, BWR_Manager.Settings.MylaShop);

            VerticalItemPanel itemPanel = new(shopPage, new Vector2(0, 350), 75, false);
            GridItemPanel costPanel = new(shopPage, Vector2.zero, 2, 100, 250, false,
                [
                shopMEF.ElementLookup[nameof(MylaShopSettings.MinimumCost)], 
                shopMEF.ElementLookup[nameof(MylaShopSettings.MaximumCost)]
                ]);
            
            itemPanel.Add(header);
            itemPanel.Add(shopMEF.ElementLookup[nameof(MylaShopSettings.Enabled)]);
            itemPanel.Add(shopMEF.ElementLookup[nameof(MylaShopSettings.IncludeInJunkShop)]);
            itemPanel.Add(costPanel);
            itemPanel.Add(shopMEF.ElementLookup[nameof(MylaShopSettings.Tolerance)]);

            itemPanel.ResetNavigation();
            itemPanel.SymSetNeighbor(Neighbor.Up, shopPage.backButton);
            itemPanel.SymSetNeighbor(Neighbor.Down, shopPage.backButton);
            SetButtonColor(shopPageButton, () => BWR_Manager.Settings.MylaShop.Enabled);
            
            return shopPage;
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
            button = Instance.settingsButton;
            return true;
        }

        private void SetButtonColor(SmallButton target, Func<bool> condition)
        {
            target.Parent.BeforeShow += () =>
            {
                target.Text.color = condition() ? Colors.TRUE_COLOR : Colors.DEFAULT_COLOR;
            };
        }

        private static void ConstructMenu(MenuPage landingPage) => Instance = new(landingPage);

        public void Disable() => wallMEF.ElementLookup[nameof(BWR_Settings.Enabled)].SetValue(false);
        public void Apply(BWR_Settings settings) => wallMEF.SetMenuValues(settings);
    } 
}
