using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SharpWoW.ADT.Cataclysm
{
    public partial class ADTFile
    {
        private void LoadAsyncData()
        {
            mpqFile.Position = 0x14;
            mHeader = mpqFile.Read<MHDR>();
            LoadTexStream();
            InitChunkOffsets();
            LoadTextureNames();
            LoadChunks();
        }

        private void LoadTexStream()
        {
            List<Stormlib.MPQFile> texFiles = new List<Stormlib.MPQFile>();
            for (int i = 0; i < 10; ++i)
            {
                string texPath = Path.GetDirectoryName(FileName) + "\\" + Path.GetFileNameWithoutExtension(FileName) + "_tex" + i + ".adt";
                try
                {
                    texFiles.Add(new Stormlib.MPQFile(texPath));
                }
                catch(Exception)
                {
                }
            }

            TexStream = new Utils.StreamedMpq(texFiles);
        }

        private void InitChunkOffsets()
        {
            SeekChunk(mpqFile, "KNCM");
            for (int i = 0; i < 256; ++i)
            {
                var ofs = new ChunkOffset()
                {
                    Offset = (uint)mpqFile.Position,
                };

                mpqFile.Position += 4;
                ofs.Size = mpqFile.Read<uint>();

                mChunkOffsets.Add(ofs);
                mpqFile.Position += ofs.Size;
            }

            SeekChunk(TexStream, "KNCM");
            for (int i = 0; i < 256; ++i)
            {
                var offset = (uint)TexStream.Position;
                TexStream.Position += 4;
                uint size = TexStream.Read<uint>();

                mChunkOffsets[i].OffsetTexStream = offset;
                TexStream.Position = offset + 8 + size;
            }
        }

        private void LoadTextureNames()
        {
            SeekChunk(TexStream, "XETM");
            TexStream.Position += 4;
            uint numBytes = TexStream.Read<uint>();
            byte[] textureData = new byte[numBytes];
            TexStream.Read(textureData, 0, (int)numBytes);
            string texString = Encoding.UTF8.GetString(textureData);
            string[] texNames = texString.Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
            mTextureNames = texNames.ToList();

            Game.GameManager.GraphicsThread.CallOnThread(() =>
                {
                    foreach (var tex in mTextureNames)
                    {
                        mTextures.Add(Video.TextureManager.GetTexture(tex));
                    }
                }
            );
        }

        private void LoadChunks()
        {
            for (int i = 0; i < 256; ++i)
            {
                ADTChunk cnk = new ADTChunk(this, mpqFile, TexStream, mChunkOffsets[i]);
                cnk.DoLoad();
                mChunks.Add(cnk);
            }
        }

        private void SeekChunk(Stream strm, string id)
        {
            strm.Position = 0;
            while (GetChunkSignature(strm) != id)
            {
                byte[] szBytes = new byte[4];
                strm.Read(szBytes, 0, 4);
                uint size = BitConverter.ToUInt32(szBytes, 0);
                strm.Position += size;
            }
            strm.Position -= 4;
        }

        private string GetChunkSignature(Stream strm)
        {
            var bytes = new byte[4];
            strm.Read(bytes, 0, 4);
            var ret = Encoding.UTF8.GetString(bytes);
            return ret;
        }

        public Video.TextureHandle GetTexture(int index) { return mTextures[index]; }

        private MHDR mHeader;
        private Utils.StreamedMpq TexStream;
        private List<ChunkOffset> mChunkOffsets = new List<ChunkOffset>();
        private List<Video.TextureHandle> mTextures = new List<Video.TextureHandle>();
        private List<string> mTextureNames = new List<string>();
    }
}
