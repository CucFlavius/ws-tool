using ProjectWS.Engine.Data.DataProcess;
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace ProjectWS.Engine.Data
{
    public static class DataManager
    {
        public static Engine? engineRef;
        public static AssetDatabase? assetDB;
        public const string ASSETDB_FILENAME = "AssetDatabase.json";

        public const int ASSETDB_VERSION = 1;

        public static Thread? thread;
        public static Action<float>? logProgress;
        public static Action<string?>? logProgressText;

        public static bool assetDBReady;


        public static void LoadAssetDatabase(string path)
        {
            if (File.Exists(path))
            {
                string? jsonString = File.ReadAllText(path);
                if (jsonString != null && jsonString != String.Empty)
                {
                    assetDB = JsonSerializer.Deserialize<AssetDatabase>(jsonString);
                }
            }

            if (assetDB.database != AssetDatabase.DataStatus.Ready ||
                assetDB.gameArt != AssetDatabase.DataStatus.Ready)
            {
                assetDBReady = false;
                Debug.LogWarning("Asset Database only partially created, go to Data Manager and click Build to continue the process.");
            }
            else
            {
                assetDBReady = true;
            }
        }

        public static void CreateAssetDB(string path, Action<float>? _logProgress = null, Action<string?>? _logProgressText = null)
        {
            if (File.Exists(path))
            {
                string? jsonString = File.ReadAllText(path);
                if (jsonString != null && jsonString != String.Empty)
                {
                    assetDB = JsonSerializer.Deserialize<AssetDatabase>(jsonString);
                }
            }
            else
            {
                assetDB = new AssetDatabase();
                assetDB.version = DataManager.ASSETDB_VERSION;
            }

            logProgress = _logProgress;
            logProgressText = _logProgressText;

            SaveAssetDB(path);

            SharedProcessData spd = new SharedProcessData();
            spd.engineRef = engineRef;
            spd.gameClientPath = Engine.settings.dataManager?.gameClientPath;
            spd.assetDBFolder = Path.GetDirectoryName(Engine.settings.dataManager?.assetDatabasePath);
            spd.assetDB = assetDB;

            BackgroundWorker worker = new BackgroundWorker();
            spd.workerRef = worker;
            worker.WorkerReportsProgress = true;
            worker.ProgressChanged += new ProgressChangedEventHandler(
            (sender, e) =>
            {
                logProgress?.Invoke(e.ProgressPercentage);
                if (e.UserState != null)
                    logProgressText?.Invoke(e.UserState as String);
            });
            worker.DoWork +=
            (s3, e3) =>
            {
                new PLoadGameData(spd).Run();

                if (assetDB?.database != AssetDatabase.DataStatus.Ready)
                {
                    assetDB.database = AssetDatabase.DataStatus.InProgress;
                    SaveAssetDB(path);
                    new PExtractDatabase(spd).Run();
                    assetDB.database = AssetDatabase.DataStatus.Ready;
                    SaveAssetDB(path);
                }
                if (assetDB?.gameArt != AssetDatabase.DataStatus.Ready)
                {
                    assetDB.gameArt = AssetDatabase.DataStatus.InProgress;
                    SaveAssetDB(path);
                    new PExtractArt(spd).Run();
                    assetDB.gameArt = AssetDatabase.DataStatus.Ready;
                    SaveAssetDB(path);
                }

                new PEnd(spd).Run();
            };

            worker.RunWorkerAsync();
        }

        static void SaveAssetDB(string path)
        {
            if (path != null)
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string data = JsonSerializer.Serialize(assetDB, options);
                File.WriteAllText(path, data);
            }
        }
    }
}
