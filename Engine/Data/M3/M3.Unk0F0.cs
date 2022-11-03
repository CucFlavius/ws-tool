using System.IO;

namespace ProjectWS.Engine.Data
{
    public partial class M3
    {
        public class Unk0F0 : ArrayData
        {
            public short[] unks;

            public override void Read(BinaryReader br, long startOffset)
            {
                this.unks = new short[92];
                for (int i = 0; i < 92; i++)
                {
                    this.unks[i] = br.ReadInt16();
                }
            }
        }
    }
}