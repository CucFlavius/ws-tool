using OpenTK.Mathematics;
using ProjectWS.Engine.Data.Extensions;
using ProjectWS.Engine.Rendering;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using static ProjectWS.Engine.Data.Area;
using static ProjectWS.Engine.World.Prop;

namespace ProjectWS.Editor.Tools
{
    public class PropTool : Tool
    {
        readonly Engine.Engine engine;
        public readonly WorldRenderer worldRenderer;
        readonly Editor editor;

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
        }

        public override void Disable()
        {
            this.isEnabled = false;
        }

        public override void Update(float deltaTime)
        {
            if (this.worldRenderer == null) return;
            if (this.worldRenderer.world == null) return;
            if (this.worldRenderer.mousePick == null) return;

            if (this.isEnabled && this.editor.keyboardFocused)
            {
                var propHit = this.worldRenderer.mousePick.propHit;
                var propInstanceHit = this.worldRenderer.mousePick.propInstanceHit;

                // TODO : Do stuff on mouse click etc
                Debug.DrawLabel(propHit.data.fileName, propInstanceHit.position, Vector4.One, true);
            }
        }
    }
}
