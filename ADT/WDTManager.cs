using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpWoW.ADT
{
    public class WDTManager
    {
        public WDTFile getWDT(string continent)
        {
            lock (mWdtFiles)
            {
                if (mWdtFiles.ContainsKey(continent) == true)
                    return mWdtFiles[continent];

                WDTFile wdt = new WDTFile(continent);
                mWdtFiles.Add(continent, wdt);
                return wdt;
            }
        }

        private Dictionary<string, WDTFile> mWdtFiles = new Dictionary<string, WDTFile>();
    }
}
