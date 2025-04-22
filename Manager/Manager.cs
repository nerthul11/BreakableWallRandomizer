using BreakableWallRandomizer.IC;
using BreakableWallRandomizer.Settings;
using ItemChanger;
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
            JsonSerializer jsonSerializer = new() {TypeNameHandling = TypeNameHandling.Auto};
            
            using Stream stream = assembly.GetManifestResourceStream("BreakableWallRandomizer.Resources.Data.BreakableWallObjects.json");
            StreamReader reader = new(stream);
            List<WallObject> wallList = jsonSerializer.Deserialize<List<WallObject>>(new JsonTextReader(reader));

            foreach (WallObject wall in wallList)
            {
                BreakableWallItem wallItem = new(wall.name, wall.sceneName, wall.gameObject, wall.fsmType, wall.persistentBool, wall.sprite, wall.extra, wall.groupWalls);
                BreakableWallLocation wallLocation = new(wall.name, wall.sceneName, wall.gameObject, wall.fsmType, wall.alsoDestroy, wall.x, wall.y, wall.groupWalls, wall.pinType);
                Finder.DefineCustomItem(wallItem);
                Finder.DefineCustomLocation(wallLocation);
            }

            using Stream gstream = assembly.GetManifestResourceStream("BreakableWallRandomizer.Resources.Data.WallGroups.json");
            StreamReader greader = new(gstream);
            List<WallObject> groupList = jsonSerializer.Deserialize<List<WallObject>>(new JsonTextReader(greader));

            foreach (WallObject group in groupList)
            {
                BreakableWallItem groupItem = new(group.name, group.sceneName, group.gameObject, group.fsmType, group.persistentBool, group.sprite, group.extra, group.groupWalls);
                BreakableWallLocation groupLocation = new(group.name, group.sceneName, group.gameObject, group.fsmType, group.alsoDestroy, group.x, group.y, group.groupWalls);
                Finder.DefineCustomItem(groupItem);
                Finder.DefineCustomLocation(groupLocation);
            }

            Finder.DefineCustomLocation(new WallShop());
        }       
    }
}