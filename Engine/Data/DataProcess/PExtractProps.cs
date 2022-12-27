namespace ProjectWS.Engine.Data.DataProcess
{
    internal class PExtractProps : Process
    {
        int count = 0;
        const string TEX = ".tex";

        public PExtractProps(SharedProcessData spd) : base(spd)
        {

        }

        public override void Run()
        {
            LogProgress(0, Info());

            if (this.sharedProcessData?.gameData != null && this.sharedProcessData?.assetDBFolder != null)
            {
                var dir = $"{Data.Archive.rootBlockName}\\Art\\Character\\Chua";
                Dictionary<string, Block.FileEntry> propFiles = new Dictionary<string, Block.FileEntry>();
                PropScan(dir, this.sharedProcessData.assetDBFolder, ref propFiles);

                count = propFiles.Count;

                int idx = 0;
                Parallel.ForEach(propFiles, new ParallelOptions() { MaxDegreeOfParallelism = 10 }, entry =>
                {
                    bool copy = true;
                    if (Path.GetExtension(entry.Key).ToLower() == TEX)
                    {
                        using (var ms = this.sharedProcessData.gameData.GetFileData(entry.Value))
                        {
                            if (ms != null)
                            {
                                FileFormats.Tex.File tex = new FileFormats.Tex.File("");
                                tex.Read(ms);

                                if (tex.header?.format == 0 && tex.header?.isCompressed == 1) // if jpeg
                                {
                                    Console.WriteLine("Converting to DXT : " + entry.Key);
                                    tex.ConvertMipDataToDXT();
                                    tex.Write(entry.Key);
                                    LogProgress(idx++);
                                    copy = false;
                                }
                            }
                        }
                    }
                    /*
                    if (copy)
                    {
                        using (var fs = new System.IO.FileStream(entry.Key, FileMode.Create, System.IO.FileAccess.Write))
                        using (var ms = this.sharedProcessData.gameData.GetFileData(entry.Value))
                        {
                            ms?.WriteTo(fs);
                            LogProgress(idx++);
                        }
                    }
                    */
                });
            }
        }

        private void PropScan(string gameDir, string assetDir, ref Dictionary<string, Block.FileEntry> propFiles)
        {
            if (this.sharedProcessData?.gameData == null) return;

            var folderList = this.sharedProcessData.gameData.GetFolderList(gameDir);
            var fileEntries = this.sharedProcessData.gameData.GetFileEntries(gameDir);
            var gameDirNoRoot = gameDir.Substring(gameDir.IndexOf('\\'));
            var realDir = $"{assetDir}\\{gameDirNoRoot}";

            if (!Directory.Exists(realDir))
                Directory.CreateDirectory(realDir);

            foreach (var entry in fileEntries)
            {
                var realFilePath = Path.Combine(realDir, entry.Key);
                propFiles.Add(realFilePath, entry.Value);
            }

            foreach (var folder in folderList)
            {
                PropScan($"{gameDir}\\{folder}", assetDir, ref propFiles);
            }
        }

        public override string Info()
        {
            return "Extracting Props.";
        }

        public override int Total()
        {
            return count;
        }
    }
}
