using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpWoW.Models.WMO
{
    public static class WMOCache
    {
        private static Dictionary<int, WMOFile> mCacheEntries = new Dictionary<int, WMOFile>();
        private static Dictionary<int, uint> mCacheRefs = new Dictionary<int, uint>();
        private static object lockObj = new object();

        public static WMOFile GetCacheEntry(string wmoFile)
        {
            lock (lockObj)
            {
                int hash = wmoFile.ToLower().GetHashCode();
                if (mCacheEntries.ContainsKey(hash))
                {
                    var ret = mCacheEntries[hash];
                    mCacheRefs[hash]++;
                    return ret;
                }

                WMOFile file = new WMOFile(wmoFile);
                mCacheEntries.Add(hash, file);
                mCacheRefs.Add(hash, 1);
                return file;
            }
        }

        public static void RemoveCacheEntry(string wmoFile)
        {
            lock (lockObj)
            {
                int hash = wmoFile.ToLower().GetHashCode();
                if (!mCacheRefs.ContainsKey(hash))
                {
                    if (mCacheEntries.ContainsKey(hash))
                    {
                        var ent = mCacheEntries[hash];
                        // ent.Unload();
                        mCacheEntries.Remove(hash);
                    }

                    return;
                }

                --mCacheRefs[hash];
                if (mCacheRefs[hash] == 0)
                {
                    var ent = mCacheEntries[hash];
                    // ent.Unload();
                    mCacheEntries.Remove(hash);
                    mCacheRefs.Remove(hash);
                }
            }
        }
    }
}
