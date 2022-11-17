﻿using OpenTK.Wpf;
using ProjectWS.Editor.Tools;
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
    public partial class WorldRendererPane : UserControl
    {
        Editor editor;

        public Action<int>? changeViewMode;
        public Action<bool>? toggleFog;
        public Action<bool>? toggleChunkGrid;
        public Action<bool>? toggleAreaGrid;

        public ObservableCollection<string>? viewModes { get; set; }

        public WorldRendererPane(Editor editor)
        {
            this.editor = editor;

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

            if (this.changeViewMode != null)
                this.changeViewMode.Invoke(this.viewModeComboBox.SelectedIndex);
        }

        private void viewModeComboBox_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void ToggleFogOn(object sender, RoutedEventArgs e)
        {
            if (this.toggleFog != null)
                this.toggleFog.Invoke(true);
        }

        private void ToggleFogOff(object sender, RoutedEventArgs e)
        {
            if (this.toggleFog != null)
                this.toggleFog.Invoke(false);
        }

        private void ToggleAreaGridOn(object sender, RoutedEventArgs e)
        {
            if (this.toggleAreaGrid != null)
                this.toggleAreaGrid.Invoke(true);
        }

        private void ToggleAreaGridOff(object sender, RoutedEventArgs e)
        {
            if (this.toggleAreaGrid != null)
                this.toggleAreaGrid.Invoke(false);
        }

        private void ToggleChunkGridOn(object sender, RoutedEventArgs e)
        {
            if (this.toggleChunkGrid != null)
                this.toggleChunkGrid.Invoke(true);
        }

        private void ToggleChunkGridOff(object sender, RoutedEventArgs e)
        {
            if (this.toggleChunkGrid != null)
                this.toggleChunkGrid.Invoke(false);
        }

        private void sculptTerrainButton_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < this.editor.tools.Count; i++)
            {
                if (this.editor.tools[i] is TerrainSculptTool)
                {
                    this.editor.tools[i].Enable();
                    (this.editor.tools[i] as TerrainSculptTool).mode = Tools.TerrainSculptTool.Mode.RaiseLower;
                }
                else
                {
                    this.editor.tools[i].Disable();
                }
            }
        }

        private void flattenTerrainButton_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < this.editor.tools.Count; i++)
            {
                if (this.editor.tools[i] is TerrainSculptTool)
                {
                    this.editor.tools[i].Enable();
                    (this.editor.tools[i] as TerrainSculptTool).mode = Tools.TerrainSculptTool.Mode.Flatten;
                }
                else
                {
                    this.editor.tools[i].Disable();
                }
            }
        }

        private void paint0TerrainButton_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < this.editor.tools.Count; i++)
            {
                if (this.editor.tools[i] is TerrainLayerPaintTool)
                {
                    this.editor.tools[i].Enable();
                    (this.editor.tools[i] as TerrainLayerPaintTool).layer = 0;
                }
                else
                {
                    this.editor.tools[i].Disable();
                }
            }
        }

        private void paint1TerrainButton_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < this.editor.tools.Count; i++)
            {
                if (this.editor.tools[i] is TerrainLayerPaintTool)
                {
                    this.editor.tools[i].Enable();
                    (this.editor.tools[i] as TerrainLayerPaintTool).layer = 1;
                }
                else
                {
                    this.editor.tools[i].Disable();
                }
            }
        }

        private void paint2TerrainButton_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < this.editor.tools.Count; i++)
            {
                if (this.editor.tools[i] is TerrainLayerPaintTool)
                {
                    this.editor.tools[i].Enable();
                    (this.editor.tools[i] as TerrainLayerPaintTool).layer = 2;
                }
                else
                {
                    this.editor.tools[i].Disable();
                }
            }
        }

        private void paint3TerrainButton_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < this.editor.tools.Count; i++)
            {
                if (this.editor.tools[i] is TerrainLayerPaintTool)
                {
                    this.editor.tools[i].Enable();
                    (this.editor.tools[i] as TerrainLayerPaintTool).layer = 3;
                }
                else
                {
                    this.editor.tools[i].Disable();
                }
            }
        }
    }
}
