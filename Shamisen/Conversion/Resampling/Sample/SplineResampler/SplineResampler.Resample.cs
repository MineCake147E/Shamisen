using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Shamisen.Utils;

namespace Shamisen.Conversion.Resampling.Sample
{
    public sealed partial class SplineResampler
    {

        #region Resample
        #region Monaural
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        private int ResampleCachedDirectMonaural(Span<float> buffer, Span<float> srcBuffer, ref Vector4 coeffPtr, ref int x, int ram, int acc, int facc, ref int rci) =>
#if NETCOREAPP3_1_OR_GREATER
            ResampleCachedDirectMonauralX86(buffer, srcBuffer, ref coeffPtr, ref x, ram, acc, facc, ref rci);
#else
            ResampleCachedDirectMonauralStandard(buffer, srcBuffer, ref coeffPtr, ref x, ram, acc, facc, ref rci);
#endif
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        private static int ResampleCachedDirectVectorFitChannelsStandard(Span<float> buffer, Span<float> srcBuffer, ref Vector4 coeffPtr, ref int x, int ram, int acc, int facc, ref int rci)
        {
            int i = 0;
            nint length = buffer.Length;
            int isx = 0;
            nint psx = x;
            nint nrci = rci;
            nint nram = ram;
            ref float src = ref MemoryMarshal.GetReference(srcBuffer);
            ref float dst = ref MemoryMarshal.GetReference(buffer);
            var vBuffer = MemoryMarshal.Cast<float, Vector<float>>(buffer);
            var vSrcBuffer = MemoryMarshal.Cast<float, Vector<float>>(srcBuffer);
            for (; i < vBuffer.Length; i++)
            {
                x += acc;
                ref var values = ref Unsafe.As<Vector<float>,
                    (Vector<float> X, Vector<float> Y, Vector<float> Z, Vector<float> W)>(ref vSrcBuffer[isx]);
                var cutmullCoeffs = Unsafe.Add(ref coeffPtr, nrci);
                isx += facc;
                bool j = ++nrci < nram;
                nint z = Unsafe.As<bool, byte>(ref j);
                var value1 = values.X * cutmullCoeffs.X;
                nrci &= -z;
                var value2 = values.Y * cutmullCoeffs.Y;
                bool h = x >= ram;
                int y = Unsafe.As<bool, byte>(ref h);
                var value3 = values.Z * cutmullCoeffs.Z;
                var value4 = values.W * cutmullCoeffs.W;
                isx += y;
                x -= -y & ram;
                vBuffer[i] = (value1 + value3) + (value2 + value4);
            }
            rci = (int)nrci;
            x = (int)psx;
            return isx;
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static int ResampleCachedDirectMonauralStandard(Span<float> buffer, Span<float> srcBuffer, ref Vector4 coeffPtr, ref int x, int ram, int acc, int facc, ref int rci)
            => (facc, acc, ram) switch
            {
                (0, 1, 2) => ResampleCachedDirectMonauralDoubleRateStandard(buffer, srcBuffer, ref coeffPtr, ref x),
                (0, 1, 4) => ResampleCachedDirectMonauralQuadrupleRateStandard(buffer, srcBuffer, ref coeffPtr, ref x),
                (0, 1, _) => ResampleCachedDirectMonauralIntegerMultipleRateStandard(buffer, srcBuffer, ref coeffPtr, ref x, ram),
                (0, _, _) => ResampleCachedDirectMonauralUpAnyRateStandard(buffer, srcBuffer, ref coeffPtr, ref x, ram, acc, ref rci),
                _ => ResampleCachedDirectMonauralAnyRateStandard(buffer, srcBuffer, ref coeffPtr, ref x, ram, acc, facc, ref rci)
            };
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        private static int ResampleCachedDirectMonauralAnyRateStandard(Span<float> buffer, Span<float> srcBuffer, ref Vector4 coeffPtr, ref int x, int ram, int acc, int facc, ref int rci)
        {
            nint i = 0;
            nint length = buffer.Length;
            nint isx = 0;
            nint psx = x;
            nint nrci = rci;
            nint nram = ram;
            ref float src = ref MemoryMarshal.GetReference(srcBuffer);
            ref float dst = ref MemoryMarshal.GetReference(buffer);
            Vector4 values;
            for (; i < length; i++)
            {
                psx += acc;
                var cutmullCoeffs = Unsafe.Add(ref coeffPtr, nrci);
                bool h = psx >= nram;
                nint y = Unsafe.As<bool, byte>(ref h);
                bool j = ++nrci < nram;
                nint z = Unsafe.As<bool, byte>(ref j);
                values = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref src, isx));
                nrci &= -z;
                Unsafe.Add(ref dst, i) = VectorUtils.FastDotProduct(values, cutmullCoeffs);
                isx += facc;
                isx += y;
                psx -= -y & nram;
            }
            rci = (int)nrci;
            x = (int)psx;
            return (int)isx;
        }

        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        private static int ResampleCachedDirectMonauralUpAnyRateStandard(Span<float> buffer, Span<float> srcBuffer, ref Vector4 coeffPtr, ref int x, int ram, int acc, ref int rci)
        {
            nint i = 0;
            nint length = buffer.Length;
            nint isx = 0;
            nint psx = x;
            nint nrci = rci;
            nint nram = ram;
            ref float src = ref MemoryMarshal.GetReference(srcBuffer);
            ref float dst = ref MemoryMarshal.GetReference(buffer);
            Vector4 values;
            for (i = 0; i < length; i++)
            {
                psx += acc;
                var cutmullCoeffs = Unsafe.Add(ref coeffPtr, nrci);
                bool h = psx >= nram;
                nint y = Unsafe.As<bool, byte>(ref h);
                bool j = ++nrci < nram;
                nint z = Unsafe.As<bool, byte>(ref j);
                values = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref src, isx));
                nrci &= -z;
                Unsafe.Add(ref dst, i) = VectorUtils.FastDotProduct(values, cutmullCoeffs);
                isx += y;
                psx -= -y & nram;
            }
            rci = (int)nrci;
            x = (int)psx;
            return (int)isx;
        }

        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        private static int ResampleCachedDirectMonauralIntegerMultipleRateStandard(Span<float> buffer, Span<float> srcBuffer, ref Vector4 coeffPtr, ref int x, int ram)
        {
            nint i = 0;
            nint length = buffer.Length;
            nint isx = 0;
            nint psx = x;
            ref float src = ref MemoryMarshal.GetReference(srcBuffer);
            ref float dst = ref MemoryMarshal.GetReference(buffer);
            Vector4 values;
            for (i = 0; i < length; i++)
            {
                values = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref src, isx));
                var cutmullCoeffs = Unsafe.Add(ref coeffPtr, psx++);
                bool h = psx >= ram;
                int y = Unsafe.As<bool, byte>(ref h);
                Unsafe.Add(ref dst, i) = VectorUtils.FastDotProduct(values, cutmullCoeffs);
                isx += y;
                psx -= -y & ram;
            }
            x = (int)psx;
            return (int)isx;
        }

        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        private static int ResampleCachedDirectMonauralDoubleRateStandard(Span<float> buffer, Span<float> srcBuffer, ref Vector4 coeffPtr, ref int x)
        {
            nint i = 0;
            nint length = buffer.Length;
            nint isx = 0;
            nint psx = x;
            ref float src = ref MemoryMarshal.GetReference(srcBuffer);
            ref float dst = ref MemoryMarshal.GetReference(buffer);
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
            x = (int)psx;
            return (int)isx;
        }
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        private static int ResampleCachedDirectMonauralQuadrupleRateStandard(Span<float> buffer, Span<float> srcBuffer, ref Vector4 coeffPtr, ref int x)
        {
            nint i = 0;
            nint length = buffer.Length;
            nint isx = 0;
            nint psx = x;
            ref float src = ref MemoryMarshal.GetReference(srcBuffer);
            ref float dst = ref MemoryMarshal.GetReference(buffer);
            Vector4 values;
            var c0 = coeffPtr;
            var c1 = Unsafe.Add(ref coeffPtr, 1);
            var c2 = Unsafe.Add(ref coeffPtr, 2);
            var c3 = Unsafe.Add(ref coeffPtr, 3);
            for (i = 0; psx != 0 && psx < 4; i++, psx++)
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
            x = (int)psx;
            return (int)isx;
        }

        #endregion
        private int ResampleCachedDirect2Channels(Span<float> buffer, Span<float> srcBuffer, ref Vector4 coeffPtr, ref int x, int ram, int acc, int facc, ref int rci) =>
#if NETCOREAPP3_1_OR_GREATER
            ResampleCachedDirect2ChannelsX86(buffer, srcBuffer, ref coeffPtr, ref x, ram, acc, facc, ref rci);
#else
            ResampleCachedDirect2ChannelsStandard(buffer, srcBuffer, ref coeffPtr, ref x, ram, acc, facc, ref rci);
#endif

        private int ResampleCachedDirect3Channels(Span<float> buffer, Span<float> srcBuffer, ref Vector4 coeffPtr, ref int x, int ram, int acc, int facc, ref int rci) => ResampleCachedDirect3ChannelsStandard(buffer, srcBuffer, ref coeffPtr, ref x, ram, acc, facc, ref rci);
        private int ResampleCachedDirect4Channels(Span<float> buffer, Span<float> srcBuffer, ref Vector4 coeffPtr, ref int x, int ram, int acc, int facc, ref int rci) => ResampleCachedDirect4ChannelsStandard(buffer, srcBuffer, ref coeffPtr, ref x, ram, acc, facc, ref rci);
        #endregion

    }
}
