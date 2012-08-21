using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;
using SlimDX.Direct3D9;

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
            lock (mInstLock)
            {
                foreach (var mat in mInstances.Values)
                {
                    if (Game.GameManager.GraphicsThread.GraphicsManager.Camera.ViewFrustum.Contains(mFile.BoundingBox, mat) == ContainmentType.Disjoint)
                        return;

                    mFile.Draw(mat);
                }
            }
        }

        public bool IsInstanceHit(out float nearestHit, out uint nearestInstance, out uint refId, out Vector3 pos, out Matrix modelMatrix)
        {
            nearestHit = 0;
            nearestInstance = 0;
            refId = 0;
            float curNear = 99999;
            bool hasHit = false;
            pos = Vector3.Zero;
            modelMatrix = Matrix.Identity;

            lock (mInstLock)
            {
                foreach (var mat in mInstances)
                {
                    var ray = Video.Picking.CalcRayForTransform(mat.Value);
                    float curHit = 0;
                    if (mFile.Intersects(ray, out curHit))
                    {
                        hasHit = true;
                        if (curHit < curNear)
                        {
                            curNear = curHit;
                            nearestInstance = mUniqueId[mat.Key];
                            refId = mat.Key;
                            pos = Vector3.TransformCoordinate(ray.Position + curHit * ray.Direction, mat.Value);
                            modelMatrix = mat.Value;
                        }
                    }
                }
            }

            nearestHit = curNear;
            return hasHit;
        }

        public uint PushInstance(uint unique, Vector3 pos, Vector3 rotation)
        {
            lock (mUniqueLock)
            {
                if (mUniqueId.ContainsValue(unique))
                    return 0xFFFFFFFF;
            }

            Matrix matRot = Matrix.RotationX(Utils.SharpMath.mirrorAngle(rotation.X) * 0.017453f);
            matRot *= Matrix.RotationY(Utils.SharpMath.mirrorAngle(rotation.Z) * 0.017453f);
            matRot *= Matrix.RotationZ(Utils.SharpMath.mirrorAngle(rotation.Y) * 0.017453f + (float)Math.PI / 2.0f);

            Matrix mat = matRot * Matrix.Translation(pos);
            uint id = RequestInstanceId();
            lock (mInstLock) mInstances.Add(id, mat);
            lock (mUniqueLock) mUniqueId.Add(id, unique);
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

        public WMOFile File { get { return mFile; } }

        private WMOFile mFile = null;
        private Dictionary<uint, Matrix> mInstances = new Dictionary<uint, Matrix>();
        private List<uint> mFreeInstances = new List<uint>();
        private List<uint> mUsedInstances = new List<uint>();
        private object mIdLock = new object();
        private object mInstLock = new object();
        private object mUniqueLock = new object();
        private Dictionary<uint, uint> mUniqueId = new Dictionary<uint, uint>();
    }
}
