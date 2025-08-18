using BreakableWallRandomizer.IC;
using BreakableWallRandomizer.Manager;
using ItemChanger;
using ItemChanger.Modules;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace BreakableWallRandomizer.Modules
{
    [Serializable]
    public class BreakableWallModule : ItemChanger.Modules.Module
    {
        public static BreakableWallModule Instance => ItemChangerMod.Modules.GetOrAdd<BreakableWallModule>();
        public List<CondensedWallObject> vanillaWalls = [];
        // Module properties
        public List<string> UnlockedBreakables = [];
        public List<string> UnlockedWalls = [];
        public List<string> UnlockedPlanks = [];
        public List<string> UnlockedDives = [];
        public List<string> UnlockedCollapsers = [];
        public override void Initialize() 
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            JsonSerializer jsonSerializer = new() {TypeNameHandling = TypeNameHandling.Auto};   
            using Stream stream = assembly.GetManifestResourceStream("BreakableWallRandomizer.Resources.Data.BreakableWallObjects.json");
            StreamReader reader = new(stream);
            List<BreakableWallItem> wallList = jsonSerializer.Deserialize<List<BreakableWallItem>>(new JsonTextReader(reader));
            foreach (BreakableWallItem wall in wallList)
                vanillaWalls.Add(new(wall.name, wall.sceneName, wall.gameObject, wall.fsmType));
                
            On.HutongGames.PlayMaker.Actions.ActivateGameObject.OnEnter += VanillaTracker;
            if (ItemChangerMod.Modules?.Get<InventoryTracker>() is InventoryTracker it)
                it.OnGenerateFocusDesc += AddWallProgress;
        }

        public override void Unload() 
        {
            On.HutongGames.PlayMaker.Actions.ActivateGameObject.OnEnter -= VanillaTracker;
            if (ItemChangerMod.Modules?.Get<InventoryTracker>() is InventoryTracker it)
                it.OnGenerateFocusDesc -= AddWallProgress;
        }

        private void AddWallProgress(StringBuilder builder)
        {
            builder.AppendLine($"Broken walls: {UnlockedWalls.Count}");
            builder.AppendLine($"Broken planks: {UnlockedPlanks.Count}");
            builder.AppendLine($"Broken dives: {UnlockedDives.Count}");
            builder.AppendLine($"Broken collapsers: {UnlockedCollapsers.Count}");
        }

        private void VanillaTracker(On.HutongGames.PlayMaker.Actions.ActivateGameObject.orig_OnEnter orig, HutongGames.PlayMaker.Actions.ActivateGameObject self)
        {
            orig(self);
            List<PersistentBoolData> boolDatas = SceneData.instance.persistentBoolItems.Where(
                boolData => boolData.sceneName == GameManager._instance.sceneName
                ).ToList();

            boolDatas.ForEach(VanillaState);
        }

        private void VanillaState(PersistentBoolData data)
        {
            List<CondensedWallObject> iterableWalls = vanillaWalls.Where(wall => wall.sceneName == data.sceneName).ToList();
            foreach(CondensedWallObject wall in iterableWalls)
            {
                string wallType = wall.name.Split('-')[0];
                string wallObjectName = wall.gameObject.Split('/')[wall.gameObject.Split('/').Length - 1];
                if (wallObjectName == data.id && data.activated)
                {
                    if (wallType == "Wall" && !UnlockedWalls.Contains(wall.name))
                        UnlockedWalls.Add(wall.name);
                    if (wallType == "Plank" && !UnlockedPlanks.Contains(wall.name))
                        UnlockedPlanks.Add(wall.name);
                    if (wallType == "Dive_Floor" && !UnlockedDives.Contains(wall.name))
                        UnlockedDives.Add(wall.name);
                    if (wallType == "Collapser" && !UnlockedCollapsers.Contains(wall.name))
                        UnlockedCollapsers.Add(wall.name);
                    if (!UnlockedBreakables.Contains(wall.name))
                        UnlockedBreakables.Add(wall.name);
                }
                CompletedChallenges();
            }
        }

        public delegate void Achieved_BWR(List<string> marks);
        public event Achieved_BWR OnAchievedBreakableWall;
        public delegate void ObtainedWalls(int walls);
        public event ObtainedWalls OnWallObtained;

        // On Hook events
        public void CompletedChallenges()
        {
            List<string> completed = [];

            int grimmWallCount = 0;
            int noskWallCount = 0;
            int villageWallCount = 0;
            int catacombsWallCount = 0;
            int buriedGeoWallCount = 0;
            int sanctumWallCount = 0;

            // Loop to see all status
            foreach (string s in UnlockedWalls)
            {
                if (s.Contains("Grimm"))
                    grimmWallCount += 1;
                if (s.Contains("Nosk"))
                    noskWallCount += 1;
                if (s.Contains("Midwife"))
                    villageWallCount += 1;
                if (s.Contains("Catacombs"))
                    catacombsWallCount += 1;
            }

            foreach (string s in UnlockedPlanks)
            {
                if (s.Contains("Grimm"))
                    grimmWallCount += 1;
                if (s.Contains("Nosk"))
                    noskWallCount += 1;
                if (s.Contains("Village"))
                    villageWallCount += 1;
                if (s.Contains("Catacombs"))
                    catacombsWallCount += 1;
            }

            foreach (string s in UnlockedDives)
            {
                if (s.Contains("420_Rock"))
                    buriedGeoWallCount += 1;
                if (s.Contains("Inner_Sanctum"))
                    sanctumWallCount += 1;
            }

            if (grimmWallCount == 3)
                completed.Add("All Grimm Walls");
            if (noskWallCount == 3)
                completed.Add("All Nosk Walls");
            if (villageWallCount == 4)
                completed.Add("All Distant Village Walls");
            if (catacombsWallCount == 9)
                completed.Add("All Catacombs Walls");
            if (buriedGeoWallCount == 11)
                completed.Add("All Buried Geo Dives");
            if (sanctumWallCount == 7)
                completed.Add("All Inner Soul Sanctum Dives");
            if (UnlockedWalls.Count == BWR_Manager.TotalWalls)
                completed.Add("All Walls broken.");
            if (UnlockedPlanks.Count == BWR_Manager.TotalPlanks)
                completed.Add("All Planks broken.");
            if (UnlockedDives.Count == BWR_Manager.TotalDives)
                completed.Add("All Dive Floors broken.");
            if (UnlockedCollapsers.Count == BWR_Manager.TotalCollapsers)
                completed.Add("All Collapsers broken.");
            if (UnlockedBreakables.Count >= (BWR_Manager.TotalWalls + BWR_Manager.TotalPlanks + BWR_Manager.TotalDives + BWR_Manager.TotalCollapsers) / 2)
                completed.Add("Half broken breakables.");
            if (UnlockedBreakables.Count == BWR_Manager.TotalWalls + BWR_Manager.TotalPlanks + BWR_Manager.TotalDives + BWR_Manager.TotalCollapsers)
                completed.Add("All broken breakables.");

            OnAchievedBreakableWall?.Invoke(completed);
            OnWallObtained?.Invoke(UnlockedBreakables.Count);
        }

        public T GetVariable<T>(string propertyName) {
            var property = typeof(BreakableWallModule).GetProperty(propertyName) ?? throw new ArgumentException($"Property '{propertyName}' not found in StatueModule class.");
            return (T)property.GetValue(this);
        }

        public void SetVariable<T>(string propertyName, T value) {
            var property = typeof(BreakableWallModule).GetProperty(propertyName) ?? throw new ArgumentException($"Property '{propertyName}' not found in StatueModule class.");
            property.SetValue(this, value);
        }
    }
}