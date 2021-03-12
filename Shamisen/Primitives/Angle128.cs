using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Primitives
{
    /// <summary>
    /// Represents an angle in 128-bit mixed-point value.<br/>
    /// Can be used for some extremely-high-precision needs of angle and frequency.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = sizeof(long) * 2)]
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public readonly struct Angle128 : IEquatable<Angle128>
    {
        [FieldOffset(0)]
        private readonly Fixed64 high;

        /// <summary>
        /// The lower part.<br/>
        /// Must be between 0 and 1.
        /// </summary>
        [FieldOffset(sizeof(ulong))]
        private readonly double low;

        /// <summary>
        /// Initializes a new instance of the <see cref="Angle128"/> struct.
        /// </summary>
        /// <param name="sampleRate">The sample rate.</param>
        /// <param name="frequency">The frequency.</param>
        public Angle128(int sampleRate, double frequency) : this(2 * frequency / sampleRate)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Angle128"/> struct.
        /// </summary>
        /// <param name="multiplesOfPi">The multiples of pi.</param>
        public Angle128(double multiplesOfPi)
        {
            double v = multiplesOfPi * 9.223372036854776E+18f;
            high = new Fixed64((long)Math.Floor(v));
            low = v - high.Value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Angle128"/> struct.
        /// </summary>
        /// <param name="high">The high part.</param>
        /// <param name="low">The low part.</param>
        public Angle128(Fixed64 high, double low)
        {
            double carry = Math.Floor(low);
            this.high = high + new Fixed64((long)carry);
            this.low = low - carry;
        }

        #region Values

        /// <summary>
        /// Gets the higher part of this <see cref="Angle128"/> value.
        /// </summary>
        /// <value>
        /// The higher part of this <see cref="Angle128"/> value.
        /// </value>
        public Fixed64 High => high;

        /// <summary>
        /// Gets the lower part of this <see cref="Angle128"/> value.
        /// </summary>
        /// <value>
        /// The lower part of this <see cref="Angle128"/> value.
        /// </value>
        public double Low => low;

        /// <summary>
        /// Gets the value in multiple of pi of this <see cref="Angle128"/> value.
        /// </summary>
        /// <value>
        /// The value in multiple of pi of this <see cref="Angle128"/> value.
        /// </value>
        public double Value => high.DoubleValue + low;

        /// <summary>
        /// Gets the value in turns.
        /// </summary>
        /// <value>
        /// The value in turns.
        /// </value>
        public double ValueInTurns => Value * 0.5;

        /// <summary>
        /// Gets the value in radians.
        /// </summary>
        /// <value>
        /// The value in radians.
        /// </value>
        public double ValueInRadians => Value * Math.PI;

        /// <summary>
        /// Gets the value in degrees.
        /// </summary>
        /// <value>
        /// The value in degrees.
        /// </value>
        public double ValueInDegrees => Value * 180;

        #endregion Values

        #region Arithmetics

        /// <summary>
        /// Adds specified <see cref="Angle128"/> value and <see cref="Angle128"/> value.
        /// </summary>
        /// <param name="left">The first value to add.</param>
        /// <param name="right">The second value to add.</param>
        /// <returns>
        /// The result of adding <paramref name="left"/> and <paramref name="right"/>.
        /// </returns>
        public static Angle128 operator +(Angle128 left, Angle128 right)
        {
            var h = left.high + right.high;
            var l = left.low + right.low;
            return new Angle128(h, l);
        }

        /// <summary>
        /// Subtracts specified <see cref="Angle128"/> value from <see cref="Angle128"/> value.
        /// </summary>
        /// <param name="left">The value to be subtract from <paramref name="left"/>.</param>
        /// <returns>
        /// The result of subtracting <paramref name="left"/> and <paramref name="right"/>.
        /// </returns>
        public static Angle128 operator -(Angle128 left, Angle128 right)
        {
            var h = left.high - right.high;
            var l = left.low - right.low;
            return new Angle128(h, l);
        }

        #endregion Arithmetics

        private string GetDebuggerDisplay() => $"{ValueInRadians} radians({high.Value}; {low:R})";

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// The value in string.
        /// </returns>
        public override string? ToString() => GetDebuggerDisplay();

        #region Equality

        /// <summary>
        /// Indicates whether the values of two specified <see cref="Angle128"/> objects are equal.
        /// </summary>
        /// <param name="left">The first <see cref="Angle128"/> to compare.</param>
        /// <param name="right">The second <see cref="Angle128"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if the left is the same as the right; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator ==(Angle128 left, Angle128 right) => left.Equals(right);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="Angle128"/> objects are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="Angle128"/> to compare.</param>
        /// <param name="right">The second  <see cref="Angle128"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if left and right are not equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator !=(Angle128 left, Angle128 right) => !(left == right);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the obj parameter; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object? obj) => obj is Angle128 angle && Equals(angle);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(Angle128 other) => high.Equals(other.high) && low == other.low;

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        public override int GetHashCode() => HashCode.Combine(high, low);

        #endregion Equality
    }
}
