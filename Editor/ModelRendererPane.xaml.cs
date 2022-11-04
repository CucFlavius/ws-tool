using OpenTK.Wpf;
using ProjectWS.Engine;
using ProjectWS.Engine.Rendering;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Editor
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ModelRendererPane : UserControl
    {
        public Action<int>? changeRenderMode;

        public ObservableCollection<string>? renderModes { get; set; }

        public ModelRendererPane()
        {
            InitializeComponent();

            this.renderModeComboBox.DataContext = this;
            this.renderModes = new ObservableCollection<string>();

            foreach (Renderer.ShadingOverride shading in (Renderer.ShadingOverride[])Enum.GetValues(typeof(Renderer.ShadingOverride)))
            {
                this.renderModes.Add(shading.ToString());
            }

            this.renderModeComboBox.SelectedIndex = 0;
        }

        public GLWpfControl GetOpenTKControl()
        {
            return this.GLWpfControl;
        }

        public Grid GetRendererGrid()
        {
            return this.RendererGrid;
        }

        private void renderModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.renderModeComboBox.Focusable = false;

            if (changeRenderMode != null)
                changeRenderMode.Invoke(this.renderModeComboBox.SelectedIndex);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void renderModeComboBox_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
