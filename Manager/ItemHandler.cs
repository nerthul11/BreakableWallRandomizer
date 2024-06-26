using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BreakableWallRandomizer.IC;
using BreakableWallRandomizer.IC.Shop;
using ItemChanger;
using ItemChanger.Locations;
using Newtonsoft.Json;
using RandomizerCore.Logic;
using RandomizerMod.Logging;
using RandomizerMod.RC;
using RandomizerMod.Settings;

namespace BreakableWallRandomizer.Manager
{
    internal static class ItemHandler
    {
        internal static void Hook()
        {
            RequestBuilder.OnUpdate.Subscribe(-500f, DefineShopRef);
            RequestBuilder.OnUpdate.Subscribe(-400f, DefineGroups);
            RequestBuilder.OnUpdate.Subscribe(-100f, RandomizeShopCost);
            RequestBuilder.OnUpdate.Subscribe(0f, AddMylaShop);
            RequestBuilder.OnUpdate.Subscribe(1f, AddWalls);
            SettingsLog.AfterLogSettings += AddFileSettings;
        }

        private static void DefineGroups(RequestBuilder rb)
        {
            if (!BWR_Manager.Settings.Enabled)
                return;
            
            int wallSettings = BWR_Manager.Settings.RockWallGroup;
            int plankSettings = BWR_Manager.Settings.WoodenPlankWallGroup;
            int diveSettings = BWR_Manager.Settings.DiveFloorGroup;
            ItemGroupBuilder wallGroup = null;
            ItemGroupBuilder plankGroup = null;
            ItemGroupBuilder diveGroup = null;

            foreach (ItemGroupBuilder igb in rb.EnumerateItemGroups())
            {
                if (igb.label == RBConsts.SplitGroupPrefix + wallSettings)
                {
                    wallGroup = igb;
                }
                if (igb.label == RBConsts.SplitGroupPrefix + plankSettings)
                {
                    plankGroup = igb;
                }
                if (igb.label == RBConsts.SplitGroupPrefix + diveSettings)
                {
                    diveGroup = igb;
                }
            }

            rb.OnGetGroupFor.Subscribe(0.07f, ResolveGroups);
            bool ResolveGroups(RequestBuilder rb, string item, RequestBuilder.ElementType type, out GroupBuilder gb)
            {
                if (type == RequestBuilder.ElementType.Transition)
                {
                    gb = default;
                    return false;
                }

                if (item.StartsWith("Wall-") && wallGroup != null)
                {
                    gb = wallGroup;
                    return true;
                }

                if (item.StartsWith("Plank-") && plankGroup != null)
                {
                    gb = plankGroup;
                    return true;
                }

                if (item.StartsWith("Dive_Floor-") && diveGroup != null)
                {
                    gb = diveGroup;
                    return true;
                }

                gb = default;
                return false;
            }
        }

        private static void DefineShopRef(RequestBuilder builder)
        {
            if (!BWR_Manager.Settings.Enabled || !BWR_Manager.Settings.MylaShop.Enabled)
                return;

            builder.EditLocationRequest("Myla_Shop", info =>
            {
                info.getLocationDef = () => new()
                {
                    Name = "Myla_Shop",
                    SceneName = SceneNames.Crossroads_45,
                    FlexibleCount = true,
                    AdditionalProgressionPenalty = true
                };
            });
        }

