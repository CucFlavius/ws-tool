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
    public partial class MapRendererPane : UserControl
    {
        public Editor editor;
        public MapRenderer mapRenderer;

        public MapRendererPane(Editor editor, MapRenderer mapRenderer)
        {
            InitializeComponent();
            this.editor = editor;
            this.mapRenderer = mapRenderer;
        }

        public GLWpfControl GetOpenTKControl()
        {
            return this.GLWpfControl;
        }

        public Grid GetRendererGrid()
        {
            return this.RendererGrid;
        }

        private void button_AddChunk_Click(object sender, RoutedEventArgs e)
        {

        }

        private void button_RemoveChunk_Click(object sender, RoutedEventArgs e)
        {

        }

        private void button_AddMap_Click(object sender, RoutedEventArgs e)
        {

        }

        private void button_RemoveMap_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mapComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void button_DeselectAll_Click(object sender, RoutedEventArgs e)
        {
            if (this.mapRenderer != null)
                this.mapRenderer.DeselectAllCells();
        }

        private void button_CutChunk_Click(object sender, RoutedEventArgs e)
        {

        }

        private void button_CopyChunk_Click(object sender, RoutedEventArgs e)
        {

        }

        private void button_PasteChunk_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ToggleGridOn(object sender, RoutedEventArgs e)
        {
            if (this.mapRenderer != null)
                this.mapRenderer.showGrid = true;
        }

        private void ToggleGridOff(object sender, RoutedEventArgs e)
        {
            if (this.mapRenderer != null)
                this.mapRenderer.showGrid = false;
        }
    }
}
