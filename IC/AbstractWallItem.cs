using System;
using System.Collections.Generic;
using BreakableWallRandomizer.Modules;
using ItemChanger;
using UnityEngine;

namespace BreakableWallRandomizer.IC
{
    [Serializable]
    public class AbstractWallItem : AbstractItem
    {
        public string gameObject;
        public string fsmType;
        public string sceneName;
        public float x;
        public float y;
        public string persistentBool;
        public string sprite;
        public List<string> alsoDestroy;
        public bool exit;
        public string group;
        public List<CondensedWallObject> groupWalls;
        public string logic;
        public Dictionary<string, string> logicOverrides;
        public Dictionary<string, Dictionary<string, string>> logicSubstitutions;

        public override void GiveImmediate(GiveInfo info)
        {
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

    [Serializable]
    public class CondensedWallObject
    {
        public string name;
        public string sceneName;
        public string gameObject;
        public string fsmType;
        public CondensedWallObject(string name, string sceneName, string gameObject, string fsmType)
        {
            this.name = name;
            this.sceneName = sceneName;
            this.gameObject = gameObject;
            this.fsmType = fsmType;
        }
    }

    public class ConnectionLogicObject
    {
        public string name;
        public string logicOverride;
        public Dictionary<string, string> logicSubstitutions;
    }
}
