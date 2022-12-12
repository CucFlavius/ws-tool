using OpenTK.Mathematics;
using ProjectWS.Engine.Data.Extensions;
using ProjectWS.Engine.Rendering;
using SharpFont;
using System.Collections.Concurrent;
using System.Diagnostics.Metrics;
using static ProjectWS.Engine.Data.Archive;
using static ProjectWS.Engine.Data.Area;

namespace ProjectWS.Engine.World
{
    public class Chunk
    {
        #region Variables

        public Data.GameData gameData;
        public World world;
        public Data.Block.FileEntry areaFileEntry;
        public string areaFilePath;
        public Vector2 coords;
        Vector2 center;
        Vector2 lowCoords;
        public Vector3 worldCoords;
        public Matrix4 worldMatrix;
        int chunkDistance;
        public volatile bool isVisible;
        int lod = -1;
        public bool lod0Available;
        public bool lod0Loading;
        public bool lod1Available;
        public bool lod1Loading;
        int[] lodQuadrants;

        public Data.BoundingBox AABB;
        public Data.Area area;
        public Data.Area areaLow;

        const float LABEL_DISTANCE = 300f;

        public readonly Vector3[] corners = new Vector3[]
        {
            new Vector3(-1, 0, -1),
            new Vector3(1, 0, -1),
            new Vector3(-1, 0, 1),
            new Vector3(1, 0, 1)
        };

        // Tasks
        public ConcurrentQueue<TaskManager.TerrainTask> terrainTasks;
        public ConcurrentQueue<TaskManager.TerrainTask> buildTasks;
        internal bool modified;

        #endregion

        /// <summary>
        /// Load chunk from game data
        /// </summary>
        /// <param name="coords"></param>
        /// <param name="file"></param>
        /// <param name="data"></param>
        /// <param name="world"></param>
        public Chunk(Vector2 coords, Data.Block.FileEntry file, Data.GameData data, World world)
        {
            this.terrainTasks = new ConcurrentQueue<TaskManager.TerrainTask>();
            this.buildTasks = new ConcurrentQueue<TaskManager.TerrainTask>();

            this.gameData = data;
            this.world = world;
            this.areaFileEntry = file;
            this.coords = coords;
            this.worldCoords = Utilities.ChunkToWorldCoords(coords);
            this.worldMatrix = new Matrix4().TRS(this.worldCoords, Quaternion.Identity, new Vector3(1, 1, 1));
            this.lowCoords = Utilities.CalculateLowCoords(coords);
            this.lodQuadrants = Utilities.CalculateLoDQuadrants(coords, this.lowCoords);
            this.AABB = new Data.BoundingBox(this.worldCoords, new Vector3(512f, 10000f, 512f));
        }

        /// <summary>
        /// Load chunk from project
        /// </summary>
        /// <param name="coords"></param>
        /// <param name="file"></param>
        /// <param name="data"></param>
        /// <param name="world"></param>
        public Chunk(Vector2 coords, string file, Data.GameData data, World world)
        {
            this.terrainTasks = new ConcurrentQueue<TaskManager.TerrainTask>();
            this.buildTasks = new ConcurrentQueue<TaskManager.TerrainTask>();

            this.gameData = data;
            this.world = world;
            this.areaFilePath = file;
            this.coords = coords;
            this.worldCoords = Utilities.ChunkToWorldCoords(coords);
            this.worldMatrix = new Matrix4().TRS(this.worldCoords, Quaternion.Identity, new Vector3(1, 1, 1));
            this.lowCoords = Utilities.CalculateLowCoords(coords);
            this.lodQuadrants = Utilities.CalculateLoDQuadrants(coords, this.lowCoords);
            this.AABB = new Data.BoundingBox(this.worldCoords, new Vector3(512f, 10000f, 512f));
        }

