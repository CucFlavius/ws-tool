using OpenTK.Mathematics;
using ProjectWS.Editor;
using ProjectWS.Engine.Rendering;

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
            //TestSkyFile();
        }

        void OnDataLoaded(Engine.Data.GameData data)
        {
            // Populate world sky list in sky editor pane
            int idx = 0;
            int defaultIdx = 0;
            foreach (var item in data.database.worldSky.records)
            {
                Program.editor.skyEditorPane.skies.Add(item.Value);
                if (item.Value.ID == 21)
                    defaultIdx = idx;

                idx++;
            }
            Program.editor.skyEditorPane.skyDataGrid.SelectedIndex = defaultIdx;

            //Program.editor.mainForm.m_gameDataWindow.PopulateTreeView(data);
            StartAfterDataLoaded();
        }

        /// <summary>
        /// Called right after the game data and database have finished loading
        /// </summary>
        void StartAfterDataLoaded()
        {
            Debug.Log("Data Loaded");

            LoadTestWorld();
            //LoadTestTexture();
            //PrintTestDatabase();
            //LoadAnM3ForDebug();
            //TestSkyFile();
            //CreateNewWorld();
        }

        void LoadTestWorld()
        {
            var testWorld = new Engine.World.World(this.engine, 0, true);
            //testWorld.LoadMap(6, new Vector2(64, 64));   // Map\Eastern, Middle of the map
            testWorld.TeleportToWorldLocation(114, -1); // !tele 3832.459 -1001.444 -4496.945 51
            //testWorld.Teleport(100, -1000, 100, 51);
            //testWorld.Teleport(256, -900, 256, 3538, 1);
        }

        void CreateNewWorld()
        {
            var newWorld = new Engine.World.World(this.engine, 0, true);
            newWorld.CreateNew("ZeeTest");
        }

        void LoadAnM3ForDebug()
        {
            var renderer = Program.editor.CreateRendererPane(Program.mainWindow, "Model", 1, 1);
            //string path0 = @"Art\Character\Chua\Male\Chua_M.m3";
            //string path0 = @"Art\Creature\AgressorBot\AgressorBot.m3";
            //string path0 = @"Art\Prop\Constructed\Quest\Taxi\TaxiKiosk\PRP_Taxi_Kiosk_000.m3";
            //string path0 = @"Art\Prop\Constructed\Ship\Defiance\PRP_Ship_DefianceTransport_000.m3";
            //string path1 = @"Art\Prop\Natural\Tree\Deciduous_RootyMangrove\PRP_Tree_Deciduous_RootyMangrove_Blue_000.m3";
            //string path1 = @"Art\Creature\Asura\Asura.m3";

            string path0 = @"Art\Sky\SkyBox\DemoSkybox.m3";

            var modelRenderer = renderer as Engine.Rendering.ModelRenderer;
            modelRenderer.objects.Add(new Engine.Objects.M3Model(path0, new Vector3(0, 0, 0), this.engine));
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