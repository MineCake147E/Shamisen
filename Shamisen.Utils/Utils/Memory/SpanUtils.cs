using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;

namespace Shamisen
{
    /// <summary>
    /// Provides some extension functions.
    /// </summary>
    public static partial class SpanUtils
    {
        #region CreateNativeSpan
        /// <inheritdoc cref="NativeSpan{T}.NativeSpan(ref T, nint)"/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static NativeSpan<T> CreateNativeSpan<T>(ref T head, nint length) => new(ref head, length);

        /// <inheritdoc cref="ReadOnlyNativeSpan{T}.ReadOnlyNativeSpan(ref T, nint)"/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static ReadOnlyNativeSpan<T> CreateReadOnlyNativeSpan<T>(ref T head, nint length) => new(ref head, length);
        #endregion

        #region GetReference
        /// <inheritdoc cref="MemoryMarshal.GetReference{T}(Span{T})"/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]

        public static ref T GetReference<T>(NativeSpan<T> span) => ref span.Head;

        /// <inheritdoc cref="MemoryMarshal.GetReference{T}(Span{T})"/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static ref T GetReference<T>(ReadOnlyNativeSpan<T> span) => ref span.Head;
        #endregion

        #region Cast

        /// <summary>
        /// Casts a <see cref="NativeSpan{T}"/> of one primitive type, <typeparamref name="T"/>, to a <c>NativeSpan&lt;byte&gt;</c>
        /// </summary>
        /// <inheritdoc cref="MemoryMarshal.AsBytes{T}(Span{T})"/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static NativeSpan<byte> AsBytes<T>(NativeSpan<T> span) where T : unmanaged => CreateNativeSpan(ref Unsafe.As<T, byte>(ref span.Head), checked(span.Length * Unsafe.SizeOf<T>()));

        /// <summary>
        /// Casts a <see cref="ReadOnlyNativeSpan{T}"/> of one primitive type, <typeparamref name="T"/>, to a <c>ReadOnlyNativeSpan&lt;byte&gt;</c>
        /// </summary>
        /// <inheritdoc cref="MemoryMarshal.AsBytes{T}(ReadOnlySpan{T})"/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static ReadOnlyNativeSpan<byte> AsBytes<T>(ReadOnlyNativeSpan<T> span) where T : unmanaged => CreateReadOnlyNativeSpan(ref Unsafe.As<T, byte>(ref span.Head), checked(span.Length * Unsafe.SizeOf<T>()));

        /// <inheritdoc cref="MemoryMarshal.Cast{TFrom, TTo}(Span{TFrom})"/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static NativeSpan<TTo> Cast<TFrom, TTo>(NativeSpan<TFrom> span) where TFrom : unmanaged where TTo : unmanaged
        {
            if (Unsafe.SizeOf<TFrom>() == Unsafe.SizeOf<TTo>())
            {
                return CreateNativeSpan(ref Unsafe.As<TFrom, TTo>(ref span.Head), span.Length);
            }
            if (Unsafe.SizeOf<TTo>() == Unsafe.SizeOf<byte>())
            {
                return CreateNativeSpan(ref Unsafe.As<TFrom, TTo>(ref span.Head), checked(span.Length * Unsafe.SizeOf<TFrom>()));
            }
            if (Unsafe.SizeOf<TFrom>() == Unsafe.SizeOf<byte>())
            {
                return CreateNativeSpan(ref Unsafe.As<TFrom, TTo>(ref span.Head), span.Length / Unsafe.SizeOf<TTo>());
            }
            if (Unsafe.SizeOf<TFrom>() > Unsafe.SizeOf<TTo>() && Unsafe.SizeOf<TFrom>() % Unsafe.SizeOf<TTo>() == 0)
            {
                return CreateNativeSpan(ref Unsafe.As<TFrom, TTo>(ref span.Head), checked(span.Length * (Unsafe.SizeOf<TFrom>() / Unsafe.SizeOf<TTo>())));
            }
            if (Unsafe.SizeOf<TTo>() > Unsafe.SizeOf<TFrom>() && Unsafe.SizeOf<TTo>() % Unsafe.SizeOf<TFrom>() == 0)
            {
                return CreateNativeSpan(ref Unsafe.As<TFrom, TTo>(ref span.Head), span.Length / (Unsafe.SizeOf<TTo>() / Unsafe.SizeOf<TFrom>()));
            }
            var t = Math.BigMul((ulong)span.Length, (ulong)Unsafe.SizeOf<TFrom>(), out var low);
            return CreateNativeSpan(ref Unsafe.As<TFrom, TTo>(ref span.Head), checked((nint)MathI.BigDivConstant(t, low, (ulong)Unsafe.SizeOf<TTo>())));
        }

