using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace ProjectWS.Engine.Data
{
    public partial class Submesh : ArrayData
    {
        #region Variables 

        public uint startIndex;
        public uint startVertex;
        public uint indexCount;
        public uint vertexCount;
        public ushort boneMapIndex;
        public ushort boneMapCount;
        public ushort unk2;
        public short materialSelector;
        public short unk3;
        public short unk4;
        public short unk5;
        public sbyte meshGroupID;
        public byte unk6;
        public short unk7;
        public BodyPart meshAnatomyID;
        public short unk8;
        public short unk9;
        public short unk10;
        public short unk11;
        public short unk12;
        public short unk13;
        public short unk14;
        public short unk15;
        public byte unk16;
        public byte unk17;
        public Vector3 boundsMin;
        public Vector3 boundsMax;
        public Vector3 size;
        public Vector3 offset;
        public Vector3 unk18;

        public uint[] indexData;
        public byte[] vertexData;

        public bool isBuilt;
        public bool positionCompressed;
        public int _vertexArrayObject;

        int buffer;

        #endregion

        public override void Read(BinaryReader br, long startOffset)
        {
            this.startIndex = br.ReadUInt32();
            this.startVertex = br.ReadUInt32();
            this.indexCount = br.ReadUInt32();
            this.vertexCount = br.ReadUInt32();
            this.boneMapIndex = br.ReadUInt16();
            this.boneMapCount = br.ReadUInt16();
            this.unk2 = br.ReadUInt16();
            this.materialSelector = br.ReadInt16();
            this.unk3 = br.ReadInt16();
            this.unk4 = br.ReadInt16();
            this.unk5 = br.ReadInt16();
            this.meshGroupID = br.ReadSByte();
            this.unk6 = br.ReadByte();
            this.unk7 = br.ReadInt16();
            this.meshAnatomyID = (BodyPart)br.ReadByte();
            br.BaseStream.Position += 5;        // Padding
            this.unk8 = br.ReadInt16();
            this.unk9 = br.ReadInt16();
            this.unk10 = br.ReadInt16();
            this.unk11 = br.ReadInt16();
            this.unk12 = br.ReadInt16();
            this.unk13 = br.ReadInt16();
            this.unk14 = br.ReadInt16();
            this.unk15 = br.ReadInt16();
            this.unk16 = br.ReadByte();
            this.unk17 = br.ReadByte();
            br.BaseStream.Position += 6;        // Padding
            this.boundsMin = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()); br.ReadSingle(); // skip W
            this.boundsMax = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()); br.ReadSingle(); // skip W
            this.unk18 = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()); br.ReadSingle(); // skip W
            this.size = Vector3.One * (this.boundsMax.Y - this.boundsMin.Y);//new Vector3(Mathf.Abs(this.boundsMax.x - this.boundsMin.x), Mathf.Abs(this.boundsMax.y - this.boundsMin.y), Mathf.Abs(this.boundsMax.z - this.boundsMin.z));
            //Debug.Log(this.scale.ToString());
            //this.offset = new Vector3(0, this.boundsMin.y, 0);
        }

        public void Build(int vertexBlockSizeInBytes, byte[] vertexBlockFieldPositions, Geometry.VertexBlockFlags vertexBlockFlags, Geometry.VertexFieldType[] vertexFieldTypes)
        {
            bool hasPositions = vertexBlockFlags.HasFlag(Geometry.VertexBlockFlags.hasPosition);
            bool hasTangents = vertexBlockFlags.HasFlag(Geometry.VertexBlockFlags.hasTangent);
            bool hasNormals = vertexBlockFlags.HasFlag(Geometry.VertexBlockFlags.hasNormal);
            bool hasBiTangents = vertexBlockFlags.HasFlag(Geometry.VertexBlockFlags.hasBiTangent);
            bool hasBoneIndices = vertexBlockFlags.HasFlag(Geometry.VertexBlockFlags.hasBoneIndices);
            bool hasBoneWeights = vertexBlockFlags.HasFlag(Geometry.VertexBlockFlags.hasBoneWeights);
            bool hasColors0 = vertexBlockFlags.HasFlag(Geometry.VertexBlockFlags.hasVertexColor0);
            bool hasColors1 = vertexBlockFlags.HasFlag(Geometry.VertexBlockFlags.hasVertexColor1);
            bool hasUV0 = vertexBlockFlags.HasFlag(Geometry.VertexBlockFlags.hasUV0);
            bool hasUV1 = vertexBlockFlags.HasFlag(Geometry.VertexBlockFlags.hasUV1);
            bool hasUnknown = vertexBlockFlags.HasFlag(Geometry.VertexBlockFlags.hasUnknown);

            this.positionCompressed = vertexFieldTypes[0] == Geometry.VertexFieldType.Vector3_16bit;

            GL.GenVertexArrays(1, out _vertexArrayObject);
            int vbo = GL.GenBuffer();
            int ebo = GL.GenBuffer();

            // bind the Vertex Array Object first, then bind and set vertex buffer(s), and then configure vertex attributes(s).
            GL.BindVertexArray(_vertexArrayObject);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);

            GL.BufferData(BufferTarget.ArrayBuffer, this.vertexData.Length, this.vertexData, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, this.indexData.Length * 4, this.indexData, BufferUsageHint.StaticDraw);

            int idx = 0;

            if (hasPositions)
            {
                GL.VertexAttribPointer(0, 3, positionCompressed ? VertexAttribPointerType.Short : VertexAttribPointerType.Float, false, vertexBlockSizeInBytes, vertexBlockFieldPositions[idx++]);
                GL.EnableVertexAttribArray(0);
            }
            if (hasTangents)
            {
                GL.VertexAttribPointer(1, 2, VertexAttribPointerType.UnsignedByte, false, vertexBlockSizeInBytes, vertexBlockFieldPositions[idx++]);
                GL.EnableVertexAttribArray(1);
            }
            if (hasNormals)
            {
                GL.VertexAttribPointer(2, 2, VertexAttribPointerType.UnsignedByte, false, vertexBlockSizeInBytes, vertexBlockFieldPositions[idx++]);
                GL.EnableVertexAttribArray(2);
            }
            if (hasBiTangents)
            {
                GL.VertexAttribPointer(3, 2, VertexAttribPointerType.UnsignedByte, false, vertexBlockSizeInBytes, vertexBlockFieldPositions[idx++]);
                GL.EnableVertexAttribArray(3);
            }
            if (hasBoneIndices)
            {
                GL.VertexAttribPointer(4, 4, VertexAttribPointerType.UnsignedByte, false, vertexBlockSizeInBytes, vertexBlockFieldPositions[idx++]);
                GL.EnableVertexAttribArray(4);
            }
            if (hasBoneWeights)
            {
                GL.VertexAttribPointer(5, 4, VertexAttribPointerType.UnsignedByte, true, vertexBlockSizeInBytes, vertexBlockFieldPositions[idx++]);
                GL.EnableVertexAttribArray(5);
            }
            if (hasColors0)
            {
                GL.VertexAttribPointer(6, 4, VertexAttribPointerType.UnsignedByte, true, vertexBlockSizeInBytes, vertexBlockFieldPositions[idx++]);
                GL.EnableVertexAttribArray(6);
            }
            if (hasColors1)
            {
                GL.VertexAttribPointer(7, 4, VertexAttribPointerType.UnsignedByte, true, vertexBlockSizeInBytes, vertexBlockFieldPositions[idx++]);
                GL.EnableVertexAttribArray(7);
            }
            if (hasUV0)
            {
                GL.VertexAttribPointer(8, 2, VertexAttribPointerType.HalfFloat, false, vertexBlockSizeInBytes, vertexBlockFieldPositions[idx++]);
                GL.EnableVertexAttribArray(8);
            }
            if (hasUV0)
            {
                GL.VertexAttribPointer(9, 2, VertexAttribPointerType.HalfFloat, false, vertexBlockSizeInBytes, vertexBlockFieldPositions[idx++]);
                GL.EnableVertexAttribArray(9);
            }
            if (hasUnknown)
            {
                GL.VertexAttribPointer(10, 1, VertexAttribPointerType.UnsignedByte, false, vertexBlockSizeInBytes, vertexBlockFieldPositions[idx++]);
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

        public void Draw()
        {
            if (!this.isBuilt) return;

            GL.BindVertexArray(this._vertexArrayObject);
            GL.DrawElements(BeginMode.Triangles, this.indexData.Length, DrawElementsType.UnsignedInt, 0);
        }

        public void DrawInstanced(Matrix4[] instances)
        {
            if (!this.isBuilt) return;

            // configure instanced array
            // -------------------------
            //int buffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, this.buffer);
            GL.BufferData( BufferTarget.ArrayBuffer, instances.Length * 64, instances, BufferUsageHint.StaticDraw);

            GL.BindVertexArray(this._vertexArrayObject);
            GL.DrawElementsInstanced(PrimitiveType.Triangles, this.indexData.Length, DrawElementsType.UnsignedInt, instances, instances.Length);
        }
    }
}