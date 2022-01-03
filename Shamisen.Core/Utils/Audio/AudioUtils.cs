using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using Shamisen.Optimization;
using Shamisen.Primitives;
using Shamisen;
#if NET5_0_OR_GREATER

using System.Runtime.Intrinsics.Arm;

#endif
#if NETCOREAPP3_1_OR_GREATER

using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

using Shamisen.Utils.Intrinsics;

#endif
namespace Shamisen.Utils
{
    /// <summary>
    /// Contains some utility functions for manipulating audio data.
    /// </summary>
    public static partial class AudioUtils
    {

        #region FastAdd

        /// <summary>
        /// Adds the <paramref name="samplesToAdd"/> to <paramref name="buffer"/>.
        /// </summary>
        /// <param name="samplesToAdd">The samples to add.</param>
        /// <param name="buffer">The buffer.</param>
        /// <exception cref="ArgumentException">samplesToAdd</exception>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static void FastAdd(ReadOnlySpan<float> samplesToAdd, Span<float> buffer)
        {
#if NET5_0_OR_GREATER
            if (AdvSimd.Arm64.IsSupported)
            {
                FastAddAdvSimdArm64(buffer, buffer, samplesToAdd);
                return;
            }
            if (AdvSimd.IsSupported)
            {
                FastAddAdvSimd(buffer, buffer, samplesToAdd);
                return;
            }
#endif
            FastAddStandardVariable(buffer, buffer, samplesToAdd);
        }

        /// <summary>
        /// Adds the <paramref name="samplesA"/> and <paramref name="samplesB"/>, and stores to <paramref name="buffer"/>.
        /// </summary>
        /// <param name="samplesA">The samples to add.</param>
        /// <param name="samplesB">The samples to add.</param>
        /// <param name="buffer">The destination.</param>
        /// <exception cref="ArgumentException">samplesToAdd</exception>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static void FastAdd(Span<float> buffer, ReadOnlySpan<float> samplesA, ReadOnlySpan<float> samplesB)
        {
#if NET5_0_OR_GREATER
            if (AdvSimd.Arm64.IsSupported)
            {
                FastAddAdvSimdArm64(buffer, samplesA, samplesB);
                return;
            }
            if (AdvSimd.IsSupported)
            {
                FastAddAdvSimd(buffer, samplesA, samplesB);
                return;
            }
#endif
            FastAddStandardVariable(buffer, samplesA, samplesB);
        }
#if NET5_0_OR_GREATER
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void FastAddAdvSimd(Span<float> buffer, ReadOnlySpan<float> samplesA, ReadOnlySpan<float> samplesB)
        {
            nint i, length = MathI.Min(buffer.Length, MathI.Min(samplesA.Length, samplesB.Length));
            ref var rsA = ref MemoryMarshal.GetReference(samplesA);
            ref var rsB = ref MemoryMarshal.GetReference(samplesB);
            ref var rD = ref MemoryMarshal.GetReference(buffer);
            var olen = length - 2 * 4 + 1;
            for (i = 0; i < olen; i += 2 * 4)
            {
                var sA0 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rsA, i + 0));
                var sA1 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rsA, i + 4));
                var sB0 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rsB, i + 0));
                sA0 = AdvSimd.Add(sA0, sB0);
                var sB1 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rsB, i + 4));
                sA1 = AdvSimd.Add(sA1, sB1);
                Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rD, i + 0)) = sA0;
                Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rD, i + 4)) = sA1;
            }
            for (; i < length; i++)
            {
                var sA = Unsafe.Add(ref rsA, i);
                Unsafe.Add(ref rD, i) = sA + Unsafe.Add(ref rsB, i);
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void FastAddAdvSimdArm64(Span<float> buffer, ReadOnlySpan<float> samplesA, ReadOnlySpan<float> samplesB)
        {
            nint i, length = MathI.Min(buffer.Length, MathI.Min(samplesA.Length, samplesB.Length));
            ref var rsA = ref MemoryMarshal.GetReference(samplesA);
            ref var rsB = ref MemoryMarshal.GetReference(samplesB);
            ref var rD = ref MemoryMarshal.GetReference(buffer);
            //This is a loop for Cortex-A75, probably not optimal in A64FX.
            //The `ldp` instruction isn't available at all, so we use simpler load.
            var olen = length - 2 * 4 + 1;
            for (i = 0; i < olen; i += 2 * 4)
            {
                var sA0 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rsA, i + 0));
                var sA1 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rsA, i + 4));
                var sB0 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rsB, i + 0));
                sA0 = AdvSimd.Add(sA0, sB0);
                var sB1 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rsB, i + 4));
                sA1 = AdvSimd.Add(sA1, sB1);
                AdvSimdUtils.Arm64.StorePair(ref Unsafe.Add(ref rD, i), sA0, sA1);
            }
            for (; i < length; i++)
            {
                var sA = Unsafe.Add(ref rsA, i);
                Unsafe.Add(ref rD, i) = sA + Unsafe.Add(ref rsB, i);
            }
        }
