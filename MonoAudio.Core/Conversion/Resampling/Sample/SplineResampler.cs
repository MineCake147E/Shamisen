using System;
using System.Collections.Generic;
using System.Text;
using static System.Runtime.InteropServices.MemoryMarshal;
using MonoAudio.Filters;
using MonoAudio.Formats;
using MonoAudio.Utils;
using System.Numerics;
using System.Runtime.CompilerServices;

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
        private float[][] SampleCache;

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
            SampleCache = new float[3][];
            for (int i = 0; i < SampleCache.Length; i++)
            {
                SampleCache[i] = new float[Channels];
            }
        }

        /// <summary>
        /// Reads the audio to the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>The length of the data written.</returns>
        public override int Read(Span<float> buffer)
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

            Resample(buffer, channels, srcBuffer);
            srcBuffer.Slice(srcBuffer.Length - channels * 3, channels * 3).CopyTo(srcBuffer);
            return buffer.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int CheckBuffer(int channels, int SampleLengthOut)
        {
            //Internal buffer length check and some boundary-sample copies
            //Reserving some samples ahead
            //Row 1: #=to read $=not to read %=read and copied to $s
            //$$$############################%%%
            // ^ process head                 ^process tail
            int internalBufferLengthRequired = (GetCeiledInputPosition(SampleLengthOut) + 3) * channels;

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
            SampleCache = null;
        }
    }
}
