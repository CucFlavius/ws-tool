namespace ProjectWS.Engine.Database.Definitions
{
	public class World : TblRecord
	{
		public override string GetFileName() => "World";
		public override uint GetID() => this.ID;

		[Flags]
		public enum Flags : uint
		{
			unk0x1 = 0x1,
			unk0x2 = 0x2,
			unk0x4 = 0x4,
			unk0x8 = 0x8,
			unk0x10 = 0x10,
			unused0x20 = 0x20,
			unk0x40 = 0x40,
			unk0x80 = 0x80,
			unk0x100 = 0x100
		}

		public uint ID { get; set; }
		public string? assetPath { get; set; }
        public uint flags { get; set; }
        public uint type { get; set; }
        public string? screenPath { get; set; }
        public string? screenModelPath { get; set; }
        public uint chunkBounds00 { get; set; }
        public uint chunkBounds01 { get; set; }
        public uint chunkBounds02 { get; set; }
        public uint chunkBounds03 { get; set; }
        public uint plugAverageHeight { get; set; }
        public uint localizedTextIdName { get; set; }
        public uint minItemLevel { get; set; }
        public uint maxItemLevel { get; set; }
        public uint primeLevelOffset { get; set; }
        public uint primeLevelMax { get; set; }
        public uint veteranTierScalingType { get; set; }
        public uint heroismMenaceLevel { get; set; }
        public uint rewardRotationContentId { get; set; }
    }
}
