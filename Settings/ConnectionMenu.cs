using MenuChanger;
using MenuChanger.MenuElements;
using MenuChanger.MenuPanels;
using MenuChanger.Extensions;
using RandomizerMod.Menu;
using BreakableWallRandomizer.Manager;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace BreakableWallRandomizer.Settings
{
    public class ConnectionMenu
    {
        internal MenuPage settingsPage;
        internal MenuElementFactory<BWR_Settings> rootMEF;
        private SmallButton shopPageButton;
        internal MenuElementFactory<MylaShopSettings> shopMEF;
        private SmallButton wallButton;
        private SmallButton plankButton;
        private SmallButton diveButton;
        private SmallButton collapserButton;
        private readonly Dictionary<string, MenuElementFactory<WallSettings>> wallMEFs = new(StringComparer.OrdinalIgnoreCase);
        internal VerticalItemPanel wallVIP;
        internal SmallButton settingsButton;
        internal static ConnectionMenu Instance { get; private set; }
        private ConnectionMenu(MenuPage landingPage)
        {
            settingsPage = new MenuPage("BreakableWallSettings", landingPage);
            rootMEF = new(settingsPage, BWR_Manager.Settings);
            wallVIP = new(settingsPage, new(0, 350), 66f, true);
            
            foreach (IValueElement e in rootMEF.Elements)
            {
                e.SelfChanged += obj => SetButtonColor(settingsButton, () => BWR_Manager.Settings.Enabled);
            }
            wallButton = new(settingsPage, "Walls");
            plankButton = new(settingsPage, "Planks");
            diveButton = new(settingsPage, "Dive Floors");
            collapserButton = new(settingsPage, "Collapsers");
            shopPageButton = new(settingsPage, "Myla Shop");

            var rockPage = DisplayWallType("RockWalls", "Walls", wallButton, () => BWR_Manager.Settings.RockWalls.Enabled);
            wallButton.AddHideAndShowEvent(settingsPage, rockPage);
            var plankPage = DisplayWallType("WoodenPlanks", "Planks", plankButton, () => BWR_Manager.Settings.WoodenPlanks.Enabled);
            plankButton.AddHideAndShowEvent(settingsPage, plankPage);
            var divePage = DisplayWallType("DiveFloors", "Dive Floors", diveButton, () => BWR_Manager.Settings.DiveFloors.Enabled);
            diveButton.AddHideAndShowEvent(settingsPage, divePage);
            var collapserPage = DisplayWallType("Collapsers", "Collapsers", collapserButton, () => BWR_Manager.Settings.Collapsers.Enabled);
            collapserButton.AddHideAndShowEvent(settingsPage, collapserPage);

            shopPageButton.AddHideAndShowEvent(DisplayShop());

            wallVIP.Add(rootMEF.ElementLookup[nameof(BWR_Settings.Enabled)]);
            wallVIP.Add(wallButton);
            wallVIP.Add(plankButton);
            wallVIP.Add(diveButton);
            wallVIP.Add(collapserButton);
            wallVIP.Add(rootMEF.ElementLookup[nameof(BWR_Settings.GodhomeWalls)]);
            wallVIP.Add(rootMEF.ElementLookup[nameof(BWR_Settings.GroupWalls)]);
            wallVIP.Add(shopPageButton);
            
            settingsButton = new(landingPage, "Breakable Walls");
            settingsButton.AddHideAndShowEvent(landingPage, settingsPage);
            SetButtonColor(settingsButton, () => BWR_Manager.Settings.Enabled);

            wallVIP.ResetNavigation();
            wallVIP.SymSetNeighbor(Neighbor.Up, settingsPage.backButton);
            wallVIP.SymSetNeighbor(Neighbor.Down, settingsPage.backButton);
        }
        
        private MenuPage DisplayWallType(string type, string heading, SmallButton targetButton, Func<bool> isEnabled)
        {
            MenuPage subPage = new(type, settingsPage);
            MenuLabel header = new(subPage, heading);
            WallSettings settings = BWR_Manager.Settings.GetVariable<WallSettings>(type);
            MenuElementFactory<WallSettings> subMEF = new(subPage, settings);
            wallMEFs[type] = subMEF;

            VerticalItemPanel itemPanel = new(subPage, new Vector2(0, 350), 75, false);
            itemPanel.Add(header);
            itemPanel.Add(subMEF.ElementLookup[nameof(WallSettings.Enabled)]);
            itemPanel.Add(subMEF.ElementLookup[nameof(WallSettings.AdditionalWalls)]);
            itemPanel.Add(subMEF.ElementLookup[nameof(WallSettings.Group)]);

            itemPanel.ResetNavigation();
            itemPanel.SymSetNeighbor(Neighbor.Up, subPage.backButton);
            itemPanel.SymSetNeighbor(Neighbor.Down, subPage.backButton);
            SetButtonColor(targetButton, isEnabled);

            return subPage;
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
            itemPanel.Add(shopMEF.ElementLookup[nameof(MylaShopSettings.IncludeVanillaItems)]);
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
            if (target != null)
            {
                RandomizerMenuAPI.GenerateStartLocationDict();
                target.Parent.BeforeShow += () =>
                {
                    target.Text.color = condition() ? Colors.TRUE_COLOR : Colors.DEFAULT_COLOR;
                };
            }
        }

        private static void ConstructMenu(MenuPage landingPage) => Instance = new(landingPage);

        public void Disable() => rootMEF.ElementLookup[nameof(BWR_Settings.Enabled)].SetValue(false);
        public void Apply(BWR_Settings settings) 
        {
            rootMEF.SetMenuValues(settings);
            wallMEFs["RockWalls"].SetMenuValues(settings.RockWalls);
            wallMEFs["WoodenPlanks"].SetMenuValues(settings.WoodenPlanks);
            wallMEFs["DiveFloors"].SetMenuValues(settings.DiveFloors);
            wallMEFs["Collapsers"].SetMenuValues(settings.Collapsers);
            shopMEF.SetMenuValues(settings.MylaShop);
        }
    } 
}
