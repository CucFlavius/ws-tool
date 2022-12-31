using OpenTK.Wpf;
using ProjectWS.Engine.Project;
using ProjectWS.Engine.Rendering;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
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

        public ObservableCollection<string>? mapNames { get; set; }
        public List<uint>? mapIDs { get; set; }

        public MapRendererPane(Editor editor, MapRenderer mapRenderer)
        {
            InitializeComponent();
            this.editor = editor;
            this.mapRenderer = mapRenderer;

            this.mapNames = new ObservableCollection<string>();
            this.mapIDs = new List<uint>();

            foreach (Project.Map map in ProjectManager.project!.Maps!)
            {
                this.mapNames.Add($"{map.worldRecord.ID}. {map.Name}");
                this.mapIDs.Add(map.worldRecord.ID);
            }

            this.mapComboBox.ItemsSource = this.mapNames;
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
            editor.OpenCreateMapWindow();
        }

        private void button_RemoveMap_Click(object sender, RoutedEventArgs e)
        {
            editor.RemoveMap();
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

        private void button_ImportMap_Click(object sender, RoutedEventArgs e)
        {
            this.editor.OpenImportMapWindow();
        }

        private void button_EditMap_Click(object sender, RoutedEventArgs e)
        {
            this.editor.EditMap();
        }
    }
}
