using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX.Direct3D9;
using SlimDX;

namespace SharpWoW.World
{
    public class SkySphere
    {
        private Mesh mSphere = null;

        public SkySphere()
        {
            mSphere = Mesh.CreateSphere(Game.GameManager.GraphicsThread.GraphicsManager.Device, 600.0f, 100, 100);
        }

        public void DrawSky()
        {
            var dev = Game.GameManager.GraphicsThread.GraphicsManager.Device;
            var matrix = Matrix.Translation(Game.GameManager.GraphicsThread.GraphicsManager.Camera.Position);
            Video.ShaderCollection.SkyShader.SetValue("matWorld", matrix);

            Video.ShaderCollection.SkyShader.DoRender((device) =>
            {
                mSphere.DrawSubset(0);
            }
            );
        }
    }
}
