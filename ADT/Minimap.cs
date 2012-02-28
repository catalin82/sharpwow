using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace SharpWoW.ADT
{
    public class Minimap
    {
        public Minimap(string continent, uint mapId)
        {
            mContinent = continent;
            mMapId = mapId;
        }

        public Bitmap CreateImage()
        {
            if (CreatedImages.ContainsKey(mMapId))
                return CreatedImages[mMapId];

            Stormlib.MPQFile file = new Stormlib.MPQFile(@"World\Maps\" + mContinent + "\\" + mContinent + ".wdl");
            uint[] offsets = new uint[64 * 64];
            string signature = "";
            while ((signature = ReadSignature(file)) != "MAOF")
                SkipChunk(file);

            file.Position += 4;
            file.Read(offsets);
            uint[] texData = new uint[64 * 17 * 64 * 17];
            short[] tile = new short[17 * 17];
            for (uint i = 0; i < 64; ++i)
            {
                for (uint j = 0; j < 64; ++j)
                {
                    if (offsets[i * 64 + j] != 0)
                    {
                        file.Position = offsets[i * 64 + j] + 0x08;
                        file.Read(tile);
                        for (uint k = 0; k < 17; ++k)
                        {
                            for (uint l = 0; l < 17; ++l)
                            {
                                short height = tile[k * 17 + l];
                                uint r = 0, g = 0, b = 0;
                                if (height > 1000)
                                {
                                    r = g = b = 255;
                                }
                                else if (height > 600)
                                {
                                    float am = (height - 600.0f) / 400.0f;
                                    r = (uint)(0.75f + am * 0.25f * 255);
                                    g = (uint)(0.5f * am * 255);
                                    b = (uint)(am * 255);
                                }
                                else if (height > 300)
                                {
                                    float am = (height - 300.0f) / 300.0f;
                                    r = (uint)(255 - am * 255);
                                    g = (uint)(1.0f);
                                    b = (uint)(0.0f);
                                }
                                else if (height > 0)
                                {
                                    float am = height / 300.0f;
                                    r = (uint)(0.75f * am * 255);
                                    g = (uint)(255 - (0.5f * am * 255));
                                    b = (uint)(0);
                                }
                                else if (height > -100)
                                {
                                    float am = (height + 100.0f) / 100.0f;
                                    r = (uint)(0.0f);
                                    g = (uint)(am * 255);
                                    b = (uint)(255);
                                }
                                else if (height > -550)
                                {
                                    float am = (height + 550) / (450.0f);
                                    r = (uint)(0.0f);
                                    g = (uint)(0.0f);
                                    b = (uint)(0x7F);
                                }
                                else if (height > -1000)
                                {
                                    r = (uint)(0);
                                    g = (uint)(0);
                                    b = (uint)(0x7F);
                                }
                                if (k == 0 || l == 0)
                                    r = g = b = 0x3F;

                                texData[(i * 17 + k) * (64 * 17) + j * 17 + l] = (uint)((b) | (g << 8) | (r << 16) | (255 << 24));
                            }
                        }
                    }
                }
            }

            Bitmap bmp = new Bitmap(64 * 17, 64 * 17);
            var data = bmp.LockBits(new Rectangle(0, 0, 64 * 17, 64 * 17), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            Utils.Memory.CopyMemory(texData, data.Scan0);
            bmp.UnlockBits(data);
            CreatedImages.Add(mMapId, bmp);
            return bmp;
        }

        private string ReadSignature(Stormlib.MPQFile file)
        {
            byte[] bytes = file.Read(4);
            bytes = bytes.Reverse().ToArray();
            return Encoding.UTF8.GetString(bytes);
        }

        private void SkipChunk(Stormlib.MPQFile file)
        {
            uint size = file.Read<uint>();
            file.Position += size;
        }

        private string mContinent;
        private uint mMapId;

        public static Dictionary<uint, Bitmap> CreatedImages { get; private set; }

        static Minimap()
        {
            CreatedImages = new Dictionary<uint, Bitmap>();
        }
    }
}
