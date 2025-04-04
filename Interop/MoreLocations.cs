using BreakableWallRandomizer.IC.Shop;
using BreakableWallRandomizer.Manager;
using MoreLocations.Rando;
using MoreLocations.Rando.Costs;
using RandomizerCore.Logic;
using System;

namespace BreakableWallRandomizer.Interop
{
    internal static class MoreLocations_Interop
    {
        public static void Hook()
        {
            ConnectionInterop.AddRandoCostProviderToJunkShop(IncludeWalls, WallCostProvider);
            ConnectionInterop.AddRandoCostProviderToJunkShop(IncludePlanks, PlankCostProvider);
            ConnectionInterop.AddRandoCostProviderToJunkShop(IncludeDives, DiveCostProvider);
            ConnectionInterop.AddRandoCostProviderToJunkShop(IncludeCollapsers, CollapserCostProvider);
        }
        private static bool IncludeWalls() {
            bool include = BWR_Manager.Settings.Enabled && BWR_Manager.Settings.MylaShop.Enabled && BWR_Manager.Settings.MylaShop.IncludeInJunkShop;
            return include && (BWR_Manager.Settings.MylaShop.IncludeVanillaItems || BWR_Manager.Settings.RockWalls);
        }
        private static bool IncludePlanks() {
            bool include = BWR_Manager.Settings.Enabled && BWR_Manager.Settings.MylaShop.Enabled && BWR_Manager.Settings.MylaShop.IncludeInJunkShop;
            return include && (BWR_Manager.Settings.MylaShop.IncludeVanillaItems || BWR_Manager.Settings.WoodenPlanks);
        }
        private static bool IncludeDives() {
            bool include = BWR_Manager.Settings.Enabled && BWR_Manager.Settings.MylaShop.Enabled && BWR_Manager.Settings.MylaShop.IncludeInJunkShop;
            return include && (BWR_Manager.Settings.MylaShop.IncludeVanillaItems || BWR_Manager.Settings.DiveFloors);
        }
        private static bool IncludeCollapsers() {
            bool include = BWR_Manager.Settings.Enabled && BWR_Manager.Settings.MylaShop.Enabled && BWR_Manager.Settings.MylaShop.IncludeInJunkShop;
            return include && (BWR_Manager.Settings.MylaShop.IncludeVanillaItems || BWR_Manager.Settings.Collapsers);
        }
        private static WallCostProvider WallCostProvider() => new();
        private static PlankCostProvider PlankCostProvider() => new();
        private static DiveCostProvider DiveCostProvider() => new();
        private static CollapserCostProvider CollapserCostProvider() => new();
    }

    public class WallCostProvider : ICostProvider
    {
        public bool HasNonFreeCostsAvailable => true;

        public LogicCost Next(LogicManager lm, Random rng)
        {
            int wallCount = BWR_Manager.TotalWalls;
            int minCost = (int)(wallCount * BWR_Manager.Settings.MylaShop.MinimumCost);
            int maxCost = (int)(wallCount * BWR_Manager.Settings.MylaShop.MaximumCost);
            return new WallLogicCost(lm.GetTermStrict("Broken_Walls"), rng.Next(minCost, maxCost), amount => new WallCost(amount));
        }

        public void PreRandomize(Random rng) { }
    }

    internal class PlankCostProvider : ICostProvider
    {
        public bool HasNonFreeCostsAvailable => true;

        public LogicCost Next(LogicManager lm, Random rng)
        {
            int wallCount = BWR_Manager.TotalPlanks;
            int minCost = (int)(wallCount * BWR_Manager.Settings.MylaShop.MinimumCost);
            int maxCost = (int)(wallCount * BWR_Manager.Settings.MylaShop.MaximumCost);
            return new WallLogicCost(lm.GetTermStrict("Broken_Planks"), rng.Next(minCost, maxCost), amount => new PlankCost(amount));
        }

        public void PreRandomize(Random rng) { }
    }

    internal class DiveCostProvider : ICostProvider
    {
        public bool HasNonFreeCostsAvailable => true;

        public LogicCost Next(LogicManager lm, Random rng)
        {
            int wallCount = BWR_Manager.TotalDives;
            int minCost = (int)(wallCount * BWR_Manager.Settings.MylaShop.MinimumCost);
            int maxCost = (int)(wallCount * BWR_Manager.Settings.MylaShop.MaximumCost);
            return new WallLogicCost(lm.GetTermStrict("Broken_Dive_Floors"), rng.Next(minCost, maxCost), amount => new DiveCost(amount));
        }

        public void PreRandomize(Random rng) { }
    }

    internal class CollapserCostProvider : ICostProvider
    {
        public bool HasNonFreeCostsAvailable => true;

        public LogicCost Next(LogicManager lm, Random rng)
        {
            int wallCount = BWR_Manager.TotalCollapsers;
            int minCost = (int)(wallCount * BWR_Manager.Settings.MylaShop.MinimumCost);
            int maxCost = (int)(wallCount * BWR_Manager.Settings.MylaShop.MaximumCost);
            return new WallLogicCost(lm.GetTermStrict("Broken_Collapsers"), rng.Next(minCost, maxCost), amount => new CollapserCost(amount));
        }

        public void PreRandomize(Random rng) { }
    }
}