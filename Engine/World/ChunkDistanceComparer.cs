using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectWS.Engine.World
{
    public class ChunkDistanceComparer : IComparer<Data.Area.SubChunk>
    {
        public int Compare(Data.Area.SubChunk a, Data.Area.SubChunk b)
        {
            if (a.distanceToCam > b.distanceToCam)
                return 1;
            else
                return -1;
        }
    }
}
