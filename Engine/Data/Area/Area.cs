using OpenTK;
using OpenTK.Mathematics;
using ProjectWS.Engine.Data.Extensions;
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
        Block.FileEntry fileEntry;
        GameData gameData;
        World.Chunk chunk;
        int lod;

        public float minHeight;
        public float maxHeight;

        public Header header;
        public List<SubChunk> subChunks;    // Variable size, not always 16*16
        public Prop[] props;
        public Dictionary<uint, Prop> uuidPropMap;
        public Curt[] curts;

        public Area(World.Chunk chunk, int lod)
        {
            this.lod = lod;
            this.chunk = chunk;
            this.fileEntry = chunk.areaFileEntry;
            this.gameData = chunk.gameData;
            this.minHeight = float.MaxValue;
            this.maxHeight = float.MinValue;

            //Debug.Log(chunk.coords.ToString());
        }

        public void Read()
        {
            try
            {
                using (Stream str = this.gameData.GetFileData(this.fileEntry))
                {
                    using (BinaryReader br = new BinaryReader(str))
                    {
                        this.header = new Header(br);

                        long streamPos = br.BaseStream.Position;

                        while (streamPos < br.BaseStream.Length)
                        {
                            br.BaseStream.Position = streamPos;
                            ChunkID chunkID = (ChunkID)br.ReadUInt32();
                            int chunkSize = br.ReadInt32();
                            streamPos = br.BaseStream.Position + chunkSize;

                            switch (chunkID)
                            {
                                case ChunkID.CHNK:
                                    {
                                        this.subChunks = new List<SubChunk>();
                                        long save = br.BaseStream.Position;
                                        int index = 0;
                                        while (br.BaseStream.Position < save + chunkSize)
                                        {
                                            var subchunk = new SubChunk(br, chunk, index++, this.lod);
                                            this.subChunks.Add(subchunk);

                                            // Calc minmax
                                            if (subchunk.mesh.minHeight < this.minHeight)
                                                this.minHeight = subchunk.mesh.minHeight;
                                            if (subchunk.mesh.maxHeight > this.maxHeight)
                                                this.maxHeight = subchunk.mesh.maxHeight;
                                        }
                                    }
                                    break;
                                case ChunkID.PROp:
                                    {
                                        long chunkStart = br.BaseStream.Position;
                                        int propCount = br.ReadInt32();
                                        this.props = new Prop[propCount];
                                        this.uuidPropMap = new Dictionary<uint, Prop>();
                                        for (int i = 0; i < propCount; i++)
                                        {
                                            this.props[i] = new Prop(br, chunkStart);
                                            this.uuidPropMap.Add(this.props[i].uniqueID, this.props[i]);
                                        }
                                        //File.WriteAllText($"D:/Props_{this.chunk.coords}.json", JsonConvert.SerializeObject(this.props, Formatting.Indented));
                                        br.BaseStream.Position = chunkStart + chunkSize;
                                    }
                                    break;
                                case ChunkID.CURT:
                                    {
                                        var chunkStart = br.BaseStream.Position;
                                        uint curtCount = br.ReadUInt32();
                                        this.curts = new Curt[curtCount];
                                        for (int i = 0; i < curtCount; i++)
                                        {
                                            this.curts[i] = new Curt(br, chunkStart);
                                        }
                                    }
                                    break;
                                default:
                                    br.SkipChunk(chunkID.ToString(), chunkSize, this.GetType().ToString());
                                    break;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Exception at : " + this.chunk.gameData.archives["ClientData"].fileNames[this.fileEntry.hash] + " " + this.chunk.coords);
                Debug.LogException(e);
            }
        }

        public void Build(int quadrant)
        {
            if (this.lod == 0)
            {
                int from = quadrant * (256 / 4);
                int to = from + (256 / 4);
                int total = this.subChunks.Count;
                for (int i = from; i < to; i++)
                {
                    if (i < total)
                    {
                        this.subChunks[i].Build();
                    }
                }
                this.chunk.lod0Available = true;

                // Update AABB with new heights
                float hMin = this.minHeight;
                float hMax = this.maxHeight;
                float dist = Math.Abs(hMax - hMin);
                Vector3 center = this.chunk.worldCoords + new Vector3(0, (dist / 2f) + hMin, 0);
                this.chunk.AABB = new BoundingBox(center + new Vector3(256f, 0, -256f), new Vector3(512f, dist, 512f));
            }
            else if (this.lod == 1)
            {
                if (quadrant < this.subChunks.Count)
                {
                    this.subChunks[quadrant].Build();
                    this.chunk.lod1Available = true;
                }
            }
        }
    }
}
