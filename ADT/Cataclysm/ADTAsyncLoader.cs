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
        /// <summary>
        /// This function loads all the asynchronous available data in a separate thread. This
        /// means all the non-DirectX data like all the I/O
        /// </summary>
        private void LoadAsyncData()
        {
            mpqFile.Position = 0x14;
            mHeader = mpqFile.Read<MHDR>();
            LoadTexStream();
            InitChunkOffsets();
            LoadTextureNames();
            LoadChunks();
        }

        /// <summary>
        /// Loads up to 10 _tex.adt files into the the texture file stream and combines them to one big file in memory.
        /// </summary>
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

        /// <summary>
        /// Loads all the offsets of the MCNK-chunks in the main file and in the tex stream and stores them in mChunkOffsets
        /// </summary>
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

        /// <summary>
        /// Reads the MTEX chunk and splits the block of texture names into the separate textures. 
        /// Loads all the textures in the main thread using Invoke
        /// </summary>
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

        /// <summary>
        /// Loads all the 256 subchunks of the ADT synchronous. The chunks are completly loaded and are ready to render.
        /// The chunk offsets need to be loaded as well as the TexStream (<see cref="InitChunkOffsets"/>)
        /// </summary>
        private void LoadChunks()
        {
            for (int i = 0; i < 256; ++i)
            {
                ADTChunk cnk = new ADTChunk(this, mpqFile, TexStream, mChunkOffsets[i]);
                cnk.DoLoad();
                mChunks.Add(cnk);
            }
        }

        /// <summary>
        /// Searches the first occurence of a chunk-ID (4 digit magic) in a stream and places the streams position to the beginning of the
        /// magic ID.
        /// </summary>
        /// <param name="strm">The stream to search for (strm.Position is modified!)</param>
        /// <param name="id">The ID to search for (4 byte magic)</param>
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

        /// <summary>
        /// Searches a given stream for the next occurence of a given 4 digit magic ID. The stream must point to a valid start of a chunk
        /// (4 byte magic, 4 byte size) when used in this function
        /// </summary>
        /// <param name="strm">The stream to search. The next 8 bytes of the stream starting from stream.Position must be a valid chunk header</param>
        /// <param name="id">The ID to search for</param>
        private void SeekNextChunk(Stream strm, string id)
        {
            while (GetChunkSignature(strm) != id)
            {
                byte[] szBytes = new byte[4];
                strm.Read(szBytes, 0, 4);
                uint size = BitConverter.ToUInt32(szBytes, 0);
                strm.Position += size;
            }
            strm.Position -= 4;
        }

        /// <summary>
        /// Gets the 4 byte magic at the current position of the stream converted to a string where each byte is converted to a char
        /// as UTF8 binary string.
        /// </summary>
        /// <param name="strm"></param>
        /// <returns></returns>
        private string GetChunkSignature(Stream strm)
        {
            var bytes = new byte[4];
            strm.Read(bytes, 0, 4);
            var ret = Encoding.UTF8.GetString(bytes);
            return ret;
        }

        /// <summary>
        /// Gets a handle to a texture at a given index. The textures must be loaded first by the function LoadTextureNames.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Video.TextureHandle GetTexture(int index) { return mTextures[index]; }

        private MHDR mHeader;
        private Utils.StreamedMpq TexStream;
        private List<ChunkOffset> mChunkOffsets = new List<ChunkOffset>();
        private List<Video.TextureHandle> mTextures = new List<Video.TextureHandle>();
        private List<string> mTextureNames = new List<string>();
    }
}
