using OpenTK.Mathematics;
using ProjectWS.Engine.Data.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
