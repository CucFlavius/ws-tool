using OpenTK.Mathematics;
using ProjectWS.Engine.Components;
using ProjectWS.Engine.Data.Extensions;
using ProjectWS.Engine.Lighting;
using ProjectWS.Engine.Objects.Gizmos;
using ProjectWS.Engine.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ProjectWS.Engine.Data.Area;

namespace ProjectWS.Engine
{
    public class MousePick
    {
        private WorldRenderer renderer;
        public Vector3 rayOrigin;
        public Vector3 rayVec;

        //public RayGizmo rayGizmo;
        public BoxGizmo hitGizmo;

        public MousePick(WorldRenderer renderer)
        {
            this.renderer = renderer;
            //this.rayGizmo = new Objects.Gizmos.RayGizmo(new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
            this.hitGizmo = new Objects.Gizmos.BoxGizmo(new Vector4(1.0f, 1.0f, 0.0f, 1.0f));

            if (renderer.gizmos != null)
            {
                //renderer.gizmos.Add(this.rayGizmo);
                renderer.gizmos.Add(this.hitGizmo);
            }
            if (renderer.engine != null)
            {
                //renderer.engine.taskManager.buildTasks.Enqueue(new TaskManager.BuildObjectTask(this.rayGizmo));
                renderer.engine.taskManager.buildTasks.Enqueue(new TaskManager.BuildObjectTask(this.hitGizmo));
            }
        }

        public void Update()
        {
            if (this.renderer.viewports == null)
                return;

            for (int v = 0; v < this.renderer.viewports.Count; v++)
            {
                var vp = this.renderer.viewports[v];

                if (vp.interactive)
                {
                    var mousePos = this.renderer.engine.input.GetMousePosition();
                    this.rayVec = Unproject(vp, mousePos);
                    this.rayOrigin = vp.mainCamera.transform.GetPosition();

                }
            }

            TerrainPick();
        }

        private void TerrainPick()
        {
            // Terrain Pick //
            if (this.renderer.world != null)
            {
                this.hitGizmo.visible = false;
                foreach (var chunkItem in this.renderer.world.chunks)
                {
                    if (chunkItem.Value.area != null && chunkItem.Value.lod0Available && chunkItem.Value.area.subChunks != null)
                    {
                        foreach (var subChunk in chunkItem.Value.area.subChunks)
                        {
                            Vector2 result = subChunk.AABB.RayBoxIntersect(this.rayOrigin, this.rayVec);

                            if (result.X <= result.Y)
                            {
                                var subPos = new Vector3(subChunk.X * 32f, 0f, subChunk.Y * 32f);// subChunk.centerPosition;//subChunk.matrix.ExtractPosition();
                                Debug.DrawLabel(
                                    subChunk.centerPosition.ToString(),
                                    subChunk.centerPosition,
                                    new Vector4(0.0f, 0.0f, 0.0f, 1.0f),
                                    false);

                                for (int i = 0; i < subChunk.mesh.indexData.Length; i+=3)
                                {
                                    uint i0 = subChunk.mesh.indexData[i];
                                    uint i1 = subChunk.mesh.indexData[i + 1];
                                    uint i2 = subChunk.mesh.indexData[i + 2];

                                    var v0 = subChunk.mesh.vertices[i0].position + subPos;
                                    var v1 = subChunk.mesh.vertices[i1].position + subPos;
                                    var v2 = subChunk.mesh.vertices[i2].position + subPos;

                                    if (RayTriangleIntersect(this.rayOrigin, this.rayVec, v0, v1, v2, out var point))
                                    {
                                        this.hitGizmo.visible = true;

                                        //var hitPoint = GetPointOnRay(this.rayVec, result.X, this.rayOrigin);
                                        var gizmoMat = Matrix4.CreateTranslation(point);
                                        this.hitGizmo.transform.SetMatrix(gizmoMat);

                                        subChunk.mesh.vertices[i0].position += Vector3.UnitY * 0.1f;
                                        subChunk.mesh.vertices[i1].position += Vector3.UnitY * 0.1f;
                                        subChunk.mesh.vertices[i2].position += Vector3.UnitY * 0.1f;

                                        subChunk.mesh.ReBuild();

                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private Vector3 Unproject(Viewport vp, Vector3 mousePos)
        {
            var ndc = MouseToNormalizedDeviceCoords(mousePos, vp.width, vp.height);
            var clip = NDCToClipCoords(ndc);

            //var projection = Matrix4.CreatePerspectiveFieldOfView(
            //    vp.mainCamera.fov, vp.mainCamera.aspectRatio, vp.mainCamera.nearDistance, vp.mainCamera.farDistance);

            var eye = ClipToEye(clip, vp.mainCamera.projection);

            var cc = vp.mainCamera.components[0] as CameraController;

            var rayvec = EyeToRayVector(eye, vp.mainCamera.view);
            return rayvec;
        }

        Vector2 MouseToNormalizedDeviceCoords(Vector3 mousePos, int width, int height)
        {
            float x = (2.0f * mousePos.X) / width - 1.0f;
            float y = 1.0f - (2.0f * mousePos.Y) / height;
            return new Vector2(x, y);
        }

        Vector4 NDCToClipCoords(Vector2 ray_nds)
        {
            return new Vector4(ray_nds.X, ray_nds.Y, -1.0f, 1.0f);
        }

        Vector4 ClipToEye(Vector4 ray_clip, Matrix4 projection_matrix)
        {
            Vector4 ray_eye = ray_clip * projection_matrix.Inverted();
            return new Vector4(ray_eye.X, ray_eye.Y, -1.0f, 0.0f);
        }

        Vector3 EyeToRayVector(Vector4 ray_eye, Matrix4 view_matrix)
        {
            Vector3 ray_wor = (ray_eye * view_matrix.Inverted()).Xyz;
            ray_wor.Normalize();

            return ray_wor;
        }

        Vector3 GetPointOnRay(Vector3 ray, float distance, Vector3 camPos)
        {
            Vector3 start = new Vector3(camPos.X, camPos.Y, camPos.Z);
            Vector3 scaledRay = new Vector3(ray.X * distance, ray.Y * distance, ray.Z * distance);
            return start + scaledRay;
        }

        bool RayTriangleIntersect(Vector3 rayOrigin, Vector3 rayVector, Vector3 vertex0, Vector3 vertex1, Vector3 vertex2, out Vector3 outIntersectionPoint)
        {
            outIntersectionPoint = Vector3.Zero;
            const float EPSILON = 0.0000001f;
            Vector3 edge1, edge2, h, s, q;
            float a, f, u, v;
            edge1 = vertex1 - vertex0;
            edge2 = vertex2 - vertex0;
            h = Vector3.Cross(rayVector, edge2);
            a = Vector3.Dot(edge1, h);

            if (a > -EPSILON && a < EPSILON)
                return false;    // This ray is parallel to this triangle.

            f = 1.0f / a;
            s = rayOrigin - vertex0;
            u = f * Vector3.Dot(s, h);

            if (u < 0.0f || u > 1.0f)
                return false;

            q = Vector3.Cross(s, edge1);
            v = f * Vector3.Dot(rayVector, q);

            if (v < 0.0f || u + v > 1.0f)
                return false;

            // At this stage we can compute t to find out where the intersection point is on the line.
            float t = f * Vector3.Dot(edge2, q);

            if (t > EPSILON) // ray intersection
            {
                outIntersectionPoint = rayOrigin + rayVector * t;
                return true;
            }

            else // This means that there is a line intersection but not a ray intersection.
                return false;
        }
    }
}
