using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SlimDX;

namespace SharpWoW.Models.WMO
{
    public class WMOGroup
    {
        public WMOGroup(string fileName, uint num, WMOFile parent)
        {
            mParent = parent;
            FileName = Path.GetDirectoryName(fileName) + "\\" + Path.GetFileNameWithoutExtension(fileName) + "_" + num.ToString("D3") + ".wmo";
        }

        public bool LoadGroup()
        {
            mFile = new Stormlib.MPQFile(FileName);
            SeekChunk("PGOM", false);
            mFile.Position += 4;
            mHeader = mFile.Read<MOGP>();

            SeekChunk("TVOM");
            uint size = mFile.Read<uint>();
            byte[] data = mFile.Read(size);

            Vector3[] vertices = new Vector3[size / 12];
            Utils.Memory.CopyMemory(data, vertices);
            return true;
        }

        private void SeekChunk(string id, bool afterHeader = true)
        {
            mFile.Position = 0;

            if (afterHeader == true)
            {
                SeekChunk("PGOM", false);
                mFile.Position += 4 + System.Runtime.InteropServices.Marshal.SizeOf(typeof(MOGP));
            }

            while (GetChunk() != id)
            {
                uint size = mFile.Read<uint>();
                mFile.Position += size;
            }
        }

        private string GetChunk()
        {
            byte[] sig = mFile.Read(4);
            var str = Encoding.UTF8.GetString(sig);
            return str;
        }

        public string FileName { get; private set; }

        private MOGP mHeader;
        private WMOFile mParent;
        private Stormlib.MPQFile mFile;
    }
}
