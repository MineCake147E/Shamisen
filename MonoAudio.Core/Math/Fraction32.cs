using System;

using SignedIntType = System.Int32;
using UnsignedIntType = System.UInt32;
namespace MonoAudio.Math
{
    /// <summary>
    /// Represents a 32bit/32bit fractions.
    /// Supports high-precision arithmetics over fractions.
    /// Its total size is 64bit.
    /// </summary>
    public readonly struct Fraction32
    {

        public readonly SignedIntType Numerator;

        public readonly UnsignedIntType Denominator;

    }
}
