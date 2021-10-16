using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

using Shamisen.Utils;

namespace Shamisen.Conversion.Resampling.Sample
{
    public sealed partial class SplineResampler
    {

        #region Resample
        #region CachedDirect

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
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
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
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        private int ResampleCachedDirect2Channels(Span<float> buffer, Span<float> srcBuffer, ref Vector4 coeffPtr, ref int x, int ram, int acc, int facc, ref int rci) =>
#if NETCOREAPP3_1_OR_GREATER
            ResampleCachedDirect2ChannelsX86(buffer, srcBuffer, ref coeffPtr, ref x, ram, acc, facc, ref rci);
#else
            ResampleCachedDirect2ChannelsStandard(buffer, srcBuffer, ref coeffPtr, ref x, ram, acc, facc, ref rci);
#endif

        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        private int ResampleCachedDirect3Channels(Span<float> buffer, Span<float> srcBuffer, ref Vector4 coeffPtr, ref int x, int ram, int acc, int facc, ref int rci) => ResampleCachedDirect3ChannelsStandard(buffer, srcBuffer, ref coeffPtr, ref x, ram, acc, facc, ref rci);
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        private int ResampleCachedDirect4Channels(Span<float> buffer, Span<float> srcBuffer, ref Vector4 coeffPtr, ref int x, int ram, int acc, int facc, ref int rci) => ResampleCachedDirect4ChannelsStandard(buffer, srcBuffer, ref coeffPtr, ref x, ram, acc, facc, ref rci);

        #region Generic

        private static int ResampleCachedDirectGeneric(Span<float> buffer, Span<float> srcBuffer, ref Vector4 coeffPtr, int channels, ref int x, int ram, int acc, int facc, ref int rci)
        {
            unchecked
            {
#if NETCOREAPP3_1_OR_GREATER
                return ResampleCachedDirectGenericX86(buffer, srcBuffer, ref coeffPtr, channels, ref x, ram, acc, facc, ref rci);
#endif
                return ResampleCachedDirectGenericStandard(buffer, srcBuffer, ref coeffPtr, channels, ref x, ram, acc, facc, ref rci);
            }
        }
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        private static int ResampleCachedDirectGenericStandard(Span<float> buffer, Span<float> srcBuffer, ref Vector4 coeffPtr, int channels, ref int x, int ram, int acc, int facc, ref int rci)
        {
            nint i = 0;
            nint isx = 0;
            nint psx = x;
            nint nram = ram;
            nint nrci = rci;
            nint nchannels = channels;
            nint length = buffer.Length - nchannels + 1;
            int rch = channels & 0x3;
            unsafe
            {
                fixed (float* rsi = srcBuffer)
                fixed (float* rdi = buffer)
                {
                    for (; i < length; i += nchannels)
                    {
                        psx += acc;
                        var y0 = Unsafe.Add(ref coeffPtr, nrci);
                        float* head = rsi + isx * nchannels;
                        float* head2 = head + nchannels * 2;
                        nint nch = 0;
                        nint cholen = nchannels - 3;
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
                        bool j = ++nrci < nram;
                        nint z = Unsafe.As<bool, byte>(ref j);
                        nrci &= -z;
                        bool h = psx >= nram;
                        nint g = Unsafe.As<bool, byte>(ref h);
                        isx += g;
                        isx += facc;
                        psx -= -g & nram;
                    }
                }
            }
            rci = (int)nrci;
            x = (int)psx;
            return (int)isx;
        }

        #endregion

        #endregion

