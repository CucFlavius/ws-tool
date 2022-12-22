using MathUtils;
using ProjectWS.Engine.Rendering;
using ProjectWS.Engine.World;
using SharpFont.MultipleMasters;

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
                        this.editor.toolboxPane?.terrainPropPlacePane.OnPropSelectionChanged(this.worldRenderer.world, propHit, propInstanceHit);
                    }
                    /*
                    var labelText = $"{this.propHit.data.fileName}\n" +
                        $"UUID:{this.propInstanceHit.uuid} Instance:{instanceIndex}\n" +
                        $"P:{this.propInstanceHit.position}\nR:{this.propInstanceHit.rotationEuler}\nS:{this.propInstanceHit.scale}";
                    Debug.DrawLabel3D(labelText, this.propInstanceHit.position, Vector4.One, true);
                    */
                    DrawOBB(propInstanceHit.obb, propInstanceHit.transform, new Vector4(1, 1, 0, 1));
                }
            }
        }

        internal void DrawOBB(OBB obb, Matrix4 transform, Vector4 color)
        {
            var positionOffsetMat = Matrix4.CreateTranslation(obb.center);
            var scaleMat = Matrix4.CreateScale(obb.size);
            var boxMat = scaleMat * positionOffsetMat * transform;

            Debug.DrawWireBox3D(boxMat, color);
        }
    }
}
