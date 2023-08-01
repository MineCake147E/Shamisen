using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Shamisen.Memory;
using Shamisen.Utils;

namespace Shamisen
{
    /// <typeparam name="T">The type of items in the <see cref="NativeSpan{T}"/>.</typeparam>
    /// <inheritdoc cref="Span{T}"/>
    [StructLayout(LayoutKind.Sequential)]
    public readonly ref struct NativeSpan<T>
    {
        private readonly ref T head;

        /// <summary>
        /// The length of this <see cref="NativeSpan{T}"/>.
        /// </summary>
        public nint Length
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get;
        }

        internal ref T Head => ref head;
        internal ref T Tail => ref Unsafe.Add(ref head, Length - 1);

        /// <summary>
        /// Returns a value that indicates whether the current <see cref="NativeSpan{T}"/> is empty.
        /// </summary>
        /// <inheritdoc cref="Span{T}.IsEmpty"/>
        public bool IsEmpty
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Length <= 0;
        }

        /// <summary>
        /// Creates a new <see cref="Span{T}"/> object that represents the current <see cref="NativeSpan{T}"/>.
        /// </summary>
        public Span<T> GetHeadSpan() => MemoryMarshal.CreateSpan(ref head, (int)MathI.Min(int.MaxValue, Length));

        /// <summary>
        /// Returns a value that indicates whether the current <see cref="NativeSpan{T}"/>'s contents can fit into <see cref="Span{T}"/>.
        /// </summary>
        public bool FitsInSpan => Length <= int.MaxValue;

        /// <summary>
        /// Returns an empty <see cref="NativeSpan{T}"/> object.
        /// </summary>
        /// <value>An empty <see cref="NativeSpan{T}"/> object.</value>
        public static NativeSpan<T> Empty
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => default;
        }

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="NativeSpan{T}"/> struct.
        /// </summary>
        /// <param name="headPointer">The head pointer.</param>
        /// <param name="length">The length.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public unsafe NativeSpan(ref T headPointer, nint length)
        {
            CheckLength(length);
            head = ref headPointer;
            Length = length;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NativeSpan{T}"/> struct.
        /// </summary>
        /// <param name="headPointer">The head pointer.</param>
        /// <param name="length">The length.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public unsafe NativeSpan(void* headPointer, nint length)
        {
            _ = new ReadOnlySpan<T>(headPointer, 0);
            CheckLength(length);
            head = ref Unsafe.AsRef<T>(headPointer);
            Length = length;
        }

        /// <summary>
        /// Creates a new <see cref="NativeSpan{T}"/> object over the entirety of a specified <paramref name="array"/>.
        /// </summary>
        /// <param name="array">The array from which to create the <see cref="NativeSpan{T}"/> object.</param>
        /// <exception cref="ArrayTypeMismatchException"><typeparamref name="T"/> is a reference type, and <paramref name="array"/> is not an array of type <typeparamref name="T"/>.</exception>
        /// <remarks>If <paramref name="array"/> is null, this constructor returns a <see langword="null"/> <see cref="NativeSpan{T}"/>.</remarks>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public NativeSpan(T[]? array)
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
        /// Creates a new <see cref="NativeSpan{T}"/> object over the entirety of a specified <paramref name="array"/>.
        /// </summary>
        /// <param name="array">The array from which to create the <see cref="NativeSpan{T}"/> object.</param>
        /// <exception cref="ArrayTypeMismatchException"><typeparamref name="T"/> is a reference type, and <paramref name="array"/> is not an array of type <typeparamref name="T"/>.</exception>
        /// <remarks>If <paramref name="array"/> is null, this constructor returns a <see langword="null"/> <see cref="NativeSpan{T}"/>.</remarks>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public NativeSpan(NativeArray<T> array)
        {
            this = (array?.IsEmpty == false) ? array.NativeSpan : default;
        }

        /// <summary>
        /// Creates a new <see cref="NativeSpan{T}"/> object over the entirety of a specified <paramref name="array"/>.
        /// </summary>
        /// <param name="array">The array from which to create the <see cref="NativeSpan{T}"/> object.</param>
        /// <exception cref="ArrayTypeMismatchException"><typeparamref name="T"/> is a reference type, and <paramref name="array"/> is not an array of type <typeparamref name="T"/>.</exception>
        /// <remarks>If <paramref name="array"/> is null, this constructor returns a <see langword="null"/> <see cref="NativeSpan{T}"/>.</remarks>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public NativeSpan(ArraySegment<T> array) : this(array.Array, array.Offset, array.Count)
        {
        }

        /// <summary>
        /// Creates a new <see cref="NativeSpan{T}"/> object that includes a specified number of elements of an array starting at a specified index.
        /// </summary>
        /// <param name="array">The source array.</param>
        /// <param name="start">The index of the first element to include in the new <see cref="NativeSpan{T}"/>.</param>
        /// <param name="length">The number of elements to include in the new <see cref="NativeSpan{T}"/>.</param>
        /// <exception cref="ArrayTypeMismatchException"><typeparamref name="T"/> is a reference type, and <paramref name="array"/> is not an array of type <typeparamref name="T"/>.</exception>
        /// <remarks>If <paramref name="array"/> is null, this constructor returns a <see langword="null"/> <see cref="NativeSpan{T}"/>.</remarks>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public NativeSpan(T[]? array, nint start, nint length)
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
        /// Creates a new <see cref="NativeSpan{T}"/> object over the entirety of a specified <paramref name="span"/>.
        /// </summary>
        /// <param name="span">The memory region from which to create the <see cref="NativeSpan{T}"/> object.</param>
        /// <remarks>If <paramref name="span"/> is null, this constructor returns a <see langword="null"/> <see cref="NativeSpan{T}"/>.</remarks>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public NativeSpan(Span<T> span)
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

        /// <inheritdoc cref="Span{T}.this[int]"/>
        public ref T this[nint index]
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

        /// <inheritdoc cref="Span{T}.this[int]"/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public ref T ElementAtUnchecked(nint index) => ref Unsafe.Add(ref head, index);

        /// <inheritdoc cref="Span{T}.GetPinnableReference()"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Browsable(false)]
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public ref T GetPinnableReference() => ref head;

        /// <inheritdoc cref="Span{T}.Slice(int)"/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public NativeSpan<T> Slice(nint start)
            => new(ref Unsafe.Add(ref head, start), Length - start);

        /// <inheritdoc cref="Span{T}.Slice(int, int)"/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public NativeSpan<T> Slice(nint start, nint length)
            => new(ref Unsafe.Add(ref head, start), length);

        #region Fill and Clear

        /// <summary>
        /// Fills the elements of this span with a specified <paramref name="value"/>.
        /// </summary>
        /// <inheritdoc cref="Span{T}.Fill(T)"/>
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        public void Fill(T value)
        {
            if (IsEmpty) return;
            switch (default(T))
            {
                case float _:
                    SpanUtils.FillWithReference(Unsafe.As<T, float>(ref value), ref Unsafe.As<T, float>(ref Head), Length);
                    return;
                case double _:
                    SpanUtils.FillWithReference(Unsafe.As<T, double>(ref value), ref Unsafe.As<T, double>(ref Head), Length);
                    return;
            }
            if (Vector.IsHardwareAccelerated)
            {
                if (Unsafe.SizeOf<T>() == 1)
                {
                    SpanUtils.FillWithReferencePreloaded(new(Unsafe.As<T, byte>(ref value)), ref Unsafe.As<T, byte>(ref Head), Length);
                    return;
                }
                else if (Unsafe.SizeOf<T>() == 2)
                {
                    SpanUtils.FillWithReferencePreloaded(new(Unsafe.As<T, ushort>(ref value)), ref Unsafe.As<T, ushort>(ref Head), Length);
                    return;
                }
                else if (Unsafe.SizeOf<T>() == 3)
                {
                    SpanUtils.FillWithReference3BytesVectorized(ref Unsafe.As<T, byte>(ref value), ref Unsafe.As<T, byte>(ref Head), Length);
                    return;
                }
                else if (Unsafe.SizeOf<T>() == 4)
                {
                    SpanUtils.FillWithReferencePreloaded(new(Unsafe.As<T, uint>(ref value)), ref Unsafe.As<T, uint>(ref Head), Length);
                    return;
                }
                else if (Unsafe.SizeOf<T>() == 5)
                {
                    SpanUtils.FillWithReference5BytesVectorized(ref Unsafe.As<T, byte>(ref value), ref Unsafe.As<T, byte>(ref Head), Length);
                    return;
                }
                else if (Unsafe.SizeOf<T>() == 8)
                {
                    SpanUtils.FillWithReferencePreloaded(new(Unsafe.As<T, ulong>(ref value)), ref Unsafe.As<T, ulong>(ref Head), Length);
                    return;
                }
                else if (Unsafe.SizeOf<T>() == 16)
                {
                    SpanUtils.FillWithReferenceVector4(Unsafe.As<T, Vector4>(ref value), ref Unsafe.As<T, Vector4>(ref Head), Length);
                    return;
                }
                else if (Unsafe.SizeOf<T>() is > 16 && Unsafe.SizeOf<T>() == Vector<byte>.Count)
                {
                    SpanUtils.FillWithReferenceVectorFit(Unsafe.As<T, Vector<byte>>(ref value), ref Unsafe.As<T, Vector<byte>>(ref Head), Length);
                    return;
                }
            }
            SpanUtils.FillWithReferenceNBytes(ref Unsafe.As<T, byte>(ref value), (nuint)Unsafe.SizeOf<T>(), ref Unsafe.As<T, byte>(ref Head), (nuint)Length * (nuint)Unsafe.SizeOf<T>());
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private void FillShort(T value)
        {
            nint i = 0, length = Length;
            ref var rsi = ref head;
            var olen = length - 7;
            for (; i < olen; i += 8)
            {
                Unsafe.Add(ref rsi, i + 0) = value;
                Unsafe.Add(ref rsi, i + 1) = value;
                Unsafe.Add(ref rsi, i + 2) = value;
                Unsafe.Add(ref rsi, i + 3) = value;
                Unsafe.Add(ref rsi, i + 4) = value;
                Unsafe.Add(ref rsi, i + 5) = value;
                Unsafe.Add(ref rsi, i + 6) = value;
                Unsafe.Add(ref rsi, i + 7) = value;
            }
            for (; i < length; i++)
            {
                Unsafe.Add(ref rsi, i) = value;
            }
        }

        /// <summary>
        /// Clears the contents of this <see cref="NativeSpan{T}"/> object.
        /// </summary>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public void Clear() => SpanUtils.FillWithReference((byte)0, ref Unsafe.As<T, byte>(ref Head), Length * Unsafe.SizeOf<T>());
        #endregion

        /// <summary>
        /// Copies the content of this <see cref="NativeSpan{T}"/> into a <paramref name="destination"/> <see cref="NativeSpan{T}"/>.
        /// </summary>
        /// <param name="destination">The destination <see cref="NativeSpan{T}"/> object.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public void CopyTo(NativeSpan<T> destination)
        {
            if (!TryCopyTo(destination)) throw new ArgumentException($"The {nameof(destination)} must be at least as long as this span!", nameof(destination));
        }

        /// <summary>
        /// Copies the content of this <see cref="NativeSpan{T}"/> into a <paramref name="destination"/> <see cref="Span{T}"/>.
        /// </summary>
        /// <param name="destination">The destination <see cref="Span{T}"/> object.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public void CopyTo(Span<T> destination)
        {
            if (!TryCopyTo(destination)) throw new ArgumentException($"The {nameof(destination)} must be at least as long as this span!", nameof(destination));
        }

        /// <summary>
        /// Attempts to copy the current <see cref="NativeSpan{T}"/> to a destination <see cref="NativeSpan{T}"/> and returns a value that indicates whether the copy operation succeeded.
        /// </summary>
        /// <inheritdoc cref="Span{T}.TryCopyTo(Span{T})"/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public bool TryCopyTo(NativeSpan<T> destination) => ReadOnlyNativeSpan<T>.TryCopyTo(this, destination);

        /// <summary>
        /// Attempts to copy the current <see cref="NativeSpan{T}"/> to a destination <see cref="Span{T}"/> and returns a value that indicates whether the copy operation succeeded.
        /// </summary>
        /// <inheritdoc cref="Span{T}.TryCopyTo(Span{T})"/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public bool TryCopyTo(Span<T> destination) => ReadOnlyNativeSpan<T>.TryCopyTo(this, destination);

        /// <summary>
        /// Determines whether the specified <paramref name="left"/> and <paramref name="right"/> shares the same region.
        /// </summary>
        /// <param name="left">The first checking <see cref="NativeSpan{T}"/>.</param>
        /// <param name="right">The second checking <see cref="NativeSpan{T}"/>.</param>
        /// <returns>The value which indicates whether the specified <paramref name="left"/> and <paramref name="right"/> shares the same region.</returns>
        public static bool IsOverlapped(NativeSpan<T> left, NativeSpan<T> right)
        {
            if (left.IsEmpty || right.IsEmpty)
            {
                return false;
            }
            var bo = Unsafe.ByteOffset(ref left.head, ref right.head);
            return (nuint)bo < (nuint)(left.Length * Unsafe.SizeOf<T>()) || (nuint)bo > (nuint)(-right.Length * Unsafe.SizeOf<T>());
        }

        /// <summary>
        /// Indicates whether the values of two specified <see cref="NativeSpan{T}"/> objects are equal.
        /// </summary>
        /// <param name="left">The first <see cref="NativeSpan{T}"/> to compare.</param>
        /// <param name="right">The second <see cref="NativeSpan{T}"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if the left is the same as the right; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool operator ==(NativeSpan<T> left, NativeSpan<T> right) => left.Length == right.Length && Unsafe.AreSame(ref left.head, ref right.head);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="NativeSpan{T}"/> objects are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="NativeSpan{T}"/> to compare.</param>
        /// <param name="right">The second  <see cref="NativeSpan{T}"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if left and right are not equal; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool operator !=(NativeSpan<T> left, NativeSpan<T> right) => !(left == right);

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
        /// Performs an implicit conversion from <see cref="Span{T}"/> to <see cref="NativeSpan{T}"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator NativeSpan<T>(Span<T> value) => new(value);

        /// <summary>
        /// Performs an implicit conversion from <see cref="Array"/> to <see cref="NativeSpan{T}"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator NativeSpan<T>(T[] value) => new(value);

        /// <summary>
        /// Performs an implicit conversion from <see cref="ArraySegment{T}"/> to <see cref="NativeSpan{T}"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator NativeSpan<T>(ArraySegment<T> value) => new(value);

        /// <summary>
        /// Performs an implicit conversion from <see cref="NativeSpan{T}"/> to <see cref="ReadOnlySpan{T}"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator ReadOnlyNativeSpan<T>(NativeSpan<T> value) => new(value);
        #endregion
    }
}
