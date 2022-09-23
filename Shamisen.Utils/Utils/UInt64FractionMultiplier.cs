using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using DivideSharp;

using Shamisen.Mathematics;

namespace Shamisen.Utils
{
    /// <summary>
    /// Represents the fraction with two <see cref="ulong"/> parts.
    /// Optimized for multiplying with <see cref="ulong"/> numbers.
    /// </summary>
    public readonly struct UInt64FractionMultiplier : IEquatable<UInt64FractionMultiplier>
    {
        private readonly UInt64Divisor divisor;
        private readonly ulong highQuotient;
        private readonly ulong highRemainder;

        /// <summary>
        /// Gets the numerator of this <see cref="UInt64FractionMultiplier"/>.
        /// </summary>
        public ulong Numerator { get; }

        /// <summary>
        /// Gets the denominator of this <see cref="UInt64FractionMultiplier"/>.
        /// </summary>
        public ulong Denominator => divisor.Divisor;

        /// <summary>
        /// Gets the maximum input number that does not overflow after multiplication.
        /// </summary>
        public ulong MaxNoOverflowInput { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UInt64FractionMultiplier"/> struct.
        /// </summary>
        /// <param name="divisor">The divisor.</param>
        /// <param name="multiplier">The multiplier.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public UInt64FractionMultiplier(UInt64Divisor divisor, ulong multiplier)
        {
            (var m, var d) = MathHelper.MinimizeDivisor(multiplier, divisor.Divisor);
            if (d != divisor.Divisor) divisor = new UInt64Divisor(d);
            this.divisor = divisor;
            Numerator = m;
            highQuotient = MathI.ReciprocalUInt64(divisor, out highRemainder);
            MaxNoOverflowInput = CalculateMaxNoOverflowInput(m, d);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UInt64FractionMultiplier"/> struct.
        /// </summary>
        /// <param name="divisor">The divisor.</param>
        /// <param name="multiplier">The multiplier.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public UInt64FractionMultiplier(ulong divisor, ulong multiplier)
        {
            (var m, var d) = MathHelper.MinimizeDivisor(multiplier, divisor);
            var dd = this.divisor = new UInt64Divisor(d);
            Numerator = m;
            highQuotient = MathI.ReciprocalUInt64(dd, out highRemainder);
            MaxNoOverflowInput = CalculateMaxNoOverflowInput(m, d);
        }

        private static ulong CalculateMaxNoOverflowInput(ulong m, ulong d)
        {
            var mnoi = 0ul;
            if (m > d)
            {
                var y = MathI.ReciprocalUInt64(m, out var rem);
                y *= d;
                rem *= d;
                y += rem / m;
                mnoi = y;
            }
            else
            {
                mnoi = ulong.MaxValue;
            }

            return mnoi;
        }

        /// <inheritdoc/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public override bool Equals(object? obj) => obj is UInt64FractionMultiplier fraction && Equals(fraction);
        /// <inheritdoc/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public bool Equals(UInt64FractionMultiplier other) => Numerator == other.Numerator && divisor.Equals(other.divisor);
        /// <inheritdoc/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public override int GetHashCode() => HashCode.Combine(Numerator, divisor);

        /// <summary>
        /// Multiplies this <see cref="UInt64FractionMultiplier"/> with <paramref name="right"/>.
        /// </summary>
        /// <param name="right">The second value to multiply.</param>
        /// <returns>
        /// The product of <paramref name="right"/> and this <see cref="UInt64FractionMultiplier"/>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public BigInteger BigMultiply(ulong right)
        {
            BigInteger m = Numerator;
            BigInteger d = divisor.Divisor;
            m *= right;
            return m / d;
        }

        #region Operator Overloads

        #region Multiplication

        /// <summary>
        /// Multiplies <paramref name="left"/> with <paramref name="right"/>.
        /// </summary>
        /// <param name="left">The first value to multiply.</param>
        /// <param name="right">The second value to multiply.</param>
        /// <returns>
        /// The product of <paramref name="left"/> and <paramref name="right"/>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static ulong operator *(UInt64FractionMultiplier left, ulong right)
        {
            var m = left.Numerator;
            var d = left.divisor;
            var hr = left.highRemainder;
            var hq = left.highQuotient;
            var high = Math.BigMul(m, right, out var low);
            var lowR = d.DivRem(low, out var lowQ);
            lowR += high * hr;
            lowQ += high * hq;
            lowQ += lowR / d;
            return lowQ;
        }
        #endregion

        #region Equality
        /// <summary>
        /// Indicates whether the values of two specified <see cref="UInt64FractionMultiplier"/> objects are equal.
        /// </summary>
        /// <param name="left">The first <see cref="UInt64FractionMultiplier"/> to compare.</param>
        /// <param name="right">The second <see cref="UInt64FractionMultiplier"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if the value of <paramref name="left"/> is the same as the value of <paramref name="right"/>; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool operator ==(UInt64FractionMultiplier left, UInt64FractionMultiplier right) => left.Equals(right);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="UInt64FractionMultiplier"/> objects are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="UInt64FractionMultiplier"/> to compare.</param>
        /// <param name="right">The second <see cref="UInt64FractionMultiplier"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if the value of <paramref name="left"/> is the same as the value of <paramref name="right"/>; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool operator !=(UInt64FractionMultiplier left, UInt64FractionMultiplier right) => !(left == right);
        #endregion

        #endregion
    }
}
