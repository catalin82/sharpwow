using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX.Direct3D9;
using SlimDX;
using System.Runtime.InteropServices;

namespace SharpWoW.Models
{
    public class SelectionBox
    {
        public SelectionBox()
        {
            mPlaneVertices[0] = new LinePlaneVertex()
            {
                Position = Vector3.Zero,
                U = 0,
                V = 0,
                S = 0,
            };
            mPlaneVertices[1] = new LinePlaneVertex()
            {
                Position = Vector3.Zero,
                U = 0,
                V = 1,
                S = 0,
            };
            mPlaneVertices[2] = new LinePlaneVertex()
            {
                Position = Vector3.Zero,
                U = 1,
                V = 1,
                S = 0,
            };
            mPlaneVertices[3] = new LinePlaneVertex()
            {
                Position = Vector3.Zero,
                U = 1,
                V = 0,
                S = 0,
            };
            mPlaneVertices[4] = new LinePlaneVertex()
            {
                Position = Vector3.Zero,
                U = 0,
                V = 0,
                S = 1,
            };
            mPlaneVertices[5] = new LinePlaneVertex()
            {
                Position = Vector3.Zero,
                U = 0,
                V = 1,
                S = 1,
            };
            mPlaneVertices[6] = new LinePlaneVertex()
            {
                Position = Vector3.Zero,
                U = 1,
                V = 1,
                S = 1,
            };
            mPlaneVertices[7] = new LinePlaneVertex()
            {
                Position = Vector3.Zero,
                U = 1,
                V = 0,
                S = 1,
            };
        }

        public void UpdateSelectionBox(BoundingBox box, Matrix transform)
        {
            mTransform = transform;
            mBox = box;

            var corners = mBox.GetCorners();
            for (int i = 0; i < 8; ++i)
                mPlaneVertices[i].Position = corners[i];

            Video.ShaderCollection.BoxShader.SetValue("matrixWorld", transform);
            mDrawBox = true;
        }

        public void ClearSelectionBox()
        {
            mDrawBox = false;
        }

        public void RenderBox()
        {
            if (mDrawBox == false)
                return;

            var dev = Game.GameManager.GraphicsThread.GraphicsManager.Device;
            var oldCull = dev.GetRenderState<Cull>(RenderState.CullMode);
            dev.SetRenderState(RenderState.CullMode, Cull.None);
            dev.SetRenderState(RenderState.AlphaBlendEnable, true);
            dev.SetRenderState(RenderState.AlphaTestEnable, true);
            dev.SetRenderState(RenderState.AlphaRef, 0.1f);

            dev.VertexFormat = VertexFormat.Position | VertexFormat.Texture2;

            Video.ShaderCollection.BoxShader.DoRender((device) =>
                {
                    device.DrawIndexedUserPrimitives(PrimitiveType.TriangleList, 0, 12, 12, mIndices, Format.Index16, mPlaneVertices, Marshal.SizeOf(typeof(LinePlaneVertex)));
                }
            );

            dev.SetRenderState(RenderState.AlphaTestEnable, false);
            dev.SetRenderState(RenderState.CullMode, oldCull);
        }

        bool mDrawBox = false;
        Line mLine;
        Matrix mTransform;
        BoundingBox mBox;
        LinePlaneVertex[] mPlaneVertices = new LinePlaneVertex[8];
        static short[] mIndices = new short[]
            {
                0, 1, 5,
                5, 4, 0,
                1, 2, 6,
                6, 5, 1,
                2, 3, 7,
                7, 6, 2,
                3, 0, 4,
                4, 7, 3,
                4, 5, 6,
                6, 7, 4,
                0, 1, 2,
                2, 3, 0
            };

        [StructLayout(LayoutKind.Sequential)]
        struct LinePlaneVertex
        {
            public Vector3 Position;
            public float U, V;
            public float S, T;
        }
    }
}
