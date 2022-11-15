using OpenTK.Mathematics;
using ProjectWS.Engine.Data.Extensions;

namespace ProjectWS.Engine.Data
{
    public class UnkBlock
    {
        // Most probably wrong
        public float unk0;
        public float unk1;
        public Vector4 unk2;
        public float unk3;
        public Vector4 unk4;
        public Vector4 unk5;
        public float unk6;
        public Vector4 unk7;
        public Vector3 vec0;
        public Vector4 unk8;
        public float unk9;

        public Vector4 unk10;
        public Vector4 unk11;
        public float unk12;
        public Vector4 unk13;
        public Vector3 vec1;
        public Vector4 unk14;
        public float unk15;

        public Vector4 unk16;
        public Vector4 unk17;
        public float unk18;
        public Vector4 unk19;
        public float unk20;

        public UnkBlock(BinaryReader br)
        {
            this.unk0 = br.ReadSingle();
            this.unk1 = br.ReadSingle();
            this.unk2 = br.ReadColor32();
            this.unk3 = br.ReadSingle();
            this.unk4 = br.ReadColor32();
            this.unk5 = br.ReadColor32();
            this.unk6 = br.ReadSingle();
            this.unk7 = br.ReadColor32();
            this.vec0 = br.ReadVector3();
            this.unk8 = br.ReadColor32();
            this.unk9 = br.ReadSingle();

            this.unk10 = br.ReadColor32();
            this.unk11 = br.ReadColor32();
            this.unk12 = br.ReadSingle();
            this.unk13 = br.ReadColor32();
            this.vec1 = br.ReadVector3();
            this.unk14 = br.ReadColor32();
            this.unk15 = br.ReadSingle();

            this.unk16 = br.ReadColor32();
            this.unk17 = br.ReadColor32();
            this.unk18 = br.ReadSingle();
            this.unk19 = br.ReadColor32();
            this.unk20 = br.ReadSingle();
        }
    }

}
