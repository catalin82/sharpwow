using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SharpWoW.ADT
{
    public class WDTFile
    {
        protected Stormlib.MPQFile mFile;

        /// <summary>
        /// Searches a chunk in the file by its 4 byte signature
        /// </summary>
        /// <param name="strm">The stream to search in</param>
        /// <param name="id">The 4 byte ID that identifies the chunk</param>
        /// <exception cref="System.IndexOutOfRangeException">If the signature wasnt found</exception>
        protected void SeekChunk(Stream strm, string id)
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
        /// Gets a 4 byte chunk signature from the current position of a stream
        /// </summary>
        /// <param name="strm">The stream to read from</param>
        /// <returns>String with the 4 byte chunk signature</returns>
        /// <exception cref="System.IndexOutOfRangeException">If the stream has not enough space to read a signature from</exception>
        protected string GetChunkSignature(Stream strm)
        {
            var bytes = new byte[4];
            strm.Read(bytes, 0, 4);
            var ret = Encoding.UTF8.GetString(bytes);
            return ret;
        }

        public WDTFile(string continent)
        {
            mFile = new Stormlib.MPQFile(@"World\Maps\" + continent + "\\" + continent + ".wdt");
            SeekChunk(mFile, "DHPM");
            mFile.Position = 8;
            Flags = mFile.Read<uint>();
        }

        public uint Flags { get; private set; }
    }
}
