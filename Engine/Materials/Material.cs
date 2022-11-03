using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectWS.Engine.Materials
{
    public abstract class Material
    {
        public Dictionary<string, uint> texturePtrs;
        public bool isBuilt = false;
        public bool isBuilding = false;

        public abstract void Build();

        public abstract void Set(Shader shader);

        public void SetTexture(string samplerName, uint ptr)
        {
            this.texturePtrs.Add(samplerName, ptr);
        }
    }
}
