using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using ProjectWS.Engine;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace ProjectWS
{
    public class Debug
    {
        const ConsoleColor LOG_COLOR = ConsoleColor.White;
        const ConsoleColor WARNING_COLOR = ConsoleColor.Yellow;
        const ConsoleColor ERROR_COLOR = ConsoleColor.DarkRed;
        const ConsoleColor EXCEPTION_COLOR = ConsoleColor.Red;

        public static ConcurrentQueue<DebugLabel> labelRenderQueue = new ConcurrentQueue<DebugLabel>();

        public static void Log(string text, ConsoleColor color = LOG_COLOR)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        public static void Log(object anything, ConsoleColor color = LOG_COLOR)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(anything.ToString());
            Console.ResetColor();
        }

        public static void Log(bool value, ConsoleColor color = LOG_COLOR)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(value.ToString());
            Console.ResetColor();
        }

        public static void Log(int value, ConsoleColor color = LOG_COLOR)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(value);
            Console.ResetColor();
        }

        public static void Log(uint value, ConsoleColor color = LOG_COLOR)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(value);
            Console.ResetColor();
        }

        public static void Log(float value, ConsoleColor color = LOG_COLOR)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(value);
            Console.ResetColor();
        }

        public static void Log(Matrix4 value, ConsoleColor color = LOG_COLOR)
        {
            Console.ForegroundColor = color;
            Console.WriteLine("Matrix4 " + value.ToString());
            Console.ResetColor();
        }

        public static void Log(string format, object arg0)
        {
            Console.ForegroundColor = LOG_COLOR;
            Console.WriteLine(format, arg0);
            Console.ResetColor();
        }

        public static void LogWarning(string text)
        {
            Console.ForegroundColor = WARNING_COLOR;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        public static void LogError(string text)
        {
            Console.ForegroundColor = ERROR_COLOR;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        public static void LogException(Exception e)
        {
            Console.ForegroundColor = EXCEPTION_COLOR;
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);
            Console.ResetColor();
        }

        public static void Log()
        {
            Console.WriteLine();
        }

        public static void DrawLabel(string text, Vector3 position,  Vector4 color, bool shadow)
        {
            labelRenderQueue.Enqueue(new DebugLabel { text = text, position = position, color = color, shadow = shadow });
        }
    
        public static void RenderLabels(Engine.Rendering.Renderer renderer, Engine.Rendering.Viewport vp)
        {
            for (int i = 0; i < labelRenderQueue.Count; i++)
            {
                if (labelRenderQueue.TryDequeue(out DebugLabel label))
                {
                    GL.Enable(EnableCap.Blend);
                    GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

                    Matrix4 projectionM = Matrix4.CreateOrthographicOffCenter(0.0f, vp.width, vp.height, 0.0f, -1.0f, 1.0f);

                    renderer.fontShader.Use();
                    renderer.fontShader.SetMat4("projection", projectionM);

                    vp.PointToScreen(label.position, out var pos);

                    Engine.Rendering.WorldRenderer.drawCalls++;

                    var shadowPos = pos - new Vector2(1.0f, 1.0f);
                    renderer.fontShader.SetColor4("textColor", label.color);
                    FreeType.RenderText(label.text, shadowPos.X, shadowPos.Y, 0.5f, new Vector2(1f, 0f));

                    if (label.shadow)
                    {
                        Engine.Rendering.WorldRenderer.drawCalls++;
                        renderer.fontShader.SetColor4("textColor", new Vector4(0.0f, 0.0f, 0.0f, 0.5f * label.color.W));
                        FreeType.RenderText(label.text, pos.X, pos.Y, 0.5f, new Vector2(1f, 0f));
                    }

                }
            }
        }

        public struct DebugLabel
        {
            public string text { get; set; }
            public Vector3 position { get; set; }
            public Vector4 color { get; set; }
            public bool shadow { get; set; }
        }
    }
}
