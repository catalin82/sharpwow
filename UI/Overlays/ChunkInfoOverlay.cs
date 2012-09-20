using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace SharpWoW.UI.Overlays
{
    public class ChunkInfoOverlay : InterfaceOverlay
    {
        public ChunkInfoOverlay()
        {
            mTextElems = new TextElement[]
            {
                new TextElement()
                {
                    Text = "Flags: ",
                    TextColor = Color.Black,
                    Position = new SlimDX.Vector2(50, Game.GameManager.GraphicsThread.GraphicsManager.RenderWindow.ClientSize.Height - 400),
                    FontSize = 20,
                    DrawFont = FontManager.GetFont("Segoe UI")
                },

                new TextElement()
                {
                    Text = "Layer 0: ",
                    TextColor = Color.Black,
                    Position = new SlimDX.Vector2(50, Game.GameManager.GraphicsThread.GraphicsManager.RenderWindow.ClientSize.Height - 375),
                    FontSize = 20,
                    DrawFont = FontManager.GetFont("Segoe UI")
                },

                new TextElement()
                {
                    Text = "Layer 1: ",
                    TextColor = Color.Black,
                    Position = new SlimDX.Vector2(50, Game.GameManager.GraphicsThread.GraphicsManager.RenderWindow.ClientSize.Height - 350),
                    FontSize = 20,
                    DrawFont = FontManager.GetFont("Segoe UI")
                },

                new TextElement()
                {
                    Text = "Layer 2: ",
                    TextColor = Color.Black,
                    Position = new SlimDX.Vector2(50, Game.GameManager.GraphicsThread.GraphicsManager.RenderWindow.ClientSize.Height - 325),
                    FontSize = 20,
                    DrawFont = FontManager.GetFont("Segoe UI")
                },

                new TextElement()
                {
                    Text = "Layer 3: ",
                    TextColor = Color.Black,
                    Position = new SlimDX.Vector2(50, Game.GameManager.GraphicsThread.GraphicsManager.RenderWindow.ClientSize.Height - 300),
                    FontSize = 20,
                    DrawFont = FontManager.GetFont("Segoe UI")
                },            
            };

            mElements.AddRange(mTextElems);
        }

        public override void update()
        {
            if (Game.GameManager.WorldManager.MouseHoverChunk == null)
            {
                mTextElems[0].Text = mTextElems[1].Text = mTextElems[2].Text = mTextElems[3].Text = mTextElems[4].Text = "(no chunk under mouse)";
                return;
            }

            var cnk = Game.GameManager.WorldManager.MouseHoverChunk;
            mTextElems[0].Text = "Flags: 0x" + cnk.Header.flags.ToString("X8");
            int i = 0;
            for ( ; i < cnk.Header.nLayers; ++i)
            {
                mTextElems[i + 1].Text = "Layer " + i + ": 0x" + cnk.getLayer(i).flags.ToString("X8") + " (Texture: " + cnk.getLayerTexture(cnk.getLayer(i));
            }
            for (; i < 4; ++i)
                mTextElems[i + 1].Text = "Layer " + i + ": not set";
        }

        TextElement[] mTextElems;
    }
}
