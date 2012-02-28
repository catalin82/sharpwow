using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX;

namespace SharpWoW.World
{
    public enum ColorTableValues
    {
        GlobalDiffuse,
        GlobalAmbient,
        Color0,
        Color1,
        Color2,
        Color3,
        Color4,
        Fog,
        Unk1,
        SunColor,
        SunHaloColor,
        Unk2,
        CloudColor,
        Unk3,
        Unk4,
        WaterDark,
        WaterLight,
        Shadow
    }

    public class WorldLightEntry
    {
        public WorldLightEntry(DBC.Light light)
        {
            for (int i = 0; i < 18; ++i)
            {
                mColorTables.Add(new List<Vector3>());
                mTimeTables.Add(new List<uint>());
            }

            mParams = DBC.DBCStores.LightParams[light.skyParam];
            mLight = light;
            Position = new Vector3(mLight.x / 36.0f, mLight.y / 36.0f, mLight.z / 36.0f);
            InitColorTables();
        }

        private void InitColorTables()
        {
            uint baseIndex = mLight.skyParam * 18;
            for (int i = 0; i < 18; ++i)
            {
                DBC.LightIntBand lib = DBC.DBCStores.LightIntBand[baseIndex + (uint)i - 17];
                for (int j = 0; j < lib.NumEntries; ++j)
                {
                    mColorTables[i].Add(ToVector(lib.Values[j]));
                    mTimeTables[i].Add(lib.Times[j]);
                }
            }
        }

        public Vector3 GetColorForTime(ColorTableValues table, float time = -1)
        {
            int idx = (int)table;
            if (time < 0)
                time = (float)((Game.GameManager.GameTime.TotalMilliseconds / 10.0f) % 2880);
            else
                time %= 2880;
            List<uint> timeValues = mTimeTables[(int)table];
            if (timeValues.Count == 0)
                return Vector3.Zero;

            if (timeValues[0] > time)
                time = timeValues[0];

            if (timeValues.Count == 1)
                return mColorTables[(int)table][0];

            Vector3 v1 = Vector3.Zero, v2 = Vector3.Zero;
            uint t1 = 0, t2 = 0;

            for (int i = 0; i < mTimeTables[(int)table].Count; ++i)
            {
                if (i + 1 >= mTimeTables[(int)table].Count)
                {
                    v1 = mColorTables[idx][i];
                    v2 = mColorTables[idx][0];
                    t1 = mTimeTables[idx][i];
                    t2 = mTimeTables[idx][0] + 2880;
                    break;
                }

                var ts = mTimeTables[(int)table][i];
                var te = mTimeTables[idx][i + 1];
                if (ts <= time && te >= time)
                {
                    t1 = ts;
                    t2 = te;
                    v1 = mColorTables[idx][i];
                    v2 = mColorTables[idx][i + 1];
                    break;
                }
            }

            uint diff = t2 - t1;
            if (diff == 0)
                return v1;

            float sat = (time - t1) / diff;
            return (v1 + sat * (v2 - v1));
        }

        public void SetNewTables(ColorTableValues table, List<uint> times, List<Vector3> colors)
        {
            mTimeTables[(int)table] = times;
            mColorTables[(int)table] = colors;

            DBC.LightIntBand band = DBC.DBCStores.LightIntBand[mLight.skyParam * 18 + (uint)table - 17];
            band.NumEntries = (uint)times.Count;
            for (int i = 0; i < times.Count; ++i)
            {
                band.Times[i] = times[i];
                band.Values[i] = ToUint(colors[i]);
            }
        }

        public bool IsGlobal { get { return mLight.x == 0 && mLight.y == 0 && mLight.z == 0; } }
        public float InnerRadius { get { return mLight.falloff / 36.0f; } }
        public float OuterRadius { get { return mLight.falloffEnd / 36.0f; } }
        public Vector3 Position { get; private set; }
        public uint ID { get { return mLight.ID; } }

        public static Vector3 ToVector(uint value)
        {
            return new Vector3(((value >> 16) & 0xFF) / 255.0f, ((value >> 8) & 0xFF) / 255.0f, ((value >> 0) & 0xFF) / 255.0f);
        }

        public static uint ToUint(Vector3 value)
        {
            uint r = (uint)(value.X * 255.0f);
            uint g = (uint)(value.Y * 255.0f);
            uint b = (uint)(value.Z * 255.0f);

            return (r << 16) | (g << 8) | (b << 0);
        }

        private DBC.Light mLight;
        private DBC.LightParams mParams;
        private List<List<Vector3>> mColorTables = new List<List<Vector3>>(18);
        private List<List<uint>> mTimeTables = new List<List<uint>>(18);

        public List<Vector3> GetColorLine(ColorTableValues table) { return mColorTables[(int)table]; }
        public List<uint> GetTimeLine(ColorTableValues table) { return mTimeTables[(int)table]; }

        public DBC.LightParams LightParams { get { return mParams; } }
        public DBC.Light LightEntry { get { return mLight; } }
    }
}
