using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ProjectWS.Engine.Data
{
    public partial class Tex
    {
        string filePath;
        Data.GameData gameData;
        public bool failedReading;
        public DataSource source;

        public Header header;
        //public byte[] rawData;
        public List<byte[]> mipData;

        public Tex(string filePath, Data.GameData gameData)
        {
            this.filePath = filePath;
            this.gameData = gameData;
            this.failedReading = false;
            if (this.gameData == null)
                this.source = DataSource.EngineResource;
            else
                this.source = DataSource.GameData;
        }

        public Tex(string filePath)
        {
            this.filePath = filePath;
            this.failedReading = false;
            this.source = DataSource.Extracted;
        }

        public void Read()
        {
            if (this.source == DataSource.GameData)
            {
                using (MemoryStream str = this.gameData.GetFileData(this.filePath))
                {
                    if (str == null)
                    {
                        this.failedReading = true;
                        return;
                    }

                    Read(str);
                }
            }
            else if (this.source == DataSource.Extracted)
            {
                using(Stream str = File.OpenRead(this.filePath))
                {
                    if (str == null)
                    {
                        this.failedReading = true;
                        return;
                    }

                    Read(str);
                }
            }
            else if (this.source == DataSource.EngineResource)
            {
                Image<Rgba32> image = Image.Load<Rgba32>(this.filePath);

                //ImageSharp loads from the top-left pixel, whereas OpenGL loads from the bottom-left, causing the texture to be flipped vertically.
                //This will correct that, making the texture display properly.
                image.Mutate(x => x.Flip(FlipMode.Vertical));

                //Convert ImageSharp's format into a byte array, so we can use it with OpenGL.
                var pixels = new List<byte>(4 * image.Width * image.Height);

                image.ProcessPixelRows(pixelAccessor =>
                {
                    for (int y = 0; y < pixelAccessor.Height; y++)
                    {
                        Span<Rgba32> row = pixelAccessor.GetRowSpan(y);

                        for (int x = 0; x < row.Length; x++)
                        {
                            pixels.Add(row[x].R);
                            pixels.Add(row[x].G);
                            pixels.Add(row[x].B);
                            pixels.Add(row[x].A);
                        }
                    }
                });

                this.mipData = new List<byte[]>() { pixels.ToArray() };
                this.header = new Header(image.Width, image.Height, 1);
            }
        }

        public void Read(Stream str)
        {
            try
            {
                if (str == null)
                {
                    this.failedReading = true;
                    return;
                }
                    
                using(BinaryReader br = new BinaryReader(str))
                {
                    this.header = new Header(br);
                    if (this.header.version > 0)
                        br.BaseStream.Position = 112;

                    this.mipData = new List<byte[]>();

                    switch (this.header.textureType)
                    {
                        case Header.TextureType.Jpeg1:
                        case Header.TextureType.Jpeg2:
                        case Header.TextureType.Jpeg3:
                            {
                                byte[][] jpegMips = new byte[this.header.imageSizesCount][];
                                byte[][] rgbaMips = new byte[this.header.imageSizesCount][];
                                Jpeg.Decoder lt = new Jpeg.Decoder();
                                for (int i = 0; i < this.header.imageSizesCount; i++)
                                {
                                    jpegMips[i] = br.ReadBytes((int)this.header.imageSizes[i]);
                                    rgbaMips[i] = lt.DecompressMip(this.header, ((int)this.header.imageSizesCount - 1) - i, jpegMips[i]);
                                    this.mipData.Add(rgbaMips[i]);
                                }
                            }
                            break;
                        case Header.TextureType.Argb1:
                        case Header.TextureType.Argb2:
                            {
                                int remainingSize = (int)(br.BaseStream.Length - br.BaseStream.Position);
                                var rawData = br.ReadBytes(remainingSize);

                                int offs = 0;
                                for (int i = 0; i < this.header.imageSizesCount; i++)
                                {
                                    int size = (int)this.header.imageSizes[i];
                                    byte[] rgbaMip = new byte[size];
                                    Buffer.BlockCopy(rawData, offs, rgbaMip, 0, size);
                                    this.mipData.Add(rgbaMip);
                                    offs += size;
                                }
                            }
                            break;
                        case Header.TextureType.Rgb:
                            break;
                        case Header.TextureType.Grayscale:
                            break;
                        case Header.TextureType.DXT1:
                            {
                                int[] DXTSizes = CalculateDXTSizes(this.header.mipCount, this.header.width, this.header.height, 8);
                                // skip to mip0
                                //int distance = 0;
                                for (int d = 0; d < this.header.mipCount; d++)
                                {
                                    //distance += DXTSizes[d];
                                    byte[] buffer = br.ReadBytes(DXTSizes[d]);
                                    this.mipData.Add(buffer);
                                }
                                //br.BaseStream.Seek(distance, SeekOrigin.Current);
                                //int remainingSize = (int)(br.BaseStream.Length - br.BaseStream.Position);
                                //this.rawData = br.ReadBytes(remainingSize);
                            }
                            break;
                        case Header.TextureType.DXT3:
                            break;
                        case Header.TextureType.DXT5:
                            {
                                int[] DXTSizes = CalculateDXTSizes(this.header.mipCount, this.header.width, this.header.height, 16);
                                //this.rawData = new byte[br.BaseStream.Length - br.BaseStream.Position];

                                //int reverseOffset = 0;
                                for (int d = 0; d < this.header.mipCount; d++)
                                {
                                    byte[] buffer = br.ReadBytes(DXTSizes[d]);
                                    this.mipData.Add(buffer);
                                    //reverseOffset += buffer.Length;
                                    //System.Buffer.BlockCopy(buffer, 0, this.rawData, this.rawData.Length - reverseOffset, buffer.Length);
                                }
                            }
                            break;
                        case Header.TextureType.Unknown:
                        default:
                            Debug.LogWarning("Unsupported texture type.");
                            return;
                    }
                }
            }
            catch(Exception e)
            {
                Debug.LogException(e);
            }
        }

        public void Write(string filePath)
        {
            if (this.source == DataSource.EngineResource)
            {
                return;
            }

            string fileName = Path.GetFileName(filePath);
            string directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            // Change to ARGB
            this.header.isCompressed = 0;
            this.header.format = 0;
            var currentTextureType = this.header.textureType;

            using(var str = File.OpenWrite(filePath))
            {
                using(var bw = new BinaryWriter(str))
                {
                    this.header.Write(bw, this.mipData);

                    if (this.header.version > 0)
                        bw.BaseStream.Position = 112;

                    switch (currentTextureType)
                    {
                        case Header.TextureType.Jpeg1:
                        case Header.TextureType.Jpeg2:
                        case Header.TextureType.Jpeg3:
                            {
                                for (int i = 0; i < this.mipData.Count; i++)
                                {
                                    bw.Write(this.mipData[i]);
                                }
                            }
                            break;
                        case Header.TextureType.Argb1:
                        case Header.TextureType.Argb2:
                            {
                                // rawdata
                            }
                            break;
                        case Header.TextureType.Rgb:
                            break;
                        case Header.TextureType.Grayscale:
                            break;
                        case Header.TextureType.DXT1:
                            {
                                // rawdata
                            }
                            break;
                        case Header.TextureType.DXT3:
                            break;
                        case Header.TextureType.DXT5:
                            {
                                // rawdata
                            }
                            break;
                        case Header.TextureType.Unknown:
                        default:
                            Debug.LogWarning("Unsupported texture type.");
                            return;
                    }
                }
            }
        }
        /*
        public System.Drawing.Bitmap GetBitmap()
        {
            RGBA2ARGB();

            byte[] pic = this.rawData;

            if (this.failedReading || this.rawData == null) return null;

            var bmp = new System.Drawing.Bitmap(header.width, header.height);

            // Faster bitmap Data copy
            var bmpdata = bmp.LockBits(new System.Drawing.Rectangle(0, 0, header.width, header.height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Marshal.Copy(pic, 0, bmpdata.Scan0, header.width * header.height * 4);
            bmp.UnlockBits(bmpdata);

            return bmp;
        }
        */
        /*
        void RGBA2ARGB()
        {
            if (this.rawData == null)
            {
                Debug.LogError("Tex.cs : RGBA2ARGB() rawData was null!");
                return;
            }
            for (int i = 0; i < this.rawData.Length; i += 4)
            {
                byte R = this.rawData[i];
                byte G = this.rawData[i + 1];
                byte B = this.rawData[i + 2];
                byte A = this.rawData[i + 3];

                this.rawData[i] = B;
                this.rawData[i + 1] = G;
                this.rawData[i + 2] = R;
                this.rawData[i + 3] = A;
            }
        }
        */
        int[] CalculateDXTSizes(int miplevels, int width, int height, int blockSize)
        {
            int[] DXTSizes = new int[miplevels];
            int increment = 0;
            for (int m = miplevels - 1; m >= 0; m--)
            {
                int w = (int)(width / Math.Pow(2, m));
                int h = (int)(height / Math.Pow(2, m));
                DXTSizes[increment] = (int)(((w + 3) / 4) * ((h + 3) / 4) * blockSize);
                increment++;
            }
            return DXTSizes;
        }
    }
}