#endif
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void FastAddStandardFixed(Span<float> buffer, ReadOnlySpan<float> samplesA, ReadOnlySpan<float> samplesB)
        {
            unsafe
            {
                nint i, length = MathI.Min(buffer.Length, MathI.Min(samplesA.Length, samplesB.Length));
                ref var rsA = ref MemoryMarshal.GetReference(samplesA);
                ref var rsB = ref MemoryMarshal.GetReference(samplesB);
                ref var rD = ref MemoryMarshal.GetReference(buffer);
                //This is a loop for Haswell, probably not optimal in Alder Lake.
                var olen = length - 8 * 4 + 1;
                for (i = 0; i < olen; i += 8 * 4)
                {
                    var sA0 = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rsA, i + 0 * 4));
                    var sA1 = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rsA, i + 1 * 4));
                    var sA2 = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rsA, i + 2 * 4));
                    var sA3 = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rsA, i + 3 * 4));
                    sA0 += Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rsB, i + 0 * 4));
                    sA1 += Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rsB, i + 1 * 4));
                    sA2 += Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rsB, i + 2 * 4));
                    sA3 += Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rsB, i + 3 * 4));
                    Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rD, i + 0 * 4)) = sA0;
                    Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rD, i + 1 * 4)) = sA1;
                    Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rD, i + 2 * 4)) = sA2;
                    Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rD, i + 3 * 4)) = sA3;
                    sA0 = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rsA, i + 4 * 4));
                    sA1 = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rsA, i + 5 * 4));
                    sA2 = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rsA, i + 6 * 4));
                    sA3 = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rsA, i + 7 * 4));
                    sA0 += Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rsB, i + 4 * 4));
                    sA1 += Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rsB, i + 5 * 4));
                    sA2 += Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rsB, i + 6 * 4));
                    sA3 += Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rsB, i + 7 * 4));
                    Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rD, i + 4 * 4)) = sA0;
                    Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rD, i + 5 * 4)) = sA1;
                    Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rD, i + 6 * 4)) = sA2;
                    Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rD, i + 7 * 4)) = sA3;
                }
                olen = length - 4 * 4 + 1;
                for (; i < olen; i += 4 * 4)
                {
                    var sA0 = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rsA, i + 0 * 4));
                    var sA1 = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rsA, i + 1 * 4));
                    var sA2 = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rsA, i + 2 * 4));
                    var sA3 = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rsA, i + 3 * 4));
                    sA0 += Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rsB, i + 0 * 4));
                    sA1 += Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rsB, i + 1 * 4));
                    sA2 += Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rsB, i + 2 * 4));
                    sA3 += Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rsB, i + 3 * 4));
                    Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rD, i + 0 * 4)) = sA0;
                    Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rD, i + 1 * 4)) = sA1;
                    Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rD, i + 2 * 4)) = sA2;
                    Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rD, i + 3 * 4)) = sA3;
                }
                for (; i < length; i++)
                {
                    var sA = Unsafe.Add(ref rsA, i);
                    Unsafe.Add(ref rD, i) = sA + Unsafe.Add(ref rsB, i);
                }
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void FastAddStandardVariable(Span<float> buffer, ReadOnlySpan<float> samplesA, ReadOnlySpan<float> samplesB)
        {
            unsafe
            {
                nint i, length = MathI.Min(buffer.Length, MathI.Min(samplesA.Length, samplesB.Length));
                ref var rsA = ref MemoryMarshal.GetReference(samplesA);
                ref var rsB = ref MemoryMarshal.GetReference(samplesB);
                ref var rD = ref MemoryMarshal.GetReference(buffer);
                //This is a loop for Haswell. Can be suboptimal in CPUs with full non-restricted AVX-512 support.
                var olen = length - 8 * Vector<float>.Count + 1;
                for (i = 0; i < olen; i += 8 * Vector<float>.Count)
                {
                    var sA0 = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rsA, i + 0 * Vector<float>.Count));
                    var sA1 = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rsA, i + 1 * Vector<float>.Count));
                    var sA2 = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rsA, i + 2 * Vector<float>.Count));
                    var sA3 = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rsA, i + 3 * Vector<float>.Count));
                    sA0 += Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rsB, i + 0 * Vector<float>.Count));
                    sA1 += Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rsB, i + 1 * Vector<float>.Count));
                    sA2 += Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rsB, i + 2 * Vector<float>.Count));
                    sA3 += Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rsB, i + 3 * Vector<float>.Count));
                    Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rD, i + 0 * Vector<float>.Count)) = sA0;
                    Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rD, i + 1 * Vector<float>.Count)) = sA1;
                    Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rD, i + 2 * Vector<float>.Count)) = sA2;
                    Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rD, i + 3 * Vector<float>.Count)) = sA3;
                    sA0 = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rsA, i + 4 * Vector<float>.Count));
                    sA1 = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rsA, i + 5 * Vector<float>.Count));
                    sA2 = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rsA, i + 6 * Vector<float>.Count));
                    sA3 = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rsA, i + 7 * Vector<float>.Count));
                    sA0 += Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rsB, i + 4 * Vector<float>.Count));
                    sA1 += Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rsB, i + 5 * Vector<float>.Count));
                    sA2 += Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rsB, i + 6 * Vector<float>.Count));
                    sA3 += Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rsB, i + 7 * Vector<float>.Count));
                    Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rD, i + 4 * Vector<float>.Count)) = sA0;
                    Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rD, i + 5 * Vector<float>.Count)) = sA1;
                    Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rD, i + 6 * Vector<float>.Count)) = sA2;
                    Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rD, i + 7 * Vector<float>.Count)) = sA3;
                }
                olen = length - 4 * Vector<float>.Count + 1;
                for (; i < olen; i += 4 * Vector<float>.Count)
                {
                    var sA0 = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rsA, i + 0 * Vector<float>.Count));
                    var sA1 = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rsA, i + 1 * Vector<float>.Count));
                    var sA2 = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rsA, i + 2 * Vector<float>.Count));
                    var sA3 = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rsA, i + 3 * Vector<float>.Count));
                    sA0 += Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rsB, i + 0 * Vector<float>.Count));
                    sA1 += Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rsB, i + 1 * Vector<float>.Count));
                    sA2 += Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rsB, i + 2 * Vector<float>.Count));
                    sA3 += Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rsB, i + 3 * Vector<float>.Count));
                    Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rD, i + 0 * Vector<float>.Count)) = sA0;
                    Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rD, i + 1 * Vector<float>.Count)) = sA1;
                    Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rD, i + 2 * Vector<float>.Count)) = sA2;
                    Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rD, i + 3 * Vector<float>.Count)) = sA3;
                }
                for (; i < length; i++)
                {
                    var sA = Unsafe.Add(ref rsA, i);
                    Unsafe.Add(ref rD, i) = sA + Unsafe.Add(ref rsB, i);
                }
            }
        }
        #endregion

        #region FastScalarMultiply

        /// <summary>
        /// Multiplies the specified samples faster, with the given <paramref name="scale"/>.
        /// </summary>
        /// <param name="span">The span to multiply.</param>
        /// <param name="scale">The value to be multiplied.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static void FastScalarMultiply(this Span<float> span, float scale = default) => FastScalarMultiplyStandardVariable(span, span, scale);

        /// <summary>
        /// Multiplies the specified samples faster, with the given <paramref name="scale"/>.
        /// </summary>
        /// <param name="span">The span to multiply.</param>
        /// <param name="scale">The value to be multiplied.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static void FastScalarMultiply(this Span<double> span, double scale = default)
        {
            if (Vector<double>.Count > span.Length)
            {
                ref var rdi = ref MemoryMarshal.GetReference(span);
                nint i, length = span.Length;
                for (i = 0; i < length; i++)
                {
                    var v = Unsafe.Add(ref rdi, i) * scale;
                    Unsafe.Add(ref rdi, i) = v;
                }
            }
            else
            {
                FastScalarMultiplyStandardVariable(span, span, scale);
            }
        }

        /// <summary>
        /// Vectorized path using <see cref="Vector{T}"/> which uses variable-sized vectors.<br/>
        /// Performs better in either ARMv8, Rocket Lake or later, or pre-Sandy-Bridge x64 CPUs due to CPU clock limits.<br/>
        /// Future versions of .NET may improve performance if <see cref="Vector{T}"/> utilizes either x64 AVX-512 or ARMv8.2-A SVE.
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="source"></param>
        /// <param name="scale"></param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void FastScalarMultiplyStandardVariable(Span<float> destination, ReadOnlySpan<float> source, float scale)
        {
            var scaleV = new Vector<float>(scale);
            ref var rsi = ref MemoryMarshal.GetReference(source);
            ref var rdi = ref MemoryMarshal.GetReference(destination);
            nint i, length = MathI.Min(source.Length, destination.Length);
            for (i = 0; i < length - (Vector<float>.Count * 8 - 1); i += Vector<float>.Count * 8)
            {
                ref var r11 = ref Unsafe.Add(ref rdi, i);
                var v0 = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rsi, i + 0 * Vector<float>.Count)) * scaleV;
                var v1 = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rsi, i + 1 * Vector<float>.Count)) * scaleV;
                var v2 = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rsi, i + 2 * Vector<float>.Count)) * scaleV;
                var v3 = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rsi, i + 3 * Vector<float>.Count)) * scaleV;
                Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref r11, 0 * Vector<float>.Count)) = v0;
                Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref r11, 1 * Vector<float>.Count)) = v1;
                Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref r11, 2 * Vector<float>.Count)) = v2;
                Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref r11, 3 * Vector<float>.Count)) = v3;
                v0 = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rsi, i + 4 * Vector<float>.Count)) * scaleV;
                v1 = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rsi, i + 5 * Vector<float>.Count)) * scaleV;
                v2 = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rsi, i + 6 * Vector<float>.Count)) * scaleV;
                v3 = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rsi, i + 7 * Vector<float>.Count)) * scaleV;
                Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref r11, 4 * Vector<float>.Count)) = v0;
                Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref r11, 5 * Vector<float>.Count)) = v1;
                Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref r11, 6 * Vector<float>.Count)) = v2;
                Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref r11, 7 * Vector<float>.Count)) = v3;
            }
            if (i < length - (Vector<float>.Count * 4 - 1))
            {
                ref var r11 = ref Unsafe.Add(ref rdi, i);
                var v0 = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rsi, i + 0 * Vector<float>.Count)) * scaleV;
                var v1 = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rsi, i + 1 * Vector<float>.Count)) * scaleV;
                var v2 = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rsi, i + 2 * Vector<float>.Count)) * scaleV;
                var v3 = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rsi, i + 3 * Vector<float>.Count)) * scaleV;
                Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref r11, 0 * Vector<float>.Count)) = v0;
                Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref r11, 1 * Vector<float>.Count)) = v1;
                Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref r11, 2 * Vector<float>.Count)) = v2;
                Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref r11, 3 * Vector<float>.Count)) = v3;
                i += Vector<float>.Count * 4;
            }
            for (; i < length; i++)
            {
                var v = Unsafe.Add(ref rsi, i) * scaleV[0];
                Unsafe.Add(ref rdi, i) = v;
            }
        }

        /// <summary>
        /// Vectorized path using <see cref="Vector{T}"/> which uses variable-sized vectors.<br/>
        /// Future versions of .NET may improve performance if <see cref="Vector{T}"/> utilizes either x64 AVX-512 or ARMv8.2-A SVE.
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="source"></param>
        /// <param name="scale"></param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void FastScalarMultiplyStandardVariable(Span<double> destination, ReadOnlySpan<double> source, double scale)
        {
            var scaleV = new Vector<double>(scale);
            ref var rsi = ref MemoryMarshal.GetReference(source);
            ref var rdi = ref MemoryMarshal.GetReference(destination);
            nint i, length = MathI.Min(source.Length, destination.Length);
            for (i = 0; i < length - (Vector<double>.Count * 8 - 1); i += Vector<double>.Count * 8)
            {
                ref var r11 = ref Unsafe.Add(ref rdi, i);
                var v0 = Unsafe.As<double, Vector<double>>(ref Unsafe.Add(ref rsi, i + 0 * Vector<double>.Count)) * scaleV;
                var v1 = Unsafe.As<double, Vector<double>>(ref Unsafe.Add(ref rsi, i + 1 * Vector<double>.Count)) * scaleV;
                var v2 = Unsafe.As<double, Vector<double>>(ref Unsafe.Add(ref rsi, i + 2 * Vector<double>.Count)) * scaleV;
                var v3 = Unsafe.As<double, Vector<double>>(ref Unsafe.Add(ref rsi, i + 3 * Vector<double>.Count)) * scaleV;
                Unsafe.As<double, Vector<double>>(ref Unsafe.Add(ref r11, 0 * Vector<double>.Count)) = v0;
                Unsafe.As<double, Vector<double>>(ref Unsafe.Add(ref r11, 1 * Vector<double>.Count)) = v1;
                Unsafe.As<double, Vector<double>>(ref Unsafe.Add(ref r11, 2 * Vector<double>.Count)) = v2;
                Unsafe.As<double, Vector<double>>(ref Unsafe.Add(ref r11, 3 * Vector<double>.Count)) = v3;
                v0 = Unsafe.As<double, Vector<double>>(ref Unsafe.Add(ref rsi, i + 4 * Vector<double>.Count)) * scaleV;
                v1 = Unsafe.As<double, Vector<double>>(ref Unsafe.Add(ref rsi, i + 5 * Vector<double>.Count)) * scaleV;
                v2 = Unsafe.As<double, Vector<double>>(ref Unsafe.Add(ref rsi, i + 6 * Vector<double>.Count)) * scaleV;
                v3 = Unsafe.As<double, Vector<double>>(ref Unsafe.Add(ref rsi, i + 7 * Vector<double>.Count)) * scaleV;
                Unsafe.As<double, Vector<double>>(ref Unsafe.Add(ref r11, 4 * Vector<double>.Count)) = v0;
                Unsafe.As<double, Vector<double>>(ref Unsafe.Add(ref r11, 5 * Vector<double>.Count)) = v1;
                Unsafe.As<double, Vector<double>>(ref Unsafe.Add(ref r11, 6 * Vector<double>.Count)) = v2;
                Unsafe.As<double, Vector<double>>(ref Unsafe.Add(ref r11, 7 * Vector<double>.Count)) = v3;
            }
            if (i < length - (Vector<double>.Count * 4 - 1))
            {
                ref var r11 = ref Unsafe.Add(ref rdi, i);
                var v0 = Unsafe.As<double, Vector<double>>(ref Unsafe.Add(ref rsi, i + 0 * Vector<double>.Count)) * scaleV;
                var v1 = Unsafe.As<double, Vector<double>>(ref Unsafe.Add(ref rsi, i + 1 * Vector<double>.Count)) * scaleV;
                var v2 = Unsafe.As<double, Vector<double>>(ref Unsafe.Add(ref rsi, i + 2 * Vector<double>.Count)) * scaleV;
                var v3 = Unsafe.As<double, Vector<double>>(ref Unsafe.Add(ref rsi, i + 3 * Vector<double>.Count)) * scaleV;
                Unsafe.As<double, Vector<double>>(ref Unsafe.Add(ref r11, 0 * Vector<double>.Count)) = v0;
                Unsafe.As<double, Vector<double>>(ref Unsafe.Add(ref r11, 1 * Vector<double>.Count)) = v1;
                Unsafe.As<double, Vector<double>>(ref Unsafe.Add(ref r11, 2 * Vector<double>.Count)) = v2;
                Unsafe.As<double, Vector<double>>(ref Unsafe.Add(ref r11, 3 * Vector<double>.Count)) = v3;
                i += Vector<double>.Count * 4;
            }
            for (; i < length; i++)
            {
                var v = Unsafe.Add(ref rsi, i) * scaleV[0];
                Unsafe.Add(ref rdi, i) = v;
            }
        }

        #endregion

        #region FastMultiply

        /// <summary>
        /// Multiplies specified <paramref name="sourceA"/> and <paramref name="sourceB"/> and stores to <paramref name="destination"/>.
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="sourceA"></param>
        /// <param name="sourceB"></param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static void FastMultiply(Span<float> destination, ReadOnlySpan<float> sourceA, ReadOnlySpan<float> sourceB)
        {
            var min = MathI.Rectify(MathI.Min(MathI.Min(sourceA.Length, sourceB.Length), destination.Length));
            if (destination.Length > min)
            {
                destination.Slice(min).FastFill(0);
            }
            if (min < 1) return;
            destination = destination.SliceWhileIfLongerThan(min);
            sourceA = sourceA.SliceWhileIfLongerThan(min);
            sourceB = sourceB.SliceWhileIfLongerThan(min);
            FastMultiplyStandardVariable(destination, sourceA, sourceB);
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void FastMultiplyStandardVariable(Span<float> destination, ReadOnlySpan<float> sourceA, ReadOnlySpan<float> sourceB)
        {
            unsafe
            {
                nint i, length = MathI.Min(destination.Length, MathI.Min(sourceA.Length, sourceB.Length));
                ref var x10 = ref MemoryMarshal.GetReference(sourceA);
                ref var x11 = ref MemoryMarshal.GetReference(sourceB);
                ref var x12 = ref MemoryMarshal.GetReference(destination);
                var olen = length - 8 * Vector<float>.Count + 1;
                for (i = 0; i < olen; i += 8 * Vector<float>.Count)
                {
                    //Getting rid of LEA-hell.
                    ref var x13 = ref Unsafe.Add(ref x10, i);
                    ref var x14 = ref Unsafe.Add(ref x11, i);
                    ref var x15 = ref Unsafe.Add(ref x12, i);
                    var sA0 = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref x13, 0 * Vector<float>.Count));
                    var sA1 = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref x13, 1 * Vector<float>.Count));
                    var sA2 = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref x13, 2 * Vector<float>.Count));
                    var sA3 = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref x13, 3 * Vector<float>.Count));
                    sA0 *= Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref x14, 0 * Vector<float>.Count));
                    sA1 *= Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref x14, 1 * Vector<float>.Count));
                    sA2 *= Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref x14, 2 * Vector<float>.Count));
                    sA3 *= Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref x14, 3 * Vector<float>.Count));
                    Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref x15, 0 * Vector<float>.Count)) = sA0;
                    Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref x15, 1 * Vector<float>.Count)) = sA1;
                    Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref x15, 2 * Vector<float>.Count)) = sA2;
                    Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref x15, 3 * Vector<float>.Count)) = sA3;
                    sA0 = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref x13, 4 * Vector<float>.Count));
                    sA1 = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref x13, 5 * Vector<float>.Count));
                    sA2 = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref x13, 6 * Vector<float>.Count));
                    sA3 = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref x13, 7 * Vector<float>.Count));
                    sA0 *= Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref x14, 4 * Vector<float>.Count));
                    sA1 *= Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref x14, 5 * Vector<float>.Count));
                    sA2 *= Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref x14, 6 * Vector<float>.Count));
                    sA3 *= Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref x14, 7 * Vector<float>.Count));
                    Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref x15, 4 * Vector<float>.Count)) = sA0;
                    Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref x15, 5 * Vector<float>.Count)) = sA1;
                    Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref x15, 6 * Vector<float>.Count)) = sA2;
                    Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref x15, 7 * Vector<float>.Count)) = sA3;
                }
                olen = length - 4 * Vector<float>.Count + 1;
                for (; i < olen; i += 4 * Vector<float>.Count)
                {
                    ref var x13 = ref Unsafe.Add(ref x10, i);
                    ref var x14 = ref Unsafe.Add(ref x11, i);
                    ref var x15 = ref Unsafe.Add(ref x12, i);
                    var sA0 = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref x13, 0 * Vector<float>.Count));
                    var sA1 = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref x13, 1 * Vector<float>.Count));
                    var sA2 = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref x13, 2 * Vector<float>.Count));
                    var sA3 = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref x13, 3 * Vector<float>.Count));
                    sA0 *= Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref x14, 0 * Vector<float>.Count));
                    sA1 *= Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref x14, 1 * Vector<float>.Count));
                    sA2 *= Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref x14, 2 * Vector<float>.Count));
                    sA3 *= Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref x14, 3 * Vector<float>.Count));
                    Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref x15, 0 * Vector<float>.Count)) = sA0;
                    Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref x15, 1 * Vector<float>.Count)) = sA1;
                    Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref x15, 2 * Vector<float>.Count)) = sA2;
                    Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref x15, 3 * Vector<float>.Count)) = sA3;
                }
                for (; i < length; i++)
                {
                    var sA = Unsafe.Add(ref x10, i);
                    Unsafe.Add(ref x12, i) = sA * Unsafe.Add(ref x11, i);
                }
            }
        }

        #endregion

        #region FastMixTwoOperands

        /// <summary>
        /// Mixes the <paramref name="samplesToMix"/> to <paramref name="buffer"/>.
        /// </summary>
        /// <param name="samplesToMix">The samples to add.</param>
        /// <param name="buffer">The buffer.</param>
        /// <param name="scale">The scale to scale <paramref name="samplesToMix"/>.</param>
        /// <exception cref="ArgumentException">samplesToMix</exception>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static void FastMix(ReadOnlySpan<float> samplesToMix, Span<float> buffer, float scale)
        {
            var min = MathI.Min(samplesToMix.Length, buffer.Length);
            if (min < 1) return;
            samplesToMix = samplesToMix.SliceWhileIfLongerThan(min);
            buffer = buffer.SliceWhileIfLongerThan(min);
            if (Vector<float>.Count > 4 && Vector.IsHardwareAccelerated)
            {
                FastMixStandardVariable(samplesToMix, buffer, scale);
                return;
            }
            FastMixStandardFixed(samplesToMix, buffer, scale);
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static void FastMixStandardFixed(ReadOnlySpan<float> samplesToMix, Span<float> buffer, float scale)
        {
            nint i = 0;
            nint length = MathI.Min(samplesToMix.Length, buffer.Length);
            ref var rsi = ref MemoryMarshal.GetReference(samplesToMix);
            ref var rdi = ref MemoryMarshal.GetReference(buffer);
            var v15_4s = new Vector4(scale);
            var olen = length - 8 * 4 + 1;
            for (i = 0; i < olen; i += 8 * 4)
            {
                var v0_4s = v15_4s * Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rsi, i));
                var v1_4s = v15_4s * Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rsi, i + 4));
                var v2_4s = v15_4s * Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rsi, i + 8));
                var v3_4s = v15_4s * Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rsi, i + 12));
                v0_4s += Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rdi, i));
                v1_4s += Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rdi, i + 4));
                v2_4s += Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rdi, i + 8));
                v3_4s += Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rdi, i + 12));
                Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rdi, i)) = v0_4s;
                Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rdi, i + 4)) = v1_4s;
                Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rdi, i + 8)) = v2_4s;
                Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rdi, i + 12)) = v3_4s;
                v0_4s = v15_4s * Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rsi, i + 16));
                v1_4s = v15_4s * Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rsi, i + 20));
                v2_4s = v15_4s * Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rsi, i + 24));
                v3_4s = v15_4s * Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rsi, i + 28));
                v0_4s += Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rdi, i + 16));
                v1_4s += Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rdi, i + 20));
                v2_4s += Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rdi, i + 24));
                v3_4s += Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rdi, i + 28));
                Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rdi, i + 16)) = v0_4s;
                Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rdi, i + 20)) = v1_4s;
                Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rdi, i + 24)) = v2_4s;
                Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rdi, i + 28)) = v3_4s;
            }
            olen = length - 3;
            for (; i < olen; i += 4)
            {
                var v0_4s = v15_4s * Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rsi, i));
                var v4_4s = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rdi, i));
                v4_4s += v0_4s;
                Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rdi, i)) = v4_4s;
            }
            for (; i < length; i++)
            {
                var s0 = v15_4s.X * Unsafe.Add(ref rsi, i);
                var s4 = Unsafe.Add(ref rdi, i);
                s4 += s0;
                Unsafe.Add(ref rdi, i) = s4;
            }
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static void FastMixStandardVariable(ReadOnlySpan<float> samplesToMix, Span<float> buffer, float scale)
        {
            nint i = 0;
            nint length = MathI.Min(samplesToMix.Length, buffer.Length);
            ref var rsi = ref MemoryMarshal.GetReference(samplesToMix);
            ref var rdi = ref MemoryMarshal.GetReference(buffer);
            var v15_ns = new Vector<float>(scale);
            var olen = length - 8 * Vector<float>.Count + 1;
            for (i = 0; i < olen; i += 8 * Vector<float>.Count)
            {
                var v0_ns = v15_ns * Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rsi, i + 0 * Vector<float>.Count));
                var v1_ns = v15_ns * Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rsi, i + 1 * Vector<float>.Count));
                var v2_ns = v15_ns * Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rsi, i + 2 * Vector<float>.Count));
                var v3_ns = v15_ns * Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rsi, i + 3 * Vector<float>.Count));
                v0_ns += Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rdi, i + 0 * Vector<float>.Count));
                v1_ns += Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rdi, i + 1 * Vector<float>.Count));
                v2_ns += Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rdi, i + 2 * Vector<float>.Count));
                v3_ns += Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rdi, i + 3 * Vector<float>.Count));
                Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rdi, i + 0 * Vector<float>.Count)) = v0_ns;
                Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rdi, i + 1 * Vector<float>.Count)) = v1_ns;
                Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rdi, i + 2 * Vector<float>.Count)) = v2_ns;
                Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rdi, i + 3 * Vector<float>.Count)) = v3_ns;
                v0_ns = v15_ns * Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rsi, i + 4 * Vector<float>.Count));
                v1_ns = v15_ns * Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rsi, i + 5 * Vector<float>.Count));
                v2_ns = v15_ns * Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rsi, i + 6 * Vector<float>.Count));
                v3_ns = v15_ns * Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rsi, i + 7 * Vector<float>.Count));
                v0_ns += Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rdi, i + 4 * Vector<float>.Count));
                v1_ns += Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rdi, i + 5 * Vector<float>.Count));
                v2_ns += Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rdi, i + 6 * Vector<float>.Count));
                v3_ns += Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rdi, i + 7 * Vector<float>.Count));
                Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rdi, i + 4 * Vector<float>.Count)) = v0_ns;
                Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rdi, i + 5 * Vector<float>.Count)) = v1_ns;
                Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rdi, i + 6 * Vector<float>.Count)) = v2_ns;
                Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rdi, i + 7 * Vector<float>.Count)) = v3_ns;
            }
            olen = length - Vector<float>.Count + 1;
            for (; i < olen; i += Vector<float>.Count)
            {
                var v0_ns = v15_ns * Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rsi, i));
                var v4_ns = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rdi, i));
                v4_ns += v0_ns;
                Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rdi, i)) = v4_ns;
            }
            for (; i < length; i++)
            {
                var s0 = v15_ns[0] * Unsafe.Add(ref rsi, i);
                var s4 = Unsafe.Add(ref rdi, i);
                s4 += s0;
                Unsafe.Add(ref rdi, i) = s4;
            }
        }
        #endregion

        #region FastMixThreeOperands
        /// <summary>
        /// Mixes the <paramref name="samplesA"/> and <paramref name="samplesB"/> to <paramref name="buffer"/>.
        /// The data inside <paramref name="buffer"/> will be destroyed.
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
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static void FastMix(Span<float> buffer, ReadOnlySpan<float> samplesA, float volumeA, ReadOnlySpan<float> samplesB, float volumeB)
        {
            // Validation
            var min = MathI.Rectify(MathI.Min(MathI.Min(samplesA.Length, samplesB.Length), buffer.Length));
            if (buffer.Length > min)
            {
                buffer.Slice(min).FastFill(0);
            }
            if (min < 1) return;
            buffer = buffer.SliceWhileIfLongerThan(min);
            samplesA = samplesA.SliceWhileIfLongerThan(min);
            samplesB = samplesB.SliceWhileIfLongerThan(min);
#if NETCOREAPP3_1_OR_GREATER
            if (Avx.IsSupported && min > 64)
            {
                FastMixAvx(buffer, samplesA, volumeA, samplesB, volumeB);
                return;
            }
            if (Sse.IsSupported)
            {
                FastMixSse(buffer, samplesA, volumeA, samplesB, volumeB);
                return;
            }
#endif
            if (Vector<float>.Count > 0 && Vector.IsHardwareAccelerated)
            {
                FastMixStandardVariable(buffer, samplesA, volumeA, samplesB, volumeB);
            }
            FastMixStandardFixed(buffer, samplesA, volumeA, samplesB, volumeB);
        }
