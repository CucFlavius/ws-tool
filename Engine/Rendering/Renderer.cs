namespace ProjectWS.Engine.Rendering
{
    public abstract class Renderer
    {
        public int ID;
        public Engine? engine;
        public Input? input;
        public bool rendering = false;
        public bool mouseOver = false;
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

            SetViewportMode(this.viewportMode);
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

            this.viewports.Clear();

            if (mode == ViewMode.Default)
            {
                // Full view
                this.viewports.Add(new Rendering.Viewport(this, this.input, this.x, this.y, this.width, this.height, true));
            }
            else if (mode == ViewMode.SideBySide)
            {
                // Side by side
                this.viewports.Add(new Rendering.Viewport(this, this.input, this.x, this.y, this.width / 2, this.height, true));
                this.viewports.Add(new Rendering.Viewport(this, this.input, this.width / 2, this.y, this.width / 2, this.height, false));
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
