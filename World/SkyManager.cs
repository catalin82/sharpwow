using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX;

namespace SharpWoW.World
{
    public class SkyManager
    {
        public SkyManager()
        {
            try
            {
                foreach (var map in DBC.DBCStores.Map.Records)
                {
                    mSkyMapper.Add(map.ID, new MapSky(map.ID));
                }
            }
            catch (Exception)
            {
            }

            mSkyTexture = new SlimDX.Direct3D9.Texture(Game.GameManager.GraphicsThread.GraphicsManager.Device, 180, 1, 1, SlimDX.Direct3D9.Usage.None, SlimDX.Direct3D9.Format.A8R8G8B8, SlimDX.Direct3D9.Pool.Managed);
            Game.GameManager.GraphicsThread.OnFrame += new Game.VideoThread.FrameRenderDelegate(_RenderSky);
        }

        void _RenderSky(SlimDX.Direct3D9.Device device, TimeSpan deltaTime)
        {
            if (Game.GameManager.IsPandaria == true)
                return;

            if (Game.GameManager.WorldManager.IsInWorld == false)
                return;

            var mapid = Game.GameManager.WorldManager.MapID;
            Update(mapid, Game.GameManager.GraphicsThread.GraphicsManager.Camera.Position);
            var cbott = mSkyMapper[mapid].GetColorEntry(ColorTableValues.Fog);
            var choriz = mSkyMapper[mapid].GetColorEntry(ColorTableValues.Color4);
            var cahoriz = mSkyMapper[mapid].GetColorEntry(ColorTableValues.Color3);
            var cmihori = mSkyMapper[mapid].GetColorEntry(ColorTableValues.Color2);
            var cmi = mSkyMapper[mapid].GetColorEntry(ColorTableValues.Color1);
            var ctop = mSkyMapper[mapid].GetColorEntry(ColorTableValues.Color0);
            uint[] texValues = new uint[180];
            for (uint i = 0; i < 60; ++i)
                texValues[i] = ToUInt(cbott);
            for (uint i = 60; i < 90; ++i)
            {
                float sat = (i - 60) / 30.0f;
                var clr = cbott + sat * (choriz - cbott);
                texValues[i] = ToUInt(clr);
            }
            for (uint i = 90; i < 95; ++i)
            {
                float sat = (i - 90) / 5.0f;
                var clr = choriz + sat * (cahoriz - choriz);
                texValues[i] = ToUInt(clr);
            }
            for (uint i = 95; i < 105; ++i)
            {
                float sat = (i - 95) / 10.0f;
                var clr = cahoriz + sat * (cmihori - cahoriz);
                texValues[i] = ToUInt(clr);
            }
            for (uint i = 105; i < 120; ++i)
            {
                float sat = (i - 105) / 15.0f;
                var clr = cmihori + sat * (cmi - cmihori);
                texValues[i] = ToUInt(clr);
            }
            for (uint i = 120; i < 180; ++i)
            {
                float sat = (i - 120) / 60.0f;
                var clr = cmi + sat * (ctop - cmi);
                texValues[i] = ToUInt(clr);
            }

            var rec = mSkyTexture.LockRectangle(0, SlimDX.Direct3D9.LockFlags.None);
            rec.Data.WriteRange(texValues);
            mSkyTexture.UnlockRectangle(0);

            Video.ShaderCollection.SkyShader.SetTexture("skyTexture", mSkyTexture);
            Video.ShaderCollection.TerrainShader.SetValue("diffuseLight", mSkyMapper[mapid].GetColorEntry(ColorTableValues.GlobalDiffuse));
            Video.ShaderCollection.MDXShader.SetValue("diffuseLight", mSkyMapper[mapid].GetColorEntry(ColorTableValues.GlobalDiffuse));
            Video.ShaderCollection.WMOShader.SetValue("diffuseLight", mSkyMapper[mapid].GetColorEntry(ColorTableValues.GlobalDiffuse));
            Video.ShaderCollection.TerrainShader.SetValue("ambientLight", mSkyMapper[mapid].GetColorEntry(ColorTableValues.GlobalAmbient));
            Video.ShaderCollection.MDXShader.SetValue("ambientLight", mSkyMapper[mapid].GetColorEntry(ColorTableValues.GlobalAmbient));
            Video.ShaderCollection.WMOShader.SetValue("ambientLight", mSkyMapper[mapid].GetColorEntry(ColorTableValues.GlobalAmbient));
            mSphere.DrawSky();

        }

        private uint ToUInt(Vector3 color)
        {
            return (uint)((0xFF000000) | (((uint)(color.X * 255.0f)) << 16) | (((uint)(color.Y * 255.0f)) << 8) | (uint)(color.Z * 255.0f));
        }

        public void Update(uint mapid, SlimDX.Vector3 position)
        {
            if (Game.GameManager.IsPandaria)
                return;

            position.X = Utils.Metrics.MidPoint + position.X;
            position.Y = Utils.Metrics.MidPoint + position.Y;
            if (mSkyMapper.ContainsKey(mapid))
            {
                mSkyMapper[mapid].UpdateSky(position);
                Video.ShaderCollection.TerrainShader.SetValue("fogColor", new Vector4(mSkyMapper[mapid].GetColorEntry(ColorTableValues.Fog), 1.0f));
                Video.ShaderCollection.MDXShader.SetValue("fogColor", new Vector4(mSkyMapper[mapid].GetColorEntry(ColorTableValues.Fog), 1.0f));
                Video.ShaderCollection.WMOShader.SetValue("fogColor", new Vector4(mSkyMapper[mapid].GetColorEntry(ColorTableValues.Fog), 1.0f));
            }
        }

        private Dictionary<uint, MapSky> mSkyMapper = new Dictionary<uint, MapSky>();
        private SkySphere mSphere = new SkySphere();
        private SlimDX.Direct3D9.Texture mSkyTexture;

        public MapSky GetSkyForMap(uint mapid) { return mSkyMapper[mapid]; }
    }
}
