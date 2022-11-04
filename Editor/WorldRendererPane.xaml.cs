using OpenTK.Wpf;
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
    public partial class WorldRendererPane : UserControl
    {
        public Action<int>? changeViewMode;
        public ObservableCollection<string>? viewModes { get; set; }

        public WorldRendererPane()
        {
            InitializeComponent();


            this.viewModeComboBox.DataContext = this;
            this.viewModes = new ObservableCollection<string>();

            foreach (Renderer.ViewMode shading in (Renderer.ViewMode[])Enum.GetValues(typeof(Renderer.ViewMode)))
            {
                this.viewModes.Add(shading.ToString());
            }

            this.viewModeComboBox.SelectedIndex = 0;
        }

        public GLWpfControl GetOpenTKControl()
        {
            return this.GLWpfControl;
        }

        public Grid GetRendererGrid()
        {
            return this.RendererGrid;
        }

        private void viewModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.viewModeComboBox.Focusable = false;

            if (changeViewMode != null)
                changeViewMode.Invoke(this.viewModeComboBox.SelectedIndex);
        }

        private void viewModeComboBox_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
