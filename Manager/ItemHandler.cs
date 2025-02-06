using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BreakableWallRandomizer.IC;
using BreakableWallRandomizer.IC.Shop;
using BreakableWallRandomizer.Modules;
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
            ProgressionInitializer.OnCreateProgressionInitializer += AddTolerance;
            RequestBuilder.OnUpdate.Subscribe(-500f, DefineShopRef);
            RequestBuilder.OnUpdate.Subscribe(-400f, DefineGroups);
            RequestBuilder.OnUpdate.Subscribe(-100f, RandomizeShopCost);
            RequestBuilder.OnUpdate.Subscribe(0f, AddMylaShop);
            RequestBuilder.OnUpdate.Subscribe(1f, AddWalls);
            SettingsLog.AfterLogSettings += AddFileSettings;
            RandoController.OnExportCompleted += InitiateModule;
        }

        private static void DefineGroups(RequestBuilder rb)
        {
            if (!BWR_Manager.Settings.Enabled)
                return;
            
            int wallSettings = BWR_Manager.Settings.RockWallGroup;
            int plankSettings = BWR_Manager.Settings.WoodenPlankWallGroup;
            int diveSettings = BWR_Manager.Settings.DiveFloorGroup;
            int collapserSettings = BWR_Manager.Settings.CollapserGroup;

            if (rb.gs.SplitGroupSettings.RandomizeOnStart && wallSettings >= 0 && wallSettings <= 2)
            {
                wallSettings = rb.rng.Next(3);
            }
            if (rb.gs.SplitGroupSettings.RandomizeOnStart && plankSettings >= 0 && plankSettings <= 2)
            {
                plankSettings = rb.rng.Next(3);
            }
            if (rb.gs.SplitGroupSettings.RandomizeOnStart && diveSettings >= 0 && diveSettings <= 2)
            {
                diveSettings = rb.rng.Next(3);
            }
            if (rb.gs.SplitGroupSettings.RandomizeOnStart && collapserSettings >= 0 && collapserSettings <= 2)
            {
                diveSettings = rb.rng.Next(3);
            }

            ItemGroupBuilder wallGroup = null;
            ItemGroupBuilder plankGroup = null;
            ItemGroupBuilder diveGroup = null;
            ItemGroupBuilder collapserGroup = null;

            if (wallSettings > 0)
            {
                try 
                {
                    wallGroup = rb.MainItemStage.AddItemGroup(RBConsts.SplitGroupPrefix + wallSettings);
                }
                catch (ArgumentException) {}
            }

            if (plankSettings > 0)
            {
                try 
                {
                    plankGroup = rb.MainItemStage.AddItemGroup(RBConsts.SplitGroupPrefix + plankSettings);
                }
                catch (ArgumentException) {}
            }

            if (diveSettings > 0)
            {
                try 
                    {
                    diveGroup = rb.MainItemStage.AddItemGroup(RBConsts.SplitGroupPrefix + diveSettings);
                }
                catch (ArgumentException) {}
            }

            if (collapserSettings > 0)
            {
                try 
                    {
                    collapserGroup = rb.MainItemStage.AddItemGroup(RBConsts.SplitGroupPrefix + collapserSettings);
                }
                catch (ArgumentException) {}
            }

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

                if ((item.StartsWith("Wall-") || item.StartsWith("Wall_Group-")) && wallGroup != null)
                {
                    gb = wallGroup;
                    return true;
                }

                if ((item.StartsWith("Plank-") || item.StartsWith("Plank_Group-")) && plankGroup != null)
                {
                    gb = plankGroup;
                    return true;
                }

                if ((item.StartsWith("Dive_Floor-") || item.StartsWith("Dive_Group-")) && diveGroup != null)
                {
                    gb = diveGroup;
                    return true;
                }

                if ((item.StartsWith("Collapser-") || item.StartsWith("Collapser_Group-")) && collapserGroup != null)
                {
                    gb = collapserGroup;
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
                    List<string> availableTerms = [];
                    List<string> usedTerms = [];
                    if (BWR_Manager.Settings.MylaShop.IncludeVanillaItems || BWR_Manager.Settings.RockWalls)
                        availableTerms.Add("Walls");
                    if (BWR_Manager.Settings.MylaShop.IncludeVanillaItems || BWR_Manager.Settings.WoodenPlanks)
                        availableTerms.Add("Planks");
                    if (BWR_Manager.Settings.MylaShop.IncludeVanillaItems || BWR_Manager.Settings.DiveFloors)
                        availableTerms.Add("Dives");
                    if (BWR_Manager.Settings.MylaShop.IncludeVanillaItems || BWR_Manager.Settings.DiveFloors)
                        availableTerms.Add("Collapsers");
                    for (int i = 0; i < rng.Next(1, 1 + availableTerms.Count); i++)
                    {
                        int termNo = rng.Next(availableTerms.Count);
                        if (availableTerms.IndexOf("Walls") == termNo && !usedTerms.Contains("Walls")) // Walls
                        {
                            int wallCount = BWR_Manager.TotalWalls;
                            int minCost = Math.Max((int)(wallCount * BWR_Manager.Settings.MylaShop.MinimumCost), 1);
                            int maxCost = Math.Max((int)(wallCount * BWR_Manager.Settings.MylaShop.MaximumCost), 1);
                            usedTerms.Add("Walls");
                            rl.AddCost(new WallLogicCost(lm.GetTermStrict("Broken_Walls"), rng.Next(minCost, maxCost), amount => new WallCost(amount)));
                        }

                        if (availableTerms.IndexOf("Planks") == termNo && !usedTerms.Contains("Planks")) // Planks
                        {
                            int wallCount = BWR_Manager.TotalPlanks;
                            int minCost = Math.Max((int)(wallCount * BWR_Manager.Settings.MylaShop.MinimumCost), 1);
                            int maxCost = Math.Max((int)(wallCount * BWR_Manager.Settings.MylaShop.MaximumCost), 1);
                            usedTerms.Add("Planks");
                            rl.AddCost(new WallLogicCost(lm.GetTermStrict("Broken_Planks"), rng.Next(minCost, maxCost), amount => new PlankCost(amount)));
                        }

                        if (availableTerms.IndexOf("Dives") == termNo && !usedTerms.Contains("Dives")) // Dives
                        {
                            int wallCount = BWR_Manager.TotalDives;
                            int minCost = Math.Max((int)(wallCount * BWR_Manager.Settings.MylaShop.MinimumCost), 1);
                            int maxCost = Math.Max((int)(wallCount * BWR_Manager.Settings.MylaShop.MaximumCost), 1);
                            usedTerms.Add("Dives");
                            rl.AddCost(new WallLogicCost(lm.GetTermStrict("Broken_Dive_Floors"), rng.Next(minCost, maxCost), amount => new DiveCost(amount)));
                        }

                        if (availableTerms.IndexOf("Collapsers") == termNo && !usedTerms.Contains("Collapsers")) // Dives
                        {
                            int wallCount = BWR_Manager.TotalCollapsers;
                            int minCost = Math.Max((int)(wallCount * BWR_Manager.Settings.MylaShop.MinimumCost), 1);
                            int maxCost = Math.Max((int)(wallCount * BWR_Manager.Settings.MylaShop.MaximumCost), 1);
                            usedTerms.Add("Dives");
                            rl.AddCost(new WallLogicCost(lm.GetTermStrict("Broken_Collapsers"), rng.Next(minCost, maxCost), amount => new DiveCost(amount)));
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
            List<AbstractWallItem> wallList = jsonSerializer.Deserialize<List<AbstractWallItem>>(new JsonTextReader(reader));
            bool useGroups = BWR_Manager.Settings.GroupTogetherNearbyWalls;
            foreach (AbstractWallItem wall in wallList)
            {
                bool include = wall.name.StartsWith("Wall") && BWR_Manager.Settings.RockWalls;
                include = include || (wall.name.StartsWith("Plank") && BWR_Manager.Settings.WoodenPlanks);
                include = include || (wall.name.StartsWith("Dive_Floor") && BWR_Manager.Settings.DiveFloors);
                if (wall.name.Contains("White_Palace") || wall.name.Contains("Path_of_Pain"))
                    include = include && rb.gs.LongLocationSettings.WhitePalaceRando != LongLocationSettings.WPSetting.ExcludeWhitePalace;
                if (wall.name.Contains("King's_Pass"))
                    include = BWR_Manager.Settings.KingsPass;
                include = include && !(wall.exit && BWR_Manager.Settings.ExcludeWallsWhichMaySoftlockYou);
                include = include && (!(wall.name.Contains("Godhome") || wall.name.Contains("Eternal_Ordeal")) || BWR_Manager.Settings.GodhomeWalls);

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
                List<AbstractWallItem> groupList = jsonSerializer.Deserialize<List<AbstractWallItem>>(new JsonTextReader(greader));

                foreach (AbstractWallItem group in groupList)
                {
                    foreach (AbstractWallItem wall in wallList)
                    {
                        if (wall.group == group.name.Split('-')[1])
                        {
                            bool include = wall.name.StartsWith("Wall") && BWR_Manager.Settings.RockWalls;
                            include = include || (wall.name.StartsWith("Plank") && BWR_Manager.Settings.WoodenPlanks);
                            include = include || (wall.name.StartsWith("Dive_Floor") && BWR_Manager.Settings.DiveFloors);
                            include = include || (wall.name.StartsWith("Collapser") && BWR_Manager.Settings.Collapsers);
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

        private static void AddTolerance(LogicManager lm, GenerationSettings gs, ProgressionInitializer pi)
        {
            if (!BWR_Manager.Settings.Enabled || !BWR_Manager.Settings.MylaShop.Enabled)
                return;
            
            int wallCount = BWR_Manager.TotalWalls;
            int wallCost = Math.Max((int)(wallCount * BWR_Manager.Settings.MylaShop.MaximumCost), 1);
            int wallTolerance = Math.Min((int)(wallCost * BWR_Manager.Settings.MylaShop.Tolerance), wallCount - wallCost);
            pi.Setters.Add(new RandomizerCore.TermValue(lm.GetTermStrict("Broken_Walls"), -wallTolerance));

            int plankCount = BWR_Manager.TotalPlanks;
            int plankCost = Math.Max((int)(plankCount * BWR_Manager.Settings.MylaShop.MaximumCost), 1);
            int plankTolerance = Math.Min((int)(plankCost * BWR_Manager.Settings.MylaShop.Tolerance), plankCount - plankCost);
            pi.Setters.Add(new RandomizerCore.TermValue(lm.GetTermStrict("Broken_Planks"), -plankTolerance));

            int diveCount = BWR_Manager.TotalDives;
            int diveCost = Math.Max((int)(diveCount * BWR_Manager.Settings.MylaShop.MaximumCost), 1);
            int diveTolerance = Math.Min((int)(diveCost * BWR_Manager.Settings.MylaShop.Tolerance), diveCount - diveCost);
            pi.Setters.Add(new RandomizerCore.TermValue(lm.GetTermStrict("Broken_Dive_Floors"), -diveTolerance));
        }

        private static void AddFileSettings(LogArguments args, TextWriter tw)
        {
            if (!BWR_Manager.Settings.Enabled)
                return;

            // Log settings into the settings file
            tw.WriteLine("Breakable Wall Randomizer Settings:");
            using JsonTextWriter jtw = new(tw) { CloseOutput = false };
            RandomizerMod.RandomizerData.JsonUtil._js.Serialize(jtw, BWR_Manager.Settings);
            tw.WriteLine();
        }

        private static void InitiateModule(RandoController controller)
        {
            if (!BWR_Manager.Settings.Enabled)
                return;
            
            ItemChangerMod.Modules.GetOrAdd<BreakableWallModule>();
        }
    }    
}