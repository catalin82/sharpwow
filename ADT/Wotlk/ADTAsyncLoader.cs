using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpWoW.ADT.Wotlk
{
    public partial class ADTFile
    {
        private void LoadAsyncData()
        {
            if (ReadSignature() != "RDHM")
                return;

            uint size = mpqFile.Read<uint>();
            if (size < MHDR.Size)
            {
                mLoadEvent.Set();
                return;
            }

            mHeader = mpqFile.Read<MHDR>();
            mpqFile.Position = 0x14 + mHeader.ofsMcin;
            if (ReadSignature() != "NICM")
            {
                return;
            }

            size = mpqFile.Read<uint>();
            if (size != 16 * 256)
            {
                return;
            }

            for (uint i = 0; i < 256; ++i)
            {
                mOffsets[i] = mpqFile.Read<MCIN>();
            }

            mpqFile.Position = 0x14 + mHeader.ofsMtex;
            if (ReadSignature() != "XETM")
            {
                return;
            }

            size = mpqFile.Read<uint>();
            byte[] strData = mpqFile.Read(size);
            string str = Encoding.UTF8.GetString(strData);
            mTextureNames = str.Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);

            Game.GameManager.GraphicsThread.CallOnThread(
                () =>
                {
                    foreach (var tex in mTextureNames)
                    {
                        if (tex.Length > 0)
                            mTextures.Add(Video.TextureManager.GetTexture(tex));
                    }
                }
            );

            mpqFile.Position = 0x14 + mHeader.ofsMmdx + 0x04;
            size = mpqFile.Read<uint>();
            byte[] data = mpqFile.Read((uint)size);
            string fullStr = Encoding.UTF8.GetString(data);
            string[] Names = fullStr.Split('\0');
            var qry = (from string n in Names where n != "" select n);
            uint ofs = 0;
            foreach (var s in qry)
            {
                var stri = s.Replace(".mdx", ".m2");
                stri = stri.Replace(".MDX", ".M2");
                DoodadNames.Add(ofs, stri);
                ofs += (uint)s.Length + 1;
            }

            mpqFile.Position = 0x14 + mHeader.ofsMddf + 0x04;
            size = mpqFile.Read<uint>();
            uint ssize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(MDDF));
            uint numEntries = size / ssize;
            var DoodadPlacements = new MDDF[numEntries];
            mpqFile.Read(DoodadPlacements);

            mpqFile.Position = 0x14 + mHeader.ofsMmid + 0x04;
            size = mpqFile.Read<uint>();
            numEntries = size / 4;
            var DoodadIds = new uint[numEntries];
            mpqFile.Read(DoodadIds);

            ModelDefinitions = DoodadPlacements.ToList();
            ModelIdentifiers = DoodadIds.ToList();

            mpqFile.Position = 0x14 + mHeader.ofsMwmo + 0x04;
            size = mpqFile.Read<uint>();
            data = mpqFile.Read(size);
            fullStr = Encoding.UTF8.GetString(data);
            Names = fullStr.Split('\0');
            qry = from n in Names where n != "" select n;
            ofs = 0;

            foreach (var s in qry)
            {
                WMONames.Add(ofs, s);
                ofs += (uint)s.Length + 1;
            }

            mpqFile.Position = 0x14 + mHeader.ofsModf + 0x04;
            size = mpqFile.Read<uint>();
            ssize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(MODF));
            numEntries = size / ssize;
            var WmoPlacements = new MODF[numEntries];
            mpqFile.Read(WmoPlacements);

            mpqFile.Position = 0x14 + mHeader.ofsMwid + 0x04;
            size = mpqFile.Read<uint>();
            numEntries = size / 4;
            var WmoIds = new uint[numEntries];
            mpqFile.Read(WmoIds);

            WMODefinitions = WmoPlacements.ToList();
            WMOIdentifiers = WmoIds.ToList();

            List<ADTChunk> chunks = new List<ADTChunk>();

            for (uint i = 0; i < 256; ++i)
            {
                ADTChunk chunk = new ADTChunk(this, mpqFile, mOffsets[i]);
                if (chunk.PreLoadChunk())
                {
                    chunks.Add(chunk);
                }
            }

            lock (mChunks) mChunks.AddRange(chunks);
        }

        public int addTexture(string textureName)
        {
            for (int i = 0; i < mTextures.Count; ++i)
            {
                if (mTextures[i].Name.ToLower() == textureName.ToLower())
                    return i;
            }

            Game.GameManager.GraphicsThread.CallOnThread(() =>
                {
                    mTextures.Add(Video.TextureManager.GetTexture(textureName));
                }
            );

            return mTextures.Count - 1;
        }

        public string getTextureName(int index)
        {
            return GetTexture(index).Name;
        }

        public int addMdxName(string name)
        {
            uint id = 0;
            if(DoodadNames.Count != 0)
                id = DoodadNames.Keys.Last() + (uint)DoodadNames.Values.Last().Length + 1;

            DoodadNames.Add(id, name);
            ModelIdentifiers.Add(id);
            return ModelIdentifiers.Count - 1;
        }

        public uint addModelDefintion(MDDF ddf)
        {
            ModelDefinitions.Add(ddf);
            return (uint)(ModelDefinitions.Count - 1);
        }

        private MHDR mHeader;
        private MCIN[] mOffsets = new MCIN[256];
        private string[] mTextureNames;
        private List<Video.TextureHandle> mTextures = new List<Video.TextureHandle>();

        public Video.TextureHandle GetTexture(int index) { return mTextures[index]; }
        public override List<string> TextureNames { get { return mTextureNames.ToList(); } }
    }
}
