
using static ProjectWS.Debug;
using System.Collections.Concurrent;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;

namespace ProjectWS.Engine
{
    public class TextRenderer
    {
        FontGenerator.FreeType freeType;
        ConcurrentQueue<Label3D> labelRenderQueue;
        int drawCalls;

        public struct Label3D
        {
            public string text { get; set; }
            public Vector3 position { get; set; }
            public Vector4 color { get; set; }
            public bool shadow { get; set; }
        }

        public TextRenderer()
        {
            this.freeType = new FontGenerator.FreeType();
            this.labelRenderQueue = new ConcurrentQueue<Label3D>();
        }

        public void Initialize()
        {
            this.freeType.Init();
        }

        public void Render(Rendering.Renderer renderer, Rendering.Viewport viewport)
        {
            this.drawCalls = 0;
            RenderLabels(renderer, viewport);
        }

        public void RenderLabels(Rendering.Renderer renderer, Rendering.Viewport vp)
        {
            for (int i = 0; i < this.labelRenderQueue.Count; i++)
            {
                if (this.labelRenderQueue.TryDequeue(out Label3D label))
                {
                    GL.Enable(EnableCap.Blend);
                    GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

                    Matrix4 projectionM = Matrix4.CreateOrthographicOffCenter(0.0f, vp.width, vp.height, 0.0f, -1.0f, 1.0f);

                    renderer.fontShader.Use();
                    renderer.fontShader.SetMat4("projection", projectionM);

                    vp.PointToScreen(label.position, out var pos);

                    this.drawCalls++;

                    var shadowPos = pos - new Vector2(1.0f, 1.0f);
                    renderer.fontShader.SetColor4("textColor", label.color);
                    this.freeType.RenderText(label.text, shadowPos.X, shadowPos.Y, 0.5f, new Vector2(1f, 0f));

                    if (label.shadow)
                    {
                        this.drawCalls++;
                        renderer.fontShader.SetColor4("textColor", new Vector4(0.0f, 0.0f, 0.0f, 0.5f * label.color.W));
                        this.freeType.RenderText(label.text, pos.X, pos.Y, 0.5f, new Vector2(1f, 0f));
                    }
                }
            }
        }

        public void DrawLabel(string text, Vector3 position, Vector4 color, bool shadow)
        {
            this.labelRenderQueue.Enqueue(new Label3D { text = text, position = position, color = color, shadow = shadow });
        }
    }
}
