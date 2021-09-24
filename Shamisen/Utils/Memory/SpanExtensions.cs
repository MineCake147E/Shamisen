using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Numerics;

using DivideSharp;

#if NET5_0_OR_GREATER
using System.Runtime.Intrinsics.Arm;
#endif
#if NETCOREAPP3_1_OR_GREATER
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Threading.Tasks;

using Shamisen.Utils.Intrinsics;
#endif

namespace Shamisen
{
    /// <summary>
    /// Provides some extension functions.
    /// </summary>
    public static partial class SpanExtensions
    {

        #region QuickFill

        /// <summary>
        /// Quickly (but slower than <see cref="FastFill(Span{float}, float)"/>) fills the specified memory region, with the given <paramref name="value"/>.
        /// </summary>
        /// <typeparam name="TSample">The type of the sample.</typeparam>
        /// <param name="span">The span to fill.</param>
        /// <param name="value">The value to fill with.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static void QuickFill<TSample>(this Span<TSample> span, TSample value)
        {
            if (span.Length < 32)
            {
                span.Fill(value);
                return;
            }
            var filled = span.SliceWhile(16);
            var remaining = span.Slice(filled.Length);
            filled.Fill(value);
            do
            {
                filled.CopyTo(remaining);
                remaining = remaining.Slice(filled.Length);
                filled = span.SliceWhile(filled.Length << 1);
            } while (remaining.Length >= filled.Length);
            filled.Slice(filled.Length - remaining.Length).CopyTo(remaining);
        }

        #endregion QuickFill

        #region Standard VectorFill
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void VectorFill<T>(Span<T> dst, T value) where T : unmanaged
        {
            if (!Vector.IsHardwareAccelerated)
            {
                dst.Fill(value);
                return;
            }
            ref var rdi = ref MemoryMarshal.GetReference(dst);
            var vv = new Vector<T>(value);
            nint i = 0, length = dst.Length;
            nint olen;
#if NETCOREAPP3_1_OR_GREATER
            if (Avx2.IsSupported)
            {
                olen = length - 32 * Vector<T>.Count + 1;
                for (; i < olen; i += 32 * Vector<T>.Count)
                {
                    Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdi, i + 0 * Vector<T>.Count)) = vv;
                    Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdi, i + 1 * Vector<T>.Count)) = vv;
                    Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdi, i + 2 * Vector<T>.Count)) = vv;
                    Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdi, i + 3 * Vector<T>.Count)) = vv;
                    Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdi, i + 4 * Vector<T>.Count)) = vv;
                    Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdi, i + 5 * Vector<T>.Count)) = vv;
                    Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdi, i + 6 * Vector<T>.Count)) = vv;
                    Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdi, i + 7 * Vector<T>.Count)) = vv;
                    Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdi, i + 8 * Vector<T>.Count)) = vv;
                    Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdi, i + 9 * Vector<T>.Count)) = vv;
                    Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdi, i + 10 * Vector<T>.Count)) = vv;
                    Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdi, i + 11 * Vector<T>.Count)) = vv;
                    Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdi, i + 12 * Vector<T>.Count)) = vv;
                    Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdi, i + 13 * Vector<T>.Count)) = vv;
                    Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdi, i + 14 * Vector<T>.Count)) = vv;
                    Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdi, i + 15 * Vector<T>.Count)) = vv;
                    Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdi, i + 16 * Vector<T>.Count)) = vv;
                    Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdi, i + 17 * Vector<T>.Count)) = vv;
                    Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdi, i + 18 * Vector<T>.Count)) = vv;
                    Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdi, i + 19 * Vector<T>.Count)) = vv;
                    Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdi, i + 20 * Vector<T>.Count)) = vv;
                    Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdi, i + 21 * Vector<T>.Count)) = vv;
                    Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdi, i + 22 * Vector<T>.Count)) = vv;
                    Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdi, i + 23 * Vector<T>.Count)) = vv;
                    Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdi, i + 24 * Vector<T>.Count)) = vv;
                    Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdi, i + 25 * Vector<T>.Count)) = vv;
                    Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdi, i + 26 * Vector<T>.Count)) = vv;
                    Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdi, i + 27 * Vector<T>.Count)) = vv;
                    Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdi, i + 28 * Vector<T>.Count)) = vv;
                    Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdi, i + 29 * Vector<T>.Count)) = vv;
                    Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdi, i + 30 * Vector<T>.Count)) = vv;
                    Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdi, i + 31 * Vector<T>.Count)) = vv;
                }
            }
            else if (Sse.IsSupported)
            {
                olen = length - 16 * Vector<T>.Count + 1;
                for (; i < olen; i += 16 * Vector<T>.Count)
                {
                    Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdi, i + 0 * Vector<T>.Count)) = vv;
                    Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdi, i + 1 * Vector<T>.Count)) = vv;
                    Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdi, i + 2 * Vector<T>.Count)) = vv;
                    Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdi, i + 3 * Vector<T>.Count)) = vv;
                    Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdi, i + 4 * Vector<T>.Count)) = vv;
                    Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdi, i + 5 * Vector<T>.Count)) = vv;
                    Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdi, i + 6 * Vector<T>.Count)) = vv;
                    Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdi, i + 7 * Vector<T>.Count)) = vv;
                    Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdi, i + 8 * Vector<T>.Count)) = vv;
                    Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdi, i + 9 * Vector<T>.Count)) = vv;
                    Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdi, i + 10 * Vector<T>.Count)) = vv;
                    Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdi, i + 11 * Vector<T>.Count)) = vv;
                    Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdi, i + 12 * Vector<T>.Count)) = vv;
                    Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdi, i + 13 * Vector<T>.Count)) = vv;
                    Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdi, i + 14 * Vector<T>.Count)) = vv;
                    Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdi, i + 15 * Vector<T>.Count)) = vv;
                }
            }
