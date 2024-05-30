using UnityEngine;

namespace Sleep0.Math
{
    public static class Vector3Extensions
    {
        public static (float, float, float) ConvertToPolar(this Vector3 centerPosition, Quaternion centerRotation, Vector3 position)
        {
            Vector3 relativePos = position - centerPosition;

            // Rotate the relative position by the inverse of the player's rotation
            relativePos = Quaternion.Inverse(centerRotation) * relativePos;

            float distance = new Vector3(relativePos.x, 0, relativePos.z).magnitude;
            float angle = Mathf.Atan2(relativePos.z, relativePos.x) * Mathf.Rad2Deg;
            if (angle < 0) angle += 360;
            float altitude = relativePos.y;

            return (distance, angle, altitude);
        }

        public static float[] ToFloatArray(this Vector3 vector)
        {
            return new float[] { vector.x, vector.y, vector.z };
        }
    }
}
