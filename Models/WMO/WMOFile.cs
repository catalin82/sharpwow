using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace SharpWoW.Models.WMO
{
    public class WMOFile
    {
        public WMOFile(string fileName, Video.TextureManager customTexMgr = null)
        {
            mCustomTextureMgr = customTexMgr;
            if (mCustomTextureMgr == null)
                mCustomTextureMgr = Video.TextureManager.Default;
            FileName = fileName;
            System.Threading.Thread t = new System.Threading.Thread(AsyncLoadProc);
            t.Start();
        }

        private void AsyncLoadProc()
        {
            mFile = new Stormlib.MPQFile(FileName);
            SeekChunk("DHOM");
            mFile.Position += 4;
            mHeader = mFile.Read<MOHD>();

            BoundingBox = new SlimDX.BoundingBox(new SlimDX.Vector3(mHeader.MinPosition.X, mHeader.MinPosition.Z, mHeader.MinPosition.Y),
                new SlimDX.Vector3(mHeader.MaxPosition.X, mHeader.MaxPosition.Z, mHeader.MaxPosition.Y));

            SeekChunk("XTOM");
            uint numBytes = mFile.Read<uint>();
            byte[] texData = mFile.Read(numBytes);
            var fullStr = Encoding.UTF8.GetString(texData);
            var textures = fullStr.Split('\0');

            uint curPos = 0;
            foreach (var tex in textures)
            {
                if (tex != "")
                    mTextureNames.Add(curPos, tex);

                curPos += (uint)tex.Length + 1;
            }

            SeekChunk("TMOM");
            mFile.Position += 4;

            for (uint i = 0; i < mHeader.nMaterials; ++i)
            {
                MOMT mat = mFile.Read<MOMT>();
                mMaterials.Add(mat);
            }

            Game.GameManager.GraphicsThread.CallOnThread(
                () =>
                {
                    foreach (var mat in mMaterials)
                    {
                        if (mCustomTextureMgr == null)
                            mTextures.Add(Video.TextureManager.GetTexture(mTextureNames[mat.ofsTexture1]));
                        else
                            mTextures.Add(mCustomTextureMgr.LoadTexture(mTextureNames[mat.ofsTexture1]));

                    }
                }
            );

            SeekChunk("IGOM");
            mFile.Position += 4;

            for (uint i = 0; i < mHeader.nGroups; ++i)
            {
                MOGI mogi = mFile.Read<MOGI>();
                mGroupInfos.Add(mogi);

                WMOGroup group = new WMOGroup(FileName, i, this);
                if (group.LoadGroup())
                {
                    lock (mGroups) mGroups.Add(group);
                }
            }

            isLoadFinished = true;
        }

        public bool Intersects(SlimDX.Ray ray, out float nearHit)
        {
            nearHit = 0;
            float curNear = 99999;
            bool hasHit = false;
            foreach (var group in mGroups)
            {
                float curHit = 0;
                if (group.Intersects(ray, out curHit))
                {
                    hasHit = true;
                    if (curHit < curNear)
                        curNear = curHit;
                }
            }

            nearHit = curNear;
            return hasHit;
        }

        public void Draw(SlimDX.Matrix transform, bool noShader = false)
        {
            lock (mGroups)
            {
                foreach (var group in mGroups)
                    group.RenderGroup(transform, noShader);
            }
        }

        public void SaveFile()
        {
            var filePath = Game.GameManager.SavePath + FileName;
            var strm = System.IO.File.OpenWrite(filePath);
            mFile.Position = 0;
            using (BinaryWriter wr = new BinaryWriter(strm))
            {
                while (true)
                {
                    string chunk = "";
                    try
                    {
                        chunk = GetChunk();
                    }
                    catch (Exception)
                    {
                        break;
                    }

                    if (chunk == "MOHD")
                    {
                        wr.Write(0x4D4F4844);
                        wr.Write(Marshal.SizeOf(mHeader));
                        wr.WriteStruct(mHeader);
                        SkipChunk();
                        continue;
                    }

                    if (chunk == "MOTX")
                    {
                        wr.Write(0x4D4F5458);
                        List<byte> textureData = new List<byte>();
                        foreach (var texture in TextureNames)
                        {
                            var bytes = Encoding.UTF8.GetBytes(texture);
                            textureData.AddRange(bytes);
                            var len = bytes.Length % 4;
                            if (len != 0)
                                textureData.AddRange(new byte[4 - len]);
                        }

                        wr.Write(textureData.Count);
                        wr.Write(textureData.ToArray());
                        SkipChunk();
                        continue;
                    }
                }
            }

            strm.Close();
        }

        private void SkipChunk()
        {
            uint size = mFile.Read<uint>();
            mFile.Position += size;
        }

        private void SeekChunk(string id)
        {
            mFile.Position = 0;
            while (GetChunk() != id)
            {
                uint size = mFile.Read<uint>();
                mFile.Position += size;
            }
        }

        private string GetChunk()
        {
            byte[] sig = mFile.Read(4);
            var str = Encoding.UTF8.GetString(sig);
            return str;
        }

        public Video.TextureHandle GetTexture(uint index) { return mTextures[(int)index]; }
        public MOMT GetMaterial(uint index) { return mMaterials[(int)index]; }

        public string FileName { get; private set; }

        private MOHD mHeader;
        private bool isLoadFinished = false;
        private Stormlib.MPQFile mFile;
        private Dictionary<uint, string> mTextureNames = new Dictionary<uint, string>();
        private List<MOMT> mMaterials = new List<MOMT>();
        private List<MOGI> mGroupInfos = new List<MOGI>();
        private List<WMOGroup> mGroups = new List<WMOGroup>();
        private List<Video.TextureHandle> mTextures = new List<Video.TextureHandle>();
        private Video.TextureManager mCustomTextureMgr = null;

        public Video.TextureManager TextureManager { get { return mCustomTextureMgr; } }
        public SlimDX.BoundingBox BoundingBox { get; set; }
        public float BoundingRadius { get { return (BoundingBox.Maximum - BoundingBox.Minimum).Length(); } }
        public SlimDX.Vector3 Center { get { return (BoundingBox.Minimum + (BoundingBox.Maximum - BoundingBox.Minimum) / 2.0f); } }
        public string[] TextureNames { get { return mTextureNames.Values.ToArray(); } }
        public MOMT[] Materials { get { return mMaterials.ToArray(); } }

        public void SetTextures(string[] names)
        {
            foreach (var handle in mTextures)
                TextureManager.UnloadTexture(handle);

            mTextures.Clear();
            mTextureNames.Clear();
            uint curOfs = 0;

            foreach (var str in names)
            {
                mTextures.Add(TextureManager.LoadTexture(str));
                mTextureNames.Add(curOfs, str);
                curOfs += (uint)str.Length + 1;
            }

            for (int i = 0; i < mMaterials.Count; ++i)
            {
                if (mTextureNames.ContainsKey(mMaterials[i].ofsTexture1) == false)
                {
                    var diffs = from pos in mTextureNames.Keys
                                select (int)mMaterials[i].ofsTexture1 - (int)pos;

                    var mat = mMaterials[i];
                    mat.ofsTexture1 = (uint)((int)mat.ofsTexture1 + diffs.Min());
                    mMaterials[i] = mat;
                }
            }
        }

        public bool LoadFinished { get { return isLoadFinished; } }
    }
}
