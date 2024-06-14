using BreakableWallRandomizer.IC;
using BreakableWallRandomizer.Manager;
using ItemChanger;
using ItemChanger.Modules;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BreakableWallRandomizer.Modules
{
    [Serializable]
    public class BreakableWallModule : Module
    {
        public static BreakableWallModule Instance => ItemChangerMod.Modules.GetOrAdd<BreakableWallModule>();
        public SaveSettings Settings { get; set; } = new();
        public class SaveSettings 
        {
            public bool WoodenPlanks { get; set; } = BWR_Manager.Settings.Enabled && BWR_Manager.Settings.WoodenPlankWalls;
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
        }

        public override void Unload() 
        {
            On.GameManager.BeginSceneTransition -= VanillaTracker;
        }

        private void VanillaTracker(On.GameManager.orig_BeginSceneTransition orig, GameManager self, GameManager.SceneLoadInfo info)
        {
            // Temporary - Log scene transitions
            BreakableWallRandomizer.Instance.Log(info.SceneName + "[" + info.EntryGateName + "]");

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
                    if (wallType == "Plank" && !UnlockedWalls.Contains(wall.name))
                        UnlockedPlanks.Add(wall.name);
                    if (wallType == "Dive_Floor" && !UnlockedWalls.Contains(wall.name))
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
                    buriedGeoWallCount += 1;
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
            // All Walls
            // All Planks
            // All Floors

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