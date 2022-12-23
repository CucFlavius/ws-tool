using MathUtils;
using ProjectWS.Engine.Rendering;
using ProjectWS.Engine.World;
using SharpFont.MultipleMasters;
using System.Windows.Media;
using System;
using ProjectWS.Editor.UI.Toolbox;
using System.Windows.Media.Imaging;

namespace ProjectWS.Editor.Tools
{
    public class PropTool : Tool
    {
        readonly Engine.Engine? engine;
        public readonly WorldRenderer? worldRenderer;
        readonly Editor? editor;
        Prop currentProp;
        Prop.Instance currentPropInstance;

        public PropTool(Engine.Engine engine, Editor editor, WorldRenderer world)
        {
            this.hasBrush = true;
            this.editor = editor;
            this.engine = engine;
            this.worldRenderer = world;
            this.isEnabled = false;
        }

        public override void Enable()
        {
            this.isEnabled = true;
            if (this.worldRenderer != null && this.worldRenderer.mousePick != null)
                this.worldRenderer.mousePick.mode = Engine.MousePick.Mode.Prop;
        }

        public override void Disable()
        {
            this.isEnabled = false;
            if (this.worldRenderer != null && this.worldRenderer.mousePick != null)
                this.worldRenderer.mousePick.mode = Engine.MousePick.Mode.Disabled;
        }

        public override void Update(float deltaTime)
        {
            if (this.worldRenderer == null) return;
            if (this.worldRenderer.world == null) return;
            if (this.worldRenderer.mousePick == null) return;

            if (this.isEnabled && this.editor != null && this.editor.keyboardFocused)
            {
                var propHit = this.worldRenderer.mousePick.propHit;
                var propInstanceHit = this.worldRenderer.mousePick.propInstanceHit;

                if (propHit != null && propInstanceHit != null)
                {
                    if (propHit != this.currentProp && propInstanceHit != this.currentPropInstance)
                    {
                        this.currentProp = propHit;
                        this.currentPropInstance = propInstanceHit;
                        OnPropSelectionChanged(this.worldRenderer.world, propHit, propInstanceHit);
                    }

                    if (propInstanceHit.obb != null && propInstanceHit.areaprop != null)
                        DrawOBB(propInstanceHit.obb, propInstanceHit.transform, Color32.Yellow);
                }
            }
        }

        internal void DrawOBB(OBB obb, Matrix4 transform, Color32 color)
        {
            var positionOffsetMat = Matrix4.CreateTranslation(obb.center);
            var scaleMat = Matrix4.CreateScale(obb.size);
            var boxMat = scaleMat * positionOffsetMat * transform;

            Debug.DrawWireBox3D(boxMat, color);
        }

        public void OnPropSelectionChanged(World world, Prop prop, Prop.Instance propInstance)
        {
            if (this.editor == null) return;
            if (this.editor.toolboxPane == null) return;

            TerrainPropPlacePane toolPane = this.editor.toolboxPane.terrainPropPlacePane;

            if (prop.data != null)
            {
                toolPane.textBlock_SelectedProp.Text = prop.data.fileName;
            }

            if (propInstance.areaprop != null)
            {
                toolPane.propertyGrid_prop.SelectedObject = propInstance.areaprop;
                toolPane.propertyGrid_prop.PropertyValueChanged += (obj, args) => 
                {
                    propInstance.position = propInstance.areaprop.position;
                    propInstance.rotation = propInstance.areaprop.rotation;
                    propInstance.scale = propInstance.areaprop.scale * Vector3.One;

                    Matrix4 mat = Matrix4.Identity;
                    propInstance.transform = mat.TRS(propInstance.position, propInstance.rotation, propInstance.scale);
                };
            }

            //string jsonString = JsonSerializer.Serialize(propInstance.areaprop, new JsonSerializerOptions { WriteIndented = true, IncludeFields = true });
            //this.textBlock_PropDebugDetails.Text = jsonString;
        }
    }
}
