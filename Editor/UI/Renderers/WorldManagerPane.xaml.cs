﻿using OpenTK.Wpf;
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
    public partial class WorldManagerPane : UserControl
    {
        public Editor editor;
        public MapRenderer mapRenderer;

        public ObservableCollection<string>? mapNames { get; set; }
        public List<uint>? mapIDs { get; set; }
        public ObservableCollection<string>? locationNames { get; set; }
        public List<uint>? locationIDs { get; set; }

        public WorldManagerPane(Editor editor, MapRenderer mapRenderer)
        {
            InitializeComponent();
            this.editor = editor;
            this.mapRenderer = mapRenderer;

            this.mapNames = new ObservableCollection<string>();
            this.mapIDs = new List<uint>();
            this.locationNames = new ObservableCollection<string>();
            this.locationIDs = new List<uint>();

            foreach (ProjectWS.Engine.Project.Project.Map map in ProjectManager.project!.Maps!)
            {
                this.mapNames.Add($"{map.worldRecord.ID}. {map.Name}");
                this.mapIDs.Add(map.worldRecord.ID);
            }

            this.mapComboBox.ItemsSource = this.mapNames;

            if (ProjectManager.project?.previousOpenMapID != 0)
            {
                this.mapComboBox.SelectedIndex = this.mapIDs.IndexOf(ProjectManager.project.previousOpenMapID);
            }
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
            Debug.Log("Changed Map");

            if (ProjectManager.project != null && this.mapComboBox != null && this.mapIDs != null)
            {
                if (ProjectManager.project.previousOpenMapID != this.mapIDs[this.mapComboBox.SelectedIndex])
                {
                    ProjectManager.project.previousOpenMapID = (uint)this.mapIDs[this.mapComboBox.SelectedIndex];
                    ProjectManager.SaveProject();
                }
            }
            /*
            if (this.locationNames == null)
                this.locationNames = new ObservableCollection<string>();

            if (this.locationIDs == null)
                this.locationIDs = new List<uint>();

            this.locationNames.Clear();
            this.locationIDs.Clear();

            int idx = this.mapComboBox.SelectedIndex;
            if (idx == -1) return;

            if (this.mapIDs == null) return;

            var ID = this.mapIDs[idx];

            if (ProjectManager.project?.Maps != null)
            {
                Project.Map? selectedMap = null;
                foreach (var map in ProjectManager.project.Maps)
                {
                    if (map.worldRecord?.ID == ID)
                    {
                        selectedMap = map;
                        break;
                    }
                }

                if (selectedMap?.worldLocations != null)
                {
                    foreach (Project.Map.WorldLocation location in selectedMap.worldLocations)
                    {
                        this.locationNames.Add($"{location.ID}");
                        this.locationIDs.Add(location.ID);
                    }
                }
            }

            this.comboBox_location.ItemsSource = this.locationNames;
            */
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

        private void comboBox_location_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}