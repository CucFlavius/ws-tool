using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace ProjectWS.Engine.Data
{
    public partial class M3
    {
        [StructLayout(LayoutKind.Sequential)]
        [Serializable]
        public class Vertex
        {
            public Vector3 position;
            public Vector3 tangent;
            public Vector3 normal;
            public Vector3 bitangent;
            public int[] boneIndices;
            public Vector4 boneWeights;
            public Vector4 color0;
            public Vector4 color1;
            public Vector2 uv0;
            public Vector2 uv1;
            public int unk;
        }
    }
}