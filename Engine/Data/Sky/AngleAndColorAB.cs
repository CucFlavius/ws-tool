using OpenTK.Mathematics;
using ProjectWS.Engine.Data.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectWS.Engine.Data
{
    public class AngleAndColorAB
    {
        public Vector4 angle;   // Euler
        public Vector4 colorA;
        public Vector4 colorB;

        public AngleAndColorAB(BinaryReader br)
        {
            this.angle = br.ReadVector4();
            this.colorA = br.ReadColor();
            this.colorB = br.ReadColor();
        }

        public override string ToString()
        {
            return $"{this.angle} {this.colorA} {this.colorB}";
        }
    }

}
