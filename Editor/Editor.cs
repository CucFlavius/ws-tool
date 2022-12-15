using AvalonDock.Layout;
using OpenTK.Mathematics;
using OpenTK.Wpf;
using ProjectWS.Editor.Tools;
using ProjectWS.Engine.Rendering;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.IO;
using System.Security.Principal;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProjectWS.Editor
{
    public class Editor
    {
        DateTime time1 = DateTime.Now;
        DateTime time2 = DateTime.Now;
        readonly float timeScale = 1.0f;
        float deltaTime;
        float elapsedTime;
        public float mouseWheelPos;
        public bool keyboardFocused;

        public Engine.Engine? engine;
        private TestArea.Tests? tests;
        public GLWpfControl? focusedControl;
        public Dictionary<int, GLWpfControl>? controls;
        public Dictionary<int, WorldRendererPane>? worldRendererPanes;
        public Dictionary<int, ModelRendererPane>? modelRendererPanes;

        public SkyEditorPane? skyEditorPane;
        public UI.Toolbox.ToolboxPane? toolboxPane;
        public List<Tool>? tools;

        public LayoutAnchorablePane layoutAnchorablePane;
        public LayoutAnchorable toolboxLayoutAnchorable;

        FPSCounter? fps;
        NamedPipeClientStream? pipeClient;

        Dictionary<OpenTK.Windowing.GraphicsLibraryFramework.Keys, Engine.Input.User32Wrapper.Key> opentkKeyMap = new Dictionary<OpenTK.Windowing.GraphicsLibraryFramework.Keys, Engine.Input.User32Wrapper.Key>()
        {
            { OpenTK.Windowing.GraphicsLibraryFramework.Keys.W, Engine.Input.User32Wrapper.Key.W },
            { OpenTK.Windowing.GraphicsLibraryFramework.Keys.S, Engine.Input.User32Wrapper.Key.S },
            { OpenTK.Windowing.GraphicsLibraryFramework.Keys.A, Engine.Input.User32Wrapper.Key.A },
            { OpenTK.Windowing.GraphicsLibraryFramework.Keys.D, Engine.Input.User32Wrapper.Key.D },
            { OpenTK.Windowing.GraphicsLibraryFramework.Keys.C, Engine.Input.User32Wrapper.Key.C },
            { OpenTK.Windowing.GraphicsLibraryFramework.Keys.Space, Engine.Input.User32Wrapper.Key.Space },
            { OpenTK.Windowing.GraphicsLibraryFramework.Keys.R, Engine.Input.User32Wrapper.Key.R },
        };

        public Editor()
        {
            this.controls = new Dictionary<int, GLWpfControl>();
            this.worldRendererPanes = new Dictionary<int, WorldRendererPane>();
            this.modelRendererPanes = new Dictionary<int, ModelRendererPane>();
            this.tools = new List<Tool>();
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

                for (int i = 0; i < this.tools.Count; i++)
                {
                    if (this.tools[i].isEnabled)
                        this.tools[i].Update(this.deltaTime);
                }

                if (Program.app != null && this.fps != null)
                    Program.app.MainWindow.Title = $"FPS:{this.fps.Get()} DrawCalls:{WorldRenderer.drawCalls} PropDrawCalls:{WorldRenderer.propDrawCalls}";
            }
        }

        public void Render(int renderer, int frameBuffer)
        {
            if (this.engine != null)
                this.engine.Render(renderer, frameBuffer);

            if (this.fps != null)
                this.fps.Update(this.deltaTime);
        }

        void CalculateDeltaTime()
        {
            this.time2 = DateTime.Now;
            this.deltaTime = (this.time2.Ticks - this.time1.Ticks) / 10000000f;
            this.time1 = this.time2;
        }

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

            this.keyboardFocused = Application.Current.MainWindow.IsKeyboardFocusWithin;

            if (this.keyboardFocused)
            {
                foreach (var item in opentkKeyMap)
                {
                    //this.engine.input.keyStates[item.Key] = Keyboard.IsKeyDown(item.Value);
                    this.engine.input.keyStates[item.Key] = Engine.Input.User32Wrapper.GetAsyncKeyState(item.Value) != 0;
                }
            }

            //var mousePosition = Mouse.GetPosition(this.focusedControl);

            ProjectWS.Engine.Input.User32Wrapper.GetCursorPos(out var lpPoint);
            var mousePosition = this.focusedControl.PointFromScreen(new Point(lpPoint.X, lpPoint.Y));

            this.engine.input.mousePosPerControl[this.engine.focusedRendererID] = new Vector3((float)mousePosition.X, (float)mousePosition.Y, this.mouseWheelPos);
            this.mouseWheelPos = 0;
            for (int r = 0; r < this.engine.renderers.Count; r++)
            {
                var renderer = this.engine.renderers[r];
                renderer.RecalculateViewports();
                if (renderer.ID == engine.focusedRendererID)
                {
                    if(this.worldRendererPanes.TryGetValue(renderer.ID, out WorldRendererPane? wPane))
                    {
                        if (renderer.viewports != null)
                        {
                            if ((Mouse.LeftButton == MouseButtonState.Pressed && !this.engine.input.LMB) || (Mouse.RightButton == MouseButtonState.Pressed && !this.engine.input.RMB))
                            {
                                if (renderer.viewports.Count == 1)
                                {
                                    renderer.viewports[0].interactive = true;
                                }
                                else
                                {
                                    for (int v = 0; v < renderer.viewports.Count; v++)
                                    {
                                        var vp = renderer.viewports[v];
                                        if (mousePosition.X < vp.x + vp.width && mousePosition.X > vp.x &&
                                            mousePosition.Y < vp.y + vp.height && mousePosition.Y > vp.y)
                                        {
                                            vp.interactive = true;
                                        }
                                        else
                                        {
                                            vp.interactive = false;
                                        }
                                    }
                                }
                            }

                            if (renderer.viewports.Count > 1)
                            {
                                wPane.ViewportRect0.Visibility = Visibility.Visible;

                                for (int v = 0; v < renderer.viewports.Count; v++)
                                {
                                    if (renderer.viewports[v].interactive)
                                    {
                                        wPane.ViewportRect0.Margin = new Thickness(renderer.viewports[v].x, renderer.viewports[v].y, 0, 0);
                                        wPane.ViewportRect0.Width = renderer.viewports[v].width;
                                        if (renderer.viewports[v].height > 0)
                                            wPane.ViewportRect0.Height = renderer.viewports[v].height - 2;
                                    }
                                }
                            }
                            else
                            {
                                wPane.ViewportRect0.Visibility = Visibility.Hidden;
                            }
                        }
                    }

                    if (this.modelRendererPanes.TryGetValue(renderer.ID, out ModelRendererPane? mPane))
                    {
                        if (renderer.viewports != null)
                        {
                            if ((Mouse.LeftButton == MouseButtonState.Pressed && !this.engine.input.LMB) || (Mouse.RightButton == MouseButtonState.Pressed && !this.engine.input.RMB))
                            {
                                if (renderer.viewports.Count == 1)
                                {
                                    renderer.viewports[0].interactive = true;
                                }
                                else
                                {
                                    for (int v = 0; v < renderer.viewports.Count; v++)
                                    {
                                        var vp = renderer.viewports[v];
                                        if (mousePosition.X < vp.x + vp.width && mousePosition.X > vp.x &&
                                            mousePosition.Y < vp.y + vp.height && mousePosition.Y > vp.y)
                                        {
                                            vp.interactive = true;
                                        }
                                        else
                                        {
                                            vp.interactive = false;
                                        }
                                    }
                                }
                            }

                            if (renderer.viewports.Count > 1)
                            {
                                mPane.ViewportRect0.Visibility = Visibility.Visible;

                                for (int v = 0; v < renderer.viewports.Count; v++)
                                {
                                    if (renderer.viewports[v].interactive)
                                    {
                                        mPane.ViewportRect0.Margin = new Thickness(renderer.viewports[v].x, renderer.viewports[v].y, 0, 0);
                                        mPane.ViewportRect0.Width = renderer.viewports[v].width;
                                        if (renderer.viewports[v].height > 0)
                                            mPane.ViewportRect0.Height = renderer.viewports[v].height - 2;
                                    }
                                }
                            }
                            else
                            {
                                mPane.ViewportRect0.Visibility = Visibility.Hidden;
                            }
                        }
                    }
                }
            }

            this.engine.input.LMB = Engine.Input.User32Wrapper.GetAsyncKeyState(Engine.Input.User32Wrapper.Key.LeftMouseBtn) != 0;
            this.engine.input.RMB = Engine.Input.User32Wrapper.GetAsyncKeyState(Engine.Input.User32Wrapper.Key.RightMouseBtn) != 0;
            this.engine.input.MMB = Engine.Input.User32Wrapper.GetAsyncKeyState(Engine.Input.User32Wrapper.Key.MidMouseBtn) != 0;
            //this.engine.input.LMB = Mouse.LeftButton == MouseButtonState.Pressed;
            //this.engine.input.RMB = Mouse.RightButton == MouseButtonState.Pressed;
            //this.engine.input.MMB = Mouse.MiddleButton == MouseButtonState.Pressed;

            // Allow mouse drag beyond the window borders
            if (this.engine.input.RMB)
                Application.Current.MainWindow.CaptureMouse();
            else
                Application.Current.MainWindow.ReleaseMouseCapture();
        }

        public void MouseWheelEventHandler(object sender, MouseWheelEventArgs e)
        {
            this.mouseWheelPos += e.Delta / 120f;
        }

        public Renderer? CreateRendererPane(MainWindow window, string name, int ID, int type)
        {
            if (this.engine == null) return null;

            Debug.Log("Create Renderer Pane, type " + type);

            Renderer renderer;
            GLWpfControl openTkControl;
            Grid rendererGrid;

            if (type == 0)
            {
                var rendererPane = new WorldRendererPane(this);
                openTkControl = rendererPane.GetOpenTKControl();
                rendererGrid = rendererPane.GetRendererGrid();
                this.controls?.Add(ID, openTkControl);

                var layoutDoc = new LayoutDocument();
                layoutDoc.Title = name;
                layoutDoc.ContentId = "Renderer_" + ID + "_" + name;
                layoutDoc.Content = rendererPane;
                layoutDoc.CanClose = false;

                var testRenderPane = new LayoutDocumentPane(layoutDoc);

                window.LayoutDocumentPaneGroup.Children.Add(testRenderPane);

                var settings = new GLWpfControlSettings { MajorVersion = 4, MinorVersion = 0, RenderContinuously = true };
                openTkControl.Start(settings);

                var worldRenderer = new WorldRenderer(this.engine, ID, this.engine.input);
                renderer = worldRenderer;

                // Create tools
                this.tools?.Add(new TerrainSculptTool(this.engine, this, worldRenderer));
                this.tools?.Add(new TerrainLayerPaintTool(this.engine, this, worldRenderer));
                this.tools?.Add(new PropTool(this.engine, this, worldRenderer));

                this.engine.renderers.Add(renderer);

                var gizmo = new Engine.Objects.Gizmos.BoxGizmo(Vector4.One);
                gizmo.transform.SetPosition(0.1f, 0.1f, 0.1f);
                if (renderer.gizmos != null)
                    renderer.gizmos.Add(gizmo);

                this.engine.taskManager.buildTasks.Enqueue(new Engine.TaskManager.BuildObjectTask(gizmo));

                rendererPane.changeViewMode = renderer.SetViewportMode;
                rendererPane.toggleFog = renderer.ToggleFog;
                rendererPane.toggleAreaGrid = renderer.ToggleAreaGrid;
                rendererPane.toggleChunkGrid = renderer.ToggleChunkGrid;
                rendererPane.fogToggle.IsChecked = Engine.Engine.settings.wRenderer.toggles.fog;
                rendererPane.displayAreaToggle.IsChecked = Engine.Engine.settings.wRenderer.toggles.displayAreaGrid;
                rendererPane.displayChunkToggle.IsChecked = Engine.Engine.settings.wRenderer.toggles.displayChunkGrid;

                var grid = new Engine.Objects.Gizmos.InfiniteGridGizmo(Vector4.One);
                if (renderer.gizmos != null)
                    renderer.gizmos.Add(grid);

                this.worldRendererPanes.Add(ID, rendererPane);

                this.engine.taskManager.buildTasks.Enqueue(new Engine.TaskManager.BuildObjectTask(grid));
            }
            else if (type == 1)
            {
                var rendererPane = new ModelRendererPane();
                openTkControl = rendererPane.GetOpenTKControl();
                rendererGrid = rendererPane.GetRendererGrid();
                this.controls.Add(ID, openTkControl);

                var layoutDoc = new LayoutDocument();
                layoutDoc.Title = name;
                layoutDoc.ContentId = "Renderer_" + ID + "_" + name;
                layoutDoc.Content = rendererPane;

                var testRenderPane = new LayoutDocumentPane(layoutDoc);

                window.LayoutDocumentPaneGroup.Children.Add(testRenderPane);

                var settings = new GLWpfControlSettings { MajorVersion = 4, MinorVersion = 0, RenderContinuously = true };
                openTkControl.Start(settings);

                renderer = new ModelRenderer(this.engine, ID, this.engine.input);
                //renderer.SetDimensions(0, 0, (int)openTkControl.ActualWidth, (int)openTkControl.ActualHeight);
                this.engine.renderers.Add(renderer);

                rendererPane.changeRenderMode = renderer.SetShadingOverride;

                var grid = new Engine.Objects.Gizmos.InfiniteGridGizmo(Vector4.One);
                if (renderer.gizmos != null)
                    renderer.gizmos.Add(grid);

                this.modelRendererPanes.Add(ID, rendererPane);

                this.engine.taskManager.buildTasks.Enqueue(new Engine.TaskManager.BuildObjectTask(grid));
            }
            else
            {
                Debug.Log("Unsupported renderer type : " + type);
                return null;
            }

            // Add events
            openTkControl.Render += (delta) => OpenTkControl_OnRender(delta, ID, openTkControl.Framebuffer);
            openTkControl.Loaded += (sender, e) => OpenTkControl_OnLoaded(sender, e, renderer, rendererGrid, openTkControl);
            openTkControl.SizeChanged += (sender, e) => OpenTkControl_OnSizeChanged(sender, e, renderer);
            
            return renderer;
        }
        /*
        public void CreateSkyEditorPane(MainWindow window)
        {
            this.skyEditorPane = new SkyEditorPane();
            this.skyEditorPane.engine = this.engine;
            var layoutAnchorable = new LayoutAnchorable();
            layoutAnchorable.Title = "Sky Editor";
            layoutAnchorable.ContentId = "SkyEditorPane";
            layoutAnchorable.Content = this.skyEditorPane;

            LayoutAnchorablePane layoutAnchorablePane = new LayoutAnchorablePane(layoutAnchorable);

            window.LayoutAnchorablePaneGroup.Children.Add(layoutAnchorablePane);
        }
        */
        public void CreateToolboxPane(MainWindow window)
        {
            if (this.toolboxPane == null)
            {
                this.toolboxPane = new UI.Toolbox.ToolboxPane();
                this.toolboxPane.engine = this.engine;
                this.toolboxPane.editor = this;
            }

            if (this.toolboxLayoutAnchorable == null)
            {
                this.toolboxLayoutAnchorable = new LayoutAnchorable();
                this.toolboxLayoutAnchorable.Title = "Toolbox";
                this.toolboxLayoutAnchorable.ContentId = "ToolboxPane";
                this.toolboxLayoutAnchorable.Content = this.toolboxPane;
            }

            if (this.layoutAnchorablePane == null)
            {
                this.layoutAnchorablePane = new LayoutAnchorablePane(this.toolboxLayoutAnchorable);
                window.LayoutAnchorablePaneGroup.Children.Add(this.layoutAnchorablePane);
            }
        }

        void OpenTkControl_OnRender(TimeSpan delta, int ID, int frameBuffer)
        {
            // Find which control is focused, and only update if ID matches
            // This is so that Update is only called one time, not for every render control call
            if (ID == this.engine.focusedRendererID)
            {
                Update();
            }

            

            Render(ID, frameBuffer);
        }

        void OpenTkControl_OnLoaded(object sender, RoutedEventArgs e, Renderer renderer, Grid control, GLWpfControl openTkControl)
        {
            renderer.SetDimensions(0, 0, (int)openTkControl.ActualWidth, (int)openTkControl.ActualHeight);
            renderer.Load();
        }

        void OpenTkControl_OnSizeChanged(object sender, SizeChangedEventArgs e, Renderer renderer)
        {
            var size = e.NewSize;
            renderer.Resize((int)size.Width, (int)size.Height);
        }

        internal void Save()
        {
            foreach (var wItem in this.engine.worlds)
            {
                foreach (var cItem in wItem.Value.chunks)
                {
                    cItem.Value.area.ProcessForExport();
                    cItem.Value.area.Write();
                }
            }

            // Update sandbox
            this.pipeClient = new NamedPipeClientStream(".", "Arctium.ChatCommand.Pipe", PipeDirection.InOut, PipeOptions.WriteThrough | PipeOptions.Asynchronous, TokenImpersonationLevel.Impersonation);
            this.pipeClient.Connect();

            using (var bw = new BinaryWriter(this.pipeClient))
            {
                var data = System.Text.Encoding.ASCII.GetBytes("!tele 256 -998 256 3538");
                bw.Write(data.Length);
                bw.Write(data);
            }
            //pipeClient.Close();
        }
    }
}
