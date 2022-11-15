using OpenTK.Mathematics;

namespace ProjectWS.Engine.Data
{
    public class SkyDataBlock
    {
        public long unk;
        public TimeTrack<Vector4>[]? colorUnk0;
        public TimeTrack<AngleAndColor>[]? colorAndAngleUnk0;
        public TimeTrack<AngleAndColorAB>[]? unkColorABAngle;  // Gradient2Block
        public TimeTrack<AngleABAndColor>[]? angleABAndColor;  // UnkColorBlock3
        public TimeTrack<UnkBlock>[]? unk7; // UnkColorBlock7 todo
        public TimeTrack<Gradient16>? skySphereGradient;    // Gradient16Block 

        public SkyDataBlock(BinaryReader br, long startOffs)
        {
            this.unk = br.ReadInt64();
            this.colorUnk0 = ReadTimeTrackColorArray(br, startOffs);
            this.colorAndAngleUnk0 = ReadTimeTrackAngleAndColorArray(br, startOffs);
            this.unkColorABAngle = ReadTimeTrackGradient2Array(br, startOffs);
            br.BaseStream.Position += 16;       // Skipping unused array
            this.angleABAndColor = ReadTimeTrackUnkColorBlock3Array(br, startOffs);
            this.unk7 = ReadTimeTrackUnkColorBlock7Array(br, startOffs);
            br.BaseStream.Position += 24;       // Skipping unused array
            this.skySphereGradient = new TimeTrackGradient16(br, startOffs);
        }

        TimeTrack<Vector4>[] ReadTimeTrackColorArray(BinaryReader br, long startOffs)
        {
            var elements = br.ReadUInt32();
            br.BaseStream.Position += 4;        // Gap
            var offset = br.ReadInt64();
            var save = br.BaseStream.Position;
            br.BaseStream.Position = startOffs + offset;
            var colorArray = new TimeTrack<Vector4>[elements];

            for (int t = 0; t < elements; t++)
            {
                colorArray[t] = new TimeTrackVector4(br, br.BaseStream.Position + 24);
            }

            br.BaseStream.Position = save;
            return colorArray;
        }

        TimeTrack<AngleAndColor>[] ReadTimeTrackAngleAndColorArray(BinaryReader br, long startOffs)
        {
            var elements = br.ReadUInt32();
            br.BaseStream.Position += 4;        // Gap
            var offset = br.ReadInt64();
            var save = br.BaseStream.Position;
            br.BaseStream.Position = startOffs + offset;
            var colorArray = new TimeTrack<AngleAndColor>[elements];

            for (int t = 0; t < elements; t++)
            {
                colorArray[t] = new TimeTrackAngleAndColor(br, br.BaseStream.Position + 24);
            }

            br.BaseStream.Position = save;
            return colorArray;
        }

        TimeTrack<AngleAndColorAB>[] ReadTimeTrackGradient2Array(BinaryReader br, long startOffs)
        {
            var elements = br.ReadUInt32();
            br.BaseStream.Position += 4;        // Gap
            var offset = br.ReadInt64();
            var save = br.BaseStream.Position;
            br.BaseStream.Position = startOffs + offset;
            var colorArray = new TimeTrack<AngleAndColorAB>[elements];

            for (int t = 0; t < elements; t++)
            {
                colorArray[t] = new TimeTrackAngleAndColorAB(br, br.BaseStream.Position + 24);
            }

            br.BaseStream.Position = save;
            return colorArray;
        }

        TimeTrack<AngleABAndColor>[] ReadTimeTrackUnkColorBlock3Array(BinaryReader br, long startOffs)
        {
            var elements = br.ReadUInt32();
            br.BaseStream.Position += 4;        // Gap
            var offset = br.ReadInt64();
            var save = br.BaseStream.Position;
            br.BaseStream.Position = startOffs + offset;
            var colorArray = new TimeTrack<AngleABAndColor>[elements];

            for (int t = 0; t < elements; t++)
            {
                colorArray[t] = new TimeTrackAngleABAndColor(br, br.BaseStream.Position + 24);
            }

            br.BaseStream.Position = save;
            return colorArray;
        }

        TimeTrack<UnkBlock>[] ReadTimeTrackUnkColorBlock7Array(BinaryReader br, long startOffs)
        {
            var elements = br.ReadUInt32();
            br.BaseStream.Position += 4;        // Gap
            var offset = br.ReadInt64();
            var save = br.BaseStream.Position;
            br.BaseStream.Position = startOffs + offset;
            var colorArray = new TimeTrack<UnkBlock>[elements];

            for (int t = 0; t < elements; t++)
            {
                colorArray[t] = new TimeTrackUnkBlock(br, br.BaseStream.Position + 24);
            }

            br.BaseStream.Position = save;
            return colorArray;
        }
    }
}