#endif
            olen = length - 4 * Vector<T>.Count + 1;
            for (; i < olen; i += 4 * Vector<T>.Count)
            {
                Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdi, i + 0 * Vector<T>.Count)) = vv;
                Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdi, i + 1 * Vector<T>.Count)) = vv;
                Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdi, i + 2 * Vector<T>.Count)) = vv;
                Unsafe.As<T, Vector<T>>(ref Unsafe.Add(ref rdi, i + 3 * Vector<T>.Count)) = vv;
            }
            for (; i < length; i++)
            {
                Unsafe.Add(ref rdi, i) = vv[0];
            }
        }
        #endregion

        #region FastFill for Int24 and UInt24
        //TODO: FastFill for Int24 and UInt24 using Sse42, Avx2, and AdvSimd
        #endregion

        #region MoveByOffset

        /// <summary>
        /// Moves the elements of specified <paramref name="span"/> right by 1 element.
        /// </summary>
        /// <param name="span">The <see cref="Span{T}"/> to move its elements.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static void ShiftRight(this Span<int> span)
        {
            unsafe
            {
                ref var q = ref MemoryMarshal.GetReference(span);
                for (var i = (IntPtr)(span.Length - 1); i.ToPointer() > IntPtr.Zero.ToPointer(); i -= 1)
                {
                    Unsafe.Add(ref q, i) = Unsafe.Add(ref q, i - 1);
                }
            }
        }

        /// <summary>
        /// Moves the elements of specified <paramref name="span"/> right by 1 element.
        /// </summary>
        /// <typeparam name="TSample"></typeparam>
        /// <param name="span">The <see cref="Span{T}"/> to move its elements.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static void ShiftRight<TSample>(this Span<TSample> span)
        {
            unsafe
            {
                ref var q = ref MemoryMarshal.GetReference(span);
                for (var i = (IntPtr)(span.Length - 1); i.ToPointer() > IntPtr.Zero.ToPointer(); i -= 1)
                {
                    Unsafe.Add(ref q, i) = Unsafe.Add(ref q, i - 1);
                }
            }
        }

        #endregion MoveByOffset

        #region LinqLikeForSpan

        /// <summary>
        /// Skips the specified step.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="span">The span.</param>
        /// <param name="step">The step.</param>
        /// <returns></returns>
        public static Span<T> Skip<T>(this Span<T> span, int step) => span.Slice(step);

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
        /// Slices the <paramref name="span"/> aligned with the multiple of <paramref name="channelsDivisor"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="span">The <see cref="Span{T}"/> to slice.</param>
        /// <param name="channelsDivisor">The divisor set to align width.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static Span<T> SliceAlign<T>(this Span<T> span, Int32Divisor channelsDivisor) => span.Slice(0, channelsDivisor.AbsFloor(span.Length));

        /// <summary>
        /// Slices the <paramref name="span"/> back from the end of <paramref name="span"/> with specified <paramref name="length"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="span">The <see cref="Span{T}"/> to slice.</param>
        /// <param name="length">The length to slice back.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static Span<T> SliceFromEnd<T>(this Span<T> span, int length) => span.Slice(span.Length - length);
        #endregion
        #region ReverseEndianness

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

