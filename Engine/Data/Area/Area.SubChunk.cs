using OpenTK.Mathematics;
using ProjectWS.Engine.Data.Extensions;

namespace ProjectWS.Engine.Data
{
    public partial class Area
    {
        public partial class SubChunk
        {
            public int index;
            public Flags flags;
            public ushort[] heightMap;
            public uint[] textureIDs;
            public byte[] blendMap;
            public byte[] colorMap;
            public ushort[] unknownMap;
            public ushort[] lodHeightMap;
            public ushort[] lodHeightRange;     // [0] Minimum, [1] Maximum


            public uint[] propUniqueIDs;
            public Curd curd;                   // https://cdn.discordapp.com/attachments/487618232279105540/924871987392823327/unknown.png
            public bool hasWater;
            public Water[] waters;

            public Mesh mesh;
            public Materials.TerrainMaterial material;
            public Matrix4 matrix;
            public float minHeight;
            public float maxHeight;
            public Data.BoundingBox AABB;
            public Data.BoundingBox cullingAABB;
            public Vector3 centerPosition;
            public volatile bool isVisible;
            public bool isOccluded;
            public bool isCulled;
            public float distanceToCam;
            //public Rect screenRect;
            public bool rectBehindCamera;
            public Vector2 subCoords;

            // Reference //
            public World.Chunk chunk;

            public SubChunk(BinaryReader br, World.Chunk chunk, int index, int lod)
            {
                this.minHeight = ushort.MaxValue;
                this.maxHeight = 0;
                this.chunk = chunk;
                uint subchunkSize = br.ReadUInt32();
                if (subchunkSize > br.BaseStream.Length)
                {
                    br.BaseStream.Position -= 4;
                    subchunkSize = br.ReadUInt16();
                    ushort unk = br.ReadUInt16();
                }

                long save = br.BaseStream.Position;
                this.index = index;

                this.flags = (Flags)br.ReadUInt32();

                // Height Map //
                if (this.flags.HasFlag(Flags.hasHeightmap))
                {
                    this.heightMap = new ushort[19 * 19];
                    for (int x = 0; x < this.heightMap.Length; x++)
                    {
                        var heightValue = br.ReadUInt16();
                        this.heightMap[x] = heightValue;
                    }
                }

                // Texture IDs //
                if (this.flags.HasFlag(Flags.hasTextureIDs))
                {
                    this.textureIDs = new uint[4];
                    for (int i = 0; i < 4; i++)
                    {
                        this.textureIDs[i] = br.ReadUInt32();
                    }
                }

                // Blend Map //
                if (this.flags.HasFlag(Flags.hasBlendMap))
                {
                    this.blendMap = new byte[65 * 65 * 4];
                    for (int i = 0; i < 65 * 65; i++)
                    {
                        ushort val = br.ReadUInt16();
                        /*
                        int value = (val & (0xF << (i * 4))) >> (i * 4);
                        byte blend = (byte)((value / 15.0f) * 255.0f);
                        this.blendMap[j] |= blend << (8 * i);
                        */
                    }
                }

                // Color Map //
                if (this.flags.HasFlag(Flags.hasColorMap))
                {
                    this.colorMap = new byte[65 * 65 * 4];
                    for (int i = 0; i < this.colorMap.Length; i++)
                    {
                        ushort val = br.ReadUInt16();
                    }

                    //uint32 r = value & 0x1F;
                    //uint32 g = (value >> 5) & 0x3F;
                    //uint32 b = (value >> 11) & 0x1F;
                    //r = (uint32) ((r / 31.0f) * 255.0f);
                    //g = (uint32) ((g / 63.0f) * 255.0f);
                    //b = (uint32) ((b / 31.0f) * 255.0f);
                }

                // Unknown Map //
                if (this.flags.HasFlag(Flags.hasUnkMap))
                {
                    this.unknownMap = new ushort[65 * 65];
                    for (int i = 0; i < this.unknownMap.Length; i++)
                    {
                        this.unknownMap[i] = br.ReadUInt16();
                    }
                }

                // Unknown data 4 bytes //
                if (this.flags.HasFlag(Flags.unk0x20))
                {
                    br.ReadBytes(4);
                }

                // Environment Data //
                if (this.flags.HasFlag(Flags.hasUnkLayerData))
                {
                    for (int i = 0; i < 4; i++)
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            br.ReadInt32();
                        }
                    }
                }

