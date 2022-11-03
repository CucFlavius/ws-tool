using OpenTK.Mathematics;
using ProjectWS.Editor;
using ProjectWS.Engine.Rendering;

//#if UNITY_EDITOR
namespace ProjectWS.TestArea
{
    public class Tests
    {
        Engine.Engine engine;
        Editor.Editor editor;

        public Tests(Engine.Engine engine, Editor.Editor editor)
        {
            this.engine = engine;
            this.editor = editor;
        }

        /// <summary>
        /// Called when project is launched
        /// </summary>
        public void Start()
        {
            Debug.Log("Starting Tests");

            // Load game data
            // This is normally loaded from the UI
            // but for debugging purposes I need it to load directly at runtime in editor so I don't waste time
            string installLocation = @"G:\Reverse Engineering\GameData\Wildstar 1.7.8.16042\";
            string cacheLocation = @"D:\Wildstar1.7.8.16042_Cache\";
            this.engine.LoadGameData(installLocation, OnDataLoaded);
            this.engine.SetCacheLocation(cacheLocation);

            //CompareBetaAndRetailTextures();
            //FindBoneFlags();
        }

        void OnDataLoaded(Engine.Data.GameData data)
        {
            //Program.editor.mainForm.m_gameDataWindow.PopulateTreeView(data);
            StartAfterDataLoaded();
        }

        /// <summary>
        /// Called right after the game data and database have finished loading
        /// </summary>
        void StartAfterDataLoaded()
        {
            Debug.Log("Data Loaded");

            //LoadTestWorld();
            //LoadTestTexture();
            //PrintTestDatabase();
            LoadAnM3ForDebug();
            //TestSkyFile();
        }

        void LoadTestWorld()
        {
            var testWorld = new Engine.World.World(this.engine, 0, true);
            //testWorld.LoadMap(6, new Vector2(64, 64));   // Map\Eastern, Middle of the map
            testWorld.TeleportToWorldLocation(114);
            //testWorld.Teleport(100, -1000, 100, 51);
        }

        void LoadAnM3ForDebug()
        {
            Program.editor.CreateRendererPane(Program.mainWindow, "Model", 1, 1);
            //string path0 = @"Art\Character\Chua\Male\Chua_M.m3";
            string path0 = @"Art\Creature\AgressorBot\AgressorBot.m3";
            //string path0 = @"Art\Prop\Constructed\Quest\Taxi\TaxiKiosk\PRP_Taxi_Kiosk_000.m3";
            //string path0 = @"Art\Prop\Constructed\Ship\Defiance\PRP_Ship_DefianceTransport_000.m3";
            //string path1 = @"Art\Prop\Natural\Tree\Deciduous_RootyMangrove\PRP_Tree_Deciduous_RootyMangrove_Blue_000.m3";
            //string path1 = @"Art\Creature\Asura\Asura.m3";
            int rendererIndex = -1;
            for (int i = 0; i < this.engine.renderers.Count; i++)
            {
                if (this.engine.renderers[i] is Engine.Rendering.ModelRenderer)
                {
                    rendererIndex = i;
                    break;
                }
            }

            if (rendererIndex != -1)
            {
                var modelRenderer = this.engine.renderers[rendererIndex] as Engine.Rendering.ModelRenderer;
                modelRenderer.objects.Add(new Engine.Objects.M3Model(path0, new Vector3(0, 0, 0), this.engine));
                //modelRenderer.SetShadingOverride(Renderer.ShadingOverride.Wireframe);
            }
            else
            {
                Debug.Log("No ModelRenderers found.");
            }
        }

        void PrintTestDatabase()
        {
            this.engine.data.database.worldLayer.Print();
        }

        void TestSkyFile()
        {
            Engine.Data.Sky sky = new Engine.Data.Sky(@"G:\Reverse Engineering\GameData\Wildstar 1.7.8.16042\Data\Sky\Cinematics_Arcterra_EXT_TowerMain.sky");
            sky.Read();
        }
    }
}
//#endif