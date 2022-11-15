namespace ProjectWS.Engine.Data
{
    public partial class Area
    {
        public partial class SubChunk
        {
            public class SkyCorner
            {
                public uint[] worldSkyIDs;
                public byte[] worldSkyWeights;

                public SkyCorner(BinaryReader br)
                {
                    this.worldSkyIDs = new uint[4] { br.ReadUInt32(), br.ReadUInt32(), br.ReadUInt32(), br.ReadUInt32() };
                }

                public void ReadWeights(BinaryReader br)
                {
                    this.worldSkyWeights = br.ReadBytes(4);
                }
            }
        }
    }
}
