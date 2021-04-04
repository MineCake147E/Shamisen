using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using Shamisen;

#if NET5_0_OR_GREATER

using System.Runtime.Intrinsics.Arm;

#endif
#if NETCOREAPP3_1_OR_GREATER

using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

#endif

namespace System
{
    /// <summary>
    /// Provides some extension functions.
    /// </summary>
    public static partial class SpanExtensions
    {
        #region SIMD-Related Functions

        /// <summary>
        /// Adds the <paramref name="samplesToAdd"/> to <paramref name="buffer"/>.
        /// </summary>
        /// <param name="samplesToAdd">The samples to add.</param>
        /// <param name="buffer">The buffer.</param>
        /// <exception cref="ArgumentException">samplesToAdd</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FastAdd(ReadOnlySpan<float> samplesToAdd, Span<float> buffer)
        {
            if (samplesToAdd.Length > buffer.Length) throw new ArgumentException("", nameof(samplesToAdd));
            unsafe
            {
                (int newLength, int remainder) = MathI.FloorStepRem(samplesToAdd.Length, Vector<float>.Count);
                if (newLength != 0)
                {
                    var src = MemoryMarshal.Cast<float, Vector<float>>(samplesToAdd);
                    var dst = MemoryMarshal.Cast<float, Vector<float>>(buffer).Slice(0, src.Length);
                    for (int i = 0; i < src.Length; i++)
                    {
                        dst[i] += src[i];
                    }
                }
                if (remainder != 0)
                {
                    var srcRem = samplesToAdd.Slice(newLength);
                    var dstRem = buffer.Slice(newLength).Slice(0, srcRem.Length);
                    for (int i = 0; i < srcRem.Length; i++)
                    {
                        dstRem[i] += srcRem[i];
                    }
                }
            }
        }

        /// <summary>
        /// Multiplies the specified samples faster, with the given <paramref name="scale"/>.
        /// </summary>
        /// <param name="span">The span to multiply.</param>
        /// <param name="scale">The value to be multiplied.</param>
        public static void FastScalarMultiply(this Span<float> span, float scale = default)
        {
            if (Vector<float>.Count > span.Length)
            {
                for (int i = 0; i < span.Length; i++)
                {
                    span[i] *= scale;
                }
            }
            else
            {
                var spanV = MemoryMarshal.Cast<float, Vector<float>>(span);
                var scaleV = new Vector<float>(scale);
                for (int i = 0; i < spanV.Length; i++)
                {
                    spanV[i] *= scaleV;
                }
                var spanR = span.Slice(spanV.Length * Vector<float>.Count);
                for (int i = 0; i < spanR.Length; i++)
                {
                    spanR[i] *= scale;
                }
            }
        }

        /// <summary>
        /// Mixes the <paramref name="samplesToMix"/> to <paramref name="buffer"/>.
        /// </summary>
        /// <param name="samplesToMix">The samples to add.</param>
        /// <param name="buffer">The buffer.</param>
        /// <param name="scale">The scale to scale <paramref name="samplesToMix"/>.</param>
        /// <exception cref="ArgumentException">samplesToMix</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FastMix(ReadOnlySpan<float> samplesToMix, Span<float> buffer, float scale)
        {
            if (samplesToMix.Length > buffer.Length) throw new ArgumentException("", nameof(samplesToMix));
            unsafe
            {
                (int newLength, int remainder) = MathI.FloorStepRem(samplesToMix.Length, Vector<float>.Count);
                if (newLength != 0)
                {
                    var scaleV = new Vector<float>(scale);
                    var src = MemoryMarshal.Cast<float, Vector<float>>(samplesToMix);
                    var dst = MemoryMarshal.Cast<float, Vector<float>>(buffer).Slice(0, src.Length);
                    for (int i = 0; i < src.Length; i++)
                    {
                        dst[i] += scaleV * src[i];
                    }
                }
                if (remainder != 0)
                {
                    var srcRem = samplesToMix.Slice(newLength);
                    var dstRem = buffer.Slice(newLength).Slice(0, srcRem.Length);
                    for (int i = 0; i < srcRem.Length; i++)
                    {
                        dstRem[i] += srcRem[i] * scale;
                    }
                }
            }
        }

