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
        public int ID;
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
        public Shader infiniteGridShader;

        public List<Camera> cameras;
        public List<Objects.GameObject> gizmos;
        public ShadingOverride shadingOverride;

        public Renderer(Engine engine)
        {
            this.engine = engine;
            this.gizmos = new List<Objects.GameObject>();
        }

        public abstract void Update(float deltaTime);
        public abstract void Render();

        public void SetViewport(int x, int y, int width, int height, int cameraID)
        {
            this.rendering = true;
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
            this.aspect = (float)(this.width / 2) / (float)this.height;
            this.cameras[cameraID].aspectRatio = aspect;

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
            for (int i = 0; i < this.cameras.Count; i++)
            {
                this.cameras[i].aspectRatio = aspect;
            }

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
