namespace ProjectWS.Engine.Data
{
    public class TimeTrackAngleABAndColor : TimeTrack<AngleABAndColor>
    {
        public TimeTrackAngleABAndColor(BinaryReader br, long startOffset)
        {
            long save = ReadTimeTrackCommon(br, startOffset);

            if (this.data != null)
            {
                // Read actual data
                for (uint i = 0; i < this.elements; i++)
                {
                    this.data[i] = new AngleABAndColor(br);
                }
            }

            br.BaseStream.Position = save;
        }
    }
}