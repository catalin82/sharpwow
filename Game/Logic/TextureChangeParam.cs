using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpWoW.Game.Logic
{
    public class TextureChangeParam
    {
        public enum FalloffMode
        {
            Cosinus,
            Linear,
            Quadratic,
            Flat
        }

        public SlimDX.Vector2 ActionSource { get; set; }
        public float InnerRadius { get; set; }
        public float OuterRadius { get; set; }
        public float FalloffTreshold { get; set; }
        public FalloffMode Falloff { get; set; }
        public string TextureName { get; set; }
        public float AlphaCap { get; set; }
        public float Strength { get; set; }

        public void ConvertValuesToUShort()
        {
            FalloffTreshold = (FalloffTreshold / 255.0f) * 65535.0f;
            AlphaCap = (AlphaCap / 255.0f) * 65535.0f;
            Strength = (Strength / 255.0f) * 65535.0f;
        }
    }
}