        /// <inheritdoc cref="MemoryMarshal.Cast{TFrom, TTo}(ReadOnlySpan{TFrom})"/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static ReadOnlyNativeSpan<TTo> Cast<TFrom, TTo>(ReadOnlyNativeSpan<TFrom> span) where TFrom : unmanaged where TTo : unmanaged
        {
            if (Unsafe.SizeOf<TFrom>() == Unsafe.SizeOf<TTo>())
            {
                return CreateReadOnlyNativeSpan(ref Unsafe.As<TFrom, TTo>(ref span.Head), checked(span.Length));
            }
            if (Unsafe.SizeOf<TTo>() == Unsafe.SizeOf<byte>())
            {
                return CreateReadOnlyNativeSpan(ref Unsafe.As<TFrom, TTo>(ref span.Head), checked(span.Length * Unsafe.SizeOf<TFrom>()));
            }
            if (Unsafe.SizeOf<TFrom>() == Unsafe.SizeOf<byte>())
            {
                return CreateReadOnlyNativeSpan(ref Unsafe.As<TFrom, TTo>(ref span.Head), checked(span.Length / Unsafe.SizeOf<TTo>()));
            }
            if (Unsafe.SizeOf<TFrom>() > Unsafe.SizeOf<TTo>() && Unsafe.SizeOf<TFrom>() % Unsafe.SizeOf<TTo>() == 0)
            {
                return CreateReadOnlyNativeSpan(ref Unsafe.As<TFrom, TTo>(ref span.Head), checked(span.Length / (Unsafe.SizeOf<TFrom>() / Unsafe.SizeOf<TTo>())));
            }
            var t = Math.BigMul((ulong)span.Length, (ulong)Unsafe.SizeOf<TFrom>(), out var low);
            return CreateReadOnlyNativeSpan(ref Unsafe.As<TFrom, TTo>(ref span.Head), checked((nint)MathI.BigDivConstant(t, low, (ulong)Unsafe.SizeOf<TTo>())));
        }
        #endregion

        #region MoveByOffset

        /// <summary>
        /// Moves the elements of specified <paramref name="span"/> right by 1 element.
        /// </summary>
        /// <param name="span">The <see cref="Span{T}"/> to move its elements.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static void ShiftRight(this Span<int> span) => span.SliceWhile(span.Length - 1).CopyTo(span[1..]);

        /// <summary>
        /// Moves the elements of specified <paramref name="span"/> right by 1 element.
        /// </summary>
        /// <typeparam name="TSample"></typeparam>
        /// <param name="span">The <see cref="Span{T}"/> to move its elements.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static void ShiftRight<TSample>(this Span<TSample> span) => span.SliceWhile(span.Length - 1).CopyTo(span[1..]);

        #endregion MoveByOffset

        #region LinqLikeForSpan

        /// <summary>
        /// Skips the specified step.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="span">The span.</param>
        /// <param name="step">The step.</param>
        /// <returns></returns>
        public static Span<T> Skip<T>(this Span<T> span, int step) => span[step..];

        #endregion LinqLikeForSpan

        #region Slice
        #region SliceWhileIfLongerThan

        /// <summary>
        /// Slices the specified <paramref name="span"/> to the specified <paramref name="maxLength"/> if the <paramref name="span"/> is longer than the <paramref name="maxLength"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="span">The span.</param>
        /// <param name="maxLength">The maximum length.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static Span<T> SliceWhileIfLongerThan<T>(this Span<T> span, int maxLength)
            => span.Length > maxLength ? span.SliceWhile(maxLength) : span;

        /// <summary>
        /// Slices the specified <paramref name="span"/> to the specified <paramref name="maxLength"/> if the <paramref name="span"/> is longer than the <paramref name="maxLength"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="span">The span.</param>
        /// <param name="maxLength">The maximum length.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static ReadOnlySpan<T> SliceWhileIfLongerThan<T>(this ReadOnlySpan<T> span, int maxLength)
            => span.Length > maxLength ? span.SliceWhile(maxLength) : span;

