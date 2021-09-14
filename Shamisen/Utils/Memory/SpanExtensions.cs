using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

using Shamisen;

using DivideSharp;

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
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
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
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static void FastScalarMultiply(this Span<float> span, float scale = default)
        {
            if (Vector<float>.Count > span.Length)
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
                FastScalarMultiplyStandardVariable(span, scale);
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        [Obsolete("Remains only for benchmark comparison!")]
        internal static void FastScalarMultiplyStandardVariableOld(Span<float> span, float scale = default)
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
        /// Vectorized path using <see cref="Vector4"/> which uses 128bits vectors like xmmN(x86) or vN.4f(ARMv8).<br/>
        /// Impractical in Rocket Lake or later due to absense of Haswell's severe CPU clock limits.<br/>
        /// </summary>
        /// <param name="span"></param>
        /// <param name="scale"></param>
        internal static void FastScalarMultiplyStandardFixed(Span<float> span, float scale)
        {
            var scaleV = new Vector4(scale);
            ref var rdi = ref MemoryMarshal.GetReference(span);
            nint i, length = span.Length;
            for (i = 0; i < length - (4 * 8 - 1); i += 4 * 8)
            {
                var v0 = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rdi, i + 0 * 4)) * scaleV;
                var v1 = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rdi, i + 1 * 4)) * scaleV;
                var v2 = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rdi, i + 2 * 4)) * scaleV;
                var v3 = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rdi, i + 3 * 4)) * scaleV;
                Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rdi, i + 0 * 4)) = v0;
                Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rdi, i + 1 * 4)) = v1;
                Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rdi, i + 2 * 4)) = v2;
                Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rdi, i + 3 * 4)) = v3;
                v0 = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rdi, i + 4 * 4)) * scaleV;
                v1 = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rdi, i + 5 * 4)) * scaleV;
                v2 = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rdi, i + 6 * 4)) * scaleV;
                v3 = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rdi, i + 7 * 4)) * scaleV;
                Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rdi, i + 4 * 4)) = v0;
                Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rdi, i + 5 * 4)) = v1;
                Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rdi, i + 6 * 4)) = v2;
                Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rdi, i + 7 * 4)) = v3;
            }
            if (i < length - (4 * 4 - 1))
            {
                var v0 = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rdi, i + 0 * 4)) * scaleV;
                var v1 = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rdi, i + 1 * 4)) * scaleV;
                var v2 = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rdi, i + 2 * 4)) * scaleV;
                var v3 = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rdi, i + 3 * 4)) * scaleV;
                Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rdi, i + 0 * 4)) = v0;
                Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rdi, i + 1 * 4)) = v1;
                Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rdi, i + 2 * 4)) = v2;
                Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rdi, i + 3 * 4)) = v3;
                i += 4 * 4;
            }
            for (; i < length; i++)
            {
                var v = Unsafe.Add(ref rdi, i) * scaleV.X;
                Unsafe.Add(ref rdi, i) = v;
            }
        }

        /// <summary>
        /// Vectorized path using <see cref="Vector{T}"/> which uses variable-sized vectors.<br/>
        /// Only practical in either ARMv8, Rocket Lake or later, or pre-Sandy-Bridge x64 CPUs due to CPU clock limits.<br/>
        /// Future versions of .NET may improve performance if <see cref="Vector{T}"/> utilizes either x64 AVX512 or ARMv8.2-A SVE.
        /// </summary>
        /// <param name="span"></param>
        /// <param name="scale"></param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void FastScalarMultiplyStandardVariable(Span<float> span, float scale)
        {
            var scaleV = new Vector<float>(scale);
            ref var rdi = ref MemoryMarshal.GetReference(span);
            nint i, length = span.Length;
            nint width = Vector<float>.Count;
            for (i = 0; i < length - (width * 8 - 1); i += width * 8)
            {
                var v0 = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rdi, i + 0 * width)) * scaleV;
                var v1 = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rdi, i + 1 * width)) * scaleV;
                var v2 = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rdi, i + 2 * width)) * scaleV;
                var v3 = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rdi, i + 3 * width)) * scaleV;
                Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rdi, i + 0 * width)) = v0;
                Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rdi, i + 1 * width)) = v1;
                Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rdi, i + 2 * width)) = v2;
                Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rdi, i + 3 * width)) = v3;
                v0 = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rdi, i + 4 * width)) * scaleV;
                v1 = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rdi, i + 5 * width)) * scaleV;
                v2 = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rdi, i + 6 * width)) * scaleV;
                v3 = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rdi, i + 7 * width)) * scaleV;
                Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rdi, i + 4 * width)) = v0;
                Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rdi, i + 5 * width)) = v1;
                Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rdi, i + 6 * width)) = v2;
                Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rdi, i + 7 * width)) = v3;
            }
            if (i < length - (width * 4 - 1))
            {
                var v0 = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rdi, i + 0 * width)) * scaleV;
                var v1 = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rdi, i + 1 * width)) * scaleV;
                var v2 = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rdi, i + 2 * width)) * scaleV;
                var v3 = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rdi, i + 3 * width)) * scaleV;
                Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rdi, i + 0 * width)) = v0;
                Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rdi, i + 1 * width)) = v1;
                Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rdi, i + 2 * width)) = v2;
                Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rdi, i + 3 * width)) = v3;
                i += width * 4;
            }
            for (; i < length; i++)
            {
                var v = Unsafe.Add(ref rdi, i) * scaleV[0];
                Unsafe.Add(ref rdi, i) = v;
            }
        }

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
            if (samplesToMix.Length > buffer.Length) throw new ArgumentException("", nameof(samplesToMix));
