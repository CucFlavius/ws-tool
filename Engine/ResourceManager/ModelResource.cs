using System.Collections;
using System.Collections.Generic;
using OpenTK;
using MathUtils;

namespace ProjectWS.Engine.Data.ResourceManager
{
    public class ModelResource
    {
        // Identifiers //
        string filePath;

        // Reference //
        World.World world;

        // Data //
        public FileFormats.M3.File m3;
        public Manager.ResourceState state;

        // Info //
        int referenceCount;

        // Usage //
        public List<ModelReference> modelReferences;

        public ModelResource(string filePath, World.World world)
        {
            this.world = world;
            this.filePath = filePath;
            this.state = Manager.ResourceState.IsLoading;
            this.modelReferences = new List<ModelReference>();
            this.m3 = new FileFormats.M3.File(filePath);
        }

        public void SetFileState(Manager.ResourceState state)
        {
            this.state = state;
        }

        public void BuildAllRefs()
        {
            for (int i = 0; i < modelReferences.Count; i++)
            {
                var item = this.modelReferences[i];
                this.world.LoadProp(this.m3, item.uuid, item.position, item.rotation, item.scale);
            }
        }

        public void TryBuildObject(uint uuid, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            if (this.state != Manager.ResourceState.IsReady)
            {
                // Unavailable //
                this.modelReferences.Add(new ModelReference() { uuid = uuid, position = position, rotation = rotation, scale = scale });
                referenceCount++;
            }
            else
            {
                this.world.LoadProp(m3, uuid, position, rotation, scale);
            }
        }

        public struct ModelReference
        {
            public Vector3 position;
            public Quaternion rotation;
            public Vector3 scale;
            public uint uuid;
        }
    }
}
