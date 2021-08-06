#if NETCOREAPP3_1_OR_GREATER

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using Shamisen.Optimization;
using Shamisen.Utils;

namespace Shamisen.Conversion.Resampling.Sample
{
    public partial class SplineResampler
    {
        #region Monaural
        private int ResampleCachedDirectMonauralX86(Span<float> buffer, Span<float> srcBuffer, ref Vector4 coeffPtr, ref int x, int ram, int acc, int facc)
        {
            switch ((facc, acc, ram))
            {
                case (0, 1, 2) when Sse41.IsSupported && X86Intrinsics.HasAllFeatures(X86IntrinsicsMask.Sse41):
                    return ResampleCachedDirectMonauralDoubleRateSse41(buffer, srcBuffer, ref coeffPtr, ref x);
                case (0, 1, 4) when Sse41.IsSupported && X86Intrinsics.HasAllFeatures(X86IntrinsicsMask.Sse41):
                    return ResampleCachedDirectMonauralQuadrupleRateSse41(buffer, srcBuffer, ref coeffPtr, ref x);
                case (0, 1, _) when Sse.IsSupported && X86Intrinsics.HasAllFeatures(X86IntrinsicsMask.Sse):
                    return ResampleCachedDirectMonauralIntegerMultipleRateX86(buffer, srcBuffer, ref coeffPtr, ref x, ram);
                case (0, _, _) when Sse.IsSupported && X86Intrinsics.HasAllFeatures(X86IntrinsicsMask.Sse):
                    return ResampleCachedDirectMonauralUpAnyRateX86(buffer, srcBuffer, ref coeffPtr, ref x, ram, acc);
                case (_, _, _) when Sse.IsSupported && X86Intrinsics.HasAllFeatures(X86IntrinsicsMask.Sse):
                    return ResampleCachedDirectMonauralAnyRateSse3(buffer, srcBuffer, ref coeffPtr, ref x, ram, acc, facc);
                default:
                    return ResampleCachedDirectMonauralStandard(buffer, srcBuffer, ref coeffPtr, ref x, ram, acc, facc);
            }
        }

