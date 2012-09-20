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
        public abstract bool Intersect(SlimDX.Ray ray, ref float dist, out IADTChunk hitChunk);
        public abstract void ChangeTerrain(Vector3 pos, bool lower);
        public abstract void FlattenTerrain(SlimDX.Vector3 pos, bool lower);
        public abstract void BlurTerrain(Vector3 pos, bool lower);
        public abstract void addModel(string name, Vector3 pos);
        public abstract Wotlk.MCLY getLayer(int index);
        public abstract string getLayerTexture(Wotlk.MCLY ly);

        public Wotlk.MCNK Header { get { return mHeader; } }

        protected Wotlk.MCNK mHeader;
    }
}
