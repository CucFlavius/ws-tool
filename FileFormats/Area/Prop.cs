using MathUtils;
using ProjectWS.FileFormats.Extensions;
using System.Text.Json.Serialization;

namespace ProjectWS.FileFormats.Area
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
        public string? path;

        [JsonIgnore]
        public bool loadRequested;

        public Prop() { }

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

        public void Write(BinaryWriter bw, int propsSize, ref Dictionary<string, uint> names, ref uint lastNameOffset)
        {
            bw.Write(this.uniqueID);
            bw.Write(this.someID);
            bw.Write(this.unk0);
            bw.Write(this.unk1);
            bw.Write((int)this.modelType);
            // Name offset
            if (this.path == null)
            {
                bw.Write(0);
            }
            else
            {
                if (names.ContainsKey(this.path))
                {
                    bw.Write((uint)(names[this.path] + propsSize));
                }
                else
                {
                    names.Add(this.path, lastNameOffset);
                    bw.Write((uint)(lastNameOffset + propsSize));
                    lastNameOffset += (uint)(this.path.Length * 2 + 2);
                }
            }
            bw.Write(this.unkOffset);
            bw.Write(this.scale);
            bw.Write(this.rotation.X);
            bw.Write(this.rotation.Y);
            bw.Write(this.rotation.Z);
            bw.Write(this.rotation.W);
            bw.Write(this.position.X);
            bw.Write(this.position.Y);
            bw.Write(this.position.Z);
            bw.Write(this.placement.minX);
            bw.Write(this.placement.minY);
            bw.Write(this.placement.maxX);
            bw.Write(this.placement.maxY);
            bw.Write(this.unk7);
            bw.Write(this.unk8);
            bw.Write(this.unk9);
            bw.Write(this.color0);
            bw.Write(this.color1);
            bw.Write(this.unk10);
            bw.Write(this.unk11);
            bw.Write(this.color2);
            bw.Write(this.unk12);
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
