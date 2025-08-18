using BreakableWallRandomizer.Manager;
using BreakableWallRandomizer.Modules;
using FStats;
using FStats.StatControllers;
using FStats.Util;
using ItemChanger;
using System;
using System.Collections.Generic;
using System.Linq;

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
            
            ItemChangerMod.Modules.GetOrAdd<BreakableWallModule>();
            generateStats(new BreakableWallStats());
        }
    }

    public class BreakableWallStats : StatController
    {
        public override void Initialize() 
        {
            BreakableWallModule.Instance.OnAchievedBreakableWall += AddMarks;
        }

        public override void Unload() 
        {
            BreakableWallModule.Instance.OnAchievedBreakableWall -= AddMarks;
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
                Title = "Breakable Wall Randomizer Timeline",
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