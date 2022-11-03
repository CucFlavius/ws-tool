using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AvalonDock.Layout;
using OpenTK.Compute.OpenCL;
using OpenTK.Mathematics;
using OpenTK.Wpf;
using ProjectWS.Engine;
using ProjectWS.Engine.Data;
using ProjectWS.Engine.Database.Definitions;
using ProjectWS.Engine.Rendering;

namespace ProjectWS.Editor
{
    public class Editor
    {
        DateTime time1 = DateTime.Now;
        DateTime time2 = DateTime.Now;
        readonly float timeScale = 1.0f;
        float deltaTime;
        float elapsedTime;

        public Engine.Engine? engine;
        private TestArea.Tests? tests;
        public GLWpfControl? focusedControl;
        FPSCounter fps;

        public Editor()
        {
            Start();
        }

        void Start()
        {
            this.engine = new Engine.Engine();
            InputInitialize();

            // Create tests
            this.tests = new TestArea.Tests(this.engine, this);
            this.tests.Start();
            this.fps = new FPSCounter();
        }

        public void Update()
        {   
            CalculateDeltaTime();
            elapsedTime += deltaTime;
            if (this.engine != null)
            {
                InputUpdate();
                this.engine.Update(this.deltaTime, this.timeScale);

                if (Program.app != null)
                    Program.app.MainWindow.Title = this.fps.Get().ToString();
            }
        }

        public void Render(int renderer)
        {
            if (this.engine != null)
            {
                this.engine.Render(renderer);
                this.fps.Update(this.deltaTime);
            }
        }

        void CalculateDeltaTime()
        {
            this.time2 = DateTime.Now;
            this.deltaTime = (this.time2.Ticks - this.time1.Ticks) / 10000000f;
            this.time1 = this.time2;
        }

        Dictionary<OpenTK.Windowing.GraphicsLibraryFramework.Keys, Key> opentkKeyMap = new Dictionary<OpenTK.Windowing.GraphicsLibraryFramework.Keys, Key>()
        { 
            { OpenTK.Windowing.GraphicsLibraryFramework.Keys.W, Key.W },
            { OpenTK.Windowing.GraphicsLibraryFramework.Keys.S, Key.S },
            { OpenTK.Windowing.GraphicsLibraryFramework.Keys.A, Key.A },
            { OpenTK.Windowing.GraphicsLibraryFramework.Keys.D, Key.D },
            { OpenTK.Windowing.GraphicsLibraryFramework.Keys.C, Key.C },
            { OpenTK.Windowing.GraphicsLibraryFramework.Keys.Space, Key.Space },
            { OpenTK.Windowing.GraphicsLibraryFramework.Keys.R, Key.R },
        };

        void InputInitialize()
        {
            if (this.engine == null) return;

            foreach (var item in opentkKeyMap)
            {
                this.engine.input.keyStates[item.Key] = false;
            }
        }

        void InputUpdate()
        {
            if (this.engine == null) return;

            if(Application.Current.MainWindow.IsKeyboardFocused)
            {
                foreach (var item in opentkKeyMap)
                {
                    //this.engine.input.keyStates[item.Key] = Keyboard.IsKeyDown(item.Value);
                }
            }

            this.engine.input.LMB = Mouse.LeftButton == MouseButtonState.Pressed;
            this.engine.input.RMB = Mouse.RightButton == MouseButtonState.Pressed;
            this.engine.input.MMB = Mouse.MiddleButton == MouseButtonState.Pressed;

            // Allow mouse drag beyond the window borders
            if (this.engine.input.RMB)
                Application.Current.MainWindow.CaptureMouse();
            else
                Application.Current.MainWindow.ReleaseMouseCapture();

            var mousePosition = Mouse.GetPosition(this.focusedControl);
            this.engine.input.mousePos = new Vector3((float)mousePosition.X, (float)mousePosition.Y, 0);
        }

        public void CreateRendererPane(MainWindow window, string name, int ID, int type)
        {
            if (this.engine == null) return;

            Debug.Log("Create Renderer Pane, type " + type);

            var openTkControl = new GLWpfControl();
            openTkControl.Name = "OpenTKControl_" + ID;

            var rendererGrid = new Grid();
            rendererGrid.Children.Add(openTkControl);

            var rendererDocument = new LayoutDocument();
            rendererDocument.Title = name;
            rendererDocument.ContentId = "Renderer_" + ID + "_" + name;
            rendererDocument.Content = rendererGrid;

            var rendererPane = new LayoutDocumentPane(rendererDocument);
            window.LayoutDocumentPaneGroup.Children.Add(rendererPane);
            //rendererDocument.Dock();

            var settings = new GLWpfControlSettings { MajorVersion = 4, MinorVersion = 0 };
            openTkControl.Start(settings);

            Renderer renderer = null;
            if (type == 0)
            {
                renderer = new WorldRenderer(this.engine, ID, this.engine.input);
                this.engine.renderers.Add(renderer);
            }
            else if (type == 1)
            {
                renderer = new ModelRenderer(this.engine, ID, this.engine.input);
                this.engine.renderers.Add(renderer);
            }
            else
            {
                Debug.Log("Unsupported renderer type : " + type);
                return;
            }

            // Add events
            openTkControl.Render += (delta) => OpenTkControl_OnRender(delta, ID);
            openTkControl.Loaded += (sender, e) => OpenTkControl_OnLoaded(sender, e, renderer, rendererGrid);
            openTkControl.SizeChanged += (sender, e) => OpenTkControl_OnSizeChanged(sender, e, renderer);
        }

        void OpenTkControl_OnRender(TimeSpan delta, int ID)
        {
            if (ID == 0)
                Update();

            Render(ID);
        }

        void OpenTkControl_OnLoaded(object sender, RoutedEventArgs e, Renderer renderer, Grid control)
        {
            renderer.SetViewport(0, 0, (int)control.ActualWidth, (int)control.ActualHeight);
            renderer.modelShader = new Shader("shader_vert.glsl", "shader_frag.glsl");
            renderer.shader = renderer.modelShader;
            renderer.wireframeShader = new Shader("wireframe_vert.glsl", "wireframe_frag.glsl");
            renderer.normalShader = new Shader("normal_vert.glsl", "normal_frag.glsl");
            renderer.terrainShader = new Shader("terrain_vert.glsl", "terrain_frag.glsl");
            renderer.lineShader = new Shader("line_vert.glsl", "line_frag.glsl");
        }

        void OpenTkControl_OnSizeChanged(object sender, SizeChangedEventArgs e, Renderer renderer)
        {
            var size = e.NewSize;
            renderer.Resize((int)size.Width, (int)size.Height);
        }
    }
}
