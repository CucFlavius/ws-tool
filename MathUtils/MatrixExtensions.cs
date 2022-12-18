namespace MathUtils
{
    public static class MatrixExtensions
    {
        /*
        public static Quaternion ExtractRotation(this Matrix4x4 matrix, bool flippedForward = false)
        {
            Vector3 forward;
            forward.X = matrix.M12;
            forward.Y = matrix.M23;
            forward.Z = matrix.M33;

            Vector3 upwards;
            upwards.X = matrix.M12;
            upwards.Y = matrix.M22;
            upwards.Z = matrix.M32;

            if (forward == Vector3.Zero)
                return Quaternion.Identity;

            return Quaternion.LookRotation(flippedForward ? -forward : forward, upwards);
        }
        */

        public static Matrix4 TRS(this Matrix4 res, Vector3 t, Quaternion r, Vector3 s)
        {
            res = Matrix4.Identity;
            res *= Matrix4.CreateScale(s);
            res *= Matrix4.CreateFromQuaternion(r);
            res *= Matrix4.CreateTranslation(t);
            /*
            res.M11 = (1.0f - 2.0f * (r.Y * r.Y + r.Z * r.Z)) * s.X;
            res.M21 = (r.X * r.Y + r.Z * r.W) * s.X * 2.0f;
            res.M31 = (r.X * r.Z - r.Y * r.W) * s.X * 2.0f;
            res.M41 = 0.0f;
            res.M12 = (r.X * r.Y - r.Z * r.W) * s.Y * 2.0f;
            res.M22 = (1.0f - 2.0f * (r.X * r.X + r.Z * r.Z)) * s.Y;
            res.M32 = (r.Y * r.Z + r.X * r.W) * s.Y * 2.0f;
            res.M42 = 0.0f;
            res.M13 = (r.X * r.Z + r.Y * r.W) * s.Z * 2.0f;
            res.M23 = (r.Y * r.Z - r.X * r.W) * s.Z * 2.0f;
            res.M33 = (1.0f - 2.0f * (r.X * r.X + r.Y * r.Y)) * s.Z;
            res.M43 = 0.0f;
            res.M14 = t.X;
            res.M24 = t.Y;
            res.M34 = t.Z;
            res.M44 = 1.0f;
            */
            return res;
        }

        public static Vector3 ExtractPosition(this Matrix4 matrix)
        {
            /// !!!! TODO !!!! this bight be broken because OpenTK is column/row swapped
            Vector3 position;
            position.X = matrix.M14;
            position.Y = matrix.M24;
            position.Z = matrix.M34;
            return position;
        }
        /*
        public static Vector3 ExtractScale(this Matrix4x4 m)
        {
            return new Vector3(
                m.GetColumn(0).Length,
                m.GetColumn(1).Length,
                m.GetColumn(2).Length
            );
        }
        */
    }
}