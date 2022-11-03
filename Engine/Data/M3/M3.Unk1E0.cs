using System.IO;

namespace ProjectWS.Engine.Data
{
    public class Unk1E0 : ArrayData
    {
        public byte[] data;

        public override void Read(BinaryReader br, long startOffset)
        {
            this.data = br.ReadBytes(152);
        }
    }
}