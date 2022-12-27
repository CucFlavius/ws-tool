using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProjectWS.Engine
{
    public class Settings
    {
        public Window? window { get; set; }
        public WorldRenderer? wRenderer { get; set; }
        public DataManager? dataManager { get; set; }

        public class WorldRenderer
        {
            public Toggles? toggles { get; set; }

            public class Toggles
            {
                public bool fog { get; set; }
                public bool displayAreaGrid { get; set; }
                public bool displayChunkGrid { get; set; }

                public Toggles()
                {
                    this.fog = true;
                    this.displayAreaGrid = false;
                    this.displayChunkGrid = false;
                }
            }

            public WorldRenderer()
            {
                this.toggles = new Toggles();
            }
        }

        public class Window
        {
            public int windowState { get; set; }
            public double left { get; set; }
            public double top { get; set; }
            public double height { get; set; }
            public double width { get; set; }

            public Window()
            {
                this.windowState = 0;
                this.left = 0;
                this.top = 0;
                this.height = 720;
                this.width = 1280;
            }
        }

        public class DataManager
        {
            public string? gameClientPath { get; set; }
            public string? assetDatabasePath { get; set; }

            public DataManager()
            {
                this.gameClientPath = String.Empty;
                this.assetDatabasePath = String.Empty;
            }
        }

        public Settings()
        {
            this.wRenderer = new WorldRenderer();
            this.window = new Window();
            this.dataManager = new DataManager();
        }
    }
}
