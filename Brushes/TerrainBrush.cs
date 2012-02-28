using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace SharpWoW.Brushes
{
    public class TerrainBrush
    {
        public float InnerRadius { get; set; }
        public float InnerSharpness { get; set; }
        public float OuterRadius { get; set; }
        public float OuterSharpness { get; set; }

        public void CreateAsBitmap()
        {
            Bitmap bmp = new Bitmap(200, 200, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var data = bmp.LockBits(new Rectangle(0, 0, 200, 200), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            uint[] clr = new uint[200 * 200];
            for (int i = 0; i < 200; ++i)
            {
                for (int j = 0; j < 200; ++j)
                {
                    float x = ((j - 100) / 100.0f) * OuterRadius;
                    float y = ((i - 100) / 100.0f) * OuterRadius;
                    float dist = (float)Math.Sqrt(x * x + y * y);
                    float val = GetValueAtDistance(dist);
                    byte pct = (byte)(val * 255.0f);
                    clr[i * 200 + j] = 0xFF000000 + pct;
                }
            }

            Utils.Memory.CopyMemory(clr, data.Scan0);

            bmp.UnlockBits(data);
            bmp.Save("Brush.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
        }

        public float GetValueAtDistance(float distance)
        {
            if (distance > OuterRadius)
                return 0.0f;

            if (distance <= InnerRadius)
                return GetValueInner(distance);

            return GetValueOuter(distance);
        }

        private float GetValueInner(float distance)
        {
            return Lerp(1.0f, InnerSharpness, distance / InnerRadius);
        }

        private float GetValueOuter(float distance)
        {
            float fac = (distance - InnerRadius) / (OuterRadius - InnerRadius);
            if (fac < OuterSharpness)
                return InnerSharpness;

            return Lerp(InnerSharpness, 0, (fac - OuterSharpness) / (1.0f - OuterSharpness));
        }

        private float Lerp(float start, float end, float fac)
        {
            return start + (end - start) * fac;
        }
    }
}
