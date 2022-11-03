using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectWS.Engine.Rendering
{
    public abstract class Renderer
    {
        public Engine engine;
        public bool rendering = false;
        public bool mouseOver = false;
        public float aspect;
        public int x;
        public int y;
        public int width;
        public int height;

        public Shader shader;
        public Shader modelShader;
        public Shader wireframeShader;
        public Shader normalShader;
        public Shader terrainShader;
        public Shader lineShader;
        public int ID;
        public Camera activeCamera;
        public List<Camera> cameras;
        public ShadingOverride shadingOverride;

        public Renderer(Engine engine)
        {
            this.engine = engine;
        }

        public abstract void Update(float deltaTime);
        public abstract void Render();

        public void SetViewport(int x, int y, int width, int height)
        {
            this.rendering = true;
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
            this.aspect = (float)(this.width / 2) / (float)this.height;
            this.activeCamera.aspectRatio = aspect;

            if (this.cameras == null)
            {
                Debug.LogWarning("Cameras == null");
                return;
            }

            for (int i = 0; i < this.cameras.Count; i++)
            {
                this.cameras[i].aspectRatio = aspect;
            }

            GL.Viewport(this.x, this.y, this.width, this.height);
            GL.ClearColor(0.3f, 0.2f, 0.3f, 1.0f);
            GL.Enable(EnableCap.DepthTest);
        }

        public void DestroyViewport()
        {
            this.rendering = false;
        }

        public void Move(int x, int y)
        {
            this.x = x;
            this.y = y;
            GL.Viewport(this.x, this.y, this.width, this.height);
        }

        public void Resize(int width, int height)
        {
            Debug.Log("Resize " + width + " " + height);

            this.width = width;
            this.height = height;
            GL.Viewport(this.x, this.y, this.width, this.height);
            this.aspect = (float)(this.width / 2) / (float)this.height;
            this.activeCamera.aspectRatio = aspect;

            if (this.cameras == null)
            {
                Debug.LogWarning("Cameras == null");
                return;
            }

            for (int i = 0; i < this.cameras.Count; i++)
            {
                this.cameras[i].aspectRatio = aspect;
            }

        }

        public void SetShadingOverride(ShadingOverride type)
        {
            this.shadingOverride = type;
        }

        public enum ShadingOverride
        {
            Shaded = 0,
            Unshaded = 1,
            Wireframe = 2,
            ShadedAndWireframe = 3,
            Normals = 4,
        }
    }
}
