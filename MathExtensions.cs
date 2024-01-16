using System;

namespace Sleep0
{
    public class MathExtensions
    {
        public static double NthRoot(double A, int N)
        {
            return Math.Pow(A, 1.0 / N);
        }

        public static float NthRoot(float A, float N)
        {
            return (float)Math.Pow(A, 1.0f / N);
        }

        public static int NthRoot(int A, int N)
        {
            return (int)Math.Pow(A, 1.0f / N);
        }

        public static int Exponent(int @base, int value)
        {
            return (int)(MathF.Log(value) / MathF.Log(@base));
        }
    }

}