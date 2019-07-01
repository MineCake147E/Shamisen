using System;
using System.Collections.Generic;
using System.Text;
using MonoAudio.Filters;
using MonoAudio.Formats;
using MonoAudio.Utils;

namespace MonoAudio.Conversion.Resampling.Sample
{
    /// <summary>
    /// Performs up-sampling using Catmull-Rom Spline interpolation.
    ///
    /// </summary>
    /// <seealso cref="MonoAudio.Conversion.Resampling.Sample.ResamplerBase" />
    public sealed class SplineResampler : ResamplerBase
    {
        private ResizableBufferWrapper<float> bufferWrapper;

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
        }

        /// <summary>
        /// Reads the audio to the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>The length of the data written.</returns>
        public override int Read(Span<float> buffer)
        {
            //Align the length of the buffer.
            buffer = buffer.SliceAlign(Format.Channels);
            int channels = Channels;
            int SampleLengthOut = buffer.Length / channels;

            //Internal buffer length check and some boundary-sample copies
            //Reserving 1 sample ahead #=to read $=not to read %=read and copied to $s
            //$$$########%%%
            int internalBufferLengthRequired = (GetCeiledInputPosition(SampleLengthOut) + 3) * channels;

            if (internalBufferLengthRequired > bufferWrapper.Buffer.Length)
            {
                ExpandBuffer(internalBufferLengthRequired);
            }

            //Resampling start
            Span<float> srcBuffer = bufferWrapper.Buffer.Slice(0, internalBufferLengthRequired);
            Source.Read(srcBuffer.Slice(channels * 3));
            int outputSamplePosition = 0;
            for (int i = 0; i < buffer.Length; i += channels)
            {
                (var inputSamplePosition, var amount) = GetConversionGradient(outputSamplePosition);
                int inputSampleIndex = inputSamplePosition * channels;
                if (amount == 0)
                {
                    srcBuffer.Slice(inputSampleIndex + channels, channels).CopyTo(buffer.Slice(i));
                }
                else
                {
                    float amountP2 = amount * amount;
                    float amountP3 = amountP2 * amount;
                    for (int ch = 0; ch < channels; ch++)
                    {
                        ref var destSample = ref buffer[i + ch];    //Persist the reference in order to eliminate boundary checks.
                        int posIn = inputSampleIndex + ch;
                        var value1 = srcBuffer[posIn];   //The control point 1.
                        var value2 = srcBuffer[posIn + channels];   //The control point 2.
                        var value3 = srcBuffer[posIn + 2 * channels];   //The control point 3.
                        var value4 = srcBuffer[posIn + 3 * channels];   //The control point 4.

                        // Use formula from http://www.mvps.org/directx/articles/catmull/
                        destSample = 0.5f * (2.0f * value2 + (value3 - value1) * amount +
                            (2.0f * value1 - 5.0f * value2 + 4.0f * value3 - value4) * amountP2 +
                            (3.0f * value2 - value1 - 3.0f * value3 + value4) * amountP3);
                    }
                }
                outputSamplePosition++;
            }
            srcBuffer.Slice(srcBuffer.Length - channels * 3, channels * 3).CopyTo(srcBuffer);
            return buffer.Length;
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
        }
    }
}
