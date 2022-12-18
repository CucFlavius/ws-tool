using System.IO;

namespace ProjectWS.Engine.Data.M3
{
    public class Unk240 : ArrayData
    {
        public byte[]? data;

        public override int GetSize()
        {
            return 112;
        }

        public override void Read(BinaryReader br, long startOffset)
        {
            this.data = br.ReadBytes(112);
        }
    }
}