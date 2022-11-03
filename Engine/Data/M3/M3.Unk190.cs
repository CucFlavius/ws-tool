using System.IO;

namespace ProjectWS.Engine.Data
{
    public class Unk190 : ArrayData
    {
        public short value;

        public override void Read(BinaryReader br, long startOffset)
        {
            this.value = br.ReadInt16();
        }
    }
}