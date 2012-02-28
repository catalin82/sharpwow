using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX;

namespace SharpWoW.ADT
{
    public abstract class IADTChunk
    {
        public abstract bool GetLandHeightFast(float x, float y, out float h);
        public abstract bool Intersect(SlimDX.Ray ray, ref float dist);
        public abstract void ChangeTerrain(Vector3 pos, bool lower);
        public abstract void FlattenTerrain(SlimDX.Vector3 pos, bool lower);
        public abstract void BlurTerrain(Vector3 pos, bool lower);

        public Wotlk.MCNK Header { get { return mHeader; } }

        protected Wotlk.MCNK mHeader;
    }
}
