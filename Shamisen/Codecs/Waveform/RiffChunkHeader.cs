using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

using Shamisen.Data;
using Shamisen.Data.Binary;

namespace Shamisen.Codecs.Waveform
{
    /// <summary>
    /// Represents a header of RIFF chunks.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public readonly struct RiffChunkHeader
    {
        [FieldOffset(0)]
        private readonly ChunkId chunkId;

        [FieldOffset(4)]
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
    }
}
