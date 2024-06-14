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
        public string sceneName;
        public string gameObject;
        public string fsmType;
        public string persistentBool;
        public List<CondensedWallObject> groupWalls;
        public BreakableWallItem(
            string name, string sceneName, string gameObject, string fsmType, string persistentBool, 
            string sprite, List<CondensedWallObject> groupWalls
        )
        {
            this.name = name;
            this.sceneName = sceneName;
            this.gameObject = gameObject;
            this.fsmType = fsmType;
            this.persistentBool = persistentBool;
            this.groupWalls = groupWalls;
            UIDef = new MsgUIDef()
            {
                name = new BoxedString(name.Replace("_", " ").Replace("-", " - ")),
                shopDesc = new BoxedString(""),
                sprite = new WallSprite(sprite)
            };
            tags = [BreakableWallItemTag()];
        }

        private InteropTag BreakableWallItemTag()
        {
            // Define sprite by item type
            string sprite = "";
            if (name.StartsWith("Wall-"))
                sprite = "mine_break_wall_03_0deg";
            if (name.StartsWith("Plank-"))
                sprite = "wood_plank_02";
            if (name.StartsWith("Dive_Floor-"))
                sprite = "break_floor";
              
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
            if (name.StartsWith("Wall_Group"))
            {
                foreach (CondensedWallObject wall in groupWalls)
                {
                    if (!BreakableWallModule.Instance.UnlockedBreakableWalls.Contains(name))
                        BreakableWallModule.Instance.UnlockedBreakableWalls.Add(name);
                    if (wall.name.StartsWith("Wall") && !BreakableWallModule.Instance.UnlockedWalls.Contains(name))
                        BreakableWallModule.Instance.UnlockedWalls.Add(wall.name);
                    if (wall.name.StartsWith("Plank") && !BreakableWallModule.Instance.UnlockedPlanks.Contains(name))
                        BreakableWallModule.Instance.UnlockedPlanks.Add(wall.name);
                    if (wall.name.StartsWith("Dive_Floor") && !BreakableWallModule.Instance.UnlockedDives.Contains(name))
                        BreakableWallModule.Instance.UnlockedDives.Add(wall.name);
                    
                    // If we're already in the same scene as the wall, break it. The wall's FSM should spawn a shiny.
                    if (GameManager.instance.sceneName == wall.sceneName)
                        GameObject.Find(wall.gameObject).LocateMyFSM(wall.fsmType).SetState("BreakSameScene");
                }
            }
            else
            {
                BreakableWallModule.Instance.UnlockedBreakableWalls.Add(name);
                if (name.StartsWith("Wall") && !BreakableWallModule.Instance.UnlockedWalls.Contains(name))
                    BreakableWallModule.Instance.UnlockedWalls.Add(name);
                if (name.StartsWith("Plank") && !BreakableWallModule.Instance.UnlockedPlanks.Contains(name))
                    BreakableWallModule.Instance.UnlockedPlanks.Add(name);
                if (name.StartsWith("Dive_Floor") && !BreakableWallModule.Instance.UnlockedDives.Contains(name))
                    BreakableWallModule.Instance.UnlockedDives.Add(name);
                
                // If we're already in the same scene as the wall, break it. The wall's FSM should spawn a shiny.
                if (GameManager.instance.sceneName == sceneName)
                    GameObject.Find(gameObject).LocateMyFSM(fsmType).SetState("BreakSameScene");
            }
            if (persistentBool != "")
                PlayerData.instance.SetBool(persistentBool, true);  

            BreakableWallModule.Instance.CompletedChallenges();         
        }
    }
}
