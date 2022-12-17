using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace ProjectWS.Engine.Data
{
    public abstract class ArrayData
    {

        public abstract void Read(BinaryReader br, long endOffset = 0);
        public abstract int GetSize();
    }
}