using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProjectWS.Engine
{
    public class Settings
    {
        public WorldRenderer? wRenderer { get; set; }

        public class WorldRenderer
        {
            public Toggles? toggles { get; set; }

            public class Toggles
            {
                public bool fog { get; set; }

                public Toggles()
                {
                    this.fog = true;
                }
            }

            public WorldRenderer()
            {
                this.toggles = new Toggles();
            }
        }

        public Settings()
        {
            this.wRenderer = new WorldRenderer();
        }
    }
}
