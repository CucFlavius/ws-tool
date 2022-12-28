using MathUtils;
using OpenTK.Graphics.OpenGL4;
using ProjectWS.Engine.Components;
using SharpFont;
using System.Security.Cryptography;
using static System.Net.Mime.MediaTypeNames;

namespace ProjectWS.Engine.Rendering
{
    public class MapRenderer : Renderer
    {
        public TextRenderer? textRenderer;
        public Vector2 highlight;
        public bool highlightVisible;
        public byte[] selectionBitmap;
        int hightlightQuadVAO;
        int bgVAO;
        int[] gridIndices;
        int gridVAO;
        private uint selectionBitmapPtr;

        public MapRenderer(Engine engine, int ID, Input.Input input) : base(engine)
        {
            Debug.Log("Create Model Renderer " + ID);
            this.ID = ID;
            this.input = input;

            SetViewportMode(0);

            this.textRenderer = new TextRenderer();
        }

        public override void Load()
        {
            this.mapTileShader = new Shader("shaders/maptile_vert.glsl", "shaders/maptile_frag.glsl");
            this.fontShader = new Shader("shaders/font_vert.glsl", "shaders/font_frag.glsl");
            this.lineShader = new Shader("shaders/line_vert.glsl", "shaders/line_frag.glsl");

            // Build geometry
            BuildMapGeometry();
            BuildGrid();

            this.selectionBitmap = new byte[128 * 128 * 4];
            for (int i = 0; i < 128 * 128 * 4; i+=4)
            {
                this.selectionBitmap[i] = 70;
                this.selectionBitmap[i + 1] = 70;
                this.selectionBitmap[i + 2] = 70;
                this.selectionBitmap[i + 3] = 255;

            }
            BuildBitmap(this.selectionBitmap, out this.selectionBitmapPtr);

            this.textRenderer?.Initialize();
        }

        public void BuildMapGeometry()
        {
            float hSize = 0.5f;
            float[] quadVertices = new float[]
            {
                // positions        // texture Coords
                -hSize, 0.0f, hSize, 0.0f, 1.0f,
                -hSize, 0.0f,-hSize, 0.0f, 0.0f,
                hSize,  0.0f, hSize, 1.0f, 1.0f,
                hSize,  0.0f,-hSize, 1.0f, 0.0f,
            };

            // setup highlight quad VAO
            this.hightlightQuadVAO = GL.GenVertexArray();
            var highlightQuadVBO = GL.GenBuffer();
            GL.BindVertexArray(this.hightlightQuadVAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, highlightQuadVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, 4 * quadVertices.Length, quadVertices, BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * 4);

            // setup Background VAO
            this.bgVAO = GL.GenVertexArray();
            var bgVBO = GL.GenBuffer();
            GL.BindVertexArray(this.bgVAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, bgVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, 4 * quadVertices.Length, quadVertices, BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * 4);
        }

        public void BuildGrid()
        {
            int max = 128;

            var line = new Vector3[max * 4];

            int idx = 0;
            for (int x = 0; x < max; x++)
            {
                line[idx++] = new Vector3(x, 0, 0);
                line[idx++] = new Vector3(x, 0, max);
                line[idx++] = new Vector3(0, 0, x);
                line[idx++] = new Vector3(max, 0, x);
            }

            this.gridIndices = new int[max * 4];
            for (int i = 0; i < max * 4; i += 2)
            {
                this.gridIndices[i] = i;
                this.gridIndices[i + 1] = i + 1;
            }

            int _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, line.Length * 3 * 4, line, BufferUsageHint.StaticDraw);

            this.gridVAO = GL.GenVertexArray();
            GL.BindVertexArray(this.gridVAO);

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);

