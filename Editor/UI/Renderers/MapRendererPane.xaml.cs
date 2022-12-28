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
        public MapRendererPane()
        {
            InitializeComponent();
        }

        public GLWpfControl GetOpenTKControl()
        {
            return this.GLWpfControl;
        }

        public Grid GetRendererGrid()
        {
            return this.RendererGrid;
        }
    }
}
