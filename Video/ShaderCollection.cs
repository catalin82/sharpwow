using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpWoW.Video
{
    public static class ShaderCollection
    {
        public static Shader TerrainShader { get; private set; }
        public static Shader SkyShader { get; private set; }
        public static Shader MDXShader { get; private set; }

        static ShaderCollection()
        {
            TerrainShader = ShaderManager.Shaders.GetShader("TerrainShader");
            TerrainShader.SetTechnique(0);
            SkyShader = ShaderManager.Shaders.GetShader("SkySphere");
            SkyShader.SetTechnique(0);
            MDXShader = ShaderManager.Shaders.GetShader("MDXShader");
            MDXShader.SetTechnique(0);
        }

        public static void CameraChanged(Video.Camera cam)
        {
            TerrainShader.SetValue("matrixViewProj", cam.ViewProj);
            TerrainShader.SetValue("CameraPosition", cam.Position);
            SkyShader.SetValue("matrixViewProj", cam.ViewProj);
            MDXShader.SetValue("matrixViewProj", cam.ViewProj);
            MDXShader.SetValue("CameraPosition", cam.Position);
        }

        public static void UpdateTime(TimeSpan time)
        {
            TerrainShader.SetValue("gameTime", (float)time.TotalSeconds);
        }
    }
}