#if NETCOREAPP3_1_OR_GREATER
            if (Sse.X64.IsSupported)
            {
                FastMixSse64(samplesToMix, buffer, scale);
                return;
            }
            else if (Sse.IsSupported)
            {
                FastMixSse(samplesToMix, buffer, scale);
                return;
            }
#endif
            FastMixStandard(samplesToMix, buffer, scale);
        }
#if NETCOREAPP3_1_OR_GREATER
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static void FastMixSse64(ReadOnlySpan<float> samplesToMix, Span<float> buffer, float scale)
        {
            nint i = 0;
            nint length = samplesToMix.Length;
            ref var rsi = ref MemoryMarshal.GetReference(samplesToMix);
            ref var rdi = ref MemoryMarshal.GetReference(buffer);
            var xmm15 = Vector128.Create(scale);
            for (i = 0; i < length - 15; i += 16)
            {
                var xmm0 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rsi, i));
                var xmm1 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rsi, i + 4));
                var xmm4 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i));
                var xmm5 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 4));
                xmm0 = Sse.Multiply(xmm0, xmm15);
                xmm1 = Sse.Multiply(xmm1, xmm15);
                xmm4 = Sse.Add(xmm4, xmm0);
                xmm5 = Sse.Add(xmm5, xmm1);
                var xmm2 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rsi, i + 8));
                var xmm3 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rsi, i + 12));
                var xmm6 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 8));
                var xmm7 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 12));
                xmm2 = Sse.Multiply(xmm2, xmm15);
                xmm3 = Sse.Multiply(xmm3, xmm15);
                xmm6 = Sse.Add(xmm6, xmm2);
                xmm7 = Sse.Add(xmm7, xmm3);
                Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i)) = xmm4;
                Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 4)) = xmm5;
                Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 8)) = xmm6;
                Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 12)) = xmm7;
            }
            for (; i < length - 3; i += 4)
            {
                var xmm0 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rsi, i));
                var xmm4 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i));
                xmm0 = Sse.Multiply(xmm0, xmm15);
                xmm4 = Sse.Add(xmm4, xmm0);
                Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i)) = xmm4;
            }
            for (; i < length; i++)
            {
                var xmm0 = Vector128.CreateScalarUnsafe(Unsafe.Add(ref rsi, i));
                var xmm4 = Vector128.CreateScalarUnsafe(Unsafe.Add(ref rdi, i));
                xmm0 = Sse.MultiplyScalar(xmm0, xmm15);
                xmm4 = Sse.AddScalar(xmm4, xmm0);
                Unsafe.Add(ref rdi, i) = xmm4.GetElement(0);
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static void FastMixSse(ReadOnlySpan<float> samplesToMix, Span<float> buffer, float scale)
        {
            nint i = 0;
            nint length = samplesToMix.Length;
            ref var rsi = ref MemoryMarshal.GetReference(samplesToMix);
            ref var rdi = ref MemoryMarshal.GetReference(buffer);
            var xmm15 = Vector128.Create(scale);
            for (i = 0; i < length - 7; i += 8)
            {
                var xmm0 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rsi, i));
                var xmm1 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rsi, i + 4));
                var xmm4 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i));
                var xmm5 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 4));
                xmm0 = Sse.Multiply(xmm0, xmm15);
                xmm1 = Sse.Multiply(xmm1, xmm15);
                xmm4 = Sse.Add(xmm4, xmm0);
                xmm5 = Sse.Add(xmm5, xmm1);
                Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i)) = xmm4;
                Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 4)) = xmm5;
                xmm0 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rsi, i + 8));
                xmm1 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rsi, i + 12));
                xmm4 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 8));
                xmm5 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 12));
                xmm0 = Sse.Multiply(xmm0, xmm15);
                xmm1 = Sse.Multiply(xmm1, xmm15);
                xmm4 = Sse.Add(xmm4, xmm0);
                xmm5 = Sse.Add(xmm5, xmm1);
                Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 8)) = xmm4;
                Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 12)) = xmm5;
            }
            for (; i < length - 3; i += 4)
            {
                var xmm0 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rsi, i));
                var xmm4 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i));
                xmm0 = Sse.Multiply(xmm0, xmm15);
                xmm4 = Sse.Add(xmm4, xmm0);
                Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i)) = xmm4;
            }
            for (; i < length; i++)
            {
                var xmm0 = Vector128.CreateScalarUnsafe(Unsafe.Add(ref rsi, i));
                var xmm4 = Vector128.CreateScalarUnsafe(Unsafe.Add(ref rdi, i));
                xmm0 = Sse.MultiplyScalar(xmm0, xmm15);
                xmm4 = Sse.AddScalar(xmm4, xmm0);
                Unsafe.Add(ref rdi, i) = xmm4.GetElement(0);
            }
        }
