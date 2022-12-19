using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace ProjectWS.Engine
{
    /// <summary>
    /// Entry point class for the project
    /// Extends MonoBehaviour so that it can be attached to a game object in the unity scene
    /// This is required in unity to gain access to its Start and Update methods
    /// Only one instance of Engine must exist, and this instance is created automatically when Unity runs
    /// Engine holds reference to all of the classes, and is used so, to keep the number of static things close to 0
    /// </summary>
    public class Engine
    {
        public static Settings settings = new Settings();
        bool running = false;
        public bool contextAvailable = false;
        public float deltaTime;
        public float frameTime;
        public float time;
        public string cacheLocation;
        public int focusedRendererID;
        public int total_mem_kb;
        public int cur_avail_mem_kb;
        public int total_mem_mb;
        public int cur_avail_mem_mb;

        public TaskManager.Manager taskManager;
        public Data.ResourceManager.Manager resourceManager;
        public Dictionary<uint, World.World> worlds;
        public Data.GameData data;
        public Input.Input input;
        public Dictionary<uint, FileFormats.Sky.File> skyData;
        public List<Rendering.Renderer> renderers;

        /// <summary>
        /// Code to be executed on application launch
        /// </summary>
        public Engine()
        {
            SettingsSerializer.Load();
            this.taskManager = new TaskManager.Manager(this);
            this.resourceManager = new Data.ResourceManager.Manager(this);
            this.renderers = new List<Rendering.Renderer>();
            this.input = new Input.Input(this);
            this.worlds = new Dictionary<uint, World.World>();
            this.skyData = new Dictionary<uint, FileFormats.Sky.File>();

            // Load engine resources
            //this.resourceManager.textureResources.Add(ResourceManager.EngineTextures.white, new Data.ResourceManager.TextureResource(ResourceManager.EngineTextures.white, this.resourceManager, null));
            //this.taskManager.textureThread.Enqueue(new TaskManager.TextureTask(ResourceManager.EngineTextures.white, TaskManager.Task.JobType.Read, this.resourceManager));
            //this.resourceManager.textureResources.Add(ResourceManager.EngineTextures.brushGradient, new Data.ResourceManager.TextureResource(ResourceManager.EngineTextures.brushGradient, this.resourceManager, null));
            //this.taskManager.textureThread.Enqueue(new TaskManager.TextureTask(ResourceManager.EngineTextures.brushGradient, TaskManager.Task.JobType.Read, this.resourceManager));

            this.running = true;
        }

        public void LoadGameData(string installLocation, Action<Data.GameData> onLoaded)
        {
            this.data = new Data.GameData(this, installLocation, onLoaded);
        }

        public void SetCacheLocation(string cacheLocation) => this.cacheLocation = cacheLocation;

        /// <summary>
        /// Code to be execute every frame
        /// </summary>
        public void Update(float deltaTime, float timeScale)
        {
            if (!this.running) return;

            this.taskManager.Update();
            this.input.Update();

            if (this.input.GetKeyPress(Keys.R))
            {
                for (int i = 0; i < this.renderers.Count; i++)
                {
                    Debug.Log($"Reload Shaders: Renderer[{i}]");
                    this.renderers[i].normalShader?.Load();
                    this.renderers[i].modelShader?.Load();
                    this.renderers[i].wireframeShader?.Load();
                    this.renderers[i].terrainShader?.Load();
                    this.renderers[i].infiniteGridShader?.Load();
                    this.renderers[i].waterShader?.Load();
                    this.renderers[i].fontShader?.Load();
                    this.renderers[i].lightPassShader?.Load();
                }
            }

            for (int i = 0; i < this.renderers.Count; i++)
            {
                this.renderers[i].Update(deltaTime);
            }

            this.deltaTime = deltaTime;
            this.time += this.deltaTime;
            this.frameTime += ((deltaTime / timeScale) - this.frameTime) * 0.03f;

            CalculateGPUMemory();
        }

        const int GL_GPU_MEM_INFO_TOTAL_AVAILABLE_MEM_NVX = 0x9048;
        const int GL_GPU_MEM_INFO_CURRENT_AVAILABLE_MEM_NVX = 0x9049;

        void CalculateGPUMemory()
        {
            this.total_mem_kb = GL.GetInteger((GetPName)GL_GPU_MEM_INFO_TOTAL_AVAILABLE_MEM_NVX);
            this.cur_avail_mem_kb =  GL.GetInteger((GetPName)GL_GPU_MEM_INFO_CURRENT_AVAILABLE_MEM_NVX);
            this.total_mem_mb = (int)((float)this.total_mem_kb / 1024f);
            this.cur_avail_mem_mb = (int)((float)this.cur_avail_mem_kb / 1024f);
        }

        public void Render(int renderer, int frameBuffer)
        {
            this.contextAvailable = true;
            this.renderers[renderer].Render(frameBuffer);

            GL.Flush();
        }

        ~Engine()
        {
            for (int i = 0; i < this.renderers.Count; i++)
            {
                if (this.renderers == null) break;
                if (this.renderers[i] == null) continue;
                this.renderers[i].rendering = false;
            }
            
            if (this.taskManager != null)
                this.taskManager.Destructor();
        }

        public void RenderStats()
        {
            /*
            string gameFPSString = (1f / this.frameTime).ToString("0.00");
            string gameFrameTimeString = (this.frameTime * 1000).ToString("000");
            GUI.Label(new Rect(Screen.width - 300, 60, 300, 20), $"{"Render FPS : ",30}{gameFPSString} ({gameFrameTimeString} ms)");

            if (this.worlds.Count > 0)
            {
                string cullingFPSString = (1f / (float)this.worlds[0].cullingFrametime).ToString("0.00");
                string cullingFrameTimeString = (this.worlds[0].cullingFrametime * 1000).ToString("000");
                GUI.Label(new Rect(Screen.width - 300, 80, 300, 20), $"{"Culling FPS : ", 30}{cullingFPSString} ({cullingFrameTimeString} ms)");
                GUI.Label(new Rect(Screen.width - 300, 100, 300, 20), $"{"Props Rendered : ", 30}{this.worlds[0].propsRendered}");
            }
            */
        }

        public FileFormats.Sky.File GetSky(uint ID)
        {
            if (this.skyData.ContainsKey(ID))
                return this.skyData[ID];

            var worldSkyRecord = this.data.database.worldSky.Get(ID);
            FileFormats.Sky.File sky = new FileFormats.Sky.File(worldSkyRecord.assetPath);
            sky.Read(this.data.GetFileData(worldSkyRecord.assetPath));
            this.skyData.Add(ID, sky);
            return sky;
        }
    }
}