using ItemChanger;
using BreakableWallRandomizer.Modules;

namespace BreakableWallRandomizer.IC.Shop
{
    public sealed record WallCost(int amount) : Cost
    {
        public override bool CanPay() => BreakableWallModule.Instance.UnlockedWalls.Count >= amount;
        public override void OnPay() { }
        public override bool HasPayEffects() => false;
        public override string GetCostText() => $"{amount} walls";
    }

    public sealed record PlankCost(int amount) : Cost
    {
        public override bool CanPay() => BreakableWallModule.Instance.UnlockedPlanks.Count >= amount;
        public override void OnPay() { }
        public override bool HasPayEffects() => false;
        public override string GetCostText() => $"{amount} planks";
    }

    public sealed record DiveCost(int amount) : Cost
    {
        public override bool CanPay() => BreakableWallModule.Instance.UnlockedDives.Count >= amount;
        public override void OnPay() { }
        public override bool HasPayEffects() => false;
        public override string GetCostText() => $"{amount} dive floors";
    }

    public sealed record CollapserCost(int amount) : Cost
    {
        public override bool CanPay() => BreakableWallModule.Instance.UnlockedCollapsers.Count >= amount;
        public override void OnPay() { }
        public override bool HasPayEffects() => false;
        public override string GetCostText() => $"{amount} collapser floors";
    }
}