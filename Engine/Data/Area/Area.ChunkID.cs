using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectWS.Engine.Data
{
    public partial class Area
    {
        public enum ChunkID : uint
        {
            CHNK = 0x43484E4B,
            PROp = 0x50524F70,
            CURT = 0x43555254,
            area = 0x61726561,
        }
    }
}