        /// <summary>
        /// Create new flat chunk
        /// </summary>
        /// <param name="coords"></param>
        /// <param name="data"></param>
        /// <param name="world"></param>
        public Chunk(Vector2 coords, Data.GameData data, World world)
        {
            this.terrainTasks = new ConcurrentQueue<TaskManager.TerrainTask>();
            this.buildTasks = new ConcurrentQueue<TaskManager.TerrainTask>();

            this.gameData = data;
            this.world = world;
            this.coords = coords;
            this.worldCoords = Utilities.ChunkToWorldCoords(coords);
            this.worldMatrix = new Matrix4().TRS(this.worldCoords, Quaternion.Identity, new Vector3(1, 1, 1));
            this.lowCoords = Utilities.CalculateLowCoords(coords);
            this.lodQuadrants = Utilities.CalculateLoDQuadrants(coords, this.lowCoords);
            this.AABB = new Data.BoundingBox(this.worldCoords, new Vector3(512f, 10000f, 512f));
        }

        public void EnqueueTerrainTask(TaskManager.TerrainTask task) => this.terrainTasks.Enqueue(task);

        public void EnqueueBuildTask(TaskManager.TerrainTask task) => this.buildTasks.Enqueue(task);

        public int GetLoDQuadrant(int index) => this.lodQuadrants[index];

        public void RenderTerrain(Shader terrainShader)
        {
            if (!this.isVisible)
                return;

            if (this.lod != -1)
            {
                if (this.lod == 0)
                {
                    bool rendered = false;
                    if (this.lod0Available)
                    {
                        RenderLod0Terrain(terrainShader);
                        rendered = true;
                    }

                    if (this.lod1Available && !rendered)
                    {
                        RenderLod1Terrain();
                        rendered = true;
                    }
                }
                else if (this.lod == 1)
                {
                    if (this.lod1Available)
                    {
                        RenderLod1Terrain();
                    }
                }
            }
        }

        public void RenderWater(Shader terrainShader)
        {
            if (!this.isVisible)
                return;

            if (this.lod != -1)
            {
                if (this.lod == 0)
                {
                    bool rendered = false;
                    if (this.lod0Available)
                    {
                        RenderLod0Water(terrainShader);
                        rendered = true;
                    }

                    if (this.lod1Available && !rendered)
                    {
                        RenderLod1Water();
                        rendered = true;
                    }
                }
                else if (this.lod == 1)
                {
                    if (this.lod1Available)
                    {
                        RenderLod1Water();
                    }
                }
            }
        }

        void RenderLod0Terrain(Shader shader)
        {
            if (this.area == null) return;

            if (this.area.subChunks == null) return;

            for (int i = 0; i < this.area.subChunks.Count; i++)
            {
                if (!this.area.subChunks[i].isVisible)
                    continue;

                if (this.area.subChunks[i].mesh != null)
                {
                    Rendering.WorldRenderer.drawCalls++;
                    shader.SetMat4("model", this.area.subChunks[i].matrix);

                    this.area.subChunks[i].Render(shader);
                }
            }
        }

        void RenderLod0Water(Shader shader)
        {
            if (this.area == null) return;

            for (int i = 0; i < this.area.subChunks.Count; i++)
            {
                if (!this.area.subChunks[i].isVisible)
                    continue;

                if (this.area.subChunks[i].hasWater)
                {
                    for (int j = 0; j < this.area.subChunks[i].waters.Length; j++)
                    {
                        this.area.subChunks[i].waters[j].Render(shader);
                    }
                }
            }
        }

