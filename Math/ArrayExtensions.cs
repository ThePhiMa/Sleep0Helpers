using System;

namespace Sleep0.Math
{
    public static class ArrayExtensions
    {
        public static void LeftShift<T>(this T[] arr, int shift)
        {
            shift = shift % arr.Length;
            T[] buffer = new T[shift];
            Array.Copy(arr, buffer, shift);
            Array.Copy(arr, shift, arr, 0, arr.Length - shift);
            Array.Copy(buffer, 0, arr, arr.Length - shift, shift);
        }
    }
}