                // Environment Weights //
                if (this.flags.HasFlag(Flags.hasUnkLayerColor))
                {
                    for (int i = 0; i < 4; i++)
                    {
                        new Vector4(br.ReadByte(), br.ReadByte(), br.ReadByte(), br.ReadByte());
                    }
                }

                // Shadow Map //
                // 4225 bytes
                if (this.flags.HasFlag(Flags.hasShadowMap))
                {
                    br.ReadBytes(65 * 65);
                }

                // LoD Height Map //
                if (this.flags.HasFlag(Flags.hasLoDHeightMap))
                {
                    this.lodHeightMap = new ushort[33 * 33];
                    for (int x = 0; x < this.lodHeightMap.Length; x++)
                    {
                        this.lodHeightMap[x] = br.ReadUInt16();
                    }
                }

                // LoD Height Range //
                if (this.flags.HasFlag(Flags.hasLoDHeightRange))
                {
                    this.lodHeightRange = new ushort[2];
                    this.lodHeightRange[0] = br.ReadUInt16();
                    this.lodHeightRange[1] = br.ReadUInt16();
                }

                // Unknown Data //
                if (this.flags.HasFlag(Flags.unk0x800))
                {
                    br.BaseStream.Position += 578;
                }

                // Unknown Data //
                // Single byte
                if (this.flags.HasFlag(Flags.unk0x1000))
                {
                    br.ReadByte();
                }

                // Color Map DXT //
                // DXT5 65x65 texture, no mips, clamp
                if (this.flags.HasFlag(Flags.hascolorMapDXT))
                {
                    this.colorMap = br.ReadBytes(4624);
                }

                // Unknown Map DXT1 //
                if (this.flags.HasFlag(Flags.hasUnkMap0))
                {
                    br.ReadBytes(2312);
                }

                // Unknown Data //
                if (this.flags.HasFlag(Flags.unk0x8000))
                {
                    br.BaseStream.Position += 8450;
                }

                // Zone Bounds //
                if (this.flags.HasFlag(Flags.hasZoneBounds))
                {
                    br.ReadBytes(64 * 64);
                }

                // Blend Map DXT //
                // DXT1 65x65 texture, no mips, clamp
                if (this.flags.HasFlag(Flags.hasBlendMapDXT))
                {
                    this.blendMap = br.ReadBytes(2312);
                }

                // Unknown Map DXT1 //
                if (this.flags.HasFlag(Flags.hasUnkMap1))
                {
                    br.ReadBytes(2312);
                }

                // Unknown Map DXT1 //
                if (this.flags.HasFlag(Flags.hasUnkMap2))
                {
                    br.ReadBytes(2312);
                }

                // Unknown Map DXT1 //
                if (this.flags.HasFlag(Flags.hasUnkMap3))
                {
                    br.ReadBytes(2312);
                }

                // Flags //
                if (this.flags.HasFlag(Flags.unk0x200000))
                {
                    var flags = br.ReadByte();
                }

                // Unknown Data //
                if (this.flags.HasFlag(Flags.unk0x400000))
                {
                    for (int i = 0; i < 4; i++)
                    {
                        br.ReadInt32();
                    }
                }

                // Unknown Data //
                if (this.flags.HasFlag(Flags.unk0x800000))
                {
                    br.BaseStream.Position += 16900;
                }

                // Unknown Data //
                if (this.flags.HasFlag(Flags.unk0x1000000))
                {
                    br.BaseStream.Position += 8;
                }

                // Unknown Data //
                if (this.flags.HasFlag(Flags.unk0x2000000))
                {
                    br.BaseStream.Position += 8450;
                }

                // Unknown Data //
                if (this.flags.HasFlag(Flags.unk0x4000000))
                {
                    br.BaseStream.Position += 21316;
                }

