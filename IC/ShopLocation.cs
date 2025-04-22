using System.Linq;
using BreakableWallRandomizer.IC.Shop;
using ItemChanger;
using ItemChanger.Locations;
using ItemChanger.Tags;
using UnityEngine;

namespace BreakableWallRandomizer.IC
{
    public class WallShop : CustomShopLocation
    {
        public WallShop()
        {
            name = "Myla_Shop";
            sceneName = SceneNames.Crossroads_45;
            objectName = "Miner";
            fsmName = "npc_control";
            flingType = FlingType.DirectDeposit;
            dungDiscount = true;
            costDisplayerSelectionStrategy = new MultiCostDisplayerSelectionStrategy
            {
                Displayers = [
                    new WallCostSupport("Wall", "mine_break_wall_03_0deg"), 
                    new WallCostSupport("Plank", "wood_plank_02"), 
                    new WallCostSupport("Dive", "break_floor_glass"),
                    new WallCostSupport("Collapser", "collapser_short_0deg")
                ]
            };
            tags = [ShopTag()];
        }

        private static InteropTag ShopTag()
        {
            InteropTag tag = new();
            tag.Properties["ModSource"] = "BreakableWallRandomizer";
            tag.Properties["PoolGroup"] = "Shops";
            tag.Properties["MapLocations"] = new (string, float, float)[] {("Crossroads_45", 0.0f, 0.3f)};
            tag.Message = "RandoSupplementalMetadata";
            return tag;
        }
        protected override void OnLoad()
        {
            base.OnLoad();
            On.DeactivateIfPlayerdataTrue.OnEnable += ForceMyla;
            On.DeactivateIfPlayerdataFalse.OnEnable += PreventMylaZombie;
        }

        protected override void OnUnload()
        {
            base.OnUnload();
            On.DeactivateIfPlayerdataTrue.OnEnable -= ForceMyla;
            On.DeactivateIfPlayerdataFalse.OnEnable -= PreventMylaZombie;
        }

        private void PreventMylaZombie(On.DeactivateIfPlayerdataFalse.orig_OnEnable orig, DeactivateIfPlayerdataFalse self)
        {
            bool AllObtained = Placement.Items.All(x => x.GetTag<CostTag>()?.Cost?.Paid ?? true);
            if (!AllObtained && (self.gameObject.name.Contains("Zombie Myla") || string.Equals(self.gameObject.name, "Myla Crazy NPC")))
            {
                self.gameObject.SetActive(false);
                return;
            }
            else if (AllObtained && (self.gameObject.name.Contains("Zombie Myla") || string.Equals(self.gameObject.name, "Myla Crazy NPC")))
                RespawnItems();
            orig(self);
        }

        private void ForceMyla(On.DeactivateIfPlayerdataTrue.orig_OnEnable orig, DeactivateIfPlayerdataTrue self)
        {
            bool AllObtained = Placement.Items.All(x => x.GetTag<CostTag>()?.Cost?.Paid ?? true);
            if (string.Equals(self.gameObject.name, "Miner") && !AllObtained)
                return;

            orig(self);
        }

        private void RespawnItems()
        {
            foreach (AbstractItem item in Placement.Items)
            {
                if (!item.IsObtained() && (item.GetTag<CostTag>()?.Cost?.Paid ?? true))
                {
                    Container c = Container.GetContainer(Container.Shiny);
                    GameObject shiny = c.GetNewContainer(new(c.Name, Placement, flingType));
                    shiny.transform.position = new(Random.Range(28.0f, 42.0f), 3.4f);
                    shiny.SetActive(true);
                }
            }
        }
    }
}