using System.IO;
using UnityEngine;

namespace ChestSort
{
    internal static class Icons
    {
        private static Sprite _scale;

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
        public static Sprite Scale => _scale != null ? _scale : (_scale = Load("scale.png"));

        private static Sprite Load(string name)
        {
            using (var stream = typeof(Icons).Assembly.GetManifestResourceStream("ChestSort." + name))
            {
                if (stream == null)
                {
                    ChestSortPlugin.Log.LogWarning($"Missing embedded icon {name}");
                    return null;
                }

                var bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);

                var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                texture.LoadImage(bytes);
                texture.filterMode = FilterMode.Bilinear;
                return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            }
        }
    }
}
