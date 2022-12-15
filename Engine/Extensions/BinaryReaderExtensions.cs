using OpenTK;
using OpenTK.Mathematics;
using System;
using System.IO;
using System.Text;

namespace ProjectWS.Engine.Data.Extensions
{
    public static class BinaryReaderExtensions
    {
        public static Vector3 ReadVector3(this BinaryReader br, bool flipZ = true)
        {
            return new Vector3(br.ReadSingle(), br.ReadSingle(), flipZ ? -br.ReadSingle() : br.ReadSingle());
        }

        public static Vector3 ReadVector3_16Bit(this BinaryReader br)
        {
            return new Vector3(br.ReadInt16(), br.ReadInt16(), -br.ReadInt16()) / 1024.0f;
        }

        public static Vector3 ReadVector3_8BitNormalized(this BinaryReader br)
        {
            float x0 = (br.ReadByte() - 127) / 127.0f;
            float y0 = (br.ReadByte() - 127) / 127.0f;
            float z0 = (float)Math.Sqrt((x0 * x0) + (y0 * y0));
            return new Vector3(x0, y0, 1.0f - z0);
        }

        public static Quaternion ReadQuaternion(this BinaryReader br, bool normalize = false)
        {
            Quaternion q = new Quaternion(br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
            if (normalize)
            {
                float f = 1f / (float)Math.Sqrt(q.X * q.X + q.Y * q.Y + q.Z * q.Z + q.W * q.W);
                return new Quaternion(q.X * f, q.Y * f, q.Z * f, q.W * f);
            }

            return q;
        }

        public static Color4 ReadColor32(this BinaryReader br)
        {
            return new Color4(br.ReadByte() / 255f, br.ReadByte() / 255f, br.ReadByte() / 255f, br.ReadByte() / 255f);
        }

        public static Vector4 ReadColor(this BinaryReader br)
        {
            return new Vector4(br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
        }

        public static Vector4 ReadVector4(this BinaryReader br)
        {
            return new Vector4(br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
        }


        public static string ReadWString(this BinaryReader br)
        {
            byte[] array = new byte[0];
            for (byte b = br.ReadByte(); b != 0; b = br.ReadByte())
            {
                br.BaseStream.Position -= 1L;
                if (br.BaseStream.Position + 2 >= br.BaseStream.Length)
                    break;
                array = array.Combine(br.ReadBytes(2));
            }
            return Encoding.Unicode.GetString(array);
        }

        public static byte[] Combine(this byte[] data, byte[] data2)
        {
            byte[] array = new byte[data.Length + data2.Length];
            Buffer.BlockCopy(data, 0, array, 0, data.Length);
            Buffer.BlockCopy(data2, 0, array, data.Length, data2.Length);
            return array;
        }

        public static string ReadString(this BinaryReader br, int count, bool unicode = true)
        {
            byte[] bytes = br.ReadBytes(unicode ? (count << 1) : count);
            if (!unicode)
                return Encoding.ASCII.GetString(bytes);
            return Encoding.Unicode.GetString(bytes);
        }

        public static string ReadChunkID(this BinaryReader br)
        {
            char[] c = br.ReadChars(4);
            return $"{c[3]}{c[2]}{c[1]}{c[0]}";
        }

        public static void SkipChunk(this BinaryReader br, string chunkID, int chunkSize, string type)
        {
            Debug.LogWarning($"{type} | Skipping chunk : {chunkID}");
            br.BaseStream.Seek(chunkSize, SeekOrigin.Current);
        }

        public static void Align(this BinaryReader br, int alignment = 4)
        {
            int loc = (int)br.BaseStream.Position;
            int pad = (alignment - (loc % alignment));
            if (pad != 0 && pad < alignment)
            {
                br.BaseStream.Position += pad;
            }
        }
    }
}
