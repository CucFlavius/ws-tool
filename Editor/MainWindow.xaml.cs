/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using AvalonDock.Layout;
using System.Diagnostics;
using System.IO;
using AvalonDock.Layout.Serialization;
using AvalonDock;
using System.Diagnostics.CodeAnalysis;
using OpenTK;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Wpf;
using ProjectWS.Engine.Rendering;
using ProjectWS.Engine;
using SevenZip.Compression.LZ;

namespace ProjectWS.Editor
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
        public Dispatcher dispatcher = Dispatcher.CurrentDispatcher;

        public MainWindow()
		{
			InitializeComponent();
			this.DataContext = this;
			Program.mainWindow = this;

			// Create main render/update loop renderer
            Program.editor.CreateRendererPane(Program.mainWindow, "World", 0, 0);
        }

		#region FocusedElement

		/// <summary>
		/// FocusedElement Dependency Property
		/// </summary>
		public static readonly DependencyProperty FocusedElementProperty =
			DependencyProperty.Register("FocusedElement", typeof(string), typeof(MainWindow),
				new FrameworkPropertyMetadata((IInputElement)null));

		/// <summary>
		/// Gets or sets the FocusedElement property.  This dependency property 
		/// indicates ....
		/// </summary>
		public string FocusedElement
		{
			get => (string)GetValue(FocusedElementProperty);
			set => SetValue(FocusedElementProperty, value);
		}

        #endregion

        private void OnLayoutRootPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var activeContent = ((LayoutRoot)sender).ActiveContent;
            if (e.PropertyName == "ActiveContent")
            {
				if (activeContent == null)
					return;

				if (activeContent.Content is Grid)
				{
					if (activeContent.Content == null)
						return;

					var grid = (activeContent.Content as Grid);

					if (grid == null)
						return;

                    foreach (var item in grid.Children)
					{
						if (item is GLWpfControl)
						{
							if (Program.editor != null)
							{
								Program.editor.focusedControl = item as GLWpfControl;
								Debug.Log(string.Format("Active Control -> {0}", item));
								return;
							}
                        }
					}
				}
            }
        }

        private void DockManager_DocumentClosing(object sender, DocumentClosingEventArgs e)
		{
			if (MessageBox.Show("Are you sure you want to close the document?", "AvalonDock Sample", MessageBoxButton.YesNo) == MessageBoxResult.No)
				e.Cancel = true;
		}
    }
}