        /// <summary>
        /// Mixes the <paramref name="samplesA"/> and <paramref name="samplesB"/> to <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">The output buffer.</param>
        /// <param name="samplesA">The samples a.</param>
        /// <param name="volumeA">The volume of <paramref name="samplesA"/>.</param>
        /// <param name="samplesB">The samples b.</param>
        /// <param name="volumeB">The volume of <paramref name="samplesB"/>.</param>
        /// <exception cref="ArgumentException">
        /// buffer must not be shorter than samplesA or samplesB! - buffer
        /// or
        /// samplesA must be as long as samplesB! - samplesA
        /// </exception>
        public static void FastMix(Span<float> buffer, ReadOnlySpan<float> samplesA, float volumeA, ReadOnlySpan<float> samplesB, float volumeB)
        {
            // Validation
            if (buffer.Length < samplesA.Length || buffer.Length < samplesB.Length)
                throw new ArgumentException("buffer must not be shorter than samplesA or samplesB!", nameof(buffer));
            if (samplesA.Length != samplesB.Length) throw new ArgumentException("samplesA must be as long as samplesB!", nameof(samplesA));

            // Preparation
            buffer = buffer.Slice(0, samplesA.Length);
            samplesB = samplesB.Slice(0, samplesA.Length);
            unsafe
            {
                (int newLength, int remainder) = MathI.FloorStepRem(samplesA.Length, Vector<float>.Count);
                if (newLength != 0)
                {
                    var scaleVA = new Vector<float>(volumeA);
                    var scaleVB = new Vector<float>(volumeB);
                    var srcA = MemoryMarshal.Cast<float, Vector<float>>(samplesA);
                    var srcB = MemoryMarshal.Cast<float, Vector<float>>(samplesB).Slice(0, srcA.Length);
                    var dst = MemoryMarshal.Cast<float, Vector<float>>(buffer).Slice(0, srcA.Length);
                    for (int i = 0; i < srcA.Length; i++)
                    {
                        var sA = srcA[i];
                        var sB = srcB[i];
                        sA *= scaleVA;
                        sB *= scaleVB;
                        dst[i] += sA + sB;
                    }
                }
                if (remainder != 0)
                {
                    var srcARem = samplesA.Slice(newLength);
                    var srcBRem = samplesB.Slice(newLength).Slice(0, srcARem.Length);
                    var dstRem = buffer.Slice(newLength).Slice(0, srcARem.Length);
                    for (int i = 0; i < srcARem.Length; i++)
                    {
                        float sA = srcARem[i];
                        float sB = srcBRem[i];
                        sA *= volumeA;
                        sB *= volumeB;
                        dstRem[i] += sA + sB;
                    }
                }
            }
        }

        #endregion SIMD-Related Functions

        #region QuickFill

        /// <summary>
        /// Quickly (but slower than <see cref="FastFill(Span{float}, float)"/>) fills the specified memory region, with the given <paramref name="value"/>.
        /// </summary>
        /// <typeparam name="TSample">The type of the sample.</typeparam>
        /// <param name="span">The span to fill.</param>
        /// <param name="value">The value to fill with.</param>
        public static void QuickFill<TSample>(this Span<TSample> span, TSample value)
        {
            if (span.Length < 32)
            {
                span.Fill(value);
                return;
            }
            var fillled = span.SliceWhile(16);
            var remaining = span.Slice(fillled.Length);
            fillled.Fill(value);
            do
            {
                fillled.CopyTo(remaining);
                remaining = remaining.Slice(fillled.Length);
                fillled = span.SliceWhile(fillled.Length << 1);
            } while (remaining.Length >= fillled.Length);
            fillled.Slice(fillled.Length - remaining.Length).CopyTo(remaining);
        }

        #endregion QuickFill

        #region MoveByOffset

