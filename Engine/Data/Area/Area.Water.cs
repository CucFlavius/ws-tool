using OpenTK.Mathematics;
using ProjectWS.Engine.Data.Extensions;

namespace ProjectWS.Engine.Data
{
    public partial class Area
    {
        public class Water
        {
            public uint worldWaterTypeID;           // Entry in WorldWaterType tbl
            public uint[] waterLayerIDs;            // Entries in WorldWaterLayer tbl, they get blended together (vertex layer blend mask)
            public uint unk0;
            public float unk1;
            public uint unk2;
            public float unk3;
            public float unk4;
            public uint unk5;
            public uint unk6;
            public float shoreLineDistance;
            public float unk7;
            public uint shoreLineWaterLayerID;      // Entry in WorldWaterLayer tbl
            public uint unk8;
            public uint indexCount;
            public uint vertexCount;
            public uint unk9;
            public uint unk10;

            // Vertex Data //
            public Vector3[] positions;
            public Vector3[] normals;
            public Vector4[] tangents;
            public Vector4[] bitangents;
            public Vector2[] uvs;
            public Vector4[] colors;
            public float[] unkFloats;
            public int[] unkInts;
            public Vector2[] layerBlendMasksA;
            public Vector2[] layerBlendMasksB;

            // Index Data //
            public int[] indices;

            //public Mesh mesh;
            //public Material material;

            public Water(BinaryReader br)
            {
                this.worldWaterTypeID = br.ReadUInt32();
                this.waterLayerIDs = new uint[4];
                for (int i = 0; i < 4; i++)
                {
                    this.waterLayerIDs[i] = br.ReadUInt32();
                }
                this.unk0 = br.ReadUInt32();
                this.unk1 = br.ReadSingle();
                this.unk2 = br.ReadUInt32();
                this.unk3 = br.ReadSingle();
                this.unk4 = br.ReadSingle();
                this.unk5 = br.ReadUInt32();
                this.unk6 = br.ReadUInt32();
                this.shoreLineDistance = br.ReadSingle();
                this.unk7 = br.ReadSingle();
                this.shoreLineWaterLayerID = br.ReadUInt32();
                this.unk8 = br.ReadUInt32();
                this.indexCount = br.ReadUInt32();
                this.vertexCount = br.ReadUInt32();
                this.unk9 = br.ReadUInt32();
                this.unk10 = br.ReadUInt32();

                this.indices = new int[this.indexCount];
                for (int i = 0; i < this.indexCount; i++)
                {
                    this.indices[i] = br.ReadInt32();
                }

                this.positions = new Vector3[this.vertexCount];
                this.normals = new Vector3[this.vertexCount];
                this.tangents = new Vector4[this.vertexCount];
                this.bitangents = new Vector4[this.vertexCount];
                this.uvs = new Vector2[this.vertexCount];
                this.colors = new Vector4[this.vertexCount];
                this.unkFloats = new float[this.vertexCount];
                this.unkInts = new int[this.vertexCount];
                this.layerBlendMasksA = new Vector2[this.vertexCount];
                this.layerBlendMasksB = new Vector2[this.vertexCount];
                for (int i = 0; i < this.vertexCount; i++)
                {
                    this.positions[i] = br.ReadVector3();
                    this.normals[i] = br.ReadVector3(false);
                    this.tangents[i] = new Vector4(br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), 0);
                    this.bitangents[i] = new Vector4(br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), 0);
                    this.uvs[i] = new Vector2(br.ReadSingle(), br.ReadSingle());
                    this.colors[i] = new Vector4(br.ReadByte(), br.ReadByte(), br.ReadByte(), br.ReadByte());
                    this.unkFloats[i] = br.ReadSingle();
                    this.unkInts[i] = br.ReadInt32();
                    this.layerBlendMasksA[i] = new Vector2(br.ReadByte() / 255f, br.ReadByte() / 255f);
                    this.layerBlendMasksB[i] = new Vector2(br.ReadByte() / 255f, br.ReadByte() / 255f);
                }
            }

            public void Build()
            {
                // Mesh //
                //this.mesh = new Mesh();
                //this.mesh.vertices = this.positions;
                //this.mesh.normals = this.normals;
                //this.mesh.tangents = this.tangents;
                //this.mesh.uv = this.uvs;
                //this.mesh.uv2 = this.layerBlendMasksA;
                //this.mesh.uv3 = this.layerBlendMasksB;
                //this.mesh.colors32 = this.colors;
                //this.mesh.triangles = this.indices;

                //this.material = new Material(Shaders.ShaderResources.water);
            }
        }
    }
}
