﻿using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using ProjectWS.Engine.Data.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ProjectWS.Engine.Rendering.TextRenderer;

namespace ProjectWS.Engine.Rendering
{
    public class ImmediateRenderer
    {
        readonly ConcurrentQueue<Box>? boxRenderQueue;
        int drawCalls;
        int boxVAO;
        int[]? boxIndices;
        Shader? lineShader;

        public struct Box
        {
            public Vector3 position { get; set; }
            public Quaternion rotation { get; set; }
            public Vector3 size { get; set; }
            public Matrix4 matrix { get; set; }
            public Vector4 color { get; set; }

            public Box(Vector3 position, Quaternion rotation, Vector3 size, Vector4 color)
            {
                this.position = position;
                this.rotation = rotation;
                this.size = size;
                this.color = color;

                this.matrix = new Matrix4();
                this.matrix = this.matrix.TRS(position, rotation, size);
            }

            public Box(Matrix4 matrix, Vector4 color)
            {
                this.matrix = matrix;
                this.color = color;

                // TODO : Fix extract TRS from matrix
                this.position = Vector3.Zero;
                this.rotation = Quaternion.Identity;
                this.size = Vector3.One;
            }
        }

        public ImmediateRenderer()
        {
            this.boxRenderQueue = new ConcurrentQueue<Box>();
            this.drawCalls = 0;
        }

        public void Initialize(Shader lineShader)
        {
            this.lineShader = lineShader;

            float cubeSize = 1.0f;
            float ch = cubeSize / 2.0f;

            var vertices = new Vector3[]
            {
                new Vector3(-ch, -ch, -ch),
                new Vector3(ch, -ch, -ch),
                new Vector3(ch, ch, -ch),
                new Vector3(-ch, ch, -ch),
                new Vector3(-ch, -ch, ch),
                new Vector3(ch, -ch, ch),
                new Vector3(ch, ch, ch),
                new Vector3(-ch, ch, ch)
            };

            this.boxIndices = new int[]
            {
                0,1, 1,2, 2,3, 3,0, 4,5, 5,6, 6,7, 7,4, 0,4, 1,5, 2,6, 3,7
            };

            int _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * 3 * 4, vertices, BufferUsageHint.StaticDraw);

            this.boxVAO = GL.GenVertexArray();
            GL.BindVertexArray(this.boxVAO);

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 12, 0);

            int _elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, this.boxIndices.Length * 4, this.boxIndices, BufferUsageHint.StaticDraw);

            GL.BindVertexArray(0);
        }

        public void Render(Renderer renderer, Viewport viewport)
        {
            this.drawCalls = 0;

            if (this.lineShader != null)
            {
                this.lineShader.Use();
                viewport.mainCamera.SetToShader(this.lineShader);
            }

            RenderBoxes(renderer, viewport);
        }

        private void RenderBoxes(Renderer renderer, Viewport viewport)
        {
            for (int i = 0; i < this.boxRenderQueue?.Count; i++)
            {
                if (this.boxRenderQueue.TryDequeue(out Box box))
                {
                    if (this.boxIndices == null) return;

                    this.lineShader?.SetMat4("model", box.matrix);
                    this.lineShader?.SetColor4("lineColor", box.color);

                    GL.BindVertexArray(this.boxVAO);
                    GL.DrawElements(BeginMode.Lines, this.boxIndices.Length, DrawElementsType.UnsignedInt, 0);

                    drawCalls++;
                }
            }
        }

        /// <summary>
        /// Render a wireframe box using immediate mode
        /// </summary>
        /// <param name="position">World space position</param>
        /// <param name="rotation">Rotation</param>
        /// <param name="size">Size</param>
        internal void DrawWireBox3D(Vector3 position, Quaternion rotation, Vector3 size, Vector4 color)
        {
            this.boxRenderQueue?.Enqueue(new Box(position, rotation, size, color));
        }

        internal void DrawWireBox3D(Matrix4 matrix, Vector4 color)
        {
            this.boxRenderQueue?.Enqueue(new Box(matrix, color));
        }
    }
}
