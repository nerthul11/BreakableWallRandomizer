using System;
using System.Collections.Generic;

namespace BreakableWallRandomizer.IC
{
    [Serializable]
    public class WallObject
    {
        public string name;
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