        #region Direct
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        private static int ResampleDirectMonauralStandard(Span<float> buffer, Span<float> srcBuffer, ref int x, int ram, int acc, int facc, float rmi)
        {
            nint i = 0;
            nint length = buffer.Length;
            nint isx = 0;
            nint psx = x;
            nint spsx = psx;
            nint nram = ram;
            ref float src = ref MemoryMarshal.GetReference(srcBuffer);
            ref float dst = ref MemoryMarshal.GetReference(buffer);
            Vector4 c0 = C0, c1 = C1, c2 = C2, c3 = C3;
            Vector4 x1 = default, x2 = default, x3 = default;
            Vector4 y0 = default, y1 = default, y2 = default;
            for (int j = 0; j < 4; j++)
            {
                var nx3 = new Vector4(spsx * rmi);
                spsx += acc;
                bool h2 = spsx >= nram;
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
                bool h = psx >= nram;
                nint g = Unsafe.As<bool, byte>(ref h);
                var values = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref src, isx));
                isx += g;
                isx += facc;
                Unsafe.Add(ref dst, i) = VectorUtils.FastDotProduct(values, y0);
                var nx3 = new Vector4(spsx * rmi);
                spsx += acc;
                bool h2 = spsx >= nram;
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
            x = (int)psx;
            return (int)isx;
        }

        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        private static int ResampleDirectVectorFitStandard(Span<float> buffer, Span<float> srcBuffer, ref int x, int ram, int acc, int facc, float rmi)
        {
            nint i = 0;
            nint isx = 0;
            nint psx = x;
            nint spsx = psx;
            nint nram = ram;
            var vBuffer = MemoryMarshal.Cast<float, Vector<float>>(buffer);
            var vSrcBuffer = MemoryMarshal.Cast<float, Vector<float>>(srcBuffer);
            nint length = vBuffer.Length;
            ref var src = ref MemoryMarshal.GetReference(vSrcBuffer);
            ref var dst = ref MemoryMarshal.GetReference(vBuffer);
            Vector4 c0 = C0, c1 = C1, c2 = C2, c3 = C3;
            Vector4 x1 = default, x2 = default, x3 = default;
            Vector4 y0 = default, y1 = default, y2 = default;
            for (int j = 0; j < 4; j++)
            {
                var nx3 = new Vector4(spsx * rmi);
                spsx += acc;
                bool h2 = spsx >= nram;
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
                bool h = psx >= nram;
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
                bool h2 = spsx >= nram;
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
            x = (int)psx;
            return (int)isx;
        }

        /// <summary>
        /// Performs resampling no matter how many channels to process.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="srcBuffer"></param>
        /// <param name="channels"></param>
        /// <param name="x"></param>
        /// <param name="ram"></param>
        /// <param name="acc"></param>
        /// <param name="facc"></param>
        /// <param name="rmi"></param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        private static int ResampleDirectGeneric(Span<float> buffer, Span<float> srcBuffer, int channels, ref int x, int ram, int acc, int facc, float rmi)
        {
            unchecked
            {
#if NETCOREAPP3_1_OR_GREATER
                return ResampleDirectGenericX86(buffer, srcBuffer, channels, ref x, ram, acc, facc, rmi);
#endif
                return ResampleDirectGenericStandard(buffer, srcBuffer, channels, ref x, ram, acc, facc, rmi);
            }
        }

        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        private static int ResampleDirectGenericStandard(Span<float> buffer, Span<float> srcBuffer, int channels, ref int x, int ram, int acc, int facc, float rmi)
        {
            nint i = 0;
            nint isx = 0;
            int psx = x;
            int spsx = psx;
            int nram = ram;
            nint nchannels = channels;
            nint length = buffer.Length - channels + 1;
            ref float src = ref MemoryMarshal.GetReference(srcBuffer);
            ref float dst = ref MemoryMarshal.GetReference(buffer);
            Vector4 c0 = C0, c1 = C1, c2 = C2, c3 = C3;
            Vector4 x1 = default, x2 = default, x3 = default;
            Vector4 y0 = default, y1 = default, y2 = default;
            for (int j = 0; j < 4; j++)
            {
                var nx3 = new Vector4(spsx * rmi);
                spsx += acc;
                bool h2 = spsx >= nram;
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
                        float* head = rsi + isx * nchannels;
                        float* head2 = head + nchannels * 2;
                        nint nch = 0;
                        nint cholen = nchannels - 3;
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
                        bool h = psx >= nram;
                        int g = Unsafe.As<bool, byte>(ref h);
                        isx += g;
                        isx += facc;
                        psx -= -g & nram;
                        var nx3 = new Vector4(spsx * rmi);
                        spsx += acc;
                        bool h2 = spsx >= nram;
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
            x = psx;
            return (int)isx;
        }

        #endregion
        #endregion

    }
}