        /// <summary>
        /// Slices the specified <paramref name="span"/> to the specified <paramref name="maxLength"/> if the <paramref name="span"/> is longer than the <paramref name="maxLength"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="span">The span.</param>
        /// <param name="maxLength">The maximum length.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static Span<T> SliceWhileIfLongerThan<T>(this Span<T> span, ulong maxLength)
            => maxLength > int.MaxValue ? span : span.SliceWhileIfLongerThan((int)maxLength);
        /// <summary>
        /// Slices the specified <paramref name="span"/> to the specified <paramref name="maxLength"/> / <paramref name="divisor"/> if the <paramref name="span"/> is longer than the <paramref name="maxLength"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="span">The span.</param>
        /// <param name="maxLength">The maximum length multiplied by <paramref name="maxLength"/>.</param>
        /// <param name="divisor">The number to divide.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static Span<T> SliceWhileIfLongerThanWithLazyDivide<T>(this Span<T> span, int maxLength, int divisor)
        {
            divisor = MathI.Rectify(divisor);
            return (divisor > 0 || span.Length * divisor > maxLength) ? span.SliceWhile(maxLength / divisor) : span;
        }

        /// <summary>
        /// Slices the specified <paramref name="span"/> to the specified <paramref name="maxLength"/> / <paramref name="divisor"/> if the <paramref name="span"/> is longer than the <paramref name="maxLength"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="span">The span.</param>
        /// <param name="maxLength">The maximum length multiplied by <paramref name="maxLength"/>.</param>
        /// <param name="divisor">The number to divide.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static ReadOnlySpan<T> SliceWhileIfLongerThanWithLazyDivide<T>(this ReadOnlySpan<T> span, int maxLength, int divisor)
        {
            divisor = MathI.Rectify(divisor);
            return (divisor > 0 || span.Length * divisor > maxLength) ? span.SliceWhile(maxLength / divisor) : span;
        }
        #endregion

        /// <summary>
        /// Slices the <paramref name="span"/> back from the end of <paramref name="span"/> with specified <paramref name="length"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="span">The <see cref="Span{T}"/> to slice.</param>
        /// <param name="length">The length to slice back.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static Span<T> SliceFromEnd<T>(this Span<T> span, int length) => span[^length..];
        #endregion
        #region ReverseEndianness

        #region 64-bits-wide
        /// <summary>
        /// Reverses the endianness of each elements in specified <paramref name="span"/>.
        /// </summary>
        /// <param name="span">The span.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static void ReverseEndianness(this Span<Fixed64> span) => MemoryMarshal.Cast<Fixed64, ulong>(span).ReverseEndianness();

        /// <summary>
        /// Reverses the endianness of each elements in specified <paramref name="span"/>.
        /// </summary>
        /// <param name="span">The span.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static void ReverseEndianness(this Span<long> span) => MemoryMarshal.Cast<long, ulong>(span).ReverseEndianness();

        /// <summary>
        /// Reverses the endianness of each elements in specified <paramref name="span"/>.
        /// </summary>
        /// <param name="span">The span.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static void ReverseEndianness(this Span<double> span) => MemoryMarshal.Cast<double, ulong>(span).ReverseEndianness();

        /// <summary>
        /// Reverses the endianness of each elements in specified <paramref name="span"/>.
        /// </summary>
        /// <param name="span">The span.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static void ReverseEndianness(this Span<ulong> span)
        {
#if NET5_0_OR_GREATER
            if (AdvSimd.Arm64.IsSupported)
            {
                ReverseEndiannessAdvSimdArm64(span);
                return;
            }
            if (AdvSimd.IsSupported)
            {
                ReverseEndiannessAdvSimd(span);
                return;
            }
#endif
#if NETCOREAPP3_1_OR_GREATER
            if (Avx2.IsSupported)
            {
                ReverseEndiannessAvx2(span);
                return;
            }
            if (Ssse3.IsSupported)
            {
                ReverseEndiannessSsse3(span);
                return;
            }
#endif
            ReverseEndiannessFallback(span);
        }

        #endregion

