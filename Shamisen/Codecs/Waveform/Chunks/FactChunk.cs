using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Shamisen.Codecs.Composing;

namespace Shamisen.Codecs.Waveform.Composing
{
    /// <summary>
    /// Represents a "fact" chunk.
    /// </summary>
    /// <seealso cref="IRf64Chunk" />
    [StructLayout(LayoutKind.Explicit)]
    public readonly struct FactChunk : IRf64Chunk, IEquatable<FactChunk>
    {
        [FieldOffset(0)]
        private readonly ChunkId chunkId;

        [FieldOffset(4)]
        private readonly uint riffSize;

        [FieldOffset(8)]
        private readonly uint sampleCountInFrames;

        /// <summary>
        /// Initializes a new instance of the <see cref="FactChunk"/> struct.
        /// </summary>
        /// <param name="sampleCountInFrames">The sample count in frames.</param>
        /// <param name="chunkId">The chunk identifier.</param>
        /// <param name="riffSize">Size of the riff.</param>
        public FactChunk(uint sampleCountInFrames, ChunkId chunkId = ChunkId.Fact, uint riffSize = 4u)
        {
            this.chunkId = chunkId;
            this.riffSize = riffSize;
            this.sampleCountInFrames = sampleCountInFrames;
        }

        /// <summary>
        /// Gets the chunk identifier.
        /// </summary>
        /// <value>
        /// The chunk identifier.
        /// </value>
        public ChunkId ChunkId => chunkId;

        /// <summary>
        /// Gets the size of the content.
        /// </summary>
        /// <value>
        /// The size of the content.
        /// </value>
        public ulong ContentSize => sizeof(uint);

        /// <summary>
        /// Gets the actual size.
        /// </summary>
        /// <value>
        /// The actual size.
        /// </value>
        public ulong ActualSize => sizeof(uint) * 3;

        /// <summary>
        /// Gets the size of the riff.
        /// </summary>
        /// <value>
        /// The size of the riff.
        /// </value>
        public uint RiffSize => sizeof(uint);

        /// <summary>
        /// Gets the contents.
        /// </summary>
        /// <value>
        /// The contents.
        /// </value>
        IEnumerable<IRf64Content>? IRf64Chunk.Contents => new IRf64Content[] { BinaryContent.CreateLittleEndian(sampleCountInFrames) };

        /// <summary>
        /// Gets the size.
        /// </summary>
        /// <value>
        /// The size.
        /// </value>
        public ulong Size => ActualSize;

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the obj parameter; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object? obj) => obj is FactChunk chunk && Equals(chunk);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(FactChunk other) => chunkId == other.chunkId && riffSize == other.riffSize && sampleCountInFrames == other.sampleCountInFrames;

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode() => HashCode.Combine(chunkId, riffSize, sampleCountInFrames);

        /// <summary>
        /// Writes this <see cref="IComposable" /> instance to <see cref="IDataSink{TSample}" />.
        /// </summary>
        /// <param name="sink">The sink.</param>
        public void WriteTo(IDataSink<byte> sink)
        {
            Span<byte> buffer = stackalloc byte[sizeof(uint) * 3];
            Unsafe.As<byte, FactChunk>(ref MemoryMarshal.GetReference(buffer)) = BitConverter.IsLittleEndian
                ? this
                : (new(BinaryExtensions.ConvertToLittleEndian(sampleCountInFrames),
                (ChunkId)BinaryExtensions.ConvertToLittleEndian((uint)ChunkId),
                BinaryExtensions.ConvertToLittleEndian(RiffSize)));
            sink.Write(buffer);
        }

        /// <summary>
        /// Implements the operator op_Equality.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(FactChunk left, FactChunk right) => left.Equals(right);

        /// <summary>
        /// Implements the operator op_Inequality.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(FactChunk left, FactChunk right) => !(left == right);
    }
}
