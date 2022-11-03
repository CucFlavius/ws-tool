using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace ProjectWS.Engine
{
    public class Input
    {
        Engine engine;
        public Vector3 mousePos;
        Vector3 mouseLastPos;
        Vector3 mouseDiff;

        public Dictionary<Keys, bool> lastKeyStates;
        public Dictionary<Keys, bool> keyStates;
        public bool hasFocus = true;
        public bool LMB;
        public bool RMB;
        public bool MMB;
        bool LMBPrevious;
        bool RMBPrevious;
        bool MMBPrevious;
        public int LMBClicked = -1;
        public int RMBClicked = -1;
        public int MMBClicked = -1;
        int mouseOverRenderer = -1;

        public Input(Engine engine)
        {
            this.keyStates = new Dictionary<Keys, bool>();
            this.lastKeyStates = new Dictionary<Keys, bool>();
            this.mousePos = new Vector3();
            this.mouseLastPos = new Vector3();
            this.mouseDiff = new Vector3();
            this.engine = engine;
        }

        public void Update()
        {
            mouseOverRenderer = -1;
            for (int i = 0; i < this.engine.renderers.Count; i++)
            {
                if (this.engine.renderers[i].mouseOver)
                    mouseOverRenderer = this.engine.renderers[i].ID;
            }

            // Get Mouse State
            this.mouseDiff.X = this.mousePos.X - this.mouseLastPos.X;
            this.mouseDiff.Y = this.mouseLastPos.Y - this.mousePos.Y; // reversed since y-coordinates go from bottom to top
            this.mouseDiff.Z = this.mousePos.Z - this.mouseLastPos.Z;

            this.mouseLastPos.X = this.mousePos.X;
            this.mouseLastPos.Y = this.mousePos.Y;
            this.mouseLastPos.Z = this.mousePos.Z;

            // Mouse button state changes
            if (LMB != this.LMBPrevious)
            {
                if (LMB)
                    if (this.hasFocus)
                        this.LMBClicked = mouseOverRenderer;
                    else
                        this.LMBClicked = -1;

                this.LMBPrevious = LMB;
            }

            if (RMB != this.RMBPrevious)
            {
                if (RMB)
                    if (this.hasFocus)
                        this.RMBClicked = mouseOverRenderer;
                    else
                        this.RMBClicked = -1;

                this.RMBPrevious = RMB;
            }

            if (MMB != this.MMBPrevious)
            {
                if (MMB)
                    if (this.hasFocus)
                        this.MMBClicked = mouseOverRenderer;
                    else
                        this.MMBClicked = -1;

                this.MMBPrevious = MMB;
            }
        }

        public bool GetKeyDown(Keys key)
        {
            return this.keyStates[key];
        }

        public bool GetKeyPress(Keys key)
        {
            if (this.lastKeyStates.ContainsKey(key))
            {
                if (this.keyStates[key] != this.lastKeyStates[key])
                {
                    this.lastKeyStates[key] = this.keyStates[key];
                    return this.keyStates[key];
                }
                else
                {
                    return false;
                }
            }
            else
            {
                this.lastKeyStates[key] = this.keyStates[key];
                return this.keyStates[key];
            }
        }

        public Vector3 GetMousePosition()
        {
            return this.mousePos;
        }

        public Vector3 GetMouseDiff()
        {
            return this.mouseDiff;
        }
    }
}
