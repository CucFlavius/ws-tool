using OpenTK;
using OpenTK.Mathematics;
using ProjectWS.Engine.Data.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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
        const string area = "area";
        const string AREA = "AREA";

        public Header header;
        public List<SubChunk>? subChunks;    // Variable size, not always 16*16
        public Prop[]? props;
        public Dictionary<uint, Prop>? uuidPropMap;
        public Curt[]? curts;

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
                using (Stream? str = this.gameData.GetFileData(this.fileEntry))
                {
                    if (str == null) return;
                    if (str.Length == 0) return;

                    using (var br = new BinaryReader(str))
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
                                        if (this.header.magic == AREA)
                                        {
                                            // Compressed Area file
                                            var chunkStart = br.BaseStream.Position;
                                            var chunkData = br.ReadBytes(chunkSize);
                                            using MemoryStream chunkMS = new MemoryStream(chunkData);
                                            using BinaryReader chunkBR = new BinaryReader(chunkMS);
                                            int size = chunkBR.ReadInt32();
                                            var save = chunkBR.BaseStream.Position;
                                            using var headerZlibStream = new ZLibStream(chunkMS, CompressionMode.Decompress);
                                            Span<byte> decompressedHeader = new byte[1024];
                                            headerZlibStream.Read(decompressedHeader);
                                            using MemoryStream decompHeaderMS = new MemoryStream(decompressedHeader.ToArray());
                                            using BinaryReader decompHeaderBR = new BinaryReader(decompHeaderMS);
                                            List<uint> blobSizes = new List<uint>();
                                            List<byte[]> blobs = new List<byte[]>();
                                            chunkBR.BaseStream.Position = save + size;
                                            for (int i = 0; i < 1024 / 4; i++)
                                            {
                                                uint blobSize = decompHeaderBR.ReadUInt32();
                                                blobSizes.Add(blobSize);
                                                if (blobSize != 0)
                                                {
                                                    blobs.Add(chunkBR.ReadBytes((int)blobSize));
                                                }
                                                else
                                                {
                                                    blobs.Add(null);
                                                }
                                            }

                                            this.subChunks = new List<SubChunk>();

                                            for (int i = 0; i < blobs.Count; i++)
                                            {
                                                if (blobSizes[i] != 0)
                                                {
                                                    using MemoryStream blobMS = new MemoryStream(blobs[i]);
                                                    using BinaryReader blobBR = new BinaryReader(blobMS);
                                                    var decompSize = blobBR.ReadInt32();
                                                    using var blobZlibStream = new ZLibStream(blobMS, CompressionMode.Decompress);

                                                    Span<byte> decompData = new byte[decompSize];
                                                    blobZlibStream.Read(decompData);

                                                    var decompBlob = new byte[decompSize + 4];

                                                    // Copy size at the front of the data
                                                    var bSize = BitConverter.GetBytes(decompSize);
                                                    decompBlob[0] = bSize[0];
                                                    decompBlob[1] = bSize[1];
                                                    decompBlob[2] = bSize[2];
                                                    decompBlob[3] = bSize[3];

                                                    // Copy data after size
                                                    Array.Copy(decompData.ToArray(), 0, decompBlob, 4, decompSize);

                                                    using (var decompDataMS = new MemoryStream(decompBlob))
                                                    {
                                                        using (var decompDataBR = new BinaryReader(decompDataMS))
                                                        {
                                                            var subchunk = new SubChunk(decompDataBR, this.chunk, i, this.lod, true);
                                                            this.subChunks.Add(subchunk);

                                                            // Calc minmax
                                                            if (subchunk.mesh.minHeight < this.minHeight)
                                                                this.minHeight = subchunk.mesh.minHeight;
                                                            if (subchunk.mesh.maxHeight > this.maxHeight)
                                                                this.maxHeight = subchunk.mesh.maxHeight;
                                                        }
                                                    }
                                                }
                                            }

                                            br.BaseStream.Position = chunkStart + chunkSize;
                                        }

                                        if (this.header.magic == area)
                                        {
                                            this.subChunks = new List<SubChunk>();
                                            long save = br.BaseStream.Position;
                                            int index = 0;

                                            while (br.BaseStream.Position < save + chunkSize)
                                            {
                                                var subchunk = new SubChunk(br, this.chunk, index++, this.lod, false);
                                                this.subChunks.Add(subchunk);

                                                // Calc minmax
                                                if (subchunk.mesh.minHeight < this.minHeight)
                                                    this.minHeight = subchunk.mesh.minHeight;
                                                if (subchunk.mesh.maxHeight > this.maxHeight)
                                                    this.maxHeight = subchunk.mesh.maxHeight;
                                            }
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
