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
            ConnectionInterop.AddRandoCostProviderToJunkShop(CanProvideCosts, WallCostProvider);
            ConnectionInterop.AddRandoCostProviderToJunkShop(CanProvideCosts, PlankCostProvider);
            ConnectionInterop.AddRandoCostProviderToJunkShop(CanProvideCosts, DiveCostProvider);
        }
        private static bool CanProvideCosts() => BWR_Manager.Settings.Enabled && BWR_Manager.Settings.MylaShop.IncludeInJunkShop;
        private static WallCostProvider WallCostProvider() => new();
        private static PlankCostProvider PlankCostProvider() => new();
        private static DiveCostProvider DiveCostProvider() => new();
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
}