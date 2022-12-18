using MathUtils;
using ProjectWS.Engine.Components;
using ProjectWS.Engine.Data;
using ProjectWS.Engine.Objects.Gizmos;
using ProjectWS.Engine.Rendering;

namespace ProjectWS.Engine
{
    public class MousePick
    {
        public Mode mode = Mode.Disabled;
        private WorldRenderer renderer;
        public Ray mouseRay;

        public Vector3 terrainHitPoint;
        public Vector2i terrainSubchunkHit;
        public Vector2i areaHit;
        public World.Prop? propHit;
        public World.Prop.Instance? propInstanceHit;

        public enum Mode
        {
            Disabled,
            Terrain,
            Prop,
        }

        public MousePick(WorldRenderer renderer)
        {
            this.renderer = renderer;
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
                    this.mouseRay = new Ray(vp.mainCamera.transform.GetPosition(), Unproject(vp, mousePos));
                }
            }

            if (this.mode == Mode.Terrain)
                TerrainPick();
            else if (this.mode == Mode.Prop)
                PropPick();
        }

        private void TerrainPick()
        {
            // Terrain Pick //
            if (this.renderer.world != null)
            {
                this.renderer.brushParameters.isEnabled = false;
                foreach (var chunkItem in this.renderer.world.chunks)
                {
                    if (chunkItem.Value.area != null && chunkItem.Value.lod0Available && chunkItem.Value.area.subChunks != null)
                    {
                        var areaPos = chunkItem.Value.worldCoords;

                        foreach (var sc in chunkItem.Value.area.subChunks)
                        {
                            if (sc.isVisible)
                            {
                                Vector2 result = sc.AABB.IntersectsRay(this.mouseRay);

                                if (result.X <= result.Y)
                                {
                                    var subPos = new Vector3(sc.X * 32f, 0f, sc.Y * 32f) + areaPos;

                                    this.terrainSubchunkHit = new Vector2i(sc.X, sc.Y);
                                    this.areaHit = chunkItem.Key;

                                    for (int i = 0; i < sc.mesh.indexData.Length; i += 3)
                                    {
                                        uint i0 = sc.mesh.indexData[i];
                                        uint i1 = sc.mesh.indexData[i + 1];
                                        uint i2 = sc.mesh.indexData[i + 2];

                                        var v0 = sc.mesh.vertices[i0].position + subPos;
                                        var v1 = sc.mesh.vertices[i1].position + subPos;
                                        var v2 = sc.mesh.vertices[i2].position + subPos;
                                        
                                        if (RayTriangleIntersect(this.mouseRay.origin, this.mouseRay.direction, v0, v1, v2, out var point))
                                        {
                                            this.renderer.brushParameters.isEnabled = true;
                                            this.terrainHitPoint = point;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Iterate over all the props in the world, and all instances that are visible
        /// and check if the mouse ray intersects with the bounding boxes, then pick the most suitable prop to be selected
        /// </summary>
        private void PropPick()
        {
            if (this.renderer.world != null)
            {
                this.renderer.brushParameters.isEnabled = false;

                foreach (var propItem in this.renderer.world.props)
                {
                    //var aabb = propItem.Value.aabb;

                    int instanceIndex = 0;
                    foreach (var instanceItem in propItem.Value.instances)
                    {
                        var instance = instanceItem.Value;
                        if (instance.visible)
                        {
                            Vector2 intersection = instance.obb.IntersectsRay(this.mouseRay, instance.position, instance.scale);
                            if (intersection.X <= intersection.Y)
                            {
                                this.propHit = propItem.Value;
                                this.propInstanceHit = instance;

                                // Exiting once a prop is found
                                // TODO : Instead of drawing all labels, check if the prop triangles were hit, and check which hit is closer to the camera
                                /*
                                var labelText = $"{propItem.Value.data.fileName}\n" +
                                    $"UUID:{instance.uuid} Instance:{instanceIndex}\n" +
                                    $"P:{instance.position}\nR:{instance.rotationEuler}\nS:{instance.scale}";
                                Debug.DrawLabel3D(labelText, instance.position, Vector4.One, true);
                                */
                                DrawOBB(instance.obb, instance.transform, new Vector4(1, 1, 0, 1));
                            }
                            /*
                            Vector2 result = instance.aabb.IntersectsRay(this.mouseRay, instance.position);

                            if (result.X <= result.Y)
                            {
                                this.propHit = propItem.Value;
                                this.propInstanceHit = instance;

                                // Exiting once a prop is found
                                // TODO : Instead of drawing all labels, check if the prop triangles were hit, and check which hit is closer to the camera

                                var labelText = $"{propItem.Value.data.fileName}\n" +
                                    $"UUID:{instance.uuid} Instance:{instanceIndex}\n" +
                                    $"P:{instance.position}\nR:{instance.rotationEuler}\nS:{instance.scale}";
                                Debug.DrawLabel3D(labelText, instance.position, Vector4.One, true);

                                instance.aabb.Draw(instance.position, instance.scale, new Vector4(0, 1, 1, 1));
                            }
                            */
                        }

                        instanceIndex++;
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

        void DrawAABB(AABB aabb, Vector3 position, Vector3 scale, Vector4 color)
        {
            var positionOffsetMat = Matrix4.CreateTranslation(new Vector3(position.X, aabb.center.Y + position.Y, position.Z));
            var scaleMat = Matrix4.CreateScale(aabb.size * scale);
            var boxMat = scaleMat * positionOffsetMat;

            Debug.DrawWireBox3D(boxMat, color);
        }

        internal void DrawOBB(OBB obb, Matrix4 transform, Vector4 color)
        {
            var positionOffsetMat = Matrix4.CreateTranslation(obb.center);
            var scaleMat = Matrix4.CreateScale(obb.size);
            var boxMat = scaleMat * positionOffsetMat * transform;

            Debug.DrawWireBox3D(boxMat, color);
        }
    }
}