        /// <summary>
        /// For arbitral sampling frequency ratio larger than 1
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="srcBuffer"></param>
        /// <param name="coeffPtr"></param>
        /// <param name="x"></param>
        /// <param name="ram"></param>
        /// <param name="acc"></param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static int ResampleCachedDirectMonauralUpAnyRateX86(Span<float> buffer, Span<float> srcBuffer, ref Vector4 coeffPtr, ref int x, int ram, int acc)
        {
            nint i = 0;
            nint length = buffer.Length;
            int isx = 0;
            int psx = x;
            ref var coeff = ref Unsafe.As<Vector4, Vector128<float>>(ref coeffPtr);
            ref var src = ref MemoryMarshal.GetReference(srcBuffer);
            ref var dst = ref MemoryMarshal.GetReference(buffer);
            var xmm7 = Unsafe.As<float, Vector128<float>>(ref src);
            for (i = 0; i < length; i++)
            {
                var cutmullCoeffs = Unsafe.Add(ref coeff, psx);
                Unsafe.Add(ref dst, i) = VectorUtils.FastDotProduct(xmm7, cutmullCoeffs);
                psx += acc;
                if (psx >= ram)
                {
                    psx -= ram;
                    isx++;
                    xmm7 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref src, isx));
                }
            }
            x = psx;
            return isx;
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static int ResampleCachedDirectMonauralIntegerMultipleRateX86(Span<float> buffer, Span<float> srcBuffer, ref Vector4 coeffPtr, ref int x, int ram)
        {
            nint i = 0;
            nint length = buffer.Length;
            int isx = 0;
            int psx = x;
            ref var coeff = ref Unsafe.As<Vector4, Vector128<float>>(ref coeffPtr);
            ref var src = ref MemoryMarshal.GetReference(srcBuffer);
            ref var dst = ref MemoryMarshal.GetReference(buffer);
            var xmm7 = Unsafe.As<float, Vector128<float>>(ref src);
            for (i = 0; i < length; i++)
            {
                var cutmullCoeffs = Unsafe.Add(ref coeff, psx);
                Unsafe.Add(ref dst, i) = VectorUtils.FastDotProduct(xmm7, cutmullCoeffs);
                psx++;
                if (psx >= ram)
                {
                    psx = 0;
                    isx++;
                    xmm7 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref src, isx));
                }
            }
            x = psx;
            return isx;
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static int ResampleCachedDirectMonauralAnyRateSse3(Span<float> buffer, Span<float> srcBuffer, ref Vector4 coeffPtr, ref int x, int ram, int acc, int facc)
        {
            nint i = 0;
            nint length = buffer.Length;
            int isx = 0;
            int psx = x;
            ref var coeff = ref Unsafe.As<Vector4, Vector128<float>>(ref coeffPtr);
            ref var src = ref MemoryMarshal.GetReference(srcBuffer);
            ref var dst = ref MemoryMarshal.GetReference(buffer);
            var xmm7 = Unsafe.As<float, Vector128<float>>(ref src);
            for (i = 0; i < length; i++)
            {
                var xmm0 = Unsafe.Add(ref coeff, psx);
                xmm0 = Sse.Multiply(xmm0, xmm7);
                psx += acc;
                xmm7 = Sse2.UnpackHigh(xmm0.AsDouble(), xmm0.AsDouble()).AsSingle();
                isx += facc;
                xmm0 = Sse.Add(xmm0, xmm7);
                bool v = psx >= ram;
                xmm7 = Sse3.MoveHighAndDuplicate(xmm0);
                byte v1 = Unsafe.As<bool, byte>(ref v);
                isx += v1;
                xmm0 = Sse.AddScalar(xmm0, xmm7);
                xmm7 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref src, isx));
                Unsafe.Add(ref dst, i) = xmm0.GetElement(0);
                psx -= ram & -v1;
            }
            x = psx;
            return isx;
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static int ResampleCachedDirectMonauralDoubleRateSse41(Span<float> buffer, Span<float> srcBuffer, ref Vector4 coeffPtr, ref int x)
        {
            nint i;
            nint length = buffer.Length;
            int isx = 0;
            int psx = x & 1;
            ref var coeff = ref Unsafe.As<Vector4, Vector128<float>>(ref coeffPtr);
            ref var src = ref MemoryMarshal.GetReference(srcBuffer);
            ref var dst = ref MemoryMarshal.GetReference(buffer);
            var xmm7 = Unsafe.As<float, Vector128<float>>(ref src);
            var xmm0 = Unsafe.Add(ref coeff, psx);
            var xmm1 = Unsafe.Add(ref coeff, psx ^ 1);
            for (i = 0; i < length - 1; i += 2)
            {
                var xmm2 = Sse.Multiply(xmm0, xmm7);
                var xmm3 = Sse.Multiply(xmm1, xmm7);
                var xmm4 = Sse2.UnpackHigh(xmm2.AsDouble(), xmm3.AsDouble()).AsSingle();
                var xmm5 = Sse2.UnpackLow(xmm2.AsDouble(), xmm3.AsDouble()).AsSingle();
                xmm3 = Sse.Add(xmm4, xmm5);
                xmm4 = Sse.Shuffle(xmm3, xmm3, 0b11_11_11_01);
                xmm5 = Sse.Shuffle(xmm3, xmm3, 0b11_11_10_00);
                xmm2 = Sse.Add(xmm4, xmm5);
                Unsafe.As<float, double>(ref Unsafe.Add(ref dst, i)) = xmm2.AsDouble().GetElement(0);
                isx++;
                xmm7 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref src, isx));
            }
            if (i < length)
            {
                var xmm2 = Sse.Multiply(xmm0, xmm7);
                var xmm4 = Sse2.UnpackHigh(xmm2.AsDouble(), xmm2.AsDouble()).AsSingle();
                xmm2 = Sse.Add(xmm4, xmm2);
                xmm4 = Sse.Shuffle(xmm2, xmm2, 0b01_01_01_01);
                xmm2 = Sse.AddScalar(xmm2, xmm4);
                Unsafe.Add(ref dst, i) = xmm2.GetElement(0);
                psx++;
                isx += psx >> 1;
                psx &= 1;
            }
            x = psx;
            return isx;
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static int ResampleCachedDirectMonauralQuadrupleRateSse41(Span<float> buffer, Span<float> srcBuffer, ref Vector4 coeffPtr, ref int x)
        {
            nint i;
            nint length = buffer.Length;
            int isx = 0;
            int psx = x & 3;
            ref var coeff = ref Unsafe.As<Vector4, Vector128<float>>(ref coeffPtr);
            ref var src = ref MemoryMarshal.GetReference(srcBuffer);
            ref var dst = ref MemoryMarshal.GetReference(buffer);
            var xmm7 = Unsafe.As<float, Vector128<float>>(ref src);
            var c0 = Unsafe.Add(ref coeff, psx);
            var c1 = Unsafe.Add(ref coeff, (psx + 1) & 3);
            var c2 = Unsafe.Add(ref coeff, (psx + 2) & 3);
            var c3 = Unsafe.Add(ref coeff, (psx + 3) & 3);
            for (i = 0; i < length - 3; i += 4)
            {
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
                xmm7 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref src, isx));
            }
            for (; i < length; i++)
            {
                var cutmullCoeffs = Unsafe.Add(ref coeff, psx);
                Unsafe.Add(ref dst, i) = VectorUtils.FastDotProduct(xmm7, cutmullCoeffs);
                psx++;
                if (psx >= 4)
                {
                    psx = 0;
                    isx++;
                    xmm7 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref src, isx));
                }
            }
            x = psx;
            return isx;
        }
        #endregion
        #region Stereo
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private int ResampleCachedDirect2ChannelsX86(Span<float> buffer, Span<float> srcBuffer, ref Vector4 coeffPtr, ref int x, int ram, int acc, int facc)
        {
            if (Sse.IsSupported && X86Intrinsics.HasAllFeatures(X86IntrinsicsMask.Sse))
            {
                return ResampleCachedDirect2ChannelsSse(buffer, srcBuffer, ref coeffPtr, ref x, ram, acc, facc);
            }
            return ResampleCachedDirect2ChannelsStandard(buffer, srcBuffer, ref coeffPtr, ref x, ram, acc, facc);
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static int ResampleCachedDirect2ChannelsSse(Span<float> buffer, Span<float> srcBuffer, ref Vector4 coeffPtr, ref int x, nint ram, nint acc, nint facc)
        {
            nint isx = 0;
            nint psx = x;
            ref var vBuffer = ref Unsafe.As<float, double>(ref MemoryMarshal.GetReference(buffer));
            ref var vSrcBuffer = ref Unsafe.As<float, double>(ref MemoryMarshal.GetReference(srcBuffer));
            ref var vCoeff = ref Unsafe.As<Vector4, Vector128<float>>(ref coeffPtr);
            var vzero = Vector128.Create(0f);
            nint i;
            nint length = buffer.Length / 2;
            if (facc > 0)
            {
                for (i = 0; i < length; i++)
                {
                    var vx = Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref vSrcBuffer, isx));
                    var vz = Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref vSrcBuffer, isx + 2));
                    var coeff = Unsafe.Add(ref vCoeff, psx);
                    var vl = Sse.Shuffle(coeff, coeff, 0b01_01_00_00);
                    var vr = Sse.Shuffle(coeff, coeff, 0b11_11_10_10);
                    vl = Sse.Multiply(vx, vl);
                    vr = Sse.Multiply(vz, vr);
                    vl = Sse.Add(vl, vr);
                    vr = Sse.Shuffle(vl, vzero, 0b00_00_11_10);
                    vl = Sse.Add(vl, vr);
                    Unsafe.Add(ref vBuffer, i) = vl.AsDouble().GetElement(0);
                    psx += acc;
                    isx += facc;
                    if (psx >= ram)
                    {
                        psx -= ram;
                        isx++;
                    }
                }
            }
            else
            {
                var vx = Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref vSrcBuffer, isx));
                var vz = Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref vSrcBuffer, isx + 2));
                for (i = 0; i < length; i++)
                {
                    var coeff = Unsafe.Add(ref vCoeff, psx);
                    var vl = Sse.Shuffle(coeff, coeff, 0b01_01_00_00);
                    var vr = Sse.Shuffle(coeff, coeff, 0b11_11_10_10);
                    vl = Sse.Multiply(vx, vl);
                    vr = Sse.Multiply(vz, vr);
                    vl = Sse.Add(vl, vr);
                    vr = Sse.Shuffle(vl, vzero, 0b00_00_11_10);
                    vl = Sse.Add(vl, vr);
                    Unsafe.Add(ref vBuffer, i) = vl.AsDouble().GetElement(0);
                    psx += acc;
                    if (psx >= ram)
                    {
                        psx -= ram;
                        isx++;
                        vx = Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref vSrcBuffer, isx));
                        vz = Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref vSrcBuffer, isx + 2));
                    }
                }
            }
            x = (int)psx;
            return (int)isx;
        }
        #endregion
    }
}

#endif
