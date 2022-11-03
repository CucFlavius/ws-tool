using System.Collections;
using System.Collections.Generic;

namespace ProjectWS.Engine.Data
{
    public partial class Tex
    {
        public partial class Header
        {
            public enum TextureType
            {
                Unknown,
                Jpeg1,
                Jpeg2,
                Jpeg3,
                Argb1,
                Argb2,
                Rgb,
                Grayscale,
                DXT1,
                DXT3,
                DXT5,
            }
        }
    }
}