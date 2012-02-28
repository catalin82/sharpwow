using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpWoW.ADT
{
    public static class ADTStaticData
    {
        internal static short[] Indices = new short[768];
        internal static float[,] TexCoords = new float[145, 2];
        internal static float[,] AlphaCoords = new float[145, 2];

        public const float HoleSize = Utils.Metrics.Chunksize / 64.0f;
        public const float HoleLen = Utils.Metrics.Chunksize / 4.0f;

        public static uint[,] HoleBitmap =
        {
                { 1 << 0, 1 << 1, 1 << 2, 1 << 3 },
                { 1 << 4, 1 << 5, 1 << 6, 1 << 7 },
                { 1 << 8, 1 << 9, 1 << 10, 1 << 11 },
                { 1 << 12, 1 << 13, 1 << 14, 1 << 15 },
        };

        static ADTStaticData()
        {
            short[,] indices = new short[64, 12];
            for (short i = 0; i < 8; ++i)
            {
                for (short j = 0; j < 8; ++j)
                {
                    short topLeft = (short)(i * 17 + j);
                    short midPoint = (short)(i * 17 + j + 9);
                    short topRight = (short)(i * 17 + j + 1);
                    short bottomRight = (short)(i * 17 + j + 18);
                    short bottomLeft = (short)((i + 1) * 17 + j);
                    indices[i * 8 + j, 0] = topLeft;
                    indices[i * 8 + j, 1] = midPoint;
                    indices[i * 8 + j, 2] = bottomLeft;
                    indices[i * 8 + j, 3] = topLeft;
                    indices[i * 8 + j, 4] = midPoint;
                    indices[i * 8 + j, 5] = topRight;
                    indices[i * 8 + j, 6] = topRight;
                    indices[i * 8 + j, 7] = midPoint;
                    indices[i * 8 + j, 8] = bottomRight;
                    indices[i * 8 + j, 9] = bottomRight;
                    indices[i * 8 + j, 10] = midPoint;
                    indices[i * 8 + j, 11] = bottomLeft;
                }
            }

            for (short i = 0; i < 64; ++i)
                for (short j = 0; j < 12; ++j)
                    Indices[i * 12 + j] = (short)(indices[i, j]);

            LoadTexCoords();
            LoadAlphaCoords();
        }

        private static void LoadTexCoords()
        {
            uint counter = 0;
            float tx, ty;
            for (int j = 0; j < 17; ++j)
            {
                for (int i = 0; i < (((j % 2) != 0) ? 8 : 9); ++i)
                {
                    tx = (float)i;
                    ty = (float)j * 0.5f;
                    if ((j % 2) != 0)
                        tx += 0.5f;

                    TexCoords[counter, 0] = tx;
                    TexCoords[counter++, 1] = ty;
                }
            }
        }

        private static void LoadAlphaCoords()
        {
            uint counter = 0;
            float tx, ty;
            for (int j = 0; j < 17; ++j)
            {
                for (int i = 0; i < (((j % 2) != 0) ? 8 : 9); ++i)
                {
                    tx = i / 8.0f;
                    ty = j / 16.0f;
                    if ((j % 2) != 0)
                        tx += 1 / 16.0f;

                    AlphaCoords[counter, 0] = tx;
                    AlphaCoords[counter++, 1] = ty;
                }
            }
        }
    }
}
