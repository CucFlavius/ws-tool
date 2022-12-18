using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ProjectWS.FileFormats.Tex
{
    public class File
    {
        string filePath;
        public bool failedReading;

        public Header? header;
        public List<byte[]>? mipData;

        public File(string filePath)
        {
            this.filePath = filePath;
            this.failedReading = false;
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
                        case TextureType.Jpeg1:
                        case TextureType.Jpeg2:
                        case TextureType.Jpeg3:
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
                        case TextureType.Argb1:
                        case TextureType.Argb2:
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
                        case TextureType.Rgb:
                            break;
                        case TextureType.Grayscale:
                            break;
                        case TextureType.DXT1:
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
                        case TextureType.DXT3:
                            break;
                        case TextureType.DXT5:
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
                        case TextureType.Unknown:
                        default:
                            Console.WriteLine("Unsupported texture type.");
                            return;
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void Write(string filePath)
        {
            if (this.header == null)
                return;

            if (this.filePath == null)
                return;

            if (this.mipData == null)
                return;

            string fileName = Path.GetFileName(filePath);
            string directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            // Change to ARGB
            this.header.isCompressed = 0;
            this.header.format = 0;
            var currentTextureType = this.header.textureType;

            using(var str = System.IO.File.OpenWrite(filePath))
            {
                using(var bw = new BinaryWriter(str))
                {
                    this.header.Write(bw, this.mipData);

                    if (this.header.version > 0)
                        bw.BaseStream.Position = 112;

                    switch (currentTextureType)
                    {
                        case TextureType.Jpeg1:
                        case TextureType.Jpeg2:
                        case TextureType.Jpeg3:
                            {
                                for (int i = 0; i < this.mipData.Count; i++)
                                {
                                    bw.Write(this.mipData[i]);
                                }
                            }
                            break;
                        case TextureType.Argb1:
                        case TextureType.Argb2:
                            {
                                // rawdata
                            }
                            break;
                        case TextureType.Rgb:
                            break;
                        case TextureType.Grayscale:
                            break;
                        case TextureType.DXT1:
                            {
                                // rawdata
                            }
                            break;
                        case TextureType.DXT3:
                            break;
                        case TextureType.DXT5:
                            {
                                // rawdata
                            }
                            break;
                        case TextureType.Unknown:
                        default:
                            Console.WriteLine("Unsupported texture type.");
                            return;
                    }
                }
            }
        }

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