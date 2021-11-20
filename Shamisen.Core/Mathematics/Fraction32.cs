using SignedIntType = System.Int32;
using UnsignedIntType = System.UInt32;

namespace Shamisen.Mathematics
{
    /// <summary>
    /// Represents a 32bit/32bit fractions.
    /// Supports high-precision arithmetics over fractions.
    /// Its total size is 64bit.
    /// </summary>
    public readonly struct Fraction32
    {
        /// <summary>
        /// The numerator.
        /// </summary>
        public readonly SignedIntType Numerator;

        /// <summary>
        /// The denominator.
        /// </summary>
        public readonly UnsignedIntType Denominator;
    }
}
