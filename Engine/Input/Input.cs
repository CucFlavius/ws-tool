using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using static ProjectWS.Engine.Input.User32Wrapper;

namespace ProjectWS.Engine.Input
{
    public class Input
    {
        Engine engine;
        //public Vector3 mousePos;
        public Dictionary<int, Vector3> mousePosPerControl;
        Vector3 mouseLastPos;
        Vector3 mouseDiff;

        public Dictionary<Keys, bool> lastKeyStates;
        public Dictionary<Keys, bool> keyStates;
        public bool LMB;
        public bool RMB;
        public bool MMB;
        bool LMBPrevious;
        bool RMBPrevious;
        bool MMBPrevious;
        public int LMBClicked = -1;
        public int RMBClicked = -1;
        public int MMBClicked = -1;

        public Input(Engine engine)
        {
            this.keyStates = new Dictionary<Keys, bool>();
            this.lastKeyStates = new Dictionary<Keys, bool>();
            //this.mousePos = new Vector3();
            this.mousePosPerControl = new Dictionary<int, Vector3>();
            this.mouseLastPos = new Vector3();
            this.mouseDiff = new Vector3();
            this.engine = engine;
        }

        public void Update()
        {
            var mousePos = this.mousePosPerControl[this.engine.focusedRendererID];

            // Get Mouse State
            this.mouseDiff.X = mousePos.X - this.mouseLastPos.X;
            this.mouseDiff.Y = this.mouseLastPos.Y - mousePos.Y; // reversed since y-coordinates go from bottom to top
            this.mouseDiff.Z = mousePos.Z - this.mouseLastPos.Z;

            this.mouseLastPos.X = mousePos.X;
            this.mouseLastPos.Y = mousePos.Y;
            this.mouseLastPos.Z = mousePos.Z;

            // Mouse button state changes
            if (this.LMB != this.LMBPrevious)
            {
                if (this.LMB)
                    this.LMBClicked = -1;

                this.LMBPrevious = this.LMB;
            }

            if (this.RMB != this.RMBPrevious)
            {
                if (this.RMB)
                    this.RMBClicked = -1;

                this.RMBPrevious = this.RMB;
            }

            if (this.MMB != this.MMBPrevious)
            {
                if (this.MMB)
                    this.MMBClicked = -1;

                this.MMBPrevious = this.MMB;
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
                this.lastKeyStates[key] = keyStates[key];
                return keyStates[key];
            }
        }

        public Vector3 GetMousePosition()
        {
            return this.mousePosPerControl[this.engine.focusedRendererID];
        }

        public Vector3 GetMouseDiff()
        {
            return this.mouseDiff;
        }
    }
}
