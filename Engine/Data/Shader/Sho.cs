using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectWS.Engine.Data
{
    public class Sho
    {
        public string filePath;
        public bool failedReading;

        public Variant[]? variants;

        public Sho(string filePath)
        {
            this.filePath = filePath;
            this.failedReading = false;
        }

        public void Read()
        {
            using (Stream str = File.OpenRead(this.filePath))
            {
                Read(str);
            }
        }

        public void Read(Stream str)
        {
            try
            {
                if (str == null)
                {
                    this.failedReading = true;
                    return;
                }

                using (BinaryReader br = new BinaryReader(str))
                {
                    uint magic = br.ReadUInt32();
                    uint version = br.ReadUInt32();
                    this.variants = new ArrayStruct<Variant>(br, 80).data;
                    // UnkArray 0
                    // UnkArray 1
                    // UnkArray 2
                    // Unk uint64
                }
            }
            catch (Exception e)
            {
                this.failedReading = true;
                Debug.LogError($"SHO : Failed Reading File {this.filePath}");
                Debug.LogException(e);
            }
        }
    }
}
