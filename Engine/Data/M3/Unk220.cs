using System.IO;

namespace ProjectWS.Engine.Data.M3
{
    public class Unk220 : ArrayData
    {
        public byte[]? data;

        public override int GetSize()
        {
            return 70;
        }

        public override void Read(BinaryReader br, long startOffset)
        {
            this.data = br.ReadBytes(70);
        }
    }
}