using ProjectWS.Engine.Data.Extensions;
using System;
using System.Collections.Generic;
using System.IO;

namespace ProjectWS.Engine.Data
{
    public partial class Sky
    {
        public string filePath;
        public GameData gameData;
        public bool failedReading;
        public DataSource source;

        public int headerSize = 1216;

        public uint version;
        public int unk0;            // 0,1
        public int unk1;            // 0,1
        public float unk2;
        public string sourceFilePath;

        public Sky(string filePath, Data.GameData gameData)
        {
            this.filePath = filePath;
            this.gameData = gameData;
            this.failedReading = false;
            this.source = DataSource.GameData;
        }

        public Sky(string filePath)
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
                    Read(str);
                }
            }
            else if (this.source == DataSource.Extracted)
            {
                using (Stream str = File.OpenRead(this.filePath))
                {
                    Read(str);
                }
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

                using (BinaryReader br = new BinaryReader(str))
                {
                    int magic = br.ReadInt32();
                    if (magic != 0x58534B59)    // XSKY
                    {
                        this.failedReading = true;
                        Debug.LogWarning($"This is not an SKY file, 0x{magic:X} != XSKY, {this.filePath}");
                        return;
                    }

                    this.version = br.ReadUInt32();
                    this.unk0 = br.ReadInt32();
                    this.unk1 = br.ReadInt32();
                    this.unk2 = br.ReadSingle();
                    br.BaseStream.Position += 4;    // Padding
                    this.sourceFilePath = new string(new ArrayWChar(br, headerSize).data);
                    //Debug.Log(this.sourceFilePath);
                }
            }
            catch (Exception e)
            {
                this.failedReading = true;
                Debug.LogError($"SKY : Failed Reading File {this.filePath}");
                Debug.LogException(e);
            }
        }
    }
}