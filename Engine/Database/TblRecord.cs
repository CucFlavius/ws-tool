using ProjectWS.Engine.Data.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ProjectWS.Engine.Database
{
    public abstract class TblRecord
    {
        public abstract string GetFileName();
        public abstract uint GetID();

        public void Read<T>(FieldCache[] fields, T entry, BinaryReader br, long recordStart) where T : TblRecord
        {
            int idx = 0;
            int fieldCount = fields.Length;

            foreach (FieldCache f in fields)
            {
                if (f.IsArray)
                {
                    switch (f)
                    {
                        case FieldCache<T, byte[]> c1:
                            c1.Setter(entry, br.ReadBytes(c1.ArraySize));
                            break;
                        case FieldCache<T, short[]> c1:
                            c1.Setter(entry, ReadShortArray(br, c1.ArraySize));
                            break;
                        case FieldCache<T, ushort[]> c1:
                            c1.Setter(entry, ReadUshortArray(br, c1.ArraySize));
                            break;
                        case FieldCache<T, int[]> c1:
                            c1.Setter(entry, ReadIntArray(br, c1.ArraySize));
                            break;
                        default:
                            throw new Exception($"Unhandled ExTable type: {f.Field.FieldType.FullName} in {f.Field.DeclaringType.FullName}.{f.Field.Name}");
                    }
                }
                else
                {
                    switch (f)
                    {
                        case FieldCache<T, int> c1:
                            c1.Setter(entry, br.ReadInt32());
                            break;
                        case FieldCache<T, uint> c1:
                            c1.Setter(entry, br.ReadUInt32());
                            break;
                        case FieldCache<T, byte> c1:
                            c1.Setter(entry, br.ReadByte());
                            break;
                        case FieldCache<T, sbyte> c1:
                            c1.Setter(entry, br.ReadSByte());
                            break;
                        case FieldCache<T, short> c1:
                            c1.Setter(entry, br.ReadInt16());
                            break;
                        case FieldCache<T, ushort> c1:
                            c1.Setter(entry, br.ReadUInt16());
                            break;
                        case FieldCache<T, float> c1:
                            c1.Setter(entry, br.ReadSingle());
                            break;
                        case FieldCache<T, long> c1:
                            c1.Setter(entry, br.ReadInt64());
                            break;
                        case FieldCache<T, ulong> c1:
                            c1.Setter(entry, br.ReadUInt64());
                            break;
                        case FieldCache<T, string> c1:
                            uint num6 = br.ReadUInt32();
                            uint num7 = br.ReadUInt32();
                            long position = br.BaseStream.Position;
                            br.BaseStream.Position = (long)(((num6 != 0) ? num6 : num7) + recordStart);
                            string str = br.ReadWString();
                            c1.Setter(entry, str);
                            br.BaseStream.Position = position;

                            if (idx < fieldCount - 1)
                            {
                                if (num6 == 0 && fields[idx + 1].GetType() != typeof(string))
                                {
                                    br.BaseStream.Position += 4;
                                }
                            }
                            //c1.Setter(entry, ReadTableString(br, recordStart));
                            break;
                        default:
                            throw new Exception($"Unhandled EXTable type: {f.Field.FieldType.FullName} in {f.Field.DeclaringType.FullName}.{f.Field.Name}");
                    }
                }

                idx++;
            }
        }

        short[] ReadShortArray(BinaryReader br, int arraySize)
        {
            short[] data = new short[arraySize];
            for (int i = 0; i < arraySize; i++)
            {
                data[i] = br.ReadInt16();
            }
            return data;
        }

        ushort[] ReadUshortArray(BinaryReader br, int arraySize)
        {
            ushort[] data = new ushort[arraySize];
            for (int i = 0; i < arraySize; i++)
            {
                data[i] = br.ReadUInt16();
            }
            return data;
        }

        int[] ReadIntArray(BinaryReader br, int arraySize)
        {
            int[] data = new int[arraySize];
            for (int i = 0; i < arraySize; i++)
            {
                data[i] = br.ReadInt32();
            }
            return data;
        }

        string ReadTableString(BinaryReader br, long recordStart)
        {
            uint num6 = br.ReadUInt32();
            uint num7 = br.ReadUInt32();
            long position = br.BaseStream.Position;
            br.BaseStream.Position = (long)(((num6 != 0) ? num6 : num7) + recordStart);
            string str = br.ReadWString();
            br.BaseStream.Position = position;

            return str;
        }
    }


    /// <summary>
    /// Using this to get general access to Tbl sub structures without a type
    /// Because I don't know how C# works
    /// </summary>
    public class NoType : TblRecord
    {
        public override string GetFileName() => "nothing";
        public override uint GetID() => 0;
    }
}