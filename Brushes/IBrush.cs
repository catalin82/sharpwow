using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpWoW.Brushes
{
    internal abstract class IBrush
    {
        public abstract void SetProperty<T>(BrushProperties property, T value);
        public abstract void SetProperty<T>(string property, T value);
        public abstract T GetProperty<T>(string property);
        public abstract T GetProperty<T>(BrushProperties property);
        public abstract double[] GetDataDouble();
        public abstract byte[] GetDataByte();
        public uint Width { get; set; }
        public uint Height { get; set; }
        public bool UseByte { get; set; }
        public abstract void GenerateData();

        protected double[] mDataDouble;
        protected byte[] mDataByte;
    }

    enum BrushProperties
    {
        Width,
        Height,
        UseByte
    }
}
