using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpWoW.Models.MDX
{
    /// <summary>
    /// Caches the information for all active models. THREADSAFE!
    /// </summary>
    public class M2InfoCache
    {
        /// <summary>
        /// Get the info of a model name
        /// </summary>
        /// <param name="modelName"></param>
        /// <returns></returns>
        public M2Info GetInfo(string modelName)
        {
            int hash = modelName.ToLower().GetHashCode();
            lock (mInfoLock)
            {
                if (cacheTable.ContainsKey(hash))
                {
                    M2CacheEntry ret = cacheTable[hash];
                    ++ret.numRefs;
                    return ret.Info;
                }
            }

            M2CacheEntry ent = new M2CacheEntry();
            ent.Info = new M2Info(modelName);
            ent.numRefs = 1;

            lock (mInfoLock) cacheTable.Add(hash, ent);
            return ent.Info;
        }

        /// <summary>
        /// Releases a an instance of the info by decreasing the ref-count if the model exists in the table
        /// </summary>
        /// <param name="modelName">Path of the modelfile</param>
        public void ReleaseInfo(string modelName)
        {
            int hash = modelName.ToLower().GetHashCode();
            lock (mInfoLock)
            {
                if (cacheTable.ContainsKey(hash))
                {
                    M2CacheEntry ent = cacheTable[hash];
                    --ent.numRefs;
                    if (ent.numRefs == 0)
                    {
                        cacheTable.Remove(hash);
                    }
                }
            }
        }

        private object mInfoLock = new object();
        private SortedList<int, M2CacheEntry> cacheTable = new SortedList<int, M2CacheEntry>();
    }

    internal class M2CacheEntry
    {
        internal uint numRefs = 0;
        internal M2Info Info = null;
    }
}
