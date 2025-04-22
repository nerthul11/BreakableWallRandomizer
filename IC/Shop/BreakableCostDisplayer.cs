using ItemChanger;

namespace BreakableWallRandomizer.IC.Shop
{
    public class WallCostDisplayer(string _cost, string _sprite) : CostDisplayer
    {
        public string cost = _cost;
        public string sprite = _sprite;
        public override ISprite CustomCostSprite { get; set; } = new WallSprite(_sprite);
        public override bool Cumulative => true;
        protected override bool SupportsCost(Cost c) 
        {
            if (cost == "Wall")
                return c is WallCost;
            if (cost == "Plank")
                return c is PlankCost;
            if (cost == "Dive")
                return c is DiveCost;
            if (cost == "Collapser")
                return c is CollapserCost;
            return false;
        }
        protected override int GetSingleCostDisplayAmount(Cost c)
        {
            if (cost == "Wall")
                return ((WallCost)c).amount;
            if (cost == "Plank")
                return ((PlankCost)c).amount;
            if (cost == "Dive")
                return ((DiveCost)c).amount;
            if (cost == "Collapser")
                return ((CollapserCost)c).amount;
            return 0;
        }
    }
}