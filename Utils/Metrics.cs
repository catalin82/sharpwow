using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX;

namespace SharpWoW.Utils
{
    public static class Metrics
    {
        public const float Tilesize = 533 + 1.0f / 3.0f;
        public const float Chunksize = Tilesize / 16.0f;
        public const float Unitsize = Chunksize / 8;
        public const float MidPoint = 32.0f * Tilesize;
        public const float ChunkRadius = 23.55549f;

        public static Vector3 ToServerCoords(Vector3 clientCoords)
        {
            var serverVec = clientCoords;
            serverVec.X = -Utils.Metrics.MidPoint + serverVec.X;
            serverVec.Y = -Utils.Metrics.MidPoint + serverVec.Y;
            return serverVec;
        }

        public static Vector3 ToClientCoords(Vector3 serverCoords)
        {
            var clientVec = serverCoords;
            clientVec.X = clientVec.X + MidPoint;
            clientVec.Y = clientVec.Y + MidPoint;
            return clientVec;
        }

        public static Vector2 ToServerCoords(Vector2 clientCoords)
        {
            var serverVec = clientCoords;
            serverVec.X = -Utils.Metrics.MidPoint + serverVec.X;
            serverVec.Y = -Utils.Metrics.MidPoint + serverVec.Y;
            return serverVec;
        }

        public static Vector2 ToClientCoords(Vector2 serverCoords)
        {
            var clientVec = serverCoords;
            clientVec.X = clientVec.X + MidPoint;
            clientVec.Y = clientVec.Y + MidPoint;
            return clientVec;
        }
    }
}
