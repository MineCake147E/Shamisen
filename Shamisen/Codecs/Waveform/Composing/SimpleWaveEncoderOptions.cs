using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Codecs.Waveform.Composing
{
    /// <summary>
    /// Represents an option for <see cref="SimpleWaveEncoder"/>.
    /// </summary>
    public readonly struct SimpleWaveEncoderOptions : IEquatable<SimpleWaveEncoderOptions>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleWaveEncoderOptions"/> struct.
        /// </summary>
        /// <param name="preferRiff">if set to <c>true</c> you prefer RIFF over RF64.</param>
        public SimpleWaveEncoderOptions(bool preferRiff) => PreferRiff = preferRiff;

        /// <summary>
        /// Gets a value indicating whether to prefer RIFF over RF64 or not.
        /// </summary>
        /// <value>
        ///   <c>true</c> if you prefer RIFF over RF64; otherwise, <c>false</c>.
        /// </value>
        public bool PreferRiff { get; }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the obj parameter; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object? obj) => obj is SimpleWaveEncoderOptions options && Equals(options);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(SimpleWaveEncoderOptions other) => PreferRiff == other.PreferRiff;

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode() => HashCode.Combine(PreferRiff);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="SimpleWaveEncoderOptions"/> objects are equal.
        /// </summary>
        /// <param name="left">The first <see cref="SimpleWaveEncoderOptions"/> to compare.</param>
        /// <param name="right">The second <see cref="SimpleWaveEncoderOptions"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if the left is the same as the right; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator ==(SimpleWaveEncoderOptions left, SimpleWaveEncoderOptions right) => left.Equals(right);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="SimpleWaveEncoderOptions"/> objects are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="SimpleWaveEncoderOptions"/> to compare.</param>
        /// <param name="right">The second  <see cref="SimpleWaveEncoderOptions"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if left and right are not equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator !=(SimpleWaveEncoderOptions left, SimpleWaveEncoderOptions right) => !(left == right);
    }
}
