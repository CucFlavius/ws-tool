using OpenTK.Mathematics;
using ProjectWS.Engine.Data.Extensions;
using System.Collections.Concurrent;

namespace ProjectWS.Engine.World
{
    public class Chunk
    {
        #region Variables

        public Data.GameData gameData;
        public World world;
        public Data.Block.FileEntry areaFileEntry;
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

        // Tasks
        public ConcurrentQueue<TaskManager.TerrainTask> terrainTasks;
        public ConcurrentQueue<TaskManager.TerrainTask> buildTasks;

        #endregion

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

        public void EnqueueTerrainTask(TaskManager.TerrainTask task) => this.terrainTasks.Enqueue(task);

        public void EnqueueBuildTask(TaskManager.TerrainTask task) => this.buildTasks.Enqueue(task);

        public int GetLoDQuadrant(int index) => this.lodQuadrants[index];

        public void Render(Shader shader)
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
                        RenderLod0(shader);
                        rendered = true;
                    }

                    if (this.lod1Available && !rendered)
                    {
                        RenderLod1();
                        rendered = true;
                    }
                }
                else if (this.lod == 1)
                {
                    if (this.lod1Available)
                    {
                        RenderLod1();
                    }
                }
            }
        }

        void RenderLod0(Shader shader)
        {
            if (this.area == null) return;

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

                /*
                if (this.area.subChunks[i].hasWater)
                {
                    for (int j = 0; j < this.area.subChunks[i].waters.Length; j++)
                    {
                        Graphics.DrawMesh(this.area.subChunks[i].waters[j].mesh, Matrix4x4.identity, this.area.subChunks[i].waters[j].material, 0, cam.camera, 0);
                    }
                }

                if (this.area.subChunks[i].propUniqueIDs != null)
                {
                    for (int j = 0; j < this.area.subChunks[i].propUniqueIDs.Length; j++)
                    {
                        var uuid = this.area.subChunks[i].propUniqueIDs[j];
                        if (this.area.uuidPropMap.TryGetValue(uuid, out Data.Area.Prop pData))
                        {
                            if (pData.modelType == Data.Area.Prop.ModelType.M3)
                            {
                                if (this.world.props.TryGetValue(pData.path, out Prop prop))
                                {
                                    prop.Render(cam.camera);
                                    this.world.propsRendered += prop.instances.Count;
                                }
                            }
                        }
                    }
                }
                */
            }
        }

        void RenderLod1()
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
