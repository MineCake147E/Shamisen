using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Shamisen.Data.Binary
{
    /// <summary>
    /// Defines a region of bits that supports creating slices based on bit offsets and lengths.
    /// </summary>
    /// <remarks>Implementations of this interface enable efficient manipulation and extraction of sub-regions
    /// within a bit-oriented data structure. Slicing operations do not modify the original instance but return a new
    /// instance representing the specified region.</remarks>
    /// <typeparam name="T">
    /// The type that implements the bit region and supports slicing operations. Must be a value type that implements
    /// <see cref="ISliceableBitRegion{T, TWord}"/> and allows ref struct semantics.
    /// </typeparam>
    /// <typeparam name="TWord">The type that the implementing type uses for operations and buffering.</typeparam>
    public interface ISliceableBitRegion<T, TWord>
        where T : struct, ISliceableBitRegion<T, TWord>, allows ref struct
        where TWord : unmanaged
    {
        /// <summary>
        /// Gets the number of bits contained in a single buffer word.
        /// </summary>
        static virtual int BitsPerBufferWord => T.BytesPerBufferWord * 8;
        /// <summary>
        /// Gets the number of bytes contained in a single buffer word (<typeparamref name="TWord"/>) for the implementing type.
        /// </summary>
        static virtual int BytesPerBufferWord => Unsafe.SizeOf<TWord>();

        /// <summary>
        /// Gets an instance of <typeparamref name="T"/> that represents an empty collection.
        /// </summary>
        static virtual T Empty => default;

        /// <summary>
        /// Gets a value indicating whether the collection contains no elements.
        /// </summary>
        bool IsEmpty { get; }

        /// <summary>
        /// Gets the length of the content, in bits.
        /// </summary>
        ulong BitLength { get; }

        /// <summary>
        /// Gets the length of the content, in bytes.
        /// </summary>
        ulong ByteLength => (ulong)BufferSize * (ulong)Unsafe.SizeOf<TWord>();

        /// <summary>
        /// Gets the size, in words, of the buffer used for data operations.
        /// </summary>
        int BufferSize { get; }

        /// <summary>
        /// Gets the bit offset of the first word within the underlying data structure, counting from MSB.
        /// </summary>
        int FirstWordBitOffset { get; }

        /// <summary>
        /// Returns a slice of the current instance starting at the specified bit offset.
        /// </summary>
        /// <param name="bitOffset">The zero-based bit offset at which to begin the slice. Must be non-negative and within the bounds of the
        /// current instance.</param>
        /// <returns>A new instance representing the slice that starts at the specified bit offset.</returns>
        T Slice(ulong bitOffset);

        /// <summary>
        /// Returns a slice of the current instance, starting at the specified bit offset and spanning the specified
        /// number of bits.
        /// </summary>
        /// <param name="bitOffset">The zero-based bit offset at which the slice begins. Must be non-negative and within the bounds of the
        /// current instance.</param>
        /// <param name="bitLength">The number of bits to include in the slice. Must be non-negative and the range defined by bitOffset and
        /// bitLength must not exceed the length of the current instance.</param>
        /// <returns>A new instance representing the specified slice of bits.</returns>
        T Slice(ulong bitOffset, ulong bitLength);

        /// <summary>
        /// Returns a slice of the underlying data starting at the current bit offset, aligned to the nearest word
        /// boundary.
        /// </summary>
        /// <remarks>The returned slice is guaranteed to start at a word-aligned position, which may
        /// improve performance for certain operations. If the bit offset does not correspond to a word boundary, the
        /// method aligns the slice to the next word boundary.</remarks>
        /// <returns>A slice of type T that begins at the current bit offset and is aligned to a word boundary.</returns>
        T SliceWordAligned();

        /// <summary>
        /// Returns a slice of the current data that is aligned to the nearest byte boundary.
        /// </summary>
        /// <returns>A value of type <typeparamref name="T"/> representing the byte-aligned slice of the data.</returns>
        T SliceByteAligned();
    }
}
