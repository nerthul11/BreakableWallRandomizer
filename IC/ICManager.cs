﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ItemChanger;
using Newtonsoft.Json;
using RandomizerCore.Logic;
using RandomizerCore.LogicItems;
using RandomizerMod.RC;
using RandomizerMod.Settings;
using UnityEngine.SceneManagement;
using RandomizerCore;
using System.Text.RegularExpressions;
using ItemChanger.UIDefs;
using Mono.Cecil;
using RandomizerMod.Menu;
using Modding;

namespace BreakableWallRandomiser.IC
{
    public class ICManager
    {
        #pragma warning disable 0649
        // This field is assigned to by JSON deserialization
        public class WallData
        {
            public class RelativeMapLocation
            {
                public string sceneName;
                public float x;
                public float y;

                public (string, float, float) repr => (sceneName, x, y);
            }

            Regex rgx = new Regex("[^a-zA-Z0-9]");
            Regex rgx_with_spaces = new Regex("[^a-zA-Z0-9 ]");

            public string gameObject;
            public string fsmType;
            public Dictionary<string, string> logicOverrides;
            public Dictionary<string, Dictionary<string, string>> logicSubstitutions;
            public string sceneName;
            public string niceName;
            public string logic; // The logic required to actually reach _and obtain_ the item at this wall.
            public string persistentBool;
            public string requiredSetting;
            public string sprite;
            public List<RelativeMapLocation> mapLocations;

            public string cleanGameObjectPath() => rgx.Replace(gameObject, "");
            public string cleanSceneName() => rgx.Replace(sceneName, "");
            public string getLocationName() => niceName != "" ? rgx_with_spaces.Replace(niceName, "") : $"Loc_Wall_{cleanSceneName()}_{cleanGameObjectPath()}";
            public string getItemName() => niceName != "" ? rgx_with_spaces.Replace(niceName, "") : $"Itm_Wall_{cleanSceneName()}_{cleanGameObjectPath()}";
            public string getTermName() => $"BREAKABLE_{cleanSceneName()}_{cleanGameObjectPath()}";
            public bool shouldBeIncluded()
            {
                if (!BreakableWallRandomiser.settings.RandomizeWalls) { return false; }
                if (requiredSetting == null) { return true; }

                // TODO: Reflection here is probably a bad idea.
                var prop = BreakableWallRandomiser.settings.GetType().GetField(requiredSetting);
                
                if (prop == null) { 
                    Modding.Logger.LogWarn($"[Wall Rando] Unknown settings property referenced: {requiredSetting}"); 
                    return true; 
                }

                return (bool)prop.GetValue(BreakableWallRandomiser.settings);
            }
        }
        #pragma warning restore 0649

        public static string WALL_GROUP = "Breakable Walls";

        public readonly static List<WallData> wallData = JsonConvert.DeserializeObject<List<WallData>>(
            System.Text.Encoding.UTF8.GetString(Properties.Resources.BreakableWallData)
        );

        readonly string[] wallShopDescriptions =
        {
            "What am I supposed to put in this description? It's a wall.",
            "Truly one of the walls of all time.",
            "Quite possibly my most favouritest wall in the game.",
            "Quite possibly my least favouritest wall in the game.",
            "Call in Bob the Builder. He won't fix it, he'll break it.",
            "Donate to Menderbug so that he can have a day off and break a wall instead of fixing one.",
            "This is probably just another useless shortcut wall. Still...",
            "Did you know there are exactly 100 breakable walls in this game? If you round a bit?",
            "Hot Loading Screen Tip: White Palace Breakable Walls and some others aren't randomized.",
            "Writing shop descriptions for these things is kinda hard.",
            "Vague and non-specific description somehow tangentially related to walls goes here.",
            "I bet you don't even know where this one is, do you?",
            "These wall names aren't very descriptive, are they?"
        };

        public void RegisterItemsAndLocations()
        {
            Random random = new Random(0x1337);

            // UnityEngine.Sprite scaledSprite = UnityEngine.Sprite.Create(uiSprite.texture, uiSprite.rect, new UnityEngine.Vector2(0.5f, 0.5f), 100);

            foreach (var wall in wallData)
            {
                BreakableWallLocation wallLocation = new()
                {
                    objectName = wall.gameObject,
                    fsmType = wall.fsmType,
                    name = wall.getLocationName(),
                    sceneName = wall.sceneName,
                    wallData = wall,
                    nonreplaceable = true,
                    tags = new() {
                        InteropTagFactory.CmiLocationTag(
                            poolGroup: WALL_GROUP,
                            pinSprite: new WallSprite(wall.sprite),
                            sceneNames: new List<string> { wall.sceneName },
                            mapLocations: wall.mapLocations.Select(x => x.repr).ToArray()
                        ),
                    }
                };

                BreakableWallItem wallItem = new()
                {
                    objectName = wall.gameObject,
                    sceneName = wall.sceneName,
                    name = wall.getItemName(),
                    wallData = wall,
                    UIDef = new MsgUIDef
                    {
                        name = new BoxedString(wall.niceName != "" ? wall.niceName : wall.getItemName()),
                        shopDesc = new BoxedString("\n" + wallShopDescriptions[random.Next(0, wallShopDescriptions.Length)]),
                        sprite = new WallSprite(wall.sprite)
                    },
                    tags = new() {
                        InteropTagFactory.CmiSharedTag(poolGroup: WALL_GROUP)
                    }
                };

                // Modding.Logger.LogDebug(wall.getLocationName() + " -> term: " + wall.getTermName() + " / itm: " + wall.getItemName());

                Finder.DefineCustomLocation(wallLocation); 
                Finder.DefineCustomItem(wallItem);
            }
        }

