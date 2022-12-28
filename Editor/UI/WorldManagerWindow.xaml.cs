using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace ProjectWS.Editor.UI
{
    /// <summary>
    /// Interaction logic for WorldManagerWindow.xaml
    /// </summary>
    public partial class WorldManagerWindow : Window
    {
        Editor editor;

        public WorldManagerWindow(Editor editor)
        {
            this.editor = editor;

            InitializeComponent();
        }
    }
}
