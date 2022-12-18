using ProjectWS.Engine.Material;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectWS.Engine.Mesh
{
    public class M3Geometry
    {
        FileFormats.M3.Geometry? data;
        public M3Mesh[]? meshes;
        public bool isBuilt;

        public M3Geometry(FileFormats.M3.Geometry data)
        {
            this.data = data;
            if (data.submeshes != null)
                this.meshes = new M3Mesh[data.submeshes.Length];
        }

        internal void Build(int m3ModelID)
        {
            if (this.data == null || this.meshes == null || this.data.submeshes == null || this.data.vertexData == null || this.data.indexData == null) return;

            for (int i = 0; i < this.meshes.Length; i++)
            {
                var submesh = this.data.submeshes[i];

                submesh.vertexData = new byte[submesh.vertexCount * this.data.vertexBlockSizeInBytes];
                Array.Copy(this.data.vertexData, submesh.startVertex * this.data.vertexBlockSizeInBytes, submesh.vertexData, 0, submesh.vertexCount * this.data.vertexBlockSizeInBytes);

                submesh.indexData = new uint[submesh.indexCount];
                Array.Copy(this.data.indexData, submesh.startIndex, submesh.indexData, 0, submesh.indexCount);


                if (submesh.meshGroupID == m3ModelID || submesh.meshGroupID == -1)
                {
                    this.meshes[i] = new M3Mesh(submesh, this.data.vertexBlockSizeInBytes, this.data.vertexBlockFieldPositions, this.data.vertexBlockFlags, this.data.vertexFieldTypes);
                    this.meshes[i].Build();
                }
            }

            this.isBuilt = true;
        }
    }
}
