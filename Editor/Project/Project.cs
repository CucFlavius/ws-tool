using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectWS.Engine.Project
{
    public class Project
    {
        public Guid UUID { get; set; }
        public string? Name { get; set; }
        public List<Map>? Maps { get; set; }
        public uint lastID { get; set; }

        public class Map
        {
            public string? Name { get; set; }
            public Database.Definitions.World? worldRecord { get; set; }
        }

        public Project()
        {
            this.Maps = new List<Map>();
        }
    }
}
