using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace SharpWoW.UI
{
    public class TextureInfoOverlay : InterfaceOverlay
    {
        public TextureInfoOverlay()
        {
            mTextElems = new TextElement[]
            {
                new TextElement()
                {
                    Text = "Texture: ",
                    TextColor = Color.Black,
                    Position = new SlimDX.Vector2(50, Game.GameManager.GraphicsThread.GraphicsManager.RenderWindow.ClientSize.Height - 50),
                    FontSize = 20,
                    DrawFont = FontManager.GetFont("Segoe UI")
                },

                new TextElement()
                {
                    Text = "Strength: ",
                    TextColor = Color.Black,
                    Position = new SlimDX.Vector2(50, Game.GameManager.GraphicsThread.GraphicsManager.RenderWindow.ClientSize.Height - 30),
                    FontSize = 20,
                    DrawFont = FontManager.GetFont("Segoe UI")
                },

                new TextElement()
                {
                    Text = "Cap: ",
                    TextColor = Color.Black,
                    Position = new SlimDX.Vector2(165, Game.GameManager.GraphicsThread.GraphicsManager.RenderWindow.ClientSize.Height - 30),
                    FontSize = 20,
                    DrawFont = FontManager.GetFont("Segoe UI")
                },

                new TextElement()
                {
                    Text = "Falloff: ",
                    TextColor = Color.Black,
                    Position = new SlimDX.Vector2(258, Game.GameManager.GraphicsThread.GraphicsManager.RenderWindow.ClientSize.Height - 30),
                    FontSize = 20,
                    DrawFont = FontManager.GetFont("Segoe UI")
                },

                new TextElement()
                {
                    Text = "Falloff mode: ",
                    TextColor = Color.Black,
                    Position = new SlimDX.Vector2(350, Game.GameManager.GraphicsThread.GraphicsManager.RenderWindow.ClientSize.Height - 30),
                    FontSize = 20,
                    DrawFont = FontManager.GetFont("Segoe UI")
                },

                new TextElement()
                {
                    Text = "Changing texture",
                    TextColor = Color.Red,
                    Position = new SlimDX.Vector2(Game.GameManager.GraphicsThread.GraphicsManager.RenderWindow.ClientSize.Width - 170, 10),
                    FontSize = 25,
                    DrawFont = FontManager.GetFont("Segoe UI")
                },
            };

            mElements.AddRange(mTextElems);
        }

        public override void update()
        {
            mTextElems[0].Text = "Texture: " + Game.GameManager.GameWindow.ToolsPanel.SelectedTexture;
            mTextElems[1].Text = "Strength: " + Game.GameManager.GameWindow.ToolsPanel.TextureStrength.ToString("F2") + " | ";
            mTextElems[2].Text = "Cap: " + Game.GameManager.GameWindow.ToolsPanel.TextureAlphaCap.ToString("F2") + " | ";
            mTextElems[3].Text = "Falloff: " + Game.GameManager.GameWindow.ToolsPanel.TextureFalloff.ToString("F2") + " | ";
            mTextElems[4].Text = "Falloff mode: " + Game.GameManager.GameWindow.ToolsPanel.TextureFallofMode;
        }

        private TextElement[] mTextElems;
    }
}
