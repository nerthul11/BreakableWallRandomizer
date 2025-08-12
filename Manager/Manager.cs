using BreakableWallRandomizer.IC;
using BreakableWallRandomizer.Settings;
using ItemChanger;
using ItemChanger.UIDefs;
using Modding;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace BreakableWallRandomizer.Manager
{
    internal static class BWR_Manager
    {
        public static BWR_Settings Settings => BreakableWallRandomizer.Instance.GS;
        public static int TotalWalls = 56;
        public static int TotalPlanks = 51;
        public static int TotalDives = 45;
        public static int TotalCollapsers = 58;
        public static void Hook()
        {
            DefineObjects();
            if (ModHooks.GetMod("Randomizer 4") is Mod)
            {
                LogicHandler.Hook();
                ItemHandler.Hook();
                ConnectionMenu.Hook();
            }
        }

        private static void DefineObjects()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            JsonSerializer jsonSerializer = new() { TypeNameHandling = TypeNameHandling.Auto };

            using Stream stream = assembly.GetManifestResourceStream("BreakableWallRandomizer.Resources.Data.BreakableWallObjects.json");
            StreamReader reader = new(stream);
            List<WallObject> wallList = jsonSerializer.Deserialize<List<WallObject>>(new JsonTextReader(reader));

            foreach (WallObject wall in wallList)
            {
                BreakableWallItem wallItem = new(wall.name, wall.sceneName, wall.gameObject, wall.fsmType, wall.persistentBool, wall.sprite, wall.extra, wall.groupWalls);
                BreakableWallLocation wallLocation = new(wall.name, wall.sceneName, wall.gameObject, wall.fsmType, wall.alsoDestroy, wall.x, wall.y, wall.groupWalls, wall.pinType, wall.backupX, wall.backupY);
                wallItem.UIDef = new MsgUIDef()
                {
                    name = new BoxedString(wall.name.Replace("_", " ").Replace("-", " - ")),
                    shopDesc = GenerateShopDescription(),
                    sprite = new WallSprite(wall.sprite)
                };
                Finder.DefineCustomItem(wallItem);
                Finder.DefineCustomLocation(wallLocation);
            }

            using Stream gstream = assembly.GetManifestResourceStream("BreakableWallRandomizer.Resources.Data.WallGroups.json");
            StreamReader greader = new(gstream);
            List<WallObject> groupList = jsonSerializer.Deserialize<List<WallObject>>(new JsonTextReader(greader));

            foreach (WallObject group in groupList)
            {
                BreakableWallItem groupItem = new(group.name, group.sceneName, group.gameObject, group.fsmType, group.persistentBool, group.sprite, group.extra, group.groupWalls);
                BreakableWallLocation groupLocation = new(group.name, group.sceneName, group.gameObject, group.fsmType, group.alsoDestroy, group.x, group.y, group.groupWalls, group.pinType, group.backupX, group.backupY);
                Finder.DefineCustomItem(groupItem);
                Finder.DefineCustomLocation(groupLocation);
            }

            Finder.DefineCustomLocation(new WallShop());
        }

        public static BoxedString GenerateShopDescription()
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
            "Fun fact: this mod adds exactly X breakable wall checks, and even more dive floor checks!",
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
            "There are many collapsers in the game.",

            "Dearest Homothety (AKA \"Moth\") (AKA \"Randoman\"): I am writing to inform you of a glaring error in your randomization algorithm for the game Hollow Knight. Though I was assured that the item locations were random, and indeed was swayed by your very name, there was not one but TWO so-called \"vanilla\" locations. Please, I implore you, fix your game.",
            "I'll cast some Bad Magic to break this wall for ya -- for a small fee.",
            "Bring in a Sock Mower to mow down this wall. What even *is* a Sockmower?",
            "FlibberZERO this wall.",
            "You Onrywon't be seeing this wall any more after you purchase this product.",
            "You can thank Bentechy66 for this wall even being a thing by breaking it.",
            "Broken walls are no longer hard as a GlowSTONEtrees. They're transparent.",
            "The road to Roma 337 walls contains.",
            "Nerthul salutes you and encourages you to spend geo on this and hope for the best.",

            "Hot Loading Screen Tip: Walls which you've unlocked, but haven't checked, will be transparent. You can walk through them!",
            "Hot Loading Screen Tip: If Group Walls are enabled, you can walk through any walls in that room if the item's obtained.",
            "Hot Loading Screen Tip: If Group Walls are enabled, breaking any wall in that room will grant you the group's check.",
            "Hot Loading Screen Tip: Breakable Walls in the white palace follow the WP Rando setting.",
            "Hot Loading Screen Tip: There's a miner, looking for shiny stuff behind walls and will reward you for breaking them.",
            "Hot Loading Screen Tip: They say a fluke thing who sells junk might accept your wall credit card.",
            "Hot Loading Screen Tip: Breakable Walls in the Abyssal Temple follow the Abyssal Temple Rando setting."
            };

            System.Random rng = new();
            return new BoxedString(descriptions[rng.Next(0, descriptions.Length)]);
        }
    }
}