using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SharpWoW.DBC
{
    public class DBCWriter<T> where T : new()
    {
        public void WriteDBC(DBCFile<T> file)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(file.FileName));
            mFile = file;

            FileStream strm = File.OpenWrite(file.FileName);
            BinaryWriter bw = new BinaryWriter(strm);
            byte[] bytes = Encoding.UTF8.GetBytes("WDBC");
            bw.Write(bytes);
            bw.Write(file.Records.Count);
            Type type = typeof(T);
            var fields = type.GetFields();
            int fieldCount = 0;
            foreach (var field in fields)
            {
                switch (Type.GetTypeCode(field.FieldType))
                {
                    case TypeCode.String:
                        {
                            var attribs = field.GetCustomAttributes(typeof(LocalizedAttribute), false);
                            if (attribs == null || attribs.Length == 0)
                                ++fieldCount;
                            else
                                fieldCount += 17;
                            break;
                        }

                    case TypeCode.Object:
                        {
                            if (field.FieldType.IsArray)
                            {
                                var attribs = field.GetCustomAttributes(typeof(ArrayAttribute), false);
                                if (attribs == null || attribs.Length == 0)
                                    throw new InvalidOperationException("For arrays the [Array] attribute must set with the desired size of the array!");

                                fieldCount += (int)(attribs[0] as ArrayAttribute).Length;
                            }
                        }
                        break;

                    default:
                        ++fieldCount;
                        break;
                }
            }

            bw.Write(fieldCount);
            bw.Write(fieldCount * 4);
            bw.Write(0);

            foreach (var rec in file.Records)
            {
                foreach (var field in fields)
                {
                    switch (Type.GetTypeCode(field.FieldType))
                    {
                        case TypeCode.Int32:
                            {
                                int value = (int)field.GetValue(rec);
                                bw.Write(value);
                                break;
                            }

                        case TypeCode.UInt32:
                            {
                                uint uvalue = (uint)field.GetValue(rec);
                                bw.Write(uvalue);
                                break;
                            }

                        case TypeCode.String:
                            {
                                var attribs = field.GetCustomAttributes(typeof(LocalizedAttribute), false);
                                if (attribs.Length == 0)
                                {
                                    string str = field.GetValue(rec) as string;
                                    bw.Write(AddStringToTable(str));
                                }
                                else
                                {
                                    int pos = AddStringToTable(field.GetValue(rec) as string);
                                    for (uint j = 0; j < file.LocalePosition; ++j)
                                    {
                                        bw.Write((int)0);
                                    }

                                    bw.Write(pos);
                                    for (uint j = file.LocalePosition + 1; j < 17; ++j)
                                        bw.Write((int)0);
                                }
                                break;
                            }

                        case TypeCode.Single:
                            {
                                float fvalue = (float)field.GetValue(rec);
                                bw.Write(fvalue);
                                break;
                            }

                        case TypeCode.Object:
                            {
                                // Info: Checks if type is array already made where numFields is calculated.
                                Type atype = field.FieldType.GetElementType();
                                var attribs = field.GetCustomAttributes(typeof(ArrayAttribute), false);
                                int len = (int)(attribs[0] as ArrayAttribute).Length;
                                Array array = field.GetValue(rec) as Array;
                                for (int q = 0; q < len; ++q)
                                {
                                    switch (Type.GetTypeCode(atype))
                                    {
                                        case TypeCode.Int32:
                                            bw.Write((int)array.GetValue(q));
                                            break;

                                        case TypeCode.UInt32:
                                            bw.Write((uint)array.GetValue(q));
                                            break;

                                        case TypeCode.Single:
                                            bw.Write((float)array.GetValue(q));
                                            break;
                                    }
                                }
                            }
                            break;
                    }
                }
            }

            foreach (var str in mStringTable.Values)
            {
                bytes = Encoding.UTF8.GetBytes(str);
                bw.Write(bytes);
                bw.Write((byte)0);
            }

            bw.BaseStream.Position = 16;
            if (mStringTable.Count > 0)
                bw.Write(mStringTable.Last().Key + Encoding.UTF8.GetByteCount(mStringTable.Last().Value) + 1);
        }

        private int AddStringToTable(string str)
        {
            if (str == null)
                str = "";

            int strHash = str.GetHashCode();

            foreach (var pair in mStringTable)
                if (pair.Value.GetHashCode() == strHash)
                    return pair.Key;

            int myPos = (mStringTable.Count == 0) ? 0 : (mStringTable.Last().Key + Encoding.UTF8.GetByteCount(mStringTable.Last().Value) + 1);
            mStringTable.Add(myPos, str);
            return myPos;
        }

        private Dictionary<int, string> mStringTable = new Dictionary<int, string>();
        private DBCFile<T> mFile;
    }
}
