using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Number = System.UInt64;

namespace MonoAudio
{
    /// <summary>
    /// Represents a length sample of <see cref="IAudioSource{TSample, TFormat}"/>.
    /// </summary>
    [Obsolete("Replacing!", true)]
    public readonly struct SourceLength : IEquatable<SourceLength>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SourceLength"/> struct.
        /// </summary>
        /// <param name="length">The length.</param>
        /// <param name="totalLength">The total length.</param>
        /// <param name="position">The position.</param>
        public SourceLength(Number length, Number totalLength, Number position)
        {
            Length = length;
            TotalLength = totalLength;
            Position = position;
        }

        /// <summary>
        /// Gets the remaining length in the <see cref="IAudioSource{TSample, TFormat}"/> in frames.
        /// </summary>
        /// <value>
        /// The remaining length in the <see cref="IAudioSource{TSample, TFormat}"/> in frames.
        /// </value>
        public Number Length { get; }

        /// <summary>
        /// Gets the total length of the <see cref="IAudioSource{TSample, TFormat}" /> in frames.
        /// </summary>
        /// <value>
        /// The total length of the <see cref="IAudioSource{TSample, TFormat}" /> in frames.
        /// </value>
        public Number TotalLength { get; }

        /// <summary>
        /// Gets the position of the <see cref="IAudioSource{TSample, TFormat}" /> in frames.
        /// </summary>
        /// <value>
        /// The position of the <see cref="IAudioSource{TSample, TFormat}" /> in frames.
        /// </value>
        public Number Position { get; }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the obj parameter; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object? obj) => obj is SourceLength length && Equals(length);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(SourceLength other) => Length.Equals(other.Length) && TotalLength.Equals(other.TotalLength) && Position.Equals(other.Position);

#if NET5_0 || NETCOREAPP3_1

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode() => HashCode.Combine(Length, TotalLength, Position);

#else
        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            int hashCode = 2044254804;
            hashCode = hashCode * -1521134295 + Length.GetHashCode();
            hashCode = hashCode * -1521134295 + TotalLength.GetHashCode();
            hashCode = hashCode * -1521134295 + Position.GetHashCode();
            return hashCode;
        }
#endif

        /// <summary>
        /// Indicates whether the values of two specified <see cref="SourceLength"/> objects are equal.
        /// </summary>
        /// <param name="left">The first <see cref="SourceLength"/> to compare.</param>
        /// <param name="right">The second <see cref="SourceLength"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if the left is the same as the right; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator ==(SourceLength left, SourceLength right) => left.Equals(right);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="SourceLength"/> objects are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="SourceLength"/> to compare.</param>
        /// <param name="right">The second  <see cref="SourceLength"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if left and right are not equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator !=(SourceLength left, SourceLength right) => !(left == right);
    }
}
