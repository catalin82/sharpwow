using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using MathNet.Numerics.Interpolation;

namespace SharpWoW.Brushes
{
    internal class SprayBrush : IBrush
    {
        public SprayBrush(uint width, uint height, bool useByte)
        {
        }

        public SprayBrush()
        {
        }

        public override void SetProperty<T>(string property, T value)
        {
            GetType().GetProperty(property).SetValue(this, value);
        }

        public override void SetProperty<T>(BrushProperties property, T value)
        {
            SetProperty<T>(property.ToString(), value);
        }

        public override T GetProperty<T>(string property)
        {
            return (T)GetType().GetProperty(property).GetValue(this);
        }

        public override T GetProperty<T>(BrushProperties property)
        {
            return GetProperty<T>(property.ToString());
        }

        public override double[] GetDataDouble()
        {
            if (mDataByte == null && mDataDouble == null)
                throw new InvalidOperationException("You need to call GenerateData first!");

            if (mDataDouble != null)
                return mDataDouble;

            double[] conv = new double[mDataByte.Length];
            for (int i = 0; i < mDataByte.Length; ++i)
                mDataDouble[i] = (double)mDataByte[i] / 255.0;

            mDataDouble = conv;
            return conv;
        }

        public override byte[] GetDataByte()
        {
            if (mDataByte == null && mDataDouble == null)
                throw new InvalidOperationException("You need to call GenerateData first!");

            if (mDataByte != null)
                return mDataByte;

            byte[] conv = new byte[mDataDouble.Length];
            for (int i = 0; i < mDataDouble.Length; ++i)
                conv[i] = (byte)(255.0 * mDataDouble[i]);

            mDataByte = conv;
            return conv;
        }

        public override void GenerateData()
        {
            if (UseByte)
                GenerateDataByte();
            else
                GenerateDataDouble();
        }

        private void GenerateDataByte()
        {
            if (InterpolationSpline == null)
                InterpolationSpline = Interpolation.CreateRational(new double[] { 0, 0.5, 1 }, new double[] { 1, FallOff, 0 });
            mDataByte = new byte[Width * Height];
            PointF midPoint = new PointF(Width / 2.0f, Height / 2.0f);
            
            for (uint i = 0; i < Width; ++i)
            {
                for (uint j = 0; j < Height; ++j)
                {
                    byte value = 0;
                    PointF curPos = new PointF(i, j);

                    double distance = Math.Sqrt(Math.Pow(curPos.X - midPoint.X, 2) + Math.Pow(curPos.Y - midPoint.Y, 2));
                    if(distance < InnerRadius)
                    {
                        double coeff = InterpolationSpline.Interpolate(distance / InnerRadius);
                        if (coeff < 0)
                            coeff = 0;

                        int curValue = (int)(mRandom.Next(0, 255) * coeff);

                        value = (byte)curValue;
                    }

                    mDataByte[j * Height + i] = value;
                }
            }
        }

        private void GenerateDataDouble()
        {
            mDataDouble = new double[Width * Height];
            PointF midPoint = new PointF(Width / 2.0f, Height / 2.0f);

            for (uint i = 0; i < Width; ++i)
            {
                for (uint j = 0; j < Height; ++j)
                {
                    byte value = 0;
                    PointF curPos = new PointF(i, j);

                    double distance = Math.Sqrt(Math.Pow(curPos.X - midPoint.X, 2) + Math.Pow(curPos.Y - midPoint.Y, 2));
                    if (distance < InnerRadius)
                    {
                        int curValue = (int)(mRandom.Next(0, 100) * (InnerRadius - distance));

                        value = (byte)((curValue >= (100 * CutOff)) ? 1 : 0);
                    }

                    mDataByte[j * Height + Width] = value;
                }
            }
        }

        public double InnerRadius { get; set; }
        public double CutOff { get; set; }
        public double FallOff { get; set; }
        public IInterpolationMethod InterpolationSpline { get; set; }

        private Random mRandom = new Random();
    }
}