#if NET5_0_OR_GREATER
        #region ARMv8

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ReverseEndiannessAdvSimd(Span<ulong> span)
        {
            ref var rdi = ref MemoryMarshal.GetReference(span);
            nint i = 0, length = span.Length;
            for (; i < length - 3; i += 4)
            {
                var v0_2d = Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 0));
                var v1_2d = Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 2));
                v0_2d = AdvSimd.ReverseElement8(v0_2d);
                v1_2d = AdvSimd.ReverseElement8(v1_2d);
                Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 0)) = v0_2d;
                Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 2)) = v1_2d;
            }
            for (; i < length; i++)
            {
                Unsafe.Add(ref rdi, i) = BinaryPrimitives.ReverseEndianness(Unsafe.Add(ref rdi, i));
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ReverseEndiannessAdvSimdArm64(Span<ulong> span)
        {
            ref var rdi = ref MemoryMarshal.GetReference(span);
            nint i = 0, length = span.Length;
            unsafe
            {
                //This is a loop for A64FX, probably not optimal in Cortex-A75.
                //The `ldp` instruction isn't available at all, so we use simpler load.
                for (; i < length - 15; i += 16)
                {
                    var v0_2d = Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 0));
                    var v1_2d = Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 2));
                    var v2_2d = Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 4));
                    var v3_2d = Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 6));
                    v0_2d = AdvSimd.ReverseElement8(v0_2d);
                    v1_2d = AdvSimd.ReverseElement8(v1_2d);
                    v2_2d = AdvSimd.ReverseElement8(v2_2d);
                    v3_2d = AdvSimd.ReverseElement8(v3_2d);
                    AdvSimdUtils.Arm64.StorePair(ref Unsafe.Add(ref rdi, i), v0_2d, v1_2d);
                    v0_2d = Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 8));
                    v1_2d = Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 10));
                    AdvSimdUtils.Arm64.StorePair(ref Unsafe.Add(ref rdi, i + 4), v2_2d, v3_2d);
                    v0_2d = AdvSimd.ReverseElement8(v0_2d);
                    v1_2d = AdvSimd.ReverseElement8(v1_2d);
                    v2_2d = Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 12));
                    v3_2d = Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 14));
                    v2_2d = AdvSimd.ReverseElement8(v2_2d);
                    AdvSimdUtils.Arm64.StorePair(ref Unsafe.Add(ref rdi, i + 8), v0_2d, v1_2d);
                    v3_2d = AdvSimd.ReverseElement8(v3_2d);
                    AdvSimdUtils.Arm64.StorePair(ref Unsafe.Add(ref rdi, i + 12), v2_2d, v3_2d);
                }
                for (; i < length - 7; i += 8)
                {
                    var v0_2d = Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 0));
                    var v1_2d = Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 2));
                    v0_2d = AdvSimd.ReverseElement8(v0_2d);
                    v1_2d = AdvSimd.ReverseElement8(v1_2d);
                    var v2_2d = Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 4));
                    var v3_2d = Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 6));
                    v2_2d = AdvSimd.ReverseElement8(v2_2d);
                    v3_2d = AdvSimd.ReverseElement8(v3_2d);
                    AdvSimdUtils.Arm64.StorePair(ref Unsafe.Add(ref rdi, i), v0_2d, v1_2d);
                    AdvSimdUtils.Arm64.StorePair(ref Unsafe.Add(ref rdi, i + 4), v2_2d, v3_2d);
                }
            }
            for (; i < length; i++)
            {
                Unsafe.Add(ref rdi, i) = BinaryPrimitives.ReverseEndianness(Unsafe.Add(ref rdi, i));
            }
        }

        #endregion
