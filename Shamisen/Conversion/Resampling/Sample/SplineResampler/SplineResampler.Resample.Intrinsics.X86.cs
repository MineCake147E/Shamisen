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
    public sealed partial class SplineResampler
    {
        #region Monaural
        private int ResampleCachedDirectMonauralX86(Span<float> buffer, Span<float> srcBuffer, ref Vector4 coeffPtr, ref int x, int ram, int acc, int facc)
            => (facc, acc, ram) switch
            {
                (0, 1, 2) when Sse41.IsSupported && X86Intrinsics.HasAllFeatures(X86IntrinsicsMask.Sse41)
                => ResampleCachedDirectMonauralDoubleRateSse41(buffer, srcBuffer, ref coeffPtr, ref x),
                (0, 1, 4) when Sse41.IsSupported && X86Intrinsics.HasAllFeatures(X86IntrinsicsMask.Sse41)
                => ResampleCachedDirectMonauralQuadrupleRateSse41(buffer, srcBuffer, ref coeffPtr, ref x),
                (0, 1, _) when Sse.IsSupported && X86Intrinsics.HasAllFeatures(X86IntrinsicsMask.Sse)
                => ResampleCachedDirectMonauralIntegerMultipleRateX86(buffer, srcBuffer, ref coeffPtr, ref x, ram),
                (0, _, _) when Sse.IsSupported && X86Intrinsics.HasAllFeatures(X86IntrinsicsMask.Sse)
                => ResampleCachedDirectMonauralUpAnyRateSse(buffer, srcBuffer, ref coeffPtr, ref x, ram, acc),
                (_, _, _) when Sse3.IsSupported && X86Intrinsics.HasAllFeatures(X86IntrinsicsMask.Sse3)
                => ResampleCachedDirectMonauralAnyRateSse3(buffer, srcBuffer, ref coeffPtr, ref x, ram, acc, facc),
                _ => ResampleCachedDirectMonauralStandard(buffer, srcBuffer, ref coeffPtr, ref x, ram, acc, facc),
            };

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
            nint isx = 0;
            nint psx = x;
            nint nram = ram;
            ref var coeff = ref Unsafe.As<Vector4, Vector128<float>>(ref coeffPtr);
            ref var src = ref MemoryMarshal.GetReference(srcBuffer);
            ref var dst = ref MemoryMarshal.GetReference(buffer);
            var xmm7 = Unsafe.As<float, Vector128<float>>(ref src);
            for (i = 0; i < length; i++)
            {
                var cutmullCoeffs = Unsafe.Add(ref coeff, psx);
                Unsafe.Add(ref dst, i) = VectorUtils.FastDotProduct(xmm7, cutmullCoeffs);
                psx += acc;
                if (psx >= nram)
                {
                    psx -= nram;
                    isx++;
                    xmm7 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref src, isx));
                }
            }
            x = (int)psx;
            return (int)isx;
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
        private static int ResampleCachedDirectMonauralUpAnyRateSse(Span<float> buffer, Span<float> srcBuffer, ref Vector4 coeffPtr, ref int x, int ram, int acc)
        {
            nint i = 0;
            nint length = buffer.Length;
            nint isx = 0;
            nint psx = x;
            nint nram = ram;
            ref var coeff = ref Unsafe.As<Vector4, Vector128<float>>(ref coeffPtr);
            ref var src = ref MemoryMarshal.GetReference(srcBuffer);
            ref var dst = ref MemoryMarshal.GetReference(buffer);
            var xmm7 = Unsafe.As<float, Vector128<float>>(ref src);
            for (i = 0; i < length; i++)
            {
                var xmm0 = Unsafe.Add(ref coeff, psx);
                xmm0 = Sse.Multiply(xmm0, xmm7);
                xmm7 = Sse.Shuffle(xmm0, xmm0, 0b11_10_11_10);
                xmm0 = Sse.Add(xmm0, xmm7);
                psx += acc;
                xmm7 = Sse.Shuffle(xmm0, xmm0, 0b01_01_01_01);
                xmm0 = Sse.AddScalar(xmm0, xmm7);
                Unsafe.Add(ref dst, i) = xmm0.GetElement(0);
                if (psx >= nram)
                {
                    psx -= nram;
                    isx++;
                    xmm7 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref src, isx));
                }
            }
            x = (int)psx;
            return (int)isx;
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static int ResampleCachedDirectMonauralIntegerMultipleRateX86(Span<float> buffer, Span<float> srcBuffer, ref Vector4 coeffPtr, ref int x, int ram)
        {
            nint i = 0;
            nint length = buffer.Length;
            nint isx = 0;
            nint psx = x;
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
            x = (int)psx;
            return (int)isx;
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static int ResampleCachedDirectMonauralAnyRateSse3(Span<float> buffer, Span<float> srcBuffer, ref Vector4 coeffPtr, ref int x, int ram, int acc, int facc)
        {
            nint i = 0;
            nint length = buffer.Length;
            nint isx = 0;
            nint psx = x;
            nint nram = ram;
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
                bool v = psx >= nram;
                xmm7 = Sse3.MoveHighAndDuplicate(xmm0);
                byte v1 = Unsafe.As<bool, byte>(ref v);
                isx += v1;
                psx -= nram & -v1;
                xmm0 = Sse.AddScalar(xmm0, xmm7);
                xmm7 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref src, isx));
                Unsafe.Add(ref dst, i) = xmm0.GetElement(0);
            }
            x = (int)psx;
            return (int)isx;
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static int ResampleCachedDirectMonauralDoubleRateSse41(Span<float> buffer, Span<float> srcBuffer, ref Vector4 coeffPtr, ref int x)
        {
            nint i = 0;
            nint length = buffer.Length;
            nint isx = 0;
            nint psx = x & 1;
            ref var coeff = ref Unsafe.As<Vector4, Vector128<float>>(ref coeffPtr);
            ref var src = ref MemoryMarshal.GetReference(srcBuffer);
            ref var dst = ref MemoryMarshal.GetReference(buffer);
            var xmm7 = Unsafe.As<float, Vector128<float>>(ref src);
            var xmm0 = Unsafe.Add(ref coeff, 0);
            var xmm1 = Unsafe.Add(ref coeff, 1);
            if (psx > 0)
            {
                var xmm2 = Sse.Multiply(xmm1, xmm7);
                var xmm4 = Sse2.UnpackHigh(xmm2.AsDouble(), xmm2.AsDouble()).AsSingle();
                xmm2 = Sse.Add(xmm4, xmm2);
                xmm4 = Sse.Shuffle(xmm2, xmm2, 0b01_01_01_01);
                xmm2 = Sse.AddScalar(xmm2, xmm4);
                Unsafe.Add(ref dst, i++) = xmm2.GetElement(0);
                isx++;
                xmm7 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref src, isx));
                psx = 0;
            }
            nint olen = length - 1;
            for (; i < olen; i += 2)
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
            x = (int)psx;
            return (int)isx;
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static int ResampleCachedDirectMonauralQuadrupleRateSse41(Span<float> buffer, Span<float> srcBuffer, ref Vector4 coeffPtr, ref int x)
        {
            nint i = 0;
            nint length = buffer.Length;
            nint isx = 0;
            nint psx = x & 3;
            ref var coeff = ref Unsafe.As<Vector4, Vector128<float>>(ref coeffPtr);
            ref var src = ref MemoryMarshal.GetReference(srcBuffer);
            ref var dst = ref MemoryMarshal.GetReference(buffer);
            var xmm7 = Unsafe.As<float, Vector128<float>>(ref src);
            if (psx > 0)
            {
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
            x = (int)psx;
            return (int)isx;
        }
        #endregion
        #region Stereo
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private int ResampleCachedDirect2ChannelsX86(Span<float> buffer, Span<float> srcBuffer, ref Vector4 coeffPtr, ref int x, int ram, int acc, int facc)
            => (facc, acc, ram) switch
            {
                (0, 1, 2) when Sse.IsSupported && X86Intrinsics.HasAllFeatures(X86IntrinsicsMask.Sse)
                => ResampleCachedDirectStereoDoubleRateSse(buffer, srcBuffer, ref coeffPtr, ref x),
                (0, 1, 4) when Sse.X64.IsSupported && X86Intrinsics.HasAllFeatures(X86IntrinsicsMask.Sse | X86IntrinsicsMask.X64)
                => ResampleCachedDirectStereoQuadrupleRateX64(buffer, srcBuffer, ref coeffPtr, ref x),
                (0, 1, _) when Sse.IsSupported && X86Intrinsics.HasAllFeatures(X86IntrinsicsMask.Sse)
                => ResampleCachedDirectStereoIntegerRateSse(buffer, srcBuffer, ref coeffPtr, ref x, ram),
                (0, _, _) when Sse.IsSupported && X86Intrinsics.HasAllFeatures(X86IntrinsicsMask.Sse)
                => ResampleCachedDirectStereoUpAnyRateSse(buffer, srcBuffer, ref coeffPtr, ref x, ram, acc),
                (_, _, _) when Sse.IsSupported && X86Intrinsics.HasAllFeatures(X86IntrinsicsMask.Sse)
                => ResampleCachedDirectStereoAnyRateSse(buffer, srcBuffer, ref coeffPtr, ref x, ram, acc, facc),
                _ => ResampleCachedDirect2ChannelsStandard(buffer, srcBuffer, ref coeffPtr, ref x, ram, acc, facc),
            };

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static int ResampleCachedDirectStereoUpAnyRateSse(Span<float> buffer, Span<float> srcBuffer, ref Vector4 coeffPtr, ref int x, int ram, int acc)
        {
            nint isx = 0;
            nint psx = x;
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
                psx += acc;
                if (psx >= ram)
                {
                    psx -= ram;
                    isx++;
                    vx = Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref vSrcBuffer, isx));
                    vz = Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref vSrcBuffer, isx + 2));
                }
            }
            x = (int)psx;
            return (int)isx;
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static int ResampleCachedDirectStereoIntegerRateSse(Span<float> buffer, Span<float> srcBuffer, ref Vector4 coeffPtr, ref int x, int ram)
        {
            nint isx = 0;
            nint psx = x;
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

            x = (int)psx;
            return (int)isx;
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static int ResampleCachedDirectStereoDoubleRateSse(Span<float> buffer, Span<float> srcBuffer, ref Vector4 coeffPtr, ref int x)
        {
            nint isx = 0;
            nint psx = x;
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

            x = (int)psx;
            return (int)isx;
        }
        /// <summary>
        /// This variant needs more than 8 xmm registers so AVX or x64 SSE is required<br/>
        /// It does not use 256bit floating-point arithmetic, making it suitable for the Haswell microarchitecture.<br/>
        /// TODO: Post-Rocket-Lake variant using 256bit floating-point arithmetic
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="srcBuffer"></param>
        /// <param name="coeffPtr"></param>
        /// <param name="x"></param>

        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static int ResampleCachedDirectStereoQuadrupleRateX64(Span<float> buffer, Span<float> srcBuffer, ref Vector4 coeffPtr, ref int x)
        {
            nint isx = 0;
            nint psx = x;
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
            x = (int)psx;

            return (int)isx;
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static int ResampleCachedDirectStereoAnyRateSse(Span<float> buffer, Span<float> srcBuffer, ref Vector4 coeffPtr, ref int x, int ram, int acc, int facc)
        {
            nint isx = 0;
            nint psx = x;
            ref var vBuffer = ref Unsafe.As<float, double>(ref MemoryMarshal.GetReference(buffer));
            ref var vSrcBuffer = ref Unsafe.As<float, double>(ref MemoryMarshal.GetReference(srcBuffer));
            ref var coeff = ref Unsafe.As<Vector4, Vector128<float>>(ref coeffPtr);
            nint i;
            nint length = buffer.Length / 2;
            for (i = 0; i < length; i++)
            {
                var vx = Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref vSrcBuffer, isx));
                var vz = Unsafe.As<double, Vector128<float>>(ref Unsafe.Add(ref vSrcBuffer, isx + 2));
                var c = Unsafe.Add(ref coeff, psx);
                var vl = Sse.Shuffle(c, c, 0b01_01_00_00);
                psx += acc;
                var vr = Sse.Shuffle(c, c, 0b11_11_10_10);
                isx += facc;
                vl = Sse.Multiply(vx, vl);
                bool a = psx >= ram;
                vr = Sse.Multiply(vz, vr);
                int b = Unsafe.As<bool, byte>(ref a);
                vl = Sse.Add(vl, vr);
                psx -= ram & (-b);
                vr = Sse.Shuffle(vl, vl, 0b00_00_11_10);
                vl = Sse.Add(vl, vr);
                isx += b;
                Unsafe.Add(ref vBuffer, i) = vl.AsDouble().GetElement(0);
            }
            x = (int)psx;
            return (int)isx;
        }

        #endregion
    }
}

#endif
