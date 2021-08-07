using System;
using System.Collections.Generic;
using System.Text;
using static System.Runtime.InteropServices.MemoryMarshal;
using Shamisen.Filters;
using Shamisen.Utils;
using System.Numerics;
using System.Runtime.CompilerServices;
using Shamisen.Mathematics;
using System.Runtime.InteropServices;
using DivideSharp;
using Shamisen.Optimization;

namespace Shamisen.Conversion.Resampling.Sample
{
    /// <summary>
    /// Performs up-sampling using Catmull-Rom Spline interpolation.
    ///
    /// </summary>
    /// <seealso cref="ResamplerBase" />
    public sealed partial class SplineResampler : ResamplerBase
    {
        private ResizableBufferWrapper<float> bufferWrapper;
        private int conversionGradient = 0;
        private int framesReserved = 1;

        /// <summary>
        /// The pre calculated catmull-rom coefficents.<br/>
        /// X: The coefficent for value1 ((-xP3 + 2 * xP2 - x) * 0.5f)<br/>
        /// Y: The coefficent for value2 (((3 * xP3) - (5 * xP2) + 2) * 0.5f)<br/>
        /// Z: The coefficent for value3 ((-(3 * xP3) + 4 * xP2 + x) * 0.5f)<br/>
        /// W: The coefficent for value4 ((xP3 - xP2) * 0.5f)<br/>
        /// </summary>
        private Vector4[] preCalculatedCatmullRomCoefficents;

        private float[][] sampleCache;
        private int samplesRemaining = 0;

        private bool isEndOfStream = false;

        private ResampleStrategy Strategy { get; }
        private X86Intrinsics X86Intrinsics { get; }

