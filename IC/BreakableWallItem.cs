using BreakableWallRandomizer.Modules;
using ItemChanger;
using ItemChanger.Tags;
using ItemChanger.UIDefs;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BreakableWallRandomizer.IC
{
    [Serializable]
    public class BreakableWallItem : AbstractWallItem
    {
        public BreakableWallItem(
            string name, string sceneName, string gameObject, string fsmType, string persistentBool, 
            string sprite, bool extra, List<CondensedWallObject> groupWalls
        )
        {
            this.name = name;
            this.sceneName = sceneName;
            this.gameObject = gameObject;
            this.fsmType = fsmType;
            this.persistentBool = persistentBool;
            this.extra = extra;
            this.groupWalls = groupWalls;
            UIDef = new MsgUIDef()
            {
                name = new BoxedString(name.Replace("_", " ").Replace("-", " - ")),
                shopDesc = GenerateShopDescription(),
                sprite = new WallSprite(sprite)
            };
            tags = [BreakableWallItemTag()];
        }

        private InteropTag BreakableWallItemTag()
        {
            // Define sprite by item type
            string sprite = "";
            if (name.StartsWith("Wall-") || name.StartsWith("Wall_Group"))
                sprite = "mine_break_wall_03_0deg";
            if (name.StartsWith("Plank-") || name.StartsWith("Plank_Group"))
                sprite = "wood_plank_02";
            if (name.StartsWith("Dive_Floor-") || name.StartsWith("Dive_Group"))
                sprite = "break_floor_glass";
            if (name.StartsWith("Collapser-") || name.StartsWith("Collapser_Group"))
                sprite = "collapser_short_0deg";
              
            InteropTag tag = new();
            tag.Properties["ModSource"] = "BreakableWallRandomizer";
            tag.Properties["PoolGroup"] = $"{name.Split('-')[0].Replace('_', ' ')}s";
            tag.Properties["PinSprite"] = new WallSprite(sprite);
            tag.Message = "RandoSupplementalMetadata";
            return tag;
        }

        public BoxedString GenerateShopDescription()
        {
            string[] descriptions = {
            "What am I supposed to put in this description? It's a wall.",
            "Truly one of the walls of all time.",
            "Quite possibly my most favouritest wall in the game.",
            "Quite possibly my least favouritest wall in the game.",
            "Call in Bob the Builder. He won't fix it, he'll break it.",
            "Donate to Menderbug so that he can have a day off and break a wall instead of fixing one.",
            "This is probably the most important wall in the game.",
            "This is probably just another useless shortcut wall. Still...",
            "Fun fact: this mod adds exactly 100 breakable wall checks, and even more dive floor checks!",
            "Yes, you might need to do four Pantheons to break that one wall.",
            "Writing shop descriptions for these things is kinda hard.",
            "Vague and non-specific description somehow tangentially related to walls goes here.",
            "I bet you don't even know where this one is, do you?",
            "Wall is love, baby don't hurt me, don't hurt me, no more.",
            "This wall is begging to be shattered. Do it for the thrill.",
            "Behind this wall lies a mystery waiting to be uncovered. Unless there isn't.",
            "I'm pretty sure the wall won't see this one coming.",
            "This wall was asking for it. I just answered the call.",
            "Rumour has it that breaking this wall will bring good luck. Worth a shot, right?",
            "This wall is a roadblock on the path to victory. It's time to remove it.",
            "Maybe, if you run really fast at this wall, it'll just let you through instead?",
            "Hey kid, wanna buy some cracks?",
            "This one's definitely the one you've been looking for. Trust me, I checked.",
            "All craftsmanship is of the lowest quality.",
            "Menderbug has been trying to get his hands on this one for years!",
            "I'm sure this is the one wall you need for Myla's shop.",
            "Is this even a wall? Or is it a plank? Or is it a dive floor? Or is it a collapser? You know, I do not.",
            "Don't look at me, I'm just a description.",
            "There are 56 rock walls in the game.",
            "There are 51 wooden planks in the game.",
            "There are 45 dive floors in the game.",
            "There are many collapsers in the game. Although King's Pass' one doesn't count.",

            "I'll cast some Bad Magic to break this wall for ya -- for a small fee.",
            "Bring in a Sock Mower to mow down this wall. What even *is* a Sockmower?",
            "FlibberZERO this wall.",
            "You Onrywon't be seeing this wall any more after you purchase this product.",
            "You can thank Bentechy66 for this wall even being a thing by breaking it.",
            "Broken walls are no longer Glowstonetrees. They're transparent.",
            "El camino a Roma 337 muros tiene.",
            "Nerthul thinks this one is a scam, but you'll buy it regardless.",

            "Hot Loading Screen Tip: Walls which you've unlocked, but haven't checked, will be transparent. You can walk through them!",
            "Hot Loading Screen Tip: If Group Walls are enabled, you can walk through any walls in that room if the item's obtained.",
            "Hot Loading Screen Tip: If Group Walls are enabled, breaking any wall in that room will grant you the group's check.",
            "Hot Loading Screen Tip: Breakable Walls in the white palace follow the WP Rando setting.",
            "Hot Loading Screen Tip: There's a miner, looking for shiny stuff behind walls and will reward you for breaking them.",
            "Hot Loading Screen Tip: They say a fluke thing who sells junk might accept your wlal credit card."
            };

            System.Random rng = new();
            return new BoxedString(descriptions[rng.Next(0, descriptions.Length)]);
        }

        public override void GiveImmediate(GiveInfo info)
        {
            // Set data in the save to indicate we got the wall
            if (name.StartsWith("Wall_Group") || name.StartsWith("Plank_Group") || name.StartsWith("Dive_Group") || name.StartsWith("Collapser_Group"))
            {
                foreach (CondensedWallObject wall in groupWalls)
                {
                    if (wall.name.StartsWith("Wall") && !BreakableWallModule.Instance.UnlockedWalls.Contains(name))
                        BreakableWallModule.Instance.UnlockedWalls.Add(wall.name);
                    if (wall.name.StartsWith("Plank") && !BreakableWallModule.Instance.UnlockedPlanks.Contains(name))
                        BreakableWallModule.Instance.UnlockedPlanks.Add(wall.name);
                    if (wall.name.StartsWith("Dive_Floor") && !BreakableWallModule.Instance.UnlockedDives.Contains(name))
                        BreakableWallModule.Instance.UnlockedDives.Add(wall.name);
                    if (name.StartsWith("Collapser") && !BreakableWallModule.Instance.UnlockedCollapsers.Contains(name))
                        BreakableWallModule.Instance.UnlockedCollapsers.Add(name);
                    
                    // If we're already in the same scene as the wall, break it.
                    if (GameManager.instance.sceneName == wall.sceneName)
                        GameObject.Find(wall.gameObject).LocateMyFSM(wall.fsmType).SetState("BreakSameScene");
                }
            }
            else
            {
                if (name.StartsWith("Wall") && !BreakableWallModule.Instance.UnlockedWalls.Contains(name))
                    BreakableWallModule.Instance.UnlockedWalls.Add(name);
                if (name.StartsWith("Plank") && !BreakableWallModule.Instance.UnlockedPlanks.Contains(name))
                    BreakableWallModule.Instance.UnlockedPlanks.Add(name);
                if (name.StartsWith("Dive_Floor") && !BreakableWallModule.Instance.UnlockedDives.Contains(name))
                    BreakableWallModule.Instance.UnlockedDives.Add(name);
                if (name.StartsWith("Collapser") && !BreakableWallModule.Instance.UnlockedCollapsers.Contains(name))
                    BreakableWallModule.Instance.UnlockedCollapsers.Add(name);
            }
            base.GiveImmediate(info);
        }
    }
}
