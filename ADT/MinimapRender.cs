using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX;
using SlimDX.Direct3D9;

namespace SharpWoW.ADT
{
    public class MinimapRender
    {
        public MinimapRender()
        {
            mRenderTarget = new Texture(Game.GameManager.GraphicsThread.GraphicsManager.Device,
                2048, 2048, 1, Usage.RenderTarget, Format.X8R8G8B8, Pool.Default);

            mRenderSurface = mRenderTarget.GetSurfaceLevel(0);
        }

        public void Unload()
        {
            Game.GameManager.GraphicsThread.CallOnThread(() =>
                {
                    mRenderSurface.Dispose();
                    mRenderTarget.Dispose();
                }
            );
        }

        public void CreateMinimap(ADT.IADTFile file)
        {
            string fileName = "";
            if (!gMinimapDir.getMinimapEntry(file.Continent, (int)file.IndexX, (int)file.IndexY, ref fileName))
                return;

            Video.ShaderCollection.TerrainShader.SetValue("minimapMode", true);
            var oldCamera = Game.GameManager.GraphicsThread.GraphicsManager.Camera;
            var oldTarget = Game.GameManager.GraphicsThread.GraphicsManager.Device.GetRenderTarget(0);
            Game.GameManager.GraphicsThread.GraphicsManager.Device.SetRenderTarget(0, mRenderSurface);
            Game.GameManager.GraphicsThread.GraphicsManager.Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, System.Drawing.Color.CornflowerBlue, 1, 0);

            var newCamera = new Video.OrthogonalCamera(1, 1, false);
            newCamera.ViewFrustum.PassAllTests = true;
            newCamera.PreventWorldUpdate = true;
            Game.GameManager.GraphicsThread.GraphicsManager.Camera = newCamera;
            Game.GameManager.GraphicsThread.GraphicsManager.Camera.SetPosition(new Vector3(Utils.Metrics.Tilesize / 2.0f, Utils.Metrics.Tilesize / 2.0f, 1000.0f), true);

            file.RenderADT(Matrix.Translation(-file.IndexX * Utils.Metrics.Tilesize + Utils.Metrics.MidPoint, -file.IndexY * Utils.Metrics.Tilesize + Utils.Metrics.MidPoint, 0));
            Game.GameManager.GraphicsThread.GraphicsManager.Device.SetRenderTarget(0, oldTarget);

            Game.GameManager.WorldManager.FogStart = 530.0f;
            Game.GameManager.GraphicsThread.GraphicsManager.Camera = oldCamera;

            Texture saveTexture = new Texture(mRenderSurface.Device, 256, 256, 1, Usage.None, Format.X8R8G8B8, Pool.Managed);
            Surface surf = saveTexture.GetSurfaceLevel(0);
            Surface.FromSurface(surf, mRenderSurface, Filter.Box, 0);
            surf.Dispose();

            Video.TextureConverter.SaveTextureAsBlp(Video.TextureConverter.BlpCompression.Dxt3, saveTexture, fileName);
            saveTexture.Dispose();
            Video.ShaderCollection.TerrainShader.SetValue("minimapMode", false);
        }

        private Texture mRenderTarget;
        private Surface mRenderSurface;

        private static MinimapDirectory gMinimapDir = new MinimapDirectory();
    }
}
