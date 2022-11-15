using ProjectWS.Engine.Input;
using OpenTK.Mathematics;
using ProjectWS.Engine.Components;

namespace ProjectWS.Engine.Rendering
{
    public abstract class Renderer
    {
        public int ID;
        public Engine engine;
        public Input.Input input;
        public bool rendering = false;
        public int x;
        public int y;
        public int width;
        public int height;

        public Shader shader;
        public Shader modelShader;
        public Shader wireframeShader;
        public Shader normalShader;
        public Shader terrainShader;
        public Shader waterShader;
        public Shader lineShader;
        public Shader infiniteGridShader;
        public Shader fontShader;

        public List<Viewport>? viewports;
        public List<Objects.GameObject>? gizmos;
        public ShadingOverride shadingOverride;
        public ViewMode viewportMode;

        public Renderer(Engine engine)
        {
            this.engine = engine;
            this.gizmos = new List<Objects.GameObject>();
            this.viewports = new List<Viewport>();
        }

        public abstract void Load();
        public abstract void Update(float deltaTime);
        public abstract void Render();

        public void SetDimensions(int x, int y, int width, int height)
        {
            this.rendering = true;
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;

            SetViewportMode(this.viewportMode);
        }

        public void Resize(int width, int height)
        {
            Debug.Log("Resize " + width + " " + height);

            this.width = width;
            this.height = height;

            RecalculateViewports();
        }

        public void SetShadingOverride(int type) => SetShadingOverride((ShadingOverride)type);

        public void SetShadingOverride(ShadingOverride type)
        {
            this.shadingOverride = type;
        }

        public void SetViewportMode(int mode) => SetViewportMode((ViewMode)mode);

        public void SetViewportMode(ViewMode mode)
        {
            if (this.viewports == null)
                this.viewports = new List<Viewport>();

            this.viewportMode = mode;

            if (mode == ViewMode.Default)
            {
                // Full view

                // Save camera controller
                Vector3 camPos = SaveCameraController();

                if (this.viewports.Count > 0)
                {
                    camPos = this.viewports[0].mainCamera.transform.GetPosition();
                    Debug.Log("Set Viewport Mode " + mode + " | Saved Pos " + camPos.ToString());
                }

                ClearViewports();

                if (this is ModelRenderer)
                {
                    this.viewports.Add(new Viewport(this, this.input, this.x, this.y, this.width, this.height, true, Camera.CameraMode.Orbit));
                }
                else if (this is WorldRenderer)
                {
                    this.viewports.Add(new Viewport(this, this.input, this.x, this.y, this.width, this.height, true, Camera.CameraMode.Fly));
                }

                // Restore camera controller
                RestoreCameraController(camPos);
            }
            else if (mode == ViewMode.SideBySide)
            {
                // Save camera controller
                Vector3 camPos = SaveCameraController();

                ClearViewports();

                // Side by side
                this.viewports.Add(new Viewport(this, this.input, this.x, this.y, this.width / 2, this.height, true, Camera.CameraMode.Fly));
                this.viewports.Add(new Viewport(this, this.input, this.width / 2, this.y, this.width / 2, this.height, false, Camera.CameraMode.OrthoTop));

                // Restore camera controller
                RestoreCameraController(camPos);

                // Temp : Hard coded map camera settings
                this.viewports[1].mainCamera.farDistance = 10000.0f;
                this.viewports[1].mainCamera.transform.SetPosition(camPos.X, 200, camPos.Z);
                this.viewports[1].mainCamera.transform.SetRotation(Quaternion.FromEulerAngles(MathHelper.DegreesToRadians(90), 0, 0));
            }
        }

        Vector3 SaveCameraController()
        {
            if (this.viewports != null)
            {
                if (this.viewports.Count > 0 && this.viewports[0].mainCamera != null && this.viewports[0].mainCamera.components != null)
                {
                    for (int i = 0; i < this.viewports[0].mainCamera.components.Count; i++)
                    {
                        if (this.viewports[0].mainCamera.components[i] is CameraController)
                        {
                            var camController = this.viewports[0].mainCamera.components[i] as CameraController;
                            if (camController != null)
                                return camController.Pos;
                        }
                    }
                }
            }

            return Vector3.Zero;
        }

        void RestoreCameraController(Vector3 camPos)
        {
            if (this.viewports == null) return;

            if (this.viewports.Count > 0 && this.viewports[0].mainCamera != null && this.viewports[0].mainCamera.components != null)
            {
                for (int i = 0; i < this.viewports[0].mainCamera.components.Count; i++)
                {
                    if (this.viewports[0].mainCamera.components[i] is CameraController)
                    {
                        var camController = this.viewports[0].mainCamera.components[i] as CameraController;
                        if (camController != null)
                            camController.Pos = camPos;
                    }
                }
            }
        }

        void ClearViewports()
        {
            if (this.viewports == null)
                this.viewports = new List<Viewport>();

            for (int v = 0; v < this.viewports.Count; v++)
            {
                var vp = this.viewports[v];

                if (vp.mainCamera != null)
                {
                    if (vp.mainCamera.gizmo != null && this.gizmos != null)
                    {
                        this.gizmos.Remove(vp.mainCamera.gizmo);
                    }
                }

            }

            this.viewports.Clear();
        }

        public void RecalculateViewports()
        {
            if (this.viewports == null) return;

            switch(this.viewportMode)
            {
                case ViewMode.Default:
                    this.viewports[0].Recalculate(this.x, this.y, this.width, this.height);
                    break;
                case ViewMode.SideBySide:
                    this.viewports[0].Recalculate(this.x, this.y, this.width / 2, this.height);
                    this.viewports[1].Recalculate(this.width / 2, this.y, this.width / 2, this.height);
                    break;
            }
        }

        public void ToggleFog(bool on)
        {
            if (Engine.settings != null && Engine.settings.wRenderer != null && Engine.settings.wRenderer.toggles != null)
            {
                Debug.Log("FOG " + on);
                Engine.settings.wRenderer.toggles.fog = on;
                SettingsSerializer.Save();
            }
        }

        public void ToggleAreaGrid(bool on)
        {
            if (Engine.settings != null && Engine.settings.wRenderer != null && Engine.settings.wRenderer.toggles != null)
            {
                Debug.Log("AREA GRID " + on);
                Engine.settings.wRenderer.toggles.displayAreaGrid = on;
                SettingsSerializer.Save();
            }
        }

        public void ToggleChunkGrid(bool on)
        {
            if (Engine.settings != null && Engine.settings.wRenderer != null && Engine.settings.wRenderer.toggles != null)
            {
                Debug.Log("CHUNK GRID " + on);
                Engine.settings.wRenderer.toggles.displayChunkGrid = on;
                SettingsSerializer.Save();
            }
        }

        public enum ShadingOverride
        {
            Shaded = 0,
            Unshaded = 1,
            Wireframe = 2,
            ShadedAndWireframe = 3,
            Normals = 4,
        }

        public enum ViewMode
        {
            Default = 0,
            SideBySide = 1,
        }
    }
}
