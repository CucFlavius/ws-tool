using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace ProjectWS.Engine.World
{
    public class World
    {
        #region Variables

        public const int WORLD_SIZE = 128;
        public const float AREA_SIZE = 512.0f;
        public const int DRAWDIST0 = 2;
        public const int DRAWDIST1 = 2;
        public const int DRAWDIST2 = 2;

        // Refs
        public Engine engine;
        Rendering.WorldRenderer renderer;

        // Data
        Data.GameData gameData;
        Database.Tables database;

        // Map
        uint worldUUID;
        uint loadedMapID;
        uint loadedWorldID;
        public Dictionary<Vector2, Chunk> chunks;
        public Dictionary<Vector2, Chunk> activeChunks;
        Dictionary<int, Chunk> areaIDtoChunk;
        List<Data.Submesh> distanceSortedSubchunksBuffer;
        ChunkDistanceComparer chunkComparer;

        // Prop
        List<string> loadedProps;
        //Dictionary<string, Prop> props;
        HashSet<uint> culledUUIDs;

        // Controller
        Controller controller;

        // Culling
        Thread cullingThread;
        bool cullingThreadRunning;
        System.Diagnostics.Stopwatch cullingStopwatch;
        float cullingFrametime;
        volatile bool cullTaskProcess;
        volatile bool cullTaskUpdate;

        #endregion

        public World(Engine engine, uint worldUUID = 0, bool setActive = false)
        {
            this.chunks = new Dictionary<Vector2, Chunk>();
            this.activeChunks = new Dictionary<Vector2, Chunk>();
            this.areaIDtoChunk = new Dictionary<int, Chunk>();
            this.distanceSortedSubchunksBuffer = new List<Data.Submesh>();
            this.chunkComparer = new ChunkDistanceComparer();
            //this.loadedProps = new Dictionary<string, Prop>();
            this.controller = new Controller(this);
            this.cullingStopwatch = new System.Diagnostics.Stopwatch();
            this.culledUUIDs = new HashSet<uint>();

            this.engine = engine;
            this.gameData = engine.data;
            this.database = this.gameData.database;
            this.worldUUID = worldUUID;
            this.loadedMapID = 0;

            if (engine.worlds.ContainsKey(worldUUID))
                this.worldUUID = (uint)Utilities.Random();
            engine.worlds.Add(this.worldUUID, this);
            if (setActive)
            {
                if (FindRenderer())
                {
                    Debug.Log("Set World");
                    this.renderer.SetWorld(this);
                }
            }
        }

        public void TeleportToWorldLocation(uint ID)
        {
            var record = this.database.worldLocation.Get(ID);

            if (record == null)
            {
                Debug.Log($"WorldLocation database doesn't contain record {ID}");
                return;
            }

            Teleport(record.position0, record.position1, record.position2, record.worldId);
        }

        public void Teleport(float x, float y, float z, uint worldID)
        {
            Debug.Log($"Teleport : {x}, {y}, {z}, {worldID}");
            // Move camera to spatial position first
            float cameraUpOffset = 8f;      // Offseting camera from player so it doesn't sit on the ground
            float cameraBehindCharacterOffset = 9.5f;

            this.controller.worldPosition = new Vector3(x, y + cameraUpOffset, z - cameraBehindCharacterOffset);

            var mainCam = this.renderer.viewports[0].mainCamera;
            var camController = mainCam.components[0] as Components.CameraController;
            mainCam.transform.SetPosition(this.controller.worldPosition);
            camController.Teleport(this.controller.worldPosition.X, this.controller.worldPosition.Y, this.controller.worldPosition.Z);

            // Load world around spawn position
            LoadWorld(worldID);
        }

        public void LoadWorld(uint ID)
        {
            var record = this.database.world.Get(ID);

            if (record == null)
            {
                Debug.Log($"World database doesn't contain record {ID}");
                return;
            }

            if (this.loadedWorldID != 0)
            {
                // A map is already loaded or loading, dispose of it
            }

            this.loadedWorldID = ID;


            List<Vector2> availableChunks = CreateChunks(record.assetPath);

            if (availableChunks == null)
            {
                Debug.LogWarning("Couldn't find any chunks");
                return;
            }

            if (availableChunks.Count == 0)
            {
                Debug.LogWarning("Couldn't find any chunks");
                return;
            }

            this.controller.spawn = true;
            this.controller.chunkPosition = Utilities.WorldToChunkCoords(this.controller.worldPosition);
            Debug.Log($"Start: chunk {this.controller.chunkPosition} | world {this.controller.worldPosition}");
        }

        // remove
        /*
        public void LoadMap(uint ID, Vector2 spawnPoint)
        {
            if (this.loadedMapID != 0)
            {
                // A map is already loaded or loading, dispose of it
            }

            // Load new Map
            this.loadedMapID = ID;
            Database.Definitions.MapContinent mapContinentRecord = this.database.mapContinent.Get(ID);

            Debug.Log($"Loading Map Continent {ID} at asset path {mapContinentRecord.assetPath}");

            if (mapContinentRecord == null)
            {
                Debug.LogWarning("MapContinent Record doesn't exist for ID " + ID);
                return;
            }

            List<Vector2> availableChunks = CreateChunks(mapContinentRecord.assetPath);

            if (availableChunks == null)
            {
                Debug.LogWarning("Couldn't find any chunks");
                return;
            }

            if (availableChunks.Count == 0)
            {
                Debug.LogWarning("Couldn't find any chunks");
                return;
            }

            this.controller.spawn = true;
            this.controller.camChunkPosition = spawnPoint;
            this.renderer.activeCamera.transform.SetPosition(Utilities.ChunkToWorldCoords(spawnPoint) + (Vector3.UnitY * 10f));
        }
        */

        public void LoadProp(Data.M3 data, uint uuid, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            /*
            if (this.props.ContainsKey(data.filePath))
            {
                this.props[data.filePath].AddInstance(uuid, position, rotation, scale);
            }
            else
            {
                this.loadedProps.Add(data.filePath);
                this.props.Add(data.filePath, new Prop(uuid, data, position, rotation, scale)); NYI
            }
            */
        }

        public void Update(float deltaTime)
        {
            // Visibility / Loading / Culling
            if (this.controller != null && this.renderer != null && this.renderer.viewports != null)
            {
                for (int v = 0; v < this.renderer.viewports.Count; v++)
                {
                    this.controller.Update(this.renderer.viewports[v].mainCamera);
                }
            }

            CullingMT();
            TasksUpdate();
        }

        public void Render(Shader shader)
        {
            if (this.activeChunks == null) return;

            GL.Disable(EnableCap.Blend);
            GL.Enable(EnableCap.DepthTest);

            foreach (KeyValuePair<Vector2, Chunk> chunk in this.activeChunks)
            {
                chunk.Value.Render(shader);
            }
        }

        void TasksUpdate()
        {
            if (this.activeChunks == null) return;

            foreach (var item in this.activeChunks)
            {
                Chunk chunk = item.Value;
                Vector2 coord = item.Key;

                // Terrain Tasks //
                if (chunk.terrainTasks.Count > 0 && this.engine.taskManager.terrainThread.tasks.Count < 4)
                {
                    if (chunk.terrainTasks.TryDequeue(out TaskManager.TerrainTask task))
                    {
                        this.engine.taskManager.terrainThread.tasks.Enqueue(task);
                    }
                }

                // Build Object Tasks //
                if (chunk.buildTasks.Count > 0 && this.engine.taskManager.buildTasks.Count < 4)
                {
                    if (chunk.buildTasks.TryDequeue(out TaskManager.TerrainTask task))
                        this.engine.taskManager.buildTasks.Enqueue(task);
                }

                /*
                if (chunk.refreshObjectTasks.Count > 0)
                {
                    int count = chunk.refreshObjectTasks.Count;
                    for (int i = 0; i < count; i++)
                    {
                        if (chunk.refreshObjectTasks.TryDequeue(out TaskHandler.Task task))
                        {
                            chunk.lod1.RefreshMesh();
                        }
                    }
                }
                */
            }
        }

        bool FindRenderer()
        {
            if (this.renderer != null) return true;

            // Find first available world renderer and set world
            // Should only be 1 world renderer
            for (int i = 0; i < engine.renderers.Count; i++)
            {
                if (engine.renderers[i] is Rendering.WorldRenderer)
                {
                    this.renderer = engine.renderers[i] as Rendering.WorldRenderer;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Main Thread Culling
        /// </summary>
        void CullingMT()
        {
            if (this.loadedWorldID != 0 || this.loadedProps.Count > 0)
            {
                // Boot CullingThread
                if (!this.cullingThreadRunning)
                {
                    this.cullingThreadRunning = true;

                    // Boot the thread //
                    this.cullingThread = new Thread(() => CullingST())
                    {
                        Name = "WRenderer.CullingThread",
                        IsBackground = true,
                        Priority = System.Threading.ThreadPriority.AboveNormal
                    };
                    this.cullingThread.Start();

                    this.cullTaskProcess = true;
                }
            }

            // Update Culling and cycle cullingUpdateQueue->cullingProcessQueue
            if (this.cullTaskUpdate)
            {
                this.cullTaskUpdate = false;

                int visibleProps = 0;
                // Update prop culling
                culledUUIDs.Clear();
                /*
                for (int i = 0; i < this.loadedProps.Count; i++)
                {
                    if (this.props.TryGetValue(this.loadedProps[i], out Prop prop))
                    {
                        prop.renderableInstances.Clear();
                        prop.renderableUUIDs.Clear();
                        for (int k = 0; k < prop.uniqueInstanceIDs.Count; k++)
                        {
                            uint uuid = prop.uniqueInstanceIDs[k];
                            if (prop.cullingResults[prop.instances[uuid].uuid])
                            {
                                prop.renderableUUIDs.Add(prop.instances[uuid].uuid);
                                prop.renderableInstances.Add(prop.instances[uuid].transform);
                                visibleProps++;

                                //if (Settings.data.debug.toggleDoodadBoundingBoxGizmos || Settings.data.debug.toggleDoodadScreenSpaceBoundsGizmos)
                                {
                                    // Get Vector (Distance essentially to use in Dot Product).
                                //    Vector3 heading = prop.instances[j].transform.ExtractPosition() - this.activeCamera.position;
                                    // Check is object is behind camera.
                                //    bool behindCamera = (Vector3.Dot(this.activeCamera.viewVector, heading) > 0) ? true : false;

                                    //if (!behindCamera)
                                //    if (prop.batches != null && !prop.culled)
                                //    {
                                        //if (Settings.data.debug.toggleDoodadBoundingBoxGizmos)
                                //            prop.boundingBox.RenderBounds(prop.instances[j].transform, new Color(0.8f, 0.8f, 1, 0.5f));
                                        //if (Settings.data.debug.toggleDoodadScreenSpaceBoundsGizmos)
                                //            prop.boundingBox.RenderScreenBounds(prop.instances[j].transform, new Color(0.6f, 0.6f, 1, 0.1f), this.activeCamera.viewMatrix, this.activeCamera.projectionMatrix);
                                //    }
                                //}

                                if (prop.boundingBox.RayBoxIntersect(this.mouseRay, prop.instances[uuid].transform) > 0)
                                {
                                    prop.boundingBox.RenderBounds(prop.instances[uuid].transform, new Color(0.8f, 0.8f, 1, 0.5f));
                                    Vector3 pos = prop.instances[uuid].transform.ExtractPosition();
                                    Vector3 rot = prop.instances[uuid].transform.ExtractRotation().eulerAngles;
                                    Vector3 sc = prop.instances[uuid].transform.ExtractScale();
                                    //string label = $"{prop.data.filePath}\nInstance {j} / {prop.instances.Count}\nP:{pos}\nR:{rot}\nS:{sc}";

                                    foreach (var item in this.activeChunks)
                                    {
                                        if (item.Value.area != null)
                                        {
                                            if (item.Value.area.uuidPropMap != null)
                                            {
                                                if (item.Value.area.uuidPropMap.TryGetValue(uuid, out Data.Area.Prop propData))
                                                {
                                                    string label = $"{prop.data.filePath}\nUUID {uuid}\nBatches {prop.batches.Length}";
                                                    IMDraw.LabelShadowed(prop.instances[uuid].transform.ExtractPosition(), Color.white, LabelPivot.MIDDLE_CENTER, LabelAlignment.CENTER, label);
                                                }
                                            }
                                        }
                                    }
                                }

                            }
                        }

                        prop.culled = prop.renderableInstances.Count == 0;

                        prop.block.Clear();

                        if (!prop.culled)
                        {
                            //prop.block.SetVectorArray("ambientColorB", prop.renderableLightingAmbientColors);
                            //prop.block.SetVectorArray("sunColorB", prop.renderableLightingSunColors);
                            //prop.block.SetVectorArray("sunVectorB", prop.renderableLightingSunVectors);
                        }
                        else
                        {
                            // TODO : if it's culled (aka all instances in the world are invisible) then submit a pause in animation thread
                        }
                    }
                }
                */
                // Cycle
                this.cullTaskProcess = true;
            }
        }

        /// <summary>
        /// Secondary Thread Culling
        /// </summary>
        public void CullingST()
        {
            while (this.cullingThreadRunning)
            {
                if (this.cullTaskProcess)
                {
                    this.cullTaskProcess = false;

                    this.cullingStopwatch.Start();

                    bool processed = false;
                    try
                    {
                        // Chunks //
                        ChunksCulling();

                        // Props //
                        //PropCulling();
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogException(e);
                    }

                    processed = true;
                    this.cullTaskUpdate = true;

                    this.cullingStopwatch.Stop();
                    if (processed)
                        this.cullingFrametime += ((this.cullingStopwatch.ElapsedMilliseconds / 1000f) - this.cullingFrametime) * 0.03f;
                    this.cullingStopwatch.Restart();
                }
            }
        }

        public void ChunksCulling()
        {
            // Chunks - Frustum and Distance //

            if (this.renderer == null) return;
            if (this.renderer.viewports == null) return;

            foreach (var item in this.activeChunks)
            {
                item.Value.CalculateCulling(this.renderer.viewports[0].mainCamera/*, this.sunLight*/);
            }

            // Chunks - Occlusion //
            //ChunksOcclusionCulling();

            // Set the visibility bools for subchunks //
            foreach (var item in this.activeChunks)
            {
                if (item.Value.isVisible)
                {
                    if (item.Value.lod0Available)
                    {
                        for (int i = 0; i < item.Value.area.subChunks.Count; i++)
                        {
                            item.Value.area.subChunks[i].isVisible = !item.Value.area.subChunks[i].isCulled && !item.Value.area.subChunks[i].isOccluded;
                        }
                    }
                }
            }
        }
        /*
        public void ChunksOcclusionCulling()
        {
            // TODO : This is very slow, lags the culling thread far behind rendering
            this.distanceSortedSubchunksBuffer.Clear();
            foreach (var item in this.activeChunks)
            {
                if (item.Value.isVisible)
                {
                    if (item.Value.lod0Available)
                    {
                        for (int i = 0; i < item.Value.area.subChunks.Count; i++)
                        {
                            if (!item.Value.area.subChunks[i].isCulled)
                            {
                                this.distanceSortedSubchunksBuffer.Add(item.Value.area.subChunks[i]);
                            }
                        }
                    }
                }
            }
            this.distanceSortedSubchunksBuffer.Sort(this.chunkComparer);
            for (int i = 0; i < this.distanceSortedSubchunksBuffer.Count; i++)
            {
                this.distanceSortedSubchunksBuffer[i].rectBehindCamera = this.distanceSortedSubChunks[i].AABB.GetScreenSpaceRect(Matrix4x4.identity, out this.distanceSortedSubChunks[i].screenRect, this.activeCamera.viewMatrix, this.activeCamera.projectionMatrix);
            }

            for (int i = 0; i < this.distanceSortedSubchunksBuffer.Count; i++)
            {
                if (!this.distanceSortedSubchunksBuffer[i].isCulled)
                {
                    for (int j = i + 1; j < this.distanceSortedSubchunksBuffer.Count; j++)
                    {
                        if (!this.distanceSortedSubchunksBuffer[j].isCulled)
                        {
                            if (!this.distanceSortedSubchunksBuffer[i].rectBehindCamera)
                                continue;

                            //if (this.distanceSortedSubChunks[i].screenRect.Overlaps(this.distanceSortedSubChunks[j].screenRect))
                            //    this.distanceSortedSubChunks[j].isOccluded = true;

                            float xminL = this.distanceSortedSubchunksBuffer[i].screenRect.min.x;
                            float xmaxL = this.distanceSortedSubchunksBuffer[i].screenRect.max.x;
                            float xminS = this.distanceSortedSubchunksBuffer[j].screenRect.min.x;
                            float xmaxS = this.distanceSortedSubchunksBuffer[j].screenRect.max.x;

                            if (xminL < xminS && xmaxL > xmaxS)
                            {
                                // Wider than further
                                float yMinL = this.distanceSortedSubchunksBuffer[i].screenRect.min.y;
                                float yMaxS = this.distanceSortedSubchunksBuffer[j].screenRect.max.y;

                                if (yMinL > yMaxS)
                                {
                                    this.distanceSortedSubchunksBuffer[j].isOccluded = true;
                                }
                            }
                        }
                    }
                }
            }
        }
        */

        public void PropCulling()
        {
            /*
            for (int i = 0; i < this.loadedProps.Count; i++)
            {
                if (this.props.TryGetValue(this.loadedProps[i], out Prop prop))
                {
                    for (int j = 0; j < prop.uniqueInstanceIDs.Count; j++)
                    {
                        prop.cullingResults[prop.uniqueInstanceIDs[j]] = false;
                    }
                }
            }

            foreach (var item in this.activeChunks)
            {
                if (item.Value.isVisible)
                {
                    if (item.Value.lod0Available)
                    {
                        for (int i = 0; i < item.Value.area.subChunks.Count; i++)
                        {
                            if (item.Value.area.subChunks[i].isVisible)
                            {
                                if (item.Value.area.subChunks[i].propUniqueIDs == null) continue;
                                for (int j = 0; j < item.Value.area.subChunks[i].propUniqueIDs.Length; j++)
                                {
                                    var uuid = item.Value.area.subChunks[i].propUniqueIDs[j];
                                    var areaprop = item.Value.area.uuidPropMap[uuid];
                                    var size = areaprop.placement.sizef;
                                    if (areaprop.path == null) continue;

                                    if (item.Value.area.subChunks[i].distanceToCam / size * areaprop.scale < 100f)
                                    {
                                        if (areaprop.loadRequested)
                                        {
                                            if (this.props.TryGetValue(areaprop.path, out Prop prop))
                                            {
                                                if (prop.boundingBox != null)
                                                {
                                                    if (prop.instances.TryGetValue(uuid, out Prop.Instance instance))
                                                    {
                                                        if (prop.boundingBox.GetScreenSpaceRect(instance.transform, out Rect rect, this.activeCamera.viewMatrix, this.activeCamera.projectionMatrix))
                                                        {
                                                            if (rect.height > 10f)
                                                                prop.cullingResults[uuid] = true;
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        if (!areaprop.loadRequested)
                                        {
                                            areaprop.loadRequested = true;
                                            if (areaprop.modelType == Data.Area.Prop.ModelType.M3)
                                            {
                                                // Load M3 Model //
                                                this.gameData.resourceManager.LoadM3Model(areaprop.path);
                                                this.gameData.resourceManager.modelResources[areaprop.path].TryBuildObject(areaprop.uniqueID, areaprop.position, areaprop.rotation, areaprop.scale * Vector3.one);
                                            }
                                            else if (areaprop.modelType == Data.Area.Prop.ModelType.I3)
                                            {
                                                // Load I3 Model //
                                            }
                                            else if (areaprop.modelType == Data.Area.Prop.ModelType.DGN)
                                            {
                                                // Load DGN File //
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            */
        }

        List<Vector2> CreateChunks(string assetPath)
        {
            List<Vector2> list = new List<Vector2>();
            var mapDirectory = $"{Data.Archive.rootBlockName}\\{assetPath}";
            Dictionary<string, Data.Block.FileEntry> allFilesInMapDir = this.gameData.GetFileEntries(mapDirectory);
            if (allFilesInMapDir == null) return null;

            string fileName = Path.GetFileName(assetPath);

            // Area Low Files //
            Dictionary<Vector2, Data.Block.FileEntry> allAreaLowFiles = new Dictionary<Vector2, Data.Block.FileEntry>();
            for (int i = 0; i < 16; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    string x = i.ToString("X").ToLower();
                    string y = j.ToString("X").ToLower();
                    string path = $"{fileName}_Low.{y}{x}.area";
                    if (allFilesInMapDir.ContainsKey(path))
                    {
                        Vector2 coords = new Vector2(i, j);
                        allAreaLowFiles.Add(coords, allFilesInMapDir[path]);
                    }
                }
            }

            for (int i = 0; i < WORLD_SIZE; i++)
            {
                for (int j = 0; j < WORLD_SIZE; j++)
                {
                    string x = i.ToString("X").ToLower();
                    string y = j.ToString("X").ToLower();
                    string path = $"{fileName}.{y}{x}.area";
                    if (allFilesInMapDir.ContainsKey(path))
                    {
                        Vector2 coords = new Vector2(i, j);
                        list.Add(coords);

                        // TODO : find which area low file belongs to this chunk and add the file reference to Chunk constructor

                        this.chunks.Add(coords, new Chunk(coords, allFilesInMapDir[path], this.gameData, this));
                    }
                }
            }

            return list;
        }
    }
}
