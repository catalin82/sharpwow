using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;

namespace SharpWoW.ADT.Wotlk
{
    public class BasicInfoProvider : IBasicInfoQuery
    {
        public BasicInfoProvider()
        {
            mQueryDelegates[InfoQuery.ChunkAreaId] = getChunkAreaId;
        }

        public override T queryValue<T>(InfoQuery property, params object[] args)
        {
            if (mQueryDelegates.ContainsKey(property) == false)
                throw new NotImplementedException();

            return (T)mQueryDelegates[property](args);
        }

        private object getChunkAreaId(object[] args)
        {
            if (args.Length != 3)
                throw new ArgumentException("Format: getChunkAreaId(string continent, Vector2 positionWorld)");

            var continent = args[0] as string;
            var position = (SlimDX.Vector2)args[1];
            var noThrow = (bool)args[2];

            if (continent == null)
            {
                if (!noThrow)
                    throw new ArgumentException("Format: getChunkAreaId(string continent, Vector2 positionWorld)");
                return (uint)0;
            }

            int adtIndexX = (int)(position.X / Utils.Metrics.Tilesize);
            int adtIndexY = (int)(position.Y / Utils.Metrics.Tilesize);

            float posX = position.X - adtIndexX * Utils.Metrics.Tilesize;
            float posY = position.Y - adtIndexY * Utils.Metrics.Tilesize;

            int chunkIndexX = (int)(posX / Utils.Metrics.Tilesize);
            int chunkIndexY = (int)(posY / Utils.Metrics.Tilesize);

            var fileName = @"World\Maps\" + continent + "\\" + continent + "_" + adtIndexX + "_" + adtIndexY + ".adt";
            if (noThrow)
            {
                if (Stormlib.MPQFile.Exists(fileName) == false)
                    return (uint)0;
            }

            var file = new Stormlib.MPQFile(fileName);
            var id = getChunkHeader(file, chunkIndexX, chunkIndexY, noThrow).areaId;
            file.Close();
            return id;
        }

        private MCNK getChunkHeader(Stormlib.MPQFile file, int chunkX, int chunkY, bool noThrow)
        {
            int cindex = chunkX + chunkY * 16;
            if (cindex < 0 || cindex >= 256)
            {
                if (noThrow)
                    return new MCNK();

                throw new ArgumentException();
            }

            if (noThrow)
            {
                if (!TrySeekChunk(file, "NICM"))
                    return new MCNK();
            }
            else
            {
                SeekChunk(file, "NICM");
            }

            file.Position += 8 + cindex * Marshal.SizeOf(typeof(MCIN));
            var mcin = file.Read<MCIN>();
            file.Position = mcin.ofsMcnk + 8;
            return file.Read<MCNK>();
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

        private bool TrySeekChunk(Stream strm, string id)
        {
            strm.Position = 0;
            string signature;
            while (TryGetChunkSignature(strm, out signature))
            {
                if (id == signature)
                {
                    strm.Position -= 4;
                    return true;
                }

                if ((strm.Length - strm.Position) < 4)
                    return false;

                byte[] szBytes = new byte[4];
                strm.Read(szBytes, 0, 4);
                uint size = BitConverter.ToUInt32(szBytes, 0);
                if ((strm.Length - strm.Position) < size)
                    return false;

                strm.Position += size;
            }

            return false;
        }

        private bool TryGetChunkSignature(Stream strm, out string signature)
        {
            signature = "";
            if ((strm.Length - strm.Position) < 4)
                return false;

            var bytes = new byte[4];
            strm.Read(bytes, 0, 4);
            var ret = Encoding.UTF8.GetString(bytes);
            signature = ret;
            return true;
        }

        private string GetChunkSignature(Stream strm)
        {
            var bytes = new byte[4];
            strm.Read(bytes, 0, 4);
            var ret = Encoding.UTF8.GetString(bytes);
            return ret;
        }

        private Dictionary<InfoQuery, Func<object[], object>> mQueryDelegates = new Dictionary<InfoQuery, Func<object[], object>>();
    }
}