                // Unknown Data //
                if (this.flags.HasFlag(Flags.unk0x8000000))
                {
                    br.BaseStream.Position += 4096;
                }

                // Zone IDs //
                if (this.flags.HasFlag(Flags.hasZoneIDs))
                {
                    for (int i = 0; i < 4; i++)
                    {
                        br.ReadUInt32();
                    }
                }

                // Unknown Data //
                if (this.flags.HasFlag(Flags.unk0x20000000))
                {
                    br.BaseStream.Position += 8450;
                }

                // Unknown Data //
                if (this.flags.HasFlag(Flags.unk0x40000000))
                {
                    br.BaseStream.Position += 8450;
                }

                // Unknown Map DXT1 //
                if (this.flags.HasFlag(Flags.hasUnkMap4))
                {
                    br.ReadBytes(2312);
                }

                while (br.BaseStream.Position < subchunkSize + save - 1)
                {
                    ChunkID chunkID = (ChunkID)br.ReadUInt32();
                    int chunkSize = br.ReadInt32();
                    switch (chunkID)
                    {
                        case ChunkID.PROP:
                            {
                                this.propUniqueIDs = new uint[chunkSize / 4];
                                for (int i = 0; i < this.propUniqueIDs.Length; i++)
                                {
                                    this.propUniqueIDs[i] = br.ReadUInt32();
                                }
                            }
                            break;
                        case ChunkID.curD:
                            {
                                this.curd = new Curd(br);
                            }
                            break;
                        case ChunkID.WAtG:
                            {
                                int count = br.ReadInt32();
                                if (count > 0)
                                {
                                    this.waters = new Water[count];
                                    for (int i = 0; i < count; i++)
                                    {
                                        this.waters[i] = new Water(br);
                                    }
                                    this.hasWater = true;
                                }
                            }
                            break;
                        default:
                            br.SkipChunk(chunkID.ToString(), chunkSize, this.GetType().ToString());
                            break;
                    }
                }

                br.BaseStream.Position = subchunkSize + save;

                if (lod == 0)
                    this.mesh = new Mesh(this.heightMap);
                else if (lod == 1)
                    this.mesh = new Mesh(this.lodHeightMap);

                this.material = new Materials.TerrainMaterial(this);

                // Calc Model Matrix
                int chunkX = index % 16;
                int chunkY = index / 16;
                this.subCoords = (this.chunk.coords * 16) + new Vector2(chunkX, chunkY);
                Vector3 subchunkRelativePosition = new Vector3(chunkX * 32f, 0, chunkY * 32f);
                this.matrix = Matrix4.CreateTranslation(subchunkRelativePosition);
                this.matrix *= chunk.worldMatrix;

                // Calc AABB
                float hMin = this.mesh.minHeight;
                float hMax = this.mesh.maxHeight;
                this.centerPosition = chunk.worldCoords + subchunkRelativePosition + new Vector3(16f, ((hMax - hMin) / 2f) + hMin, -16f);
                this.AABB = new Data.BoundingBox(this.centerPosition, new Vector3(32f, hMax - hMin, 32f));            // Exact bounds
                this.cullingAABB = new Data.BoundingBox(this.centerPosition, new Vector3(64f, (hMax - hMin) * 2, 64f));        // Increased bounds to account for thread delay
            }

            internal void Render(Shader shader)
            {
                this.material.Set(shader);
                this.mesh.Draw();
            }

            public void Build()
            {
                // Mesh //
                this.mesh.Build();

                // Material //
                this.material.Build();

                // Water //
                //if (this.waters != null && this.hasWater)
                //    BuildWater();

                // Curd test //
                // Seem to be some sort of paths or poligonal areas
                /*
                for (int i = 0; i < this.curd.positionCount; i++)
                {
                    GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    go.transform.position = this.curd.positions[i];
                    go.transform.localScale = Vector3.one * 10f;
                }
                */
            }

            void BuildWater()
            {
                for (int i = 0; i < this.waters.Length; i++)
                {
                    this.waters[i].Build();
                }
            }
        }
    }
}