#endif
#if NETCOREAPP3_1_OR_GREATER

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ReverseEndiannessAvx2(Span<ulong> span)
        {
            //The Internal number gets deconstructed in little-endian so the values are written in BIG-ENDIAN.
            var mask = Vector128.Create(0x0001020304050607ul, 0x08090a0b0c0d0e0ful).AsByte();
            var mask256 = Vector256.Create(mask, mask);
            ref var rdi = ref MemoryMarshal.GetReference(span);
            nint i = 0, length = span.Length;
            for (; i < length - 31; i += 32)
            {
                var ymm0 = Unsafe.As<ulong, Vector256<ulong>>(ref Unsafe.Add(ref rdi, i + 0));
                var ymm1 = Unsafe.As<ulong, Vector256<ulong>>(ref Unsafe.Add(ref rdi, i + 4));
                var ymm2 = Unsafe.As<ulong, Vector256<ulong>>(ref Unsafe.Add(ref rdi, i + 8));
                var ymm3 = Unsafe.As<ulong, Vector256<ulong>>(ref Unsafe.Add(ref rdi, i + 12));
                ymm0 = Avx2.Shuffle(ymm0.AsByte(), mask256).AsUInt64();
                ymm1 = Avx2.Shuffle(ymm1.AsByte(), mask256).AsUInt64();
                ymm2 = Avx2.Shuffle(ymm2.AsByte(), mask256).AsUInt64();
                ymm3 = Avx2.Shuffle(ymm3.AsByte(), mask256).AsUInt64();
                Unsafe.As<ulong, Vector256<ulong>>(ref Unsafe.Add(ref rdi, i + 0)) = ymm0;
                Unsafe.As<ulong, Vector256<ulong>>(ref Unsafe.Add(ref rdi, i + 4)) = ymm1;
                Unsafe.As<ulong, Vector256<ulong>>(ref Unsafe.Add(ref rdi, i + 8)) = ymm2;
                Unsafe.As<ulong, Vector256<ulong>>(ref Unsafe.Add(ref rdi, i + 12)) = ymm3;
                ymm0 = Unsafe.As<ulong, Vector256<ulong>>(ref Unsafe.Add(ref rdi, i + 16));
                ymm1 = Unsafe.As<ulong, Vector256<ulong>>(ref Unsafe.Add(ref rdi, i + 20));
                ymm2 = Unsafe.As<ulong, Vector256<ulong>>(ref Unsafe.Add(ref rdi, i + 24));
                ymm3 = Unsafe.As<ulong, Vector256<ulong>>(ref Unsafe.Add(ref rdi, i + 28));
                ymm0 = Avx2.Shuffle(ymm0.AsByte(), mask256).AsUInt64();
                ymm1 = Avx2.Shuffle(ymm1.AsByte(), mask256).AsUInt64();
                ymm2 = Avx2.Shuffle(ymm2.AsByte(), mask256).AsUInt64();
                ymm3 = Avx2.Shuffle(ymm3.AsByte(), mask256).AsUInt64();
                Unsafe.As<ulong, Vector256<ulong>>(ref Unsafe.Add(ref rdi, i + 16)) = ymm0;
                Unsafe.As<ulong, Vector256<ulong>>(ref Unsafe.Add(ref rdi, i + 20)) = ymm1;
                Unsafe.As<ulong, Vector256<ulong>>(ref Unsafe.Add(ref rdi, i + 24)) = ymm2;
                Unsafe.As<ulong, Vector256<ulong>>(ref Unsafe.Add(ref rdi, i + 28)) = ymm3;
            }
            for (; i < length - 3; i += 4)
            {
                var xmm0 = Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 0));
                var xmm1 = Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 2));
                xmm0 = Ssse3.Shuffle(xmm0.AsByte(), mask256.GetLower()).AsUInt64();
                xmm1 = Ssse3.Shuffle(xmm1.AsByte(), mask256.GetLower()).AsUInt64();
                Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 0)) = xmm0;
                Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 2)) = xmm1;
            }
            for (; i < length; i++)
            {
                Unsafe.Add(ref rdi, i) = BinaryPrimitives.ReverseEndianness(Unsafe.Add(ref rdi, i));
            }
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ReverseEndiannessSsse3(Span<ulong> span)
        {
            //The Internal number gets deconstructed in little-endian so the values are written in BIG-ENDIAN.
            var mask = Vector128.Create(0x0001020304050607ul, 0x08090a0b0c0d0e0ful).AsByte();
            ref var rdi = ref MemoryMarshal.GetReference(span);
            nint i = 0, length = span.Length;
            for (; i < length - 15; i += 16)
            {
                var xmm0 = Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 0));
                var xmm1 = Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 2));
                var xmm2 = Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 4));
                var xmm3 = Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 6));
                xmm0 = Ssse3.Shuffle(xmm0.AsByte(), mask).AsUInt64();
                xmm1 = Ssse3.Shuffle(xmm1.AsByte(), mask).AsUInt64();
                xmm2 = Ssse3.Shuffle(xmm2.AsByte(), mask).AsUInt64();
                xmm3 = Ssse3.Shuffle(xmm3.AsByte(), mask).AsUInt64();
                Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 0)) = xmm0;
                Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 2)) = xmm1;
                Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 4)) = xmm2;
                Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 6)) = xmm3;
                xmm0 = Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 8));
                xmm1 = Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 10));
                xmm2 = Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 12));
                xmm3 = Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 14));
                xmm0 = Ssse3.Shuffle(xmm0.AsByte(), mask).AsUInt64();
                xmm1 = Ssse3.Shuffle(xmm1.AsByte(), mask).AsUInt64();
                xmm2 = Ssse3.Shuffle(xmm2.AsByte(), mask).AsUInt64();
                xmm3 = Ssse3.Shuffle(xmm3.AsByte(), mask).AsUInt64();
                Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 8)) = xmm0;
                Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 10)) = xmm1;
                Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 12)) = xmm2;
                Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 14)) = xmm3;
            }
            for (; i < length - 3; i += 4)
            {
                var xmm0 = Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 0));
                var xmm1 = Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 2));
                xmm0 = Ssse3.Shuffle(xmm0.AsByte(), mask).AsUInt64();
                xmm1 = Ssse3.Shuffle(xmm1.AsByte(), mask).AsUInt64();
                Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 0)) = xmm0;
                Unsafe.As<ulong, Vector128<ulong>>(ref Unsafe.Add(ref rdi, i + 2)) = xmm1;
            }
            for (; i < length; i++)
            {
                Unsafe.Add(ref rdi, i) = BinaryPrimitives.ReverseEndianness(Unsafe.Add(ref rdi, i));
            }
        }

