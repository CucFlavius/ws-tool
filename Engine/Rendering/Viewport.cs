using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using ProjectWS.Engine;

namespace ProjectWS.Engine.Rendering
{
    public class Viewport
    {
        public Camera mainCamera;
        public int width, height;
        public int x, y;
        public float aspect;
        public bool interactive;

        public Viewport(Renderer renderer, Input input, int x, int y, int width, int height, bool interactive, Camera.CameraMode cameraMode)
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
        }

        void CalculateAspect()
        {
            this.aspect = (float)this.width / (float)this.height;
        }

        public void Use()
        {
            GL.Viewport(this.x, this.y, this.width, this.height);
        }
    }
}
