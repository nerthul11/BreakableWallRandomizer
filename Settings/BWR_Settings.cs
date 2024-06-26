namespace BreakableWallRandomizer.Settings
{
    public class BWR_Settings
    {
        public bool Enabled = false;
        public bool WoodenPlankWalls = false;
        public bool RockWalls = false;
        public bool DiveFloors = false;
        public bool KingsPass = false;

        [MenuChanger.Attributes.MenuRange(-1, 99)]
        public int WoodenPlankWallGroup = -1;

        [MenuChanger.Attributes.MenuRange(-1, 99)]
        public int RockWallGroup = -1;

        [MenuChanger.Attributes.MenuRange(-1, 99)]
        public int DiveFloorGroup = -1;        
        public bool GroupTogetherNearbyWalls = false;
        public bool ExcludeWallsWhichMaySoftlockYou = false;
        public MylaShopSettings MylaShop = new();
    }
}