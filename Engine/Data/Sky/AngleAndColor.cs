using OpenTK.Mathematics;
using ProjectWS.Engine.Data.Extensions;

namespace ProjectWS.Engine.Data
{
    public class AngleAndColor
    {
        public Vector4 angle;   // Euler
        public Vector4 color;

        public AngleAndColor(BinaryReader br)
        {
            this.angle = br.ReadVector4();
            this.color = br.ReadColor();
        }

        public override string ToString()
        {
            return $"{this.color} {this.angle}";
        }
    }

}
