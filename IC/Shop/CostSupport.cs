using ItemChanger;

namespace BreakableWallRandomizer.IC.Shop
{
    public record WallCostSupport(string displayer, string sprite) : IMixedCostSupport
    {
        public CostDisplayer GetDisplayer(Cost c)
        {
            return new WallCostDisplayer(displayer, sprite);
        }

        public bool MatchesCost(Cost c)
        {
            return c is WallCost && displayer == "Wall" || c is PlankCost && displayer == "Plank" || c is DiveCost && displayer == "Dive";
        }
    }
}