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
            keyStates = new Dictionary<Keys, bool>();
            lastKeyStates = new Dictionary<Keys, bool>();
            //this.mousePos = new Vector3();
            mousePosPerControl = new Dictionary<int, Vector3>();
            mouseLastPos = new Vector3();
            mouseDiff = new Vector3();
            this.engine = engine;
        }

        public void Update()
        {
            var mousePos = mousePosPerControl[engine.focusedRendererID];

            // Get Mouse State
            mouseDiff.X = mousePos.X - mouseLastPos.X;
            mouseDiff.Y = mouseLastPos.Y - mousePos.Y; // reversed since y-coordinates go from bottom to top
            mouseDiff.Z = mousePos.Z - mouseLastPos.Z;

            mouseLastPos.X = mousePos.X;
            mouseLastPos.Y = mousePos.Y;
            mouseLastPos.Z = mousePos.Z;

            // Mouse button state changes
            if (LMB != LMBPrevious)
            {
                if (LMB)
                    LMBClicked = -1;

                LMBPrevious = LMB;
            }

            if (RMB != RMBPrevious)
            {
                if (RMB)
                    RMBClicked = -1;

                RMBPrevious = RMB;
            }

            if (MMB != MMBPrevious)
            {
                if (MMB)
                    MMBClicked = -1;

                MMBPrevious = MMB;
            }
        }

        public bool GetKeyDown(Keys key)
        {
            return keyStates[key];
        }

        public bool GetKeyPress(Keys key)
        {
            if (lastKeyStates.ContainsKey(key))
            {
                if (keyStates[key] != lastKeyStates[key])
                {
                    lastKeyStates[key] = keyStates[key];
                    return keyStates[key];
                }
                else
                {
                    return false;
                }
            }
            else
            {
                lastKeyStates[key] = keyStates[key];
                return keyStates[key];
            }
        }

        public Vector3 GetMousePosition()
        {
            return mousePosPerControl[engine.focusedRendererID];
        }

        public Vector3 GetMouseDiff()
        {
            return mouseDiff;
        }
    }
}