            int _elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, this.gridIndices.Length * 4, this.gridIndices, BufferUsageHint.StaticDraw);

            GL.BindVertexArray(0);
        }


        void BuildBitmap(byte[] data, out uint ptr)
        {
            if (data == null) { ptr = 0; return; }

            GL.GenTextures(1, out ptr);
            GL.BindTexture(TextureTarget.Texture2D, ptr);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.GenerateMipmap, 0);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 0);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 128, 128, 0, PixelFormat.Rgba, PixelType.UnsignedByte, data);
        }

        public void UpdateBitmap(uint ptr, byte[] data)
        {
            GL.BindTexture(TextureTarget.Texture2D, ptr);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 128, 128, 0, PixelFormat.Rgba, PixelType.UnsignedByte, data);
        }

        public override void Update(float deltaTime)
        {
            if (this.viewports == null) return;

            for (int i = 0; i < this.viewports?.Count; i++)
            {
                this.viewports[i].mainCamera?.Update(deltaTime);
                for (int c = 0; c < this.viewports[i].mainCamera?.components.Count; c++)
                {
                    this.viewports[i].mainCamera?.components[c].Update(deltaTime);
                }
            }

            textRenderer?.DrawLabel3D("TEST", new Vector3(0, 0.1f, 0), new Vector4(1, 1, 0, 1), true);

            // Determine which quad mouse is over
            var vp = this.viewports![0];

            this.highlightVisible = false;

            if (this.ID == this.engine.focusedRendererID)
            {
                if (vp.interactive)
                {
                    var mousePos = this.engine.input.GetMousePosition();

                    if (mousePos.X >= 0 && mousePos.Y >= 0)
                    {
                        this.highlightVisible = true;
                        var vpSize = new Vector2((float)vp.width, (float)vp.height);
                        var oCamera = vp.mainCamera as OrthoCamera;
                        if (oCamera != null)
                        {
                            this.highlight = ((-mousePos.Xy + (vpSize / 2)) / oCamera.zoom) + oCamera.transform.GetPosition().Xz;

                            this.highlight.X = (int)this.highlight.X;
                            this.highlight.Y = (int)this.highlight.Y;

                            // Out of map bounds check
                            if (this.highlight.X < 0 || this.highlight.X >= 128 || this.highlight.Y < 0 || this.highlight.Y >= 128)
                                this.highlightVisible = false;
                        }
                    }
                }
            }

            if (this.highlightVisible)
            {
                if (this.engine.input.LMBClicked == Input.Input.ClickState.MouseButtonDown)
                {
                    var linearCoord = (int)((this.highlight.Y * 512) + (this.highlight.X * 4));
                    var R = this.selectionBitmap[linearCoord];
                    if (R == 70)
                    {
                        this.selectionBitmap[linearCoord] = 255;
                    }
                    else
                    {
                        this.selectionBitmap[linearCoord] = 70;
                    }

                    UpdateBitmap(this.selectionBitmapPtr, this.selectionBitmap);
                }
            }
        }

        public override void Render(int frameBuffer)
        {
            if (!this.rendering) return;

            //GL.Viewport(this.x, this.y, this.width, this.height);
            GL.ClearColor(0.15f, 0.1f, 0.15f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            GL.Disable(EnableCap.Blend);
            GL.Enable(EnableCap.DepthTest);

            this.viewports?[0]?.Use();

            // Render BG
            this.mapTileShader.Use();
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, this.selectionBitmapPtr);
            this.viewports?[0]?.mainCamera?.SetToShader(this.mapTileShader);
            var bgMat = Matrix4.CreateTranslation(0.5f, 0, 0.5f);
            bgMat *= Matrix4.CreateScale(128);
            this.mapTileShader.SetMat4("model", ref bgMat);

            GL.BindVertexArray(this.bgVAO);
            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
            GL.BindVertexArray(0);

            // Render highlight quad
            if (this.highlightVisible)
            {
                this.lineShader.Use();
                this.viewports?[0]?.mainCamera?.SetToShader(this.lineShader);
                var quadMat = Matrix4.CreateTranslation(this.highlight.X + 0.5f, 0.1f, this.highlight.Y + 0.5f);
                this.lineShader.SetMat4("model", ref quadMat);
                this.lineShader.SetColor("lineColor", new Color(0.4f, 0.4f, 0.4f, 1.0f));
                GL.BindVertexArray(this.hightlightQuadVAO);
                GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
                GL.BindVertexArray(0);
            }

            // Render grid
            this.lineShader.Use();
            this.viewports?[0]?.mainCamera?.SetToShader(this.lineShader);
            var gridMat = Matrix4.CreateTranslation(0, 0.2f, 0);
            this.lineShader.SetMat4("model", ref gridMat);
            this.lineShader.SetColor("lineColor", new Color(0.2f, 0.2f, 0.2f, 1.0f));
            GL.BindVertexArray(this.gridVAO);
            GL.DrawElements(BeginMode.Lines, this.gridIndices.Length, DrawElementsType.UnsignedInt, 0);

            // Render Text
            this.textRenderer?.Render(this, this.viewports[0]);
        }
    }
}