        /// <summary>
        /// Moves the elements of specified <paramref name="span"/> right by 1 element.
        /// </summary>
        /// <typeparam name="TSample"></typeparam>
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
        public static Span<T> SliceWhileIfLongerThan<T>(this Span<T> span, ulong maxLength)
            => maxLength > int.MaxValue ? span : span.SliceWhileIfLongerThan((int)maxLength);

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

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static void ReverseEndiannessAdvSimd(Span<ulong> span)
        {
            var q = MemoryUtils.CastSplit<ulong, Vector128<ulong>>(span, out var rem);
            for (int i = 0; i < q.Length; i++)
            {
                var t = q[i];
                q[i] = AdvSimd.ReverseElement8(t);  //REV64 Vd.16B, Vn.16B
            }
            ReverseEndiannessSimple(rem);
        }

#endif
#if NETCOREAPP3_1_OR_GREATER

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static void ReverseEndiannessAvx2(Span<ulong> span)
        {
            var mask = Vector128.Create(0x0706050403020100ul, 0x0f0e0d0c0b0a0908ul).AsByte();
            var mask256 = Vector256.Create(mask, mask);
            var qq = MemoryUtils.CastSplit<ulong, (Vector256<byte> v0, Vector256<byte> v1, Vector256<byte> v2, Vector256<byte> v3, Vector256<byte> v4, Vector256<byte> v5, Vector256<byte> v6, Vector256<byte> v7)>(span, out var rem);
            for (int i = 0; i < qq.Length; i++)
            {
                ref var t = ref qq[i];
                var x = t.v0;   //Let RyuJIT not to emit unnecessary 'lea' instructions
                var v0 = Avx2.Shuffle(x.AsByte(), mask256);
                x = t.v1;
                var v1 = Avx2.Shuffle(x.AsByte(), mask256);
                x = t.v2;
                var v2 = Avx2.Shuffle(x.AsByte(), mask256);
                x = t.v3;
                var v3 = Avx2.Shuffle(x.AsByte(), mask256);
                t.v0 = v0;
                x = t.v4;
                v0 = Avx2.Shuffle(x.AsByte(), mask256);
                t.v1 = v1;
                x = t.v5;
                v1 = Avx2.Shuffle(x.AsByte(), mask256);
                t.v2 = v2;
                x = t.v6;
                v2 = Avx2.Shuffle(x.AsByte(), mask256);
                t.v3 = v3;
                x = t.v7;
                v3 = Avx2.Shuffle(x.AsByte(), mask256);
                t.v4 = v0;
                t.v5 = v1;
                t.v6 = v2;
                t.v7 = v3;
            }
            var q = MemoryUtils.CastSplit<ulong, Vector256<ulong>>(rem, out rem);
            for (int i = 0; i < q.Length; i++)
            {
                var t = q[i];
                q[i] = Avx2.Shuffle(t.AsByte(), mask256).AsUInt64();
            }
            ReverseEndiannessSimple(rem);
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static void ReverseEndiannessSsse3(Span<ulong> span)
        {
            var mask = Vector128.Create(0x0706050403020100ul, 0x0f0e0d0c0b0a0908ul).AsByte();
            var qq = MemoryUtils.CastSplit<ulong, (Vector128<byte> v0, Vector128<byte> v1, Vector128<byte> v2, Vector128<byte> v3, Vector128<byte> v4, Vector128<byte> v5, Vector128<byte> v6, Vector128<byte> v7)>(span, out var rem);
            for (int i = 0; i < qq.Length; i++)
            {
                ref var t = ref qq[i];

                var x = t.v0;
                var v0 = Ssse3.Shuffle(x.AsByte(), mask);
                x = t.v1;
                var v1 = Ssse3.Shuffle(x.AsByte(), mask);
                x = t.v2;
                var v2 = Ssse3.Shuffle(x.AsByte(), mask);
                x = t.v3;
                var v3 = Ssse3.Shuffle(x.AsByte(), mask);
                t.v0 = v0;
                x = t.v4;
                v0 = Ssse3.Shuffle(x.AsByte(), mask);
                t.v1 = v1;
                x = t.v5;
                v1 = Ssse3.Shuffle(x.AsByte(), mask);
                t.v2 = v2;
                x = t.v6;
                v2 = Ssse3.Shuffle(x.AsByte(), mask);
                t.v3 = v3;
                x = t.v7;
                v3 = Ssse3.Shuffle(x.AsByte(), mask);
                t.v4 = v0;
                t.v5 = v1;
                t.v6 = v2;
                t.v7 = v3;
            }
            var q = MemoryUtils.CastSplit<ulong, Vector128<ulong>>(rem, out rem);
            for (int i = 0; i < q.Length; i++)
            {
                var t = q[i];
                q[i] = Ssse3.Shuffle(t.AsByte(), mask).AsUInt64();
            }
            ReverseEndiannessSimple(rem);
        }

#endif

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ReverseEndiannessFallback(Span<ulong> span)
        {
            var qq = MemoryUtils.CastSplit<ulong, (ulong v0, ulong v1, ulong v2, ulong v3, ulong v4, ulong v5, ulong v6, ulong v7)>(span, out var rem);
            for (int i = 0; i < qq.Length; i++)
            {
                ref var t = ref qq[i];

                var x = t.v0;
                var v0 = BinaryPrimitives.ReverseEndianness(x);
                x = t.v1;
                var v1 = BinaryPrimitives.ReverseEndianness(x);
                x = t.v2;
                var v2 = BinaryPrimitives.ReverseEndianness(x);
                x = t.v3;
                var v3 = BinaryPrimitives.ReverseEndianness(x);
                t.v0 = v0;
                x = t.v4;
                v0 = BinaryPrimitives.ReverseEndianness(x);
                t.v1 = v1;
                x = t.v5;
                v1 = BinaryPrimitives.ReverseEndianness(x);
                t.v2 = v2;
                x = t.v6;
                v2 = BinaryPrimitives.ReverseEndianness(x);
                t.v3 = v3;
                x = t.v7;
                v3 = BinaryPrimitives.ReverseEndianness(x);
                t.v4 = v0;
                t.v5 = v1;
                t.v6 = v2;
                t.v7 = v3;
            }
            ReverseEndiannessSimple(rem);
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