        internal SplineResampler(IReadableAudioSource<float, SampleFormat> source, int destinationSampleRate, X86Intrinsics x86Intrinsics)
            : base(source.Format.SampleRate > destinationSampleRate
                ? new BiQuadFilter(source, BiQuadParameter.CreateLPFParameter(source.Format.SampleRate, destinationSampleRate* 0.5, 0.70710678118654752440084436210485))
                : source, destinationSampleRate)
        {
            X86Intrinsics = x86Intrinsics;
            bufferWrapper = new ResizablePooledBufferWrapper<float>(1);
            sampleCache = new float[3][];
            for (int i = 0; i < sampleCache.Length; i++)
            {
                sampleCache[i] = new float[Channels];
            }
            Strategy = ResampleStrategy.Direct;
            preCalculatedCatmullRomCoefficents = new Vector4[] { };
            if (source.Format.SampleRate < destinationSampleRate)
            {
                if (RateMul <= 512)
                {
                    Strategy = ResampleStrategy.CachedDirect;
                    preCalculatedCatmullRomCoefficents = new Vector4[RateMul];
                    for (int i = 0; i < preCalculatedCatmullRomCoefficents.Length; i++)
                    {
                        var x = i * RateMulInverse;
                        preCalculatedCatmullRomCoefficents[i] = CalculateCatmullRomCoeffs(x);
                    }
                }
                else if (RateMul <= 1024)
                {
                    if ((RateMul & 1) > 0)
                    {
                        Strategy = ResampleStrategy.CachedWrappedOdd;
                        preCalculatedCatmullRomCoefficents = new Vector4[(RateMul / 2) + 1];
                        for (int i = 0; i < preCalculatedCatmullRomCoefficents.Length; i++)
                        {
                            var x = i * RateMulInverse;
                            preCalculatedCatmullRomCoefficents[i] = CalculateCatmullRomCoeffs(x);
                        }
                    }
                    else
                    {
                        Strategy = ResampleStrategy.CachedWrappedEven;
                        preCalculatedCatmullRomCoefficents = new Vector4[(RateMul / 2)];
                        for (int i = 0; i < preCalculatedCatmullRomCoefficents.Length; i++)
                        {
                            var x = i * RateMulInverse;
                            preCalculatedCatmullRomCoefficents[i] = CalculateCatmullRomCoeffs(x);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SplineResampler"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="destinationSampleRate">The destination sample rate.</param>
        public SplineResampler(IReadableAudioSource<float, SampleFormat> source, int destinationSampleRate) :
            this(source, destinationSampleRate, IntrinsicsUtils.X86Intrinsics)
        {
            
        }

        /// <summary>
        /// Reads the audio to the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>The length of the data written.</returns>
        public override ReadResult Read(Span<float> buffer)
        {
            int channels = Channels;
            if (buffer.Length < channels) throw new InvalidOperationException($"The length of buffer is less than {channels}!");
            if (isEndOfStream) return ReadResult.EndOfStream;

            #region Initialize and Read

            //Align the length of the buffer.
            buffer = buffer.SliceAlign(Format.Channels);

            int SampleLengthOut = (int)((uint)buffer.Length / ChannelsDivisor);
            int internalBufferLengthRequired = CheckBuffer(channels, SampleLengthOut);

            //Resampling start
            var srcBuffer = bufferWrapper.Buffer.Slice(0, internalBufferLengthRequired);
            int lengthReserved = channels * framesReserved;
            var readBuffer = srcBuffer.Slice(lengthReserved).SliceAlign(ChannelsDivisor);
            var rr = Source.Read(readBuffer);

            #endregion Initialize and Read

            if (rr.HasData || readBuffer.Length == 0)
            {
                return Process(buffer, channels, SampleLengthOut, srcBuffer, lengthReserved, readBuffer, rr);
            }
            else
            {
                if (rr.IsEndOfStream)
                {
                    isEndOfStream = true;
                    readBuffer.FastFill(0);
                    //Process the last block with silence connected behind it.
                    return Process(buffer, channels, SampleLengthOut, srcBuffer, lengthReserved, readBuffer, readBuffer.Length);
                }
                else
                {
                    return ReadResult.WaitingForSource;
                }
            }
        }

        private ReadResult Process(Span<float> buffer, int channels, int sampleLengthOut, Span<float> srcBuffer, int lengthReserved, Span<float> readBuffer, ReadResult rr)
        {
            if (rr.Length < readBuffer.Length)   //The input result was not as long as the buffer we gave
            {
                int v = sampleLengthOut * RateDiv + conversionGradient;
                var h = RateMulDivisor.DivRem((uint)v, out var b);
                int readSamples = rr.Length + lengthReserved;
                srcBuffer = srcBuffer.SliceWhile(readSamples).SliceAlign(ChannelsDivisor);
                var framesAvailable = (int)((uint)readSamples / ChannelsDivisor);
                var bA = framesAvailable - 3 - (h > 0 ? 1 : 0);
                var vA = h + bA * RateMul;
                var outLenFrames = (int)((uint)vA / RateDivDivisor);
                buffer = buffer.SliceWhile(outLenFrames * channels).SliceAlign(ChannelsDivisor);
            }
            int lastInputSampleIndex = -1;

            switch (Strategy)
            {
                case ResampleStrategy.Direct:
                    lastInputSampleIndex = ResampleDirect(buffer, channels, srcBuffer);
                    break;
                case ResampleStrategy.CachedDirect:
                    lastInputSampleIndex = ResampleCachedDirect(buffer, channels, srcBuffer);
                    break;
                case ResampleStrategy.CachedWrappedOdd:
                    lastInputSampleIndex = ResampleCachedWrappedOdd(buffer, channels, srcBuffer);
                    break;
                case ResampleStrategy.CachedWrappedEven:
                    lastInputSampleIndex = ResampleCachedWrappedEven(buffer, channels, srcBuffer);
                    break;
            }
            Span<float> reservingRegion = srcBuffer.Slice(lastInputSampleIndex * channels);
            reservingRegion.CopyTo(srcBuffer);
            framesReserved = (int)((uint)reservingRegion.Length / ChannelsDivisor);
#if false   //For Test purpose only
                int lisi_tail = lastInputSampleIndex + 3 - (conversionGradient < RateDiv ? 1 : 0);
                Console.WriteLine($"inputLength:{srcBuffer.Length}, " +
                    $"lastInputSampleIndex: {lisi_tail}(value:{srcBuffer[lisi_tail]}), " +
                    $"nextFirstSampleIndex: {lastInputSampleIndex}(value:{srcBuffer[lastInputSampleIndex]}), " +
                    $"conversionGradient: {conversionGradient}, " +
                    $"framesReserved:{framesReserved}");
#endif

            return buffer.Length;
        }

        #region Resample
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private int ResampleCachedDirectMonaural(Span<float> buffer, Span<float> srcBuffer, ref Vector4 coeffPtr, ref int x, int ram, int acc, int facc)
        {
#if NETCOREAPP3_1_OR_GREATER
            return ResampleCachedDirectMonauralX86(buffer, srcBuffer, ref coeffPtr, ref x, ram, acc, facc);
#endif
            return ResampleCachedDirectMonauralStandard(buffer, srcBuffer, ref coeffPtr, ref x, ram, acc, facc);
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static int ResampleCachedDirectMonauralStandard(Span<float> buffer, Span<float> srcBuffer, ref Vector4 coeffPtr, ref int x, int ram, int acc, int facc)
        {
            nint i = 0;
            nint length = buffer.Length;
            int isx = 0;
            int psx = x;
            ref var src = ref MemoryMarshal.GetReference(srcBuffer);
            ref var dst = ref MemoryMarshal.GetReference(buffer);
            Vector4 values = Unsafe.As<float, Vector4>(ref src);
            if(facc > 0)
            {
                for (i = 0; i < length; i++)
                {
                    var cutmullCoeffs = Unsafe.Add(ref coeffPtr, psx);
                    // Use formula from http://www.mvps.org/directx/articles/catmull/
                    Unsafe.Add(ref dst, i) = VectorUtils.FastDotProduct(values, cutmullCoeffs);
                    psx += acc;
                    isx += facc;
                    if (psx >= ram)
                    {
                        psx -= ram;
                        isx++;
                    }
                    values = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref src, isx));
                }
            }
            else
            {
                if(acc == 1)
                {
                    switch (ram)
                    {
                        case 2:
                            {
                                var c0 = Unsafe.Add(ref coeffPtr, 0);
                                var c1 = Unsafe.Add(ref coeffPtr, 1);
                                if (psx > 0)
                                {
                                    var cutmullCoeffs = c1;
                                    // Use formula from http://www.mvps.org/directx/articles/catmull/
                                    Unsafe.Add(ref dst, i) = VectorUtils.FastDotProduct(values, cutmullCoeffs);
                                    psx = 0;
                                    isx++;
                                    values = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref src, isx));
                                }
                                for (i = 0; i < length - 1; i += 2)
                                {
                                    // Use formula from http://www.mvps.org/directx/articles/catmull/
                                    Unsafe.Add(ref dst, i) = VectorUtils.FastDotProduct(values, c0);
                                    Unsafe.Add(ref dst, i + 1) = VectorUtils.FastDotProduct(values, c1);
                                    isx++;
                                    values = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref src, isx));
                                }
                                for (; i < length; i++)
                                {
                                    var cutmullCoeffs = Unsafe.Add(ref coeffPtr, psx);
                                    // Use formula from http://www.mvps.org/directx/articles/catmull/
                                    Unsafe.Add(ref dst, i) = VectorUtils.FastDotProduct(values, cutmullCoeffs);
                                    psx++;
                                    if (psx >= 2)
                                    {
                                        psx -= 2;
                                        isx++;
                                        values = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref src, isx));
                                    }
                                }
                            }
                            break;
                        case 4:
                            {
                                var c0 = coeffPtr;
                                var c1 = Unsafe.Add(ref coeffPtr, 1);
                                var c2 = Unsafe.Add(ref coeffPtr, 2);
                                var c3 = Unsafe.Add(ref coeffPtr, 3);
                                for (i = 0; psx != 0 && psx < 4; i++, psx++)
                                {
                                    var cutmullCoeffs = Unsafe.Add(ref coeffPtr, psx);
                                    Unsafe.Add(ref dst, i) = VectorUtils.FastDotProduct(values, cutmullCoeffs);
                                    psx++;
                                    if (psx >= 4)
                                    {
                                        psx -= 4;
                                        isx++;
                                        values = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref src, isx));
                                    }
                                }
                                for (; i < length - 3; i += 4)
                                {
                                    Unsafe.Add(ref dst, i) = VectorUtils.FastDotProduct(values, c0);
                                    Unsafe.Add(ref dst, i + 1) = VectorUtils.FastDotProduct(values, c1);
                                    Unsafe.Add(ref dst, i + 2) = VectorUtils.FastDotProduct(values, c2);
                                    Unsafe.Add(ref dst, i + 3) = VectorUtils.FastDotProduct(values, c3);
                                    isx++;
                                    values = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref src, isx));
                                }
                                for (; i < length; i++)
                                {
                                    var cutmullCoeffs = Unsafe.Add(ref coeffPtr, psx);
                                    Unsafe.Add(ref dst, i) = VectorUtils.FastDotProduct(values, cutmullCoeffs);
                                    psx++;
                                    if (psx >= 4)
                                    {
                                        psx -= ram;
                                        isx++;
                                        values = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref src, isx));
                                    }
                                }
                            }
                            break;
                        default:
                            for (i = 0; i < length; i++)
                            {
                                var cutmullCoeffs = Unsafe.Add(ref coeffPtr, psx);
                                // Use formula from http://www.mvps.org/directx/articles/catmull/
                                Unsafe.Add(ref dst, i) = VectorUtils.FastDotProduct(values, cutmullCoeffs);
                                psx++;
                                if (psx >= ram)
                                {
                                    psx -= ram;
                                    isx++;
                                    values = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref src, isx));
                                }
                            }
                            break;
                    }
                }
                else
                {
                    for (i = 0; i < length; i++)
                    {
                        var cutmullCoeffs = Unsafe.Add(ref coeffPtr, psx);
                        // Use formula from http://www.mvps.org/directx/articles/catmull/
                        Unsafe.Add(ref dst, i) = VectorUtils.FastDotProduct(values, cutmullCoeffs);
                        psx += acc;
                        if (psx >= ram)
                        {
                            psx -= ram;
                            isx++;
                            values = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref src, isx));
                        }
                    }
                }
            }
            x = psx;
            return isx;
        }

        private int ResampleCachedDirect2Channels(Span<float> buffer, Span<float> srcBuffer, ref Vector4 coeffPtr, ref int x, int ram, int acc, int facc)
        {
#if NETCOREAPP3_1_OR_GREATER
            return ResampleCachedDirect2ChannelsX86(buffer, srcBuffer, ref coeffPtr, ref x, ram, acc, facc);
#endif
            return ResampleCachedDirect2ChannelsStandard(buffer, srcBuffer, ref coeffPtr, ref x, ram, acc, facc);
        }
        private int ResampleCachedDirect3Channels(Span<float> buffer, Span<float> srcBuffer, ref Vector4 coeffPtr, ref int x, int ram, int acc, int facc)
        {
            return ResampleCachedDirect3ChannelsStandard(buffer, srcBuffer, ref coeffPtr, ref x, ram, acc, facc);
        }
        private int ResampleCachedDirect4Channels(Span<float> buffer, Span<float> srcBuffer, ref Vector4 coeffPtr, ref int x, int ram, int acc, int facc)
        {
            return ResampleCachedDirect4ChannelsStandard(buffer, srcBuffer, ref coeffPtr, ref x, ram, acc, facc);
        }
        #endregion


        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void ActualDispose(bool disposing)
        {
            if (disposing)
            {
            }
            Source.Dispose();
            bufferWrapper.Dispose();
            //sampleCache = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint Abs(int value)
        {
            //TODO: get outside
            int mask = value >> 31;
            return (uint)((value + mask) ^ mask);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector4 CalculateCatmullRomCoeffs(float x)
        {
            //Horner's Method for Catmull-Rom Coeffs
            var vx = new Vector4(x);
            var y = new Vector4(-0.5f, 1.5f, -1.5f, 0.5f);
            y *= vx;
            y += new Vector4(1.0f, -2.5f, 2.0f, -0.5f);
            y *= vx;
            y += new Vector4(-0.5f, 0.0f, 0.5f, 0.0f);
            y *= vx;
            y += new Vector4(0.0f, 1.0f, 0.0f, 0.0f);
            return y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int AdvanceConversionGradient(ref int conversionGradient)
        {
            unchecked
            {
                conversionGradient += GradientIncrement;
                var t = IndexIncrement;
                if (conversionGradient >= RateMul)
                {
                    t++;
                }
                return t;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int AdvanceConversionGradient(ref int conversionGradient, int rad, int ram)
        {
            unchecked
            {
                conversionGradient += rad;
                int l = 0;
                while (conversionGradient >= ram)
                {
                    conversionGradient -= ram;
                    l++;
                }
                return l;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int CheckBuffer(int channels, int sampleLengthOut)
        {
            //Internal buffer length check and some boundary-sample copies
            //Reserving some samples ahead
            //Row 1: #=to read $=not to read %=read and copied to $s
            //$$$############################%%%%
            // ^ process head                 ^process tail
            int v = sampleLengthOut * RateDiv + conversionGradient;
            var h = RateMulDivisor.DivRem((uint)v, out var b);
            int samplesRequired = (int)b + 3 + (h > 0 ? 1 : 0);
            int internalBufferLengthRequired = samplesRequired * channels;
            if (internalBufferLengthRequired > bufferWrapper.Buffer.Length)
            {
                ExpandBuffer(internalBufferLengthRequired);
            }

            return internalBufferLengthRequired;
        }

        private void ExpandBuffer(int internalBufferLengthRequired)
        {
            int lengthReserved = framesReserved * Channels;
            Span<float> a = stackalloc float[lengthReserved];

            if (bufferWrapper.Buffer.Length > lengthReserved) bufferWrapper.Buffer.Slice(0, lengthReserved).CopyTo(a);
            bufferWrapper.Resize(internalBufferLengthRequired);
            a.CopyTo(bufferWrapper.Buffer);
        }
    }
}
