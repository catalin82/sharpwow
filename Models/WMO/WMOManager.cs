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

        public static uint AddInstance(string name, SlimDX.Vector3 pos, uint uniqueId, SlimDX.Vector3 rotation)
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
                    return mRenders[hash].PushInstance(uniqueId, pos, rotation);
                else
                {
                    WMORender rdr = new WMORender(name);
                    mRenders.Add(hash, rdr);
                    return rdr.PushInstance(uniqueId, pos, rotation);
                }
            }
        }

        public static bool IsWmoHit(out WMOHitInformation info, out SlimDX.Vector3 hitPos)
        {
            info = null;
            bool hasHit = false;
            uint uniqueId = 0;
            uint refId = 0;
            hitPos = SlimDX.Vector3.Zero;
            SlimDX.Matrix modelMatrix = SlimDX.Matrix.Identity;
            WMOFile hitFile = null;
            lock (lockobj)
            {
                float curNear = 99999;
                
                foreach (var rndr in mRenders)
                {
                    float curHit = 0;
                    uint curInst = 0;
                    uint curRef = 0;
                    SlimDX.Vector3 pos;
                    SlimDX.Matrix tmpMatrix;
                    if (rndr.Value.IsInstanceHit(out curHit, out curInst, out curRef, out pos, out tmpMatrix))
                    {
                        hasHit = true;
                        if (curHit < curNear)
                        {
                            curNear = curHit;
                            uniqueId = curInst;
                            refId = curRef;
                            hitPos = pos;
                            modelMatrix = tmpMatrix;
                            hitFile = rndr.Value.File;
                        }
                    }
                }
            }

            if (hasHit)
            {
                info = ADT.ADTManager.GetWmoInformation(uniqueId, refId);
                info.HitPoint = hitPos;
                info.ModelMatrix = modelMatrix;
                info.Model = hitFile;
            }

            return hasHit;
        }          
    }
}