        private static void RandomizeShopCost(RequestBuilder builder)
        {
            if (!BWR_Manager.Settings.Enabled || !BWR_Manager.Settings.MylaShop.Enabled)
                return;
            
            builder.CostConverters.Subscribe(150f, RandomizeCost);
            builder.EditLocationRequest("Myla_Shop", info =>
            {
                info.customPlacementFetch += (factory, rp) =>
                {
                    if (factory.TryFetchPlacement(rp.Location.Name, out AbstractPlacement plt))
                        return plt;
                    ShopLocation shop = (ShopLocation)factory.MakeLocation(rp.Location.Name);
                    plt = shop.Wrap();
                    factory.AddPlacement(plt);
                    return plt;
                };

                info.onRandoLocationCreation += (factory, rl) =>
                {
                    LogicManager lm = factory.lm;
                    Random rng = factory.rng;
                    List<string> usedTerms = [];
                    for (int i = 0; i < rng.Next(1, 5); i++)
                    {
                        int termNo = rng.Next(3);
                        if (termNo == 0 && !usedTerms.Contains("Walls")) // Walls
                        {
                            int wallCount = 53;
                            int minCost = Math.Max((int)(wallCount * BWR_Manager.Settings.MylaShop.MinimumCost), 1);
                            int maxCost = Math.Max((int)(wallCount * BWR_Manager.Settings.MylaShop.MaximumCost), 1);
                            usedTerms.Add("Walls");
                            rl.AddCost(new WallLogicCost(lm.GetTermStrict("Broken_Walls"), rng.Next(minCost, maxCost), amount => new WallCost(amount)));
                        }

                        if (termNo == 1 && !usedTerms.Contains("Planks")) // Planks
                        {
                            int wallCount = 49;
                            int minCost = Math.Max( (int)(wallCount * BWR_Manager.Settings.MylaShop.MinimumCost), 1);
                            int maxCost = Math.Max( (int)(wallCount * BWR_Manager.Settings.MylaShop.MaximumCost), 1);
                            usedTerms.Add("Planks");
                            rl.AddCost(new WallLogicCost(lm.GetTermStrict("Broken_Planks"), rng.Next(minCost, maxCost), amount => new PlankCost(amount)));
                        }

                        if (termNo == 2 && !usedTerms.Contains("Dives")) // Dives
                        {
                            int wallCount = 44;
                            int minCost = Math.Max( (int)(wallCount * BWR_Manager.Settings.MylaShop.MinimumCost), 1);
                            int maxCost = Math.Max( (int)(wallCount * BWR_Manager.Settings.MylaShop.MaximumCost), 1);
                            usedTerms.Add("Dives");
                            rl.AddCost(new WallLogicCost(lm.GetTermStrict("Broken_Dive_Floors"), rng.Next(minCost, maxCost), amount => new DiveCost(amount)));
                        }
                    }
                };
            });
        }

        private static bool RandomizeCost(LogicCost logicCost, out Cost cost)
        {
            if (logicCost is WallLogicCost c)
            {
                cost = c.GetIcCost();
                return true;
            }
            else
            {
                cost = default;
                return false;
            }
        }

        private static void AddMylaShop(RequestBuilder builder)
        {
            if (BWR_Manager.Settings.Enabled && BWR_Manager.Settings.MylaShop.Enabled)
                builder.AddLocationByName("Myla_Shop");
        }

