using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.Intrinsics;
using System.Text;
using System.Threading.Channels;

using Shamisen.Data;
using Shamisen.Utils.Buffers;

namespace Shamisen.Data.Binary
{
    /// <summary>
    /// Represents a read-only, memory-efficient sequence of bits backed by immutable memory.
    /// </summary>
    /// <remarks>Use this struct to access and manipulate bits without modifying the underlying data. The
    /// buffer is backed by read-only memory, ensuring that the bit sequence cannot be changed after creation. This type
    /// is suitable for scenarios where bit-level access is required without the need for mutation, such as parsing
    /// binary data or implementing bit flags in a thread-safe manner.</remarks>
    public readonly partial struct ReadOnlyBitMemory : ISliceableBitRegion<ReadOnlyBitMemory, byte>
    {
        private const ulong BitsPerByte = 8;
        private const ulong BitsPerWord = BitsPerByte * sizeof(byte);
        private readonly byte start;
        private readonly byte lastWordInvalidBits;
        private readonly ReadOnlyMemory<byte> buffer;

        /// <inheritdoc/>
        public bool IsEmpty => buffer.IsEmpty;

        /// <inheritdoc/>
        public ulong BitLength => BitSpanUtils.CalculateBitLengthUnchecked<byte>(buffer.Length, start, lastWordInvalidBits);

        /// <inheritdoc/>
        public int BufferSize => buffer.Length;

        /// <inheritdoc/>
        public int FirstWordBitOffset => start;

        /// <summary>
        /// Gets the underlying buffer as a read-only sequence of 64-bit unsigned integers.
        /// </summary>
        public ReadOnlyMemory<byte> Buffer => buffer;

        /// <summary>
        /// Gets a read-only view of the bits in the current buffer as a contiguous span.
        /// </summary>
        /// <remarks>The returned span reflects the current state of the buffer and does not allow
        /// modification of its contents. Changes to the underlying buffer after retrieving the span may be reflected in
        /// the returned value.</remarks>
        public ReadOnlyBitSpan Span => new(buffer.Span, start, BitLength);

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyBitMemory"/> struct using the specified buffer of 64-bit words.
        /// </summary>
        /// <param name="buffer">A read-only memory region containing the underlying 64-bit words to be used as the bit storage.</param>
        public ReadOnlyBitMemory(ReadOnlyMemory<byte> buffer)
        {
            start = 0;
            lastWordInvalidBits = 0;
            this.buffer = buffer;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyBitMemory"/> struct using the specified buffer and starting bit
        /// offset.
        /// </summary>
        /// <remarks>The buffer is not copied; changes to the underlying memory (if any) after
        /// construction may affect the view. The start parameter allows for bit-level alignment within the first ulong
        /// of the buffer.</remarks>
        /// <param name="buffer">The underlying read-only memory buffer containing the bits to be represented.</param>
        /// <param name="start">The zero-based bit offset within the first element of the buffer at which the bit memory view begins.</param>
        public ReadOnlyBitMemory(ReadOnlyMemory<byte> buffer, ulong start)
        {
            this = new ReadOnlyBitMemory(buffer).Slice(start);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyBitMemory"/> structure using the specified buffer and bit range.
        /// </summary>
        /// <remarks>Use this constructor to create a read-only view over a sequence of bits stored in a
        /// ulong-based buffer. The combination of start and lastWordInvalidBits defines the valid bit range within the
        /// buffer.</remarks>
        /// <param name="buffer">The underlying read-only memory buffer containing the bits to be represented.</param>
        /// <param name="start">The zero-based index of the first valid bit in the buffer.</param>
        /// <param name="lastWordInvalidBits">The number of invalid bits in the last ulong of the buffer. Must be between 0 and 63, inclusive.</param>
        public ReadOnlyBitMemory(ReadOnlyMemory<byte> buffer, byte start, byte lastWordInvalidBits)
        {
            _ = BitSpanUtils.CalculateBitLengthChecked<byte>(buffer.Length, start, lastWordInvalidBits);
            this = new(start, lastWordInvalidBits, buffer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyBitMemory"/> structure using the specified buffer and bit range.
        /// </summary>
        /// <remarks>Use this constructor to create a read-only view over a sequence of bits stored in a
        /// ulong-based buffer. The combination of start and lastWordInvalidBits defines the valid bit range within the
        /// buffer.</remarks>
        /// <param name="start">The zero-based index of the first valid bit in the buffer.</param>
        /// <param name="lastWordInvalidBits">The number of invalid bits in the last ulong of the buffer. Must be between 0 and 63, inclusive.</param>
        /// <param name="buffer">The underlying read-only memory buffer containing the bits to be represented.</param>
        internal ReadOnlyBitMemory(byte start, byte lastWordInvalidBits, ReadOnlyMemory<byte> buffer)
        {
            this.start = start;
            this.lastWordInvalidBits = lastWordInvalidBits;
            this.buffer = buffer;
        }

        /// <inheritdoc/>
        public ReadOnlyBitMemory Slice(ulong bitOffset)
        {
            var newStart = start + bitOffset;
            var truncatedWords = newStart / BitsPerWord;
            newStart %= BitsPerWord;
            var newBuffer = buffer.Slice((int)truncatedWords);
            return new ReadOnlyBitMemory((byte)newStart, lastWordInvalidBits, newBuffer);
        }

        /// <inheritdoc/>
        public ReadOnlyBitMemory Slice(ulong bitOffset, ulong bitLength)
        {
            var intermediate = Slice(bitOffset);
            return intermediate.SliceWhileInternal(bitLength);
        }

        internal ReadOnlyBitMemory SliceWhileInternal(ulong bitLength)
        {
            var newEnd = start + bitLength;
            var newBufferLength = newEnd / BitsPerWord;
            ArgumentOutOfRangeException.ThrowIfGreaterThan(newBufferLength, (ulong)buffer.Length);
            var newBuffer = buffer.Slice(0, (int)newBufferLength);
            var newLastWordInvalidBits = (byte)(unchecked(~newEnd + 1ul) % BitsPerWord);
            return new ReadOnlyBitMemory(start, newLastWordInvalidBits, newBuffer);
        }

        /// <inheritdoc/>
        public ReadOnlyBitMemory SliceWordAligned() => SliceWordAlignedInternal();

        internal ReadOnlyBitMemory SliceWordAlignedInternal()
        {
            var newStart = start + (BitsPerWord - 1);
            var truncatedWords = newStart / BitsPerWord;
            var newBuffer = buffer.Slice((int)truncatedWords);
            return new ReadOnlyBitMemory(0, lastWordInvalidBits, newBuffer);
        }

        /// <inheritdoc/>
        public ReadOnlyBitMemory SliceByteAligned()
        {
            var newStart = (start + 7u) & ~7u;
            var truncatedWords = newStart / BitsPerWord;
            newStart %= (uint)BitsPerWord;
            var newBuffer = buffer.Slice((int)truncatedWords);
            return new ReadOnlyBitMemory((byte)newStart, lastWordInvalidBits, newBuffer);
        }
    }
}
