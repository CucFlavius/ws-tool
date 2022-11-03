using OpenTK;
using OpenTK.Mathematics;
using ProjectWS.Engine.Data.Extensions;
using System.IO;

namespace ProjectWS.Engine.Data
{
    public partial class Area
    {
        public struct Curd
        {
            public uint groupCount;
            public Group[] groups;

            public Curd(BinaryReader br)
            {
                var save = br.BaseStream.Position;
                this.groupCount = br.ReadUInt32();
                this.groups = new Group[this.groupCount];
                for (int i = 0; i < this.groupCount; i++)
                {
                    this.groups[i] = new Group(br);
                }
            }

            public struct Group
            {
                public uint positionCount;
                public Vector3[] positions;

                public Group(BinaryReader br)
                {
                    this.positionCount = br.ReadUInt32();
                    this.positions = new Vector3[this.positionCount];
                    for (int i = 0; i < this.positionCount; i++)
                    {
                        this.positions[i] = br.ReadVector3();
                    }
                }
            }
        }
    }
}
