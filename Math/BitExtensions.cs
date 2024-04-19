// Source: https://www.dotnetperls.com/set-bit-zero

using System;

namespace Sleep0.Math
{
    static class BitExtensions
    {
        public static int SetBitTo1(this int value, int position)
        {
            // Set a bit at position to 1.
            return value |= (1 << position);
        }

        public static int SetBitTo0(this int value, int position)
        {
            // Set a bit at position to 0.
            return value & ~(1 << position);
        }

        public static bool IsBitSetTo1(this int value, int position)
        {
            // Return whether bit at position is set to 1.
            return (value & (1 << position)) != 0;
        }

        public static bool IsBitSetTo0(this int value, int position)
        {
            // If not 1, bit is 0.
            return !IsBitSetTo1(value, position);
        }

        // Source: https://stackoverflow.com/a/37795876
        public static bool HasExactOneBitSet(this int value)
        {
            return ((value - 1) & value) == 0;
        }

        public static bool HasMultipleBitSet(this int value)
        {
            return !HasExactOneBitSet(value);
        }

        public static string ToBitsString(this byte bits)
        {
            return Convert.ToString(bits, 2).PadLeft(8, '0');
        }
    }
}
