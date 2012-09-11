using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpWoW.ADT.Wotlk
{
    public partial class ADTFile : IADTFile
    {
        public ADTFile(string fileName, uint indexX, uint indexY, bool initial = false)
        {
            IndexX = indexX;
            IndexY = indexY;
            FileName = fileName;
            mLoadEvent = new System.Threading.ManualResetEvent(false);
            if (initial == false)
                Game.GameManager.ThreadManager.LaunchThread(AsyncLoadProc);
            else
                AsyncLoadProc();

            ADTManager.AddADT(this);
        }

        public override void RenderADT(SlimDX.Matrix preTransform)
        { 
            lock (mChunks)
            {
                foreach (var chunk in mChunks)
                {
                    chunk.Render(preTransform);
                }
            }
        }

        public bool IsLoaded()
        {
            return mChunks.Count == 256;
        }

        public override void Unload()
        {
            ADTManager.RemoveADT(this);
            Game.GameManager.ThreadManager.LaunchThread(() =>
                {
                    mLoadEvent.WaitOne();
                    foreach (var cnk in mChunks)
                        cnk.Unload();
                    mChunks.Clear();
                    mChunks = null;
                    mTextureNames = null;
                    foreach (var tex in mTextures)
                        Video.TextureManager.RemoveTexture(tex);
                    mTextures.Clear();
                    mTextures = null;
                    mOffsets = null;
                    mpqFile = null;
                }
            );
        }

        public override Models.WMO.WMOHitInformation GetWmoInfo(uint uniqueId, uint refId)
        {
            foreach (var plc in WMODefinitions)
            {
                if (plc.uniqueId == uniqueId)
                {
                    string name = WMONames[WMOIdentifiers[(int)plc.idMWID]];
                    return new Models.WMO.WMOHitInformation()
                    {
                        Name = name,
                        Placement = plc
                    };
                }
            }

            return null;
        }

        public override IADTChunk GetChunk(uint index)
        {
            if (index < mChunks.Count)
                return mChunks[(int)index];

            throw new IndexOutOfRangeException();
        }

        public override bool Intersect(SlimDX.Ray ray, ref float dist)
        {
            bool hasHit = false;
            float nearHit = 99999999;
            foreach (var chunk in mChunks)
            {
                float curHit = 0;
                if (chunk.Intersect(ray, ref curHit))
                {
                    hasHit = true;
                    if (curHit < nearHit)
                        nearHit = curHit;
                }
            }

            if (hasHit)
                dist = nearHit;

            return hasHit;
        }

        public override void ChangeTerrain(SlimDX.Vector3 pos, bool lower)
        {
            foreach (var chunk in mChunks)
                chunk.ChangeTerrain(pos, lower);
        }

        public override void FlattenTerrain(SlimDX.Vector3 pos, bool lower)
        {
            foreach (var chunk in mChunks)
                chunk.FlattenTerrain(pos, lower);
        }

        public override void BlurTerrain(SlimDX.Vector3 pos, bool lower)
        {
            foreach (var chunk in mChunks)
                chunk.BlurTerrain(pos, lower);
        }

        public override void TextureTerrain(Game.Logic.TextureChangeParam param)
        {
            foreach (var chunk in mChunks)
                chunk.textureTerrain(param);
        }

        private void AsyncLoadProc()
        {
            try
            {
                mpqFile = new Stormlib.MPQFile(FileName);
            }
            catch (System.IO.FileNotFoundException)
            {
                mLoadEvent.Set();
                return;
            }

            if (ReadSignature() != "REVM")
                return;

            uint size = mpqFile.Read<uint>();
            if (size != 4)
                return;

            uint version = mpqFile.Read<uint>();
            if (version != 18)
                return;

            LoadAsyncData();
            mLoadEvent.Set();
        }

        public override void WaitLoad()
        {
            mLoadEvent.WaitOne();
        }

        private System.Threading.ManualResetEvent mLoadEvent;
        private List<ADTChunk> mChunks = new List<ADTChunk>();
        private Stormlib.MPQFile mpqFile;
        private string ReadSignature()
        {
            byte[] bytes = mpqFile.Read(4);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
