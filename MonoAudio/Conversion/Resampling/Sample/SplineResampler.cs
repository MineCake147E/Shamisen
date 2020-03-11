using System;
using System.Collections.Generic;
using System.Text;
using static System.Runtime.InteropServices.MemoryMarshal;
using MonoAudio.Filters;
using MonoAudio.Utils;
using System.Numerics;
using System.Runtime.CompilerServices;
using MonoAudio.Mathematics;

namespace MonoAudio.Conversion.Resampling.Sample
{
    /// <summary>
    /// Performs up-sampling using Catmull-Rom Spline interpolation.
    ///
    /// </summary>
    /// <seealso cref="ResamplerBase" />
    public sealed partial class SplineResampler : ResamplerBase
    {
        private ResizableBufferWrapper<float> bufferWrapper;
        private float[][] sampleCache;

        /// <summary>
        /// The pre calculated catmull-rom coefficents.<br/>
        /// X: The coefficent for value1 ((-xP3 + 2 * xP2 - x) * 0.5f)<br/>
        /// Y: The coefficent for value2 (((3 * xP3) - (5 * xP2) + 2) * 0.5f)<br/>
        /// Z: The coefficent for value3 ((-(3 * xP3) + 4 * xP2 + x) * 0.5f)<br/>
        /// W: The coefficent for value4 ((xP3 - xP2) * 0.5f)<br/>
        /// </summary>
        private Vector4[] preCalculatedCatmullRomCoefficents;

        private int conversionGradient = 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int AdvanceConversionGradient(ref int conversionGradient)
        {
            unchecked
            {
                conversionGradient += RateDiv;
                if (conversionGradient >= RateMul)
                {
                    conversionGradient = (int)RateMulDivisor.DivRem((uint)conversionGradient, out var posDiff);
                    return (int)posDiff;
                }
                else
                {
                    return 0;
                }
            }
        }

        [Obsolete("", true)]
        private bool IsCatmullRomOptimized { get; }

        private ResampleStrategy Strategy { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SplineResampler"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="destinationSampleRate">The destination sample rate.</param>
        public SplineResampler(IReadableAudioSource<float, SampleFormat> source, int destinationSampleRate) :
            base(source.Format.SampleRate > destinationSampleRate
                ? new BiQuadFilter(source, BiQuadParameter.CreateLPFParameter(source.Format.SampleRate, destinationSampleRate * 0.5, 0.70710678118654752440084436210485))
                : source, destinationSampleRate)
        {
            bufferWrapper = new ResizablePooledBufferWrapper<float>(1);
            sampleCache = new float[3][];
            for (int i = 0; i < sampleCache.Length; i++)
            {
                sampleCache[i] = new float[Channels];
            }
            if (source.Format.SampleRate < destinationSampleRate)
            {
                if (RateMul < 512)
                {
                    Strategy = ResampleStrategy.CachedDirect;
                    preCalculatedCatmullRomCoefficents = new Vector4[RateMul];
                    for (int i = 0; i < preCalculatedCatmullRomCoefficents.Length; i++)
                    {
                        var x = i * RateMulInverse;
                        preCalculatedCatmullRomCoefficents[i] = CalculateCatmullRomCoeffs(x);
                    }
                }
                else if (RateMul < 1024)
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector4 CalculateCatmullRomCoeffs(float x)
        {
            var xP2 = x * x;
            var xP3 = xP2 * x;
            return new Vector4(
                (-xP3 + (2 * xP2) - x) * 0.5f,
                ((3 * xP3) - (5 * xP2) + 2) * 0.5f,
                (-(3 * xP3) + (4 * xP2) + x) * 0.5f,
                (xP3 - xP2) * 0.5f);
        }

        /// <summary>
        /// Reads the audio to the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>The length of the data written.</returns>
        public override ReadResult Read(Span<float> buffer)
        {
            int channels = Channels;

            #region Initialize and Read

            //Align the length of the buffer.
            buffer = buffer.SliceAlign(Format.Channels);

            int SampleLengthOut = buffer.Length / channels;
            int internalBufferLengthRequired = CheckBuffer(channels, SampleLengthOut);

            //Resampling start
            Span<float> srcBuffer = bufferWrapper.Buffer.Slice(0, internalBufferLengthRequired);
            Source.Read(srcBuffer.Slice(channels * 3));

            #endregion Initialize and Read

            var strategy = Strategy;
            switch (strategy)
            {
                case ResampleStrategy.Direct:
                    ResampleDirect(ref buffer, channels, ref srcBuffer);
                    break;
                case ResampleStrategy.CachedDirect:
                    ResampleCachedDirect(ref buffer, channels, ref srcBuffer);
                    break;
                case ResampleStrategy.CachedWrappedOdd:
                    ResampleCachedWrappedOdd(ref buffer, channels, ref srcBuffer);
                    break;
                case ResampleStrategy.CachedWrappedEven:
                    ResampleCachedWrappedEven(ref buffer, channels, ref srcBuffer);
                    break;
            }
            srcBuffer.Slice(srcBuffer.Length - channels * 3, channels * 3).CopyTo(srcBuffer);
            return buffer.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int CheckBuffer(int channels, int sampleLengthOut)
        {
            //Internal buffer length check and some boundary-sample copies
            //Reserving some samples ahead
            //Row 1: #=to read $=not to read %=read and copied to $s
            //$$$############################%%%
            // ^ process head                 ^process tail
            int internalBufferLengthRequired = (GetCeiledInputPosition(sampleLengthOut) + 3) * channels;

            if (internalBufferLengthRequired > bufferWrapper.Buffer.Length)
            {
                ExpandBuffer(internalBufferLengthRequired);
            }

            return internalBufferLengthRequired;
        }

        private void ExpandBuffer(int internalBufferLengthRequired)
        {
            Span<float> a = stackalloc float[3 * Channels];
            if (bufferWrapper.Buffer.Length > 3 * Channels) bufferWrapper.Buffer.Slice(0, Channels * 3).CopyTo(a);
            bufferWrapper.Resize(internalBufferLengthRequired);
            a.CopyTo(bufferWrapper.Buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint Abs(int value)
        {
            //TODO: get outside
            int mask = value >> 31;
            return (uint)((value + mask) ^ mask);
        }

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
            sampleCache = null;
        }
    }
}
