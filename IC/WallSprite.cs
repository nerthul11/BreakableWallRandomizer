using ItemChanger;
using ItemChanger.Internal;
using Newtonsoft.Json;
using System;
using UnityEngine;

namespace BreakableWallRandomizer.IC
{
    [Serializable]
    public class WallSprite : ISprite
    {
        private static SpriteManager EmbeddedSpriteManager = new(typeof(WallSprite).Assembly, "BreakableWallRandomizer.Resources.Sprites.");
        public string Key { get; set; }
        public WallSprite(string key)
        {
            if (!string.IsNullOrEmpty(key))
                Key = key;
        }

        [JsonIgnore]
        public Sprite Value => EmbeddedSpriteManager.GetSprite(Key);
        public ISprite Clone() => (ISprite)MemberwiseClone();
    }
}