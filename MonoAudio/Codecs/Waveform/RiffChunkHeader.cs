using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace MonoAudio.Codecs.Waveform
{
    /// <summary>
    /// Represents a header of RIFF chunks.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct RiffChunkHeader : IEndiannessReversible<RiffChunkHeader>
    {
        private readonly ChunkId chunkId;
        private readonly uint length;

        /// <summary>
        /// Initializes a new instance of the <see cref="RiffChunkHeader"/> struct.
        /// </summary>
        /// <param name="chunkId">The chunk identifier.</param>
        /// <param name="length">The length.</param>
        public RiffChunkHeader(ChunkId chunkId, uint length)
        {
            this.chunkId = chunkId;
            this.length = length;
        }

        /// <summary>
        /// Gets or sets the chunk identifier.
        /// </summary>
        /// <value>
        /// The chunk identifier.
        /// </value>
        public ChunkId ChunkId => chunkId;

        /// <summary>
        /// Gets or sets the length of this chunk.
        /// </summary>
        /// <value>
        /// The length of this chunk.
        /// </value>
        public uint Length => length;

        /// <summary>
        /// Reverses endianness for all fields, and returns a new value.
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RiffChunkHeader ReverseEndianness()
        {
            var ckId = (ChunkId)BinaryPrimitives.ReverseEndianness((uint)chunkId);
            var ckLen = BinaryPrimitives.ReverseEndianness(length);
            return new RiffChunkHeader(ckId, ckLen);
        }
    }
}
