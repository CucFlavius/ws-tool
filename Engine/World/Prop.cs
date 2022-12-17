using OpenTK.Mathematics;
using ProjectWS.Engine.Data.Extensions;
using ProjectWS.Engine.Materials;
using ProjectWS.Engine.Rendering;
using System.Collections.Concurrent;
using static ProjectWS.Engine.Data.Area;

namespace ProjectWS.Engine.World
{
    public class Prop
    {
        // Animation Model
        public Batch[] batches;

        // Instance Buffers //
        public ConcurrentDictionary<uint, Instance> instances;
        public List<Matrix4> renderableInstances;
        public HashSet<uint> renderableUUIDs;
        //public MaterialPropertyBlock block;
        public Dictionary<uint, bool> cullingResults;                // This indicates if instance is culled or not
        public List<uint> uniqueInstanceIDs;

        public Data.M3 data;
        public Data.AABB boundingBox;
        //Mesh[] meshes;
        List<uint> textureResources;
        public bool culled;                             // Determined if renderableInstances.Count == 0

        public Prop(uint uuid, Data.M3 data, Vector3 position, Quaternion rotation, Vector3 scale/*, PropLighting lighting*/)
        {
            this.data = data;
            if (data.bounds != null)
            {
                this.boundingBox = data.bounds[0].bbA;
            }
            else
            {
                this.boundingBox = new Data.AABB(Vector3.Zero, Vector3.One);
            }

            this.cullingResults = new Dictionary<uint, bool>();
            this.renderableInstances = new List<Matrix4>();
            this.renderableUUIDs = new HashSet<uint>();
            this.instances = new ConcurrentDictionary<uint, Instance>();
            this.uniqueInstanceIDs = new List<uint>();
            //this.block = new MaterialPropertyBlock();
            //this.block.Clear();
            if (this.data == null) return;
            if (this.data.geometries == null) return;
            if (this.data.geometries.Length <= 0) return;
            if (this.data.geometries[0].submeshes == null) return;

            this.batches = new Batch[this.data.geometries[0].submeshes.Length];

            AddInstance(uuid, position, rotation, scale);

            this.data.Build();

            for (int i = 0; i < this.batches.Length; i++)
            {
                this.batches[i] = new Batch();
                this.batches[i].mesh = this.data.geometries[0].submeshes[i];
                //this.batches[i].material = mat;
            }
        }

        public void AddInstance(uint uuid, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            if (this.cullingResults.ContainsKey(uuid)) return;

            //Matrix4 mat = Matrix4.Identity;
            //mat = mat.TRS(position, rotation, scale);
            //this.instances.Add(uuid, new Instance(mat, uuid));
            this.instances.TryAdd(uuid, new Instance(position, rotation, scale, uuid));
            this.uniqueInstanceIDs.Add(uuid);
            this.cullingResults.Add(uuid, false);
        }

        public void Render(Shader shader, Matrix4 model)
        {
            if (!this.data.isBuilt) return;

            for (int k = 0; k < this.data.geometries[0].submeshes.Length; k++)
            {
                var submesh = this.data.geometries[0].submeshes[k];

                if (!submesh.isBuilt) continue;
                int matSelector = submesh.materialSelector;

                if (submesh.positionCompressed)
                    shader.SetMat4("model", WorldRenderer.decompressMat * model);
                else
                    shader.SetMat4("model", model);

                this.data.materials[matSelector].mat.SetToShader(shader);

                submesh.Draw();
                Rendering.WorldRenderer.propDrawCalls++;
            }

            /*
            if (this.batches != null && !this.culled)
            {
                for (int b = 0; b < this.batches.Length; b++)
                {
                    this.batches[b].mesh.Draw();
                    Rendering.WorldRenderer.drawCalls++;
                }
            }
            */
        }

        public class Instance
        {
            public Matrix4 transform;
            public Vector3 position;
            public Quaternion rotation;
            public Vector3 rotationEuler;
            public Vector3 scale;
            public Type type;
            public uint uuid;
            internal bool visible;

            //public Rect screenSpace;

            public Instance(Matrix4 transform, uint uuid)
            {
                this.transform = transform;
                this.position = transform.ExtractPosition();
                this.uuid = uuid;
                this.type = Type.WorldProp;
                
                // TODO : Fix TRS extracting and assign pos/rot/scale
            }

            public Instance(Vector3 position, Quaternion rotation, Vector3 scale, uint uuid)
            {
                Matrix4 mat = Matrix4.Identity;
                this.transform = mat.TRS(position, rotation, scale);

                this.uuid = uuid;
                this.type = Type.WorldProp;
                this.position = position;
                this.rotation = rotation;
                this.scale = scale;
                rotation.ToEulerAngles(out this.rotationEuler);
                // Convert to Degree
                this.rotationEuler *= (float)(180f / Math.PI);
            }

            public enum Type
            {
                WorldProp,
                I3Prop,
            }
        }

        public class Batch
        {
            public Data.Submesh mesh;
            public M3Material material;
            //public ShadowCastingMode shadowCastingMode;
            //public bool receiveShadows;
        }
    }
}
