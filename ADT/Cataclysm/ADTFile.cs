using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpWoW.ADT.Cataclysm
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
            {
                Game.GameManager.ThreadManager.LaunchThread(
                    () => 
                    { 
                        try 
                        { 
                            AsyncLoadProc(); 
                        } 
                        catch (Exception) 
                        { 
                        } 
                    }
                );
            }
            else
                AsyncLoadProc();

            ADTManager.AddADT(this);
        }

        public override Models.WMO.WMOHitInformation GetWmoInfo(uint uniqueId, uint refId)
        {
            throw new NotImplementedException();
        }

        public override void BlurTerrain(SlimDX.Vector3 pos, bool lower)
        {
            foreach (var chunk in mChunks)
                chunk.BlurTerrain(pos, lower);
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

        public override void TextureTerrain(Game.Logic.TextureChangeParam param)
        {
            throw new NotImplementedException();
        }

        public override IADTChunk GetChunk(uint index)
        {
            return mChunks[(int)index];
        }

        public override bool Intersect(SlimDX.Ray ray, ref float height)
        {
            if (loadFinished == false)
                return false;

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
                height = nearHit;

            return hasHit;
        }

        public override void RenderADT(SlimDX.Matrix preTransform)
        {
            if (loadFinished == false)
                return;

            foreach (var chunk in mChunks)
                chunk.Render();
        }

        public override void Unload()
        {
            loadFinished = false;
            ADT.ADTManager.RemoveADT(this);

            Game.GameManager.GraphicsThread.CallOnThread(
                () =>
                {
                    foreach (var tex in mTextures)
                        Video.TextureManager.RemoveTexture(tex);

                    foreach (var chunk in mChunks)
                        chunk.Unload();
                }
            );

            foreach (var chunk in mChunks)
                chunk.AsyncUnload();

            mChunks.Clear();
            mTextures.Clear();

            mpqFile.Close();
            TexStream = null;
        }

        public override void WaitLoad()
        {
            mLoadEvent.WaitOne();
        }

        public override List<string> TextureNames
        {
            get { return mTextureNames; }
        }

        private void AsyncLoadProc()
        {
            mpqFile = new Stormlib.MPQFile(FileName);
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
            loadFinished = true;
        }

        private System.Threading.ManualResetEvent mLoadEvent;
        private Stormlib.MPQFile mpqFile;
        private List<ADTChunk> mChunks = new List<ADTChunk>();
        private bool loadFinished = false;

        private string ReadSignature()
        {
            byte[] bytes = mpqFile.Read(4);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
