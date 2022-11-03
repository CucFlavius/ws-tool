using OpenTK.Mathematics;
using ProjectWS.Engine.Data.Extensions;

namespace ProjectWS.Engine.Data
{
    public partial class Area
    {
        public class Prop
        {
            public uint uniqueID;
            public uint someID;
            public int unk0;
            public int unk1;
            public ModelType modelType;
            public int nameOffset;
            public int unkOffset;
            public float scale;
            public Quaternion rotation;
            public Vector3 position;
            public Placement placement;
            public int unk7;
            public int unk8;
            public int unk9;
            public int color0;
            public int color1;
            public int unk10;
            public int unk11;
            public int color2;
            public int unk12;
            public string path;

            public bool loadRequested;

            public Prop(BinaryReader br, long chunkStart)
            {
                this.uniqueID = br.ReadUInt32();
                this.someID = br.ReadUInt32();
                this.unk0 = br.ReadInt32();
                this.unk1 = br.ReadInt32();
                this.modelType = (ModelType)br.ReadInt32();
                this.nameOffset = br.ReadInt32();
                this.unkOffset = br.ReadInt32();
                this.scale = br.ReadSingle();
                this.rotation = br.ReadQuaternion(true);
                this.position = br.ReadVector3();
                this.placement = new Placement(br);
                this.unk7 = br.ReadInt32();
                this.unk8 = br.ReadInt32();
                this.unk9 = br.ReadInt32();
                this.color0 = br.ReadInt32();
                this.color1 = br.ReadInt32();
                this.unk10 = br.ReadInt32();
                this.unk11 = br.ReadInt32();
                this.color2 = br.ReadInt32();
                this.unk12 = br.ReadInt32();

                this.loadRequested = false;

                if (this.nameOffset != 0)
                {
                    long save = br.BaseStream.Position;
                    br.BaseStream.Position = chunkStart + this.nameOffset;
                    this.path = br.ReadWString();
                    br.BaseStream.Position = save;
                }
                else
                {
                    this.path = null;
                }
            }

            public enum ModelType
            {
                M3 = 0,
                I3 = 1,
                Unk_2 = 2,          // Maybe light or volume, doesn't have a file path
                DGN = 3,
                Unk_4 = 4,          // Maybe light or volume, doesn't have a file path
            }
        }
    }
}
