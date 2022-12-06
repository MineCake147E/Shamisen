using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Shamisen.Utils;

namespace Shamisen
{
    /// <typeparam name="T">The type of items in the <see cref="ReadOnlyNativeSpan{T}"/>.</typeparam>
    /// <inheritdoc cref="ReadOnlySpan{T}"/>
    [StructLayout(LayoutKind.Sequential)]
    public readonly ref struct ReadOnlyNativeSpan<T>
    {
        private readonly ref T head;

        /// <summary>
        /// The length of this <see cref="ReadOnlyNativeSpan{T}"/>.
        /// </summary>
        public nint Length
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get;
        }

        internal ref T Head => ref head;
        internal ref T Tail => ref Unsafe.Add(ref head, Length - 1);

        /// <summary>
        /// Returns a value that indicates whether the current <see cref="ReadOnlyNativeSpan{T}"/> is empty.
        /// </summary>
        /// <inheritdoc cref="ReadOnlySpan{T}.IsEmpty"/>
        public bool IsEmpty
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Length <= 0;
        }

        /// <summary>
        /// Creates a new <see cref="ReadOnlySpan{T}"/> object that represents the current <see cref="ReadOnlyNativeSpan{T}"/>.
        /// </summary>
        public ReadOnlySpan<T> GetHeadReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(ref head, (int)MathI.Min(int.MaxValue, Length));

        /// <summary>
        /// Returns a value that indicates whether the current <see cref="ReadOnlyNativeSpan{T}"/>'s contents can fit into <see cref="ReadOnlySpan{T}"/>.
        /// </summary>
        public bool FitsInReadOnlySpan => Length <= int.MaxValue;

        /// <summary>
        /// Returns an empty <see cref="ReadOnlyNativeSpan{T}"/> object.
        /// </summary>
        /// <value>An empty <see cref="ReadOnlyNativeSpan{T}"/> object.</value>
        public static ReadOnlyNativeSpan<T> Empty
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => default;
        }

        #region Constructors
        /// <summary>
        /// Creates a new <see cref="ReadOnlyNativeSpan{T}"/> object over the entirety of a specified <paramref name="span"/>.
        /// </summary>
        /// <param name="span">The memory region from which to create the <see cref="ReadOnlyNativeSpan{T}"/> object.</param>
        /// <remarks>If <paramref name="span"/> is null, this constructor returns a <see langword="null"/> <see cref="ReadOnlyNativeSpan{T}"/>.</remarks>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public ReadOnlyNativeSpan(NativeSpan<T> span)
        {
            if (span.IsEmpty)
            {
                this = default;
                return;
            }
            this = new(ref SpanUtils.GetReference(span), span.Length);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyNativeSpan{T}"/> struct.
        /// </summary>
        /// <param name="headPointer">The head pointer.</param>
        /// <param name="length">The length.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public unsafe ReadOnlyNativeSpan(ref T headPointer, nint length)
        {
            CheckLength(length);
            head = ref headPointer;
            Length = length;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyNativeSpan{T}"/> struct.
        /// </summary>
        /// <param name="headPointer">The head pointer.</param>
        /// <param name="length">The length.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public unsafe ReadOnlyNativeSpan(void* headPointer, nint length)
        {
            _ = new ReadOnlySpan<T>(headPointer, 0);
            CheckLength(length);
            head = ref Unsafe.AsRef<T>(headPointer);
            Length = length;
        }

        /// <summary>
        /// Creates a new <see cref="ReadOnlyNativeSpan{T}"/> object over the entirety of a specified <paramref name="array"/>.
        /// </summary>
        /// <param name="array">The array from which to create the <see cref="ReadOnlyNativeSpan{T}"/> object.</param>
        /// <exception cref="ArrayTypeMismatchException"><typeparamref name="T"/> is a reference type, and <paramref name="array"/> is not an array of type <typeparamref name="T"/>.</exception>
        /// <remarks>If <paramref name="array"/> is null, this constructor returns a <see langword="null"/> <see cref="ReadOnlyNativeSpan{T}"/>.</remarks>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public ReadOnlyNativeSpan(T[]? array)
        {
            if (array is null)
            {
                this = default;
                return;
            }
            CheckArrayTypeMismatch(array);
            head = ref MemoryMarshal.GetArrayDataReference(array);
            Length = (nint)array.LongLength;
        }

        /// <summary>
        /// Creates a new <see cref="ReadOnlyNativeSpan{T}"/> object over the entirety of a specified <paramref name="array"/>.
        /// </summary>
        /// <param name="array">The array from which to create the <see cref="ReadOnlyNativeSpan{T}"/> object.</param>
        /// <exception cref="ArrayTypeMismatchException"><typeparamref name="T"/> is a reference type, and <paramref name="array"/> is not an array of type <typeparamref name="T"/>.</exception>
        /// <remarks>If <paramref name="array"/> is null, this constructor returns a <see langword="null"/> <see cref="ReadOnlyNativeSpan{T}"/>.</remarks>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public ReadOnlyNativeSpan(ArraySegment<T> array) : this(array.Array, array.Offset, array.Count)
        {
        }

        /// <summary>
        /// Creates a new <see cref="ReadOnlyNativeSpan{T}"/> object that includes a specified number of elements of an array starting at a specified index.
        /// </summary>
        /// <param name="array">The source array.</param>
        /// <param name="start">The index of the first element to include in the new <see cref="ReadOnlyNativeSpan{T}"/>.</param>
        /// <param name="length">The number of elements to include in the new <see cref="ReadOnlyNativeSpan{T}"/>.</param>
        /// <exception cref="ArrayTypeMismatchException"><typeparamref name="T"/> is a reference type, and <paramref name="array"/> is not an array of type <typeparamref name="T"/>.</exception>
        /// <remarks>If <paramref name="array"/> is null, this constructor returns a <see langword="null"/> <see cref="ReadOnlyNativeSpan{T}"/>.</remarks>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public ReadOnlyNativeSpan(T[]? array, nint start, nint length)
        {
            if (array is null)
            {
                this = default;
                return;
            }
            CheckArrayTypeMismatch(array);
            //range checks throws automatically
            _ = array[start];
            _ = array[start + length];
            head = ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(array), start);
            Length = length;
        }

        /// <summary>
        /// Creates a new <see cref="ReadOnlyNativeSpan{T}"/> object over the entirety of a specified <paramref name="span"/>.
        /// </summary>
        /// <param name="span">The memory region from which to create the <see cref="ReadOnlyNativeSpan{T}"/> object.</param>
        /// <remarks>If <paramref name="span"/> is null, this constructor returns a <see langword="null"/> <see cref="ReadOnlyNativeSpan{T}"/>.</remarks>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public ReadOnlyNativeSpan(ReadOnlySpan<T> span)
        {
            if (span.IsEmpty)
            {
                this = default;
                return;
            }
            this = new(ref MemoryMarshal.GetReference(span), span.Length);
        }

        #endregion

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static void CheckArrayTypeMismatch(T[]? array)
        {
            if (array is not null && !(typeof(T).IsValueType || array.GetType() == typeof(T[])))
            {
                ThrowHelper.Throw(new ArrayTypeMismatchException());
            }
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static unsafe void CheckLength(nint length)
        {
            if (length < 0) ThrowHelper.Throw(new ArgumentOutOfRangeException(nameof(length), length, $"{nameof(length)} must be positive!"));
        }

        /// <inheritdoc cref="ReadOnlySpan{T}.this[int]"/>
        public ref readonly T this[nint index]
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get
            {
                //Range Check Elimination doesn't work for custom struct.
                if ((nuint)index >= (nuint)Length)
                {
                    ThrowHelper.ThrowIndexOutOfRangeException();
                }
                return ref Unsafe.Add(ref head, index);
            }
        }

        /// <inheritdoc cref="ReadOnlySpan{T}.this[int]"/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public ref readonly T ElementAtUnchecked(nint index) => ref Unsafe.Add(ref head, index);

        /// <inheritdoc cref="ReadOnlySpan{T}.GetPinnableReference()"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Browsable(false)]
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public ref T GetPinnableReference() => ref head;

        /// <inheritdoc cref="ReadOnlySpan{T}.Slice(int)"/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public ReadOnlyNativeSpan<T> Slice(nint start)
            => new(ref Unsafe.Add(ref head, start), Length - start);

        /// <inheritdoc cref="ReadOnlySpan{T}.Slice(int, int)"/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public ReadOnlyNativeSpan<T> Slice(nint start, nint length)
            => new(ref Unsafe.Add(ref head, start), length);

        /// <summary>
        /// Copies the content of this <see cref="ReadOnlyNativeSpan{T}"/> into a <paramref name="destination"/> <see cref="NativeSpan{T}"/>.
        /// </summary>
        /// <param name="destination">The destination <see cref="NativeSpan{T}"/> object.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public void CopyTo(NativeSpan<T> destination)
        {
            if (!TryCopyTo(this, destination)) throw new ArgumentException($"The {nameof(destination)} must be at least as long as this span!", nameof(destination));
        }

        /// <summary>
        /// Copies the content of this <see cref="ReadOnlyNativeSpan{T}"/> into a <paramref name="destination"/> <see cref="Span{T}"/>.
        /// </summary>
        /// <param name="destination">The destination <see cref="ReadOnlySpan{T}"/> object.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public void CopyTo(Span<T> destination)
        {
            if (!TryCopyTo(this, destination)) throw new ArgumentException($"The {nameof(destination)} must be at least as long as this span!", nameof(destination));
        }

        /// <inheritdoc cref="ReadOnlySpan{T}.TryCopyTo(Span{T})"/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public bool TryCopyTo(NativeSpan<T> destination) => TryCopyTo(this, destination);

        /// <inheritdoc cref="ReadOnlySpan{T}.TryCopyTo(Span{T})"/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public bool TryCopyTo(Span<T> destination) => TryCopyTo(this, destination);

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static bool TryCopyTo(ReadOnlyNativeSpan<T> source, NativeSpan<T> destination)
        {
            if (destination.IsEmpty || source.IsEmpty)
            {
                return source.IsEmpty;
            }
            if (destination.Length < source.Length)
            {
                return false;
            }
            destination = destination.Slice(0, source.Length);
            if (Unsafe.AreSame(ref source.head, ref destination.Head)) return true;
            if (source.FitsInReadOnlySpan)
            {
                return source.GetHeadReadOnlySpan().TryCopyTo(destination.GetHeadSpan());
            }
            var length1 = checked((nuint)source.Length * (nuint)Unsafe.SizeOf<T>());//We can't have memory more than nuint.MaxValue bytes in the first place
            UnsafeUtils.MoveMemory(ref Unsafe.As<T, byte>(ref destination.Head), ref Unsafe.As<T, byte>(ref source.head), length1);
            return true;
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static bool TryCopyTo(ReadOnlyNativeSpan<T> source, Span<T> destination)
        {
            if (destination.IsEmpty || source.IsEmpty)
            {
                return source.IsEmpty;
            }
            if (destination.Length < source.Length)
            {
                return false;
            }
            if (Unsafe.AreSame(ref source.head, ref MemoryMarshal.GetReference(destination))) return true;
            return source.FitsInReadOnlySpan && source.GetHeadReadOnlySpan().TryCopyTo(destination);
        }

        /// <summary>
        /// Determines whether the specified <paramref name="left"/> and <paramref name="right"/> shares the same region.
        /// </summary>
        /// <param name="left">The first checking <see cref="ReadOnlyNativeSpan{T}"/>.</param>
        /// <param name="right">The second checking <see cref="ReadOnlyNativeSpan{T}"/>.</param>
        /// <returns>The value which indicates whether the specified <paramref name="left"/> and <paramref name="right"/> shares the same region.</returns>
        public static bool IsOverlapped(ReadOnlyNativeSpan<T> left, ReadOnlyNativeSpan<T> right)
        {
            if (left.IsEmpty || right.IsEmpty)
            {
                return false;
            }
            var bo = Unsafe.ByteOffset(ref left.head, ref right.head);
            return (nuint)bo < (nuint)(left.Length * Unsafe.SizeOf<T>()) || (nuint)bo > (nuint)(-right.Length * Unsafe.SizeOf<T>());
        }

        /// <summary>
        /// Indicates whether the values of two specified <see cref="ReadOnlyNativeSpan{T}"/> objects are equal.
        /// </summary>
        /// <param name="left">The first <see cref="ReadOnlyNativeSpan{T}"/> to compare.</param>
        /// <param name="right">The second <see cref="ReadOnlyNativeSpan{T}"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if the left is the same as the right; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool operator ==(ReadOnlyNativeSpan<T> left, ReadOnlyNativeSpan<T> right) => left.Length == right.Length && Unsafe.AreSame(ref left.head, ref right.head);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="ReadOnlyNativeSpan{T}"/> objects are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="ReadOnlyNativeSpan{T}"/> to compare.</param>
        /// <param name="right">The second  <see cref="ReadOnlyNativeSpan{T}"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if left and right are not equal; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool operator !=(ReadOnlyNativeSpan<T> left, ReadOnlyNativeSpan<T> right) => !(left == right);

        #region Equals and GetHashCode
        /// <inheritdoc/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public override bool Equals(object? obj) => false;

        /// <inheritdoc/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public override unsafe int GetHashCode() => HashCode.Combine((nint)Unsafe.AsPointer(ref head), Length);
        #endregion

        #region Conversions
        /// <summary>
        /// Performs an implicit conversion from <see cref="ReadOnlySpan{T}"/> to <see cref="ReadOnlyNativeSpan{T}"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator ReadOnlyNativeSpan<T>(ReadOnlySpan<T> value) => new(value);

        /// <summary>
        /// Performs an implicit conversion from <see cref="Array"/> to <see cref="ReadOnlyNativeSpan{T}"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator ReadOnlyNativeSpan<T>(T[] value) => new(value);

        /// <summary>
        /// Performs an implicit conversion from <see cref="ArraySegment{T}"/> to <see cref="ReadOnlyNativeSpan{T}"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator ReadOnlyNativeSpan<T>(ArraySegment<T> value) => new(value);
        #endregion
    }
}
