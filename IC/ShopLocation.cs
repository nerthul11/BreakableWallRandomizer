using System.Collections.Generic;
using System.Linq;
using BreakableWallRandomizer.IC.Shop;
using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.Locations;
using ItemChanger.Tags;
using ItemChanger.Util;
using UnityEngine;
using UnityEngine.SceneManagement;

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
                    new WallCostSupport("Dive", "break_floor_glass")
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
            bool AllObtained = true;
            foreach (AbstractItem item in Placement.Items)
            {
                if (!item.WasEverObtained()) 
                    AllObtained = false;
            }

            if (AllObtained && (self.gameObject.name.Contains("Zombie Myla") || string.Equals(self.gameObject.name, "Myla Crazy NPC")))
            {
                self.gameObject.SetActive(false);
                return;
            }
            else if (self.gameObject.name.Contains("Zombie Myla") || string.Equals(self.gameObject.name, "Myla Crazy NPC"))
            {
                GameObject myla = self.gameObject;
                if (Placement.CheckVisitedAny(VisitState.Accepted) && !Placement.AllObtained())
                    {
                        Container c = Container.GetContainer(Container.Shiny);

                        ContainerInfo info = new(c.Name, Placement, flingType, (Placement as ItemChanger.Placements.ISingleCostPlacement)?.Cost);
                        GameObject shiny = c.GetNewContainer(info);

                        c.ApplyTargetContext(shiny, myla.transform.position.x, myla.transform.position.y, 0f);
                        ShinyUtility.FlingShinyRandomly(shiny.LocateMyFSM("Shiny Control"));
                    }
            }
            orig(self);
        }

        private void ForceMyla(On.DeactivateIfPlayerdataTrue.orig_OnEnable orig, DeactivateIfPlayerdataTrue self)
        {
            bool AllObtained = true;
            foreach (AbstractItem item in Placement.Items)
            {
                if (!item.WasEverObtained()) 
                    AllObtained = false;
            }

            if (string.Equals(self.gameObject.name, "Miner") && GameManager.instance.sceneName == sceneName && AllObtained)
                return;

            orig(self);
        }

        private void MakeShinyForRespawnedItems(GameObject myla)
        {
            if (Placement.CheckVisitedAny(VisitState.Accepted) && !Placement.AllObtained())
            {
                Container c = Container.GetContainer(Container.Shiny);

                ContainerInfo info = new(c.Name, Placement, flingType, (Placement as ItemChanger.Placements.ISingleCostPlacement)?.Cost);
                GameObject shiny = c.GetNewContainer(info);

                c.ApplyTargetContext(shiny, myla.transform.position.x, myla.transform.position.y, 0f);
                ShinyUtility.FlingShinyLeft(shiny.LocateMyFSM("Shiny Control"));
            }
        }
    }
}