        private static void AddWalls(RequestBuilder rb)
        {
            if (!BWR_Manager.Settings.Enabled)
                return;
            
            Assembly assembly = Assembly.GetExecutingAssembly();
            JsonSerializer jsonSerializer = new() {TypeNameHandling = TypeNameHandling.Auto};
            
            using Stream stream = assembly.GetManifestResourceStream("BreakableWallRandomizer.Resources.Data.BreakableWallObjects.json");
            StreamReader reader = new(stream);
            List<WallObject> wallList = jsonSerializer.Deserialize<List<WallObject>>(new JsonTextReader(reader));
            bool useGroups = BWR_Manager.Settings.GroupTogetherNearbyWalls;
            foreach (WallObject wall in wallList)
            {
                bool include = wall.name.StartsWith("Wall") && BWR_Manager.Settings.RockWalls;
                include = include || (wall.name.StartsWith("Plank") && BWR_Manager.Settings.WoodenPlankWalls);
                include = include || (wall.name.StartsWith("Dive_Floor") && BWR_Manager.Settings.DiveFloors);
                if (wall.name.Contains("White_Palace") || wall.name.Contains("Path_of_Pain"))
                    include = include && rb.gs.LongLocationSettings.WhitePalaceRando != LongLocationSettings.WPSetting.ExcludeWhitePalace;
                if (wall.name.Contains("King's_Pass"))
                    include = include && BWR_Manager.Settings.KingsPass;
                include = include && !(wall.exit && BWR_Manager.Settings.ExcludeWallsWhichMaySoftlockYou);

                if (include)
                {
                    if (!useGroups || (useGroups && wall.group == ""))
                    {
                        rb.AddItemByName(wall.name);
                        rb.EditItemRequest(wall.name, info =>
                        {
                            info.getItemDef = () => new()
                            {
                                Name = wall.name,
                                Pool = $"{wall.name.Split('-')[0]}s",
                                MajorItem = false,
                                PriceCap = 500
                            };
                        });
                        rb.AddLocationByName(wall.name);
                        rb.EditLocationRequest(wall.name, info =>
                        {
                            info.getLocationDef = () => new()
                            {
                                Name = wall.name,
                                SceneName = wall.sceneName,
                                FlexibleCount = false,
                                AdditionalProgressionPenalty = false
                            };
                        });
                    }
                }
                else
                    rb.AddToVanilla(new(wall.name, wall.name));
            }

            // Add wall groups only if Grouping is enabled
            if (useGroups)
            {
                using Stream gstream = assembly.GetManifestResourceStream("BreakableWallRandomizer.Resources.Data.WallGroups.json");
                StreamReader greader = new(gstream);
                List<WallObject> groupList = jsonSerializer.Deserialize<List<WallObject>>(new JsonTextReader(greader));

                foreach (WallObject group in groupList)
                {
                    foreach (WallObject wall in wallList)
                    {
                        if (wall.group == group.name.Split('-')[1])
                        {
                            bool include = wall.name.StartsWith("Wall") && BWR_Manager.Settings.RockWalls;
                            include = include || (wall.name.StartsWith("Plank") && BWR_Manager.Settings.WoodenPlankWalls);
                            include = include || (wall.name.StartsWith("Dive_Floor") && BWR_Manager.Settings.DiveFloors);
                            if (wall.name.Contains("White_Palace") || wall.name.Contains("Path_of_Pain"))
                                include = include && rb.gs.LongLocationSettings.WhitePalaceRando != LongLocationSettings.WPSetting.ExcludeWhitePalace;
                            if (wall.name.Contains("Tutorial"))
                                include = include && BWR_Manager.Settings.KingsPass;
                            include = include && !(wall.exit && BWR_Manager.Settings.ExcludeWallsWhichMaySoftlockYou);
                            if (include)
                                group.groupWalls.Add(new(wall.name, wall.sceneName, wall.gameObject, wall.fsmType));
                        }
                    }

                    if (group.groupWalls.Count > 0)
                    {
                        BreakableWallItem groupItem = new(group.name, group.sceneName, group.gameObject, group.fsmType, group.persistentBool, group.sprite, group.groupWalls);
                        BreakableWallLocation groupLocation = new(group.name, group.sceneName, group.gameObject, group.fsmType, group.alsoDestroy, group.x, group.y, group.exit, group.groupWalls);
                        
                        // By default, the items are defined with empty group walls, so they need to be redefined.
                        Finder.UndefineCustomItem(group.name);
                        Finder.UndefineCustomLocation(group.name);
                        Finder.DefineCustomItem(groupItem);
                        Finder.DefineCustomLocation(groupLocation);

                        rb.AddItemByName(group.name);
                        rb.EditItemRequest(group.name, info =>
                        {
                            info.getItemDef = () => new()
                            {
                                Name = group.name,
                                Pool = $"{group.name.Split('-')[0].Replace('_', ' ')}s",
                                MajorItem = false,
                                PriceCap = 500
                            };
                        });
                        rb.AddLocationByName(group.name);
                        rb.EditLocationRequest(group.name, info =>
                        {
                            info.getLocationDef = () => new()
                            {
                                Name = group.name,
                                SceneName = group.sceneName,
                                FlexibleCount = false,
                                AdditionalProgressionPenalty = false
                            };
                        });
                    }
                }
            }
        }

        private static void AddFileSettings(LogArguments args, TextWriter tw)
        {
            // Log settings into the settings file
            tw.WriteLine("Breakable Wall Randomizer Settings:");
            using JsonTextWriter jtw = new(tw) { CloseOutput = false };
            RandomizerMod.RandomizerData.JsonUtil._js.Serialize(jtw, BWR_Manager.Settings);
            tw.WriteLine();
        }
    }    
}