using MathUtils;
using OpenTK.Graphics.OpenGL4;

namespace ProjectWS.Engine.Mesh
{
    public class M3Mesh : Mesh
    {
        public FileFormats.M3.Submesh? data;
        int vertexBlockSizeInBytes;
        byte[]? vertexBlockFieldPositions;
        FileFormats.M3.Geometry.VertexBlockFlags vertexBlockFlags;
        FileFormats.M3.Geometry.VertexFieldType[]? vertexFieldTypes;

        public bool isBuilt;
        public bool positionCompressed;
        public bool renderable;
        public int _vertexArrayObject;

        public Matrix4[]? instances;
        int instanceBuffer;

        public M3Mesh(FileFormats.M3.Submesh submesh, int vertexBlockSizeInBytes, byte[] vertexBlockFieldPositions, FileFormats.M3.Geometry.VertexBlockFlags vertexBlockFlags, FileFormats.M3.Geometry.VertexFieldType[] vertexFieldTypes)
        {
            this.data = submesh;
            this.vertexBlockSizeInBytes = vertexBlockSizeInBytes;
            this.vertexBlockFieldPositions = vertexBlockFieldPositions;
            this.vertexBlockFlags = vertexBlockFlags;
            this.vertexFieldTypes = vertexFieldTypes;
        }

        public override void Build()
        {
            this.renderable = true;

            if (this.data == null || this.data.vertexData == null || this.data.indexData == null || this.vertexFieldTypes == null || this.vertexBlockFieldPositions == null)
            {
                this.renderable = false;
                return;
            }

            if (this.data.unk16 == 10)
            {
                this.renderable = false;
            }

            bool hasPositions = vertexBlockFlags.HasFlag(FileFormats.M3.Geometry.VertexBlockFlags.hasPosition);
            bool hasTangents = vertexBlockFlags.HasFlag(FileFormats.M3.Geometry.VertexBlockFlags.hasTangent);
            bool hasNormals = vertexBlockFlags.HasFlag(FileFormats.M3.Geometry.VertexBlockFlags.hasNormal);
            bool hasBiTangents = vertexBlockFlags.HasFlag(FileFormats.M3.Geometry.VertexBlockFlags.hasBiTangent);
            bool hasBoneIndices = vertexBlockFlags.HasFlag(FileFormats.M3.Geometry.VertexBlockFlags.hasBoneIndices);
            bool hasBoneWeights = vertexBlockFlags.HasFlag(FileFormats.M3.Geometry.VertexBlockFlags.hasBoneWeights);
            bool hasColors0 = vertexBlockFlags.HasFlag(FileFormats.M3.Geometry.VertexBlockFlags.hasVertexColor0);
            bool hasColors1 = vertexBlockFlags.HasFlag(FileFormats.M3.Geometry.VertexBlockFlags.hasVertexColor1);
            bool hasUV0 = vertexBlockFlags.HasFlag(FileFormats.M3.Geometry.VertexBlockFlags.hasUV0);
            bool hasUV1 = vertexBlockFlags.HasFlag(FileFormats.M3.Geometry.VertexBlockFlags.hasUV1);
            bool hasUnknown = vertexBlockFlags.HasFlag(FileFormats.M3.Geometry.VertexBlockFlags.hasUnknown);

            this.positionCompressed = vertexFieldTypes[0] == FileFormats.M3.Geometry.VertexFieldType.Vector3_16bit;

            GL.GenVertexArrays(1, out _vertexArrayObject);
            int vbo = GL.GenBuffer();
            int ebo = GL.GenBuffer();

            // bind the Vertex Array Object first, then bind and set vertex buffer(s), and then configure vertex attributes(s).
            GL.BindVertexArray(_vertexArrayObject);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);

            GL.BufferData(BufferTarget.ArrayBuffer, this.data.vertexData.Length, this.data.vertexData, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, this.data.indexData.Length * 4, this.data.indexData, BufferUsageHint.StaticDraw);

            int idx = 0;

            if (hasPositions)
            {
                GL.VertexAttribPointer(0, 3, positionCompressed ? VertexAttribPointerType.Short : VertexAttribPointerType.Float, false, vertexBlockSizeInBytes, vertexBlockFieldPositions[idx]);
                GL.EnableVertexAttribArray(0);
            }
            idx++;

