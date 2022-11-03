namespace ProjectWS.Engine.Database.Definitions
{
	public class WorldSky : TblRecord
	{
		public override string GetFileName() => "WorldSky";
		public override uint GetID() => this.ID;
		
		public uint ID;
		public string assetPath;
		public string assetPathInFlux;
		public uint color;
	}
}
