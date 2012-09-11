using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX.Direct3D9;
using System.Runtime.InteropServices;

namespace SharpWoW.Video
{
    public static class TextureConverter
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct BlpHeader
        {
            public uint Signature;
            public uint Version;
            public byte Compression;
            public byte AlphaDepth;
            public byte AlphaEncoding;
            public byte HasMipMap;
            public int Width;
            public int Height;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public int[] Offsets;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public int[] Sizes;
        }

        public enum BlpCompression
        {
            Dxt1,
            Dxt3,
            Dxt5
        }

        public static void SaveTextureAsBlp(BlpCompression compression, Texture texture, string fileName)
        {
            Format surfaceFormat = Format.Unknown;
            int blockSize = 0;

            texture.GenerateMipSublevels();
            BlpHeader header = new BlpHeader();
            header.Version = 1;
            header.AlphaDepth = 8;
            header.Offsets = new int[16];
            header.Sizes = new int[16];
            header.Compression = 2;
            switch (compression)
            {
                case BlpCompression.Dxt1:
                    header.AlphaEncoding = 0;
                    surfaceFormat = Format.Dxt1;
                    blockSize = 2;
                    break;

                case BlpCompression.Dxt3:
                    header.AlphaEncoding = 1;
                    surfaceFormat = Format.Dxt3;
                    blockSize = 4;
                    break;

                case BlpCompression.Dxt5:
                    header.AlphaEncoding = 7;
                    surfaceFormat = Format.Dxt5;
                    blockSize = 4;
                    break;
            }

            header.HasMipMap = (texture.LevelCount > 1) ? (byte)1 : (byte)0;
            header.Width = texture.GetLevelDescription(0).Width;
            header.Height = texture.GetLevelDescription(0).Height;
            header.Signature = 0x32504C42;
            System.IO.Stream strm = System.IO.File.OpenWrite(fileName);
            using (System.IO.BinaryWriter bw = new System.IO.BinaryWriter(strm))
            {
                bw.WriteStruct(header);
                for (int i = 0; i < 16; ++i)
                {
                    if (i >= texture.LevelCount)
                    {
                        header.Offsets[i] = 0;
                        header.Sizes[i] = 0;

                        continue;
                    }

                    var desc = texture.GetLevelDescription(i);
                    Texture tmpTexture = new Texture(texture.Device, desc.Width, desc.Height, 1, Usage.None, surfaceFormat, Pool.Managed);
                    var dstSurface = tmpTexture.GetSurfaceLevel(0);
                    Surface.FromSurface(dstSurface, texture.GetSurfaceLevel(i), Filter.Point, 0);
                    var rect = dstSurface.LockRectangle(LockFlags.None);
                    var size = blockSize * desc.Width * desc.Height;
                    byte[] buffer = new byte[size];
                    header.Offsets[i] = (int)bw.BaseStream.Position;
                    header.Sizes[i] = size;
                    rect.Data.Read(buffer, 0, size);
                    bw.Write(buffer);
                    dstSurface.Dispose();
                    tmpTexture.Dispose();
                }

                bw.BaseStream.Position = 0;
                bw.WriteStruct(header);
            }
        }
    }
}
