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
        public Vector2i highlight;
        public Vector2 mousePosMapSpace;
        public bool mouseOverGrid;
        public byte[]? selectionBitmap;
        int bgVAO;
        int selectionLayerVAO;
        int[]? gridIndices;
        int gridVAO;
        private uint selectionBitmapPtr;
        private bool marqueeVisible;
        public Vector2 marqueeMin;
        public Vector2 marqueeMax;
        public Vector2 marqueeMaxPrevious;
        private int[]? lineSquareIndices;
        private int lineSquareVAO;
        public bool deselectMode;
        public bool showGrid = true;

        const int MAP_SIZE = 128;
        readonly Color32 envColor = new Color32(10, 10, 20, 255);
        readonly Color32 selectedCellColor = new Color32(255, 255, 0, 128);
        readonly Color32 deselectedCellColor = new Color32(0, 0, 0, 0);
        readonly Color32 backgroundCellColor = new Color32(50, 50, 50, 255);
        readonly Color32 hasAreaColor = new Color32(80, 80, 80, 255);
        readonly Color32 gridColor = new Color32(20, 20, 20, 255);
        const float halfQuad = 0.5f;

        readonly float[] quadVertices = new float[]
        {
            // positions                // texture Coords
            -halfQuad, 0.0f, halfQuad,  0.0f, 1.0f,
            -halfQuad, 0.0f,-halfQuad,  0.0f, 0.0f,
             halfQuad, 0.0f, halfQuad,  1.0f, 1.0f,
             halfQuad, 0.0f,-halfQuad,  1.0f, 0.0f,
        };


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
            this.marqueeShader = new Shader("shaders/marquee_vert.glsl", "shaders/marquee_frag.glsl");

            // setup marqueue quad
            BuildLineSquare();

            // setup Background
            this.bgVAO = BuildQuad(quadVertices);

            // setup Selection Layer
            this.selectionLayerVAO = BuildQuad(quadVertices);

            BuildGrid();

            this.selectionBitmapPtr = BuildBitmap(this.deselectedCellColor, MAP_SIZE, MAP_SIZE, out this.selectionBitmap);

            this.textRenderer?.Initialize();
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

        public void BuildGrid()
        {
            var line = new Vector3[MAP_SIZE * 4];

            int idx = 0;
            for (int x = 0; x < MAP_SIZE; x++)
            {
                line[idx++] = new Vector3(x, 0, 0);
                line[idx++] = new Vector3(x, 0, MAP_SIZE);
                line[idx++] = new Vector3(0, 0, x);
                line[idx++] = new Vector3(MAP_SIZE, 0, x);
            }

            this.gridIndices = new int[MAP_SIZE * 4];
            for (int i = 0; i < MAP_SIZE * 4; i += 2)
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

        public void BuildLineSquare()
        {
            var line = new Vector3[]
            {
                new Vector3(-0.5f, 0, -0.5f),
                new Vector3(-0.5f, 0, 0.5f),
                new Vector3(0.5f, 0, 0.5f),
                new Vector3(0.5f, 0, -0.5f),
            };

            this.lineSquareIndices = new int[]
            {
                0, 1, 1, 2, 2, 3, 3, 0
            };

            int _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, line.Length * 3 * 4, line, BufferUsageHint.StaticDraw);

            this.lineSquareVAO = GL.GenVertexArray();
            GL.BindVertexArray(this.lineSquareVAO);

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);

            int _elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, this.lineSquareIndices.Length * 4, this.lineSquareIndices, BufferUsageHint.StaticDraw);

            GL.BindVertexArray(0);
        }

        uint BuildBitmap(Color32 baseColor, int w, int h, out byte[] data)
        {
            uint ptr = 0;
            data = new byte[w * h * 4];
            for (int i = 0; i < w * h * 4; i += 4)
            {
                data[i] = baseColor.R;
                data[i + 1] = baseColor.G;
                data[i + 2] = baseColor.B;
                data[i + 3] = baseColor.A;
            }

            GL.GenTextures(1, out ptr);
            GL.BindTexture(TextureTarget.Texture2D, ptr);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.GenerateMipmap, 0);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 0);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, w, h, 0, PixelFormat.Rgba, PixelType.UnsignedByte, data);

            return ptr;
        }

        public void UpdateBitmap(uint ptr, byte[] data)
        {
            GL.BindTexture(TextureTarget.Texture2D, ptr);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, MAP_SIZE, MAP_SIZE, 0, PixelFormat.Rgba, PixelType.UnsignedByte, data);
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

            //textRenderer?.DrawLabel3D("TEST", new Vector3(0, 0.1f, 0), new Vector4(1, 1, 0, 1), true);

            // Determine which quad mouse is over
            var vp = this.viewports![0];

            this.mouseOverGrid = false;

            if (this.ID == this.engine.focusedRendererID)
            {
                if (vp.interactive)
                {
                    var mousePos = this.engine.input.GetMousePosition();

                    if (mousePos.X >= 0 && mousePos.Y >= 0)
                    {
                        this.mouseOverGrid = true;
                        var vpSize = new Vector2((float)vp.width, (float)vp.height);
                        var oCamera = vp.mainCamera as OrthoCamera;
                        if (oCamera != null)
                        {
                            this.mousePosMapSpace = ((mousePos.Xy - (vpSize / 2)) / oCamera.zoom) + oCamera.transform.GetPosition().Xz;

                            this.highlight.X = (int)this.mousePosMapSpace.X;
                            this.highlight.Y = (int)this.mousePosMapSpace.Y;

                            // Out of map bounds check
                            if (this.highlight.X < 0 || this.highlight.X >= MAP_SIZE || this.highlight.Y < 0 || this.highlight.Y >= MAP_SIZE)
                                this.mouseOverGrid = false;
                        }
                    }
                }
            }

            if (this.engine.input.LMBClicked == Input.Input.ClickState.MouseButtonDown)
            {
                this.marqueeMin = mousePosMapSpace;
                this.marqueeVisible = true;

                if (this.engine.input.keyStates[OpenTK.Windowing.GraphicsLibraryFramework.Keys.LeftAlt] ||
                    this.engine.input.keyStates[OpenTK.Windowing.GraphicsLibraryFramework.Keys.RightAlt])
                    this.deselectMode = true;
                else
                    this.deselectMode = false;
            }
            if (this.engine.input.LMB)
            {
                this.marqueeMax = this.mousePosMapSpace;

                if (this.marqueeMax != this.marqueeMaxPrevious)
                {
                    this.marqueeMaxPrevious = this.marqueeMax;

                    var minX = MathF.Min(this.marqueeMin.X, this.marqueeMax.X);
                    var maxX = MathF.Max(this.marqueeMin.X, this.marqueeMax.X);
                    var minY = MathF.Min(this.marqueeMin.Y, this.marqueeMax.Y);
                    var maxY = MathF.Max(this.marqueeMin.Y, this.marqueeMax.Y);

                    for (int x = (int)minX; x <= (int)maxX; x++)
                    {
                        for (int y = (int)minY; y <= (int)maxY; y++)
                        {
                            if (deselectMode)
                                DeselectCell(x, y);
                            else
                                SelectCell(x, y);
                        }
                    }

                    UpdateBitmap(this.selectionBitmapPtr, this.selectionBitmap);
                }
            }
            if (this.engine.input.LMBClicked == Input.Input.ClickState.MouseButtonUp)
            {
                this.marqueeVisible = false;

                if ((int)this.marqueeMin.X == (int)this.marqueeMax.X &&
                    (int)this.marqueeMin.Y == (int)this.marqueeMax.Y)
                {
                    if (this.mouseOverGrid)
                    {
                        if (deselectMode)
                            DeselectCell(this.highlight.X, this.highlight.Y);
                        else
                            SelectCell(this.highlight.X, this.highlight.Y);

                        UpdateBitmap(this.selectionBitmapPtr, this.selectionBitmap);
                    }
                }
            }

            //Debug.Log(this.marqueueVisible + " " + this.marqueueMin + " " + this.marqueueMax);
        }

        public void SelectCell(int x, int y)
        {
            if (x < 0 || y < 0 || x >= MAP_SIZE || y >= MAP_SIZE)
                return;
            var linearCoord = (int)((y * 4 * MAP_SIZE) + (x * 4));
            this.selectionBitmap![linearCoord] = selectedCellColor.R;
            this.selectionBitmap[linearCoord + 1] = selectedCellColor.G;
            this.selectionBitmap[linearCoord + 2] = selectedCellColor.B;
            this.selectionBitmap[linearCoord + 3] = selectedCellColor.A;
        }

        public void DeselectCell(int x, int y)
        {
            if (x < 0 || y < 0 || x >= MAP_SIZE || y >= MAP_SIZE)
                return;
            var linearCoord = (int)((y * 4 * MAP_SIZE) + (x * 4));
            this.selectionBitmap![linearCoord] = deselectedCellColor.R;
            this.selectionBitmap[linearCoord + 1] = deselectedCellColor.G;
            this.selectionBitmap[linearCoord + 2] = deselectedCellColor.B;
            this.selectionBitmap[linearCoord + 3] = deselectedCellColor.A;
        }

        public void DeselectAllCells()
        {
            for (int x = 0; x < MAP_SIZE; x++)
            {
                for (int y = 0; y < MAP_SIZE; y++)
                {
                    DeselectCell(x, y);
                }
            }

            UpdateBitmap(this.selectionBitmapPtr, this.selectionBitmap);
        }

        public override void Render(int frameBuffer)
        {
            if (!this.rendering) return;

            //GL.Viewport(this.x, this.y, this.width, this.height);
            GL.ClearColor(envColor.R / 255f, envColor.G / 255f, envColor.B / 255f, envColor.A / 255f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            GL.Disable(EnableCap.Blend);
            GL.Enable(EnableCap.DepthTest);

            this.viewports?[0]?.Use();

            // Render BG
            RenderBackground(0);

            // Render highlight quad
            //RenderHighlight();

            // Render grid
            if (showGrid)
                RenderGrid(100);

            // render marqueue
            RenderMarqueue(20);

            // render selection
            RenderSelectionLayer(10);

            // Render Text
            //this.textRenderer?.Render(this, this.viewports![0]);
        }

        private void RenderGrid(int layer)
        {
            GL.Disable(EnableCap.Blend);

            this.lineShader.Use();
            this.viewports?[0]?.mainCamera?.SetToShader(this.lineShader);
            var gridMat = Matrix4.CreateTranslation(0, layer/1000f, 0);
            this.lineShader.SetMat4("model", ref gridMat);
            this.lineShader.SetColor("lineColor", gridColor);
            GL.BindVertexArray(this.gridVAO);
            GL.DrawElements(BeginMode.Lines, this.gridIndices!.Length, DrawElementsType.UnsignedInt, 0);
        }

        private void RenderMarqueue(int layer)
        {
            if (this.marqueeVisible)
            {
                GL.Disable(EnableCap.Blend);

                this.marqueeShader.Use();
                this.viewports?[0]?.mainCamera?.SetToShader(this.marqueeShader);

                // Calculate the position and size of the quad
                Vector2 position = (this.marqueeMin + this.marqueeMax) / 2;
                Vector2 size = this.marqueeMax - this.marqueeMin;

                // Create the matrix that transforms the quad
                var quadMat = Matrix4.CreateScale(size.X, 1f, size.Y) * Matrix4.CreateTranslation(position.X, layer / 1000f, position.Y);

                this.marqueeShader.SetMat4("model", ref quadMat);
                this.marqueeShader.SetColor("lineColor", new Color(1f, 1f, 1f, 1.0f));
                this.marqueeShader.SetFloat("aspectRatio", this.viewports![0].aspect);

                GL.BindVertexArray(this.lineSquareVAO);
                GL.DrawElements(BeginMode.Lines, this.lineSquareIndices!.Length, DrawElementsType.UnsignedInt, 0);
            }
        }

        private void RenderBackground(int layer)
        {
            GL.Disable(EnableCap.Blend);

            this.lineShader.Use();
            this.viewports?[0]?.mainCamera?.SetToShader(this.lineShader);
            var bgMat = Matrix4.CreateScale(MAP_SIZE) * Matrix4.CreateTranslation(MAP_SIZE / 2, layer / 1000f, MAP_SIZE / 2);
            this.lineShader.SetMat4("model", ref bgMat);
            this.lineShader.SetColor("lineColor", backgroundCellColor);

            GL.BindVertexArray(this.bgVAO);
            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
            GL.BindVertexArray(0);
        }

        private void RenderSelectionLayer(int layer)
        {
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            this.mapTileShader.Use();
            this.viewports?[0]?.mainCamera?.SetToShader(this.mapTileShader);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, this.selectionBitmapPtr);
            var bgMat = Matrix4.CreateScale(MAP_SIZE) * Matrix4.CreateTranslation(MAP_SIZE / 2, layer / 1000f, MAP_SIZE / 2);
            this.mapTileShader.SetMat4("model", ref bgMat);

            GL.BindVertexArray(this.selectionLayerVAO);
            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
            GL.BindVertexArray(0);
        }
    }
}