        public void Hook()
        {
            RCData.RuntimeLogicOverride.Subscribe(15f, ApplyLogic);

            RequestBuilder.OnUpdate.Subscribe(0.3f, AddWalls);

            RandomizerMenuAPI.OnGenerateStartLocationDict += RandomizerMenuAPI_OnGenerateStartLocationDict;
        }

        private void RandomizerMenuAPI_OnGenerateStartLocationDict(Dictionary<string, RandomizerMod.RandomizerData.StartDef> startDefs)
        {
            try
            {
                if (!BreakableWallRandomiser.settings.RandomizeWalls) { return; }

                (string westBlueLakeName, RandomizerMod.RandomizerData.StartDef westBlueLakeStart)
                        = startDefs.First(pair => pair.Value.SceneName == SceneNames.Crossroads_50);

                startDefs[westBlueLakeName] = westBlueLakeStart with
                {
                    Logic = "FALSE",
                    RandoLogic = "FALSE"
                };

                (string coloStartName, RandomizerMod.RandomizerData.StartDef coloStart)
                        = startDefs.First(pair => pair.Value.SceneName == SceneNames.Deepnest_East_09);

                startDefs[coloStartName] = coloStart with
                {
                    Logic = $"({coloStart.Logic}) + OBSCURESKIPS + ENEMYPOGOS + PRECISEMOVEMENT",
                    RandoLogic = "FALSE" // Only about 5 checks are reachable even *with* the above settings.
                };
            } catch (InvalidOperationException)
            {
                Modding.Logger.LogWarn("[Breakable Walls] Couldn't patch start locations.");
            }
        }

        private void AddWalls(RequestBuilder rb)
        {
            foreach (var wall in wallData)
            {
                if (!wall.shouldBeIncluded()) { continue; }

                rb.EditItemRequest(wall.getItemName(), info =>
                {
                    info.getItemDef = () => new()
                    {
                        Name = wall.getItemName(),
                        Pool = WALL_GROUP,
                        MajorItem = false,
                        PriceCap = 150
                    };
                });

                rb.EditLocationRequest(wall.getLocationName(), info =>
                {
                    info.getLocationDef = () => new()
                    {
                        Name = wall.getLocationName(),
                        SceneName = wall.sceneName,
                        FlexibleCount = false,
                        AdditionalProgressionPenalty = false
                    };
                });
            }

            if (BreakableWallRandomiser.settings.WallGroup > 0)
            {

                ItemGroupBuilder wallGroup = null;
                string label = RBConsts.SplitGroupPrefix + BreakableWallRandomiser.settings.WallGroup;

                foreach (ItemGroupBuilder igb in rb.EnumerateItemGroups())
                {
                    if (igb.label == label)
                    {
                        wallGroup = igb;
                        break;
                    }
                }

                wallGroup ??= rb.MainItemStage.AddItemGroup(label);

                rb.OnGetGroupFor.Subscribe(0.01f, ResolveWallGroup);

                bool ResolveWallGroup(RequestBuilder rb, string item, RequestBuilder.ElementType type, out GroupBuilder gb)
                {
                    if (wallData.Any(x => x.getItemName() == item || x.getLocationName() == item))
                    {
                        gb = wallGroup;
                        return true;
                    }

                    gb = default;
                    return false;
                }
            }

            foreach (var wall in wallData)
            {
                if (!wall.shouldBeIncluded()) { continue; }

                rb.AddItemByName(wall.getItemName());
                rb.AddLocationByName(wall.getLocationName());
            }
        }

        private void ApplyLogic(GenerationSettings gs, LogicManagerBuilder lmb)
        {
            foreach (var wall in wallData)
            {
                if (!wall.shouldBeIncluded()) { continue; }

                Term wallTerm = lmb.GetOrAddTerm(wall.getTermName());
                lmb.AddItem(new SingleItem(wall.getItemName(), new TermValue(wallTerm, 1)));

                lmb.AddLogicDef(new(wall.getLocationName(), wall.logic));

                foreach (var logicOverride in wall.logicOverrides)
                {
                    lmb.DoLogicEdit(new(logicOverride.Key, logicOverride.Value));
                }

                foreach (var substitutionDef in wall.logicSubstitutions)
                { 
                    foreach (var substitution in substitutionDef.Value)
                    {
                        lmb.DoSubst(new(substitutionDef.Key, substitution.Key, substitution.Value));  
                    }
                }
            }
        }
    }
}
