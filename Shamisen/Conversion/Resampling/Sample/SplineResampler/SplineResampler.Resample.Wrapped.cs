using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Channels;

using Shamisen.Utils;

namespace Shamisen.Conversion.Resampling.Sample
{
    public sealed partial class SplineResampler
    {
        #region WrappedOdd
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        private static (int inputSampleIndex, int x, int rearrangedCoeffsIndex, int rearrangedCoeffsDirection) ResampleCachedWrappedOddMonauralStandard(Span<float> buffer, Span<float> srcBuffer, Span<Vector4> cspan, int x, int ram, int acc, int facc, int rearrangedCoeffsIndex, int rearrangedCoeffsDirection)
        {
            int isx = 0;
            int psx = x;
            ref var coeffPtr = ref MemoryMarshal.GetReference(cspan);
            int rec = rearrangedCoeffsIndex;
            int red = rearrangedCoeffsDirection;
            int rew = cspan.Length;
            nint i = 0, length = buffer.Length;
            ref float rsi = ref MemoryMarshal.GetReference(srcBuffer);
            ref float rdi = ref MemoryMarshal.GetReference(buffer);
            if (red < 0)
            {
                nint olen = MathI.Min(length, rec);
                for (; i < olen; i++)
                {
                    var values = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rsi, isx));
                    var cutmullCoeffs = VectorUtils.ReverseElements(Unsafe.Add(ref coeffPtr, rec--));
                    Unsafe.Add(ref rdi, i) = VectorUtils.FastDotProduct(values, cutmullCoeffs);
                    psx += acc;
                    isx += facc;
                    bool h = psx >= ram;
                    int y = Unsafe.As<bool, byte>(ref h);
                    isx += y;
                    psx -= -y & ram;
                }
                if (rec == 0)
                {
                    red = 1;
                }
            }
            while (i < length)
            {
                nint olen = MathI.Min(length, i - rec + rew);
                for (; i < olen; i++)
                {
                    var values = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rsi, isx));
                    var cutmullCoeffs = Unsafe.Add(ref coeffPtr, rec++);
                    Unsafe.Add(ref rdi, i) = VectorUtils.FastDotProduct(values, cutmullCoeffs);
                    psx += acc;
                    isx += facc;
                    bool h = psx >= ram;
                    int y = Unsafe.As<bool, byte>(ref h);
                    isx += y;
                    psx -= -y & ram;
                }
                if (rec >= rew)
                {
                    red = -1;
                    rec--;
                }
                olen = MathI.Min(length, i + rec);
                for (; i < olen; i++)
                {
                    var values = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rsi, isx));
                    var cutmullCoeffs = VectorUtils.ReverseElements(Unsafe.Add(ref coeffPtr, rec--));
                    Unsafe.Add(ref rdi, i) = VectorUtils.FastDotProduct(values, cutmullCoeffs);
                    psx += acc;
                    isx += facc;
                    bool h = psx >= ram;
                    int y = Unsafe.As<bool, byte>(ref h);
                    isx += y;
                    psx -= -y & ram;
                }
                if (rec == 0)
                {
                    red = 1;
                }
            }
            return (isx, psx, rec, red);
        }

        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        private static (int inputSampleIndex, int x, int rearrangedCoeffsIndex, int rearrangedCoeffsDirection) ResampleCachedWrappedOddVectorFitChannelsStandard(Span<float> buffer, Span<float> srcBuffer, Span<Vector4> cspan, int x, int ram, int acc, int facc, int rearrangedCoeffsIndex, int rearrangedCoeffsDirection)
        {
            int isx = 0;
            int psx = x;
            ref var coeffPtr = ref MemoryMarshal.GetReference(cspan);
            int rec = rearrangedCoeffsIndex;
            int red = rearrangedCoeffsDirection;
            int rew = cspan.Length;
            var vBuffer = MemoryMarshal.Cast<float, Vector<float>>(buffer);
            var vSrcBuffer = MemoryMarshal.Cast<float, Vector<float>>(srcBuffer);
            nint i = 0, length = vBuffer.Length;
            ref var rsi = ref MemoryMarshal.GetReference(vSrcBuffer);
            ref var rdi = ref MemoryMarshal.GetReference(vBuffer);
            if (red < 0)
            {
                nint olen = MathI.Min(length, rec);
                for (; i < olen; i++)
                {
                    ref var values = ref Unsafe.As<Vector<float>,
                        (Vector<float> X, Vector<float> Y, Vector<float> Z, Vector<float> W)>(ref Unsafe.Add(ref rsi, isx));
                    var cutmullCoeffs = VectorUtils.ReverseElements(Unsafe.Add(ref coeffPtr, rec--));
                    var value1 = values.X * cutmullCoeffs.X; //The control point 1.
                    var value2 = values.Y * cutmullCoeffs.Y; //The control point 2.
                    var value3 = values.Z * cutmullCoeffs.Z; //The control point 3.
                    var value4 = values.W * cutmullCoeffs.W; //The control point 4.
                    Unsafe.Add(ref rdi, i) = (value1 + value3) + (value2 + value4);
                    psx += acc;
                    isx += facc;
                    bool h = psx >= ram;
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
                nint olen = MathI.Min(length, i - rec + rew);
                for (; i < olen; i++)
                {
                    ref var values = ref Unsafe.As<Vector<float>, (Vector<float> X, Vector<float> Y, Vector<float> Z, Vector<float> W)>
                                            (ref Unsafe.Add(ref rsi, isx));
                    var cutmullCoeffs = Unsafe.Add(ref coeffPtr, rec++);
                    var value1 = values.X * cutmullCoeffs.X; //The control point 1.
                    var value2 = values.Y * cutmullCoeffs.Y; //The control point 2.
                    var value3 = values.Z * cutmullCoeffs.Z; //The control point 3.
                    var value4 = values.W * cutmullCoeffs.W; //The control point 4.
                    Unsafe.Add(ref rdi, i) = (value1 + value3) + (value2 + value4);
                    psx += acc;
                    isx += facc;
                    bool h = psx >= ram;
                    int y = Unsafe.As<bool, byte>(ref h);
                    isx += y;
                    psx -= -y & ram;
                }
                if (rec >= rew)
                {
                    red = -1;
                    rec--;
                }
                olen = MathI.Min(length, i + rec);
                for (; i < olen; i++)
                {
                    ref var values = ref Unsafe.As<Vector<float>, (Vector<float> X, Vector<float> Y, Vector<float> Z, Vector<float> W)>
                                            (ref Unsafe.Add(ref rsi, isx));
                    var cutmullCoeffs = VectorUtils.ReverseElements(Unsafe.Add(ref coeffPtr, rec--));
                    var value1 = values.X * cutmullCoeffs.X; //The control point 1.
                    var value2 = values.Y * cutmullCoeffs.Y; //The control point 2.
                    var value3 = values.Z * cutmullCoeffs.Z; //The control point 3.
                    var value4 = values.W * cutmullCoeffs.W; //The control point 4.
                    Unsafe.Add(ref rdi, i) = (value1 + value3) + (value2 + value4);
                    psx += acc;
                    isx += facc;
                    bool h = psx >= ram;
                    int y = Unsafe.As<bool, byte>(ref h);
                    isx += y;
                    psx -= -y & ram;
                }
                if (rec == 0)
                {
                    red = 1;
                }
            }
            return (isx, psx, rec, red);
        }

        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        private static (int inputSampleIndex, int x, int rearrangedCoeffsIndex, int rearrangedCoeffsDirection) ResampleCachedWrappedOddGeneric(Span<float> buffer, Span<float> srcBuffer, Span<Vector4> cspan, int channels, int x, int ram, int acc, int facc, int rearrangedCoeffsIndex, int rearrangedCoeffsDirection)
        {
            unchecked
            {
#if NETCOREAPP3_1_OR_GREATER
                return ResampleCachedWrappedOddGenericX86(buffer, srcBuffer, cspan, channels, x, ram, acc, facc, rearrangedCoeffsIndex, rearrangedCoeffsDirection);
#endif
                return ResampleCachedWrappedOddGenericStandard(buffer, srcBuffer, cspan, channels, x, ram, acc, facc, rearrangedCoeffsIndex, rearrangedCoeffsDirection);
            }
        }

        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        private static (int inputSampleIndex, int x, int rearrangedCoeffsIndex, int rearrangedCoeffsDirection) ResampleCachedWrappedOddGenericStandard(Span<float> buffer, Span<float> srcBuffer, Span<Vector4> cspan, int channels, int x, int ram, int acc, int facc, int rearrangedCoeffsIndex, int rearrangedCoeffsDirection)
        {
            nint isx = 0;
            nint psx = x;
            nint nchannels = channels;
            ref var coeffPtr = ref MemoryMarshal.GetReference(cspan);
            int rec = rearrangedCoeffsIndex;
            int red = rearrangedCoeffsDirection;
            int rew = cspan.Length;
            nint rewc = rew * nchannels;
            nint i = 0, length = buffer.Length - nchannels + 1;
            unsafe
            {
                fixed (float* rsi = srcBuffer)
                fixed (float* rdi = buffer)
                {
                    if (red < 0)
                    {
                        nint olen = MathI.Min(length, rec * nchannels);
                        for (; i < olen; i += nchannels)
                        {
                            var cutmullCoeffs = VectorUtils.ReverseElements(Unsafe.Add(ref coeffPtr, rec--));
                            float* head = rsi + isx * nchannels;
                            FastDotProductGeneric(nchannels, i, rdi, cutmullCoeffs, head);
                            psx += acc;
                            isx += facc;
                            bool h = psx >= ram;
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
                        nint olen = MathI.Min(length, i - rec * nchannels + rewc);
                        for (; i < olen; i += nchannels)
                        {
                            var cutmullCoeffs = Unsafe.Add(ref coeffPtr, rec++);
                            float* head = rsi + isx * nchannels;
                            FastDotProductGeneric(nchannels, i, rdi, cutmullCoeffs, head);
                            psx += acc;
                            isx += facc;
                            bool h = psx >= ram;
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
                            var cutmullCoeffs = VectorUtils.ReverseElements(Unsafe.Add(ref coeffPtr, rec--));
                            float* head = rsi + isx * nchannels;
                            FastDotProductGeneric(nchannels, i, rdi, cutmullCoeffs, head);
                            psx += acc;
                            isx += facc;
                            bool h = psx >= ram;
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

        #region WrappedEven
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        private static (int inputSampleIndex, int x, int rearrangedCoeffsIndex, int rearrangedCoeffsDirection) ResampleCachedWrappedEvenMonauralStandard(Span<float> buffer, Span<float> srcBuffer, Span<Vector4> cspan, int x, int ram, int acc, int facc, int rearrangedCoeffsIndex, int rearrangedCoeffsDirection)
        {
            int isx = 0;
            int psx = x;
            ref var coeffPtr = ref MemoryMarshal.GetReference(cspan);
            int rec = rearrangedCoeffsIndex;
            int red = rearrangedCoeffsDirection;
            int rew = cspan.Length;
            nint i = 0, length = buffer.Length;
            ref float rsi = ref MemoryMarshal.GetReference(srcBuffer);
            ref float rdi = ref MemoryMarshal.GetReference(buffer);
            if (red < 0)
            {
                for (; i < length && rec >= 0; i++)
                {
                    var values = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rsi, isx));
                    var cutmullCoeffs = VectorUtils.ReverseElements(Unsafe.Add(ref coeffPtr, rec--));
                    Unsafe.Add(ref rdi, i) = VectorUtils.FastDotProduct(values, cutmullCoeffs);
                    psx += acc;
                    isx += facc;
                    bool h = psx >= ram;
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
            while (i < length)
            {
                for (; i < length && rec < rew; i++)
                {
                    var values = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rsi, isx));
                    var cutmullCoeffs = Unsafe.Add(ref coeffPtr, rec++);
                    Unsafe.Add(ref rdi, i) = VectorUtils.FastDotProduct(values, cutmullCoeffs);
                    psx += acc;
                    isx += facc;
                    bool h = psx >= ram;
                    int y = Unsafe.As<bool, byte>(ref h);
                    isx += y;
                    psx -= -y & ram;
                }
                if (rec >= rew)
                {
                    red = -1;
                    rec--;
                }
                for (; i < length && rec >= 0; i++)
                {
                    var values = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rsi, isx));
                    var cutmullCoeffs = VectorUtils.ReverseElements(Unsafe.Add(ref coeffPtr, rec--));
                    Unsafe.Add(ref rdi, i) = VectorUtils.FastDotProduct(values, cutmullCoeffs);
                    psx += acc;
                    isx += facc;
                    bool h = psx >= ram;
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
            return (isx, psx, rec, red);
        }

        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        private static (int inputSampleIndex, int x, int rearrangedCoeffsIndex, int rearrangedCoeffsDirection) ResampleCachedWrappedEvenVectorFitChannelsStandard(Span<float> buffer, Span<float> srcBuffer, Span<Vector4> cspan, int x, int ram, int acc, int facc, int rearrangedCoeffsIndex, int rearrangedCoeffsDirection)
        {
            int isx = 0;
            int psx = x;
            ref var coeffPtr = ref MemoryMarshal.GetReference(cspan);
            int rec = rearrangedCoeffsIndex;
            int red = rearrangedCoeffsDirection;
            int rew = cspan.Length;
            var vBuffer = MemoryMarshal.Cast<float, Vector<float>>(buffer);
            var vSrcBuffer = MemoryMarshal.Cast<float, Vector<float>>(srcBuffer);
            nint i = 0, length = vBuffer.Length;
            ref var rsi = ref MemoryMarshal.GetReference(vSrcBuffer);
            ref var rdi = ref MemoryMarshal.GetReference(vBuffer);
            if (red < 0)
            {
                for (; i < length && rec >= 0; i++)
                {
                    ref var values = ref Unsafe.As<Vector<float>,
                        (Vector<float> X, Vector<float> Y, Vector<float> Z, Vector<float> W)>(ref Unsafe.Add(ref rsi, isx));
                    var cutmullCoeffs = VectorUtils.ReverseElements(Unsafe.Add(ref coeffPtr, rec--));
                    var value1 = values.X * cutmullCoeffs.X;
                    var value2 = values.Y * cutmullCoeffs.Y;
                    var value3 = values.Z * cutmullCoeffs.Z;
                    var value4 = values.W * cutmullCoeffs.W;
                    Unsafe.Add(ref rdi, i) = (value1 + value3) + (value2 + value4);
                    psx += acc;
                    isx += facc;
                    bool h = psx >= ram;
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
            while (i < length)
            {
                for (; i < length && rec < rew; i++)
                {
                    ref var values = ref Unsafe.As<Vector<float>, (Vector<float> X, Vector<float> Y, Vector<float> Z, Vector<float> W)>
                                            (ref Unsafe.Add(ref rsi, isx));
                    var cutmullCoeffs = Unsafe.Add(ref coeffPtr, rec++);
                    var value1 = values.X * cutmullCoeffs.X;
                    var value2 = values.Y * cutmullCoeffs.Y;
                    var value3 = values.Z * cutmullCoeffs.Z;
                    var value4 = values.W * cutmullCoeffs.W;
                    Unsafe.Add(ref rdi, i) = (value1 + value3) + (value2 + value4);
                    psx += acc;
                    isx += facc;
                    bool h = psx >= ram;
                    int y = Unsafe.As<bool, byte>(ref h);
                    isx += y;
                    psx -= -y & ram;
                }
                if (rec >= rew)
                {
                    red = -1;
                    rec--;
                }
                for (; i < length && rec >= 0; i++)
                {
                    ref var values = ref Unsafe.As<Vector<float>, (Vector<float> X, Vector<float> Y, Vector<float> Z, Vector<float> W)>
                                            (ref Unsafe.Add(ref rsi, isx));
                    var cutmullCoeffs = VectorUtils.ReverseElements(Unsafe.Add(ref coeffPtr, rec--));
                    var value1 = values.X * cutmullCoeffs.X;
                    var value2 = values.Y * cutmullCoeffs.Y;
                    var value3 = values.Z * cutmullCoeffs.Z;
                    var value4 = values.W * cutmullCoeffs.W;
                    Unsafe.Add(ref rdi, i) = (value1 + value3) + (value2 + value4);
                    psx += acc;
                    isx += facc;
                    bool h = psx >= ram;
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
            return (isx, psx, rec, red);
        }
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        private static (int inputSampleIndex, int x, int rearrangedCoeffsIndex, int rearrangedCoeffsDirection) ResampleCachedWrappedEvenGeneric(Span<float> buffer, Span<float> srcBuffer, Span<Vector4> cspan, int channels, int x, int ram, int acc, int facc, int rearrangedCoeffsIndex, int rearrangedCoeffsDirection)
        {
            unchecked
            {
#if NETCOREAPP3_1_OR_GREATER
                return ResampleCachedWrappedEvenGenericX86(buffer, srcBuffer, cspan, channels, x, ram, acc, facc, rearrangedCoeffsIndex, rearrangedCoeffsDirection);
#endif
                return ResampleCachedWrappedEvenGenericStandard(buffer, srcBuffer, cspan, channels, x, ram, acc, facc, rearrangedCoeffsIndex, rearrangedCoeffsDirection);
            }
        }


        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        private static (int inputSampleIndex, int x, int rearrangedCoeffsIndex, int rearrangedCoeffsDirection) ResampleCachedWrappedEvenGenericStandard(Span<float> buffer, Span<float> srcBuffer, Span<Vector4> cspan, int channels, int x, int ram, int acc, int facc, int rearrangedCoeffsIndex, int rearrangedCoeffsDirection)
        {
            nint isx = 0;
            nint psx = x;
            nint nchannels = channels;
            ref var coeffPtr = ref MemoryMarshal.GetReference(cspan);
            int rec = rearrangedCoeffsIndex;
            int red = rearrangedCoeffsDirection;
            int rew = cspan.Length;
            nint rewc = rew * nchannels;
            nint i = 0, length = buffer.Length - nchannels + 1;
            unsafe
            {
                fixed (float* rsi = srcBuffer)
                fixed (float* rdi = buffer)
                {
                    if (red < 0)
                    {
                        nint olen = MathI.Min(length, rec * nchannels + nchannels);
                        for (; i < olen; i += nchannels)
                        {
                            var cutmullCoeffs = VectorUtils.ReverseElements(Unsafe.Add(ref coeffPtr, rec--));
                            float* head = rsi + isx * nchannels;
                            FastDotProductGeneric(nchannels, i, rdi, cutmullCoeffs, head);
                            psx += acc;
                            isx += facc;
                            bool h = psx >= ram;
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
                        nint olen = MathI.Min(length, i - rec * nchannels + rewc);
                        for (; i < olen; i += nchannels)
                        {
                            var cutmullCoeffs = Unsafe.Add(ref coeffPtr, rec++);
                            float* head = rsi + isx * nchannels;
                            FastDotProductGeneric(nchannels, i, rdi, cutmullCoeffs, head);
                            psx += acc;
                            isx += facc;
                            bool h = psx >= ram;
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
                            var cutmullCoeffs = VectorUtils.ReverseElements(Unsafe.Add(ref coeffPtr, rec--));
                            float* head = rsi + isx * nchannels;
                            FastDotProductGeneric(nchannels, i, rdi, cutmullCoeffs, head);
                            psx += acc;
                            isx += facc;
                            bool h = psx >= ram;
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

    }
}
