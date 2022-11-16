using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectWS.Editor.Tools
{
    public abstract class Tool
    {
        public bool isEnabled;

        public abstract void Update(float deltaTime);
    }
}
