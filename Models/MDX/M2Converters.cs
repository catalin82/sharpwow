using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SharpWoW.Models.MDX
{
    public class AnimInterpolator
    {
        public T Interpolate<T>(T val1, T val2, float pct)
        {
            if (!Methods.ContainsKey(val1.GetType()))
            {
                string methodName = "Interpolate" + val1.GetType().Name;
                bool found = false;
                foreach (var t in ExtensionTypes)
                {
                    if (t.GetMethod(methodName) != null)
                    {
                        found = true;
                        Methods.Add(val1.GetType(), t.GetMethod(methodName));
                        break;
                    }
                }
                if (found == false)
                    Methods.Add(val1.GetType(), null);
            }

            if (Methods[val1.GetType()] != null)
            {
                MethodInfo mi = Methods[val1.GetType()];
                return (T)mi.Invoke(null, new object[] { this, val1, val2, pct });
            }

            return val1;
        }

        public List<Type> ExtensionTypes = new List<Type>();
        Dictionary<Type, MethodInfo> Methods = new Dictionary<Type, MethodInfo>();
        public static AnimInterpolator Current = new AnimInterpolator();
    }

    public class AnimConverter
    {
        public virtual object Convert(object value)
        {
            return value;
        }

        public static AnimConverter Default { get; private set; }

        static AnimConverter()
        {
            Default = new AnimConverter();
        }
    }

    public class QuaternionConverter : AnimConverter
    {
        public bool UseFixed { get; set; }

        public override object Convert(object value)
        {
            Quaternion16 q16 = (Quaternion16)value;
            return (object)q16.ToQuaternion(UseFixed);
        }
    }

    public static class VectorInterpolator
    {
        public static Vector3 InterpolateVector3(this AnimInterpolator ap, Vector3 v1, Vector3 v2, float pct)
        {
            return Vector3.Lerp(v1, v2, pct);
        }

        public static Quaternion16 InterpolateQuaternion16(this AnimInterpolator ap, Quaternion16 v1, Quaternion16 v2, float pct)
        {
            short d1 = (short)(v2.x - v1.x);
            short d2 = (short)(v2.y - v1.y);
            short d3 = (short)(v2.z - v1.z);
            short d4 = (short)(v2.w - v1.w);

            Quaternion16 ret = new Quaternion16();
            ret.x = (short)(v1.x + (short)(d1 * pct));
            ret.y = (short)(v1.y + (short)(d2 * pct));
            ret.z = (short)(v1.z + (short)(d3 * pct));
            ret.w = (short)(v1.w + (short)(d4 * pct));

            return ret;
        }

        public static Quaternion InterpolateQuaternion(this AnimInterpolator ap, Quaternion v1, Quaternion v2, float pct)
        {
            return Quaternion.Slerp(v1, v2, pct);
        }

        public static void Init()
        {
            AnimInterpolator.Current.ExtensionTypes.Add(typeof(VectorInterpolator));
        }
    }

    public static class M2Converters
    {
        public static void Init()
        {
            VectorInterpolator.Init();
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Quaternion16
    {
        public short x, y, z, w;

        public Quaternion ToQuaternion(bool fix = false)
        {
            if (fix)
            {
                return new Quaternion(
                    -((float)(x < 0 ? x + 32768 : x - 32767) / 32767.0f),
                    -((float)(z < 0 ? z + 32768 : z - 32767) / 32767.0f),
                    -((float)(y < 0 ? y + 32768 : y - 32767) / 32767.0f),
                    +((float)(w < 0 ? w + 32768 : w - 32767) / 32767.0f)
                    );
            }
            else
            {
                return new Quaternion(
                    ((float)(x < 0 ? x + 32768 : x - 32767) / 32767.0f),
                    ((float)(y < 0 ? y + 32768 : y - 32767) / 32767.0f),
                    ((float)(z < 0 ? z + 32768 : z - 32767) / 32767.0f),
                    (float)(w < 0 ? w + 32768 : w - 32767) / 32767.0f
                    );
            }
        }
    }
}
