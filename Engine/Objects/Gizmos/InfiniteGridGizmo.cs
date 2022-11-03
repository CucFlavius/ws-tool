using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace ProjectWS.Engine.Objects.Gizmos
{
    public class InfiniteGridGizmo : GameObject
    {
        int _vertexArrayObject;
        bool isBuilt;
        int[] lineIndices;
        Vector4 color;

        public InfiniteGridGizmo(Vector4 color) => this.color = color;

        public override void Build()
        {
            float size = 1000.0f;
            float ch = size / 2.0f;

            var cube_vertices = new Vector3[]
            {
                new Vector3(-ch, 0, -ch),
                new Vector3(ch, 0, -ch),
                new Vector3(ch, 0, ch),
                new Vector3(-ch, 0, ch),
            };

            lineIndices = new int[]
            {
                0,1, 1,2, 2,3, 3,0
            };

            int _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, cube_vertices.Length * 3 * 4, cube_vertices, BufferUsageHint.StaticDraw);

            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 12, 0);

            int _elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, lineIndices.Length * 4, lineIndices, BufferUsageHint.StaticDraw);

            GL.BindVertexArray(0);

            this.isBuilt = true;
        }

        public override void Render(Matrix4 model, Shader shader)
        {
            if (!this.isBuilt) return;

            shader.SetMat4("model", model * transform.GetMatrix());
            shader.SetColor4("lineColor", this.color);

            GL.BindVertexArray(_vertexArrayObject);
            GL.DrawElements(BeginMode.Lines, lineIndices.Length, DrawElementsType.UnsignedInt, 0);
        }
    }
}
