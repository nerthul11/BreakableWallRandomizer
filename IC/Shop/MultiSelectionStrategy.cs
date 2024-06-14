using ItemChanger;
using ItemChanger.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace BreakableWallRandomizer.IC.Shop
{
    public class MultiCostDisplayerSelectionStrategy : ICostDisplayerSelectionStrategy
    {
        public List<IMixedCostSupport> Displayers { get; set; } = new();
        public CostDisplayer CostDisplayer;
        public CostDisplayer GetCostDisplayer(AbstractItem item) 
        {
            Cost c = item.GetTag<CostTag>()?.Cost;
            if (c == null)
            {
                return new GeoCostDisplayer();
            }

            Cost baseCost = c.GetBaseCost();
            if (baseCost is MultiCost mc)
            {
                return FindBestMatchForBaseCosts(mc.Select(cc => cc.GetBaseCost()));
            }
            else
            {
                return FindBestMatchForBaseCosts(baseCost.Yield());
            }
        }

        private CostDisplayer FindBestMatchForBaseCosts(IEnumerable<Cost> costs)
        {
            foreach (Cost c in costs)
            {
                CostDisplayer bestDisplayer = Displayers
                    .Where(cap => cap.MatchesCost(c))
                    .Select(cap => cap.GetDisplayer(c))
                    .FirstOrDefault();
                if (bestDisplayer != null)
                {
                    return bestDisplayer;
                }
            }
            return new GeoCostDisplayer();
        }
    }
}