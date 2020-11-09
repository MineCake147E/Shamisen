using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace MonoAudio.Codecs.Waveform.Rf64
{
    /// <summary>
    /// Represents a chunk's size entry of RF64.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public readonly struct ChunkSizeTableEntry : IEquatable<ChunkSizeTableEntry>
    {
        /// <summary>
        /// The identifier of the corresponding chunk.
        /// </summary>
        [FieldOffset(0)]
        public readonly ChunkId Id;

        /// <summary>
        /// The chunk size.
        /// </summary>
        [FieldOffset(4)]
        public readonly ulong ChunkSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChunkSizeTableEntry"/> struct.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="chunkSize">Size of the chunk.</param>
        public ChunkSizeTableEntry(ChunkId id, ulong chunkSize) : this()
        {
            Id = id;
            ChunkSize = chunkSize;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the obj parameter; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj) => obj is ChunkSizeTableEntry entry && Equals(entry);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(ChunkSizeTableEntry other) => Id == other.Id && ChunkSize == other.ChunkSize;

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            int hashCode = 1414532747;
            hashCode = hashCode * -1521134295 + Id.GetHashCode();
            hashCode = hashCode * -1521134295 + ChunkSize.GetHashCode();
            return hashCode;
        }

        /// <summary>
        /// Indicates whether the values of two specified <see cref="ChunkSizeTableEntry"/> objects are equal.
        /// </summary>
        /// <param name="left">The first <see cref="ChunkSizeTableEntry"/> to compare.</param>
        /// <param name="right">The second <see cref="ChunkSizeTableEntry"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if the left is the same as the right; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator ==(ChunkSizeTableEntry left, ChunkSizeTableEntry right) => left.Equals(right);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="ChunkSizeTableEntry"/> objects are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="ChunkSizeTableEntry"/> to compare.</param>
        /// <param name="right">The second  <see cref="ChunkSizeTableEntry"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if left and right are not equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator !=(ChunkSizeTableEntry left, ChunkSizeTableEntry right) => !(left == right);
    }
}
