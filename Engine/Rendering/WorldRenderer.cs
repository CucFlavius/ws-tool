using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectWS.Engine.Rendering
{
    public class WorldRenderer : Renderer
    {
        World.World? world;
        Input input;
        public static int drawCalls;

        public WorldRenderer(Engine engine, int ID, Input input) : base(engine)
        {
            Debug.Log("Create World Renderer " + ID);
            this.ID = ID;
            this.input = input;
            //this.objects = new List<Objects.GameObject>();
            //this.lights = new List<Lighting.Light>();
            this.cameras = new List<Camera>();
            AddDefaultCamera();
            //AddDefaultLight();
        }

        void AddDefaultCamera()
        {
            Debug.Log("World Renderer : Add Default Camera.");

            this.aspect = (float)(this.width / 2) / (float)this.height;

            var mainCamera = new Camera(this, new Vector3(0, 0, 0), MathHelper.DegreesToRadians(45), this.aspect, 0.1f, 1000.0f);
            var camController = new Components.CameraController(mainCamera, this.input);
            camController.cameraMode = Components.CameraController.CameraMode.Fly;
            mainCamera.components.Add(camController);
            this.cameras.Add(mainCamera);
            this.activeCamera = mainCamera;

            var secondCamera = new Camera(this, new Vector3(0, 200, 0), MathHelper.DegreesToRadians(45), this.aspect, 0.1f, 10000.0f);
            this.cameras.Add(secondCamera);
            secondCamera.transform.SetRotation(Quaternion.FromEulerAngles(MathHelper.DegreesToRadians(90), 0, 0));
        }

        public void SetWorld(World.World world) => this.world = world;

        public override void Render()
        {
            drawCalls = 0;

            GL.ClearColor(new Color4(0.1f, 0.1f, 0.1f, 1.0f));
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // render 1
            GL.Viewport(0, 0, this.width / 2, this.height);
            if (world != null)
            {
                terrainShader.Use();
                this.activeCamera.Set(terrainShader);
                this.world.Render(terrainShader);
            }

            // render gizmos 1
            lineShader.Use();
            this.activeCamera.Set(lineShader);
            for (int i = 0; i < this.engine.gizmos.Count; i++)
            {
                //if (this.engine.gizmos[i] is Objects.Gizmos.CameraGizmo)
                //    if ((this.engine.gizmos[i] as Objects.Gizmos.CameraGizmo).camera == this.activeCamera)
                //        continue;

                this.engine.gizmos[i].Render(Matrix4.Identity, lineShader);
            }

            // render 2
            GL.Viewport(this.width / 2, 0, this.width / 2, this.height);
            if (world != null)
            {
                terrainShader.Use();
                this.cameras[1].Set(terrainShader);
                this.world.Render(terrainShader);
            }

            // render gizmos 2
            lineShader.Use();
            this.cameras[1].Set(lineShader);
            for (int i = 0; i < this.engine.gizmos.Count; i++)
            {
                //if (this.engine.gizmos[i] is Objects.Gizmos.CameraGizmo)
                //    if ((this.engine.gizmos[i] as Objects.Gizmos.CameraGizmo).camera == this.cameras[1])
                //        continue;

                this.engine.gizmos[i].Render(Matrix4.Identity, lineShader);
            }

            GL.Viewport(0, 0, this.width, this.height); //restore default

        }

        public override void Update(float deltaTime)
        {
            for (int i = 0; i < this.cameras.Count; i++)
            {
                for (int c = 0; c < this.cameras[i].components.Count; c++)
                {
                    this.cameras[i].components[c].Update(deltaTime);
                }
                this.cameras[i].Update(deltaTime);
            }

            // Temp : Updating topdown camera manually
            var p = this.cameras[0].transform.GetPosition() + new Vector3(0, 1000, 0);
            this.cameras[1].view = Matrix4.LookAt(p, p - Vector3.UnitY, Vector3.UnitZ);
            this.cameras[1].transform.SetPosition(this.cameras[0].transform.GetPosition() + new Vector3(0, 1000, 0));

            if (this.world != null)
                this.world.Update(deltaTime);
        }
    }
}
