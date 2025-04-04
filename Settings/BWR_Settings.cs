namespace BreakableWallRandomizer.Settings
{
    public class BWR_Settings
    {
        public bool Enabled = false;
        public bool WoodenPlanks = false;
        public bool RockWalls = false;
        public bool DiveFloors = false;
        public bool Collapsers = false;
        public bool ExtraWalls = false;
        public bool GodhomeWalls = false;

        [MenuChanger.Attributes.MenuRange(-1, 99)]
        public int WoodenPlankWallGroup = -1;

        [MenuChanger.Attributes.MenuRange(-1, 99)]
        public int RockWallGroup = -1;

        [MenuChanger.Attributes.MenuRange(-1, 99)]
        public int DiveFloorGroup = -1;
        [MenuChanger.Attributes.MenuRange(-1, 99)]
        public int CollapserGroup = -1;        
        public bool GroupWalls = false;
        
        public MylaShopSettings MylaShop = new();
    }
}