#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Only usable in Rocket Lake or later due to CPU clock limits
        /// (but Clang suggests it and actually performs approximately 1.34 times better than
        /// <see cref="FastMixSse(Span{float}, ReadOnlySpan{float}, float, ReadOnlySpan{float}, float)"/>, even in Haswell)
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="samplesA"></param>
        /// <param name="volumeA"></param>
        /// <param name="samplesB"></param>
        /// <param name="volumeB"></param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void FastMixAvx(Span<float> buffer, ReadOnlySpan<float> samplesA, float volumeA, ReadOnlySpan<float> samplesB, float volumeB)
        {
            unsafe
            {
                nint i, length = MathI.Min(MathI.Min(samplesA.Length, samplesB.Length), buffer.Length);
                var ymm14 = Vector256.Create(volumeA);
                var ymm15 = Vector256.Create(volumeB);
                ref var r8 = ref MemoryMarshal.GetReference(buffer);
                ref var r10 = ref MemoryMarshal.GetReference(samplesA);
                ref var r11 = ref MemoryMarshal.GetReference(samplesB);
                for (i = 0; i < length - 31; i += 32)
                {
                    var ymm0 = Avx.Multiply(ymm14, Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref r10, i)));
                    var ymm1 = Avx.Multiply(ymm14, Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref r10, i + 8)));
                    var ymm2 = Avx.Multiply(ymm14, Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref r10, i + 16)));
                    var ymm3 = Avx.Multiply(ymm14, Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref r10, i + 24)));
                    var ymm4 = Avx.Multiply(ymm15, Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref r11, i)));
                    ymm0 = Avx.Add(ymm0, ymm4);
                    var ymm5 = Avx.Multiply(ymm15, Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref r11, i + 8)));
                    ymm1 = Avx.Add(ymm1, ymm5);
                    var ymm6 = Avx.Multiply(ymm15, Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref r11, i + 16)));
                    ymm2 = Avx.Add(ymm2, ymm6);
                    var ymm7 = Avx.Multiply(ymm15, Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref r11, i + 24)));
                    ymm3 = Avx.Add(ymm3, ymm7);
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref r8, i)) = ymm0;
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref r8, i + 8)) = ymm1;
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref r8, i + 16)) = ymm2;
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref r8, i + 24)) = ymm3;
                }
                for (; i < length; i++)
                {
                    var xmm0 = Vector128.CreateScalarUnsafe(Unsafe.Add(ref r10, i));
                    var xmm4 = Vector128.CreateScalarUnsafe(Unsafe.Add(ref r11, i));
                    xmm0 = Sse.MultiplyScalar(xmm0, ymm14.GetLower());
                    xmm4 = Sse.MultiplyScalar(xmm4, ymm15.GetLower());
                    xmm0 = Sse.AddScalar(xmm0, xmm4);
                    Unsafe.Add(ref r8, i) = xmm0.GetElement(0);
                }
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void FastMixSse(Span<float> buffer, ReadOnlySpan<float> samplesA, float volumeA, ReadOnlySpan<float> samplesB, float volumeB)
        {
            unsafe
            {
                nint i, length = MathI.Min(MathI.Min(samplesA.Length, samplesB.Length), buffer.Length);
                var xmm14 = Vector128.Create(volumeA);
                var xmm15 = Vector128.Create(volumeB);
                ref var r8 = ref MemoryMarshal.GetReference(buffer);
                ref var r10 = ref MemoryMarshal.GetReference(samplesA);
                ref var r11 = ref MemoryMarshal.GetReference(samplesB);
                for (i = 0; i < length - 15; i += 16)
                {
                    var xmm0 = Sse.Multiply(xmm14, Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref r10, i)));
                    var xmm1 = Sse.Multiply(xmm14, Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref r10, i + 4)));
                    var xmm2 = Sse.Multiply(xmm14, Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref r10, i + 8)));
                    var xmm3 = Sse.Multiply(xmm14, Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref r10, i + 12)));
                    var xmm4 = Sse.Multiply(xmm15, Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref r11, i)));
                    xmm0 = Sse.Add(xmm0, xmm4);
                    var xmm5 = Sse.Multiply(xmm15, Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref r11, i + 4)));
                    xmm1 = Sse.Add(xmm1, xmm5);
                    var xmm6 = Sse.Multiply(xmm15, Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref r11, i + 8)));
                    var xmm7 = Sse.Multiply(xmm15, Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref r11, i + 12)));
                    xmm2 = Sse.Add(xmm2, xmm6);
                    xmm3 = Sse.Add(xmm3, xmm7);
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref r8, i)) = xmm0;
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref r8, i + 4)) = xmm1;
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref r8, i + 8)) = xmm2;
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref r8, i + 12)) = xmm3;
                }
                for (; i < length; i++)
                {
                    var xmm0 = Sse.MultiplyScalar(xmm14, Vector128.CreateScalarUnsafe(Unsafe.Add(ref r10, i)));
                    var xmm4 = Sse.MultiplyScalar(xmm15, Vector128.CreateScalarUnsafe(Unsafe.Add(ref r11, i)));
                    xmm0 = Sse.AddScalar(xmm0, xmm4);
                    Unsafe.Add(ref r8, i) = xmm0.GetElement(0);
                }
            }
        }
