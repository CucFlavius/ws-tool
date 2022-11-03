using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectWS.Engine.Materials
{
    public class TerrainMaterial : Material
    {
        Data.Area.SubChunk subChunk;
        uint blendMapPtr;
        uint colorMapPtr;
        Vector4 heightScale;
        Vector4 heightOffset;
        Vector4 parallaxScale;
        Vector4 parallaxOffset;
        Vector4 metersPerTextureTile;

        const string LAYER0 = "layer0";
        const string LAYER1 = "layer1";
        const string LAYER2 = "layer2";
        const string LAYER3 = "layer3";
        const string NORMAL0 = "normal0";
        const string NORMAL1 = "normal1";
        const string NORMAL2 = "normal2";
        const string NORMAL3 = "normal3";

        public TerrainMaterial(Data.Area.SubChunk subChunk)
        {
            this.subChunk = subChunk;
        }

        public override void Build()
        {
            if (this.isBuilt || this.isBuilding) return;

            this.isBuilding = true;

            this.texturePtrs = new Dictionary<string, uint>();

            BuildMap(this.subChunk.blendMap, InternalFormat.CompressedRgbaS3tcDxt1Ext, out blendMapPtr);
            BuildMap(this.subChunk.colorMap, InternalFormat.CompressedRgbaS3tcDxt5Ext, out colorMapPtr);

            if (this.subChunk.textureIDs != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (this.subChunk.textureIDs[i] != 0)
                    {
                        var record = this.subChunk.chunk.gameData.database.worldLayer.Get(this.subChunk.textureIDs[i]);
                        if (record != null)
                        {
                            this.subChunk.chunk.gameData.resourceManager.AssignTexture(record.ColorMapPath, this, $"layer{i}");
                            this.subChunk.chunk.gameData.resourceManager.AssignTexture(record.NormalMapPath, this, $"normal{i}");

                            heightScale[i] = record.HeightScale;
                            heightOffset[i] = record.HeightOffset;
                            parallaxScale[i] = record.ParallaxScale;
                            parallaxOffset[i] = record.ParallaxOffset;
                            metersPerTextureTile[i] = record.MetersPerTextureTile;
                        }
                    }
                }
            }

            this.isBuilt = true;
        }

        public override void Set(Shader shader)
        {
            if (!this.isBuilt) return;

            if (this.texturePtrs.TryGetValue(LAYER0, out uint layer0Ptr))
            {
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, layer0Ptr);
            }
            else
            {
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, -1);
            }

            if (this.texturePtrs.TryGetValue(LAYER1, out uint layer1Ptr))
            {
                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2D, layer1Ptr);
            }
            else
            {
                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2D, -1);
            }

            if (this.texturePtrs.TryGetValue(LAYER2, out uint layer2Ptr))
            {
                GL.ActiveTexture(TextureUnit.Texture2);
                GL.BindTexture(TextureTarget.Texture2D, layer2Ptr);
            }
            else
            {
                GL.ActiveTexture(TextureUnit.Texture2);
                GL.BindTexture(TextureTarget.Texture2D, -1);
            }

            if (this.texturePtrs.TryGetValue(LAYER3, out uint layer3Ptr))
            {
                GL.ActiveTexture(TextureUnit.Texture3);
                GL.BindTexture(TextureTarget.Texture2D, layer3Ptr);
            }
            else
            {
                GL.ActiveTexture(TextureUnit.Texture3);
                GL.BindTexture(TextureTarget.Texture2D, -1);
            }

            if (this.texturePtrs.TryGetValue(NORMAL0, out uint normal0Ptr))
            {
                GL.ActiveTexture(TextureUnit.Texture4);
                GL.BindTexture(TextureTarget.Texture2D, normal0Ptr);
            }

            if (this.texturePtrs.TryGetValue(NORMAL1, out uint normal1Ptr))
            {
                GL.ActiveTexture(TextureUnit.Texture5);
                GL.BindTexture(TextureTarget.Texture2D, normal1Ptr);
            }

            if (this.texturePtrs.TryGetValue(NORMAL2, out uint normal2Ptr))
            {
                GL.ActiveTexture(TextureUnit.Texture6);
                GL.BindTexture(TextureTarget.Texture2D, normal2Ptr);
            }

            if (this.texturePtrs.TryGetValue(NORMAL3, out uint normal3Ptr))
            {
                GL.ActiveTexture(TextureUnit.Texture7);
                GL.BindTexture(TextureTarget.Texture2D, normal3Ptr);
            }

            GL.ActiveTexture(TextureUnit.Texture8);
            GL.BindTexture(TextureTarget.Texture2D, this.blendMapPtr);

            if (this.colorMapPtr != 0)
            {
                GL.ActiveTexture(TextureUnit.Texture9);
                GL.BindTexture(TextureTarget.Texture2D, this.colorMapPtr);
                shader.SetInt("enableColorMap", 1);
            }
            else
            {
                shader.SetInt("enableColorMap", 0);
                //GL.ActiveTexture(TextureUnit.Texture9);
                //GL.BindTexture(TextureTarget.Texture2D, this.subChunk.chunk.world.engine.resourceManager.textureResources[ResourceManager.EngineTextures.white].texturePtr);
            }

            shader.SetVec4("heightScale", this.heightScale);
            shader.SetVec4("heightOffset", this.heightOffset);
            shader.SetVec4("parallaxScale", this.parallaxScale);
            shader.SetVec4("parallaxOffset", this.parallaxOffset);
            shader.SetVec4("metersPerTextureTile", this.metersPerTextureTile);
        }

        void BuildMap(byte[] data, InternalFormat format, out uint ptr)
        {
            if (data == null) { ptr = 0; return; }

            GL.GenTextures(1, out ptr);
            GL.BindTexture(TextureTarget.Texture2D, ptr);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.GenerateMipmap, 0);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 0);
            GL.CompressedTexImage2D(TextureTarget.Texture2D, 0, format, 65, 65, 0, data.Length, data);
        }
    }
}
