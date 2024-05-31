using System;

namespace Sleep0
{
    public static class ArrayExtensions
    {
        /// <summary>
        /// Shifts the array to the left by the given amount.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr"></param>
        /// <param name="shift"></param>
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