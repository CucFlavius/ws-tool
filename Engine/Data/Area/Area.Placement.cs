using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectWS.Engine.Data
{
    public partial class Area
    {
        public struct Placement
        {
            public ushort minX;
            public ushort minY;
            public ushort maxX;
            public ushort maxY;

            public ushort size;     // 1 2 4 8 16 32 ...
            public float sizef;
            public int[] subChunkIndices;

            public Placement(BinaryReader br)
            {
                this.minX = br.ReadUInt16();
                this.minY = br.ReadUInt16();
                this.maxX = br.ReadUInt16();
                this.maxY = br.ReadUInt16();

                this.size = (ushort)((this.maxX - this.minX) * (this.maxY - this.minY));
                this.sizef = (float)this.size;
                this.subChunkIndices = new int[this.size];

                // TODO : needs accounting for the fact that the referenced subchunks can be part of adjacent chunks too
                /*
                int i = 0;
                for (int x = this.minX; x < this.maxX; x++)
                {
                    for (int y = this.minY; y < this.maxY; y++)
                    {
                        this.subChunkIndices[i] = 
                        i++;
                    }
                }
                */
            }
        }
    }
}
