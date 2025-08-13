using MenuChanger.Attributes;

namespace BreakableWallRandomizer.Settings
{
    public class WallSettings
    {
        public bool Enabled { get; set; } = false;

        public bool AdditionalWalls { get; set; } = false;
        
        [MenuRange(-1, 99)]
        public int Group = -1;
    }
}