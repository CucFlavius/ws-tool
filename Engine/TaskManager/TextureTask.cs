using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;


namespace ProjectWS.Engine.TaskManager
{
    public class TextureTask : Task
    {
        string filePath;
        Data.ResourceManager.Manager resourceManager;

        public TextureTask(string filePath, JobType jobType, Data.ResourceManager.Manager resourceManager)
        {
            this.filePath = filePath;
            this.dataType = DataType.Texture;
            this.jobType = jobType;
            this.resourceManager = resourceManager;
        }

        public override void PerformTask()
        {
            switch (this.jobType)
            {
                case JobType.Read:
                    this.resourceManager.textureResources[this.filePath].Read();
                    this.jobType = JobType.Build;
                    this.resourceManager.engine.taskManager.buildTasks.Enqueue(this);
                    break;
                case JobType.Write:
                    break;
                case JobType.Build:
                    this.resourceManager.textureResources[this.filePath].LoadTexture();
                    this.resourceManager.textureResources[this.filePath].AssignTextureToAllMatRef();
                    break;
                case JobType.Destroy:
                    break;
                default:
                    break;
            }
        }
    }
}