using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpWoW.Models.WMO
{
    public static class WMOManager
    {
        private static Dictionary<int, WMORender> mRenders = new Dictionary<int, WMORender>();
        private static object lockobj = new object();

        public static uint AddInstance(string name, SlimDX.Vector3 pos)
        {
            pos.X -= Utils.Metrics.MidPoint;
            float tmpY = pos.Z - Utils.Metrics.MidPoint;
            float tmpZ = pos.Y;
            pos.Y = tmpY;
            pos.Z = tmpZ;

            int hash = name.ToLower().GetHashCode();
            lock (lockobj)
            {
                if (mRenders.ContainsKey(hash))
                    return mRenders[hash].PushInstance(pos);
                else
                {
                    WMORender rdr = new WMORender(name);
                    mRenders.Add(hash, rdr);
                    return rdr.PushInstance(pos);
                }
            }
        }
    }
}
