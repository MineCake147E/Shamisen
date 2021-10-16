using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Codecs.Flac.Parsing
{
    /// <summary>
    /// Calculates a CRC-8 for FLAC stream. Polynomial: CRC-8-CCITT
    /// </summary>
    public readonly partial struct FlacCrc8 : IEquatable<FlacCrc8>
    {
        private readonly byte state;

        /// <summary>
        /// Initializes a new instance of the <see cref="FlacCrc8"/> struct.
        /// </summary>
        /// <param name="state">The state.</param>
        public FlacCrc8(byte state)
        {
            this.state = state;
        }

        /// <summary>
        /// Gets the state.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        public byte State => state;

        /// <summary>
        /// Performs an implicit conversion from <see cref="FlacCrc8"/> to <see cref="byte"/>.
        /// </summary>
        /// <param name="crc8">The CRC8.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static implicit operator byte(FlacCrc8 crc8) => crc8.state;

        /// <summary>
        /// Calculates the next value of CRC-8-CCITT with <paramref name="right"/>.
        /// </summary>
        /// <param name="left">The current CRC-8-CCITT value.</param>
        /// <param name="right">The new byte.</param>
        /// <returns>
        /// The next value of CRC8.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static FlacCrc8 operator *(FlacCrc8 left, byte right) => new(GetTableAt(left.state ^ right));

        /// <summary>
        /// Calculates the next value of CRC-8-CCITT with <paramref name="right"/>.
        /// </summary>
        /// <param name="left">The current CRC-8-CCITT value.</param>
        /// <param name="right">The new byte.</param>
        /// <returns>
        /// The next value of CRC8.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static FlacCrc8 operator *(FlacCrc8 left, ushort right) => left * (byte)(right >> 8) * (byte)right;

        /// <summary>
        /// Calculates the next value of CRC-16-IBM with <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public FlacCrc8 GenerateNext(byte value) => new(GetTableAt(state ^ value));

        /// <summary>
        /// Calculates the next value of CRC-8-CCITT with <paramref name="right"/>.
        /// </summary>
        /// <param name="left">The current CRC-8-CCITT value.</param>
        /// <param name="right">The new bytes.</param>
        /// <returns>
        /// The next value of CRC8.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static FlacCrc8 operator *(FlacCrc8 left, Span<byte> right)
        {
            var value = left;
            for (int i = 0; i < right.Length; i++)
            {
                value *= right[i];
            }
            return value;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the obj parameter; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object? obj) => obj is FlacCrc8 crc && Equals(crc);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(FlacCrc8 other) => state == other.state;

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        public override int GetHashCode() => HashCode.Combine(state);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="FlacCrc8"/> objects are equal.
        /// </summary>
        /// <param name="left">The first <see cref="FlacCrc8"/> to compare.</param>
        /// <param name="right">The second <see cref="FlacCrc8"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if the left is the same as the right; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator ==(FlacCrc8 left, FlacCrc8 right) => left.Equals(right);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="FlacCrc8"/> objects are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="FlacCrc8"/> to compare.</param>
        /// <param name="right">The second  <see cref="FlacCrc8"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if left and right are not equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator !=(FlacCrc8 left, FlacCrc8 right) => !(left == right);
    }
}
