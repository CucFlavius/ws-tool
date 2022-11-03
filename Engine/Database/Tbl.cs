using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ProjectWS.Engine.Data.Extensions;

namespace ProjectWS.Engine.Database
{
    public class Tbl<T> where T : TblRecord, new()
    {
        public const int HEADER_SIZE = 96;

        //public EXD[] pages;
        public Dictionary<uint, T> records;
        public uint[] keys;
        public string name;
        public string path;
        public Header header;
        public long headerEndOffset;
        public string internalName;
        public Column[] columns;

        public Tbl(string name, Data.GameData data)
        {
            this.name = name;
            this.path = $"DB\\{name}.tbl";
            this.records = new Dictionary<uint, T>();
            FieldCache[] fieldCache = FieldsCache<T>.Cache;

            using (Stream ms = data.GetFileData(this.path))
            {
                using (BinaryReader br = new BinaryReader(ms))
                {
                    this.header = new Header(br);
                    this.headerEndOffset = br.BaseStream.Position;
                    this.internalName = br.ReadString((int)this.header.tableNameLength, true);
                    this.columns = new Column[this.header.fieldCount];
                    for (int i = 0; i < this.header.fieldCount; i++)
                    {
                        br.BaseStream.Position = this.header.fieldOffset + HEADER_SIZE + (24 * i);
                        this.columns[i] = new Column(br, this.header);
                        //Debug.Log(this.columns[i].name + " " + this.columns[i].dataType);
                    }
                    long offset = this.header.recordOffset + HEADER_SIZE;
                    this.keys = new uint[this.header.recordCount];
                    for (uint i = 0; i < this.header.recordCount; i++)
                    {
                        br.BaseStream.Position = offset + (this.header.recordSize * i);

                        T rec = new T();
                        rec.Read(fieldCache, rec, br, offset);
                        uint id = rec.GetID();
                        this.keys[i] = id;
                        records.Add(id, rec);
                    }
                }
            }
        }

        public struct Header
        {
            public string magic;
            public int version;
            public uint tableNameLength;
            public long unk0;
            public uint recordSize;
            public long fieldCount;
            public long fieldOffset;
            public uint recordCount;
            public long totalRecordSize;
            public long recordOffset;
            public long maxID;
            public long lookupOffset;

            public Header(BinaryReader br)
            {
                this.magic = br.ReadChunkID();
                this.version = br.ReadInt32();
                this.tableNameLength = br.ReadUInt32();
                br.BaseStream.Position += 4;            // Padding
                this.unk0 = br.ReadInt64();
                this.recordSize = br.ReadUInt32();
                br.BaseStream.Position += 4;            // Padding
                this.fieldCount = br.ReadInt64();
                this.fieldOffset = br.ReadInt64();
                this.recordCount = br.ReadUInt32();
                br.BaseStream.Position += 4;            // Padding
                this.totalRecordSize = br.ReadInt64();
                this.recordOffset = br.ReadInt64();
                this.maxID = br.ReadInt64();
                this.lookupOffset = br.ReadInt64();
                br.BaseStream.Position += 8;            // Padding
            }
        }

        public struct Column
        {
            public uint nameLength;
            public uint unk0;
            public long nameOffset;
            public DataType dataType;
            public uint unk1;
            public string name;

            public Column(BinaryReader br, Header header)
            {
                this.nameLength = br.ReadUInt32();
                this.unk0 = br.ReadUInt32();
                this.nameOffset = br.ReadInt64();
                this.dataType = (DataType)br.ReadUInt16();
                br.BaseStream.Position += 2;        // padding
                this.unk1 = br.ReadUInt32();
                long offset = (long)(HEADER_SIZE + header.fieldCount * 24 + header.fieldOffset + this.nameOffset);
                br.BaseStream.Position = ((header.fieldCount % 2L == 0) ? offset : (offset + 8));
                this.name = br.ReadString((int)(this.nameLength - 1), true);
            }

            public enum DataType
            {
                Uint = 3,
                Float = 4,
                Flags = 11,
                Ulong = 20,
                String = 130
            }
        }

        public static Tbl<T> Open(Data.GameData data)
        {
            try
            {
                T table = new T();
                string fileName = table.GetFileName();
                var r = new Tbl<T>(fileName, data);
                return r;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return null;
            }
        }

        public T Get(uint id)
        {
            this.records.TryGetValue(id, out T rec);
            return rec;
        }

        public void Print()
        {
            /*
            foreach (KeyValuePair<uint, T> item in this.records)
            {
                Debug.Log(JsonConvert.SerializeObject(item.Value, Formatting.Indented));
            }
            */
        }
    }
}