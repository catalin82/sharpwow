using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpWoW.Models.WMO
{
    public class WMOFile
    {
        public WMOFile(string fileName)
        {
            FileName = fileName;
            //System.Threading.Thread t = new System.Threading.Thread(AsyncLoadProc);
            //t.Start();
            AsyncLoadProc();
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
                        mTextures.Add(Video.TextureManager.GetTexture(mTextureNames[mat.ofsTexture1]));
                        mDebugTextures.Add(mTextureNames[mat.ofsTexture1]);
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
                    mGroups.Add(group);
            }

            isLoadFinished = true;
        }

        public void Draw(SlimDX.Matrix transform)
        {
            foreach (var group in mGroups)
                group.RenderGroup(transform);
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
        public MOMT DEBUG_GetMaterial(uint index) { return mMaterials[(int)index]; }
        public string DEBUG_GetTextureName(uint index) { return mDebugTextures[(int)index]; }

        public string FileName { get; private set; }

        private MOHD mHeader;
        private bool isLoadFinished = false;
        private Stormlib.MPQFile mFile;
        private Dictionary<uint, string> mTextureNames = new Dictionary<uint, string>();
        private List<MOMT> mMaterials = new List<MOMT>();
        private List<MOGI> mGroupInfos = new List<MOGI>();
        private List<WMOGroup> mGroups = new List<WMOGroup>();
        private List<Video.TextureHandle> mTextures = new List<Video.TextureHandle>();
        private List<string> mDebugTextures = new List<string>();

        public SlimDX.BoundingBox BoundingBox { get; set; }

        public bool LoadFinished { get { return isLoadFinished; } }
    }
}
