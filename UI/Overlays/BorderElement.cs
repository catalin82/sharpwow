using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;
using SlimDX.Direct3D9;
using System.Runtime.InteropServices;

namespace SharpWoW.UI.Overlays
{
    public class BorderElement
    {
        public Vector2 Position { get; set; }
        public Vector2 Size { get; set; }
        public byte Opacity { get { return DrawColor.A; } set { DrawColor = System.Drawing.Color.FromArgb(value, DrawColor); } }

        public void Draw()
        {
            int color = DrawColor.ToArgb();
            VertexPositionColor[] vertices = new VertexPositionColor[]
            {
                new VertexPositionColor()
                {
                    Position = new Vector4(Position, 0, 0),
                    Color = color
                },
                new VertexPositionColor()
                {
                    Position = new Vector4(Position.X + Size.X, Position.Y, 0, 0),
                    Color = color
                },
                new VertexPositionColor()
                {
                    Position = new Vector4(Position.X + Size.X, Position.Y + Size.Y, 0, 0),
                    Color = color
                },
                new VertexPositionColor()
                {
                    Position = new Vector4(Position.X, Position.Y + Size.Y, 0, 0),
                    Color = color
                },
            };

            var dev = Game.GameManager.GraphicsThread.GraphicsManager.Device;
            dev.SetRenderState(RenderState.AlphaBlendEnable, true);
            dev.VertexFormat = VertexPositionColor.FVF;
            dev.DrawUserPrimitives(PrimitiveType.TriangleFan, 2, vertices);
        }

        private System.Drawing.Color DrawColor = System.Drawing.Color.DarkGray;

        [StructLayout(LayoutKind.Sequential)]
        private struct VertexPositionColor
        {
            public Vector4 Position;
            public int Color;

            public const VertexFormat FVF = VertexFormat.Diffuse | VertexFormat.PositionRhw;
            public const int Stride = 20;
        };
    }
}
