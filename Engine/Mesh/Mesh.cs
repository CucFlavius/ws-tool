using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectWS.Engine.Mesh
{
    public abstract class Mesh
    {
        public abstract void Build();
        public abstract void Draw();
        public abstract void DrawInstanced();
    }
}
