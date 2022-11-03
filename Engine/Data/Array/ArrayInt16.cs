using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace ProjectWS.Engine.Data
{
    public class ArrayInt16 : Array<short>
    {
        public ArrayInt16(BinaryReader br, long startOffset)
        {
            long save = ReadArrayCommon(br, startOffset);

            // Read actual data
            for (uint i = 0; i < this.elements; i++)
            {
                data[i] = br.ReadInt16();
            }

            br.BaseStream.Position = save;
        }
    }
}