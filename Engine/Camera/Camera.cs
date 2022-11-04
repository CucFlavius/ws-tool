using OpenTK;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectWS.Engine
{
    public class Camera : Objects.GameObject
    {
        public Objects.Gizmos.CameraGizmo gizmo;
        public Rendering.Renderer renderer;
        public float fov;
        public float aspectRatio;
        public float nearDistance;
        public float farDistance;
        public Frustum frustum;
        public Matrix4 projection;
        public Matrix4 view;

        public Camera(Rendering.Renderer renderer, Vector3 position, float fov, float aspectRatio, float nearDistance, float farDistance)
        {
            this.renderer = renderer;
            this.transform.SetPosition(position);
            this.fov = fov;
            this.aspectRatio = aspectRatio;
            this.nearDistance = nearDistance;
            this.farDistance = farDistance;
            this.frustum = new Frustum();
            this.gizmo = new Objects.Gizmos.CameraGizmo(this);
            if (this.renderer.gizmos != null)
                this.renderer.gizmos.Add(this.gizmo);
            if (this.renderer.engine != null)
                this.renderer.engine.taskManager.buildTasks.Enqueue(new TaskManager.BuildObjectTask(this.gizmo));
        }

        public void SetToShader(Shader shader)
        {
            if (this.aspectRatio == 0)
                this.aspectRatio = 1;

            this.projection = Matrix4.CreatePerspectiveFieldOfView(this.fov, this.aspectRatio, this.nearDistance, this.farDistance);

            shader.SetMat4("projection", this.projection);
            shader.SetMat4("view", this.view);
            shader.SetVec3("viewPos", this.transform.GetPosition());
        }

        public override void Update(float deltaTime)
        {
            // Update frustum
            this.frustum.CalculateFrustum(this.projection, this.view);
        }
    }
}
