using OpenTK.Wpf;
using ProjectWS.Engine.Rendering;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace ProjectWS.Editor
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
