using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Shamisen.Data.Binary
{
    /// <summary>
    /// Represents a read-only contiguous region of bits, providing efficient access and manipulation without copying
    /// the underlying data.
    /// </summary>
    /// <remarks>Use this type to perform bitwise operations or queries on a sequence of bits without
    /// modifying the source data. The lifetime of the underlying data must outlive the span; do not store instances
    /// beyond the scope in which they are created. This type is a ref struct and therefore cannot be boxed, assigned to
    /// variables of type object, or captured by lambda expressions.</remarks>
    [StructLayout(LayoutKind.Sequential)]
    public readonly ref partial struct ReadOnlyBitSpan : ISliceableBitRegion<ReadOnlyBitSpan, byte>
    {
        private const ulong BitsPerByte = 8;
        private const ulong BitsPerWord = BitsPerByte * sizeof(byte);
        private readonly byte startOffset;
        private readonly ref readonly byte head;
        private readonly ulong bitLength;

        /// <inheritdoc/>
        public bool IsEmpty => bitLength == 0;

        /// <inheritdoc/>
        public ulong BitLength => bitLength;

        /// <inheritdoc/>
        public int BufferSize => (int)((bitLength + startOffset + BitsPerWord - 1) / BitsPerWord);

        /// <inheritdoc/>
        public int FirstWordBitOffset => startOffset;

        /// <inheritdoc/>
        public ulong ByteLength => (bitLength + BitsPerByte - 1) / BitsPerByte;

        /// <summary>
        /// Gets a read-only span that provides access to the underlying buffer of 64-bit unsigned integers.<br/>
        /// The first bit of a complete word is at MSB (most significant bit).
        /// </summary>
        public ReadOnlySpan<byte> Buffer => MemoryMarshal.CreateReadOnlySpan(in head, BufferSize);

        /// <summary>
        /// Gets the value of the bit at the specified index within the bit sequence.
        /// </summary>
        /// <param name="bitIndex">The zero-based index of the bit to retrieve. Must be less than the total number of bits in the sequence.</param>
        /// <returns>A byte value representing the bit at the specified index. The value is 1 if the bit is set; otherwise, 0.</returns>
        public byte this[ulong bitIndex]
        {
            get
            {
                var absoluteBitIndex = startOffset + bitIndex;
                ref var localHead = ref Unsafe.AsRef(in head);
                var wordIndex = (nuint)(absoluteBitIndex / BitsPerWord);
                ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(bitIndex, bitLength);
                var bitOffset = (byte)absoluteBitIndex;
                var word = (long)Unsafe.Add(ref localHead, wordIndex) << -8;
                return Unsafe.BitCast<bool, byte>(word << bitOffset < 0);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyBitSpan"/> struct using the specified buffer of 64-bit words.
        /// </summary>
        /// <param name="buffer">A read-only memory region containing the underlying 64-bit words to be used as the bit storage.</param>
        public ReadOnlyBitSpan(ReadOnlySpan<byte> buffer)
        {
            startOffset = 0;
            head = ref MemoryMarshal.GetReference(buffer);
            bitLength = (ulong)buffer.Length * BitsPerWord;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyBitSpan"/> struct using the specified buffer and starting bit
        /// offset.
        /// </summary>
        /// <remarks>The buffer is not copied; changes to the underlying memory (if any) after
        /// construction may affect the view. The start parameter allows for bit-level alignment within the first ulong
        /// of the buffer.</remarks>
        /// <param name="buffer">The underlying read-only memory buffer containing the bits to be represented.</param>
        /// <param name="start">The zero-based bit offset within the first element of the buffer at which the bit memory view begins. Must
        /// be between 0 and 63, inclusive.</param>
        public ReadOnlyBitSpan(ReadOnlySpan<byte> buffer, ulong start)
        {
            this = new ReadOnlyBitSpan(buffer).Slice(start);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyBitSpan"/> structure using the specified buffer and bit range.
        /// </summary>
        /// <remarks>Use this constructor to create a read-only view over a sequence of bits stored in a
        /// ulong-based buffer. The combination of start and lastWordInvalidBits defines the valid bit range within the
        /// buffer.</remarks>
        /// <param name="buffer">The underlying read-only memory buffer containing the bits to be represented.</param>
        /// <param name="start">The zero-based index of the first valid bit in the buffer.</param>
        /// <param name="bitLength">The number of bits to be visible.</param>
        public ReadOnlyBitSpan(ReadOnlySpan<byte> buffer, ulong start, ulong bitLength)
        {
            this = new ReadOnlyBitSpan(buffer).Slice(start, bitLength);
        }

        /// <summary>
        /// Initializes a new instance of the ReadOnlyBitSpan structure that represents a sequence of bits starting at
        /// the specified offset within the provided data.
        /// </summary>
        /// <param name="startOffset">The zero-based bit offset at which the span begins within the referenced data.</param>
        /// <param name="head">A reference to the first 64 bits of the underlying data from which the bit span is created. The referenced
        /// value is not modified.</param>
        /// <param name="bitLength">The number of bits to include in the span, starting from the specified offset. Must be less than or equal to
        /// the number of available bits in the referenced data.</param>
        internal ReadOnlyBitSpan(byte startOffset, ref readonly byte head, ulong bitLength)
        {
            this.startOffset = startOffset;
            this.head = ref head;
            this.bitLength = bitLength;
        }

        /// <inheritdoc/>
        public ReadOnlyBitSpan Slice(ulong bitOffset)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(bitOffset, bitLength);
            var newStart = startOffset + bitOffset;
            var truncatedWords = newStart / BitsPerWord;
            newStart %= BitsPerWord;
            ref var newHead = ref Unsafe.Add(ref Unsafe.AsRef(in head), (nuint)truncatedWords);
            var newLength = bitLength - bitOffset;
            return new ReadOnlyBitSpan((byte)newStart, in newHead, newLength);
        }

        /// <inheritdoc/>
        public ReadOnlyBitSpan Slice(ulong bitOffset, ulong bitLength)
        {
            var intermediate = Slice(bitOffset);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(bitLength, intermediate.bitLength);
            return intermediate.SliceWhileInternal(bitLength);
        }

        internal ReadOnlyBitSpan SliceWhileInternal(ulong bitLength)
        {
            var newStart = startOffset;
            ref var newHead = ref Unsafe.AsRef(in head);
            return new ReadOnlyBitSpan(newStart, in newHead, bitLength);
        }

        /// <inheritdoc/>
        public ReadOnlyBitSpan SliceWordAligned() => SliceWordAlignedInternal();

        internal ReadOnlyBitSpan SliceWordAlignedInternal()
        {
            var localStartOffset = startOffset;
            var newStart = (localStartOffset + BitsPerWord - 1) & ~(BitsPerWord - 1);
            var bitOffset = newStart - localStartOffset;
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(bitOffset, bitLength);
            var truncatedWords = newStart / BitsPerWord;
            newStart %= BitsPerWord;
            ref var newHead = ref Unsafe.Add(ref Unsafe.AsRef(in head), (nuint)truncatedWords);
            var newLength = bitLength - bitOffset;
            return new ReadOnlyBitSpan((byte)newStart, in newHead, newLength);
        }

        /// <inheritdoc/>
        public ReadOnlyBitSpan SliceByteAligned()
        {
            var localStartOffset = startOffset;
            var newStart = (localStartOffset + BitsPerByte - 1ul) & ~(BitsPerByte - 1ul);
            var bitOffset = newStart - localStartOffset;
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(bitOffset, bitLength);
            var truncatedWords = newStart / BitsPerWord;
            newStart %= BitsPerWord;
            ref var newHead = ref Unsafe.Add(ref Unsafe.AsRef(in head), (nuint)truncatedWords);
            var newLength = bitLength - bitOffset;
            return new ReadOnlyBitSpan((byte)newStart, in newHead, newLength);
        }
    }
}
