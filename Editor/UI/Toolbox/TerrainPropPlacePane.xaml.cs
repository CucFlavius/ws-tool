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

            string jsonString = JsonSerializer.Serialize(propInstance.areaprop, new JsonSerializerOptions { WriteIndented = true, IncludeFields = true });
            this.textBlock_PropDebugDetails.Text = jsonString;
        }
    }
}
