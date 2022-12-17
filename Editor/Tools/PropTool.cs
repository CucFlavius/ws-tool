using OpenTK.Mathematics;
using ProjectWS.Engine.Rendering;

namespace ProjectWS.Editor.Tools
{
    public class PropTool : Tool
    {
        readonly Engine.Engine? engine;
        public readonly WorldRenderer? worldRenderer;
        readonly Editor? editor;

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

                // TODO : Do stuff on mouse click etc
                if (propHit != null)
                    Debug.DrawLabel3D(propHit.data.fileName, propInstanceHit.position, Vector4.One, true);
            }
        }
    }
}
