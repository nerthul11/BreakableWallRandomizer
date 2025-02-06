using BreakableWallRandomizer.Fsm;
using BreakableWallRandomizer.Modules;
using HutongGames.PlayMaker.Actions;
using ItemChanger;
using ItemChanger.Tags;
using ItemChanger.Util;
using Satchel;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BreakableWallRandomizer.IC
{
    [Serializable]
    public class WallLocation : AbstractWallLocation
    {
        public WallLocation(
            string name, string sceneName, string objectName, string fsmType, List<string> alsoDestroy, 
            float x, float y,  bool exit, List<CondensedWallObject> groupWalls
        )
        {
            this.name = name;
            this.sceneName = sceneName;
            this.objectName = objectName;
            this.fsmType = fsmType;
            this.alsoDestroy = alsoDestroy;
            this.exit = exit;
            this.groupWalls = groupWalls;
            flingType = FlingType.DirectDeposit;
            tags = [BreakableWallLocationTag(x, y)];
        }

        private InteropTag BreakableWallLocationTag(float x, float y)
        {
            // Define sprite by location type
            string sprite = "";
            if (name.StartsWith("Wall-") || name.StartsWith("Wall_Group"))
                sprite = "mine_break_wall_03_0deg";
            if (name.StartsWith("Plank-") || name.StartsWith("Plank_Group"))
                sprite = "wood_plank_02";
            if (name.StartsWith("Dive_Floor-") || name.StartsWith("Dive_Group"))
                sprite = "break_floor_glass";
            
            // Replace map name for pinless-maps
            string mapSceneName = sceneName;
            Dictionary<string, string> sceneOverride = [];
            sceneOverride.Add("Deepnest_45_v02", "Deepnest_39");
            sceneOverride.Add("Deepnest_Spider_Town", "Deepnest_10");
            sceneOverride.Add("Deepnest_East_17", "Deepnest_East_14");
            sceneOverride.Add("GG_Workshop", "GG_Waterways");
            sceneOverride.Add("GG_Atrium_Roof", "GG_Waterways");
            sceneOverride.Add("Mines_35", "Mines_28");
            sceneOverride.Add("Room_Colosseum_02", "Deepnest_East_09");
            sceneOverride.Add("Room_Colosseum_Spectate", "Deepnest_East_09");
            sceneOverride.Add("Room_Fungus_Shaman", "Fungus3_44");
            sceneOverride.Add("Room_GG_Shortcut", "GG_Waterways");
            sceneOverride.Add("White_Palace_06", "Abyss_05");
            sceneOverride.Add("White_Palace_09", "Abyss_05");
            sceneOverride.Add("White_Palace_12", "Abyss_05");
            sceneOverride.Add("White_Palace_15", "Abyss_05");

            if (sceneOverride.ContainsKey(sceneName))
                mapSceneName = sceneOverride[sceneName];

            InteropTag tag = new();
            tag.Properties["ModSource"] = "BreakableWallRandomizer";
            tag.Properties["PoolGroup"] = $"{name.Split('-')[0].Replace('_', ' ')}s";
            tag.Properties["PinSprite"] = new WallSprite(sprite);
            tag.Properties["VanillaItem"] = name;
            tag.Properties["MapLocations"] = new (string, float, float)[] {(mapSceneName, x, y)};
            tag.Message = "RandoSupplementalMetadata";
            return tag;
        }
        protected override void OnLoad()
        {           
            if (groupWalls.Count > 0)
            {
                foreach (CondensedWallObject wall in groupWalls)
                    Events.AddFsmEdit(wall.sceneName, new(wall.gameObject, wall.fsmType), ModifyWallBehaviour);
            }
            else
            {
                Events.AddFsmEdit(sceneName, new(objectName, fsmType), ModifyWallBehaviour);
                if (name == "Wall-Deepnest_Fungal")
                    Events.AddFsmEdit(SceneNames.Fungus2_20, new("/Breakable Wall Waterways", fsmType), ModifyWallBehaviour);
                if (name == "Plank-King's_Pass")
                    Events.AddFsmEdit(sceneName, new("Collapser Tute 01", "collapse tute"), ManageKPCollapse);
            }
        }

        protected override void OnUnload()
        {
            if (groupWalls.Count > 0)
            {
                foreach (CondensedWallObject wall in groupWalls)
                    Events.RemoveFsmEdit(wall.sceneName, new(wall.gameObject, wall.fsmType), ModifyWallBehaviour);
            }
            else
            {            
                Events.RemoveFsmEdit(sceneName, new(objectName, fsmType), ModifyWallBehaviour);
                if (name == "Wall-Fungal_Deepnest_Two_Way")
                    Events.RemoveFsmEdit(SceneNames.Fungus2_20, new("/Breakable Wall Waterways", fsmType), ModifyWallBehaviour);
                if (name == "Plank-King's_Pass")
                    Events.RemoveFsmEdit(sceneName, new("Collapser Tute 01", "collapse tute"), ManageKPCollapse);
            }
        }

        private void ModifyWallBehaviour(PlayMakerFSM fsm)
        {
            // This edit will affect all individual walls, so we list them to iterate
            List<CondensedWallObject> wallList = [];
            if (groupWalls.Count > 0)
            {
                foreach (CondensedWallObject wallObject in groupWalls)
                    wallList.Add(wallObject);
            }
            else
            {
                wallList.Add(new(name, sceneName, objectName, fsmType));
            }

            foreach (CondensedWallObject wall in wallList)
            {
                if (wall.fsmType != fsm.FsmName)
                    continue;

                // If a location is present, it means that it's not vanilla
                BreakableWallModule.Instance.vanillaWalls.RemoveAll(wall => wall.name == name);

                // This particular mask should be destroyed regardless of wall state
                if (wall.name == "Wall-Shade_Soul_Shortcut")
                    GameObject.Destroy(GameObject.Find("/Breakable Wall Ruin Lift/Masks"));

                // The wall will delete itself based on its state if we don't do this.
                if (wall.fsmType == "break_floor" || wall.fsmType == "FSM")
                {
                    fsm.ChangeTransition("Initiate", "ACTIVATE", "Idle");
                }
                else if (wall.fsmType == "breakable_wall_v2")
                {
                    // Shade Soul Shortcut is the only example of a two-way wall that can be accessed from both sides but
                    // destroyed only from one of them. TC made stuff happen to prevent players from destroying it from
                    // the left end - behaviour we'll preserve but only if the item isn't obtained.
                    if (wall.name == "Wall-Shade_Soul_Shortcut" && BreakableWallModule.Instance.UnlockedBreakables.Contains(wall.name))
                    {
                        fsm.ChangeTransition("Activated?", "ACTIVATE", "Initiate");
                        fsm.ChangeTransition("Activated?", "FINISHED", "Initiate");
                    }
                    else
                        fsm.ChangeTransition("Activated?", "ACTIVATE", "Ruin Lift?");
                } else if (wall.fsmType == "quake_floor")
                {
                    fsm.ChangeTransition("Init", "ACTIVATE", "Solid");
                    if (fsm.GetValidState("Transient").GetActions<SetBoxColliderTrigger>().Length >= 1)
                        fsm.RemoveAction("Transient", 0);
                    if (fsm.GetValidState("Solid").GetActions<SetBoxColliderTrigger>().Length >= 1)
                        fsm.RemoveAction("Solid", 0);

                    var collider = fsm.gameObject.GetComponent<BoxCollider2D>();
                    collider.isTrigger = true; // Make the first collider always a trigger

                    // Add our own collider for physics collision.
                    var newCollider = fsm.gameObject.AddComponent<BoxCollider2D>();
                    newCollider.offset = collider.offset;
                    newCollider.size = collider.size;
                } else if (wall.fsmType == "Detect Quake")
                {
                    fsm.ChangeTransition("Init", "ACTIVATE", "Detect");
                }

                // If the wall item had been obtained when calling GiveItem, destroy the wall on trigger.
                fsm.AddState("DeleteWall");
                fsm.AddCustomAction("DeleteWall", () => MakeWallPassable(fsm.gameObject, true));
                if (fsm.GetTransition(originalBreakStateName, "FINISHED") is not null)
                    fsm.ChangeTransition(originalBreakStateName, "FINISHED", "DeleteWall");
                else
                    fsm.AddTransition(originalBreakStateName, "FINISHED", "DeleteWall");

                // Add GiveItem state
                fsm.AddState("GiveItem");
                fsm.AddCustomAction("GiveItem", () =>
                {
                    ItemUtility.GiveSequentially(Placement.Items, Placement, new GiveInfo()
                    {
                        FlingType = FlingType.Everywhere,
                        MessageType = MessageType.Corner,
                    });

                    Placement.AddVisitFlag(VisitState.Opened);
                });
                fsm.AddAction("GiveItem", new CustomFsmBooleanCheck(
                    BreakableWallModule.Instance.UnlockedBreakables.Contains(wall.name), "OBTAINED", ""
                    ));
                fsm.AddTransition("GiveItem", "OBTAINED", originalBreakStateName);

                // If we already unlocked this wall, make it passable or destroy it.
                if (BreakableWallModule.Instance.UnlockedBreakables.Contains(wall.name))
                {
                    MakeWallPassable(fsm.gameObject, Placement.AllObtained());
                }
                else
                // If we didn't unlock this door yet...
                {
                    // ...and we already obtained the item at this location, set the wall to an unhittable state:
                    if (Placement.AllObtained())
                    {
                        fsm.SetState("GiveItem");
                    }
                    // ...and there are items left to collect:
                    else
                    {
                        foreach (var action in fsm.GetValidState(originalBreakStateName).Actions)
                        {
                            if (action is AudioPlayerOneShotSingle or PlayParticleEmitter or AudioPlayerOneShot)
                            {
                                fsm.AddAction("GiveItem", action);
                            }
                        }

                        // In case we're in the same scene when it breaks, check if there are items left,
                        // and then set states accordingly

                        fsm.AddState("BreakSameScene");

                        // In any of the cases, the wall is expected to become passable.
                        fsm.AddCustomAction("BreakSameScene", () =>
                        {
                            MakeWallPassable(fsm.gameObject, Placement.AllObtained());
                            Placement.AddVisitFlag(VisitState.Opened);
                        });

                        // If placement is cleared, make the wall disappear. Otherwise, set to hittable state.
                        fsm.AddAction("BreakSameScene", new CustomFsmBooleanCheck(
                                Placement.AllObtained(),
                                "CLEARED",
                                "UNCLEARED"
                            ));
                        fsm.AddTransition("BreakSameScene", "UNCLEARED", originalIdleStateName);
                        fsm.AddTransition("BreakSameScene", "CLEARED", originalBreakStateName);
                    }                    
                }

                if (wall.fsmType == "breakable_wall_v2")
                {
                    fsm.ChangeTransition("Idle", "WALL BREAKER", "GiveItem");
                    fsm.ChangeTransition("Pause Frame", "FINISHED", "GiveItem");
                    fsm.ChangeTransition("Spell Destroy", "FINISHED", "GiveItem");
                }
                else if (wall.fsmType == "FSM")
                {
                    fsm.ChangeTransition("Pause Frame", "FINISHED", "GiveItem");
                    fsm.ChangeTransition("Spell Destroy", "FINISHED", "GiveItem");
                }
                else if (wall.fsmType == "break_floor")
                {
                    fsm.ChangeTransition("Hit", "HIT 3", "GiveItem");
                } else if (wall.fsmType == "quake_floor")
                {
                    fsm.ChangeTransition("Transient", "DESTROY", "GiveItem");
                } else if (wall.fsmType == "Detect Quake")
                {
                    fsm.ChangeTransition("Quake Hit", "FINISHED", "GiveItem");
                }
            }
        }

        private void ManageKPCollapse(PlayMakerFSM fsm)
        {
            if (BreakableWallModule.Instance.UnlockedBreakables.Contains(name))
            {
                fsm.ChangeTransition("Init", "FINISHED", "Activate");   
            }
            else
            {
                fsm.ChangeTransition("Init", "ACTIVATE", "Idle");
                fsm.AddState("Give item");
                fsm.AddCustomAction("Give item", () => {
                    ItemUtility.GiveSequentially(Placement.Items, Placement, new GiveInfo()
                    {
                        FlingType = FlingType.DirectDeposit,
                        MessageType = MessageType.Corner,
                    });
                });
                fsm.ChangeTransition("Crumble", "BREAK", "Give item");
                if (Placement.GetUIName() == name.Replace("_", " ").Replace("-", " - "))
                {
                    fsm.AddCustomAction("Give item", () => {
                        var obj = GameObject.Find(objectName);
                        GameObject.Destroy(obj);
                    });
                    fsm.AddTransition("Give item", "FINISHED", "Break");
                }
            }
        }
    }
}