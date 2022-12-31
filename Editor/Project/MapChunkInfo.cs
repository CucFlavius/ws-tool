using MathUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectWS.Editor.Project
{
    public class MapChunkInfo
    {
        public List<Vector2>? chunks { get; set; }
        public List<Vector2>? chunksLow { get; set; }
        public List<Vector2>? minimaps { get; set; }
    }
}
