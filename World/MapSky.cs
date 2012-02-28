using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX;

namespace SharpWoW.World
{
    public class MapSky
    {
        public MapSky(uint mapId)
        {
            for (int i = 0; i < 18; ++i)
                mColors.Add(Vector3.Zero);

            foreach (var light in DBC.DBCStores.Light.Records)
            {
                if (light.MapID == mapId)
                    mLights.Add(new WorldLightEntry(light));
            }

            SortLights();

            foreach (var wle in mLights)
                mLightEntries.Add(new VisualLightEntry(wle));
        }

        public void UpdateSky(Vector3 position)
        {
            float[] weights = new float[mLights.Count];
            CalculateWeights(position, weights);

            for (int i = 0; i < 18; ++i)
                mColors[i] = new Vector3(1, 1, 1);

            uint j = 0;
            foreach (var light in mLights)
            {
                if (weights[j] > 0)
                {
                    for (int k = 0; k < 18; ++k)
                        mColors[k] += light.GetColorForTime((ColorTableValues)k) * weights[j];
                }

                ++j;
            }

            for (int i = 0; i < 18; ++i)
                mColors[i] -= new Vector3(1, 1, 1);
        }

        public void AddNewLight(WorldLightEntry wle)
        {
            mLights.Add(wle);
            SortLights();
            foreach (var wlee in mLights)
                mLightEntries.Add(new VisualLightEntry(wlee));
        }

        private void SortLights()
        {
            mLights.Sort(
               (l1, l2) =>
               {
                   if (l1.IsGlobal && l2.IsGlobal)
                       return 0;

                   if (l1.IsGlobal)
                       return -1;

                   if (l2.IsGlobal)
                       return 1;

                   if (l1.OuterRadius > l2.OuterRadius)
                       return 1;

                   if (l2.OuterRadius > l1.OuterRadius)
                       return -1;

                   if (l2.OuterRadius == l1.OuterRadius)
                       return 0;

                   throw new InvalidOperationException();
               }
           );
        }
        private void CalculateWeights(Vector3 pos, float[] w)
        {
            List<int> globals = new List<int>();

            for (int i = w.Length - 1; i > -1; --i)
            {
                var le = mLights[i];
                if (le.IsGlobal)
                {
                    globals.Add(i);
                    continue;
                }

                var dist = (new Vector3(pos.X, pos.Z, pos.Y) - le.Position).Length();
                if (dist < le.InnerRadius)
                {
                    w[i] = 1;
                    for (int j = i + 1; j < w.Length; ++j)
                        w[j] = 0;
                }
                else if (dist < le.OuterRadius)
                {
                    float sat = (dist - le.InnerRadius) / (le.OuterRadius - le.InnerRadius);
                    w[i] = 1 - sat;
                    for (int j = i + 1; j < w.Length; ++j)
                        w[j] *= sat;
                }
                else
                    w[i] = 0.0f;
            }

            float totalW = 0.0f;
            for (int i = 0; i < w.Length; ++i)
                totalW += w[i];

            if (totalW == 1)
            {
                for (int i = 0; i < w.Length; ++i)
                    mLightEntries[i].Weight = w[i];
                return;
            }

            float perGlobalW = (1.0f - totalW) / globals.Count;
            foreach (var glob in globals)
                w[glob] = perGlobalW;

            for (int i = 0; i < w.Length; ++i)
                mLightEntries[i].Weight = w[i];
        }

        public Vector3 GetColorEntry(ColorTableValues table)
        {
            return mColors[(int)table];
        }

        private List<WorldLightEntry> mLights = new List<WorldLightEntry>();
        private List<Vector3> mColors = new List<Vector3>();

        private List<VisualLightEntry> mLightEntries = new List<VisualLightEntry>();

        public List<VisualLightEntry> VisualLightEntries { get { return mLightEntries; } }
    }

    public class VisualLightEntry
    {
        public VisualLightEntry(WorldLightEntry wle)
        {
            lightEntry = wle;
            Weight = 0.0f;
        }

        public override string ToString()
        {
            return lightEntry.ID.ToString() + " - Weight: " + Weight;
        }

        private WorldLightEntry lightEntry;

        public float Weight { get; set; }
        public uint ID { get { return lightEntry.ID; } }

        public WorldLightEntry Entry { get { return lightEntry; } }
    }
}