#endif

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ReverseEndiannessFallback(Span<ulong> span)
        {
            ref var rdi = ref MemoryMarshal.GetReference(span);
            nint i = 0, length = span.Length;
            for (; i < length - 7; i += 8)
            {
                var x0 = Unsafe.Add(ref rdi, i + 0);
                var x1 = Unsafe.Add(ref rdi, i + 1);
                Unsafe.Add(ref rdi, i + 0) = BinaryPrimitives.ReverseEndianness(x0);
                Unsafe.Add(ref rdi, i + 1) = BinaryPrimitives.ReverseEndianness(x1);
                x0 = Unsafe.Add(ref rdi, i + 2);
                Unsafe.Add(ref rdi, i + 2) = BinaryPrimitives.ReverseEndianness(x0);
                x0 = Unsafe.Add(ref rdi, i + 3);
                Unsafe.Add(ref rdi, i + 3) = BinaryPrimitives.ReverseEndianness(x0);
                x0 = Unsafe.Add(ref rdi, i + 4);
                Unsafe.Add(ref rdi, i + 4) = BinaryPrimitives.ReverseEndianness(x0);
                x0 = Unsafe.Add(ref rdi, i + 5);
                Unsafe.Add(ref rdi, i + 5) = BinaryPrimitives.ReverseEndianness(x0);
                x0 = Unsafe.Add(ref rdi, i + 6);
                Unsafe.Add(ref rdi, i + 6) = BinaryPrimitives.ReverseEndianness(x0);
                x0 = Unsafe.Add(ref rdi, i + 7);
                Unsafe.Add(ref rdi, i + 7) = BinaryPrimitives.ReverseEndianness(x0);
            }
            for (; i < length; i++)
            {
                Unsafe.Add(ref rdi, i) = BinaryPrimitives.ReverseEndianness(Unsafe.Add(ref rdi, i));
            }
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ReverseEndiannessSimple(Span<ulong> span)
        {
            for (int i = 0; i < span.Length; i++)
            {
                span[i] = BinaryPrimitives.ReverseEndianness(span[i]);
            }
        }

        #endregion ReverseEndianness
    }
}
