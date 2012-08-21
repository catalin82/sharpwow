using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX.Direct3D9;
using System.Drawing;

namespace SharpWoW.UI
{
    public abstract class InterfaceOverlay
    {
        protected class TextElement
        {
            public string Text { get; set; }
            public Color TextColor { get; set; }
            public Font DrawFont { get; set; }
            public float FontSize { get; set; }
            public SlimDX.Vector2 Position { get; set; }
        }

        protected Device mDevice;
        protected List<TextElement> mElements = new List<TextElement>();
        protected List<Overlays.BorderElement> mBorderElements = new List<Overlays.BorderElement>();

        public InterfaceOverlay()
        {
            mDevice = Game.GameManager.GraphicsThread.GraphicsManager.Device;
        }

        public void Draw()
        {
            update();

            foreach (var border in mBorderElements)
                border.Draw();

            foreach (var elem in mElements)
            {
                elem.DrawFont.DrawString(elem.Position, elem.Text, elem.TextColor, elem.FontSize);
            }
        }

        public abstract void update();
    }
}