        public void RenderDebug()
        {
            if (this.area == null) return;
            if (this.area.subChunks == null) return;

            for (int i = 0; i < this.area.subChunks.Count; i++)
            {
                if (this.area.subChunks[i] == null)
                    continue;

                if (!this.area.subChunks[i].isVisible)
                    continue;

                if (this.area.subChunks[i].mesh != null)
                {
                    var pos = this.area.subChunks[i].centerPosition;

                    if (this.area.subChunks[i].distanceToCam < LABEL_DISTANCE)
                    {
                        if (this.world.controller.subchunkIndex == i)
                        {
                            float fade = MathF.Max(MathF.Min(1.0f - (this.area.subChunks[i].distanceToCam / LABEL_DISTANCE), 1.0f), 0.0f);
                            if (this.areaFileEntry != null)
                                Debug.DrawLabel($"{this.areaFileEntry.name} | {i}", pos, new Vector4(1.0f, 0.5f, 1.0f, fade), true);
                            else if (this.areaFilePath != null)
                                Debug.DrawLabel($"{this.areaFilePath} | {i}", pos, new Vector4(1.0f, 0.5f, 1.0f, fade), true);
                        }
                        /*
                        for (int j = 0; j < 4; j++)
                        {
                            var corner = this.area.subChunks[i].skyCorners[j];

                            if (corner != null)
                            {
                                //this.currentWorldSkyID = corner.worldSkyIDs[j];
                                //var worldSkyRecord = this.database.worldSky.Get(corner.worldSkyIDs[j]);
                                if (this.world.controller.subchunkIndex == i)
                                {
                                    //Debug.DrawLabel(j.ToString(), pos + (directions[j] * 16.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f), true);
                                    Debug.DrawLabel(corner.worldSkyIDs[3].ToString(), pos + (corners[j] * 16.0f), new Vector4(1.0f, 1.0f, 0.0f, 1.0f), true);
                                }
                                else
                                {
                                    Debug.DrawLabel(corner.worldSkyIDs[3].ToString(), pos + (corners[j] * 16.0f), Vector4.One, true);
                                }
                            }
                        }
                        */
                    }
                }
            }
        }

        void RenderLod1Terrain()
        {
            if (this.areaLow != null)
            {

            }
        }

        void RenderLod1Water()
        {
            if (this.areaLow != null)
            {

            }
        }

        public void SetLod(int lod, Vector2 center)
        {
            if (this.lod != lod)
            {
                this.lod = lod;
            }

            if (lod != -1)
            {
                this.chunkDistance = (int)Math.Max(Math.Abs(center.X - this.coords.X), Math.Abs(center.Y - this.coords.Y));
                this.center = center;
            }

            switch (this.lod)
            {
                case 0:
                    LoadLod1();
                    LoadLod0();
                    break;
                case 1:
                    LoadLod1();
                    break;
            }

            if (this.lod == -1)
            {
                // TOOD: Later on, check how many chunks are loaded in memory, and if this is an older unloaded one then actually dispose of it to free memory
            }
        }

        void LoadLod0()
        {
            if (!this.lod0Available && !this.lod0Loading)
            {
                this.lod0Loading = true;

                this.area = new Data.Area(this, 0);
                this.terrainTasks.Enqueue(new TaskManager.TerrainTask(this, 0, TaskManager.Task.JobType.Read));
            }
        }

        void LoadLod1()
        {
            /*
            if (!this.lod1Available && !this.lod1Loading)
            {
                this.lod1Loading = true;

                this.areaLow = new Area(this, 1);
                this.terrainTasks.Enqueue(new TaskManager.TerrainTask(this, 1, TaskManager.Task.JobType.Read));
            }
            */
        }

        public void CalculateCulling(Camera camera/*, Light sunLight*/)
        {
            //Vector3 sunVector = sunLight.lightObj.transform.forward;      // Main thread

            // Chunk //

            // Frustum Culling //
            this.isVisible = camera.frustum.VolumeVsFrustum(this.AABB.center + (this.AABB.extents / 2), this.AABB.extents.X, this.AABB.extents.Y, this.AABB.extents.Z);

            // SubChunks //
            if (this.isVisible)
            {
                if (this.lod0Available)
                {
                    for (int i = 0; i < this.area.subChunks.Count; i++)
                    {
                        // Reset occlusion //
                        this.area.subChunks[i].isOccluded = false;

                        // Distance Culling //
                        this.area.subChunks[i].distanceToCam = Math.Abs(Vector3.Distance(this.area.subChunks[i].centerPosition, camera.transform.GetPosition()));
                        this.area.subChunks[i].isCulled = this.area.subChunks[i].distanceToCam > 1024f;  // 1024 being the distance between 2 chunks

                        // Frustum Culling //
                        if (!this.area.subChunks[i].isCulled)
                        {
                            var aabb = this.area.subChunks[i].AABB;
                            this.area.subChunks[i].isCulled = !camera.frustum.VolumeVsFrustum(aabb.center + (aabb.extents / 2), aabb.extents.X, aabb.extents.Y, aabb.extents.Z);
                        }
                    }
                }
            }
        }
    }
}
