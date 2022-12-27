using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ProjectWS.Engine.Data
{
    public class AssetDatabase
    {
        public int version { get; set; }

        public AssetDatabase()
        {
        }
    }
}
