using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using ProjectWS.Engine.Rendering.ShaderParams;
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
        uint unkMap2Ptr;
        Vector4 heightScale = Vector4.One;
        Vector4 heightOffset = Vector4.Zero;
        Vector4 parallaxScale = Vector4.One;
        Vector4 parallaxOffset = Vector4.Zero;
        Vector4 metersPerTextureTile;

        const string LAYER0 = "layer0";
        const string LAYER1 = "layer1";
        const string LAYER2 = "layer2";
        const string LAYER3 = "layer3";
        const string NORMAL0 = "normal0";
        const string NORMAL1 = "normal1";
        const string NORMAL2 = "normal2";
        const string NORMAL3 = "normal3";

        TerrainParameters tParams;

        public TerrainMaterial(Data.Area.SubChunk subChunk)
        {
            this.subChunk = subChunk;
            this.tParams = new TerrainParameters();
        }

        public override void Build()
        {
            if (this.isBuilt || this.isBuilding) return;

            this.isBuilding = true;

            this.texturePtrs = new Dictionary<string, uint>();

            if (this.subChunk.blendMapMode == Data.Area.SubChunk.MapMode.DXT1)
                BuildMap(this.subChunk.blendMap, InternalFormat.CompressedRgbaS3tcDxt1Ext, out blendMapPtr);
            else if (this.subChunk.blendMapMode == Data.Area.SubChunk.MapMode.Raw)
                BuildMap(this.subChunk.blendMap, InternalFormat.Rgba, out blendMapPtr);

            BuildMap(this.subChunk.unknownMap2, InternalFormat.CompressedRgbaS3tcDxt1Ext, out unkMap2Ptr);

            if (this.subChunk.colorMapMode == Data.Area.SubChunk.MapMode.DXT1)
                BuildMap(this.subChunk.colorMap, InternalFormat.CompressedRgbaS3tcDxt5Ext, out colorMapPtr);
            else if (this.subChunk.colorMapMode == Data.Area.SubChunk.MapMode.Raw)
                BuildMap(this.subChunk.colorMap, InternalFormat.Rgba, out colorMapPtr);

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
            else
            {
                this.subChunk.chunk.gameData.resourceManager.AssignTexture("Art\\Dev\\BLANK_Grey.tex", this, $"layer0");
                this.subChunk.chunk.gameData.resourceManager.AssignTexture("Art\\Dev\\BLANK_Normal.tex", this, $"normal0");

                for (int i = 0; i < 4; i++)
                {
                    metersPerTextureTile[i] = 1.0f;
                }
            }

            this.isBuilt = true;
        }

        public override void SetToShader(Shader shader)
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

            this.tParams.enableColorMap = this.colorMapPtr != 0;

            if (this.tParams.enableColorMap)
            {
                GL.ActiveTexture(TextureUnit.Texture9);
                GL.BindTexture(TextureTarget.Texture2D, this.colorMapPtr);
            }

            if (this.unkMap2Ptr != 0)
            {
                GL.ActiveTexture(TextureUnit.Texture10);
                GL.BindTexture(TextureTarget.Texture2D, this.unkMap2Ptr);
            }

            this.tParams.heightScale = this.heightScale;
            this.tParams.heightOffset = this.heightOffset;
            this.tParams.parallaxScale = this.parallaxScale;
            this.tParams.parallaxOffset = this.parallaxOffset;
            this.tParams.metersPerTextureTile = this.metersPerTextureTile;

            this.tParams.SetToShader(shader);
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
            
            if (format == InternalFormat.Rgba)
            {
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 65, 65, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Rgba, PixelType.UnsignedByte, data);
                //GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 65, 65, 0, PixelFormat.Rgba, PixelType.Byte, data);
            }
            else
            {
                GL.CompressedTexImage2D(TextureTarget.Texture2D, 0, format, 65, 65, 0, data.Length, data);
            }
        }
    }
}
