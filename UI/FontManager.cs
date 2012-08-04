using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;
using SlimDX.Direct3D9;

namespace SharpWoW.UI
{
    public static class FontManager
    {
        private static Dictionary<string, Font> mFonts = new Dictionary<string, Font>();
        private static Sprite mSprite;

        public static void init()
        {
            mSprite = new Sprite(Game.GameManager.GraphicsThread.GraphicsManager.Device);
        }

        public static void beginFrame()
        {
            mSprite.Begin(SpriteFlags.AlphaBlend);
        }

        public static void endFrame()
        {
            mSprite.End();
        }

        public static Font GetFont(string face)
        {
            if (mFonts.ContainsKey(face.ToLower()))
                return mFonts[face.ToLower()];

            Font font = new Font(face);
            mFonts.Add(face.ToLower(), font);

            return font;
        }

        public static Sprite Sprite { get { return mSprite; } }
    }
}
