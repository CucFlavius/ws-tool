namespace ProjectWS.Engine.Data
{
    public class TimeTrackUnkBlock : TimeTrack<UnkBlock>
    {
        public TimeTrackUnkBlock(BinaryReader br, long startOffset)
        {
            long save = ReadTimeTrackCommon(br, startOffset);

            if (this.data != null)
            {
                // Read actual data
                for (uint i = 0; i < this.elements; i++)
                {
                    this.data[i] = new UnkBlock(br);
                }
            }

            br.BaseStream.Position = save;
        }
    }
}