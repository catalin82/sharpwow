using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace SharpWoW
{
    public static class BinaryWriterExtension
    {
        public static void WriteUTF8String(this BinaryWriter writer, string str)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            writer.Write(bytes);
            writer.Write((byte)0);
        }

        public static unsafe void WriteStruct<T>(this BinaryWriter writer, T value) where T : struct
        {
            int size = Marshal.SizeOf(value);
            byte[] bytes = new byte[size];

            fixed (byte* ptr = bytes)
            {
                Marshal.StructureToPtr(value, (IntPtr)ptr, false);
            }

            writer.Write(bytes);
        }
    }
}
