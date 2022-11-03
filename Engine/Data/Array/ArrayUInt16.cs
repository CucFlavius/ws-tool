using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace ProjectWS.Engine.Data
{
    public class ArrayUInt16 : Array<ushort>
    {
        public ArrayUInt16(BinaryReader br, long startOffset)
        {
            long save = ReadArrayCommon(br, startOffset);

            // Read actual data
            for (uint i = 0; i < this.elements; i++)
            {
                data[i] = br.ReadUInt16();
            }

            br.BaseStream.Position = save;
        }
    }
}