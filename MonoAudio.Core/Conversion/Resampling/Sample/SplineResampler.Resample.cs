
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
    public sealed partial class SplineResampler
    {
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

            int outputSamplePosition = 0;
            // Use formula from http://www.mvps.org/directx/articles/catmull/

            if (channels == Vector<float>.Count)    //SIMD Optimized Multi-Channel Audio Resampling
            {
                var vBuffer = Cast<float, Vector<float>>(buffer);
                var vSrcBuffer = Cast<float, Vector<float>>(srcBuffer);
                for (int i = 0; i < vBuffer.Length; i++)
                {
                    (var inputSamplePosition, var x) = GetConversionGradient(i);
                    int inputSampleIndex = inputSamplePosition;
                    if (x == 0)
                    {
                        //srcBuffer.Slice(inputSampleIndex + channels, channels).CopyTo(buffer.Slice(i));
                        vBuffer[i] = vSrcBuffer[inputSampleIndex + 1];
                    }
                    else
                    {
                        float xP2 = x * x;
                        float xP3 = xP2 * x;
                        var value1 = vSrcBuffer[inputSampleIndex];   //The control point 1.
                        var value2 = vSrcBuffer[inputSampleIndex + 1];   //The control point 2.
                        var value3 = vSrcBuffer[inputSampleIndex + 2];   //The control point 3.
                        var value4 = vSrcBuffer[inputSampleIndex + 3];   //The control point 4.

                        // Use formula from http://www.mvps.org/directx/articles/catmull/
                        vBuffer[i] = 0.5f * (
									2.0f * value2 +
									(-value1 + value3) * x +
									(2.0f * value1 - 5.0f * value2 + 4.0f * value3 - value4) * xP2 +
									(3.0f * value2 - value1 - 3.0f * value3 + value4) * xP3);
					}
                }
            }
            else
            {
                switch (channels)
                {
                    case 1: //Monaural
                        for (int i = 0; i < buffer.Length; i++)
                        {
                            (var inputSamplePosition, var x) = GetConversionGradient(i);
                            int inputSampleIndex = inputSamplePosition;
                            if (x == 0)
                            {
                                //srcBuffer.Slice(inputSampleIndex + channels, channels).CopyTo(buffer.Slice(i));
                                buffer[i] = srcBuffer[inputSampleIndex + 1];
                            }
                            else
                            {
                                float xP2 = x * x;
                                float xP3 = xP2 * x;
                                var value1 = srcBuffer[inputSampleIndex];   //The control point 1.
                                var value2 = srcBuffer[inputSampleIndex + 1];   //The control point 2.
                                var value3 = srcBuffer[inputSampleIndex + 2];   //The control point 3.
                                var value4 = srcBuffer[inputSampleIndex + 3];   //The control point 4.

                                // Use formula from http://www.mvps.org/directx/articles/catmull/
                                buffer[i] = 0.5f * (
											2.0f * value2 +
											(-value1 + value3) * x +
											(2.0f * value1 - 5.0f * value2 + 4.0f * value3 - value4) * xP2 +
											(3.0f * value2 - value1 - 3.0f * value3 + value4) * xP3);
                            }
                        }
                        break;

                    #region SIMD Optimized Multi-Channel Audio Resampling
					case 2:
                        {
                            var vBuffer = Cast<float, Vector2>(buffer);
                            var vSrcBuffer = Cast<float, Vector2>(srcBuffer);
                            for (int i = 0; i < vBuffer.Length; i++)
                            {
                                (var inputSamplePosition, var x) = GetConversionGradient(i);
                                int inputSampleIndex = inputSamplePosition;
                                if (x == 0)
                                {
                                    //srcBuffer.Slice(inputSampleIndex + channels, channels).CopyTo(buffer.Slice(i));
                                    vBuffer[i] = vSrcBuffer[inputSampleIndex + 1];
                                }
                                else
                                {
                                    float xP2 = x * x;
                                    float xP3 = xP2 * x;
                                    var value1 = vSrcBuffer[inputSampleIndex];   //The control point 1.
                                    var value2 = vSrcBuffer[inputSampleIndex + 1];   //The control point 2.
                                    var value3 = vSrcBuffer[inputSampleIndex + 2];   //The control point 3.
                                    var value4 = vSrcBuffer[inputSampleIndex + 3];   //The control point 4.

                                    // Use formula from http://www.mvps.org/directx/articles/catmull/
                                    vBuffer[i] = 0.5f * (
												2.0f * value2 +
												(-value1 + value3) * x +
												(2.0f * value1 - 5.0f * value2 + 4.0f * value3 - value4) * xP2 +
												(3.0f * value2 - value1 - 3.0f * value3 + value4) * xP3);
                                }
                            }
                        }
                        break;
					case 3:
                        {
                            var vBuffer = Cast<float, Vector3>(buffer);
                            var vSrcBuffer = Cast<float, Vector3>(srcBuffer);
                            for (int i = 0; i < vBuffer.Length; i++)
                            {
                                (var inputSamplePosition, var x) = GetConversionGradient(i);
                                int inputSampleIndex = inputSamplePosition;
                                if (x == 0)
                                {
                                    //srcBuffer.Slice(inputSampleIndex + channels, channels).CopyTo(buffer.Slice(i));
                                    vBuffer[i] = vSrcBuffer[inputSampleIndex + 1];
                                }
                                else
                                {
                                    float xP2 = x * x;
                                    float xP3 = xP2 * x;
                                    var value1 = vSrcBuffer[inputSampleIndex];   //The control point 1.
                                    var value2 = vSrcBuffer[inputSampleIndex + 1];   //The control point 2.
                                    var value3 = vSrcBuffer[inputSampleIndex + 2];   //The control point 3.
                                    var value4 = vSrcBuffer[inputSampleIndex + 3];   //The control point 4.

                                    // Use formula from http://www.mvps.org/directx/articles/catmull/
                                    vBuffer[i] = 0.5f * (
												2.0f * value2 +
												(-value1 + value3) * x +
												(2.0f * value1 - 5.0f * value2 + 4.0f * value3 - value4) * xP2 +
												(3.0f * value2 - value1 - 3.0f * value3 + value4) * xP3);
                                }
                            }
                        }
                        break;
					case 4:
                        {
                            var vBuffer = Cast<float, Vector4>(buffer);
                            var vSrcBuffer = Cast<float, Vector4>(srcBuffer);
                            for (int i = 0; i < vBuffer.Length; i++)
                            {
                                (var inputSamplePosition, var x) = GetConversionGradient(i);
                                int inputSampleIndex = inputSamplePosition;
                                if (x == 0)
                                {
                                    //srcBuffer.Slice(inputSampleIndex + channels, channels).CopyTo(buffer.Slice(i));
                                    vBuffer[i] = vSrcBuffer[inputSampleIndex + 1];
                                }
                                else
                                {
                                    float xP2 = x * x;
                                    float xP3 = xP2 * x;
                                    var value1 = vSrcBuffer[inputSampleIndex];   //The control point 1.
                                    var value2 = vSrcBuffer[inputSampleIndex + 1];   //The control point 2.
                                    var value3 = vSrcBuffer[inputSampleIndex + 2];   //The control point 3.
                                    var value4 = vSrcBuffer[inputSampleIndex + 3];   //The control point 4.

                                    // Use formula from http://www.mvps.org/directx/articles/catmull/
                                    vBuffer[i] = 0.5f * (
												2.0f * value2 +
												(-value1 + value3) * x +
												(2.0f * value1 - 5.0f * value2 + 4.0f * value3 - value4) * xP2 +
												(3.0f * value2 - value1 - 3.0f * value3 + value4) * xP3);
                                }
                            }
                        }
                        break;

                    #endregion SIMD Optimized Multi-Channel Audio Resampling

                    default:

                        #region Channels that is not SIMD optimized

                        {
                            unsafe
                            {
                                fixed (float* srcBufPtr = srcBuffer)
                                {
                                    for (int i = 0; i < buffer.Length; i += channels)
                                    {
                                        (var inputSamplePosition, var x) = GetConversionGradient(outputSamplePosition);
                                        int inputSampleIndex = inputSamplePosition * channels;
                                        if (x == 0)
                                        {
                                            srcBuffer.Slice(inputSampleIndex + channels, channels).CopyTo(buffer.Slice(i));
                                        }
                                        else
                                        {
                                            float xP2 = x * x;
                                            float xP3 = xP2 * x;
                                            var cache = srcBufPtr + inputSampleIndex;

                                            for (int ch = 0; ch < channels; ch++)
                                            {
                                                ref var destSample = ref buffer[i + ch];    //Persist the reference in order to eliminate boundary checks.
                                                var value1 = cache[ch];   //The control point 1.
                                                var value2 = cache[channels + ch];   //The control point 2.
                                                var value3 = cache[channels * 2 + ch];   //The control point 3.
                                                var value4 = cache[channels * 3 + ch];   //The control point 4.

                                                // Use formula from http://www.mvps.org/directx/articles/catmull/
                                                destSample = 0.5f * (
														2.0f * value2 +
														(-value1 + value3) * x +
														(2.0f * value1 - 5.0f * value2 + 4.0f * value3 - value4) * xP2 +
														(3.0f * value2 - value1 - 3.0f * value3 + value4) * xP3);
                                            }
                                        }
                                        outputSamplePosition++;
                                    }
                                }
                            }
                        }

                        #endregion Channels that is not SIMD optimized

                        break;
                }
            }

            srcBuffer.Slice(srcBuffer.Length - channels * 3, channels * 3).CopyTo(srcBuffer);
            return buffer.Length;
        }

	}
}