#endif
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static void FastMixStandard(ReadOnlySpan<float> samplesToMix, Span<float> buffer, float scale)
        {
            unsafe
            {
                (int newLength, int remainder) = MathI.FloorStepRem(samplesToMix.Length, 4);
                if (newLength != 0)
                {
                    var scaleV = new Vector4(scale);
                    var src = MemoryMarshal.Cast<float, Vector4>(samplesToMix);
                    var dst = MemoryMarshal.Cast<float, Vector4>(buffer).Slice(0, src.Length);
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
            if (buffer.Length > samplesA.Length || buffer.Length > samplesB.Length)
                throw new ArgumentException("buffer must not be shorter than samplesA or samplesB!", nameof(buffer));
#if NETCOREAPP3_1_OR_GREATER
            if (Sse.IsSupported)
            {
                FastMixSse(buffer, samplesA, volumeA, samplesB, volumeB);
                return;
            }
#endif
            FastMixStandard(buffer, samplesA, volumeA, samplesB, volumeB);
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
                nint i, length = buffer.Length;
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
                nint i, length = buffer.Length;
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
        internal static void FastMixStandard(Span<float> buffer, ReadOnlySpan<float> samplesA, float volumeA, ReadOnlySpan<float> samplesB, float volumeB)
        {
            unsafe
            {
                nint i, length = buffer.Length;
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
                //    var sA = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rsA, i));
                //    var sB = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rsB, i));
                //    sA *= scaleVA;
                //    sB *= scaleVB;
                //    Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rD, i)) = sA + sB;
                //}
                for (; i < length; i++)
                {
                    var sA = Unsafe.Add(ref rsA, i);
                    var sB = Unsafe.Add(ref rsB, i);
                    sA *= scaleVA.X;
                    sB *= scaleVB.X;
                    Unsafe.Add(ref rD, i) = sA + sB;
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

        /// <summary>
        /// Slices the <paramref name="span"/> aligned with the multiple of <paramref name="channelsDivisor"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="span">The <see cref="Span{T}"/> to slice.</param>
        /// <param name="channelsDivisor">The divisor set to align width.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static Span<T> SliceAlign<T>(this Span<T> span, Int32Divisor channelsDivisor) => span.Slice(0, channelsDivisor.AbsFloor(span.Length));

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
            //The Internal number gets deconstructed in little-endian so the values are written in BIG-ENDIAN.
            var mask = Vector128.Create(0x0001020304050607ul, 0x08090a0b0c0d0e0ful).AsByte();
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
            //The Internal number gets deconstructed in little-endian so the values are written in BIG-ENDIAN.
            var mask = Vector128.Create(0x0001020304050607ul, 0x08090a0b0c0d0e0ful).AsByte();
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
