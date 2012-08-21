using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpWoW.UI.Overlays
{
    public class ModelInfoOverlay : UI.InterfaceOverlay
    {
        public ModelInfoOverlay(Models.MDX.MdxIntersectionResult result)
        {
            mModelName = result.Model.FileDirectory;
            initOverlay();
        }

        void initOverlay()
        {
            var font = FontManager.GetFont("Segoe UI");
            mTextElements = new TextElement[]
            {
                new TextElement()
                {
                    DrawFont = font,
                    FontSize = 20,
                    Text = "Selected model: " + mModelName,
                    Position = new SlimDX.Vector2(50, 50),
                    TextColor = System.Drawing.Color.Black
                },

                new TextElement()
                {
                    DrawFont = font,
                    FontSize = 20,
                    Text = "Left",
                    Position = new SlimDX.Vector2(50, 75),
                    TextColor = System.Drawing.Color.Blue
                },

                new TextElement()
                {
                    DrawFont = font,
                    FontSize = 20,
                    Text = "Middle",
                    Position = new SlimDX.Vector2(90, 75),
                    TextColor = System.Drawing.Color.Green
                },

                new TextElement()
                {
                    DrawFont = font,
                    FontSize = 20,
                    Text = "Right",
                    Position = new SlimDX.Vector2(150, 75),
                    TextColor = System.Drawing.Color.Red
                },
            };

            mElements.AddRange(mTextElements);

            BorderElement colorBorder = new BorderElement()
            {
                Position = new SlimDX.Vector2(45, 70),
                Size = new SlimDX.Vector2(150, 30),
                Opacity = 200
            };

            mBorderElements.Add(colorBorder);
        }

        public void UpdateModel(Models.MDX.MdxIntersectionResult result)
        {
            mModelName = result.Model.FileDirectory;
            mTextElements[0].Text = "Selected model: " + mModelName;
        }

        public override void update()
        {
        }

        private TextElement[] mTextElements;
        private string mModelName;
    }
}
