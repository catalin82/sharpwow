using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpWoW.Utils
{
    public static class Metrics
    {
        public const float Tilesize = 533 + 1.0f / 3.0f;
        public const float Chunksize = Tilesize / 16.0f;
        public const float Unitsize = Chunksize / 8;
        public const float MidPoint = 32.0f * Tilesize;
        public const float ChunkRadius = 23.55549f;
    }
}
