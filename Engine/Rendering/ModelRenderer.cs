using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace ProjectWS.Engine.Rendering
{
    public class ModelRenderer : Renderer
    {
        public List<Objects.GameObject> objects;
        public List<Lighting.Light> lights;
        Input input;
        bool wireframe;

        public ModelRenderer(Engine engine, int ID, Input input) : base(engine)
        {
            Debug.Log("Create Model Renderer " + ID);
            this.ID = ID;
            this.input = input;
            this.objects = new List<Objects.GameObject>();
            this.lights = new List<Lighting.Light>();
            this.cameras = new List<Camera>();
            AddDefaultCamera();
            AddDefaultLight();
        }

        void AddDefaultCamera()
        {
            Debug.Log("Model Renderer : Add Default Camera.");

            this.aspect = (float)(this.width / 2) / (float)this.height;

            var mainCamera = new Camera(this, new Vector3(0, 0, 0), MathHelper.DegreesToRadians(45), this.aspect, 0.1f, 1000.0f);
            var camController = new Components.CameraController(mainCamera, this.input);
            camController.cameraMode = Components.CameraController.CameraMode.Orbit;
            mainCamera.components.Add(camController);
            this.cameras.Add(mainCamera);
            this.activeCamera = mainCamera;
        }

        void AddDefaultLight()
        {
            Vector3 lightPos = new Vector3(100f, 100f, 100f);
            Vector4 lightColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
            this.lights.Add(new Lighting.DirectLight(lightPos, lightColor));

            Vector4 ambientColor = new Vector4(0.2f, 0.2f, 0.2f, 1.0f);
            this.lights.Add(new Lighting.AmbientLight(ambientColor));
        }

        public override void Update(float deltaTime)
        {
            for (int i = 0; i < this.cameras.Count; i++)
            {
                this.cameras[i].Update(deltaTime);
                for (int c = 0; c < this.cameras[i].components.Count; c++)
                {
                    this.cameras[i].components[c].Update(deltaTime);
                }
            }
        }

        public override void Render()
        {
            if (!this.rendering) return;

            GL.ClearColor(new Color4(0.1f, 0.1f, 0.2f, 1.0f));
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            this.shadingOverride = ShadingOverride.Wireframe;

            if (this.shadingOverride == ShadingOverride.ShadedAndWireframe)
            {
                RenderWireframe();
                RenderShaded();
            }
            else if (this.shadingOverride == ShadingOverride.Wireframe)
            {
                RenderWireframe();
            }
            else if (this.shadingOverride == ShadingOverride.Shaded)
            {
                RenderShaded();
            }
            else if (this.shadingOverride == ShadingOverride.Unshaded)
            {
                RenderUnshaded();
            }
            else if (this.shadingOverride == ShadingOverride.Normals)
            {
                RenderNormals();
            }
        }

        void RenderWireframe()
        {
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            GL.LineWidth(1.0f);
            //GL.Enable(EnableCap.LineSmooth);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            RenderInternal(wireframeShader);
        }

        void RenderNormals()
        {
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            GL.Disable(EnableCap.Blend);

            this.normalShader.SetColor4("lightColor", Vector4.Zero);
            this.normalShader.SetColor4("ambientColor", Vector4.One);

            RenderInternal(this.normalShader);
        }

        void RenderShaded()
        {
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            GL.Disable(EnableCap.Blend);

            for (int i = 0; i < this.lights.Count; i++)
            {
                this.lights[i].ApplyToShader(this.modelShader);
            }

            this.modelShader.SetInt("diffuseMap0", 0);
            this.modelShader.SetInt("normalMap0", 1);
            this.modelShader.SetInt("diffuseMap1", 2);
            this.modelShader.SetInt("normalMap1", 3);
            this.modelShader.SetInt("diffuseMap2", 4);
            this.modelShader.SetInt("normalMap2", 5);
            this.modelShader.SetInt("diffuseMap3", 6);
            this.modelShader.SetInt("normalMap3", 7);

            RenderInternal(this.modelShader);
        }

        void RenderUnshaded()
        {
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            GL.Disable(EnableCap.Blend);

            this.modelShader.SetColor4("lightColor", Vector4.Zero);
            this.modelShader.SetColor4("ambientColor", Vector4.One);

            this.modelShader.SetInt("diffuseMap0", 0);
            this.modelShader.SetInt("normalMap0", 1);
            this.modelShader.SetInt("diffuseMap1", 2);
            this.modelShader.SetInt("normalMap1", 3);
            this.modelShader.SetInt("diffuseMap2", 4);
            this.modelShader.SetInt("normalMap2", 5);
            this.modelShader.SetInt("diffuseMap3", 6);
            this.modelShader.SetInt("normalMap3", 7);

            RenderInternal(this.modelShader);
        }

        void RenderInternal(Shader shader)
        {
            // camera/view transformation
            this.activeCamera.Set(shader);

            shader.SetVec3("objectColor", 1.0f, 1.0f, 1.0f);

            shader.Use();

            if (this.objects == null) return;

            for (int i = 0; i < this.objects.Count; i++)
            {
                if (this.objects[i] is Objects.M3Model)
                {
                    // pass model matrix
                    Matrix4 model = this.objects[i].transform.GetMatrix();
                    this.objects[i].Render(model, shader);
                }
            }
        }
    }
}