#endif
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void FastMixStandardVariable(Span<float> buffer, ReadOnlySpan<float> samplesA, float volumeA, ReadOnlySpan<float> samplesB, float volumeB)
        {
            unsafe
            {
                nint i, length = MathI.Min(MathI.Min(samplesA.Length, samplesB.Length), buffer.Length);
                var scaleVA = new Vector<float>(volumeA);
                var scaleVB = new Vector<float>(volumeB);
                ref var rsA = ref MemoryMarshal.GetReference(samplesA);
                ref var rsB = ref MemoryMarshal.GetReference(samplesB);
                ref var rD = ref MemoryMarshal.GetReference(buffer);
                var olen = length - 4 * Vector<float>.Count + 1;
                for (i = 0; i < olen; i += 4 * Vector<float>.Count)
                {
                    var sA0 = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rsA, i + 0 * Vector<float>.Count)) * scaleVA;
                    var sA1 = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rsA, i + 1 * Vector<float>.Count)) * scaleVA;
                    var sA2 = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rsA, i + 2 * Vector<float>.Count)) * scaleVA;
                    var sA3 = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rsA, i + 3 * Vector<float>.Count)) * scaleVA;
                    var sB0 = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rsB, i + 0 * Vector<float>.Count)) * scaleVB;
                    sA0 += sB0;
                    var sB1 = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rsB, i + 1 * Vector<float>.Count)) * scaleVB;
                    sA1 += sB1;
                    var sB2 = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rsB, i + 2 * Vector<float>.Count)) * scaleVB;
                    var sB3 = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rsB, i + 3 * Vector<float>.Count)) * scaleVB;
                    sA2 += sB2;
                    sA3 += sB3;
                    Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rD, i + 0 * Vector<float>.Count)) = sA0;
                    Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rD, i + 1 * Vector<float>.Count)) = sA1;
                    Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rD, i + 2 * Vector<float>.Count)) = sA2;
                    Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rD, i + 3 * Vector<float>.Count)) = sA3;
                }
                //for (; i < length - 3; i += 4)
                //{
                //    var sA = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rsA, i));
                //    var sB = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rsB, i));
                //    sA *= scaleVA;
                //    sB *= scaleVB;
                //    Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rD, i)) = sA + sB;
                //}
                for (; i < length; i++)
                {
                    var sA = scaleVA[0] * Unsafe.Add(ref rsA, i);
                    var sB = scaleVB[0] * Unsafe.Add(ref rsB, i);
                    Unsafe.Add(ref rD, i) = sA + sB;
                }
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void FastMixStandardFixed(Span<float> buffer, ReadOnlySpan<float> samplesA, float volumeA, ReadOnlySpan<float> samplesB, float volumeB)
        {
            unsafe
            {
                nint i, length = MathI.Min(MathI.Min(samplesA.Length, samplesB.Length), buffer.Length);
                var scaleVA = new Vector4(volumeA);
                var scaleVB = new Vector4(volumeB);
                ref var rsA = ref MemoryMarshal.GetReference(samplesA);
                ref var rsB = ref MemoryMarshal.GetReference(samplesB);
                ref var rD = ref MemoryMarshal.GetReference(buffer);
                for (i = 0; i < length - 15; i += 16)
                {
                    var sA0 = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rsA, i + 0)) * scaleVA;
                    var sA1 = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rsA, i + 4)) * scaleVA;
                    var sA2 = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rsA, i + 8)) * scaleVA;
                    var sA3 = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rsA, i + 12)) * scaleVA;
                    var sB0 = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rsB, i + 0)) * scaleVB;
                    sA0 += sB0;
                    var sB1 = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rsB, i + 4)) * scaleVB;
                    sA1 += sB1;
                    var sB2 = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rsB, i + 8)) * scaleVB;
                    var sB3 = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rsB, i + 12)) * scaleVB;
                    sA2 += sB2;
                    sA3 += sB3;
                    Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rD, i + 0)) = sA0;
                    Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rD, i + 4)) = sA1;
                    Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rD, i + 8)) = sA2;
                    Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rD, i + 12)) = sA3;
                }
                //for (; i < length - 3; i += 4)
                //{
                //    var sA = scaleVA * Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rsA, i));
                //    var sB = scaleVB * Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rsB, i));
                //    Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rD, i)) = sA + sB;
                //}
                for (; i < length; i++)
                {
                    var sA = scaleVA.X * Unsafe.Add(ref rsA, i);
                    var sB = scaleVB.X * Unsafe.Add(ref rsB, i);
                    Unsafe.Add(ref rD, i) = sA + sB;
                }
            }
        }

        #endregion

        #region Interleave

        /// <summary>
        /// Interleaves and stores Stereo samples to <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">The output buffer.</param>
        /// <param name="left">The input buffer for left channel.</param>
        /// <param name="right">The input buffer for right channel.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static void InterleaveStereo(Span<float> buffer, ReadOnlySpan<float> left, ReadOnlySpan<float> right)
            => InterleaveStereo(MemoryMarshal.Cast<float, int>(buffer), MemoryMarshal.Cast<float, int>(left), MemoryMarshal.Cast<float, int>(right));

        /// <summary>
        /// Interleaves and stores Stereo samples to <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">The output buffer.</param>
        /// <param name="left">The input buffer for left channel.</param>
        /// <param name="right">The input buffer for right channel.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static void InterleaveStereo(Span<int> buffer, ReadOnlySpan<int> left, ReadOnlySpan<int> right)
        {
            unchecked
            {
#if NET5_0
                //Arm intrinsics

#endif
#if NETCOREAPP3_1_OR_GREATER
                if (X86.IsSupported)
                {
                    X86.InterleaveStereoInt32(buffer, left, right);
                    return;
                }
#endif
                Fallback.InterleaveStereoInt32(buffer, left, right);
            }
        }

        /// <summary>
        /// Interleaves and stores Stereo samples to <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">The output buffer.</param>
        /// <param name="left">The input buffer for left channel.</param>
        /// <param name="right">The input buffer for right channel.</param>
        /// <param name="center">The input buffer for center channel.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static void InterleaveThree(Span<int> buffer, ReadOnlySpan<int> left, ReadOnlySpan<int> right, ReadOnlySpan<int> center)
        {
            unchecked
            {
#if NET5_0
                //Arm intrinsics
#endif
#if NETCOREAPP3_1_OR_GREATER
                if (X86.IsSupported)
                {
                    X86.InterleaveThreeInt32(buffer, left, right, center);
                    return;
                }
#endif
                Fallback.InterleaveThreeInt32(buffer, left, right, center);
            }
        }

        /// <summary>
        /// Interleaves and stores Stereo samples to <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">The output buffer.</param>
        /// <param name="frontLeft">The input buffer for front left channel.</param>
        /// <param name="frontRight">The input buffer for front right channel.</param>
        /// <param name="rearLeft">The input buffer for rear left channel.</param>
        /// <param name="rearRight">The input buffer for rear right channel.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static void InterleaveQuad(Span<int> buffer, ReadOnlySpan<int> frontLeft, ReadOnlySpan<int> frontRight, ReadOnlySpan<int> rearLeft, ReadOnlySpan<int> rearRight)
        {
            unchecked
            {
#if NET5_0
                //Arm intrinsics
#endif
#if NETCOREAPP3_1_OR_GREATER
                if (X86.IsSupported)
                {
                    X86.InterleaveQuadInt32(buffer, frontLeft, frontRight, rearLeft, rearRight);
                    return;
                }
#endif
                Fallback.InterleaveQuadInt32(buffer, frontLeft, frontRight, rearLeft, rearRight);
            }
        }
        #endregion

        #region DuplicateMonauralToChannels

        /// <summary>
        /// Duplicates specified monaural signal <paramref name="source"/> to the specified stereo <paramref name="destination"/>.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <param name="source">The source.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static void DuplicateMonauralToStereo(Span<float> destination, ReadOnlySpan<float> source)
        {
            destination = destination.SliceWhileIfLongerThan(source.Length * 2);
            source = source.SliceWhileIfLongerThan(destination.Length / 2);
#if NETCOREAPP3_1_OR_GREATER
            if (X86.IsSupported)
            {
                X86.DuplicateMonauralToStereo(destination, source);
                return;
            }
#endif
            Fallback.DuplicateMonauralToStereo(destination, source);
        }
        /// <summary>
        /// Duplicates specified monaural signal <paramref name="source"/> to the specified <paramref name="destination"/> with 3 channels.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <param name="source">The source.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static void DuplicateMonauralToThree(Span<float> destination, ReadOnlySpan<float> source)
        {
            destination = destination.SliceWhileIfLongerThan(source.Length * 3);
            source = source.SliceWhileIfLongerThan(destination.Length / 3);
#if NETCOREAPP3_1_OR_GREATER
            if (X86.IsSupported)
            {
                X86.DuplicateMonauralTo3Channels(destination, source);
                return;
            }
#endif
            Fallback.DuplicateMonauralTo3Channels(destination, source);
        }
        /// <summary>
        /// Duplicates specified monaural signal <paramref name="source"/> to the specified <paramref name="destination"/> with 4 channels.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <param name="source">The source.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static void DuplicateMonauralToQuad(Span<float> destination, ReadOnlySpan<float> source)
        {
            destination = destination.SliceWhileIfLongerThan(source.Length * 4);
            source = source.SliceWhileIfLongerThan(destination.Length / 4);
#if NETCOREAPP3_1_OR_GREATER
            if (X86.IsSupported)
            {
                X86.DuplicateMonauralTo4Channels(destination, source);
                return;
            }
#endif
            Fallback.DuplicateMonauralTo4Channels(destination, source);
        }

        /// <summary>
        /// Duplicates specified monaural samples <paramref name="source"/> to <paramref name="channels"/>, and writes to <paramref name="destination"/>.<br/>
        /// This method assumes that either <paramref name="source"/> and <paramref name="destination"/> don't overlap at all,
        ///  or <paramref name="source"/> is at the very end of the <paramref name="destination"/> if you allow <paramref name="source"/> to be overwritten.<br/>
        /// Otherwise both the <paramref name="source"/> and <paramref name="destination"/> will be corrupted and results in complete chaos!
        /// </summary>
        /// <param name="destination">The multi-channel destination.</param>
        /// <param name="source">The monaural source.</param>
        /// <param name="channels">The number of channels.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static void DuplicateMonauralToChannels(Span<float> destination, ReadOnlySpan<float> source, int channels)
        {
            switch (channels)
            {
                case < 2:   //Less than or equals to Monaural
                    return;
                case 2:
                    DuplicateMonauralToStereo(destination, source);
                    return;
                case 3:
                    DuplicateMonauralToThree(destination, source);
                    return;
                case 4:
                    DuplicateMonauralToQuad(destination, source);
                    return;
                case 8 when Vector<float>.Count != 8:
                    DuplicateMonauralToOctaple(destination, source);
                    return;
                case 12:
                    //There were no system that has 384-bits-wide hardware vector system I ever heard.
                    DuplicateMonauralTo12Channels(destination, source);
                    return;
                case 16:
                    //Neither AVX-512 nor SVE are supported in .NET 6.
                    DuplicateMonauralTo16Channels(destination, source);
                    return;
                case < 16 when Vector<float>.Count != channels:
                    DuplicateMonauralToLessThan16Channels(destination, source, channels);
                    return;
                case > 0 when Vector<float>.Count == channels:
                    DuplicateMonauralToVectorAlignedChannels(destination, source);
                    return;
                default:
                    DuplicateMonauralToChannelsOrdinal(destination, source, channels);
                    return;
            }
        }

        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        private static void DuplicateMonauralToChannelsOrdinal(Span<float> destination, ReadOnlySpan<float> source, int channels)
        {
            var h = 0;
            for (var i = 0; i < source.Length; i++, h += channels)
            {
                var value = source[i];
                destination.Slice(h, channels).FastFill(value);
            }
        }

        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        private static void DuplicateMonauralToVectorAlignedChannels(Span<float> destination, ReadOnlySpan<float> source)
        {
            var h = 0;
            ref var dst = ref MemoryMarshal.GetReference(destination);
            for (var i = 0; i < source.Length; i++, h += Vector<float>.Count)
            {
                var v4v = new Vector<float>(source[i]);
                Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref dst, h)) = v4v;
            }
        }

        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        private static void DuplicateMonauralToLessThan16Channels(Span<float> destination, ReadOnlySpan<float> source, int channels)
        {
            var h = 0;
            ref var dst = ref MemoryMarshal.GetReference(destination);
            for (var i = 0; i < source.Length; i++, h += channels)
            {
                var v4v = new Vector4(source[i]);
                var j = 0;
                var olen = channels - 3;
                for (; j < olen; j += 4)
                {
                    Unsafe.As<float, Vector4>(ref Unsafe.Add(ref dst, h + j)) = v4v;
                }
                for (; j < channels; j++)
                {
                    Unsafe.Add(ref dst, h + j) = v4v.X;
                }
            }
        }

        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        internal static void DuplicateMonauralTo16Channels(Span<float> destination, ReadOnlySpan<float> source)
        {
            destination = destination.SliceWhileIfLongerThan(source.Length * 16);
            source = source.SliceWhileIfLongerThan(destination.Length / 16);
            var h = 0;
            ref var dst = ref MemoryMarshal.GetReference(destination);
            for (var i = 0; i < source.Length; i++, h += 16)
            {
                var v4v = new Vector4(source[i]);
                Unsafe.As<float, Vector4>(ref Unsafe.Add(ref dst, h)) = v4v;
                Unsafe.As<float, Vector4>(ref Unsafe.Add(ref dst, h + 4)) = v4v;
                Unsafe.As<float, Vector4>(ref Unsafe.Add(ref dst, h + 8)) = v4v;
                Unsafe.As<float, Vector4>(ref Unsafe.Add(ref dst, h + 12)) = v4v;
            }
        }
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        internal static void DuplicateMonauralTo12Channels(Span<float> destination, ReadOnlySpan<float> source)
        {
            destination = destination.SliceWhileIfLongerThan(source.Length * 12);
            source = source.SliceWhileIfLongerThanWithLazyDivide(destination.Length, 12);
            var h = 0;
            ref var dst = ref MemoryMarshal.GetReference(destination);
            for (var i = 0; i < source.Length; i++, h += 12)
            {
                var v4v = new Vector4(source[i]);
                Unsafe.As<float, Vector4>(ref Unsafe.Add(ref dst, h)) = v4v;
                Unsafe.As<float, Vector4>(ref Unsafe.Add(ref dst, h + 4)) = v4v;
                Unsafe.As<float, Vector4>(ref Unsafe.Add(ref dst, h + 8)) = v4v;
            }
        }

        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        internal static void DuplicateMonauralToOctaple(Span<float> destination, ReadOnlySpan<float> source)
        {
            destination = destination.SliceWhileIfLongerThan(source.Length * 8);
            source = source.SliceWhileIfLongerThan(destination.Length / 8);
            var h = 0;
            ref var dst = ref MemoryMarshal.GetReference(destination);
            for (var i = 0; i < source.Length; i++, h += 8)
            {
                var v4v = new Vector4(source[i]);
                Unsafe.As<float, Vector4>(ref Unsafe.Add(ref dst, h)) = v4v;
                Unsafe.As<float, Vector4>(ref Unsafe.Add(ref dst, h + 4)) = v4v;
            }
        }
        #endregion

        #region Deinterleave
        /// <summary>
        /// Deinterleaves <paramref name="buffer"/> to <paramref name="left"/> and <paramref name="right"/>.
        /// </summary>
        /// <param name="buffer">The source.</param>
        /// <param name="left">The destination to store even-indexed samples.</param>
        /// <param name="right">The destination to store odd-indexed samples.</param>
        public static void DeinterleaveStereoSingle(ReadOnlySpan<float> buffer, Span<float> left, Span<float> right)
        {
            unchecked
            {
#if NETCOREAPP3_1_OR_GREATER
                if (X86.IsSupported)
                {
                    X86.DeinterleaveStereoSingleX86(buffer, left, right);
                    return;
                }
#endif
                Fallback.DeinterleaveStereoSingle(buffer, left, right);
            }
        }
        #endregion

        #region FFT Utils
        #region ConvertRealSingleToComplexF
        /// <summary>
        /// Converts <see cref="float"/> values of <paramref name="src"/> as real part of <see cref="ComplexF"/> values in <paramref name="dst"/>.
        /// </summary>
        /// <param name="dst"></param>
        /// <param name="src"></param>
        public static void ConvertRealSingleToComplexF(Span<ComplexF> dst, ReadOnlySpan<float> src)
        {

        }
        #endregion
        #endregion
    }
}
