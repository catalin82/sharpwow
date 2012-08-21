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
            mModelName = result.Model.ModelPath;
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
                    TextColor = System.Drawing.Color.LightGreen
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
                Position = new SlimDX.Vector2(45, 73),
                Size = new SlimDX.Vector2(150, 24),
                Opacity = 200
            };

            BorderElement nameBorder = new BorderElement()
            {
                Position = new SlimDX.Vector2(45, 50),
                Size = new SlimDX.Vector2(mTextElements[0].DrawFont.MeasureString(new SlimDX.Vector2(45, 47), mTextElements[0].Text, mTextElements[0].FontSize).X + 10, 23),
                Opacity = 200
            };

            mBorderElements.Add(colorBorder);
            mBorderElements.Add(nameBorder);
        }

        public void UpdateModel(Models.MDX.MdxIntersectionResult result)
        {
            mModelName = result.Model.ModelPath;
            mTextElements[0].Text = "Selected model: " + mModelName;
            mBorderElements[1].Size = new SlimDX.Vector2(mTextElements[0].DrawFont.MeasureString(new SlimDX.Vector2(45, 47), mTextElements[0].Text, mTextElements[0].FontSize).X + 10, 23);
        }

        public override void update()
        {
        }

        private TextElement[] mTextElements;
        private string mModelName;
    }
}
