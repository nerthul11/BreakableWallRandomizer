using ItemChanger.Locations;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BreakableWallRandomizer.IC
{
    [Serializable]
    public class AbstractWallLocation : AutoLocation
    {
        public string objectName;
        public string fsmType;
        public List<string> alsoDestroy;
        public bool exit;
        public List<CondensedWallObject> groupWalls;
        protected override void OnLoad() {}

        protected override void OnUnload() {}

        public void MakeWallPassable(GameObject go, bool destroy)
        {
            foreach (var objectName in alsoDestroy)
            {
                try
                {
                    var obj = GameObject.Find(objectName);
                    GameObject.Destroy(obj);
                } catch 
                { 
                    BreakableWallRandomizer.Instance.LogWarn($"{objectName} not found.");
                }
            }
            MakeChildrenPassable(go, destroy);
        }

        // Recursively set all colliders as triggers on a given gameObject.
        // Also recursively set any SpriteRenderers on a given gameObject to 0.5 alpha.
        // Also remove any object called "Camera lock" or any textures beginning with msk_. 
        private void MakeChildrenPassable(GameObject go, bool destroy)
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
            if (go.name.Contains("Camera") || go.name.Contains("Mask"))
            {
                GameObject.Destroy(go);
            }

            for (var i = 0; i < go.transform.childCount; i++)
            {
                MakeWallPassable(go.transform.GetChild(i).gameObject, destroy);
                if (destroy)
                    GameObject.Destroy(go);
            }
        }
    }
}