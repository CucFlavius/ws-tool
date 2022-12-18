using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ProjectWS.Engine.Data.M3;
using static ProjectWS.Engine.World.Prop;

namespace ProjectWS.Engine.Data
{
    public class OBB
    {
        public Vector3 center;
        public Quaternion orientation;
        public Vector3 size;

        public Matrix4 boxMat;      // A matrix that when applied to a 1,1,1 box will transform it into this obb
                                    // Used strictly for rendering the OBB for debug
        public bool boxMatNeedsUpdate;

        public OBB(Vector3 center, Quaternion orientation, Vector3 halfSizes)
        {
            this.center = center;
            this.orientation = orientation;
            this.size = halfSizes;

            this.boxMat = Matrix4.Identity;
            this.boxMatNeedsUpdate = true;
        }

        public OBB(AABB aabb, Quaternion orientation)
        {
            this.center = aabb.center;
            this.orientation = orientation;
            this.size = aabb.size;

            this.boxMat = Matrix4.Identity;
            this.boxMatNeedsUpdate = true;
        }

        public Vector2 IntersectsRay(Ray ray, Vector3 worldPosition, Vector3 scale)
        {
            // Transform the ray into the local space of the OBB
            Vector3 rayOrigin = ray.origin - worldPosition;
            Vector3 rayDirection = ray.direction;
            Quaternion inverseOrientation = Quaternion.Invert(this.orientation);
            rayOrigin = inverseOrientation * rayOrigin;
            rayDirection = inverseOrientation * rayDirection;

            var min = (this.center - ((this.size * scale) * 0.5f));
            var max = (this.center + ((this.size * scale) * 0.5f));

            Vector3 tMin = (min - rayOrigin) / rayDirection;
            Vector3 tMax = (max - rayOrigin) / rayDirection;
            Vector3 t1 = new Vector3(MathF.Min(tMin.X, tMax.X), MathF.Min(tMin.Y, tMax.Y), MathF.Min(tMin.Z, tMax.Z));
            Vector3 t2 = new Vector3(MathF.Max(tMin.X, tMax.X), MathF.Max(tMin.Y, tMax.Y), MathF.Max(tMin.Z, tMax.Z));
            float tNear = MathF.Max(MathF.Max(t1.X, t1.Y), t1.Z);
            float tFar = MathF.Min(MathF.Min(t2.X, t2.Y), t2.Z);
            return new Vector2(tNear, tFar);
        }

        public AABB GetEncapsulatingAABB()
        {
            // Transform the OBB's eight corner vertices into world space
            Vector3[] vertices = new Vector3[8];
            for (int i = 0; i < 8; i++)
            {
                vertices[i] = this.orientation * ((this.size * 0.5f) * GetVertexSigns(i)) + this.center;
            }

            // Find the minimum and maximum x, y, and z coordinates
            Vector3 min = vertices[0];
            Vector3 max = vertices[0];
            for (int i = 1; i < 8; i++)
            {
                min.X = Math.Min(min.X, vertices[i].X);
                min.Y = Math.Min(min.Y, vertices[i].Y);
                min.Z = Math.Min(min.Z, vertices[i].Z);
                max.X = Math.Max(max.X, vertices[i].X);
                max.Y = Math.Max(max.Y, vertices[i].Y);
                max.Z = Math.Max(max.Z, vertices[i].Z);
            }

            // Return the AABB as a Bounds object
            return new AABB((min + max) * 0.5f, (max - min) * 0.5f);
        }

        private Vector3 GetVertexSigns(int i)
        {
            return new Vector3(
                (i & 1) == 0 ? 1 : -1,
                (i & 2) == 0 ? 1 : -1,
                (i & 4) == 0 ? 1 : -1
            );
        }

        internal void Draw(Matrix4 transform, Vector4 color)
        {
            if (this.boxMatNeedsUpdate)
            {
                var positionOffsetMat = Matrix4.CreateTranslation(this.center);
                var scaleMat = Matrix4.CreateScale(this.size);
                this.boxMat = scaleMat * positionOffsetMat * transform;
                this.boxMatNeedsUpdate = false;
            }

            Debug.DrawWireBox3D(this.boxMat, color);
        }
    }
}
