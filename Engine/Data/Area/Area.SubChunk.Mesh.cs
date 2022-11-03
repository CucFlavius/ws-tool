using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace ProjectWS.Engine.Data
{
    public partial class Area
    {
        public partial class SubChunk
        {
            public class Mesh
            {
                const int VERTEXCOUNT = 361;
                const int VERTEXSIZE = 32;

                [StructLayout(LayoutKind.Sequential)]
                struct TerrainVertex
                {
                    public Vector3 position;
                    public Vector3 normal;
                    public Vector2 uv;
                }

                TerrainVertex[] vertices;

                public float minHeight;
                public float maxHeight;

                public uint[] indexData;
                public bool isBuilt;
                public int _vertexArrayObject;

                public Mesh(ushort[] heightMap)
                {
                    if (heightMap == null) return;
                    this.minHeight = float.MaxValue;
                    this.maxHeight = float.MinValue;

                    if (heightMap.Length == VERTEXCOUNT)
                    {
                        // LoD 0 //
                        this.vertices = new TerrainVertex[VERTEXCOUNT];

                        int index = 0;
                        for (int y = -1; y < 18; ++y)
                        {
                            for (int x = -1; x < 18; ++x)
                            {
                                float h = ((heightMap[(y + 1) * 19 + x + 1] & 0x7FFF) / 8.0f) - 2048.0f;

                                // Calc minmax
                                if (h < this.minHeight)
                                    this.minHeight = h;
                                if (h > this.maxHeight)
                                    this.maxHeight = h;

                                // Positions //
                                this.vertices[index].position = new Vector3(x * 2, h, y * 2);

                                // Normals //
                                if (y > 0 && x > 0 && y <= 17 && y <= 17)
                                {
                                    Vector3 tl = this.vertices[(y - 1) * 19 + x - 1].position;
                                    Vector3 tr = this.vertices[(y - 1) * 19 + x + 1].position;
                                    Vector3 br = this.vertices[(y + 1) * 19 + x + 1].position;
                                    Vector3 bl = this.vertices[(y + 1) * 19 + x - 1].position;
                                    Vector3 v = this.vertices[y * 19 + x].position;
                                    Vector3 P1 = new Vector3(tl.X, tl.Y, tl.Z);
                                    Vector3 P2 = new Vector3(tr.X, tr.Y, tr.Z);
                                    Vector3 P3 = new Vector3(br.X, br.Y, br.Z);
                                    Vector3 P4 = new Vector3(bl.X, bl.Y, bl.Z);
                                    Vector3 vert = new Vector3(v.X, v.Y, v.Z);
                                    Vector3 N1 = Vector3.Cross((P2 - vert), (P1 - vert));
                                    Vector3 N2 = Vector3.Cross((P3 - vert), (P2 - vert));
                                    Vector3 N3 = Vector3.Cross((P4 - vert), (P3 - vert));
                                    Vector3 N4 = Vector3.Cross((P1 - vert), (P4 - vert));
                                    Vector3 norm = Vector3.Normalize(N1 + N2 + N3 + N4);
                                    this.vertices[y * 19 + x].normal = norm;
                                }
                                index++;
                            }
                        }
                        this.vertices = Trim19x19to17x17(this.vertices);

                        // Triangles //
                        this.indexData = new uint[16 * 16 * 2 * 3];
                        int triOffset = 0;
                        for (int strip = 0; strip < 16; strip++)
                        {
                            //   case Up-Left   //
                            for (int t = 0; t < 16; t++)
                            {
                                this.indexData[triOffset + 0] = (uint)(t + strip * 17);
                                this.indexData[triOffset + 1] = (uint)(t + 1 + strip * 17);
                                this.indexData[triOffset + 2] = (uint)(t + (strip + 1) * 17);
                                triOffset = triOffset + 3;
                            }
                            //   case Down-Right   //
                            for (int t = 0; t < 16; t++)
                            {
                                this.indexData[triOffset + 0] = (uint)(t + 1 + strip * 17);
                                this.indexData[triOffset + 1] = (uint)(t + 1 + (strip + 1) * 17);
                                this.indexData[triOffset + 2] = (uint)(t + (strip + 1) * 17);
                                triOffset = triOffset + 3;
                            }
                        }

                        // UVs //
                        //this.uvs = new Vector2[17 * 17];
                        for (int u = 0; u < 17; u++)
                        {
                            for (int v = 0; v < 17; v++)
                            {
                                this.vertices[u + v * 17].uv = new Vector2(u / 16.0f, v / 16.0f);
                            }
                        }
                    }
                    else
                    {
                        // LoD 1 //
                    }
                }

                public void Build()
                {
                    // Some subchunks don't exist
                    if (this.vertices == null) return;

                    int _vertexBufferObject = GL.GenBuffer();
                    GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
                    GL.BufferData(BufferTarget.ArrayBuffer, this.vertices.Length * VERTEXSIZE, this.vertices, BufferUsageHint.StaticDraw);

                    _vertexArrayObject = GL.GenVertexArray();
                    GL.BindVertexArray(_vertexArrayObject);

                    GL.EnableVertexAttribArray(0);
                    GL.EnableVertexAttribArray(1);
                    GL.EnableVertexAttribArray(2);

                    GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, VERTEXSIZE, 0);
                    GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, true, VERTEXSIZE, 12);
                    GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, VERTEXSIZE, 24);

                    int _elementBufferObject = GL.GenBuffer();
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
                    GL.BufferData(BufferTarget.ElementArrayBuffer, indexData.Length * 4, indexData, BufferUsageHint.StaticDraw);

                    GL.BindVertexArray(0);

                    this.isBuilt = true;
                }

                public void Draw()
                {
                    if (!this.isBuilt) return;

                    GL.BindVertexArray(_vertexArrayObject);
                    GL.DrawElements(BeginMode.Triangles, indexData.Length, DrawElementsType.UnsignedInt, 0);
                }

                Vector3[] Trim19x19to17x17(Vector3[] vector3in)
                {
                    Vector3[] vector3out = new Vector3[17 * 17];
                    for (int x = 1; x <= 17; x++)
                    {
                        for (int y = 1; y <= 17; y++)
                        {
                            vector3out[(x - 1) + (y - 1) * 17] = vector3in[x + y * 19];
                        }
                    }
                    return vector3out;
                }

                TerrainVertex[] Trim19x19to17x17(TerrainVertex[] vector3in)
                {
                    TerrainVertex[] vector3out = new TerrainVertex[17 * 17];
                    for (int x = 1; x <= 17; x++)
                    {
                        for (int y = 1; y <= 17; y++)
                        {
                            vector3out[(x - 1) + (y - 1) * 17] = vector3in[x + y * 19];
                        }
                    }
                    return vector3out;
                }
            }
        }
    }
}
