using OpenTK.Mathematics;

namespace ProjectWS.Engine.Rendering
{
    public class FogParameters
    {
        public Vector3 color;
        public float linearStart;
        public float linearEnd;
        public float density;

        public int equation;
        public bool isEnabled;

        public FogParameters(Vector3 color, float linearStart, float linearEnd, float density, int equation)
        {
            this.color = color;
            this.linearStart = linearStart;
            this.linearEnd = linearEnd;
            this.density = density;
            this.equation = equation;
            this.isEnabled = true;
        }

        public FogParameters(Color4 color, float linearStart, float linearEnd, float density, int equation)
        {
            this.color = new Vector3(color.R, color.G, color.B);
            this.linearStart = linearStart;
            this.linearEnd = linearEnd;
            this.density = density;
            this.equation = equation;
            this.isEnabled = true;
        }

        public void Toggle(bool on)
        {
            this.isEnabled = on;
        }
    }
}