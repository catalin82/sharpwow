using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpWoW.ADT
{
    public static class ADTAlphaHandler
    {
        private static object mLockObj = new object();
        private static List<SlimDX.Direct3D9.Texture> mFreeTextures = new List<SlimDX.Direct3D9.Texture>();

        public static SlimDX.Direct3D9.Texture FreeTexture()
        {
            lock (mLockObj)
            {
                if (mFreeTextures.Count == 0)
                    return null;

                var ret = mFreeTextures[0];
                mFreeTextures.RemoveAt(0);
                return ret;
            }
        }

        public static void AddFreeTexture(SlimDX.Direct3D9.Texture texture)
        {
            lock (mLockObj)
            {
                mFreeTextures.Add(texture);
            }
        }
    }
}
