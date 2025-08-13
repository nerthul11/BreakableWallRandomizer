using System;

namespace BreakableWallRandomizer.Settings
{
    public class BWR_Settings
    {
        public bool Enabled { get; set; } = false;
        public WallSettings WoodenPlanks { get; set; } = new();
        public WallSettings RockWalls { get; set; } = new();
        public WallSettings DiveFloors { get; set; } = new();
        public WallSettings Collapsers { get; set; } = new();
        public bool GodhomeWalls { get; set; } = false;
        public bool GroupWalls { get; set; } = false;
        public MylaShopSettings MylaShop { get; set; } = new();
        public T GetVariable<T>(string propertyName) {
            var property = typeof(BWR_Settings).GetProperty(propertyName);
            if (property == null) {
                throw new ArgumentException($"Property '{propertyName}' not found in class.");
            }
            return (T)property.GetValue(this);
        }

        public void SetVariable<T>(string propertyName, T value) {
            var property = typeof(BWR_Settings).GetProperty(propertyName);
            if (property == null) {
                throw new ArgumentException($"Property '{propertyName}' not found in class.");
            }
            property.SetValue(this, value);
        }
    }
    
}