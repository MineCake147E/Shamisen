#if NETCOREAPP3_1_OR_GREATER

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

using Shamisen.Optimization;
using Shamisen.Utils;

namespace Shamisen.Conversion.Resampling.Sample
{
    public sealed partial class SplineResampler
    {
        #region Resample
        #region X86
        #region CachedDirect
        #region Monaural
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private unsafe ResampleFunc GetFuncCachedDirectMonauralX86(in UnifiedResampleArgs args)
            => (args.IndexIncrement, args.GradientIncrement, args.RateMul) switch
            {
                (0, 1, 2) when Sse41.IsSupported && X86Intrinsics.HasAllFeatures(X86IntrinsicsMask.Sse41)
                => new(&ResampleCachedDirectMonauralDoubleRateSse41),
                (0, 1, 2) => new(&ResampleCachedDirectMonauralDoubleRateStandard),
                (0, 1, 4) when Sse41.IsSupported && X86Intrinsics.HasAllFeatures(X86IntrinsicsMask.Sse41)
                => new(&ResampleCachedDirectMonauralQuadrupleRateSse41),
                (0, 1, 4) => new(&ResampleCachedDirectMonauralQuadrupleRateStandard),
                (0, 1, _) => new(&ResampleCachedDirectMonauralIntegerMultipleRateX86),
                (0, _, _) when Sse.IsSupported && X86Intrinsics.HasAllFeatures(X86IntrinsicsMask.Sse)
                => GetFuncCachedDirectMonauralUpAnyRateX86(args),
                (_, _, _) when Sse3.IsSupported && X86Intrinsics.HasAllFeatures(X86IntrinsicsMask.Sse3)
                => new(&ResampleCachedDirectMonauralAnyRateSse3),
                _ => GetFuncCachedDirectMonauralStandard(args),
            };

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static unsafe ResampleFunc GetFuncCachedDirectMonauralUpAnyRateX86(in UnifiedResampleArgs args)
        {
            var ram = args.RateMul;
            var acc = args.GradientIncrement;
            return ram - ram / 4 < acc && Sse2.IsSupported
                ? (new(&ResampleCachedDirectMonauralUpAnyRateUnrolledSse2))
                : (new(&ResampleCachedDirectMonauralUpAnyRateGenericSse));
        }

        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        internal static ResampleResult ResampleCachedDirectMonauralUpAnyRateUnrolledSse2(UnifiedResampleArgs args, Span<float> buffer, Span<float> srcBuffer, Span<Vector4> cspan)
        {
            nint i = 0;
            nint length = buffer.Length;
            _ = srcBuffer.Length - 3;
            nint isx = 0;
            nint psx = args.ConversionGradient;
            nint nram = args.RateMul;
            nint nacc = args.GradientIncrement;
            var nracc = nram - nacc;
            var nram16 = nram * 16;
            nint nrci = args.RearrangedCoeffsIndex * 16;
            ref var coeffPtr = ref MemoryMarshal.GetReference(cspan);
            ref var coeff = ref Unsafe.As<Vector4, Vector128<float>>(ref coeffPtr);
            ref var src = ref MemoryMarshal.GetReference(srcBuffer);
            ref var dst = ref MemoryMarshal.GetReference(buffer);
            Vector128<float> xmm7;
            var oram = nracc * 3;
            var olen = length - 3;
            var nracc4 = -nracc * 4;
            while (true)
            {
                if (i >= olen) break;
                if (psx > oram)
                {
                    xmm7 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref src, isx + 0));
                    var xmm8 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref src, isx + 1));
                    var xmm9 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref src, isx + 2));
                    var xmm10 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref src, isx + 3));
                    xmm7 = Sse.Multiply(xmm7, Unsafe.AddByteOffset(ref coeff, nrci + 0));
                    xmm8 = Sse.Multiply(xmm8, Unsafe.AddByteOffset(ref coeff, nrci + 16));
                    xmm9 = Sse.Multiply(xmm9, Unsafe.AddByteOffset(ref coeff, nrci + 32));
                    xmm10 = Sse.Multiply(xmm10, Unsafe.AddByteOffset(ref coeff, nrci + 48));
                    psx += nracc4;
                    isx += 4;
                    nrci += 64;
                    var j = nrci >= nram16;
                    nint z = Unsafe.As<bool, byte>(ref j);
                    nrci -= -z & nram16;
                    var xmm2 = Sse2.UnpackHigh(xmm7.AsDouble(), xmm8.AsDouble()).AsSingle();
                    var xmm0 = Sse2.UnpackLow(xmm7.AsDouble(), xmm8.AsDouble()).AsSingle();
                    var xmm1 = Sse2.UnpackHigh(xmm9.AsDouble(), xmm10.AsDouble()).AsSingle();
                    xmm0 = Sse.Add(xmm0, xmm2);
                    xmm10 = Sse2.UnpackLow(xmm9.AsDouble(), xmm10.AsDouble()).AsSingle();
                    xmm1 = Sse.Add(xmm1, xmm10);
                    var h = psx < 0;
                    nint y = Unsafe.As<bool, byte>(ref h);
                    xmm2 = Sse.Shuffle(xmm0, xmm1, 0b11_01_11_01);
                    xmm7 = Sse.Shuffle(xmm0, xmm1, 0b10_00_10_00);
                    y = -y;
                    xmm0 = Sse.Add(xmm2, xmm7);
                    psx += nram & y;
                    isx += y;
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref dst, i)) = xmm0;
                    i += 4;
                }
                else
                {
                    xmm7 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref src, isx));
                    var xmm0 = Unsafe.AddByteOffset(ref coeff, nrci);
                    nrci += 16;
                    xmm0 = Sse.Multiply(xmm0, xmm7);
                    psx += nacc;
                    var j = nrci < nram16;
                    nint z = Unsafe.As<bool, byte>(ref j);
                    var xmm6 = Sse.Shuffle(xmm0, xmm0, 0b11_10_11_10);
                    xmm0 = Sse.Add(xmm0, xmm6);
                    nrci &= -z;
                    xmm6 = Sse.Shuffle(xmm0, xmm0, 0b01_01_01_01);
                    xmm0 = Sse.AddScalar(xmm0, xmm6);
                    var h = psx >= nram;
                    nint y = Unsafe.As<bool, byte>(ref h);
                    isx += y;
                    y = -y;
                    Unsafe.Add(ref dst, i) = xmm0.GetElement(0);
                    i++;
                    psx -= nram & y;
                }
            }
            for (; i < length; i++)
            {
                xmm7 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref src, isx));
                var xmm0 = Unsafe.AddByteOffset(ref coeff, nrci);
                nrci += 16;
                xmm0 = Sse.Multiply(xmm0, xmm7);
                var xmm6 = Sse.Shuffle(xmm0, xmm0, 0b11_10_11_10);
                xmm0 = Sse.Add(xmm0, xmm6);
                psx += nacc;
                var j = nrci < nram16;
                nint z = Unsafe.As<bool, byte>(ref j);
                xmm6 = Sse.Shuffle(xmm0, xmm0, 0b01_01_01_01);
                nrci &= -z;
                xmm0 = Sse.AddScalar(xmm0, xmm6);
                var h = psx >= nram;
                nint y = Unsafe.As<bool, byte>(ref h);
                isx += y;
                y = -y;
                Unsafe.Add(ref dst, i) = xmm0.GetElement(0);
                psx -= nram & y;
            }
            return new((int)(nuint)isx, (int)psx, (int)((nuint)nrci / 16));
        }

        /// <summary>
        /// For arbitrary sampling frequency ratio larger than 1
        /// </summary>
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        internal static ResampleResult ResampleCachedDirectMonauralUpAnyRateGenericSse(UnifiedResampleArgs args, Span<float> buffer, Span<float> srcBuffer, Span<Vector4> cspan)
        {
            nint i = 0;
            nint length = buffer.Length;
            nint slen = srcBuffer.Length - 3;
            nint isx = 0;
            nint psx = args.ConversionGradient;
            nint nrci = args.RearrangedCoeffsIndex;
            nint nram = args.RateMul;
            nint acc = args.GradientIncrement;
            ref var coeffPtr = ref MemoryMarshal.GetReference(cspan);
            ref var coeff = ref Unsafe.As<Vector4, Vector128<float>>(ref coeffPtr);
            ref var src = ref MemoryMarshal.GetReference(srcBuffer);
            ref var dst = ref MemoryMarshal.GetReference(buffer);
            var xmm7 = Unsafe.As<float, Vector128<float>>(ref src);
            for (i = 0; i < length; i++)
            {
                var xmm0 = Unsafe.Add(ref coeff, nrci);
                xmm0 = Sse.Multiply(xmm0, xmm7);
                var xmm6 = Sse.Shuffle(xmm0, xmm0, 0b11_10_11_10);
                xmm0 = Sse.Add(xmm0, xmm6);
                psx += acc;
                var j = ++nrci < nram;
                nint z = Unsafe.As<bool, byte>(ref j);
                xmm6 = Sse.Shuffle(xmm0, xmm0, 0b01_01_01_01);
                nrci &= -z;
                xmm0 = Sse.AddScalar(xmm0, xmm6);
                Unsafe.Add(ref dst, i) = xmm0.GetElement(0);
                if (psx < nram) continue;
                psx -= nram;
                isx++;
                if (isx >= slen) continue;
                xmm7 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref src, isx));
            }
            return new((int)isx, (int)psx, (int)nrci);
        }

        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        internal static ResampleResult ResampleCachedDirectMonauralIntegerMultipleRateX86(UnifiedResampleArgs args, Span<float> buffer, Span<float> srcBuffer, Span<Vector4> cspan)
        {
            nint i = 0;
            nint length = buffer.Length;
            nint slen = srcBuffer.Length - 3;
            nint isx = 0;
            nint psx = args.ConversionGradient;
            nint ram = args.RateMul;
            ref var coeffPtr = ref MemoryMarshal.GetReference(cspan);
            ref var coeff = ref Unsafe.As<Vector4, Vector128<float>>(ref coeffPtr);
            ref var src = ref MemoryMarshal.GetReference(srcBuffer);
            ref var dst = ref MemoryMarshal.GetReference(buffer);
            var xmm7 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref src, isx));
            for (i = 0; i < length; i++)
            {
                var cutmullCoeffs = Unsafe.Add(ref coeff, psx);
                Unsafe.Add(ref dst, i) = VectorUtils.FastDotProduct(xmm7, cutmullCoeffs);
                psx++;
                if (psx < ram) continue;
                psx = 0;
                isx++;
                if (isx >= slen) continue;
                xmm7 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref src, isx));
            }
            return new((int)isx, (int)psx);
        }

        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        internal static ResampleResult ResampleCachedDirectMonauralAnyRateSse3(UnifiedResampleArgs args, Span<float> buffer, Span<float> srcBuffer, Span<Vector4> cspan)
        {
            nint i = 0;
            nint length = buffer.Length;
            nint isx = 0;
            nint psx = args.ConversionGradient;
            nint acc = args.GradientIncrement;
            nint facc = args.IndexIncrement;
            nint nram = args.RateMul;
            nint nrci = args.RearrangedCoeffsIndex;
            ref var coeffPtr = ref MemoryMarshal.GetReference(cspan);
            ref var coeff = ref Unsafe.As<Vector4, Vector128<float>>(ref coeffPtr);
            ref var src = ref MemoryMarshal.GetReference(srcBuffer);
            ref var dst = ref MemoryMarshal.GetReference(buffer);
            Vector128<float> xmm7;
            for (i = 0; i < length; i++)
            {
                xmm7 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref src, isx));
                var xmm0 = Unsafe.Add(ref coeff, nrci);
                xmm0 = Sse.Multiply(xmm0, xmm7);
                var j = ++nrci < nram;
                nint z = Unsafe.As<bool, byte>(ref j);
                psx += acc;
                xmm7 = Sse2.UnpackHigh(xmm0.AsDouble(), xmm0.AsDouble()).AsSingle();
                nrci &= -z;
                isx += facc;
                xmm0 = Sse.Add(xmm0, xmm7);
                var v = psx >= nram;
                nint v1 = Unsafe.As<bool, byte>(ref v);
                xmm7 = Sse3.MoveHighAndDuplicate(xmm0);
                isx += v1;
                psx -= nram & -v1;
                xmm0 = Sse.AddScalar(xmm0, xmm7);
                Unsafe.Add(ref dst, i) = xmm0.GetElement(0);
            }
            return new((int)isx, (int)psx, (int)nrci);
        }
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        internal static ResampleResult ResampleCachedDirectMonauralDoubleRateSse41(UnifiedResampleArgs args, Span<float> buffer, Span<float> srcBuffer, Span<Vector4> cspan)
        {
            nint i = 0;
            nint length = buffer.Length;
            nint isx = 0;
            nint psx = args.ConversionGradient & 1;
            ref var coeffPtr = ref MemoryMarshal.GetReference(cspan);
            ref var coeff = ref Unsafe.As<Vector4, Vector128<float>>(ref coeffPtr);
            ref var src = ref MemoryMarshal.GetReference(srcBuffer);
            ref var dst = ref MemoryMarshal.GetReference(buffer);
            var xmm6 = Unsafe.As<float, Vector128<float>>(ref src);
            var xmm0 = Unsafe.Add(ref coeff, 0);
            var xmm1 = Unsafe.Add(ref coeff, 1);
            if (psx > 0)
            {
                var xmm2 = Sse.Multiply(xmm1, xmm6);
                var xmm4 = Sse2.UnpackHigh(xmm2.AsDouble(), xmm2.AsDouble()).AsSingle();
                xmm2 = Sse.Add(xmm4, xmm2);
                xmm4 = Sse.Shuffle(xmm2, xmm2, 0b01_01_01_01);
                xmm2 = Sse.AddScalar(xmm2, xmm4);
                Unsafe.Add(ref dst, i++) = xmm2.GetElement(0);
                isx++;
                xmm6 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref src, isx));
                psx = 0;
            }
            var olen = length - 3;
            for (; i < olen; i += 4)
            {
                var xmm7 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref src, isx + 1));
                isx += 2;
                var xmm2 = Sse.Multiply(xmm6, xmm0);
                var xmm3 = Sse.Multiply(xmm6, xmm1);
                var xmm4 = Sse2.UnpackHigh(xmm2.AsDouble(), xmm3.AsDouble()).AsSingle();
                xmm2 = Sse2.UnpackLow(xmm2.AsDouble(), xmm3.AsDouble()).AsSingle();
                xmm2 = Sse.Add(xmm2, xmm4);
                xmm4 = Sse.Multiply(xmm7, xmm0);
                xmm6 = Sse.Multiply(xmm7, xmm1);
                xmm3 = Sse2.UnpackHigh(xmm4.AsDouble(), xmm6.AsDouble()).AsSingle();
                xmm4 = Sse2.UnpackLow(xmm4.AsDouble(), xmm6.AsDouble()).AsSingle();
                xmm3 = Sse.Add(xmm3, xmm4);
                xmm4 = Sse.Shuffle(xmm2, xmm3, 0b11_01_11_01);
                xmm6 = Sse.Shuffle(xmm2, xmm3, 0b10_00_10_00);
                xmm2 = Sse.Add(xmm4, xmm6);
                xmm6 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref src, isx));
                Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref dst, i)) = xmm2;
            }
            olen = length - 1;
            for (; i < olen; i += 2)
            {
                var xmm2 = Sse.Multiply(xmm0, xmm6);
                var xmm3 = Sse.Multiply(xmm1, xmm6);
                var xmm4 = Sse2.UnpackHigh(xmm2.AsDouble(), xmm3.AsDouble()).AsSingle();
                var xmm5 = Sse2.UnpackLow(xmm2.AsDouble(), xmm3.AsDouble()).AsSingle();
                xmm3 = Sse.Add(xmm4, xmm5);
                xmm4 = Sse.Shuffle(xmm3, xmm3, 0b11_11_11_01);
                xmm5 = Sse.Shuffle(xmm3, xmm3, 0b11_11_10_00);
                xmm2 = Sse.Add(xmm4, xmm5);
                Unsafe.As<float, double>(ref Unsafe.Add(ref dst, i)) = xmm2.AsDouble().GetElement(0);
                isx++;
                xmm6 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref src, isx));
            }
            for (; i < length; i++)
            {
                var cutmullCoeffs = Unsafe.Add(ref coeff, psx);
                Unsafe.Add(ref dst, i) = VectorUtils.FastDotProduct(xmm6, cutmullCoeffs);
                psx++;
                if (psx < 2)
                {
                    continue;
                }
                psx = 0;
                isx++;
                xmm6 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref src, isx));
            }
            return new((int)isx, (int)psx);
        }
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        internal static ResampleResult ResampleCachedDirectMonauralQuadrupleRateSse41(UnifiedResampleArgs args, Span<float> buffer, Span<float> srcBuffer, Span<Vector4> cspan)
        {
            nint i = 0;
            nint length = buffer.Length;
            nint isx = 0;
            nint psx = args.ConversionGradient & 3;
            ref var coeffPtr = ref MemoryMarshal.GetReference(cspan);
            ref var coeff = ref Unsafe.As<Vector4, Vector128<float>>(ref coeffPtr);
            ref var src = ref MemoryMarshal.GetReference(srcBuffer);
            ref var dst = ref MemoryMarshal.GetReference(buffer);
            Vector128<float> xmm7;
            if (psx > 0)
            {
                for (; i < length; i++)
                {
                    xmm7 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref src, isx));
                    var cutmullCoeffs = Unsafe.Add(ref coeff, psx);
                    Unsafe.Add(ref dst, i) = VectorUtils.FastDotProduct(xmm7, cutmullCoeffs);
                    psx++;
                    if (psx >= 4)
                    {
                        psx = 0;
                        isx++;
                        break;
                    }
                }
            }
            var c0 = Unsafe.Add(ref coeff, 0);
            var c1 = Unsafe.Add(ref coeff, 1);
            var c2 = Unsafe.Add(ref coeff, 2);
            var c3 = Unsafe.Add(ref coeff, 3);
            var olen = length - 3;
            for (; i < olen; i += 4)
            {
                xmm7 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref src, isx));
                var xmm0 = Sse.Multiply(xmm7, c0);
                var xmm1 = Sse.Multiply(xmm7, c1);
                var xmm2 = Sse2.UnpackHigh(xmm0.AsDouble(), xmm1.AsDouble()).AsSingle();
                xmm0 = Sse2.UnpackLow(xmm0.AsDouble(), xmm1.AsDouble()).AsSingle();
                xmm0 = Sse.Add(xmm0, xmm2);
                xmm2 = Sse.Multiply(xmm7, c2);
                xmm7 = Sse.Multiply(xmm7, c3);
                xmm1 = Sse2.UnpackHigh(xmm2.AsDouble(), xmm7.AsDouble()).AsSingle();
                xmm2 = Sse2.UnpackLow(xmm2.AsDouble(), xmm7.AsDouble()).AsSingle();
                xmm1 = Sse.Add(xmm1, xmm2);
                xmm2 = Sse.Shuffle(xmm0, xmm1, 0b11_01_11_01);
                xmm7 = Sse.Shuffle(xmm0, xmm1, 0b10_00_10_00);
                xmm0 = Sse.Add(xmm2, xmm7);
                Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref dst, i)) = xmm0;
                isx++;
            }
            for (; i < length; i++)
            {
                xmm7 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref src, isx));
                var cutmullCoeffs = Unsafe.Add(ref coeff, psx);
                Unsafe.Add(ref dst, i) = VectorUtils.FastDotProduct(xmm7, cutmullCoeffs);
                psx++;
                if (psx >= 4)
                {
                    psx = 0;
                    isx++;
                }
            }
            return new((int)isx, (int)psx);
        }
        #endregion
        #region Stereo
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private unsafe ResampleFunc GetFuncCachedDirect2ChannelsX86(in UnifiedResampleArgs args)
            => (args.IndexIncrement, args.GradientIncrement, args.RateMul) switch
            {
                (0, 1, 2) when Sse.IsSupported && X86Intrinsics.HasAllFeatures(X86IntrinsicsMask.Sse)
                => new(&ResampleCachedDirectStereoDoubleRateSse),
                (0, 1, 4) when Sse.X64.IsSupported && X86Intrinsics.HasAllFeatures(X86IntrinsicsMask.Sse | X86IntrinsicsMask.X64)
                => new(&ResampleCachedDirectStereoQuadrupleRateX64),
                (0, 1, _) when Sse.IsSupported && X86Intrinsics.HasAllFeatures(X86IntrinsicsMask.Sse)
                => new(&ResampleCachedDirectStereoIntegerRateSse),
                (0, _, _) when Sse.IsSupported && X86Intrinsics.HasAllFeatures(X86IntrinsicsMask.Sse)
                => new(&ResampleCachedDirectStereoUpAnyRateSse),
                (_, _, _) when Sse.IsSupported && X86Intrinsics.HasAllFeatures(X86IntrinsicsMask.Sse)
                => new(&ResampleCachedDirectStereoAnyRateSse),
                _ => new(&ResampleCachedDirect2ChannelsStandard),
            };

        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        internal static ResampleResult ResampleCachedDirectStereoUpAnyRateSse(UnifiedResampleArgs args, Span<float> buffer, Span<float> srcBuffer, Span<Vector4> cspan)
        {
            nint isx = 0;
            nint psx = args.ConversionGradient;
            nint acc = args.GradientIncrement;
            nint nram = args.RateMul;
            nint nrci = args.RearrangedCoeffsIndex;
            ref var coeffPtr = ref MemoryMarshal.GetReference(cspan);
            ref var vBuffer = ref Unsafe.As<float, double>(ref MemoryMarshal.GetReference(buffer));
            ref var vSrcBuffer = ref Unsafe.As<float, double>(ref MemoryMarshal.GetReference(srcBuffer));
            ref var coeff = ref Unsafe.As<Vector4, Vector128<float>>(ref coeffPtr);
            nint i;
            nint length = buffer.Length / 2;
            var vx = Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref vSrcBuffer, isx));
            var vz = Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref vSrcBuffer, isx + 2));
            for (i = 0; i < length; i++)
            {
                var c = Unsafe.Add(ref coeff, nrci++);
                var vl = Sse.Shuffle(c, c, 0b01_01_00_00);
                var vr = Sse.Shuffle(c, c, 0b11_11_10_10);
                vl = Sse.Multiply(vx, vl);
                vr = Sse.Multiply(vz, vr);
                var j = nrci < nram;
                nint z = Unsafe.As<bool, byte>(ref j);
                vl = Sse.Add(vl, vr);
                vr = Sse.Shuffle(vl, vl, 0b00_00_11_10);
                nrci &= -z;
                vl = Sse.Add(vl, vr);
                Unsafe.Add(ref vBuffer, i) = vl.AsDouble().GetElement(0);
                psx += acc;
                if (psx >= nram)
                {
                    psx -= nram;
                    isx++;
                    vx = Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref vSrcBuffer, isx));
                    vz = Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref vSrcBuffer, isx + 2));
                }
            }
            return new((int)isx, (int)psx, (int)nrci);
        }

        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        internal static ResampleResult ResampleCachedDirectStereoIntegerRateSse(UnifiedResampleArgs args, Span<float> buffer, Span<float> srcBuffer, Span<Vector4> cspan)
        {
            nint isx = 0;
            nint psx = args.ConversionGradient;
            nint ram = args.RateMul;
            ref var coeffPtr = ref MemoryMarshal.GetReference(cspan);
            ref var vBuffer = ref Unsafe.As<float, double>(ref MemoryMarshal.GetReference(buffer));
            ref var vSrcBuffer = ref Unsafe.As<float, double>(ref MemoryMarshal.GetReference(srcBuffer));
            ref var coeff = ref Unsafe.As<Vector4, Vector128<float>>(ref coeffPtr);
            nint i;
            nint length = buffer.Length / 2;
            var vx = Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref vSrcBuffer, isx));
            var vz = Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref vSrcBuffer, isx + 2));
            for (i = 0; i < length; i++)
            {
                var c = Unsafe.Add(ref coeff, psx);
                var vl = Sse.Shuffle(c, c, 0b01_01_00_00);
                var vr = Sse.Shuffle(c, c, 0b11_11_10_10);
                vl = Sse.Multiply(vx, vl);
                vr = Sse.Multiply(vz, vr);
                vl = Sse.Add(vl, vr);
                vr = Sse.Shuffle(vl, vl, 0b00_00_11_10);
                vl = Sse.Add(vl, vr);
                Unsafe.Add(ref vBuffer, i) = vl.AsDouble().GetElement(0);
                psx++;
                if (psx >= ram)
                {
                    psx = 0;
                    isx++;
                    vx = Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref vSrcBuffer, isx));
                    vz = Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref vSrcBuffer, isx + 2));
                }
            }

            return new((int)isx, (int)psx);
        }

        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        internal static ResampleResult ResampleCachedDirectStereoDoubleRateSse(UnifiedResampleArgs args, Span<float> buffer, Span<float> srcBuffer, Span<Vector4> cspan)
        {
            nint isx = 0;
            nint psx = args.ConversionGradient;
            ref var coeffPtr = ref MemoryMarshal.GetReference(cspan);
            ref var vBuffer = ref Unsafe.As<float, double>(ref MemoryMarshal.GetReference(buffer));
            ref var vSrcBuffer = ref Unsafe.As<float, double>(ref MemoryMarshal.GetReference(srcBuffer));
            ref var coeff = ref Unsafe.As<Vector4, Vector128<float>>(ref coeffPtr);
            nint i = 0;
            nint length = buffer.Length / 2;

            var xmm1 = Unsafe.Add(ref coeff, 0);
            var xmm3 = Unsafe.Add(ref coeff, 1);
            var xmm0 = Sse.Shuffle(xmm1, xmm1, 0b01_01_00_00);  //C0, C0, C1, C1
            xmm1 = Sse.Shuffle(xmm1, xmm1, 0b11_11_10_10);      //C2, C2, C3, C3
            var xmm2 = Sse.Shuffle(xmm3, xmm3, 0b01_01_00_00);  //C4, C4, C5, C5
            xmm3 = Sse.Shuffle(xmm3, xmm3, 0b11_11_10_10);      //C6, C6, C7, C7
            var xmm6 = Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref vSrcBuffer, isx));        //L0, R0, L1, R1
            var xmm7 = Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref vSrcBuffer, isx + 2));    //L2, R2, L3, R3
            if (psx > 0)
            {
                var xmm4 = Sse.Multiply(xmm6, xmm2);
                var xmm5 = Sse.Multiply(xmm7, xmm3);
                xmm4 = Sse.Add(xmm4, xmm5);
                xmm5 = Sse.Shuffle(xmm4, xmm4, 0b00_00_11_10);
                xmm4 = Sse.Add(xmm4, xmm5);
                Unsafe.Add(ref vBuffer, i++) = xmm4.AsDouble().GetElement(0);
                isx++;
                xmm6 = Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref vSrcBuffer, isx));
                xmm7 = Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref vSrcBuffer, isx + 2));
                psx = 0;
            }
            var olen = length - 1;
            for (; i < olen; i += 2)
            {
                var xmm4 = Sse.Multiply(xmm6, xmm0);    //L0 * C0, R0 * C0, L1 * C1, R1 * C1
                var xmm5 = Sse.Multiply(xmm7, xmm1);    //L2 * C2, R2 * C2, L3 * C3, R3 * C3
                xmm6 = Sse.Multiply(xmm6, xmm2);        //L4 * C4, R4 * C4, L5 * C5, R5 * C5
                xmm7 = Sse.Multiply(xmm7, xmm3);        //L6 * C6, R6 * C6, L7 * C7, R7 * C7
                xmm4 = Sse.Add(xmm4, xmm5);
                xmm6 = Sse.Add(xmm6, xmm7);
                xmm5 = Sse.Shuffle(xmm4, xmm6, 0b11_10_11_10);
                xmm7 = Sse.Shuffle(xmm4, xmm6, 0b01_00_01_00);
                xmm4 = Sse.Add(xmm5, xmm7);
                Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref vBuffer, i)) = xmm4;
                isx++;
                xmm6 = Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref vSrcBuffer, isx));
                xmm7 = Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref vSrcBuffer, isx + 2));
            }
            if (i < length)
            {
                var xmm4 = Sse.Multiply(xmm6, xmm0);
                var xmm5 = Sse.Multiply(xmm7, xmm1);
                xmm4 = Sse.Add(xmm4, xmm5);
                xmm5 = Sse.Shuffle(xmm4, xmm4, 0b00_00_11_10);
                xmm4 = Sse.Add(xmm4, xmm5);
                Unsafe.Add(ref vBuffer, i) = xmm4.AsDouble().GetElement(0);
                psx++;
                isx += psx >> 1;
                psx &= 1;
            }

            return new((int)isx, (int)psx);
        }
        /// <summary>
        /// This variant needs more than 8 xmm registers so AVX or x64 SSE is required<br/>
        /// It does not use 256bit floating-point arithmetic, making it suitable for the Haswell micro-architecture.<br/>
        /// TODO: Post-Rocket-Lake variant using 256bit floating-point arithmetic
        /// </summary>
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        internal static ResampleResult ResampleCachedDirectStereoQuadrupleRateX64(UnifiedResampleArgs args, Span<float> buffer, Span<float> srcBuffer, Span<Vector4> cspan)
        {
            nint isx = 0;
            nint psx = args.ConversionGradient;
            ref var coeffPtr = ref MemoryMarshal.GetReference(cspan);
            ref var vBuffer = ref Unsafe.As<float, double>(ref MemoryMarshal.GetReference(buffer));
            ref var vSrcBuffer = ref Unsafe.As<float, double>(ref MemoryMarshal.GetReference(srcBuffer));
            ref var coeff = ref Unsafe.As<Vector4, Vector128<float>>(ref coeffPtr);
            nint i = 0;
            nint length = buffer.Length / 2;
            var xmm14 = Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref vSrcBuffer, isx));        //L0, R0, L1, R1
            var xmm15 = Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref vSrcBuffer, isx + 2));    //L2, R2, L3, R3
            if (psx > 0)
            {
                for (; i < length; i++)
                {
                    var xmm9 = Unsafe.Add(ref coeff, psx);
                    var xmm8 = Sse.Shuffle(xmm9, xmm9, 0b01_01_00_00);
                    xmm9 = Sse.Shuffle(xmm9, xmm9, 0b11_11_10_10);
                    xmm8 = Sse.Multiply(xmm14, xmm8);
                    xmm9 = Sse.Multiply(xmm15, xmm9);
                    xmm8 = Sse.Add(xmm8, xmm9);
                    xmm9 = Sse.Shuffle(xmm8, xmm8, 0b00_00_11_10);
                    xmm8 = Sse.Add(xmm8, xmm9);
                    Unsafe.Add(ref vBuffer, i) = xmm8.AsDouble().GetElement(0);
                    psx++;
                    if (psx >= 4)
                    {
                        psx = 0;
                        isx++;
                        xmm14 = Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref vSrcBuffer, isx));
                        xmm15 = Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref vSrcBuffer, isx + 2));
                        break;
                    }
                }
            }
            var xmm1 = Unsafe.Add(ref coeff, 0);
            var xmm3 = Unsafe.Add(ref coeff, 1);
            var xmm5 = Unsafe.Add(ref coeff, 2);
            var xmm7 = Unsafe.Add(ref coeff, 3);
            var xmm0 = Sse.Shuffle(xmm1, xmm1, 0b01_01_00_00);  //C0, C0, C1, C1
            xmm1 = Sse.Shuffle(xmm1, xmm1, 0b11_11_10_10);      //C2, C2, C3, C3
            var xmm2 = Sse.Shuffle(xmm3, xmm3, 0b01_01_00_00);  //C4, C4, C5, C5
            xmm3 = Sse.Shuffle(xmm3, xmm3, 0b11_11_10_10);      //C6, C6, C7, C7
            var xmm4 = Sse.Shuffle(xmm5, xmm5, 0b01_01_00_00);  //C0, C0, C1, C1
            xmm5 = Sse.Shuffle(xmm5, xmm5, 0b11_11_10_10);      //C2, C2, C3, C3
            var xmm6 = Sse.Shuffle(xmm7, xmm7, 0b01_01_00_00);  //C4, C4, C5, C5
            xmm7 = Sse.Shuffle(xmm7, xmm7, 0b11_11_10_10);      //C6, C6, C7, C7
            var olen = length - 3;
            for (; i < olen; i += 4)
            {
                var xmm8 = Sse.Multiply(xmm14, xmm0);
                var xmm9 = Sse.Multiply(xmm15, xmm1);
                var xmm10 = Sse.Multiply(xmm14, xmm2);
                var xmm11 = Sse.Multiply(xmm15, xmm3);
                var xmm12 = Sse.Multiply(xmm14, xmm4);
                var xmm13 = Sse.Multiply(xmm15, xmm5);
                xmm14 = Sse.Multiply(xmm14, xmm6);
                xmm15 = Sse.Multiply(xmm15, xmm7);
                xmm8 = Sse.Add(xmm8, xmm9);
                xmm10 = Sse.Add(xmm10, xmm11);
                xmm12 = Sse.Add(xmm12, xmm13);
                xmm14 = Sse.Add(xmm14, xmm15);
                xmm9 = Sse.Shuffle(xmm8, xmm10, 0b11_10_11_10);
                xmm11 = Sse.Shuffle(xmm8, xmm10, 0b01_00_01_00);
                xmm13 = Sse.Shuffle(xmm12, xmm14, 0b11_10_11_10);
                xmm15 = Sse.Shuffle(xmm12, xmm14, 0b01_00_01_00);
                xmm8 = Sse.Add(xmm9, xmm11);
                xmm9 = Sse.Add(xmm13, xmm15);
                Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref vBuffer, i)) = xmm8;
                Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref vBuffer, i + 2)) = xmm9;
                isx++;
                xmm14 = Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref vSrcBuffer, isx));
                xmm15 = Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref vSrcBuffer, isx + 2));
            }
            for (; i < length; i++)
            {
                var xmm9 = Unsafe.Add(ref coeff, psx);
                var xmm8 = Sse.Shuffle(xmm9, xmm9, 0b01_01_00_00);
                xmm9 = Sse.Shuffle(xmm9, xmm9, 0b11_11_10_10);
                xmm8 = Sse.Multiply(xmm14, xmm8);
                xmm9 = Sse.Multiply(xmm15, xmm9);
                xmm8 = Sse.Add(xmm8, xmm9);
                xmm9 = Sse.Shuffle(xmm8, xmm8, 0b00_00_11_10);
                xmm8 = Sse.Add(xmm8, xmm9);
                Unsafe.Add(ref vBuffer, i) = xmm8.AsDouble().GetElement(0);
                psx++;
                if (psx >= 4)
                {
                    psx = 0;
                    isx++;
                    xmm14 = Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref vSrcBuffer, isx));
                    xmm15 = Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref vSrcBuffer, isx + 2));
                }
            }
            return new((int)isx, (int)psx);
        }
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        internal static ResampleResult ResampleCachedDirectStereoAnyRateSse(UnifiedResampleArgs args, Span<float> buffer, Span<float> srcBuffer, Span<Vector4> cspan)
        {
            nint isx = 0;
            nint psx = args.ConversionGradient;
            nint acc = args.GradientIncrement;
            nint facc = args.IndexIncrement;
            nint ram = args.RateMul;
            ref var coeffPtr = ref MemoryMarshal.GetReference(cspan);
            nint nrci = args.RearrangedCoeffsIndex;
            var nram = ram;
            ref var vBuffer = ref Unsafe.As<float, double>(ref MemoryMarshal.GetReference(buffer));
            ref var vSrcBuffer = ref Unsafe.As<float, double>(ref MemoryMarshal.GetReference(srcBuffer));
            ref var coeff = ref Unsafe.As<Vector4, Vector128<float>>(ref coeffPtr);
            nint i;
            nint length = buffer.Length / 2;
            for (i = 0; i < length; i++)
            {
                var vx = Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref vSrcBuffer, isx));
                var vz = Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref vSrcBuffer, isx + 2));
                var c = Unsafe.Add(ref coeff, nrci++);
                var vl = Sse.Shuffle(c, c, 0b01_01_00_00);
                var vr = Sse.Shuffle(c, c, 0b11_11_10_10);
                psx += acc;
                isx += facc;
                vl = Sse.Multiply(vx, vl);
                vr = Sse.Multiply(vz, vr);
                var j = nrci < nram;
                nint z = Unsafe.As<bool, byte>(ref j);
                vl = Sse.Add(vl, vr);
                vr = Sse.Shuffle(vl, vl, 0b00_00_11_10);
                vl = Sse.Add(vl, vr);
                Unsafe.Add(ref vBuffer, i) = vl.AsDouble().GetElement(0);
                nrci &= -z;
                var a = psx >= nram;
                int b = Unsafe.As<bool, byte>(ref a);
                isx += b;
                psx -= nram & (-b);
            }
            return new((int)isx, (int)psx, (int)nrci);
        }

        #endregion
        #region Generic
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static unsafe ResampleFunc GetFuncCachedDirectGenericX86(in UnifiedResampleArgs args) => Avx2.IsSupported ? (new(&ResampleCachedDirectGenericAvx2)) : (new(&ResampleCachedDirectGenericStandard));
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        internal static ResampleResult ResampleCachedDirectGenericAvx2(UnifiedResampleArgs args, Span<float> buffer, Span<float> srcBuffer, Span<Vector4> cspan)
        {
            nint i = 0;
            nint isx = 0;
            nint psx = args.ConversionGradient;
            var channels = args.Channels;
            nint nchannels = channels;
            nint acc = args.GradientIncrement;
            nint facc = args.IndexIncrement;
            nint nram = args.RateMul;
            nint nrci = args.RearrangedCoeffsIndex;
            ref var coeffPtr = ref MemoryMarshal.GetReference(cspan);
            var length = buffer.Length - nchannels + 1;
            var rch = (int)nchannels & 0x3;
            var mask = Sse2.CompareGreaterThan(Vector128.Create(rch), Vector128.Create(0, 1, 2, 3)).AsSingle();

            unsafe
            {
                fixed (float* rsi = srcBuffer)
                fixed (float* rdi = buffer)
                {
                    for (; i < length; i += nchannels)
                    {
                        psx += acc;
                        var y0 = Unsafe.Add(ref coeffPtr, nrci);
                        var head = rsi + isx * nchannels;
                        var head2 = head + nchannels * 2;
                        nint nch = 0;
                        var cholen = nchannels - 7;
                        for (; nch < cholen; nch += 8)
                        {
                            var vy0 = y0.AsVector128();
                            var v0 = Avx2.BroadcastScalarToVector256(Sse.Shuffle(vy0, vy0, 0b00_00_00_00));
                            var v2 = Avx2.BroadcastScalarToVector256(Sse.Shuffle(vy0, vy0, 0b10_10_10_10));
                            var v1 = Avx2.BroadcastScalarToVector256(Sse.Shuffle(vy0, vy0, 0b01_01_01_01));
                            var v3 = Avx2.BroadcastScalarToVector256(Sse.Shuffle(vy0, vy0, 0b11_11_11_11));
                            v0 = Avx.Multiply(v0, *(Vector256<float>*)(head + nch));
                            v2 = Avx.Multiply(v2, *(Vector256<float>*)(head2 + nch));
                            v0 = Avx.Add(v0, v2);
                            v1 = Avx.Multiply(v1, *(Vector256<float>*)(head + nchannels + nch));
                            v3 = Avx.Multiply(v3, *(Vector256<float>*)(head2 + nchannels + nch));
                            v1 = Avx.Add(v1, v3);
                            v0 = Avx.Add(v0, v1);
                            *(Vector256<float>*)(rdi + i + nch) = v0;
                        }
                        cholen = nchannels - 3;
                        for (; nch < cholen; nch += 4)
                        {
                            var vy0 = y0.AsVector128();
                            var v0 = Sse.Shuffle(vy0, vy0, 0b00_00_00_00);
                            var v2 = Sse.Shuffle(vy0, vy0, 0b10_10_10_10);
                            var v1 = Sse.Shuffle(vy0, vy0, 0b01_01_01_01);
                            var v3 = Sse.Shuffle(vy0, vy0, 0b11_11_11_11);
                            v0 = Sse.Multiply(v0, *(Vector128<float>*)(head + nch));
                            v2 = Sse.Multiply(v2, *(Vector128<float>*)(head2 + nch));
                            v0 = Sse.Add(v0, v2);
                            v1 = Sse.Multiply(v1, *(Vector128<float>*)(head + nchannels + nch));
                            v3 = Sse.Multiply(v3, *(Vector128<float>*)(head2 + nchannels + nch));
                            v1 = Sse.Add(v1, v3);
                            v0 = Sse.Add(v0, v1);
                            *(Vector128<float>*)(rdi + i + nch) = v0;
                        }
                        if (nch < nchannels)
                        {
                            var vy0 = y0.AsVector128();
                            var v0 = Avx.MaskLoad(head + nch, mask);
                            var v2 = Avx.MaskLoad(head2 + nch, mask);
                            var v1 = Avx.MaskLoad(head + nchannels + nch, mask);
                            var v3 = Avx.MaskLoad(head2 + nchannels + nch, mask);
                            v0 = Sse.Multiply(v0, Sse.Shuffle(vy0, vy0, 0b00_00_00_00));
                            v2 = Sse.Multiply(v2, Sse.Shuffle(vy0, vy0, 0b10_10_10_10));
                            v0 = Sse.Add(v0, v2);
                            v1 = Sse.Multiply(v1, Sse.Shuffle(vy0, vy0, 0b01_01_01_01));
                            v3 = Sse.Multiply(v3, Sse.Shuffle(vy0, vy0, 0b11_11_11_11));
                            v1 = Sse.Add(v1, v3);
                            v0 = Sse.Add(v0, v1);
                            Avx.MaskStore(rdi + i + nch, mask, v0);
                        }
                        var j = ++nrci < nram;
                        nint z = Unsafe.As<bool, byte>(ref j);
                        nrci &= -z;
                        var h = psx >= nram;
                        nint g = Unsafe.As<bool, byte>(ref h);
                        isx += g;
                        isx += facc;
                        psx -= -g & nram;
                    }
                }
            }
            return new((int)isx, (int)psx, (int)nrci);
        }
        #endregion
        #endregion

        #region CachedWrappedEven
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static unsafe ResampleFunc GetFuncCachedWrappedEvenGenericX86(in UnifiedResampleArgs args) => Avx2.IsSupported ? (new(&ResampleCachedWrappedEvenGenericAvx2)) : (new(&ResampleCachedWrappedEvenGenericStandard));
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        internal static ResampleResult ResampleCachedWrappedEvenGenericAvx2(UnifiedResampleArgs args, Span<float> buffer, Span<float> srcBuffer, Span<Vector4> cspan)
        {
            nint isx = 0;
            nint psx = args.ConversionGradient;
            nint nchannels = args.Channels;
            nint acc = args.GradientIncrement;
            nint facc = args.IndexIncrement;
            nint ram = args.RateMul;
            var rec = args.RearrangedCoeffsIndex;
            var red = args.RearrangedCoeffsDirection;
            ref var coeffPtr = ref MemoryMarshal.GetReference(cspan);
            var rew = cspan.Length;
            var rewc = rew * nchannels;
            var rch = (int)nchannels & 0x3;
            var mask = Sse2.CompareGreaterThan(Vector128.Create(rch), Vector128.Create(0, 1, 2, 3)).AsSingle();
            nint i = 0, length = buffer.Length - nchannels + 1;
            unsafe
            {
                fixed (float* rsi = srcBuffer)
                fixed (float* rdi = buffer)
                {
                    if (red < 0)
                    {
                        var olen = MathI.Min(length, rec * nchannels + nchannels);
                        for (; i < olen; i += nchannels)
                        {
                            var y0 = VectorUtils.ReverseElements(Unsafe.Add(ref coeffPtr, rec--));
                            var head = rsi + isx * nchannels;
                            var head2 = head + nchannels * 2;
                            nint nch = 0;
                            var cholen = nchannels - 7;
                            for (; nch < cholen; nch += 8)
                            {
                                var vy0 = y0.AsVector128();
                                var v0 = Avx2.BroadcastScalarToVector256(Sse.Shuffle(vy0, vy0, 0b00_00_00_00));
                                var v2 = Avx2.BroadcastScalarToVector256(Sse.Shuffle(vy0, vy0, 0b10_10_10_10));
                                var v1 = Avx2.BroadcastScalarToVector256(Sse.Shuffle(vy0, vy0, 0b01_01_01_01));
                                var v3 = Avx2.BroadcastScalarToVector256(Sse.Shuffle(vy0, vy0, 0b11_11_11_11));
                                v0 = Avx.Multiply(v0, *(Vector256<float>*)(head + nch));
                                v2 = Avx.Multiply(v2, *(Vector256<float>*)(head2 + nch));
                                v0 = Avx.Add(v0, v2);
                                v1 = Avx.Multiply(v1, *(Vector256<float>*)(head + nchannels + nch));
                                v3 = Avx.Multiply(v3, *(Vector256<float>*)(head2 + nchannels + nch));
                                v1 = Avx.Add(v1, v3);
                                v0 = Avx.Add(v0, v1);
                                *(Vector256<float>*)(rdi + i + nch) = v0;
                            }
                            cholen = nchannels - 3;
                            for (; nch < cholen; nch += 4)
                            {
                                var vy0 = y0.AsVector128();
                                var v0 = Sse.Shuffle(vy0, vy0, 0b00_00_00_00);
                                var v2 = Sse.Shuffle(vy0, vy0, 0b10_10_10_10);
                                var v1 = Sse.Shuffle(vy0, vy0, 0b01_01_01_01);
                                var v3 = Sse.Shuffle(vy0, vy0, 0b11_11_11_11);
                                v0 = Sse.Multiply(v0, *(Vector128<float>*)(head + nch));
                                v2 = Sse.Multiply(v2, *(Vector128<float>*)(head2 + nch));
                                v0 = Sse.Add(v0, v2);
                                v1 = Sse.Multiply(v1, *(Vector128<float>*)(head + nchannels + nch));
                                v3 = Sse.Multiply(v3, *(Vector128<float>*)(head2 + nchannels + nch));
                                v1 = Sse.Add(v1, v3);
                                v0 = Sse.Add(v0, v1);
                                *(Vector128<float>*)(rdi + i + nch) = v0;
                            }
                            if (nch < nchannels)
                            {
                                var vy0 = y0.AsVector128();
                                var v0 = Avx.MaskLoad(head + nch, mask);
                                var v2 = Avx.MaskLoad(head2 + nch, mask);
                                var v1 = Avx.MaskLoad(head + nchannels + nch, mask);
                                var v3 = Avx.MaskLoad(head2 + nchannels + nch, mask);
                                v0 = Sse.Multiply(v0, Sse.Shuffle(vy0, vy0, 0b00_00_00_00));
                                v2 = Sse.Multiply(v2, Sse.Shuffle(vy0, vy0, 0b10_10_10_10));
                                v0 = Sse.Add(v0, v2);
                                v1 = Sse.Multiply(v1, Sse.Shuffle(vy0, vy0, 0b01_01_01_01));
                                v3 = Sse.Multiply(v3, Sse.Shuffle(vy0, vy0, 0b11_11_11_11));
                                v1 = Sse.Add(v1, v3);
                                v0 = Sse.Add(v0, v1);
                                Avx.MaskStore(rdi + i + nch, mask, v0);
                            }
                            psx += acc;
                            isx += facc;
                            var h = psx >= ram;
                            int y = Unsafe.As<bool, byte>(ref h);
                            isx += y;
                            psx -= -y & ram;
                        }
                        if (rec <= 0)
                        {
                            red = 1;
                            rec = 0;
                        }
                    }
                    while (i < length)
                    {
                        var olen = MathI.Min(length, i - rec * nchannels + rewc);
                        for (; i < olen; i += nchannels)
                        {
                            var y0 = Unsafe.Add(ref coeffPtr, rec++);
                            var head = rsi + isx * nchannels;
                            var head2 = head + nchannels * 2;
                            nint nch = 0;
                            var cholen = nchannels - 7;
                            for (; nch < cholen; nch += 8)
                            {
                                var vy0 = y0.AsVector128();
                                var v0 = Avx2.BroadcastScalarToVector256(Sse.Shuffle(vy0, vy0, 0b00_00_00_00));
                                var v2 = Avx2.BroadcastScalarToVector256(Sse.Shuffle(vy0, vy0, 0b10_10_10_10));
                                var v1 = Avx2.BroadcastScalarToVector256(Sse.Shuffle(vy0, vy0, 0b01_01_01_01));
                                var v3 = Avx2.BroadcastScalarToVector256(Sse.Shuffle(vy0, vy0, 0b11_11_11_11));
                                v0 = Avx.Multiply(v0, *(Vector256<float>*)(head + nch));
                                v2 = Avx.Multiply(v2, *(Vector256<float>*)(head2 + nch));
                                v0 = Avx.Add(v0, v2);
                                v1 = Avx.Multiply(v1, *(Vector256<float>*)(head + nchannels + nch));
                                v3 = Avx.Multiply(v3, *(Vector256<float>*)(head2 + nchannels + nch));
                                v1 = Avx.Add(v1, v3);
                                v0 = Avx.Add(v0, v1);
                                *(Vector256<float>*)(rdi + i + nch) = v0;
                            }
                            cholen = nchannels - 3;
                            for (; nch < cholen; nch += 4)
                            {
                                var vy0 = y0.AsVector128();
                                var v0 = Sse.Shuffle(vy0, vy0, 0b00_00_00_00);
                                var v2 = Sse.Shuffle(vy0, vy0, 0b10_10_10_10);
                                var v1 = Sse.Shuffle(vy0, vy0, 0b01_01_01_01);
                                var v3 = Sse.Shuffle(vy0, vy0, 0b11_11_11_11);
                                v0 = Sse.Multiply(v0, *(Vector128<float>*)(head + nch));
                                v2 = Sse.Multiply(v2, *(Vector128<float>*)(head2 + nch));
                                v0 = Sse.Add(v0, v2);
                                v1 = Sse.Multiply(v1, *(Vector128<float>*)(head + nchannels + nch));
                                v3 = Sse.Multiply(v3, *(Vector128<float>*)(head2 + nchannels + nch));
                                v1 = Sse.Add(v1, v3);
                                v0 = Sse.Add(v0, v1);
                                *(Vector128<float>*)(rdi + i + nch) = v0;
                            }
                            if (nch < nchannels)
                            {
                                var vy0 = y0.AsVector128();
                                var v0 = Avx.MaskLoad(head + nch, mask);
                                var v2 = Avx.MaskLoad(head2 + nch, mask);
                                var v1 = Avx.MaskLoad(head + nchannels + nch, mask);
                                var v3 = Avx.MaskLoad(head2 + nchannels + nch, mask);
                                v0 = Sse.Multiply(v0, Sse.Shuffle(vy0, vy0, 0b00_00_00_00));
                                v2 = Sse.Multiply(v2, Sse.Shuffle(vy0, vy0, 0b10_10_10_10));
                                v0 = Sse.Add(v0, v2);
                                v1 = Sse.Multiply(v1, Sse.Shuffle(vy0, vy0, 0b01_01_01_01));
                                v3 = Sse.Multiply(v3, Sse.Shuffle(vy0, vy0, 0b11_11_11_11));
                                v1 = Sse.Add(v1, v3);
                                v0 = Sse.Add(v0, v1);
                                Avx.MaskStore(rdi + i + nch, mask, v0);
                            }
                            psx += acc;
                            isx += facc;
                            var h = psx >= ram;
                            int y = Unsafe.As<bool, byte>(ref h);
                            isx += y;
                            psx -= -y & ram;
                        }
                        if (rec >= rew)
                        {
                            red = -1;
                            rec--;
                        }
                        olen = MathI.Min(length, i + rec * nchannels + nchannels);
                        for (; i < olen; i += nchannels)
                        {
                            var y0 = VectorUtils.ReverseElements(Unsafe.Add(ref coeffPtr, rec--));
                            var head = rsi + isx * nchannels;
                            var head2 = head + nchannels * 2;
                            nint nch = 0;
                            var cholen = nchannels - 7;
                            for (; nch < cholen; nch += 8)
                            {
                                var vy0 = y0.AsVector128();
                                var v0 = Avx2.BroadcastScalarToVector256(Sse.Shuffle(vy0, vy0, 0b00_00_00_00));
                                var v2 = Avx2.BroadcastScalarToVector256(Sse.Shuffle(vy0, vy0, 0b10_10_10_10));
                                var v1 = Avx2.BroadcastScalarToVector256(Sse.Shuffle(vy0, vy0, 0b01_01_01_01));
                                var v3 = Avx2.BroadcastScalarToVector256(Sse.Shuffle(vy0, vy0, 0b11_11_11_11));
                                v0 = Avx.Multiply(v0, *(Vector256<float>*)(head + nch));
                                v2 = Avx.Multiply(v2, *(Vector256<float>*)(head2 + nch));
                                v0 = Avx.Add(v0, v2);
                                v1 = Avx.Multiply(v1, *(Vector256<float>*)(head + nchannels + nch));
                                v3 = Avx.Multiply(v3, *(Vector256<float>*)(head2 + nchannels + nch));
                                v1 = Avx.Add(v1, v3);
                                v0 = Avx.Add(v0, v1);
                                *(Vector256<float>*)(rdi + i + nch) = v0;
                            }
                            cholen = nchannels - 3;
                            for (; nch < cholen; nch += 4)
                            {
                                var vy0 = y0.AsVector128();
                                var v0 = Sse.Shuffle(vy0, vy0, 0b00_00_00_00);
                                var v2 = Sse.Shuffle(vy0, vy0, 0b10_10_10_10);
                                var v1 = Sse.Shuffle(vy0, vy0, 0b01_01_01_01);
                                var v3 = Sse.Shuffle(vy0, vy0, 0b11_11_11_11);
                                v0 = Sse.Multiply(v0, *(Vector128<float>*)(head + nch));
                                v2 = Sse.Multiply(v2, *(Vector128<float>*)(head2 + nch));
                                v0 = Sse.Add(v0, v2);
                                v1 = Sse.Multiply(v1, *(Vector128<float>*)(head + nchannels + nch));
                                v3 = Sse.Multiply(v3, *(Vector128<float>*)(head2 + nchannels + nch));
                                v1 = Sse.Add(v1, v3);
                                v0 = Sse.Add(v0, v1);
                                *(Vector128<float>*)(rdi + i + nch) = v0;
                            }
                            if (nch < nchannels)
                            {
                                var vy0 = y0.AsVector128();
                                var v0 = Avx.MaskLoad(head + nch, mask);
                                var v2 = Avx.MaskLoad(head2 + nch, mask);
                                var v1 = Avx.MaskLoad(head + nchannels + nch, mask);
                                var v3 = Avx.MaskLoad(head2 + nchannels + nch, mask);
                                v0 = Sse.Multiply(v0, Sse.Shuffle(vy0, vy0, 0b00_00_00_00));
                                v2 = Sse.Multiply(v2, Sse.Shuffle(vy0, vy0, 0b10_10_10_10));
                                v0 = Sse.Add(v0, v2);
                                v1 = Sse.Multiply(v1, Sse.Shuffle(vy0, vy0, 0b01_01_01_01));
                                v3 = Sse.Multiply(v3, Sse.Shuffle(vy0, vy0, 0b11_11_11_11));
                                v1 = Sse.Add(v1, v3);
                                v0 = Sse.Add(v0, v1);
                                Avx.MaskStore(rdi + i + nch, mask, v0);
                            }
                            psx += acc;
                            isx += facc;
                            var h = psx >= ram;
                            int y = Unsafe.As<bool, byte>(ref h);
                            isx += y;
                            psx -= -y & ram;
                        }
                        if (rec < 0)
                        {
                            red = 1;
                            rec = 0;
                        }
                    }
                }
            }
            return ((int)isx, (int)psx, rec, red);
        }
        #endregion

        #region CachedWrappedOdd
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static unsafe ResampleFunc GetFuncCachedWrappedOddGenericX86(in UnifiedResampleArgs args) => Avx2.IsSupported ? new(&ResampleCachedWrappedOddGenericAvx2) : new(&ResampleCachedWrappedOddGenericStandard);
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        internal static ResampleResult ResampleCachedWrappedOddGenericAvx2(UnifiedResampleArgs args, Span<float> buffer, Span<float> srcBuffer, Span<Vector4> cspan)
        {
            nint isx = 0;
            nint psx = args.ConversionGradient;
            nint nchannels = args.Channels;
            nint acc = args.GradientIncrement;
            nint facc = args.IndexIncrement;
            nint ram = args.RateMul;
            var rec = args.RearrangedCoeffsIndex;
            var red = args.RearrangedCoeffsDirection;
            ref var coeffPtr = ref MemoryMarshal.GetReference(cspan);
            var rew = cspan.Length;
            var rewc = rew * nchannels;
            var rch = (int)nchannels & 0x3;
            var mask = Sse2.CompareGreaterThan(Vector128.Create(rch), Vector128.Create(0, 1, 2, 3)).AsSingle();
            nint i = 0, length = buffer.Length - nchannels + 1;
            unsafe
            {
                fixed (float* rsi = srcBuffer)
                fixed (float* rdi = buffer)
                {
                    if (red < 0)
                    {
                        var olen = MathI.Min(length, rec * nchannels);
                        for (; i < olen; i += nchannels)
                        {
                            var y0 = VectorUtils.ReverseElements(Unsafe.Add(ref coeffPtr, rec--));
                            var vy0 = y0.AsVector128().ToVector256Unsafe();
                            var vy1 = Sse.Shuffle(vy0.GetLower(), vy0.GetLower(), 0b01_01_01_01).ToVector256Unsafe();
                            var vy2 = Sse.Shuffle(vy0.GetLower(), vy0.GetLower(), 0b10_10_10_10).ToVector256Unsafe();
                            var vy3 = Sse.Shuffle(vy0.GetLower(), vy0.GetLower(), 0b11_11_11_11).ToVector256Unsafe();
                            vy0 = Sse.Shuffle(vy0.GetLower(), vy0.GetLower(), 0b00_00_00_00).ToVector256Unsafe();
                            vy0 = vy0.WithUpper(vy0.GetLower());
                            vy1 = vy1.WithUpper(vy1.GetLower());
                            vy2 = vy2.WithUpper(vy2.GetLower());
                            vy3 = vy3.WithUpper(vy3.GetLower());
                            var head = rsi + isx * nchannels;
                            var head2 = head + nchannels * 2;
                            nint nch = 0;
                            var cholen = nchannels - 7;
                            for (; nch < cholen; nch += 8)
                            {
                                var v0 = Avx.Multiply(vy0, *(Vector256<float>*)(head + nch));
                                var v2 = Avx.Multiply(vy2, *(Vector256<float>*)(head2 + nch));
                                v0 = Avx.Add(v0, v2);
                                var v1 = Avx.Multiply(vy1, *(Vector256<float>*)(head + nchannels + nch));
                                var v3 = Avx.Multiply(vy3, *(Vector256<float>*)(head2 + nchannels + nch));
                                v1 = Avx.Add(v1, v3);
                                v0 = Avx.Add(v0, v1);
                                *(Vector256<float>*)(rdi + i + nch) = v0;
                            }
                            cholen = nchannels - 3;
                            for (; nch < cholen; nch += 4)
                            {
                                var v0 = Sse.Multiply(vy0.GetLower(), *(Vector128<float>*)(head + nch));
                                var v2 = Sse.Multiply(vy2.GetLower(), *(Vector128<float>*)(head2 + nch));
                                v0 = Sse.Add(v0, v2);
                                var v1 = Sse.Multiply(vy1.GetLower(), *(Vector128<float>*)(head + nchannels + nch));
                                var v3 = Sse.Multiply(vy3.GetLower(), *(Vector128<float>*)(head2 + nchannels + nch));
                                v1 = Sse.Add(v1, v3);
                                v0 = Sse.Add(v0, v1);
                                *(Vector128<float>*)(rdi + i + nch) = v0;
                            }
                            if (nch < nchannels)
                            {
                                var v0 = Avx.MaskLoad(head + nch, mask);
                                var v2 = Avx.MaskLoad(head2 + nch, mask);
                                var v1 = Avx.MaskLoad(head + nchannels + nch, mask);
                                var v3 = Avx.MaskLoad(head2 + nchannels + nch, mask);
                                v0 = Sse.Multiply(v0, vy0.GetLower());
                                v2 = Sse.Multiply(v2, vy2.GetLower());
                                v0 = Sse.Add(v0, v2);
                                v1 = Sse.Multiply(v1, vy1.GetLower());
                                v3 = Sse.Multiply(v3, vy3.GetLower());
                                v1 = Sse.Add(v1, v3);
                                v0 = Sse.Add(v0, v1);
                                Avx.MaskStore(rdi + i + nch, mask, v0);
                            }
                            psx += acc;
                            isx += facc;
                            var h = psx >= ram;
                            int y = Unsafe.As<bool, byte>(ref h);
                            isx += y;
                            psx -= -y & ram;
                        }
                        if (rec <= 0)
                        {
                            red = 1;
                            rec = 0;
                        }
                    }
                    while (i < length)
                    {
                        var olen = MathI.Min(length, i - rec * nchannels + rewc);
                        for (; i < olen; i += nchannels)
                        {
                            var y0 = Unsafe.Add(ref coeffPtr, rec++);
                            var vy0 = y0.AsVector128().ToVector256Unsafe();
                            var vy1 = Sse.Shuffle(vy0.GetLower(), vy0.GetLower(), 0b01_01_01_01).ToVector256Unsafe();
                            var vy2 = Sse.Shuffle(vy0.GetLower(), vy0.GetLower(), 0b10_10_10_10).ToVector256Unsafe();
                            var vy3 = Sse.Shuffle(vy0.GetLower(), vy0.GetLower(), 0b11_11_11_11).ToVector256Unsafe();
                            vy0 = Sse.Shuffle(vy0.GetLower(), vy0.GetLower(), 0b00_00_00_00).ToVector256Unsafe();
                            vy0 = vy0.WithUpper(vy0.GetLower());
                            vy1 = vy1.WithUpper(vy1.GetLower());
                            vy2 = vy2.WithUpper(vy2.GetLower());
                            vy3 = vy3.WithUpper(vy3.GetLower());
                            var head = rsi + isx * nchannels;
                            var head2 = head + nchannels * 2;
                            nint nch = 0;
                            var cholen = nchannels - 7;
                            for (; nch < cholen; nch += 8)
                            {
                                var v0 = Avx.Multiply(vy0, *(Vector256<float>*)(head + nch));
                                var v2 = Avx.Multiply(vy2, *(Vector256<float>*)(head2 + nch));
                                v0 = Avx.Add(v0, v2);
                                var v1 = Avx.Multiply(vy1, *(Vector256<float>*)(head + nchannels + nch));
                                var v3 = Avx.Multiply(vy3, *(Vector256<float>*)(head2 + nchannels + nch));
                                v1 = Avx.Add(v1, v3);
                                v0 = Avx.Add(v0, v1);
                                *(Vector256<float>*)(rdi + i + nch) = v0;
                            }
                            cholen = nchannels - 3;
                            for (; nch < cholen; nch += 4)
                            {
                                var v0 = Sse.Multiply(vy0.GetLower(), *(Vector128<float>*)(head + nch));
                                var v2 = Sse.Multiply(vy2.GetLower(), *(Vector128<float>*)(head2 + nch));
                                v0 = Sse.Add(v0, v2);
                                var v1 = Sse.Multiply(vy1.GetLower(), *(Vector128<float>*)(head + nchannels + nch));
                                var v3 = Sse.Multiply(vy3.GetLower(), *(Vector128<float>*)(head2 + nchannels + nch));
                                v1 = Sse.Add(v1, v3);
                                v0 = Sse.Add(v0, v1);
                                *(Vector128<float>*)(rdi + i + nch) = v0;
                            }
                            if (nch < nchannels)
                            {
                                var v0 = Avx.MaskLoad(head + nch, mask);
                                var v2 = Avx.MaskLoad(head2 + nch, mask);
                                var v1 = Avx.MaskLoad(head + nchannels + nch, mask);
                                var v3 = Avx.MaskLoad(head2 + nchannels + nch, mask);
                                v0 = Sse.Multiply(v0, vy0.GetLower());
                                v2 = Sse.Multiply(v2, vy2.GetLower());
                                v0 = Sse.Add(v0, v2);
                                v1 = Sse.Multiply(v1, vy1.GetLower());
                                v3 = Sse.Multiply(v3, vy3.GetLower());
                                v1 = Sse.Add(v1, v3);
                                v0 = Sse.Add(v0, v1);
                                Avx.MaskStore(rdi + i + nch, mask, v0);
                            }
                            psx += acc;
                            isx += facc;
                            var h = psx >= ram;
                            int y = Unsafe.As<bool, byte>(ref h);
                            isx += y;
                            psx -= -y & ram;
                        }
                        if (rec >= rew)
                        {
                            red = -1;
                            rec--;
                        }
                        olen = MathI.Min(length, i + rec * nchannels);
                        for (; i < olen; i += nchannels)
                        {
                            var y0 = VectorUtils.ReverseElements(Unsafe.Add(ref coeffPtr, rec--));
                            var vy0 = y0.AsVector128().ToVector256Unsafe();
                            var vy1 = Sse.Shuffle(vy0.GetLower(), vy0.GetLower(), 0b01_01_01_01).ToVector256Unsafe();
                            var vy2 = Sse.Shuffle(vy0.GetLower(), vy0.GetLower(), 0b10_10_10_10).ToVector256Unsafe();
                            var vy3 = Sse.Shuffle(vy0.GetLower(), vy0.GetLower(), 0b11_11_11_11).ToVector256Unsafe();
                            vy0 = Sse.Shuffle(vy0.GetLower(), vy0.GetLower(), 0b00_00_00_00).ToVector256Unsafe();
                            vy0 = vy0.WithUpper(vy0.GetLower());
                            vy1 = vy1.WithUpper(vy1.GetLower());
                            vy2 = vy2.WithUpper(vy2.GetLower());
                            vy3 = vy3.WithUpper(vy3.GetLower());
                            var head = rsi + isx * nchannels;
                            var head2 = head + nchannels * 2;
                            nint nch = 0;
                            var cholen = nchannels - 7;
                            for (; nch < cholen; nch += 8)
                            {
                                var v0 = Avx.Multiply(vy0, *(Vector256<float>*)(head + nch));
                                var v2 = Avx.Multiply(vy2, *(Vector256<float>*)(head2 + nch));
                                v0 = Avx.Add(v0, v2);
                                var v1 = Avx.Multiply(vy1, *(Vector256<float>*)(head + nchannels + nch));
                                var v3 = Avx.Multiply(vy3, *(Vector256<float>*)(head2 + nchannels + nch));
                                v1 = Avx.Add(v1, v3);
                                v0 = Avx.Add(v0, v1);
                                *(Vector256<float>*)(rdi + i + nch) = v0;
                            }
                            cholen = nchannels - 3;
                            for (; nch < cholen; nch += 4)
                            {
                                var v0 = Sse.Multiply(vy0.GetLower(), *(Vector128<float>*)(head + nch));
                                var v2 = Sse.Multiply(vy2.GetLower(), *(Vector128<float>*)(head2 + nch));
                                v0 = Sse.Add(v0, v2);
                                var v1 = Sse.Multiply(vy1.GetLower(), *(Vector128<float>*)(head + nchannels + nch));
                                var v3 = Sse.Multiply(vy3.GetLower(), *(Vector128<float>*)(head2 + nchannels + nch));
                                v1 = Sse.Add(v1, v3);
                                v0 = Sse.Add(v0, v1);
                                *(Vector128<float>*)(rdi + i + nch) = v0;
                            }
                            if (nch < nchannels)
                            {
                                var v0 = Avx.MaskLoad(head + nch, mask);
                                var v2 = Avx.MaskLoad(head2 + nch, mask);
                                var v1 = Avx.MaskLoad(head + nchannels + nch, mask);
                                var v3 = Avx.MaskLoad(head2 + nchannels + nch, mask);
                                v0 = Sse.Multiply(v0, vy0.GetLower());
                                v2 = Sse.Multiply(v2, vy2.GetLower());
                                v0 = Sse.Add(v0, v2);
                                v1 = Sse.Multiply(v1, vy1.GetLower());
                                v3 = Sse.Multiply(v3, vy3.GetLower());
                                v1 = Sse.Add(v1, v3);
                                v0 = Sse.Add(v0, v1);
                                Avx.MaskStore(rdi + i + nch, mask, v0);
                            }
                            psx += acc;
                            isx += facc;
                            var h = psx >= ram;
                            int y = Unsafe.As<bool, byte>(ref h);
                            isx += y;
                            psx -= -y & ram;
                        }
                        if (rec == 0)
                        {
                            red = 1;
                        }
                    }
                }
            }
            return ((int)isx, (int)psx, rec, red);
        }
        #endregion

        #region Direct
        #region Generic
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static unsafe ResampleFunc GetFuncDirectGenericX86(in UnifiedResampleArgs args) => Avx2.IsSupported ? new(&ResampleDirectGenericAvx2) : new(&ResampleDirectGenericStandard);

        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        internal static ResampleResult ResampleDirectGenericAvx2(UnifiedResampleArgs args, Span<float> buffer, Span<float> srcBuffer, Span<Vector4> cspan)
        {
            (var channels, var x, var ram, var acc, var facc, var rmi) = (args.Channels, args.ConversionGradient, args.RateMul, args.GradientIncrement, args.IndexIncrement, args.RateMulInverse);
            nint i = 0;
            nint isx = 0;
            nint psx = x;
            var spsx = psx;
            nint nram = ram;
            nint nchannels = channels;
            var length = buffer.Length - nchannels + 1;
            Vector4 c0 = C0, c1 = C1, c2 = C2, c3 = C3;
            Vector4 x1 = default, x2 = default, x3 = default;
            Vector4 y0 = default, y1 = default, y2 = default;
            var rch = channels & 0x3;
            var mask = Sse2.CompareGreaterThan(Vector128.Create(rch, rch - 1, rch - 2, rch - 3), Vector128<int>.Zero).AsSingle();
            for (var j = 0; j < 4; j++)
            {
                var nx3 = new Vector4(spsx * rmi);
                spsx += acc;
                var h2 = spsx >= nram;
                int g2 = Unsafe.As<bool, byte>(ref h2);
                y0 = y1 * x1;
                y0 += c3;
                y1 = y2 * x2;
                y1 += c2;
                y2 = c0 * x3;
                y2 += c1;
                spsx -= -g2 & nram;
                x1 = x2;
                x2 = x3;
                x3 = nx3;
            }
            unsafe
            {
                fixed (float* rsi = srcBuffer)
                fixed (float* rdi = buffer)
                {
                    for (; i < length; i += nchannels)
                    {
                        psx += acc;
                        var head = rsi + isx * nchannels;
                        var head2 = head + nchannels * 2;
                        nint nch = 0;
                        var cholen = nchannels - 7;
                        for (; nch < cholen; nch += 8)
                        {
                            var vy0 = y0.AsVector128();
                            var v0 = Avx2.BroadcastScalarToVector256(Sse.Shuffle(vy0, vy0, 0b00_00_00_00));
                            var v2 = Avx2.BroadcastScalarToVector256(Sse.Shuffle(vy0, vy0, 0b10_10_10_10));
                            var v1 = Avx2.BroadcastScalarToVector256(Sse.Shuffle(vy0, vy0, 0b01_01_01_01));
                            var v3 = Avx2.BroadcastScalarToVector256(Sse.Shuffle(vy0, vy0, 0b11_11_11_11));
                            v0 = Avx.Multiply(v0, *(Vector256<float>*)(head + nch));
                            v2 = Avx.Multiply(v2, *(Vector256<float>*)(head2 + nch));
                            v0 = Avx.Add(v0, v2);
                            v1 = Avx.Multiply(v1, *(Vector256<float>*)(head + nchannels + nch));
                            v3 = Avx.Multiply(v3, *(Vector256<float>*)(head2 + nchannels + nch));
                            v1 = Avx.Add(v1, v3);
                            v0 = Avx.Add(v0, v1);
                            *(Vector256<float>*)(rdi + i + nch) = v0;
                        }
                        cholen = nchannels - 3;
                        for (; nch < cholen; nch += 4)
                        {
                            var vy0 = y0.AsVector128();
                            var v0 = Sse.Shuffle(vy0, vy0, 0b00_00_00_00);
                            var v2 = Sse.Shuffle(vy0, vy0, 0b10_10_10_10);
                            var v1 = Sse.Shuffle(vy0, vy0, 0b01_01_01_01);
                            var v3 = Sse.Shuffle(vy0, vy0, 0b11_11_11_11);
                            v0 = Sse.Multiply(v0, *(Vector128<float>*)(head + nch));
                            v2 = Sse.Multiply(v2, *(Vector128<float>*)(head2 + nch));
                            v0 = Sse.Add(v0, v2);
                            v1 = Sse.Multiply(v1, *(Vector128<float>*)(head + nchannels + nch));
                            v3 = Sse.Multiply(v3, *(Vector128<float>*)(head2 + nchannels + nch));
                            v1 = Sse.Add(v1, v3);
                            v0 = Sse.Add(v0, v1);
                            *(Vector128<float>*)(rdi + i + nch) = v0;
                        }
                        if (nch < nchannels)
                        {
                            var vy0 = y0.AsVector128();
                            var v0 = Avx.MaskLoad(head + nch, mask);
                            var v2 = Avx.MaskLoad(head2 + nch, mask);
                            v0 = Sse.Multiply(v0, Sse.Shuffle(vy0, vy0, 0b00_00_00_00));
                            v2 = Sse.Multiply(v2, Sse.Shuffle(vy0, vy0, 0b10_10_10_10));
                            v0 = Sse.Add(v0, v2);
                            var v1 = Avx.MaskLoad(head + nchannels + nch, mask);
                            var v3 = Avx.MaskLoad(head2 + nchannels + nch, mask);
                            v1 = Sse.Multiply(v1, Sse.Shuffle(vy0, vy0, 0b01_01_01_01));
                            v3 = Sse.Multiply(v3, Sse.Shuffle(vy0, vy0, 0b11_11_11_11));
                            v1 = Sse.Add(v1, v3);
                            v0 = Sse.Add(v0, v1);
                            Avx.MaskStore(rdi + i + nch, mask, v0);
                        }
                        var h = psx >= nram;
                        int g = Unsafe.As<bool, byte>(ref h);
                        isx += g;
                        isx += facc;
                        psx -= -g & nram;
                        var nx3 = new Vector4(spsx * rmi);
                        spsx += acc;
                        var h2 = spsx >= nram;
                        int g2 = Unsafe.As<bool, byte>(ref h2);
                        y0 = y1 * x1;
                        y0 += c3;
                        y1 = y2 * x2;
                        y1 += c2;
                        y2 = c0 * x3;
                        y2 += c1;
                        spsx -= -g2 & nram;
                        x1 = x2;
                        x2 = x3;
                        x3 = nx3;
                    }
                }
            }
            return new((int)isx, (int)psx);
        }
        #endregion
        #endregion
        #endregion
        #endregion
    }
}

#endif
