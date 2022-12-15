using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectWS.Engine.Data
{
    public class Variant : ArrayData
    {
        public byte[]? data;

        public override void Read(BinaryReader br, long startOffset)
        {
            //Debug.Log(startOffset);
            this.data = new ArrayByte(br, startOffset).data;
        }

        public override int GetSize()
        {
            return 16;
        }
    }
}
