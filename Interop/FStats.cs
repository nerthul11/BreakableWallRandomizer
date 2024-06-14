using FStats;
using FStats.StatControllers;
using FStats.Util;
using BreakableWallRandomizer.Manager;
using BreakableWallRandomizer.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using System.IO;
using BreakableWallRandomizer.IC;
using RandomizerMod.Settings;

namespace BreakableWallRandomizer.Interop
{
    public static class FStats_Interop
    {
        public static void Hook()
        {
            API.OnGenerateFile += GenerateStats;
        }

        private static void GenerateStats(Action<StatController> generateStats)
        {
            if (!BWR_Manager.Settings.Enabled)
                return;
            
            generateStats(new BreakableWallStats());

            Assembly assembly = Assembly.GetExecutingAssembly();
            JsonSerializer jsonSerializer = new() {TypeNameHandling = TypeNameHandling.Auto};
            
            using Stream stream = assembly.GetManifestResourceStream("BreakableWallRandomizer.Resources.Data.BreakableWallObjects.json");
            StreamReader reader = new(stream);
            List<WallObject> wallList = jsonSerializer.Deserialize<List<WallObject>>(new JsonTextReader(reader));
            BreakableWallModule module = BreakableWallModule.Instance;
            foreach (WallObject wall in wallList)
            {
                // Filter randomized walls. We don't care about if they're grouped or not here.
                bool include = wall.name.StartsWith("Wall") && module.Settings.RockWalls;
                include = include || (wall.name.StartsWith("Plank") && module.Settings.WoodenPlanks);
                include = include || (wall.name.StartsWith("Dive_Floor") && module.Settings.DiveFloors);
                if (wall.name.Contains("King's_Pass"))
                    include = include && module.Settings.KingsPass;
                include = include && !(wall.exit && module.Settings.SoftlockWalls);

                // To be improved - Always include WP walls as vanilla and remove them from vanilla list using the location.
                // Ideally it would be done directly using generation settings.
                if (wall.name.Contains("White_Palace") || wall.name.Contains("Path_of_Pain"))
                    include = false;
                
                if (!include)
                    module.vanillaWalls.Add(new(wall.name, wall.sceneName, wall.gameObject, wall.fsmType));
            }
        }
    }

    public class BreakableWallStats : StatController
    {
        public override void Initialize() 
        {
            BreakableWallModule.Instance.OnAchievedBreakableWall += AddMarks;
        }

        private void AddMarks(List<string> marks)
        {
            foreach (string mark in marks)
                if (!OneTimeCheck.Contains(mark))
                {
                    OneTimeCheck.Add(mark);
                    BreakableWallMarks.Add(new BreakableWallMark(mark, FStatsMod.LS.Get<Common>().CountedTime));
                }
        }

        public record BreakableWallMark(string Mark, float Timestamp);
        public List<BreakableWallMark> BreakableWallMarks = [];
        public List<string> OneTimeCheck = [];

        public override IEnumerable<DisplayInfo> GetDisplayInfos()
        {
            List<string> rows = BreakableWallMarks.OrderBy(x => x.Timestamp).Select(x => $"{x.Mark}: {x.Timestamp.PlaytimeHHMMSS()}").ToList();
            if (BreakableWallMarks.Count == 0) 
                yield break;
            
            yield return new()
            {
                Title = "Godhome Randomizer Timeline",
                MainStat = $"{BreakableWallMarks.Count}",
                StatColumns = Columnize(rows),
                Priority = BuiltinScreenPriorityValues.ExtensionStats
            };
        }
        private const int COL_SIZE = 10;

        private List<string> Columnize(List<string> rows)
        {
            int columnCount = (rows.Count + COL_SIZE - 1) / COL_SIZE;
            List<string> list = [];
            for (int i = 0; i < columnCount; i++)
            {
                list.Add(string.Join("\n", rows.Slice(i, columnCount)));
            }
            return list;
        }
    }
}