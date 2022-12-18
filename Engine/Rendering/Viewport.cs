using ProjectWS.Engine.Input;
using OpenTK.Graphics.OpenGL4;
using MathUtils;

namespace ProjectWS.Engine.Rendering
{
    public class Viewport
    {
        public Camera mainCamera;
        public int width, height;
        public int x, y;
        public float aspect;
        public bool interactive;

        // Frame buffers
        public int gBuffer;
        public int gDiffuse, gSpecular, gNormal, gMisc;
        public int rboDepth;

        public Viewport(Renderer renderer, Input.Input input, int x, int y, int width, int height, bool interactive, Camera.CameraMode cameraMode)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
            this.interactive = interactive;
            CalculateAspect();

            this.mainCamera = new Camera(renderer, new Vector3(0, 0, 0), MathHelper.DegreesToRadians(45), this.aspect, 0.1f, 1000.0f);
            var camController = new Components.CameraController(this.mainCamera, input);
            camController.cameraMode = cameraMode;
            this.mainCamera.components.Add(camController);
        }

        public void Recalculate(int x, int y, int w, int h)
        {
            this.x = x;
            this.y = y;
            this.width = w;
            this.height = h;
            CalculateAspect();
            this.mainCamera.aspectRatio = this.aspect;
            //ConfigureGBuffer(this);
        }

        void ConfigureGBuffer(Viewport vp)
        {
            // Configure G-buffer
            if (this.gBuffer == 0)
                this.gBuffer = GL.GenFramebuffer();

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, this.gBuffer);

            // Diffuse //
            if (this.gDiffuse == 0)
                this.gDiffuse = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, gDiffuse);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, vp.width, vp.height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, gDiffuse, 0);

            // Specular //
            if (this.gSpecular == 0)
                this.gSpecular = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, gSpecular);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, vp.width, vp.height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment1, TextureTarget.Texture2D, gSpecular, 0);

            // Normal //
            if (this.gNormal == 0)
                this.gNormal = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, this.gNormal);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, vp.width, vp.height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment2, TextureTarget.Texture2D, this.gNormal, 0);

            // Unknown //
            if (this.gMisc == 0)
                this.gMisc = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, gMisc);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, vp.width, vp.height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment3, TextureTarget.Texture2D, gMisc, 0);

            // tell OpenGL which color attachments we'll use (of this framebuffer) for rendering 
            DrawBuffersEnum[] attachments = new DrawBuffersEnum[] { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1, DrawBuffersEnum.ColorAttachment2, DrawBuffersEnum.ColorAttachment3 };
            GL.DrawBuffers(4, attachments);

            // create and attach depth buffer (renderbuffer)
            if (this.rboDepth == 0)
                this.rboDepth = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, rboDepth);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent, vp.width, vp.height);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, rboDepth);

            // finally check if framebuffer is complete
            if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
                Debug.LogWarning("Framebuffer not complete!");

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }


        void CalculateAspect()
        {
            this.aspect = (float)this.width / (float)this.height;
        }

        public void Use()
        {
            GL.Viewport(this.x, this.y, this.width, this.height);
        }

        public bool PointToScreen(Vector3 point, out Vector2 screen)
        {
            var coord = new Vector4(point, 1.0f);

            // OpenTK matrices use row instead of column vectors so multiplication order is reversed
            coord *= this.mainCamera.view * this.mainCamera.projection;

            if (coord.W == 0)
            {
                screen = Vector2.Zero;
                return false;
            }

            coord.X /= coord.W;
            coord.Y /= coord.W;
            coord.Z /= coord.W;

            coord.X = (coord.X + 1.0f) * this.width * 0.5f;
            coord.Y = this.height - ((coord.Y + 1.0f) * this.height * 0.5f);
            screen = new Vector2(coord.X, coord.Y);

            return true;
        }
    }
}
