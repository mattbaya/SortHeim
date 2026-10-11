using System.Collections.Generic;
using UnityEngine;

namespace SortHeim
{
    internal static class Icons
    {
        private static readonly Dictionary<string, Sprite> Embedded = new Dictionary<string, Sprite>();

        /// <summary>The game's own "Encumbered" status-effect icon, or null if ObjectDB isn't loaded yet.</summary>
        public static Sprite Encumbered
        {
            get
            {
                var db = ObjectDB.instance;
                if (db == null) return null;
                var effect = db.GetStatusEffect(SEMan.s_statusEffectEncumbered);
                return effect != null ? effect.m_icon : null;
            }
        }

        /// <summary>Fallback scale icon embedded in the DLL (see Resources/).</summary>
        public static Sprite Scale => LoadEmbedded("scale.png");

        /// <summary>Gem icon embedded in the DLL (see Resources/).</summary>
        public static Sprite Gem => LoadEmbedded("gem.png");

        private static Sprite LoadEmbedded(string name)
        {
            if (Embedded.TryGetValue(name, out var cached) && cached != null) return cached;

            using (var stream = typeof(Icons).Assembly.GetManifestResourceStream("SortHeim." + name))
            {
                if (stream == null)
                {
                    SortHeimPlugin.Log.LogWarning($"Missing embedded icon {name}");
                    return null;
                }

                var bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);

                var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                texture.LoadImage(bytes);
                texture.filterMode = FilterMode.Bilinear;
                var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                Embedded[name] = sprite;
                return sprite;
            }
        }
    }
}