        #region 32-bits-wide
        /// <summary>
        /// Reverses the endianness of each elements in specified <paramref name="span"/>.
        /// </summary>
        /// <param name="span">The span.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static void ReverseEndianness(this Span<Fixed32> span) => MemoryMarshal.Cast<Fixed32, int>(span).ReverseEndianness();

        /// <summary>
        /// Reverses the endianness of each elements in specified <paramref name="span"/>.
        /// </summary>
        /// <param name="span">The span.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static void ReverseEndianness(this Span<uint> span) => MemoryMarshal.Cast<uint, int>(span).ReverseEndianness();

        /// <summary>
        /// Reverses the endianness of each elements in specified <paramref name="span"/>.
        /// </summary>
        /// <param name="span">The span.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static void ReverseEndianness(this Span<float> span) => MemoryMarshal.Cast<float, int>(span).ReverseEndianness();

        /// <summary>
        /// Reverses the endianness of each elements in specified <paramref name="span"/>.
        /// </summary>
        /// <param name="span">The span.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static void ReverseEndianness(this Span<int> span)
        {
#if NET5_0_OR_GREATER
            if (AdvSimd.Arm64.IsSupported)
            {
                ReverseEndiannessAdvSimdArm64(span);
                return;
            }
            if (AdvSimd.IsSupported)
            {
                ReverseEndiannessAdvSimd(span);
                return;
            }
#endif

#if NETCOREAPP3_1_OR_GREATER
            if (Avx2.IsSupported)
            {
                ReverseEndiannessAvx2(span);
                return;
            }
            if (Ssse3.IsSupported)
            {
                ReverseEndiannessSsse3(span);
                return;
            }
#endif
            ReverseEndiannessFallback(span);
        }

        #endregion

        #region 24-bits-wide
        /// <summary>
        /// Reverses the endianness of each elements in specified <paramref name="span"/>.
        /// </summary>
        /// <param name="span">The span.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static void ReverseEndianness(this Span<Int24> span)
        {
            /*
#if NET5_0_OR_GREATER
            if (AdvSimd.Arm64.IsSupported)
            {
                ReverseEndiannessAdvSimdArm64(span);
                return;
            }
            if (AdvSimd.IsSupported)
            {
                ReverseEndiannessAdvSimd(span);
                return;
            }
#endif
            */
#if NETCOREAPP3_1_OR_GREATER
            if (Ssse3.IsSupported)
            {
                ReverseEndiannessSsse3(span);
                return;
            }
#endif
            ReverseEndiannessFallback(span);
        }

        #endregion

        #region 16-bits-wide
        /// <summary>
        /// Reverses the endianness of each elements in specified <paramref name="span"/>.
        /// </summary>
        /// <param name="span">The span.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static void ReverseEndianness(this Span<Fixed16> span) => MemoryMarshal.Cast<Fixed16, short>(span).ReverseEndianness();

        /// <summary>
        /// Reverses the endianness of each elements in specified <paramref name="span"/>.
        /// </summary>
        /// <param name="span">The span.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static void ReverseEndianness(this Span<ushort> span) => MemoryMarshal.Cast<ushort, short>(span).ReverseEndianness();

#if NET5_0_OR_GREATER
        /// <summary>
        /// Reverses the endianness of each elements in specified <paramref name="span"/>.
        /// </summary>
        /// <param name="span">The span.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static void ReverseEndianness(this Span<Half> span) => MemoryMarshal.Cast<Half, short>(span).ReverseEndianness();

#endif

        /// <summary>
        /// Reverses the endianness of each elements in specified <paramref name="span"/>.
        /// </summary>
        /// <param name="span">The span.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static void ReverseEndianness(this Span<short> span)
        {
#if NET5_0_OR_GREATER
            if (AdvSimd.Arm64.IsSupported)
            {
                ReverseEndiannessAdvSimdArm64(span);
                return;
            }
            if (AdvSimd.IsSupported)
            {
                ReverseEndiannessAdvSimd(span);
                return;
            }
#endif

#if NETCOREAPP3_1_OR_GREATER
            if (Avx2.IsSupported)
            {
                ReverseEndiannessAvx2(span);
                return;
            }
            if (Ssse3.IsSupported)
            {
                ReverseEndiannessSsse3(span);
                return;
            }
#endif
            ReverseEndiannessFallback(span);
        }
        #endregion
        #endregion ReverseEndianness
    }
}
