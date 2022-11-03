using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectWS.Engine.Data
{
    public partial class Area
    {
        public partial class SubChunk
        {
            public enum ChunkID : uint
            {
                PROP = 0x50524F50,
                curD = 0x63757244,
                WAtG = 0x57417447,
                wbsP = 0x77627350,
            }
        }
    }
}
