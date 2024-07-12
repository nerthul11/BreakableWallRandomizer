using BreakableWallRandomizer.Modules;
using HutongGames.PlayMaker.Actions;
using ItemChanger;
using ItemChanger.Locations;
using ItemChanger.Tags;
using ItemChanger.Util;
using Satchel;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BreakableWallRandomizer.IC
{
    [Serializable]
    public class BreakableWallLocation : AutoLocation
    {
        public string objectName;
        public string fsmType;
        public List<string> alsoDestroy;
        public bool exit;
        public List<CondensedWallObject> groupWalls;
        public BreakableWallLocation(
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
            if (name.StartsWith("Wall-"))
                sprite = "mine_break_wall_03_0deg";
            if (name.StartsWith("Plank-"))
                sprite = "wood_plank_02";
            if (name.StartsWith("Dive_Floor-"))
                sprite = "break_floor_glass";
            if (name.StartsWith("Wall_Group-"))
                sprite = "mine_break_wall_03_0deg";
            
            // Replace map name for pinless-maps
            string mapSceneName = sceneName;
            Dictionary<string, string> sceneOverride = [];
            sceneOverride.Add("Deepnest_45_v02", "Deepnest_39");
            sceneOverride.Add("Deepnest_Spider_Town", "Deepnest_10");
            sceneOverride.Add("Deepnest_East_17", "Deepnest_East_14");
            sceneOverride.Add("GG_Workshop", "GG_Waterways");
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

        private void MakeWallPassable(GameObject go)
        {
            foreach (var objectName in alsoDestroy)
            {
                try
                {
                    var obj = GameObject.Find(objectName);
                    GameObject.Destroy(obj);
                } catch { }
            }
            Recursive_MakeWallPassable(go);
        }

        // Recursively set all colliders as triggers on a given gameObject.
        // Also recursively set any SpriteRenderers on a given gameObject to 0.5 alpha.
        // Also remove any object called "Camera lock" or any textures beginning with msk_. 
        private void Recursive_MakeWallPassable(GameObject go)
        {
            foreach (var collider in go.GetComponents<Collider2D>())
            {
                // Triggers can still be hit by a nail, but won't impede player movement.
                collider.isTrigger = true;
            }

            // Make sprites transparent
            foreach (var sprite in go.GetComponents<SpriteRenderer>())
            {
                Color tmp = sprite.color;
                if (fsmType == "Detect Quake" || fsmType == "quake_floor")
                {
                    tmp.a = 0.4f;
                } else
                {
                    tmp.a = 0.5f;
                }
                sprite.color = tmp;

                if (sprite.sprite && sprite.sprite.name.StartsWith("msk"))
                {
                    sprite.enabled = false;
                }
            }

            if (go.name.Contains("Camera"))
            {
                GameObject.Destroy(go);
            }

            for (var i = 0; i < go.transform.childCount; i++)
            {
                MakeWallPassable(go.transform.GetChild(i).gameObject);
            }
        }

        private void ModifyWallBehaviour(PlayMakerFSM fsm)
        {
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
                    if (wall.name == "Wall-Shade_Soul_Shortcut" && BreakableWallModule.Instance.UnlockedBreakableWalls.Contains(wall.name))
                    {
                        fsm.ChangeTransition("Activated?", "ACTIVATE", "Initiate");
                        fsm.ChangeTransition("Activated?", "FINISHED", "Initiate");
                    }
                    else
                        fsm.ChangeTransition("Activated?", "ACTIVATE", "Ruin Lift?");
                } else if (wall.fsmType == "quake_floor")
                {
                    fsm.ChangeTransition("Init", "ACTIVATE", "Solid");
                    fsm.RemoveAction("Transient", 0); // Sets the floor to a trigger
                    if (fsm.GetState("Solid").GetActions<SetBoxColliderTrigger>().Length >= 1)
                    {
                        fsm.RemoveAction("Solid", 0); // Sets the floor to a triggern't
                    }

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

                fsm.AddState("GiveItem");
                fsm.AddCustomAction("GiveItem", () =>
                {
                    ItemUtility.GiveSequentially(Placement.Items, Placement, new GiveInfo()
                    {
                        FlingType = FlingType.Everywhere,
                        MessageType = MessageType.Corner,
                    });

                    Placement.AddVisitFlag(VisitState.Opened);

                    if (BreakableWallModule.Instance.UnlockedBreakableWalls.Contains(wall.name))
                    {
                        // Delete the wall entirely.
                        if (fsmType == "quake_floor") 
                            fsm.SetState("Destroy");
                        else if (fsmType == "Detect Quake") 
                            fsm.SetState("Break 2");
                        else 
                            fsm.SetState("Break");
                    }
                });

                // If we already unlocked this wall, and items are still left there, make it passable.
                if (BreakableWallModule.Instance.UnlockedBreakableWalls.Contains(wall.name))
                {
                    // If items are left, make wall semi-transparent and passable
                    if (!Placement.AllObtained())
                    {
                        MakeWallPassable(fsm.gameObject);
                    }
                    else
                    {
                        // Ensure the wall deletes on-load.
                        if (wall.fsmType == "quake_floor")
                        {
                            fsm.ChangeTransition("Init", "FINISHED", "Activate");
                            fsm.ChangeTransition("Init", "ACTIVATE", "Activate");
                        } else if (wall.fsmType == "Detect Quake") {
                            fsm.ChangeTransition("Init", "ACTIVATE", "Activate !!!");
                            fsm.ChangeTransition("Init", "FINISHED", "Activate !!!");
                        } else
                        {
                            fsm.ChangeTransition("Initiate", "FINISHED", "Activated");
                            fsm.ChangeTransition("Initiate", "ACTIVATE", "Activated");
                        }
                    }
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
                        var originalIdleStateName = wall.fsmType switch
                        {
                            "quake_floor" => "Solid",

                            "Detect Quake" => "Detect",

                            _ => "Idle"
                        };

                        // Copy sound and particles from original
                        var originalBreakStateName = wall.fsmType switch
                        {
                            "quake_floor" => "Glass",

                            "Detect Quake" => "Break 2",

                            _ => "Break"
                        };

                        foreach (var action in fsm.GetState(originalBreakStateName).Actions)
                        {
                            if (action is AudioPlayerOneShotSingle or PlayParticleEmitter or AudioPlayerOneShot)
                            {
                                fsm.AddAction("GiveItem", action);
                            }
                        }

                        // In case we're in the same scene when it breaks, check if there are items left,
                        // and then set states accordingly

                        fsm.AddState("BreakSameScene");

                        fsm.InsertCustomAction("BreakSameScene", () =>
                        {
                            if (Placement.AllObtained())
                            {
                                MakeWallPassable(fsm.gameObject);
                                fsm.SetState(originalIdleStateName);
                            }
                            else
                            {
                                if (wall.fsmType == "quake_floor") 
                                {
                                    MakeWallPassable(fsm.gameObject);
                                } // ensure everything is passable.
                                fsm.SetState(originalBreakStateName);
                            }

                            Placement.AddVisitFlag(VisitState.Opened);
                        }, 0);
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
                    fsm.ChangeTransition("PD Bool?", "FINISHED", "GiveItem");
                } else if (wall.fsmType == "Detect Quake")
                {
                    fsm.ChangeTransition("Quake Hit", "FINISHED", "GiveItem");
                }
            }
        }

        private void ManageKPCollapse(PlayMakerFSM fsm)
        {
            if (BreakableWallModule.Instance.UnlockedBreakableWalls.Contains(name))
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
