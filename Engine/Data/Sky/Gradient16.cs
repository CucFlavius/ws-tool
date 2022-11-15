using OpenTK.Mathematics;
using ProjectWS.Engine.Data.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectWS.Engine.Data
{
    public class Gradient16
    {
        public byte[] padding;
        public Vector4[] colors;

        public Gradient16(BinaryReader br)
        {
            this.padding = br.ReadBytes(16);
            this.colors = new Vector4[16];
            for (int i = 0; i < 16; i++)
            {
                this.colors[i] = br.ReadColor();
            }
        }
    }

}
