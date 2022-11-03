using System.IO;

namespace ProjectWS.Engine.Data
{
    public partial class M3
    {
        public class Bounds : ArrayData
        {
            public short[] unkShorts;
            public BoundingBox bbA;
            public BoundingBox bbB;

            public override void Read(BinaryReader br, long startOffset)
            {
                this.unkShorts = new short[10];
                for (int i = 0; i < 10; i++)
                {
                    this.unkShorts[i] = br.ReadInt16();
                }
                br.BaseStream.Position += 12;   // Padding ??
                this.bbA = new BoundingBox(br);
                this.bbB = new BoundingBox(br);
                br.BaseStream.Position += 16;   // Padding ??
            }
        }
    }
}