            if (hasTangents)
            {
                GL.VertexAttribPointer(1, 2, VertexAttribPointerType.UnsignedByte, false, vertexBlockSizeInBytes, vertexBlockFieldPositions[idx]);
                GL.EnableVertexAttribArray(1);
            }
            idx++;

            if (hasNormals)
            {
                GL.VertexAttribPointer(2, 2, VertexAttribPointerType.UnsignedByte, false, vertexBlockSizeInBytes, vertexBlockFieldPositions[idx]);
                GL.EnableVertexAttribArray(2);
            }
            idx++;

            if (hasBiTangents)
            {
                GL.VertexAttribPointer(3, 2, VertexAttribPointerType.UnsignedByte, false, vertexBlockSizeInBytes, vertexBlockFieldPositions[idx]);
                GL.EnableVertexAttribArray(3);
            }
            idx++;

            if (hasBoneIndices)
            {
                GL.VertexAttribPointer(4, 4, VertexAttribPointerType.UnsignedByte, false, vertexBlockSizeInBytes, vertexBlockFieldPositions[idx]);
                GL.EnableVertexAttribArray(4);
            }
            idx++;

            if (hasBoneWeights)
            {
                GL.VertexAttribPointer(5, 4, VertexAttribPointerType.UnsignedByte, true, vertexBlockSizeInBytes, vertexBlockFieldPositions[idx]);
                GL.EnableVertexAttribArray(5);
            }
            idx++;

            if (hasColors0)
            {
                GL.VertexAttribPointer(6, 4, VertexAttribPointerType.UnsignedByte, true, vertexBlockSizeInBytes, vertexBlockFieldPositions[idx]);
                GL.EnableVertexAttribArray(6);
            }
            idx++;

            if (hasColors1)
            {
                GL.VertexAttribPointer(7, 4, VertexAttribPointerType.UnsignedByte, true, vertexBlockSizeInBytes, vertexBlockFieldPositions[idx]);
                GL.EnableVertexAttribArray(7);
            }
            idx++;

            if (hasUV0)
            {
                GL.VertexAttribPointer(8, 2, VertexAttribPointerType.HalfFloat, false, vertexBlockSizeInBytes, vertexBlockFieldPositions[idx]);
                GL.EnableVertexAttribArray(8);
            }
            idx++;

            if (hasUV1)
            {
                GL.VertexAttribPointer(9, 2, VertexAttribPointerType.HalfFloat, false, vertexBlockSizeInBytes, vertexBlockFieldPositions[idx]);
                GL.EnableVertexAttribArray(9);
            }
            idx++;

            if (hasUnknown)
            {
                GL.VertexAttribPointer(10, 1, VertexAttribPointerType.UnsignedByte, false, vertexBlockSizeInBytes, vertexBlockFieldPositions[idx]);
                GL.EnableVertexAttribArray(10);
            }
            // note that this is allowed, the call to glVertexAttribPointer registered VBO as the vertex attribute's bound vertex buffer object so afterwards we can safely unbind
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            //buffer = GL.GenBuffer();
            //GL.BindBuffer(BufferTarget.ArrayBuffer, buffer);


            // remember: do NOT unbind the EBO while a VAO is active as the bound element buffer object IS stored in the VAO; keep the EBO bound.
            //glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0);

            // You can unbind the VAO afterwards so other VAO calls won't accidentally modify this VAO, but this rarely happens. Modifying other
            // VAOs requires a call to glBindVertexArray anyways so we generally don't unbind VAOs (nor VBOs) when it's not directly necessary.
            GL.BindVertexArray(0);

            this.isBuilt = true;
        }

        public override void Draw()
        {
            if (!this.isBuilt || !this.renderable || this.data == null || this.data.indexData == null) return;

            GL.BindVertexArray(this._vertexArrayObject);
            GL.DrawElements(BeginMode.Triangles, this.data.indexData.Length, DrawElementsType.UnsignedInt, 0);
        }

        public override void DrawInstanced()
        {
            if (!this.isBuilt || !this.renderable || this.data == null || this.data.indexData == null || this.instances == null) return;

            // configure instanced array
            // -------------------------
            //int buffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, this.instanceBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, this.instances.Length * 64, this.instances, BufferUsageHint.StaticDraw);

            GL.BindVertexArray(this._vertexArrayObject);
            GL.DrawElementsInstanced(PrimitiveType.Triangles, this.data.indexData.Length, DrawElementsType.UnsignedInt, this.data.indexData, this.instances.Length);
        }

    }
}
