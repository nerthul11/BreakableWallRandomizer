using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BreakableWallRandomizer.IC;
using Newtonsoft.Json;
using RandomizerCore.Logic;
using RandomizerCore.StringItems;
using RandomizerMod.RC;
using RandomizerMod.Settings;

namespace BreakableWallRandomizer.Manager
{
    internal static class LogicHandler
    {
        internal static void Hook()
        {
            RCData.RuntimeLogicOverride.Subscribe(1f, ApplyLogic);
            RCData.RuntimeLogicOverride.Subscribe(99999f, LogicPatch);
        }

        private static void ApplyLogic(GenerationSettings gs, LogicManagerBuilder lmb)
        {
            if (!BWR_Manager.Settings.Enabled)
                return;
            
            Assembly assembly = Assembly.GetExecutingAssembly();
            JsonSerializer jsonSerializer = new() {TypeNameHandling = TypeNameHandling.Auto};
            
            using Stream stream = assembly.GetManifestResourceStream("BreakableWallRandomizer.Resources.Data.BreakableWallObjects.json");
            StreamReader reader = new(stream);
            List<WallObject> wallList = jsonSerializer.Deserialize<List<WallObject>>(new JsonTextReader(reader));
            
            lmb.GetOrAddTerm("Broken_Walls");
            lmb.GetOrAddTerm("Broken_Planks");
            lmb.GetOrAddTerm("Broken_Dive_Floors");

            lmb.AddLogicDef(new("Myla_Shop", "(Crossroads_45[left1] | Crossroads_45[right1]) + LISTEN?TRUE"));
            
            foreach (WallObject wall in wallList)
            {
                lmb.GetOrAddTerm(wall.name);
                lmb.AddItem(new StringItemTemplate(wall.name, $"Broken_{wall.name.Split('-')[0]}s++ >> {wall.name}++"));
                lmb.AddLogicDef(new(wall.name, wall.logic));

                foreach(var logicOverride in wall.logicOverrides)
                {
                    bool exists = lmb.LogicLookup.TryGetValue(logicOverride.Key, out _);
                    if (exists)
                        lmb.DoLogicEdit(new(logicOverride.Key, logicOverride.Value));
                }

                foreach (var substitutionDef in wall.logicSubstitutions)
                { 
                    foreach (var substitution in substitutionDef.Value)
                    {
                        bool exists = lmb.LogicLookup.TryGetValue(substitutionDef.Key, out _);
                        if (exists)
                            lmb.DoSubst(new(substitutionDef.Key, substitution.Key, substitution.Value));  
                    }
                }
            }

            using Stream gstream = assembly.GetManifestResourceStream("BreakableWallRandomizer.Resources.Data.WallGroups.json");
            StreamReader greader = new(gstream);
            List<WallObject> groupList = jsonSerializer.Deserialize<List<WallObject>>(new JsonTextReader(greader));

            foreach (WallObject g in groupList)
            {
                string groupName = g.name.Split('-')[1];
                string effect = "";
                string wallTerms = "";
                int wallCount = 0;
                int plankCount = 0;
                int floorCount = 0;
                foreach (WallObject wall in wallList)
                {
                    if (wall.group == groupName)
                    {
                        if (wall.name.StartsWith("Wall"))
                            wallCount++;
                        if (wall.name.StartsWith("Plank"))
                            plankCount++;
                        if (wall.name.StartsWith("Dive_Floor"))
                            floorCount++;
                        wallTerms += $"{wall.name}++ >> ";
                    }
                }
                if (wallCount > 0)
                    effect += $"Broken_Walls+{(wallCount > 1 ? $"={wallCount}" : '+')} >> ";
                if (plankCount > 0)
                    effect += $"Broken_Planks+{(plankCount > 1 ? $"={plankCount}" : '+')} >> ";
                if (floorCount > 0)
                    effect += $"Broken_Dive_Floors+{(floorCount > 1 ? $"={floorCount}" : '+')} >> "; 
                effect += wallTerms;
                effect = effect.Remove(effect.Length - 4);

                lmb.AddItem(new StringItemTemplate(g.name, effect));
                lmb.AddLogicDef(new(g.name, g.logic));

                foreach(var logicOverride in g.logicOverrides)
                {
                    bool exists = lmb.LogicLookup.TryGetValue(logicOverride.Key, out _);
                    if (exists)
                        lmb.DoLogicEdit(new(logicOverride.Key, logicOverride.Value));
                }

                foreach (var substitutionDef in g.logicSubstitutions)
                { 
                    foreach (var substitution in substitutionDef.Value)
                    {
                        bool exists = lmb.LogicLookup.TryGetValue(substitutionDef.Key, out _);
                        if (exists)
                            lmb.DoSubst(new(substitutionDef.Key, substitution.Key, substitution.Value));  
                    }
                }
            }
        }

        // Apply Logic Overrides and Substitutions to connections
        private static void LogicPatch(GenerationSettings gs, LogicManagerBuilder lmb)
        {
            if (!BWR_Manager.Settings.Enabled)
                return;

            Assembly assembly = Assembly.GetExecutingAssembly();
            JsonSerializer jsonSerializer = new() {TypeNameHandling = TypeNameHandling.Auto};
            
            using Stream stream = assembly.GetManifestResourceStream("BreakableWallRandomizer.Resources.Logic.ConnectionOverrides.json");
            StreamReader reader = new(stream);
            List<ConnectionLogicObject> objectList = jsonSerializer.Deserialize<List<ConnectionLogicObject>>(new JsonTextReader(reader));

            BreakableWallRandomizer.Instance.Log(objectList.Count);

            foreach (ConnectionLogicObject o in objectList)
            {
                BreakableWallRandomizer.Instance.Log("Substitutions");
                foreach (var sub in o.logicSubstitutions)
                {
                    bool exists = lmb.LogicLookup.TryGetValue(o.name, out _);
                    BreakableWallRandomizer.Instance.Log(o.name);
                    BreakableWallRandomizer.Instance.Log(sub.Key);
                    BreakableWallRandomizer.Instance.Log(exists);
                    if (exists)
                        lmb.DoSubst(new(o.name, sub.Key, sub.Value));  
                }

                BreakableWallRandomizer.Instance.Log("Override");
                if (o.logicOverride != "")
                {
                    bool exists = lmb.LogicLookup.TryGetValue(o.name, out _);
                    BreakableWallRandomizer.Instance.Log(o.name);
                    BreakableWallRandomizer.Instance.Log(exists);
                    if (exists)
                        lmb.DoLogicEdit(new(o.name, o.logicOverride));
                }
            }
        }
    }
}