using UnityEngine;

namespace Sleep0.Math
{
    public static class QuaternionExtensions
    {
        public static Quaternion Slerp(this Quaternion a, Quaternion b, float t)
        {
            // Calculate the angle between the two quaternions.
            float cosHalfTheta = a.w * b.w + a.x * b.x + a.y * b.y + a.z * b.z;

            // If a and b are very close, return a.
            if (cosHalfTheta >= 1.0f)
            {
                return a;
            }

            // Calculate temporary values.
            float halfTheta = Mathf.Acos(cosHalfTheta);
            float sinHalfTheta = Mathf.Sqrt(1.0f - cosHalfTheta * cosHalfTheta);

            // If the angle is too small, return a linear interpolation.
            if (Mathf.Abs(sinHalfTheta) < 0.001f)
            {
                return new Quaternion(
                    a.x * (1.0f - t) + b.x * t,
                    a.y * (1.0f - t) + b.y * t,
                    a.z * (1.0f - t) + b.z * t,
                    a.w * (1.0f - t) + b.w * t
                );
            }

            // Calculate the new quaternion.
            float ratioA = Mathf.Sin((1.0f - t) * halfTheta) / sinHalfTheta;
            float ratioB = Mathf.Sin(t * halfTheta) / sinHalfTheta;

            return new Quaternion(
                a.x * ratioA + b.x * ratioB,
                a.y * ratioA + b.y * ratioB,
                a.z * ratioA + b.z * ratioB,
                a.w * ratioA + b.w * ratioB
            );
        }

        public static Quaternion Add(this Quaternion a, Quaternion b)
        {
            return new Quaternion(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w);
        }

        public static Quaternion Substract(this Quaternion a, Quaternion b)
        {
            return new Quaternion(a.x - b.x, a.y - b.y, a.z - b.z, a.w - b.w);
        }

        public static Quaternion Multiply(this Quaternion q, float scalar)
        {
            return new Quaternion(q.x * scalar, q.y * scalar, q.z * scalar, q.w * scalar);
        }

        public static Quaternion Multiply(this Quaternion a, Quaternion b)
        {
            return new Quaternion(
                a.w * b.x + a.x * b.w + a.y * b.z - a.z * b.y,
                a.w * b.y - a.x * b.z + a.y * b.w + a.z * b.x,
                a.w * b.z + a.x * b.y - a.y * b.x + a.z * b.w,
                a.w * b.w - a.x * b.x - a.y * b.y - a.z * b.z
            );
        }

        public static Quaternion Multiply(this Quaternion q, Matrix4x4 matrix)
        {
            var vector = new Vector4(q.w, q.x, q.y, q.z);
            Vector4 result = matrix * vector;

            return new Quaternion(result.y, result.z, result.w, result.x);

            //return new Quaternion(
            //    matrix.m00 * q.x + matrix.m01 * q.y + matrix.m02 * q.z + matrix.m03 * q.w,
            //    matrix.m10 * q.x + matrix.m11 * q.y + matrix.m12 * q.z + matrix.m13 * q.w,
            //    matrix.m20 * q.x + matrix.m21 * q.y + matrix.m22 * q.z + matrix.m23 * q.w,
            //    matrix.m30 * q.x + matrix.m31 * q.y + matrix.m32 * q.z + matrix.m33 * q.w
            //);
        }

        public static Quaternion Divide(this Quaternion a, Quaternion b)
        {
            return a.Multiply(Quaternion.Inverse(b));
        }

        /// <summary>
        /// Normalizes the quaternion.
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        public static Quaternion Normalize(this Quaternion q)
        {
            float magnitude = Mathf.Sqrt(q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w);
            return new Quaternion(q.x / magnitude, q.y / magnitude, q.z / magnitude, q.w / magnitude);
        }

        /// <summary>
        /// Returns the conjugate of the quaternion.
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        public static Quaternion Conjugate(this Quaternion q)
        {
            return new Quaternion(-q.x, -q.y, -q.z, q.w);
        }

        /// <summary>
        /// Returns the rotation delta between two quaternions.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static Quaternion RotationDelta(this Quaternion a, Quaternion target)
        {
            Quaternion result = target * Quaternion.Inverse(a);

            if (result.w < 0.0f)
            {
                result.x *= -1.0f;
                result.y *= -1.0f;
                result.z *= -1.0f;
                result.w *= -1.0f;
            }

            return result;
        }
    }
}