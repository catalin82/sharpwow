using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace SharpWoW.UI
{
    public class TerrainInfoOverlay : InterfaceOverlay
    {
        public TerrainInfoOverlay()
        {
            mTextElements = new TextElement[]
            {
                new TextElement()
                {
                    Text = "Radius: ",
                    TextColor = Color.Black,
                    Position = new SlimDX.Vector2(50, Game.GameManager.GraphicsThread.GraphicsManager.RenderWindow.ClientSize.Height - 50),
                    FontSize = 20,
                    DrawFont = FontManager.GetFont("Segoe UI")
                },

                new TextElement()
                {
                    Text = "Intensity: ",
                    TextColor = Color.Black,
                    Position = new SlimDX.Vector2(50, Game.GameManager.GraphicsThread.GraphicsManager.RenderWindow.ClientSize.Height - 30),
                    FontSize = 20,
                    DrawFont = FontManager.GetFont("Segoe UI")
                },

                new TextElement()
                {
                    Text = "Changing height",
                    TextColor = Color.Red,
                    Position = new SlimDX.Vector2(Game.GameManager.GraphicsThread.GraphicsManager.RenderWindow.ClientSize.Width - 170, 10),
                    FontSize = 25,
                    DrawFont = FontManager.GetFont("Segoe UI")
                },
            };

            mElements.AddRange(mTextElements);
        }

        public override void update()
        {
            mTextElements[0].Text = "Radius: " + Game.GameManager.TerrainLogic.Radius.ToString("F2") + " (Increase: R, Decrease: T)";
            mTextElements[1].Text = "Intensity: " + Game.GameManager.TerrainLogic.Intensity.ToString("F2") + " (Increase: C, Decrease: V)";
        }

        TextElement[] mTextElements;
    }
}
