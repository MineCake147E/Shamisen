using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
        #region SIMD-Related Functions
        #region FastAddTwoOperands

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
#if NET5_0_OR_GREATER
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void FastAddAdvSimd(Span<float> buffer, ReadOnlySpan<float> samplesA, ReadOnlySpan<float> samplesB)
        {
            nint i, length = buffer.Length;
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
            nint i, length = buffer.Length;
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
                nint i, length = buffer.Length;
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
                nint i, length = buffer.Length;
                ref var rsA = ref MemoryMarshal.GetReference(samplesA);
                ref var rsB = ref MemoryMarshal.GetReference(samplesB);
                ref var rD = ref MemoryMarshal.GetReference(buffer);
                //This is a loop for Alder Lake, probably not optimal in Haswell.
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

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        [Obsolete("Remains only for benchmark comparison!")]
        internal static void FastAddOld(ReadOnlySpan<float> samplesToAdd, Span<float> buffer)
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
        #endregion
        #region FastScalarMultiply

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
            if (buffer.Length > samplesA.Length || buffer.Length > samplesB.Length)
                throw new ArgumentException("buffer must not be shorter than samplesA or samplesB!", nameof(buffer));
#if NETCOREAPP3_1_OR_GREATER
            if (Sse.IsSupported)
            {
                FastMixSse(buffer, samplesA, volumeA, samplesB, volumeB);
                return;
            }
#endif
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
        internal static void FastMixStandardVariable(Span<float> buffer, ReadOnlySpan<float> samplesA, float volumeA, ReadOnlySpan<float> samplesB, float volumeB)
        {
            unsafe
            {
                nint i, length = buffer.Length;
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

        #endregion SIMD-Related Functions


        #region Interleave

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

        #region DuplicateMonauralToStereo


        /// <summary>
        /// Duplicates specified monaural signal <paramref name="source"/> to the specified stereo <paramref name="destination"/>.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <param name="source">The source.</param>
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
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


        #endregion

        #region DuplicateMonauralToChannels

        /// <summary>
        /// Duplicates specified monaural samples <paramref name="source"/> to <paramref name="channels"/>, and writes to <paramref name="destination"/>.
        /// </summary>
        /// <param name="destination">The multi-channel destination.</param>
        /// <param name="source">The monaural source.</param>
        /// <param name="channels">The number of channels.</param>
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        public static void DuplicateMonauralToChannels(Span<float> destination, ReadOnlySpan<float> source, int channels)
        {
            switch (channels)
            {
                case < 1:
                case 1:
                    return;
                case 2:
                    {
                        if (Vector<float>.Count == 2) goto default;
                        DuplicateMonauralToStereo(destination, source);
                        return;
                    }
                case 3:
                    {
                        destination = destination.SliceWhileIfLongerThan(source.Length * 3);
                        source = source.SliceWhileIfLongerThanWithLazyDivide(destination.Length, 3);
                        var bispan = MemoryMarshal.Cast<float, int>(source);
                        InterleaveThree(MemoryMarshal.Cast<float, int>(destination), bispan, bispan, bispan);
                        return;
                    }
                case 4:
                    {
                        if (Vector<float>.Count == 4) goto default;
                        DuplicateMonauralToQuad(destination, source);
                        return;
                    }
                case 8:
                    {
                        if (Vector<float>.Count == 8) goto default;
                        DuplicateMonauralToOctaple(destination, source);
                        return;
                    }
                case 12:
                    {
                        DuplicateMonauralTo12Channels(destination, source);
                        return;
                    }
                case 16:
                    {
                        if (Vector<float>.Count == channels) goto default;
                        DuplicateMonauralTo16Channels(destination, source);
                        return;
                    }
                case < 16:
                    {
                        if (Vector<float>.Count == channels) goto default;
                        int h = 0;
                        ref var dst = ref MemoryMarshal.GetReference(destination);
                        for (int i = 0; i < source.Length; i++, h += channels)
                        {
                            var v4v = new Vector4(source[i]);
                            int j = 0;
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
                        return;
                    }
                default:
                    {
                        if (Vector<float>.Count == channels)
                        {
                            int h = 0;
                            ref var dst = ref MemoryMarshal.GetReference(destination);
                            for (int i = 0; i < source.Length; i++, h += channels)
                            {
                                var v4v = new Vector<float>(source[i]);
                                Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref dst, h)) = v4v;
                            }
                            return;
                        }
                        else
                        {
                            int h = 0;
                            for (int i = 0; i < source.Length; i++, h += channels)
                            {
                                var value = source[i];
                                destination.Slice(h, channels).FastFill(value);
                            }
                            return;
                        }
                    }
            }
        }

        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        internal static void DuplicateMonauralTo16Channels(Span<float> destination, ReadOnlySpan<float> source)
        {
            destination = destination.SliceWhileIfLongerThan(source.Length * 16);
            source = source.SliceWhileIfLongerThan(destination.Length / 16);
            int h = 0;
            ref var dst = ref MemoryMarshal.GetReference(destination);
            for (int i = 0; i < source.Length; i++, h += 16)
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
            int h = 0;
            ref var dst = ref MemoryMarshal.GetReference(destination);
            for (int i = 0; i < source.Length; i++, h += 12)
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
            int h = 0;
            ref var dst = ref MemoryMarshal.GetReference(destination);
            for (int i = 0; i < source.Length; i++, h += 8)
            {
                var v4v = new Vector4(source[i]);
                Unsafe.As<float, Vector4>(ref Unsafe.Add(ref dst, h)) = v4v;
                Unsafe.As<float, Vector4>(ref Unsafe.Add(ref dst, h + 4)) = v4v;
            }
        }
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        internal static void DuplicateMonauralToQuad(Span<float> destination, ReadOnlySpan<float> source)
        {
            destination = destination.SliceWhileIfLongerThan(source.Length * 4);
            source = source.SliceWhileIfLongerThan(destination.Length / 4);
            int h = 0;
            ref var dst = ref MemoryMarshal.GetReference(destination);
            for (int i = 0; i < source.Length; i++, h += 4)
            {
                var v4v = new Vector4(source[i]);
                Unsafe.As<float, Vector4>(ref Unsafe.Add(ref dst, h)) = v4v;
            }
        }

        #endregion
    }
}
