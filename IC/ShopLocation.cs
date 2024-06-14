using BreakableWallRandomizer.IC.Shop;
using ItemChanger;
using ItemChanger.Locations;
using ItemChanger.Tags;

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
                    new WallCostSupport("Dive", "break_floor")
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
            if ((self.gameObject.name.Contains("Zombie Myla") || string.Equals(self.gameObject.name, "Myla Crazy NPC")) && !Placement.AllObtained())
            {
                self.gameObject.SetActive(false);
                return;
            }
            orig(self);
        }

        private void ForceMyla(On.DeactivateIfPlayerdataTrue.orig_OnEnable orig, DeactivateIfPlayerdataTrue self)
        {
            if (string.Equals(self.gameObject.name, "Miner") && !Placement.AllObtained())
                return;
            orig(self);
        }
    }
}