using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpWoW.Models.WMO
{
    public class WMOFile
    {
        public WMOFile(string fileName)
        {
            FileName = fileName;
            //System.Threading.Thread t = new System.Threading.Thread(AsyncLoadProc);
            //t.Start();
            AsyncLoadProc();
        }

        private void AsyncLoadProc()
        {
            mFile = new Stormlib.MPQFile(FileName);
            SeekChunk("DHOM");
            mFile.Position += 4;
            mHeader = mFile.Read<MOHD>();

            isLoadFinished = true;
        }

        private void SeekChunk(string id)
        {
            mFile.Position = 0;
            while (GetChunk() != id)
            {
                uint size = mFile.Read<uint>();
                mFile.Position += size;
            }
        }

        private string GetChunk()
        {
            byte[] sig = new byte[4];
            var str = Encoding.UTF8.GetString(sig);
            return str;
        }

        public string FileName { get; private set; }

        private MOHD mHeader;
        private bool isLoadFinished = false;
        private Stormlib.MPQFile mFile;

        public bool LoadFinished { get { return isLoadFinished; } }
    }
}
