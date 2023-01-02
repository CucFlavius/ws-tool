using MathUtils;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace ProjectWS.Engine.World
{
    public class MinimapChunk
    {
        public bool exists;
        public string? path;
        public FileFormats.Tex.File? texFile;
        internal bool isVisible;
        internal volatile bool isRead;
        public Matrix4 matrix;

        int[]? minimapPtr;
        int mipCount;

        public void Read()
        {
            if (this.path == null || this.texFile == null)
            {
                this.exists = false;
                this.isRead = false;
                return;
            }

            using (var fs = File.OpenRead(this.path))
            {
                this.texFile.Read(fs);

                if (this.texFile.mipData == null)
                {
                    this.exists = false;
                    this.isRead = false;
                    return;
                }

                this.mipCount = this.texFile.mipData.Count;
                this.minimapPtr = new int[this.mipCount];
            }

            this.isRead = true;
        }

        public void BuildMip(int mip)
        {
            var resolution = 512;
            for (int i = 0; i < mip; i++)
            {
                resolution /= 2;
            }
            var mipIndex = (this.mipCount - 1) - mip;

            GL.GenTextures(1, out this.minimapPtr[mipIndex]);
            GL.BindTexture(TextureTarget.Texture2D, this.minimapPtr[mipIndex]);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.GenerateMipmap, 0);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 0);

            GL.CompressedTexImage2D(TextureTarget.Texture2D, 0, InternalFormat.CompressedRgbaS3tcDxt1Ext, resolution, resolution, 0, this.texFile.mipData[mipIndex].Length, this.texFile.mipData[mipIndex]);
        }

        internal void Render(Shader shader, int mip, int quadVAO)
        {
            var mipIndex = (this.mipCount - 1) - mip;

            if (this.minimapPtr == null)
                this.minimapPtr = new int[this.mipCount];

            if (mipIndex == -1 || mipIndex >= this.mipCount)
                mipIndex = this.mipCount - 1;

            if (this.minimapPtr[mipIndex] == 0)
                BuildMip(mip);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, this.minimapPtr[mipIndex]);

            shader.SetMat4("model", ref matrix);

            GL.BindVertexArray(quadVAO);
            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
        }

        private int BuildQuad(float[] quadVertices)
        {
            int vao = GL.GenVertexArray();
            var vbo = GL.GenBuffer();
            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, 4 * quadVertices.Length, quadVertices, BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * 4);

            return vao;
        }

    }
}
