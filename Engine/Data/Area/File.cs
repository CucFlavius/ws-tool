using BCnEncoder.Encoder;
using BCnEncoder.Shared;
using MathUtils;
using ProjectWS.FileFormats.Extensions;
using System.IO.Compression;

namespace ProjectWS.Engine.Data.Area
{
    public class Area
    {
        Block.FileEntry? fileEntry;
        string? filePath;
        GameData? gameData;
        World.Chunk? chunk;
        int lod;

        public float minHeight;
        public float maxHeight;
        const uint area = 1634887009;
        const uint AREA = 1095910721;

        public Header header;
        public List<SubChunk>? subChunks;    // Variable size, not always 16*16
        public List<Prop>? props;
        public Dictionary<uint, Prop>? propLookup;
        public HashSet<uint>? renderedProps;
        public Curt[]? curts;

        public Action? onFinishedReading;

        public Area(World.Chunk chunk, int lod)
        {
            this.lod = lod;
            this.chunk = chunk;
            this.fileEntry = chunk.areaFileEntry;
            this.filePath = chunk.areaFilePath;
            this.gameData = chunk.gameData;
            this.minHeight = float.MaxValue;
            this.maxHeight = float.MinValue;
            renderedProps = new HashSet<uint>();
        }

        public Area(string filePath)
        {
            this.minHeight = float.MaxValue;
            this.maxHeight = float.MinValue;
            this.filePath = filePath;
        }

        public void Create()
        {
            this.header = new Header(1634887009, 0);
            this.subChunks = new List<SubChunk>();
            int idx = 0;
            for (int i = 0; i < 16 * 16; i++)
            {
                SubChunk sub = new SubChunk(this.chunk, idx, 0);
                this.subChunks.Add(sub);
                idx++;
            }

            this.props = new List<Prop>();
            this.propLookup = new Dictionary<uint, Prop>();
            //this.curts = new Curt[0];
            ProcessForExport();
        }

        public void Read()
        {
            //try
            {
                if (this.fileEntry == null)
                {
                    if (this.filePath != null)
                    {
                        // Load project area
                        using (Stream? str = File.OpenRead(this.filePath))
                        {
                            if (str == null) return;
                            if (str.Length == 0) return;

                            using (var br = new BinaryReader(str))
                            {
                                Read(br);
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning("No File Path or Entry provided for Area.");
                    }
                }
                else
                {
                    using (Stream? str = this.gameData.GetFileData(this.fileEntry))
                    {
                        if (str == null) return;
                        if (str.Length == 0) return;

                        using (var br = new BinaryReader(str))
                        {
                            Read(br);
                        }
                    }
                }

                if (this.onFinishedReading != null)
                    this.onFinishedReading.Invoke();
            }
            //catch (Exception e)
            {
                //Debug.LogError("Exception at : " + this.chunk.gameData.archives["ClientData"].fileNames[this.fileEntry.hash] + " " + this.chunk.coords);
                //Debug.LogException(e);
            }
        }

        void Read(BinaryReader br)
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
                            this.props = new List<Prop>();
                            this.propLookup = new Dictionary<uint, Prop>();
                            for (int i = 0; i < propCount; i++)
                            {
                                this.props.Add(new Prop(br, chunkStart));
                                this.propLookup.Add(this.props[i].uniqueID, this.props[i]);
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

        public uint AddProp(string path, Vector3 position, Quaternion rotation, float scale)
        {
            if (this.props == null)
                this.props = new List<Prop>();

            if (this.propLookup == null)
                this.propLookup = new Dictionary<uint, Prop>();

            Prop p = new Prop();
            p.path = path;
            p.position = position;
            p.rotation = rotation;
            p.scale = scale;

            p.uniqueID = 55419076;

            p.color0 = -8421505;
            p.color1 = -1;
            p.color2 = -1;

            p.placement = new Placement(0, 0, 3000, 3000);

            this.props.Add(p);
            this.propLookup.Add(p.uniqueID, p);

            // TODO : Determine which chunks it fits in
            for (int i = 0; i < this.subChunks.Count; i++)
            {
                if (this.subChunks[i].propUniqueIDs == null)
                    this.subChunks[i].propUniqueIDs = new List<uint>();

                this.subChunks[i].propUniqueIDs.Add(p.uniqueID);
            }


            return p.uniqueID;
        }

        public void ProcessForExport()
        {
            foreach (var sc in subChunks)
            {
                if (sc.blendMap != null)
                {
                    if (sc.blendMap.Length == 65 * 65 * 4 && sc.flags.HasFlag(SubChunk.Flags.hasBlendMapDXT))
                    {
                        // Convert chunk to dxt
                        BcEncoder encoder = new BcEncoder();
                        encoder.OutputOptions.GenerateMipMaps = false;
                        encoder.OutputOptions.Quality = CompressionQuality.Balanced;
                        encoder.OutputOptions.Format = CompressionFormat.Bc1;
                        sc.blendMap = encoder.EncodeBytesToRawBytes(sc.blendMap, 65, 65, CompressionFormat.Rgba32, 0, out int _, out int _);
                    }
                }
            }
        }

        public void Write()
        {
            if (File.Exists(this.filePath))
                File.Delete(this.filePath);

            using(var str = File.OpenWrite(this.filePath))
            {
                using(BinaryWriter bw = new BinaryWriter(str))
                {
                    this.header.Write(bw);
                    if (this.subChunks != null && this.subChunks.Count > 0)
                    {
                        long chnkStart = bw.BaseStream.Position;
                        bw.Write((uint)1128812107); // CHNK
                        bw.Write((uint)0);          // Size pad
                        long subStart = bw.BaseStream.Position;
                        for (int i = 0; i < this.subChunks.Count; i++)
                        {
                            this.subChunks[i].Write(bw);
                        }
                        long subEnd = bw.BaseStream.Position;
                        uint chnkSize = (uint)(subEnd - subStart);
                        bw.BaseStream.Position = chnkStart + 4;
                        bw.Write(chnkSize);         // Size calculated
                        bw.BaseStream.Position = subEnd;
                    }

                    if (this.props != null && this.props.Count > 0)
                    {
                        long chnkStart = bw.BaseStream.Position;
                        bw.Write((uint)1347571568);     // PROp
                        bw.Write((uint)0);              // Size pad
                        long subStart = bw.BaseStream.Position;
                        bw.Write(this.props.Count);    // Prop count
                        Dictionary<string, uint> paths = new Dictionary<string, uint>();
                        List<uint> pathStarts = new List<uint>();
                        Dictionary<uint, string> pathLookups = new Dictionary<uint, string>();
                        uint lastNameOffset = 0;
                        int propsSize = this.props.Count * 104;
                        for (int i = 0; i < this.props.Count; i++)
                        {
                            pathStarts.Add(lastNameOffset);
                            this.props[i].Write(bw, propsSize + 4, ref paths, ref lastNameOffset);
                        }
                        foreach (var item in paths)
                        {
                            pathLookups.Add(item.Value, item.Key);
                        }
                        for (int i = 0; i < pathStarts.Count; i++)
                        {
                            bw.WriteWString(pathLookups[pathStarts[i]]);
                        }
                        long subEnd = bw.BaseStream.Position;
                        uint chnkSize = (uint)(subEnd - subStart);
                        bw.BaseStream.Position = chnkStart + 4;
                        bw.Write(chnkSize);         // Size calculated
                        bw.BaseStream.Position = subEnd;
                    }
                    // TODO : write curts
                    // .. and other stuff
                }
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
                this.chunk.AABB = new AABB(center + new Vector3(256f, 0, -256f), new Vector3(512f, dist, 512f));
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
