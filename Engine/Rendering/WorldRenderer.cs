using ProjectWS.Engine.Input;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using ProjectWS.Engine.Objects.Gizmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Windowing.Desktop;

namespace ProjectWS.Engine.Rendering
{
    public class WorldRenderer : Renderer
    {
        World.World? world;
        ShaderParams.FogParameters fogParameters;
        ShaderParams.TerrainEditorParameters tEditorParameters;
        ShaderParams.SunParameters sunParameters;
        ShaderParams.EnvironmentParameters envParameters;

        public static int drawCalls;

        Color4 envColor = new Color4(0.7f, 0.7f, 0.7f, 1.0f);

        public WorldRenderer(Engine engine, int ID, Input.Input input) : base(engine)
        {
            Debug.Log("Create World Renderer " + ID);
            this.ID = ID;
            this.input = input;
            //this.objects = new List<Objects.GameObject>();
            //this.lights = new List<Lighting.Light>();
            SetViewportMode(ViewMode.Default);
            //AddDefaultLight();
            //this.fogParameters = new FogParameters(this.envColor, 0, 1000, 0.1f, 0);  // Linear
            this.fogParameters = new ShaderParams.FogParameters(this.envColor, 0, 0, 0.0025f, 2);    // Exponential
            this.tEditorParameters = new ShaderParams.TerrainEditorParameters(false, false);
            this.sunParameters = new ShaderParams.SunParameters(new Vector3(1.0f, 1.0f, 1.0f), new Vector3(1.0f, 1.0f, 1.0f), 1.0f);
            this.envParameters = new ShaderParams.EnvironmentParameters(new Vector3(0.4f, 0.4f, 0.4f));
        }

        public void SetWorld(World.World world) => this.world = world;

        public override void Render()
        {
            if (this.viewports == null) return;

            drawCalls = 0;

            GL.ClearColor(this.envColor);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

            for (int v = 0; v < this.viewports.Count; v++)
            {
                this.viewports[v].Use();

                // Set global shader parameters
                if (Engine.settings != null && Engine.settings.wRenderer != null && Engine.settings.wRenderer.toggles != null)
                {
                    this.fogParameters.Toggle(Engine.settings.wRenderer.toggles.fog);
                    this.tEditorParameters.enableAreaGrid = Engine.settings.wRenderer.toggles.displayAreaGrid;
                    this.tEditorParameters.enableChunkGrid = Engine.settings.wRenderer.toggles.displayChunkGrid;
                }

                // Render World
                if (world != null)
                {
                    // Terrain
                    this.terrainShader.Use();
                    this.fogParameters.SetToShader(this.terrainShader);
                    this.tEditorParameters.SetToShader(this.terrainShader);
                    this.sunParameters.SetToShader(this.terrainShader);
                    this.envParameters.SetToShader(this.terrainShader);
                    this.viewports[v].mainCamera.SetToShader(this.terrainShader);
                    this.world.RenderTerrain(this.terrainShader);

                    // Water
                    this.waterShader.Use();
                    this.viewports[v].mainCamera.SetToShader(this.waterShader);
                    this.waterShader.SetMat4("model", Matrix4.Identity);    // Water vertices are in world space
                    this.world.RenderWater(this.waterShader);
                }

                // Render Gizmos
                if (this.gizmos != null)
                {
                    for (int i = 0; i < this.gizmos.Count; i++)
                    {
                        if (this.gizmos[i] == null) continue;

                        if (this.gizmos[i] is Objects.Gizmos.CameraGizmo)
                        {
                            var camGizmo = this.gizmos[i] as Objects.Gizmos.CameraGizmo;
                            if (camGizmo != null)
                                if (camGizmo.camera == this.viewports[v].mainCamera)
                                    continue;
                        }

                        if (this.gizmos[i] is InfiniteGridGizmo)
                        {
                            this.infiniteGridShader.Use();
                            this.viewports[v].mainCamera.SetToShader(this.infiniteGridShader);
                            this.gizmos[i].Render(Matrix4.Identity, this.infiniteGridShader);
                        }
                        else
                        {
                            this.lineShader.Use();
                            this.viewports[v].mainCamera.SetToShader(this.lineShader);
                            this.gizmos[i].Render(Matrix4.Identity, this.lineShader);
                        }
                    }
                }
            }
        }

        public override void Update(float deltaTime)
        {
            if (this.viewports == null) return;

            for (int v = 0; v < this.viewports.Count; v++)
            {
                this.viewports[v].mainCamera.Update(deltaTime);
                for (int c = 0; c < this.viewports[v].mainCamera.components.Count; c++)
                {
                    if (this.viewports[v].interactive)
                        this.viewports[v].mainCamera.components[c].Update(deltaTime);
                }
            }

            // Temp : Updating topdown camera manually
            if (this.viewports.Count == 2)
            {
                var p = this.viewports[0].mainCamera.transform.GetPosition() + new Vector3(0, 1000, 0);
                this.viewports[1].mainCamera.view = Matrix4.LookAt(p, p - Vector3.UnitY, Vector3.UnitZ);
                this.viewports[1].mainCamera.transform.SetPosition(p);
            }

            if (this.world != null)
                this.world.Update(deltaTime);
        }
    }
}
