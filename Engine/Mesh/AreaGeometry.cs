using ProjectWS.Engine.Material;
using ProjectWS.Engine.World;

namespace ProjectWS.Engine.Mesh
{
    public class AreaGeometry
    {
        Chunk chunk;
        Data.Area? data;
        public AreaMesh[]? terrainMeshes;
        public TerrainMaterial[]? terrainMaterials;
        public WaterMesh[]? waterMeshes;
        public WaterMaterial[] waterMaterials;
        public float minHeight;
        public float maxHeight;

        public AreaGeometry(Data.Area data, Chunk chunk)
        {
            this.chunk = chunk;
            this.data = data;
            this.terrainMeshes = new AreaMesh[256];
            this.terrainMaterials = new TerrainMaterial[256];
            this.minHeight = float.MaxValue;
            this.maxHeight = float.MinValue;
        }

        public void Build(int quadrant, int lod)
        {
            if (this.terrainMeshes == null || this.data == null || this.data.subChunks == null) return;

            int from = quadrant * (256 / 4);
            int to = from + (256 / 4);
            int total = this.data.subChunks.Count;
            for (int i = from; i < to; i++)
            {
                if (i < total)
                {
                    if (lod == 0)
                    {
                        this.terrainMeshes[i] = new AreaMesh(this.data.subChunks[i].heightMap, this.data.subChunks[i], this.chunk, i);
                    }
                    else if (lod == 1)
                    {
                        this.terrainMeshes[i] = new AreaMesh(this.data.subChunks[i].lodHeightMap, this.data.subChunks[i], this.chunk, i);
                    }

                    // Calc minmax
                    if (this.terrainMeshes[i].minHeight < this.minHeight)
                        this.minHeight = this.terrainMeshes[i].minHeight;
                    if (this.terrainMeshes[i].maxHeight > this.maxHeight)
                        this.maxHeight = this.terrainMeshes[i].maxHeight;

                    this.terrainMeshes[i].Build();


                    if (this.terrainMaterials != null)
                    {
                        this.terrainMaterials[i] = new Material.TerrainMaterial(this.chunk, this.data.subChunks[i]);
                        this.terrainMaterials[i].Build();
                    }

                    // Water //
                    /*
                    if (this.data.subChunks[i].waters != null && this.data.subChunks[i].hasWater)
                    {
                        for (int i = 0; i < this.data.subChunks[i].waters.Length; i++)
                        {
                            this.waters[i].Build();
                        }
                    }
                    */
                }
            }
        }
    }
}
