using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace ProjectWS.Engine.Rendering.ShaderParams
{
    public class TerrainParameters
    {
        public Vector4 heightScale;
        public Vector4 heightOffset;
        public Vector4 parallaxScale;
        public Vector4 parallaxOffset;
        public Vector4 metersPerTextureTile;
        public float specularPower;
        public Vector2 scrollSpeed;
        public bool enableColorMap;
        public bool enableUnkMap2;

        public TerrainParameters()
        {
            this.metersPerTextureTile = new Vector4(32.0f, 32.0f, 32.0f, 32.0f);
        }


        public void SetToShader(Shader shader)
        {
            GL.Uniform4(GL.GetUniformLocation(shader.Handle, "terrainParams.heightScale"), this.heightScale);
            GL.Uniform4(GL.GetUniformLocation(shader.Handle, "terrainParams.heightOffset"), this.heightOffset);
            GL.Uniform4(GL.GetUniformLocation(shader.Handle, "terrainParams.parallaxScale"), this.parallaxScale);
            GL.Uniform4(GL.GetUniformLocation(shader.Handle, "terrainParams.parallaxOffset"), this.parallaxOffset);
            GL.Uniform4(GL.GetUniformLocation(shader.Handle, "terrainParams.metersPerTextureTile"), this.metersPerTextureTile);
            GL.Uniform1(GL.GetUniformLocation(shader.Handle, "terrainParams.specularPower"), this.specularPower);
            GL.Uniform2(GL.GetUniformLocation(shader.Handle, "terrainParams.scrollSpeed"), this.scrollSpeed);
            GL.Uniform1(GL.GetUniformLocation(shader.Handle, "terrainParams.enableColorMap"), this.enableColorMap ? 1 : 0);
            GL.Uniform1(GL.GetUniformLocation(shader.Handle, "terrainParams.enableUnkMap2"), this.enableUnkMap2 ? 1 : 0);
        }
    }
}