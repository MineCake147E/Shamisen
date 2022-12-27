using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using Shamisen.Utils;

namespace Shamisen.Conversion.Resampling.Sample
{
    public sealed partial class SplineResampler
    {
        #region Resample
        #region CachedDirect

        #region Monaural
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private ResampleFunc GetFuncCachedDirectMonaural(in UnifiedResampleArgs args) =>
            GetFuncCachedDirectMonauralX86(args);

        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        internal static ResampleResult ResampleCachedDirectVectorFitChannelsStandard(UnifiedResampleArgs args, Span<float> buffer, Span<float> srcBuffer, Span<Vector4> cspan)
        {
            nint i = 0;
            nint isx = 0;
            nint psx = args.ConversionGradient;
            nint nrci = args.RearrangedCoeffsIndex;
            nint ram = args.RateMul;
            nint acc = args.GradientIncrement;
            nint facc = args.IndexIncrement;
            ref var src = ref MemoryMarshal.GetReference(srcBuffer);
            ref var dst = ref MemoryMarshal.GetReference(buffer);
            ref var coeffPtr = ref MemoryMarshal.GetReference(cspan);
            nint length = buffer.Length - Vector<float>.Count + 1;
            var vfacc = facc * Vector<float>.Count;
            for (; i < length; i += Vector<float>.Count)
            {
                psx += acc;
                var cutmullCoeffs = Unsafe.Add(ref coeffPtr, nrci);
                var ymm0 = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref src, isx));
                var ymm4 = new Vector<float>(cutmullCoeffs.X);
                var ymm1 = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref src, isx + Vector<float>.Count));
                var ymm5 = new Vector<float>(cutmullCoeffs.Y);
                var ymm2 = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref src, isx + 2 * Vector<float>.Count));
                var ymm6 = new Vector<float>(cutmullCoeffs.Z);
                var ymm3 = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref src, isx + 3 * Vector<float>.Count));
                var ymm7 = new Vector<float>(cutmullCoeffs.W);
                isx += vfacc;
                var j = ++nrci < ram;
                nint z = Unsafe.As<bool, byte>(ref j);
                var value1 = ymm0 * ymm4;
                nrci &= -z;
                var value2 = ymm1 * ymm5;
                var h = psx >= ram;
                int y = Unsafe.As<bool, byte>(ref h);
                value1 += ymm2 * ymm6;
                value2 += ymm3 * ymm7;
                isx += y * Vector<float>.Count;
                value1 += value2;
                psx -= -y & ram;
                Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref dst, i)) = value1;
            }
            return new((int)(isx / Vector<float>.Count), (int)psx, (int)nrci);
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static unsafe ResampleFunc GetFuncCachedDirectMonauralStandard(in UnifiedResampleArgs args)
            => new((args.IndexIncrement, args.GradientIncrement, args.RateMul) switch
            {
                (0, 1, 2) => &ResampleCachedDirectMonauralDoubleRateStandard,
                (0, 1, 4) => &ResampleCachedDirectMonauralQuadrupleRateStandard,
                (0, 1, _) => &ResampleCachedDirectMonauralIntegerMultipleRateStandard,
                (0, _, _) => &ResampleCachedDirectMonauralUpAnyRateStandard,
                _ => &ResampleCachedDirectMonauralAnyRateStandard
            });
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        internal static ResampleResult ResampleCachedDirectMonauralAnyRateStandard(UnifiedResampleArgs args, Span<float> buffer, Span<float> srcBuffer, Span<Vector4> cspan)
        {
            nint i = 0;
            nint length = buffer.Length;
            nint isx = 0;
            nint psx = args.ConversionGradient;
            nint nrci = args.RearrangedCoeffsIndex;
            nint nram = args.RateMul;
            nint acc = args.GradientIncrement;
            nint facc = args.IndexIncrement;
            ref var coeffPtr = ref MemoryMarshal.GetReference(cspan);
            ref var src = ref MemoryMarshal.GetReference(srcBuffer);
            ref var dst = ref MemoryMarshal.GetReference(buffer);
            Vector4 values;
            for (; i < length; i++)
            {
                psx += acc;
                var cutmullCoeffs = Unsafe.Add(ref coeffPtr, nrci);
                var h = psx >= nram;
                nint y = Unsafe.As<bool, byte>(ref h);
                var j = ++nrci < nram;
                nint z = Unsafe.As<bool, byte>(ref j);
                values = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref src, isx));
                nrci &= -z;
                Unsafe.Add(ref dst, i) = VectorUtils.FastDotProduct(values, cutmullCoeffs);
                isx += facc;
                isx += y;
                psx -= -y & nram;
            }
            return new((int)isx, (int)psx, (int)nrci);
        }

        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        internal static ResampleResult ResampleCachedDirectMonauralUpAnyRateStandard(UnifiedResampleArgs args, Span<float> buffer, Span<float> srcBuffer, Span<Vector4> cspan)
        {
            nint i = 0;
            nint length = buffer.Length;
            nint isx = 0;
            nint psx = args.ConversionGradient;
            nint nrci = args.RearrangedCoeffsIndex;
            nint nram = args.RateMul;
            nint acc = args.GradientIncrement;
            _ = args.IndexIncrement;
            ref var coeffPtr = ref MemoryMarshal.GetReference(cspan);
            ref var src = ref MemoryMarshal.GetReference(srcBuffer);
            ref var dst = ref MemoryMarshal.GetReference(buffer);
            Vector4 values;
            for (; i < length; i++)
            {
                psx += acc;
                var cutmullCoeffs = Unsafe.Add(ref coeffPtr, nrci);
                var h = psx >= nram;
                nint y = Unsafe.As<bool, byte>(ref h);
                var j = ++nrci < nram;
                nint z = Unsafe.As<bool, byte>(ref j);
                values = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref src, isx));
                nrci &= -z;
                Unsafe.Add(ref dst, i) = VectorUtils.FastDotProduct(values, cutmullCoeffs);
                isx += y;
                psx -= -y & nram;
            }
            return new((int)isx, (int)psx, (int)nrci);
        }

        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        internal static ResampleResult ResampleCachedDirectMonauralIntegerMultipleRateStandard(UnifiedResampleArgs args, Span<float> buffer, Span<float> srcBuffer, Span<Vector4> cspan)
        {
            nint length = buffer.Length;
            nint isx = 0;
            nint psx = args.ConversionGradient;
            nint nram = args.RateMul;
            ref var coeffPtr = ref MemoryMarshal.GetReference(cspan);
            ref var src = ref MemoryMarshal.GetReference(srcBuffer);
            ref var dst = ref MemoryMarshal.GetReference(buffer);
            Vector4 values;
            nint i;
            for (i = 0; i < length; i++)
            {
                values = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref src, isx));
                var cutmullCoeffs = Unsafe.Add(ref coeffPtr, psx++);
                var h = psx >= nram;
                int y = Unsafe.As<bool, byte>(ref h);
                Unsafe.Add(ref dst, i) = VectorUtils.FastDotProduct(values, cutmullCoeffs);
                isx += y;
                psx -= -y & nram;
            }
            return new((int)isx, (int)psx);
        }

        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        internal static ResampleResult ResampleCachedDirectMonauralDoubleRateStandard(UnifiedResampleArgs args, Span<float> buffer, Span<float> srcBuffer, Span<Vector4> cspan)
        {
            nint i = 0;
            nint length = buffer.Length;
            nint isx = 0;
            nint psx = args.ConversionGradient;
            ref var coeffPtr = ref MemoryMarshal.GetReference(cspan);
            ref var src = ref MemoryMarshal.GetReference(srcBuffer);
            ref var dst = ref MemoryMarshal.GetReference(buffer);
            var c0 = Unsafe.Add(ref coeffPtr, 0);
            var c1 = Unsafe.Add(ref coeffPtr, 1);
            Vector4 values, values2;
            if (psx > 0)
            {
                values = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref src, isx));
                var cutmullCoeffs = c1;
                Unsafe.Add(ref dst, i++) = VectorUtils.FastDotProduct(values, cutmullCoeffs);
                psx = 0;
                isx++;
            }
            for (; i < length - 3; i += 4)
            {
                values = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref src, isx));
                values2 = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref src, isx + 1));
                Unsafe.Add(ref dst, i) = VectorUtils.FastDotProduct(values, c0);
                Unsafe.Add(ref dst, i + 1) = VectorUtils.FastDotProduct(values, c1);
                Unsafe.Add(ref dst, i + 2) = VectorUtils.FastDotProduct(values2, c0);
                Unsafe.Add(ref dst, i + 3) = VectorUtils.FastDotProduct(values2, c1);
                isx += 2;
            }
            for (; i < length - 1; i += 2)
            {
                values = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref src, isx));
                Unsafe.Add(ref dst, i) = VectorUtils.FastDotProduct(values, c0);
                Unsafe.Add(ref dst, i + 1) = VectorUtils.FastDotProduct(values, c1);
                isx++;
            }
            for (; i < length; i++)
            {
                values = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref src, isx));
                var cutmullCoeffs = Unsafe.Add(ref coeffPtr, psx);
                Unsafe.Add(ref dst, i) = VectorUtils.FastDotProduct(values, cutmullCoeffs);
                psx++;
                if (psx >= 2)
                {
                    psx -= 2;
                    isx++;
                }
            }
            return new((int)isx, (int)psx);
        }
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        internal static ResampleResult ResampleCachedDirectMonauralQuadrupleRateStandard(UnifiedResampleArgs args, Span<float> buffer, Span<float> srcBuffer, Span<Vector4> cspan)
        {
            nint length = buffer.Length;
            nint isx = 0;
            nint psx = args.ConversionGradient;
            ref var coeffPtr = ref MemoryMarshal.GetReference(cspan);
            ref var src = ref MemoryMarshal.GetReference(srcBuffer);
            ref var dst = ref MemoryMarshal.GetReference(buffer);
            Vector4 values;
            var c0 = coeffPtr;
            var c1 = Unsafe.Add(ref coeffPtr, 1);
            var c2 = Unsafe.Add(ref coeffPtr, 2);
            var c3 = Unsafe.Add(ref coeffPtr, 3);
            nint i;
            for (i = 0; psx is not 0 and < 4; i++, psx++)
            {
                values = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref src, isx));
                var cutmullCoeffs = Unsafe.Add(ref coeffPtr, psx);
                Unsafe.Add(ref dst, i) = VectorUtils.FastDotProduct(values, cutmullCoeffs);
                psx++;
                if (psx >= 4)
                {
                    psx = 0;
                    isx++;
                    break;
                }
            }
            for (; i < length - 3; i += 4)
            {
                values = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref src, isx));
                Unsafe.Add(ref dst, i) = VectorUtils.FastDotProduct(values, c0);
                Unsafe.Add(ref dst, i + 1) = VectorUtils.FastDotProduct(values, c1);
                Unsafe.Add(ref dst, i + 2) = VectorUtils.FastDotProduct(values, c2);
                Unsafe.Add(ref dst, i + 3) = VectorUtils.FastDotProduct(values, c3);
                isx++;
            }
            for (; i < length; i++)
            {
                values = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref src, isx));
                var cutmullCoeffs = Unsafe.Add(ref coeffPtr, psx);
                Unsafe.Add(ref dst, i) = VectorUtils.FastDotProduct(values, cutmullCoeffs);
                psx++;
                if (psx >= 4)
                {
                    psx -= 4;
                    isx++;
                }
            }
            return new((int)isx, (int)psx);
        }

        #endregion
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private ResampleFunc GetFuncCachedDirect2Channels(in UnifiedResampleArgs args)
            => GetFuncCachedDirect2ChannelsX86(args);

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private unsafe ResampleFunc GetFuncCachedDirect3Channels(in UnifiedResampleArgs args)
            => new(&ResampleCachedDirect3ChannelsStandard);
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private unsafe ResampleFunc GetFuncCachedDirect4Channels(in UnifiedResampleArgs args)
            => new(&ResampleCachedDirect4ChannelsStandard);

        #region Generic

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static ResampleFunc GetFuncCachedDirectGeneric(in UnifiedResampleArgs args)
        {
            unchecked
            {
#if NETCOREAPP3_1_OR_GREATER
                return GetFuncCachedDirectGenericX86(args);
#endif
            }
        }
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        internal static ResampleResult ResampleCachedDirectGenericStandard(UnifiedResampleArgs args, Span<float> buffer, Span<float> srcBuffer, Span<Vector4> cspan)
        {
            nint i = 0;
            nint isx = 0;
            nint psx = args.ConversionGradient;
            nint nrci = args.RearrangedCoeffsIndex;
            nint nram = args.RateMul;
            nint acc = args.GradientIncrement;
            nint facc = args.IndexIncrement;
            nint nchannels = args.Channels;
            ref var coeffPtr = ref MemoryMarshal.GetReference(cspan);
            var length = buffer.Length - nchannels + 1;
            var rch = nchannels & 0x3;
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
                        var cholen = nchannels - 3;
                        for (; nch < cholen; nch += 4)
                        {
                            var vy0 = y0;
                            var v0 = new Vector4(vy0.X);
                            var v2 = new Vector4(vy0.Z);
                            var v1 = new Vector4(vy0.Y);
                            var v3 = new Vector4(vy0.W);
                            v0 *= *(Vector4*)(head + nch);
                            v2 *= *(Vector4*)(head2 + nch);
                            v0 += v2;
                            v1 *= *(Vector4*)(head + nchannels + nch);
                            v3 *= *(Vector4*)(head2 + nchannels + nch);
                            v1 += v3;
                            v0 += v1;
                            *(Vector4*)(rdi + i + nch) = v0;
                        }
                        for (; nch < nchannels; nch++)
                        {
                            var values = new Vector4(*(head + nch), *(head + nchannels + nch), *(head2 + nch), *(head2 + nchannels + nch));
                            *(rdi + i + nch) = VectorUtils.FastDotProduct(values, y0);
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

        #region Direct
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        internal static ResampleResult ResampleDirectMonauralStandard(UnifiedResampleArgs args, Span<float> buffer, Span<float> srcBuffer, Span<Vector4> cspan)
        {
            nint i = 0;
            nint length = buffer.Length;
            nint isx = 0;
            nint psx = args.ConversionGradient;
            nint nram = args.RateMul;
            nint acc = args.GradientIncrement;
            nint facc = args.IndexIncrement;
            var rmi = args.RateMulInverse;
            var spsx = psx;
            ref var src = ref MemoryMarshal.GetReference(srcBuffer);
            ref var dst = ref MemoryMarshal.GetReference(buffer);
            Vector4 c0 = C0, c1 = C1, c2 = C2, c3 = C3;
            Vector4 x1 = default, x2 = default, x3 = default;
            Vector4 y0 = default, y1 = default, y2 = default;
            for (var j = 0; j < 4; j++)
            {
                var nx3 = new Vector4(spsx * rmi);
                spsx += acc;
                var h2 = spsx >= nram;
                nint g2 = Unsafe.As<bool, byte>(ref h2);
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
            for (; i < length; i++)
            {
                psx += acc;
                var h = psx >= nram;
                nint g = Unsafe.As<bool, byte>(ref h);
                var values = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref src, isx));
                isx += g;
                isx += facc;
                Unsafe.Add(ref dst, i) = VectorUtils.FastDotProduct(values, y0);
                var nx3 = new Vector4(spsx * rmi);
                spsx += acc;
                var h2 = spsx >= nram;
                nint g2 = Unsafe.As<bool, byte>(ref h2);
                y0 = y1 * x1;
                y0 += c3;
                y1 = y2 * x2;
                y1 += c2;
                y2 = c0 * x3;
                y2 += c1;
                psx -= -g & nram;
                spsx -= -g2 & nram;
                x1 = x2;
                x2 = x3;
                x3 = nx3;
            }
            return new((int)isx, (int)psx);
        }

        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        internal static ResampleResult ResampleDirectVectorFitStandard(UnifiedResampleArgs args, Span<float> buffer, Span<float> srcBuffer, Span<Vector4> cspan)
        {
            nint i = 0;
            nint isx = 0;
            nint psx = args.ConversionGradient;
            nint nram = args.RateMul;
            nint acc = args.GradientIncrement;
            nint facc = args.IndexIncrement;
            var rmi = args.RateMulInverse;
            var spsx = psx;
            var vBuffer = MemoryMarshal.Cast<float, Vector<float>>(buffer);
            var vSrcBuffer = MemoryMarshal.Cast<float, Vector<float>>(srcBuffer);
            nint length = vBuffer.Length;
            ref var src = ref MemoryMarshal.GetReference(vSrcBuffer);
            ref var dst = ref MemoryMarshal.GetReference(vBuffer);
            Vector4 c0 = C0, c1 = C1, c2 = C2, c3 = C3;
            Vector4 x1 = default, x2 = default, x3 = default;
            Vector4 y0 = default, y1 = default, y2 = default;
            for (var j = 0; j < 4; j++)
            {
                var nx3 = new Vector4(spsx * rmi);
                spsx += acc;
                var h2 = spsx >= nram;
                nint g2 = Unsafe.As<bool, byte>(ref h2);
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
            for (; i < length; i++)
            {
                psx += acc;
                var h = psx >= nram;
                nint g = Unsafe.As<bool, byte>(ref h);
                ref var values = ref Unsafe.As<Vector<float>, (Vector<float> X, Vector<float> Y, Vector<float> Z, Vector<float> W)>(ref Unsafe.Add(ref src, isx));
                isx += g;
                isx += facc;
                var value1 = new Vector<float>(y0.X);
                var value2 = new Vector<float>(y0.Y);
                var value3 = new Vector<float>(y0.Z);
                var value4 = new Vector<float>(y0.W);
                value1 *= values.X;
                value2 *= values.Y;
                value3 *= values.Z;
                value4 *= values.W;
                psx -= -g & nram;
                Unsafe.Add(ref dst, i) = value1 + value3 + (value2 + value4);
                var nx3 = new Vector4(spsx * rmi);
                spsx += acc;
                var h2 = spsx >= nram;
                nint g2 = Unsafe.As<bool, byte>(ref h2);
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
            return new((int)isx, (int)psx);
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static ResampleFunc GetFuncDirectGeneric(in UnifiedResampleArgs args)
        {
            unchecked
            {
#if NETCOREAPP3_1_OR_GREATER
                return GetFuncDirectGenericX86(args);
#endif
            }
        }

        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        internal static ResampleResult ResampleDirectGenericStandard(UnifiedResampleArgs args, Span<float> buffer, Span<float> srcBuffer, Span<Vector4> cspan)
        {
            (var channels, var x, var ram, var acc, var facc, var rmi) = (args.Channels, args.ConversionGradient, args.RateMul, args.GradientIncrement, args.IndexIncrement, args.RateMulInverse);
            nint i = 0;
            nint isx = 0;
            var psx = x;
            var spsx = psx;
            var nram = ram;
            nint nchannels = channels;
            nint length = buffer.Length - channels + 1;
            ref var src = ref MemoryMarshal.GetReference(srcBuffer);
            ref var dst = ref MemoryMarshal.GetReference(buffer);
            Vector4 c0 = C0, c1 = C1, c2 = C2, c3 = C3;
            Vector4 x1 = default, x2 = default, x3 = default;
            Vector4 y0 = default, y1 = default, y2 = default;
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
                    for (; i < length; i += channels)
                    {
                        psx += acc;
                        var head = rsi + isx * nchannels;
                        var head2 = head + nchannels * 2;
                        nint nch = 0;
                        var cholen = nchannels - 3;
                        for (; nch < cholen; nch += 4)
                        {
                            var vy0 = y0;
                            var v0 = new Vector4(vy0.X);
                            var v2 = new Vector4(vy0.Z);
                            var v1 = new Vector4(vy0.Y);
                            var v3 = new Vector4(vy0.W);
                            v0 *= *(Vector4*)(head + nch);
                            v2 *= *(Vector4*)(head2 + nch);
                            v0 += v2;
                            v1 *= *(Vector4*)(head + nchannels + nch);
                            v3 *= *(Vector4*)(head2 + nchannels + nch);
                            v1 += v3;
                            v0 += v1;
                            *(Vector4*)(rdi + i + nch) = v0;
                        }
                        for (; nch < nchannels; nch++)
                        {
                            var values = new Vector4(*(head + nch), *(head + nchannels + nch), *(head2 + nch), *(head2 + nchannels + nch));
                            *(rdi + i + nch) = VectorUtils.FastDotProduct(values, y0);
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
            return new((int)isx, psx);
        }

        #endregion
        #endregion

    }
}
