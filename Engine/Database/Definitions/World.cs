namespace ProjectWS.Engine.Database.Definitions
{
	public class World : TblRecord
	{
		public override string GetFileName() => "World";
		public override uint GetID() => this.ID;


		public uint ID;
		public string assetPath;
		public uint flags;
		public uint type;
		public string screenPath;
		public string screenModelPath;
		public uint chunkBounds00;
		public uint chunkBounds01;
		public uint chunkBounds02;
		public uint chunkBounds03;
		public uint plugAverageHeight;
		public uint localizedTextIdName;
		public uint minItemLevel;
		public uint maxItemLevel;
		public uint primeLevelOffset;
		public uint primeLevelMax;
		public uint veteranTierScalingType;
		public uint heroismMenaceLevel;
		public uint rewardRotationContentId;
	}
}
