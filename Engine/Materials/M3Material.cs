using OpenTK.Graphics.OpenGL4;
using ProjectWS.Engine.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ProjectWS.Engine.Data.M3;

namespace ProjectWS.Engine.Materials
{
    public class M3Material : Material
    {
        M3.Material matData;
        M3 m3;

        public M3Material(M3.Material matData, M3 m3)
        {
            this.matData = matData;
            this.m3 = m3;
        }

        public override void Build()
        {
            if (this.isBuilt || this.isBuilding) return;

            this.isBuilding = true;

            ProjectWS.Engine.Data.ResourceManager.Manager rm = m3.gameData.resourceManager;

            this.texturePtrs = new Dictionary<string, uint>();
            for (int i = 0; i < this.matData.materialDescriptions.Length; i++)
            {
                short textureA = this.matData.materialDescriptions[i].textureSelectorA;
                if (textureA != -1)
                {
                    var texture = m3.textures[textureA];

                    if (texture.textureType == M3.Texture.TextureType.Diffuse)
                        rm.AssignTexture(texture.texturePath, this, $"diffuseMap{i}");
                    else if (texture.textureType == M3.Texture.TextureType.Normal)
                        rm.AssignTexture(texture.texturePath, this, $"normalMap{i}");
                }

                short textureB = this.matData.materialDescriptions[i].textureSelectorB;
                if (textureB != -1)
                {
                    var texture = m3.textures[textureB];
                    if (texture.textureType == M3.Texture.TextureType.Diffuse)
                        rm.AssignTexture(texture.texturePath, this, $"diffuseMap{i}");
                    else if (texture.textureType == M3.Texture.TextureType.Normal)
                        rm.AssignTexture(texture.texturePath, this, $"normalMap{i}");
                }
            }


            this.isBuilt = true;
        }

        public override void SetToShader(Shader shader)
        {
            if (!this.isBuilt) return;

            for (int j = 0; j < this.matData.materialDescriptions.Length; j++)
            {
                if (this.texturePtrs.TryGetValue($"diffuseMap{j}", out uint texDiffusePtr))
                {
                    if (j == 0)
                        GL.ActiveTexture(TextureUnit.Texture0);
                    else if (j == 1)
                        GL.ActiveTexture(TextureUnit.Texture2);
                    else if (j == 2)
                        GL.ActiveTexture(TextureUnit.Texture4);
                    else if (j == 3)
                        GL.ActiveTexture(TextureUnit.Texture6);
                    GL.BindTexture(TextureTarget.Texture2D, texDiffusePtr);
                }
                if (this.texturePtrs.TryGetValue($"normalMap{j}", out uint texNormalPtr))
                {
                    if (j == 0)
                        GL.ActiveTexture(TextureUnit.Texture1);
                    else if (j == 1)
                        GL.ActiveTexture(TextureUnit.Texture3);
                    else if (j == 2)
                        GL.ActiveTexture(TextureUnit.Texture5);
                    else if (j == 3)
                        GL.ActiveTexture(TextureUnit.Texture7);
                    GL.BindTexture(TextureTarget.Texture2D, texNormalPtr);
                }
            }
        }
    }
}
