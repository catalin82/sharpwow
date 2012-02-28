using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpWoW.Models.MDX
{
    public class M2Manager
    {
        internal M2Manager()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="modelName"></param>
        /// <param name="df"></param>
        public uint AddInstance(string modelName, ADT.Wotlk.MDDF df)
        {
            renderLock.WaitOne();
            int hash = modelName.ToLower().GetHashCode();
            float rotX = (float)(Math.PI * df.orientationX / 180.0f);
            float rotZ = (float)(Math.PI * df.orientationY / 180.0f);
            float rotY = (float)(Math.PI * df.orientationZ / 180.0f);
            if (BatchRenderers.ContainsKey(hash))
            {
                uint id = BatchRenderers[hash].AddInstance(-Utils.Metrics.MidPoint + df.posX, -Utils.Metrics.MidPoint + df.posZ, df.posY, df.scaleFactor / 1024.0f, new SlimDX.Vector3(rotX, rotY, rotZ));
                renderLock.ReleaseMutex();
                return id;
            }
            try
            {
                M2BatchRender rndr = new M2BatchRender(modelName);
                uint ret = rndr.AddInstance(-Utils.Metrics.MidPoint + df.posX, -Utils.Metrics.MidPoint + df.posZ, df.posY, df.scaleFactor / 1024.0f, new SlimDX.Vector3(rotX, rotY, rotZ));
                BatchRenderers.Add(hash, rndr);
                renderLock.ReleaseMutex();
                return ret;
            }
            catch (Exception)
            {
                renderLock.ReleaseMutex();
                throw;
            }
        }

        private System.Threading.Mutex renderLock = new System.Threading.Mutex();
        private Dictionary<int, M2BatchRender> BatchRenderers = new Dictionary<int, M2BatchRender>();
    }
}
