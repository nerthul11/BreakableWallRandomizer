using BreakableWallRandomizer.Modules;
using ItemChanger;
using ItemChanger.Tags;
using ItemChanger.UIDefs;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BreakableWallRandomizer.IC
{
    [Serializable]
    public class BreakableWallItem : AbstractItem
    {
        public string gameObject;
        public string fsmType;
        public string sceneName;
        public string persistentBool;
        public bool extra;
        public List<CondensedWallObject> groupWalls;
        public BreakableWallItem(
            string name, string sceneName, string gameObject, string fsmType, string persistentBool, 
            string sprite, bool extra, List<CondensedWallObject> groupWalls
        )
        {
            this.name = name;
            this.sceneName = sceneName;
            this.gameObject = gameObject;
            this.fsmType = fsmType;
            this.persistentBool = persistentBool;
            this.extra = extra;
            this.groupWalls = groupWalls;
            tags = [BreakableWallItemTag()];
        }

        private InteropTag BreakableWallItemTag()
        {
            // Define sprite by item type
            string sprite = "";
            if (name.StartsWith("Wall-") || name.StartsWith("Wall_Group"))
                sprite = "mine_break_wall_03_0deg";
            if (name.StartsWith("Plank-") || name.StartsWith("Plank_Group"))
                sprite = "wood_plank_02";
            if (name.StartsWith("Dive_Floor-") || name.StartsWith("Dive_Group"))
                sprite = "break_floor_glass";
            if (name.StartsWith("Collapser-") || name.StartsWith("Collapser_Group"))
                sprite = "collapser_short_0deg";
              
            InteropTag tag = new();
            tag.Properties["ModSource"] = "BreakableWallRandomizer";
            tag.Properties["PoolGroup"] = $"{name.Split('-')[0].Replace('_', ' ')}s";
            tag.Properties["PinSprite"] = new WallSprite(sprite);
            tag.Message = "RandoSupplementalMetadata";
            return tag;
        }

        public override void GiveImmediate(GiveInfo info)
        {
            // Set data in the save to indicate we got the wall
            if (name.StartsWith("Wall_Group") || name.StartsWith("Plank_Group") || name.StartsWith("Dive_Group") || name.StartsWith("Collapser_Group"))
            {
                foreach (CondensedWallObject wall in groupWalls)
                {
                    if (wall.name.StartsWith("Wall") && !BreakableWallModule.Instance.UnlockedWalls.Contains(name))
                        BreakableWallModule.Instance.UnlockedWalls.Add(wall.name);
                    if (wall.name.StartsWith("Plank") && !BreakableWallModule.Instance.UnlockedPlanks.Contains(name))
                        BreakableWallModule.Instance.UnlockedPlanks.Add(wall.name);
                    if (wall.name.StartsWith("Dive_Floor") && !BreakableWallModule.Instance.UnlockedDives.Contains(name))
                        BreakableWallModule.Instance.UnlockedDives.Add(wall.name);
                    if (name.StartsWith("Collapser") && !BreakableWallModule.Instance.UnlockedCollapsers.Contains(name))
                        BreakableWallModule.Instance.UnlockedCollapsers.Add(name);
                    
                    // If we're already in the same scene as the wall, break it.
                    if (GameManager.instance.sceneName == wall.sceneName)
                        GameObject.Find(wall.gameObject).LocateMyFSM(wall.fsmType).SetState("BreakSameScene");
                }
            }
            else
            {
                if (name.StartsWith("Wall") && !BreakableWallModule.Instance.UnlockedWalls.Contains(name))
                    BreakableWallModule.Instance.UnlockedWalls.Add(name);
                if (name.StartsWith("Plank") && !BreakableWallModule.Instance.UnlockedPlanks.Contains(name))
                    BreakableWallModule.Instance.UnlockedPlanks.Add(name);
                if (name.StartsWith("Dive_Floor") && !BreakableWallModule.Instance.UnlockedDives.Contains(name))
                    BreakableWallModule.Instance.UnlockedDives.Add(name);
                if (name.StartsWith("Collapser") && !BreakableWallModule.Instance.UnlockedCollapsers.Contains(name))
                    BreakableWallModule.Instance.UnlockedCollapsers.Add(name);

                // If we're already in the same scene as the wall, break it.
                if (GameManager.instance.sceneName == sceneName)
                    GameObject.Find(gameObject).LocateMyFSM(fsmType).SetState("BreakSameScene");
            }

            foreach (CondensedWallObject wall in groupWalls)
            {
                if (!BreakableWallModule.Instance.UnlockedBreakables.Contains(wall.name))
                    BreakableWallModule.Instance.UnlockedBreakables.Add(wall.name);
                if (GameManager.instance.sceneName == wall.sceneName)
                    GameObject.Find(wall.gameObject).LocateMyFSM(wall.fsmType).SetState("BreakSameScene");
            }
            if (!BreakableWallModule.Instance.UnlockedBreakables.Contains(name))
                    BreakableWallModule.Instance.UnlockedBreakables.Add(name);
            if (persistentBool != "")
                PlayerData.instance.SetBool(persistentBool, true);
            
            BreakableWallModule.Instance.CompletedChallenges();
        }
    }
}
