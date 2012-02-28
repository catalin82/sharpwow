using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;

namespace SharpWoW.Generators
{
    public class PerlinGenerator
    {
        public PerlinGenerator()
        {
            PerlinSeed = mRandom.Next();
        }

        double Noise(double x, double y)
        {
            int n = (int)x + (int)y * PerlinSeed;
            n = (n << 13) ^ n;
            int nn = (n * (n * n * 60493 + 19990303) + 1376312589) & 0x7fffffff;
            return 1.0 - ((double)nn / 1073741824.0);
        }

        double SmoothNoise(double x, double y)
        {
            double corners = (Noise(x - 1, y - 1) + Noise(x + 1, y - 1) + Noise(x - 1, y + 1) + Noise(x + 1, y + 1)) / 16;
            double sides = (Noise(x - 1, y) + Noise(x + 1, y) + Noise(x, y - 1) + Noise(x, y + 1)) / 8;
            double center = Noise(x, y) / 4;
            return corners + sides + center;
        }

        double CosinusInterpolate(double a, double b, double x)
        {
            double ft = x * 3.1415927;
            double f = (1.0 - Math.Cos(ft)) * 0.5;
            return a * (1.0 - f) + b * f;
        }

        double InterpolateNoise(double x, double y)
        {
            int integer_X = (int)x;
            double fractional_X = x - integer_X;

            int integer_Y = (int)y;
            double fractional_Y = y - integer_Y;

            double v1 = SmoothNoise(integer_X, integer_Y);
            double v2 = SmoothNoise(integer_X + 1, integer_Y);
            double v3 = SmoothNoise(integer_X, integer_Y + 1);
            double v4 = SmoothNoise(integer_X + 1, integer_Y + 1);

            double i1 = CosinusInterpolate(v1, v2, fractional_X);
            double i2 = CosinusInterpolate(v3, v4, fractional_X);

            return CosinusInterpolate(i1, i2, fractional_Y);
        }

        double GetPerlinNoise(double x, double y, double zoom)
        {
            double getnoise = 0;
            double p = Persistance;
            for (int a = 0; a < Octaves - 1; a++)
            {
                double frequency = Math.Pow(2, a);
                double amplitude = Math.Pow(p, a);
                getnoise += InterpolateNoise(x * frequency / zoom, y * frequency / zoom) * amplitude;
            }
            return getnoise;
        }

        public Bitmap CreatePerlinNoise()
        {
            int[] bmpData = new int[PerlinHeight * PerlinWidth];
            Bitmap bmp = new Bitmap(PerlinWidth, PerlinHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            double zoom = PerlinHeight / Zoom;
            for (int y = 0; y < PerlinHeight; ++y)
            {
                for (int x = 0; x < PerlinWidth; ++x)
                {
                    double value = GetPerlinNoise(x, y, zoom);
                    if (value > 0.999)
                        value = 0.999;
                    if (value < -1.0)
                        value = -1.0;
                    byte clr = (byte)((value * 128) + 128);
                    clr = Math.Max((byte)20, Math.Min(clr, (byte)220));
                    int color = (0xFF << 24) | (clr << 16) | (clr << 8) | (clr);
                    bmpData[y * PerlinWidth + x] = color;
                    NoiseValues[y * PerlinWidth + x] = (clr / 255.0f);
                }
            }

            Rectangle rect = new Rectangle(0, 0, PerlinWidth, PerlinHeight);
            BitmapData data = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            Marshal.Copy(bmpData, 0, data.Scan0, PerlinWidth * PerlinHeight);
            bmp.UnlockBits(data);
            Image = bmp;
            return bmp;
        }

        public double GetNoiseValue(double facX, double facY)
        {
            int posRel = (int)Math.Floor(facX * PerlinWidth);
            int posRelY = (int)Math.Floor(facY * PerlinHeight);
            return NoiseValues[posRelY * PerlinWidth + posRel];
        }


        public int PerlinSeed { get; private set; }
        public int PerlinWidth { get; set; }
        public int PerlinHeight { get; set; }
        public double[] NoiseValues { get; private set; }
        public int Octaves { get; set; }
        public double Persistance { get; set; }
        public int Zoom { get; set; }
        public Bitmap Image { get; private set; }

        private Random mRandom = new Random();
    }
}
