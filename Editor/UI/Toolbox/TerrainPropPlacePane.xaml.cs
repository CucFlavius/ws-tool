using ProjectWS.Engine.World;
using System.Windows.Controls;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProjectWS.Editor.UI.Toolbox
{
    /// <summary>
    /// Interaction logic for TerrainToolPane.xaml
    /// </summary>
    public partial class TerrainPropPlacePane : UserControl
    {
        public TerrainPropPlacePane()
        {
            InitializeComponent();
        }

        public void OnPropSelectionChanged(World world, Prop prop, Prop.Instance propInstance)
        {
            this.textBlock_SelectedProp.Text = prop.data.fileName;

            foreach (System.Collections.Generic.KeyValuePair<MathUtils.Vector2i, Chunk> chunkItem in world.activeChunks)
            {
                if (chunkItem.Value != null && chunkItem.Value.area != null && chunkItem.Value.area.propLookup != null)
                {
                    if (chunkItem.Value.area.propLookup.TryGetValue(propInstance.uuid, out FileFormats.Area.Prop? areaProp))
                    {
                        string jsonString = JsonSerializer.Serialize(areaProp, new JsonSerializerOptions { WriteIndented = true, IncludeFields = true });
                        this.textBlock_PropDebugDetails.Text = jsonString;
                    }
                }
            }
        }
    }
}
