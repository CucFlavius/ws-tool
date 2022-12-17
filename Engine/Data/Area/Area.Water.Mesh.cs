using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using ProjectWS.Engine.Data.Extensions;
using System.Runtime.InteropServices;

namespace ProjectWS.Engine.Data
{
    public partial class Area
    {
        public partial class Water
        {
            public class Mesh
            {
                const int VERTEXSIZE = 104;

                [StructLayout(LayoutKind.Sequential)]
                public struct WaterVertex
                {
                    public Vector3 position;
                    public Vector3 normal;
                    public Vector4 tangent;
                    public Vector4 bitangent;
                    public Vector2 uv;
                    public Vector4 color;
                    public float unk0;
                    public int unk1;
                    public Vector4 layerBlendMask;

                    public WaterVertex(BinaryReader br)
                    {
                        this.position = br.ReadVector3();
                        this.normal = br.ReadVector3();
                        this.tangent = new Vector4(br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), 0);
                        this.bitangent = new Vector4(br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), 0);
                        this.uv = new Vector2(br.ReadSingle(), br.ReadSingle());
                        this.color = new Vector4(br.ReadByte() / 255f, br.ReadByte() / 255f, br.ReadByte() / 255f, br.ReadByte() / 255f);
                        this.unk0 = br.ReadSingle();
                        this.unk1 = br.ReadInt32();
                        this.layerBlendMask = new Vector4(br.ReadByte() / 255f, br.ReadByte() / 255f, br.ReadByte() / 255f, br.ReadByte() / 255f);
                    }
                }

                public WaterVertex[]? vertices;

                public float minHeight;
                public float maxHeight;

                public uint[]? indexData;
                public bool isBuilt;
                public int vertexArrayObject;

                public void Build()
                {
                    // Some subchunks don't exist
                    if (this.vertices == null) return;

                    int _vertexBufferObject = GL.GenBuffer();
                    GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
                    GL.BufferData(BufferTarget.ArrayBuffer, this.vertices.Length * VERTEXSIZE, this.vertices, BufferUsageHint.StaticDraw);

                    vertexArrayObject = GL.GenVertexArray();
                    GL.BindVertexArray(vertexArrayObject);

                    GL.EnableVertexAttribArray(0);
                    GL.EnableVertexAttribArray(1);
                    GL.EnableVertexAttribArray(2);
                    GL.EnableVertexAttribArray(3);
                    GL.EnableVertexAttribArray(4);
                    GL.EnableVertexAttribArray(5);
                    GL.EnableVertexAttribArray(6);
                    GL.EnableVertexAttribArray(7);
                    GL.EnableVertexAttribArray(8);

                    GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, VERTEXSIZE, 0);      // position 3 * 4 = 12
                    GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, true, VERTEXSIZE, 12);      // normal 12 + 3 * 4 = 24
                    GL.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, true, VERTEXSIZE, 24);      // tangent 24 + 4 * 4 = 40
                    GL.VertexAttribPointer(3, 4, VertexAttribPointerType.Float, true, VERTEXSIZE, 40);      // bitangent 40 + 4 * 4 = 56
                    GL.VertexAttribPointer(4, 2, VertexAttribPointerType.Float, false, VERTEXSIZE, 56);     // uv 56 + 2 * 4 = 64
                    GL.VertexAttribPointer(5, 4, VertexAttribPointerType.Float, false, VERTEXSIZE, 64);     // color 64 + 4 * 4 = 80
                    GL.VertexAttribPointer(6, 1, VertexAttribPointerType.Float, false, VERTEXSIZE, 80);     // unk0 80 + 4 = 84
                    GL.VertexAttribPointer(7, 1, VertexAttribPointerType.Int, false, VERTEXSIZE, 84);       // unk1 84 + 4 = 88
                    GL.VertexAttribPointer(8, 4, VertexAttribPointerType.Float, false, VERTEXSIZE, 88);    // mask 88 + 4 * 4 = 104

                    int _elementBufferObject = GL.GenBuffer();
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
                    GL.BufferData(BufferTarget.ElementArrayBuffer, indexData.Length * 4, indexData, BufferUsageHint.StaticDraw);

                    GL.BindVertexArray(0);

                    this.isBuilt = true;
                }

                public void Draw()
                {
                    if (!this.isBuilt) return;

                    GL.BindVertexArray(vertexArrayObject);
                    GL.DrawElements(BeginMode.Triangles, indexData.Length, DrawElementsType.UnsignedInt, 0);
                }
            }
        }
    }
}
