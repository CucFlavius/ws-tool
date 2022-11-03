using System.Collections;
using System.Collections.Generic;

namespace ProjectWS.Engine.Data.ResourceManager
{
    public class Manager
    {
        public Engine engine;
        public Dictionary<string, TextureResource> textureResources;
        public Dictionary<string, ModelResource> modelResources;

        public Manager(Engine engine)
        {
            this.engine = engine;
            this.textureResources = new Dictionary<string, TextureResource>();
            this.modelResources = new Dictionary<string, ModelResource>();
        }

        public void AssignTexture(string filePath, Materials.Material material, string samplerName)
        {
            if (!this.textureResources.ContainsKey(filePath))
            {
                this.textureResources.Add(filePath, new TextureResource(filePath, this.engine.resourceManager, this.engine.data));
                this.engine.taskManager.textureThread.Enqueue(new TaskManager.TextureTask(filePath, TaskManager.Task.JobType.Read, this));
            }
            this.textureResources[filePath].AssignTexture(material, samplerName);
        }

        /*
        public void LoadM3Model(string path)
        {
            if (this.modelResources.ContainsKey(path)) return;

            this.modelResources.Add(path, new ModelResource(path, this.engine.data, this.engine.worlds[0]));

            this.engine.taskManager.modelThread.Enqueue(new TaskManager.ModelTask(path, TaskManager.Task.JobType.Read, this));
        }
        */
        /*
        public TextureResource GetTextureResource(string filePath)
        {
            if (this.textureResources.ContainsKey(filePath))
                return this.textureResources[filePath];
            else
                return null;
        }
        */
        public enum ResourceState
        {
            None,
            IsLoading,
            IsLoaded,
            IsParsed,
            IsReady
        }
    }
}