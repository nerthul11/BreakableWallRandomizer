using MenuChanger.Attributes;

namespace BreakableWallRandomizer.Settings
{
    public class MylaShopSettings
    {
        public bool Enabled { get; set; } = false;
        [MenuRange(0f, 1f)]
        [DynamicBound(nameof(MaximumCost), true)]
        public float MinimumCost { get; set; } = 0.25f;
        [MenuRange(0f, 1f)]
        [DynamicBound(nameof(MinimumCost), false)]
        public float MaximumCost { get; set; } = 0.75f;
        [MenuRange(0f, 1f)]
        public float Tolerance { get; set; } = 0.2f;
        public bool IncludeVanillaItems { get; set; } = false;
        public bool IncludeInJunkShop { get; set; } = false;
    }
}