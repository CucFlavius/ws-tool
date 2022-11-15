using ProjectWS.Engine.Input;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace ProjectWS.Engine.Rendering
{
    public class Viewport
    {
        public Camera mainCamera;
        public int width, height;
        public int x, y;
        public float aspect;
        public bool interactive;

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
