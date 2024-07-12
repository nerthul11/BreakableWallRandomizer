using BreakableWallRandomizer.IC;
using BreakableWallRandomizer.Manager;
using ItemChanger;
using ItemChanger.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BreakableWallRandomizer.Modules
{
    [Serializable]
    public class BreakableWallModule : Module
    {
        public static BreakableWallModule Instance => ItemChangerMod.Modules.GetOrAdd<BreakableWallModule>();
        public SaveSettings Settings { get; set; } = new();
        public class SaveSettings 
        {
            public bool WoodenPlanks { get; set; } = BWR_Manager.Settings.Enabled && BWR_Manager.Settings.WoodenPlanks;
            public bool RockWalls { get; set; } = BWR_Manager.Settings.Enabled && BWR_Manager.Settings.RockWalls;
            public bool DiveFloors { get; set; } = BWR_Manager.Settings.Enabled && BWR_Manager.Settings.DiveFloors;
            public bool KingsPass { get; set; } = BWR_Manager.Settings.Enabled && BWR_Manager.Settings.KingsPass;
            public bool SoftlockWalls { get; set; } = BWR_Manager.Settings.Enabled && BWR_Manager.Settings.ExcludeWallsWhichMaySoftlockYou;
        }
        public List<CondensedWallObject> vanillaWalls = [];
        // Module properties
        public List<string> UnlockedBreakableWalls = [];
        public List<string> UnlockedWalls = [];
        public List<string> UnlockedPlanks = [];
        public List<string> UnlockedDives = [];
        public override void Initialize() 
        {
            On.GameManager.BeginSceneTransition += VanillaTracker;
            if (ItemChangerMod.Modules?.Get<InventoryTracker>() is InventoryTracker it)
                it.OnGenerateFocusDesc += AddWallProgress;
        }

        public override void Unload() 
        {
            On.GameManager.BeginSceneTransition -= VanillaTracker;
            if (ItemChangerMod.Modules?.Get<InventoryTracker>() is InventoryTracker it)
                it.OnGenerateFocusDesc -= AddWallProgress;
        }

        private void AddWallProgress(StringBuilder builder)
        {
            builder.AppendLine($"Broken walls: {UnlockedWalls.Count}");
            builder.AppendLine($"Broken planks: {UnlockedPlanks.Count}");
            builder.AppendLine($"Broken dives: {UnlockedDives.Count}");
        }

        private void VanillaTracker(On.GameManager.orig_BeginSceneTransition orig, GameManager self, GameManager.SceneLoadInfo info)
        {
            List<PersistentBoolData> boolDatas = SceneData.instance.persistentBoolItems.Where(
                boolData => boolData.sceneName == self.sceneName || boolData.sceneName == info.SceneName
                ).ToList();

            boolDatas.ForEach(VanillaState);
            orig(self, info);
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
                }
                CompletedChallenges();
            }
        }

        public delegate void Achieved_BWR(List<string> marks);
        public event Achieved_BWR OnAchievedBreakableWall;

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
            if (UnlockedBreakableWalls.Count >= (BWR_Manager.TotalWalls + BWR_Manager.TotalPlanks + BWR_Manager.TotalDives) / 2)
                completed.Add("Half broken breakables.");
            if (UnlockedBreakableWalls.Count == BWR_Manager.TotalWalls + BWR_Manager.TotalPlanks + BWR_Manager.TotalDives)
                completed.Add("All broken breakables.");

            OnAchievedBreakableWall?.Invoke(completed);
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