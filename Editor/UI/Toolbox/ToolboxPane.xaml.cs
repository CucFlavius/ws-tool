using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace ProjectWS.Editor.UI.Toolbox
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ToolboxPane : UserControl
    {
        public Engine.Engine? engine;

        public ToolboxPane()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void NoToolButton_Checked(object sender, RoutedEventArgs e)
        {
            if (NoToolControl != null)
            {
                NoToolControl.Visibility = Visibility.Visible;
                TerrainSculptControl.Visibility = Visibility.Collapsed;
                TerrainLayerPaintControl.Visibility = Visibility.Collapsed;
                TerrainColorPaintControl.Visibility = Visibility.Collapsed;
                TerrainSkyPaintControl.Visibility = Visibility.Collapsed;
                TerrainPropPlaceControl.Visibility = Visibility.Collapsed;
            }
        }

        private void TerrainSculptButton_Checked(object sender, RoutedEventArgs e)
        {
            if (NoToolControl != null)
            {
                NoToolControl.Visibility = Visibility.Collapsed;
                TerrainSculptControl.Visibility = Visibility.Visible;
                TerrainLayerPaintControl.Visibility = Visibility.Collapsed;
                TerrainColorPaintControl.Visibility = Visibility.Collapsed;
                TerrainSkyPaintControl.Visibility = Visibility.Collapsed;
                TerrainPropPlaceControl.Visibility = Visibility.Collapsed;
            }
        }

        private void TerrainLayerPaintToolButton_Checked(object sender, RoutedEventArgs e)
        {
            if (NoToolControl != null)
            {
                NoToolControl.Visibility = Visibility.Collapsed;
                TerrainSculptControl.Visibility = Visibility.Collapsed;
                TerrainLayerPaintControl.Visibility = Visibility.Visible;
                TerrainColorPaintControl.Visibility = Visibility.Collapsed;
                TerrainSkyPaintControl.Visibility = Visibility.Collapsed;
                TerrainPropPlaceControl.Visibility = Visibility.Collapsed;
            }
        }

        private void TerrainColorPaintToolButton_Checked(object sender, RoutedEventArgs e)
        {
            NoToolControl.Visibility = Visibility.Collapsed;
            TerrainSculptControl.Visibility = Visibility.Collapsed;
            TerrainLayerPaintControl.Visibility = Visibility.Collapsed;
            TerrainColorPaintControl.Visibility = Visibility.Visible;
            TerrainSkyPaintControl.Visibility = Visibility.Collapsed;
            TerrainPropPlaceControl.Visibility = Visibility.Collapsed;
        }

        private void SkyPaintToolButton_Checked(object sender, RoutedEventArgs e)
        {
            NoToolControl.Visibility = Visibility.Collapsed;
            TerrainSculptControl.Visibility = Visibility.Collapsed;
            TerrainLayerPaintControl.Visibility = Visibility.Collapsed;
            TerrainColorPaintControl.Visibility = Visibility.Collapsed;
            TerrainSkyPaintControl.Visibility = Visibility.Visible;
            TerrainPropPlaceControl.Visibility = Visibility.Collapsed;
        }

        private void PropToolButton_Checked(object sender, RoutedEventArgs e)
        {
            NoToolControl.Visibility = Visibility.Collapsed;
            TerrainSculptControl.Visibility = Visibility.Collapsed;
            TerrainLayerPaintControl.Visibility = Visibility.Collapsed;
            TerrainColorPaintControl.Visibility = Visibility.Collapsed;
            TerrainSkyPaintControl.Visibility = Visibility.Collapsed;
            TerrainPropPlaceControl.Visibility = Visibility.Visible;
        }
    }
}
