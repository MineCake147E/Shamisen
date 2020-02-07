using System;
using System.Collections.Generic;
using System.Text;

namespace MonoAudio.Codecs.Waveform.Chunks
{
    /// <summary>
    /// Represents an RIFF chunk.
    /// </summary>
    public readonly struct RiffChunk : IEquatable<RiffChunk>
    {
        /// <summary>
        /// The header
        /// </summary>
        public readonly RiffChunkHeader Header;

        /// <summary>
        /// The wave identifier
        /// </summary>
        public readonly uint WaveId;

        /// <summary>
        /// Initializes a new instance of the <see cref="RiffChunk"/> struct.
        /// </summary>
        /// <param name="header">The header.</param>
        /// <param name="waveId">The wave identifier.</param>
        public RiffChunk(RiffChunkHeader header, uint waveId)
        {
            Header = header;
            WaveId = waveId;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the obj parameter; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj) => obj is RiffChunk chunk && Equals(chunk);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(RiffChunk other) => EqualityComparer<RiffChunkHeader>.Default.Equals(Header, other.Header) && WaveId == other.WaveId;

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            var hashCode = -702696999;
            hashCode = hashCode * -1521134295 + EqualityComparer<RiffChunkHeader>.Default.GetHashCode(Header);
            hashCode = hashCode * -1521134295 + WaveId.GetHashCode();
            return hashCode;
        }

        /// <summary>
        /// Indicates whether the values of two specified <see cref="RiffChunk"/> objects are equal.
        /// </summary>
        /// <param name="left">The first <see cref="RiffChunk"/> to compare.</param>
        /// <param name="right">The second <see cref="RiffChunk"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if the left is the same as the right; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator ==(RiffChunk left, RiffChunk right) => left.Equals(right);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="RiffChunk"/> objects are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="RiffChunk"/> to compare.</param>
        /// <param name="right">The second  <see cref="RiffChunk"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if left and right are not equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator !=(RiffChunk left, RiffChunk right) => !(left == right);
    }
}
