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

        private MHDR mHeader;
        private MCIN[] mOffsets = new MCIN[256];
        private string[] mTextureNames;
        private List<Video.TextureHandle> mTextures = new List<Video.TextureHandle>();

        public Video.TextureHandle GetTexture(int index) { return mTextures[index]; }
        public override List<string> TextureNames { get { return mTextureNames.ToList(); } }
    }
}
