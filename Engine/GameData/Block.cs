using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace ProjectWS.Engine.Data
{
    public class Block
    {
        public uint directoryCount;
        public uint fileCount;
        public DirectoryEntry[] directoryEntries;
        public FileEntry[] fileEntries;
        public char[] nameData;

        public Block(BinaryReader br, uint blockIndex, string breadCrumb, Archive archive)
        {
            br.BaseStream.Position = (long)archive.index.blockHeaders[blockIndex].blockOffset;

            this.directoryCount = br.ReadUInt32();
            this.fileCount = br.ReadUInt32();

            if (this.directoryCount > 0)
            {
                this.directoryEntries = new DirectoryEntry[this.directoryCount];
                for (int i = 0; i < this.directoryCount; i++)
                {
                    this.directoryEntries[i] = new DirectoryEntry(br);
                }
            }

            if (this.fileCount > 0)
            {
                this.fileEntries = new FileEntry[this.fileCount];
                for (int i = 0; i < this.fileCount; i++)
                {
                    this.fileEntries[i] = new FileEntry(br);
                }
            }

            long remainingSize = (long)archive.index.blockHeaders[blockIndex].blockSize - (br.BaseStream.Position - (long)archive.index.blockHeaders[blockIndex].blockOffset);
            this.nameData = br.ReadChars((int)remainingSize);

            if (this.directoryEntries != null)
            {
                foreach (var directoryEntry in this.directoryEntries)
                {
                    string word = GetWord(directoryEntry.nameOffset);
                    directoryEntry.name = word;
                    string newPath = $"{breadCrumb}\\{word}";
                    Block block = new Block(br, directoryEntry.nextBlock, newPath, archive);
                    archive.blockTree.Add(newPath, block);
                }
            }

            if (this.fileEntries != null)
            {
                foreach (var fileEntry in this.fileEntries)
                {
                    byte[] hash = fileEntry.hash;
                    string word = GetWord(fileEntry.nameOffset);
                    fileEntry.name = word;
                    if (!archive.fileNames.ContainsKey(hash))
                    {
                        archive.fileNames.Add(hash, word);
                    }
                    archive.fileList.Add($"{breadCrumb}\\{word}".ToLower(), fileEntry);
                }
            }
        }

        string GetWord(uint offset)
        {
            string word = "";
            int increment = 0;
            for (int t = 0; t < 200; t++)
            {
                char c = this.nameData[offset + increment];
                increment++;
                if (c != '\0')
                    word += c;
                else
                    break;
            }
            return word;
        }

        public class DirectoryEntry
        {
            public uint nameOffset;
            public uint nextBlock;
            public string name;

            public DirectoryEntry(BinaryReader br)
            {
                this.nameOffset = br.ReadUInt32();
                this.nextBlock = br.ReadUInt32();
            }
        }

        public class FileEntry
        {
            public uint nameOffset;
            public Compression compression;
            public ulong writeTime; // uint64 // FILETIME
            public ulong uncompressedSize;
            public ulong compressedSize;
            public byte[] hash;
            public uint unk2;
            public string name;

            public FileEntry(BinaryReader br)
            {
                this.nameOffset = br.ReadUInt32();
                this.compression = (Compression)br.ReadUInt32();
                this.writeTime = br.ReadUInt64();
                this.uncompressedSize = br.ReadUInt64();
                this.compressedSize = br.ReadUInt64();
                this.hash = br.ReadBytes(20);
                this.unk2 = br.ReadUInt32();
            }

            public enum Compression
            {
                Uncompressed = 1,
                ZLib = 3,
                LZMA = 5,
            }
        }
    }
}