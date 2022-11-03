using System.IO;
using ProjectWS.Engine.Data.Extensions;

namespace ProjectWS.Engine.Data
{
    public partial class Area
    {
        public struct Header
        {
            public string magic;
            public uint version;

            public Header(BinaryReader br)
            {
                this.magic = br.ReadChunkID();
                this.version = br.ReadUInt32();
            }
        }
    }
}
