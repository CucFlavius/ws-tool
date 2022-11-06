using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProjectWS.Engine
{
    public class Settings
    {
        public WorldRenderer wRenderer = new WorldRenderer();

        [JsonSerializable(typeof(WorldRenderer))]
        public class WorldRenderer
        {
            public Toggles toggles = new Toggles();

            public class Toggles
            {
                public bool fog = true;
            }
        }
    }
}
