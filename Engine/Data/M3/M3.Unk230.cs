using System.IO;

namespace ProjectWS.Engine.Data
{
    public class Unk230 : ArrayData
    {
        public byte[]? data;

        public override int GetSize()
        {
            return 4;
        }

        public override void Read(BinaryReader br, long startOffset)
        {
            this.data = br.ReadBytes(4);
        }
    }
}