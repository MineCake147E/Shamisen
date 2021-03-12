using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Shamisen.Codecs.Composing;
using Shamisen.Codecs.Waveform.Composing;
using Shamisen.Codecs.Waveform.Rf64;

namespace Shamisen.Codecs.Waveform.Chunks
{
    /// <summary>
    /// Represents a "ds64" chunk(excluding <see cref="ChunkSizeTableEntry"/>).
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Pack = 1, Size = sizeof(uint) * 9)]
    public readonly struct Rf64DataSizeChunkHeader : IRf64Chunk, IEquatable<Rf64DataSizeChunkHeader>
    {
        [FieldOffset(0)]
        private readonly ChunkId chunkId;

        [FieldOffset(4)]
        private readonly uint chunkSize;

        [FieldOffset(8)]
        private readonly ulong riffChunkSize;

        [FieldOffset(16)]
        private readonly ulong dataSize;

        [FieldOffset(24)]
        private readonly ulong frameCount;

        [FieldOffset(32)]
        private readonly uint tableLength;

        /// <summary>
        /// Initializes a new instance of the <see cref="Rf64DataSizeChunkHeader"/> struct.
        /// </summary>
        /// <param name="riffSize">Size of the RF64 chunk.</param>
        /// <param name="dataSize">Size of the data chunk.</param>
        /// <param name="frameCount">The number of frames in the file.</param>
        /// <param name="tableLength">Length of the table of <see cref="ChunkSizeTableEntry"/>.</param>
        /// <param name="chunkId">The chunk identifier.</param>
        /// <param name="chunkSize">Size of the chunk.</param>
        public Rf64DataSizeChunkHeader(ulong riffSize, ulong dataSize, ulong frameCount, uint tableLength, ChunkId chunkId = ChunkId.Rf64DataSize, uint? chunkSize = null)
        {
            this.riffChunkSize = riffSize;
            this.dataSize = dataSize;
            this.frameCount = frameCount;
            this.tableLength = tableLength;
            this.chunkId = chunkId;
            this.chunkSize = chunkSize
                ?? (uint)Unsafe.SizeOf<Rf64DataSizeChunkHeader>() - sizeof(uint) * 2u + tableLength * (uint)Unsafe.SizeOf<ChunkSizeTableEntry>();
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
        public ulong ContentSize => (uint)Unsafe.SizeOf<Rf64DataSizeChunkHeader>() - sizeof(uint) * 2u;

        /// <summary>
        /// Gets the actual size.
        /// </summary>
        /// <value>
        /// The actual size.
        /// </value>
        public ulong ActualSize => (uint)Unsafe.SizeOf<Rf64DataSizeChunkHeader>();

        /// <summary>
        /// Gets the size of the riff.
        /// </summary>
        /// <value>
        /// The size of the riff.
        /// </value>
        public uint RiffSize => chunkSize;

        /// <summary>
        /// Gets the contents.
        /// </summary>
        /// <value>
        /// The contents.
        /// </value>
        IEnumerable<IRf64Content>? IRf64Chunk.Contents => new IRf64Content[] {
            BinaryContent.CreateLittleEndian(riffChunkSize),
            BinaryContent.CreateLittleEndian(dataSize),
            BinaryContent.CreateLittleEndian(frameCount),
            BinaryContent.CreateLittleEndian(tableLength)
        };

        /// <summary>
        /// Gets the size.
        /// </summary>
        /// <value>
        /// The size.
        /// </value>
        public ulong Size => ActualSize;

        /// <summary>
        /// Gets the size of the riff chunk.
        /// </summary>
        /// <value>
        /// The size of the riff chunk.
        /// </value>
        public ulong RiffChunkSize => riffChunkSize;

        /// <summary>
        /// Gets the size of the data chunk.
        /// </summary>
        /// <value>
        /// The size of the data chunk.
        /// </value>
        public ulong DataChunkSize => dataSize;

        /// <summary>
        /// Gets the frame count.
        /// </summary>
        /// <value>
        /// The frame count.
        /// </value>
        public ulong FrameCount => frameCount;

        /// <summary>
        /// Gets the length of the table of <see cref="ChunkSizeTableEntry"/>.
        /// </summary>
        /// <value>
        /// The length of the table of <see cref="ChunkSizeTableEntry"/>.
        /// </value>
        public uint TableLength => tableLength;

        /// <summary>
        /// Writes this <see cref="IComposable" /> instance to <see cref="IDataSink{TSample}" />.
        /// </summary>
        /// <param name="sink">The sink.</param>
        public void WriteTo(IDataSink<byte> sink)
        {
            Span<byte> buffer = stackalloc byte[Unsafe.SizeOf<Rf64DataSizeChunkHeader>()];
            Unsafe.As<byte, Rf64DataSizeChunkHeader>(ref MemoryMarshal.GetReference(buffer)) = BitConverter.IsLittleEndian
                ? this
                : (new(BinaryExtensions.ConvertToLittleEndian(riffChunkSize),
                BinaryExtensions.ConvertToLittleEndian(dataSize),
                BinaryExtensions.ConvertToLittleEndian(frameCount),
                BinaryExtensions.ConvertToLittleEndian(tableLength),
                (ChunkId)BinaryExtensions.ConvertToLittleEndian((uint)ChunkId),
                BinaryExtensions.ConvertToLittleEndian(chunkSize)));
            sink.Write(buffer);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the obj parameter; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object? obj) => obj is Rf64DataSizeChunkHeader header && Equals(header);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(Rf64DataSizeChunkHeader other) => chunkId == other.chunkId && chunkSize == other.chunkSize && riffChunkSize == other.riffChunkSize && dataSize == other.dataSize && frameCount == other.frameCount && tableLength == other.tableLength;

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode() => HashCode.Combine(chunkId, chunkSize, riffChunkSize, dataSize, frameCount, tableLength);

        /// <summary>
        /// Implements the operator op_Equality.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(Rf64DataSizeChunkHeader left, Rf64DataSizeChunkHeader right) => left.Equals(right);

        /// <summary>
        /// Implements the operator op_Inequality.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(Rf64DataSizeChunkHeader left, Rf64DataSizeChunkHeader right) => !(left == right);
    }
}
