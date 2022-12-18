using MathUtils;
using ProjectWS.FileFormats.Extensions;

namespace ProjectWS.Engine.Data
{
    public class ColorAB
    {
        public Vector4 colorA;
        public Vector4 colorB;

        public ColorAB(BinaryReader br)
        {
            this.colorA = br.ReadColor();
            this.colorB = br.ReadColor();
        }

        public override string ToString()
        {
            return $"{this.colorA} {this.colorB}";
        }
    }

}
