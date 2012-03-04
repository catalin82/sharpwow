using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;

namespace SharpWoW.Models.WMO
{
    public class WMORender
    {
        public WMORender(string wmoName)
        {
            mFile = WMOCache.GetCacheEntry(wmoName);
            Game.GameManager.GraphicsThread.OnFrame += new Game.VideoThread.FrameRenderDelegate(RenderInstances);
        }

        void RenderInstances(SlimDX.Direct3D9.Device device, TimeSpan deltaTime)
        {
            device.SetRenderState(SlimDX.Direct3D9.RenderState.AlphaBlendEnable, true);
            lock (mInstLock)
            {
                foreach (var mat in mInstances.Values)
                {
                    if (Game.GameManager.GraphicsThread.GraphicsManager.Camera.ViewFrustum.Contains(mFile.BoundingBox, mat) == ContainmentType.Disjoint)
                        return;

                    mFile.Draw(mat);
                }
            }
            device.SetRenderState(SlimDX.Direct3D9.RenderState.AlphaBlendEnable, false);
        }

        public uint PushInstance(Vector3 pos)
        {
            Matrix mat = Matrix.Translation(pos);
            uint id = RequestInstanceId();
            lock (mInstLock) mInstances.Add(id, mat);
            return id;
        }

        private uint RequestInstanceId()
        {
            lock (mIdLock)
            {
                if (mFreeInstances.Count > 0)
                {
                    var ret = mFreeInstances.First();
                    mFreeInstances.RemoveAt(0);
                    mUsedInstances.Add(ret);
                    return ret;
                }

                if (mUsedInstances.Count > 0)
                {
                    var ret = mUsedInstances.Max() + 1;
                    mUsedInstances.Add(ret);
                    return ret;
                }

                mUsedInstances.Add(0);
                return 0;
            }
        }

        private WMOFile mFile = null;
        private Dictionary<uint, Matrix> mInstances = new Dictionary<uint, Matrix>();
        private List<uint> mFreeInstances = new List<uint>();
        private List<uint> mUsedInstances = new List<uint>();
        private object mIdLock = new object();
        private object mInstLock = new object();
    }
}
