
using System;
using System.Collections.Generic;
using System.Text;
using static System.Runtime.InteropServices.MemoryMarshal;
using System.Numerics;
using System.Runtime.CompilerServices;
using MonoAudio.Filters;
using MonoAudio.Formats;
using MonoAudio.Utils;

namespace MonoAudio.Conversion.Resampling.Sample {
    /// <summary>
    /// Performs up-sampling using Catmull-Rom Spline interpolation.
    ///
    /// </summary>
    /// <seealso cref="ResamplerBase" />
    public sealed partial class SplineResampler {
        private void ResampleCachedDirect (ref Span<float> buffer, int channels, ref Span<float> srcBuffer) {
            ref var coeffPtr = ref GetReference (preCalculatedCatmullRomCoefficents.AsSpan ());
            int outputSamplePosition = 0;
            // Use formula from http://www.mvps.org/directx/articles/catmull/

            if (channels == Vector<float>.Count) //SIMD Optimized Multi-Channel Audio Resampling
            {
                var vBuffer = Cast<float, Vector<float>> (buffer);
                var vSrcBuffer = Cast<float, Vector<float>> (srcBuffer);
                for (int i = 0; i < vBuffer.Length; i++) {
                    (var inputSamplePosition, var x) = GetConversionPosition (i);
                    int inputSampleIndex = inputSamplePosition;
                    if (x == 0) {
                        //srcBuffer.Slice(inputSampleIndex + channels, channels).CopyTo(buffer.Slice(i));
                        vBuffer[i] = vSrcBuffer[inputSampleIndex + 1];
                    } else {
                        ref var values = ref Unsafe.As < Vector<float>,
                            (Vector<float> X, Vector<float> Y, Vector<float> Z, Vector<float> W) > (ref vSrcBuffer[inputSampleIndex]);
                        var cutmullCoeffs = Unsafe.Add (ref coeffPtr, x);
                        var value1 = values.X * cutmullCoeffs.X; //The control point 1.
                        var value2 = values.Y * cutmullCoeffs.Y; //The control point 2.
                        var value3 = values.Z * cutmullCoeffs.Z; //The control point 3.
                        var value4 = values.W * cutmullCoeffs.W; //The control point 4.

                        // Use formula from http://www.mvps.org/directx/articles/catmull/
                        vBuffer[i] = value1 + value2 + value3 + value4;
                    }
                }
            } else {
                switch (channels) {
                    case 1: //Monaural
                        for (int i = 0; i < buffer.Length; i++) {
                            (var inputSamplePosition, var x) = GetConversionPosition (i);
                            int inputSampleIndex = inputSamplePosition;
                            if (x == 0) {
                                //srcBuffer.Slice(inputSampleIndex + channels, channels).CopyTo(buffer.Slice(i));
                                buffer[i] = srcBuffer[inputSampleIndex + 1];
                            } else {
                                var values = Unsafe.As<float, Vector4> (ref srcBuffer[inputSampleIndex]);
                                var cutmullCoeffs = Unsafe.Add (ref coeffPtr, x);

                                // Use formula from http://www.mvps.org/directx/articles/catmull/
                                buffer[i] = Vector4.Dot (values, cutmullCoeffs);
                            }
                        }
                        break;

                        #region SIMD Optimized Multi-Channel Audio Resampling
                            case 2 :
                                {
                                    var vBuffer = Cast < float,
                                        Vector2> (buffer);
                                    var vSrcBuffer = Cast < float,
                                        Vector2> (srcBuffer);
                                    for (int i = 0; i < vBuffer.Length; i++) {
                                        (var inputSamplePosition, var x) = GetConversionPosition (i);
                                        int inputSampleIndex = inputSamplePosition;
                                        if (x == 0) {
                                            //srcBuffer.Slice(inputSampleIndex + channels, channels).CopyTo(buffer.Slice(i));
                                            vBuffer[i] = vSrcBuffer[inputSampleIndex + 1];
                                        } else {
                                            ref var values = ref Unsafe.As <Vector2,
                                                ( Vector2 X, Vector2 Y, Vector2 Z, Vector2 W) > (ref vSrcBuffer[inputSampleIndex]);
                                            var cutmullCoeffs = Unsafe.Add (ref coeffPtr, x);
                                            var value1 = values.X * cutmullCoeffs.X; //The control point 1.
                                            var value2 = values.Y * cutmullCoeffs.Y; //The control point 2.
                                            var value3 = values.Z * cutmullCoeffs.Z; //The control point 3.
                                            var value4 = values.W * cutmullCoeffs.W; //The control point 4.
                                            vBuffer[i] = value1 + value2 + value3 + value4;
                                        }
                                    }
                                }
                                break;
                            case 3 :
                                {
                                    var vBuffer = Cast < float,
                                        Vector3> (buffer);
                                    var vSrcBuffer = Cast < float,
                                        Vector3> (srcBuffer);
                                    for (int i = 0; i < vBuffer.Length; i++) {
                                        (var inputSamplePosition, var x) = GetConversionPosition (i);
                                        int inputSampleIndex = inputSamplePosition;
                                        if (x == 0) {
                                            //srcBuffer.Slice(inputSampleIndex + channels, channels).CopyTo(buffer.Slice(i));
                                            vBuffer[i] = vSrcBuffer[inputSampleIndex + 1];
                                        } else {
                                            ref var values = ref Unsafe.As <Vector3,
                                                ( Vector3 X, Vector3 Y, Vector3 Z, Vector3 W) > (ref vSrcBuffer[inputSampleIndex]);
                                            var cutmullCoeffs = Unsafe.Add (ref coeffPtr, x);
                                            var value1 = values.X * cutmullCoeffs.X; //The control point 1.
                                            var value2 = values.Y * cutmullCoeffs.Y; //The control point 2.
                                            var value3 = values.Z * cutmullCoeffs.Z; //The control point 3.
                                            var value4 = values.W * cutmullCoeffs.W; //The control point 4.
                                            vBuffer[i] = value1 + value2 + value3 + value4;
                                        }
                                    }
                                }
                                break;
                            case 4 :
                                {
                                    var vBuffer = Cast < float,
                                        Vector4> (buffer);
                                    var vSrcBuffer = Cast < float,
                                        Vector4> (srcBuffer);
                                    for (int i = 0; i < vBuffer.Length; i++) {
                                        (var inputSamplePosition, var x) = GetConversionPosition (i);
                                        int inputSampleIndex = inputSamplePosition;
                                        if (x == 0) {
                                            //srcBuffer.Slice(inputSampleIndex + channels, channels).CopyTo(buffer.Slice(i));
                                            vBuffer[i] = vSrcBuffer[inputSampleIndex + 1];
                                        } else {
                                            ref var values = ref Unsafe.As <Vector4,
                                                ( Vector4 X, Vector4 Y, Vector4 Z, Vector4 W) > (ref vSrcBuffer[inputSampleIndex]);
                                            var cutmullCoeffs = Unsafe.Add (ref coeffPtr, x);
                                            var value1 = values.X * cutmullCoeffs.X; //The control point 1.
                                            var value2 = values.Y * cutmullCoeffs.Y; //The control point 2.
                                            var value3 = values.Z * cutmullCoeffs.Z; //The control point 3.
                                            var value4 = values.W * cutmullCoeffs.W; //The control point 4.
                                            vBuffer[i] = value1 + value2 + value3 + value4;
                                        }
                                    }
                                }
                                break;

                        #endregion SIMD Optimized Multi-Channel Audio Resampling

                    default:

                        #region Channels that is not SIMD optimized

                        {
                            unsafe {
                                fixed (float * srcBufPtr = srcBuffer) {
                                    for (int i = 0; i < buffer.Length; i += channels) {
                                        (var inputSamplePosition, var x) = GetConversionPosition (outputSamplePosition);
                                        int inputSampleIndex = inputSamplePosition * channels;
                                        if (x == 0) {
                                            srcBuffer.Slice (inputSampleIndex + channels, channels).CopyTo (buffer.Slice (i));
                                        } else {
                                            var cache = srcBufPtr + inputSampleIndex;
                                            var cutmullCoeffs = Unsafe.Add (ref coeffPtr, x);
                                            for (int ch = 0; ch < channels; ch++) {
                                                ref var destSample = ref buffer[i + ch]; //Persist the reference in order to eliminate boundary checks.
                                                var value1 = cache[ch] * cutmullCoeffs.X; //The control point 1.
                                                var value2 = cache[channels + ch] * cutmullCoeffs.Y; //The control point 2.
                                                var value3 = cache[channels * 2 + ch] * cutmullCoeffs.Z; //The control point 3.
                                                var value4 = cache[channels * 3 + ch] * cutmullCoeffs.W; //The control point 4.

                                                // Use formula from http://www.mvps.org/directx/articles/catmull/
                                                destSample = value1 + value2 + value3 + value4;
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
        }
        private void ResampleCachedWrappedOdd (ref Span<float> buffer, int channels, ref Span<float> srcBuffer) {
            ref var coeffPtr = ref GetReference (preCalculatedCatmullRomCoefficents.AsSpan ());
            int outputSamplePosition = 0;
            // Use formula from http://www.mvps.org/directx/articles/catmull/
            Vector4 GetCatmullRomCoefficents (ref Vector4 coeffs, int i) {
                int x = i;
                if (i > RateMul / 2) x = RateMul - i;
                return Unsafe.Add (ref coeffs, x);
            }

            if (channels == Vector<float>.Count) //SIMD Optimized Multi-Channel Audio Resampling
            {
                var vBuffer = Cast<float, Vector<float>> (buffer);
                var vSrcBuffer = Cast<float, Vector<float>> (srcBuffer);
                for (int i = 0; i < vBuffer.Length; i++) {
                    (var inputSamplePosition, var x) = GetConversionPosition (i);
                    int inputSampleIndex = inputSamplePosition;
                    if (x == 0) {
                        //srcBuffer.Slice(inputSampleIndex + channels, channels).CopyTo(buffer.Slice(i));
                        vBuffer[i] = vSrcBuffer[inputSampleIndex + 1];
                    } else {
                        ref var values = ref Unsafe.As < Vector<float>,
                            (Vector<float> X, Vector<float> Y, Vector<float> Z, Vector<float> W) > (ref vSrcBuffer[inputSampleIndex]);
                        var cutmullCoeffs = GetCatmullRomCoefficents (ref coeffPtr, x);
                        var value1 = values.X * cutmullCoeffs.X; //The control point 1.
                        var value2 = values.Y * cutmullCoeffs.Y; //The control point 2.
                        var value3 = values.Z * cutmullCoeffs.Z; //The control point 3.
                        var value4 = values.W * cutmullCoeffs.W; //The control point 4.

                        // Use formula from http://www.mvps.org/directx/articles/catmull/
                        vBuffer[i] = value1 + value2 + value3 + value4;
                    }
                }
            } else {
                switch (channels) {
                    case 1: //Monaural
                        for (int i = 0; i < buffer.Length; i++) {
                            (var inputSamplePosition, var x) = GetConversionPosition (i);
                            int inputSampleIndex = inputSamplePosition;
                            if (x == 0) {
                                //srcBuffer.Slice(inputSampleIndex + channels, channels).CopyTo(buffer.Slice(i));
                                buffer[i] = srcBuffer[inputSampleIndex + 1];
                            } else {
                                var values = Unsafe.As<float, Vector4> (ref srcBuffer[inputSampleIndex]);
                                var cutmullCoeffs = GetCatmullRomCoefficents (ref coeffPtr, x);

                                // Use formula from http://www.mvps.org/directx/articles/catmull/
                                buffer[i] = Vector4.Dot (values, cutmullCoeffs);
                            }
                        }
                        break;

                        #region SIMD Optimized Multi-Channel Audio Resampling
                            case 2 :
                                {
                                    var vBuffer = Cast < float,
                                        Vector2> (buffer);
                                    var vSrcBuffer = Cast < float,
                                        Vector2> (srcBuffer);
                                    for (int i = 0; i < vBuffer.Length; i++) {
                                        (var inputSamplePosition, var x) = GetConversionPosition (i);
                                        int inputSampleIndex = inputSamplePosition;
                                        if (x == 0) {
                                            //srcBuffer.Slice(inputSampleIndex + channels, channels).CopyTo(buffer.Slice(i));
                                            vBuffer[i] = vSrcBuffer[inputSampleIndex + 1];
                                        } else {
                                            ref var values = ref Unsafe.As <Vector2,
                                                ( Vector2 X, Vector2 Y, Vector2 Z, Vector2 W) > (ref vSrcBuffer[inputSampleIndex]);
                                            var cutmullCoeffs = GetCatmullRomCoefficents (ref coeffPtr, x);
                                            var value1 = values.X * cutmullCoeffs.X; //The control point 1.
                                            var value2 = values.Y * cutmullCoeffs.Y; //The control point 2.
                                            var value3 = values.Z * cutmullCoeffs.Z; //The control point 3.
                                            var value4 = values.W * cutmullCoeffs.W; //The control point 4.
                                            vBuffer[i] = value1 + value2 + value3 + value4;
                                        }
                                    }
                                }
                                break;
                            case 3 :
                                {
                                    var vBuffer = Cast < float,
                                        Vector3> (buffer);
                                    var vSrcBuffer = Cast < float,
                                        Vector3> (srcBuffer);
                                    for (int i = 0; i < vBuffer.Length; i++) {
                                        (var inputSamplePosition, var x) = GetConversionPosition (i);
                                        int inputSampleIndex = inputSamplePosition;
                                        if (x == 0) {
                                            //srcBuffer.Slice(inputSampleIndex + channels, channels).CopyTo(buffer.Slice(i));
                                            vBuffer[i] = vSrcBuffer[inputSampleIndex + 1];
                                        } else {
                                            ref var values = ref Unsafe.As <Vector3,
                                                ( Vector3 X, Vector3 Y, Vector3 Z, Vector3 W) > (ref vSrcBuffer[inputSampleIndex]);
                                            var cutmullCoeffs = GetCatmullRomCoefficents (ref coeffPtr, x);
                                            var value1 = values.X * cutmullCoeffs.X; //The control point 1.
                                            var value2 = values.Y * cutmullCoeffs.Y; //The control point 2.
                                            var value3 = values.Z * cutmullCoeffs.Z; //The control point 3.
                                            var value4 = values.W * cutmullCoeffs.W; //The control point 4.
                                            vBuffer[i] = value1 + value2 + value3 + value4;
                                        }
                                    }
                                }
                                break;
                            case 4 :
                                {
                                    var vBuffer = Cast < float,
                                        Vector4> (buffer);
                                    var vSrcBuffer = Cast < float,
                                        Vector4> (srcBuffer);
                                    for (int i = 0; i < vBuffer.Length; i++) {
                                        (var inputSamplePosition, var x) = GetConversionPosition (i);
                                        int inputSampleIndex = inputSamplePosition;
                                        if (x == 0) {
                                            //srcBuffer.Slice(inputSampleIndex + channels, channels).CopyTo(buffer.Slice(i));
                                            vBuffer[i] = vSrcBuffer[inputSampleIndex + 1];
                                        } else {
                                            ref var values = ref Unsafe.As <Vector4,
                                                ( Vector4 X, Vector4 Y, Vector4 Z, Vector4 W) > (ref vSrcBuffer[inputSampleIndex]);
                                            var cutmullCoeffs = GetCatmullRomCoefficents (ref coeffPtr, x);
                                            var value1 = values.X * cutmullCoeffs.X; //The control point 1.
                                            var value2 = values.Y * cutmullCoeffs.Y; //The control point 2.
                                            var value3 = values.Z * cutmullCoeffs.Z; //The control point 3.
                                            var value4 = values.W * cutmullCoeffs.W; //The control point 4.
                                            vBuffer[i] = value1 + value2 + value3 + value4;
                                        }
                                    }
                                }
                                break;

                        #endregion SIMD Optimized Multi-Channel Audio Resampling

                    default:

                        #region Channels that is not SIMD optimized

                        {
                            unsafe {
                                fixed (float * srcBufPtr = srcBuffer) {
                                    for (int i = 0; i < buffer.Length; i += channels) {
                                        (var inputSamplePosition, var x) = GetConversionPosition (outputSamplePosition);
                                        int inputSampleIndex = inputSamplePosition * channels;
                                        if (x == 0) {
                                            srcBuffer.Slice (inputSampleIndex + channels, channels).CopyTo (buffer.Slice (i));
                                        } else {
                                            var cache = srcBufPtr + inputSampleIndex;
                                            var cutmullCoeffs = GetCatmullRomCoefficents (ref coeffPtr, x);
                                            for (int ch = 0; ch < channels; ch++) {
                                                ref var destSample = ref buffer[i + ch]; //Persist the reference in order to eliminate boundary checks.
                                                var value1 = cache[ch] * cutmullCoeffs.X; //The control point 1.
                                                var value2 = cache[channels + ch] * cutmullCoeffs.Y; //The control point 2.
                                                var value3 = cache[channels * 2 + ch] * cutmullCoeffs.Z; //The control point 3.
                                                var value4 = cache[channels * 3 + ch] * cutmullCoeffs.W; //The control point 4.

                                                // Use formula from http://www.mvps.org/directx/articles/catmull/
                                                destSample = value1 + value2 + value3 + value4;
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
        }
        private void ResampleCachedWrappedEven (ref Span<float> buffer, int channels, ref Span<float> srcBuffer) {
            ref var coeffPtr = ref GetReference (preCalculatedCatmullRomCoefficents.AsSpan ());
            int outputSamplePosition = 0;
            // Use formula from http://www.mvps.org/directx/articles/catmull/
            Vector4 GetCatmullRomCoefficents (ref Vector4 coeffs, int i) {
                int x = i;
                if (i >= RateMul / 2) x = RateMul - i - 1;
                return Unsafe.Add (ref coeffs, x);
            }

            if (channels == Vector<float>.Count) //SIMD Optimized Multi-Channel Audio Resampling
            {
                var vBuffer = Cast<float, Vector<float>> (buffer);
                var vSrcBuffer = Cast<float, Vector<float>> (srcBuffer);
                for (int i = 0; i < vBuffer.Length; i++) {
                    (var inputSamplePosition, var x) = GetConversionPosition (i);
                    int inputSampleIndex = inputSamplePosition;
                    if (x == 0) {
                        //srcBuffer.Slice(inputSampleIndex + channels, channels).CopyTo(buffer.Slice(i));
                        vBuffer[i] = vSrcBuffer[inputSampleIndex + 1];
                    } else {
                        ref var values = ref Unsafe.As < Vector<float>,
                            (Vector<float> X, Vector<float> Y, Vector<float> Z, Vector<float> W) > (ref vSrcBuffer[inputSampleIndex]);
                        var cutmullCoeffs = GetCatmullRomCoefficents (ref coeffPtr, x);
                        var value1 = values.X * cutmullCoeffs.X; //The control point 1.
                        var value2 = values.Y * cutmullCoeffs.Y; //The control point 2.
                        var value3 = values.Z * cutmullCoeffs.Z; //The control point 3.
                        var value4 = values.W * cutmullCoeffs.W; //The control point 4.

                        // Use formula from http://www.mvps.org/directx/articles/catmull/
                        vBuffer[i] = value1 + value2 + value3 + value4;
                    }
                }
            } else {
                switch (channels) {
                    case 1: //Monaural
                        for (int i = 0; i < buffer.Length; i++) {
                            (var inputSamplePosition, var x) = GetConversionPosition (i);
                            int inputSampleIndex = inputSamplePosition;
                            if (x == 0) {
                                //srcBuffer.Slice(inputSampleIndex + channels, channels).CopyTo(buffer.Slice(i));
                                buffer[i] = srcBuffer[inputSampleIndex + 1];
                            } else {
                                var values = Unsafe.As<float, Vector4> (ref srcBuffer[inputSampleIndex]);
                                var cutmullCoeffs = GetCatmullRomCoefficents (ref coeffPtr, x);

                                // Use formula from http://www.mvps.org/directx/articles/catmull/
                                buffer[i] = Vector4.Dot (values, cutmullCoeffs);
                            }
                        }
                        break;

                        #region SIMD Optimized Multi-Channel Audio Resampling
                            case 2 :
                                {
                                    var vBuffer = Cast < float,
                                        Vector2> (buffer);
                                    var vSrcBuffer = Cast < float,
                                        Vector2> (srcBuffer);
                                    for (int i = 0; i < vBuffer.Length; i++) {
                                        (var inputSamplePosition, var x) = GetConversionPosition (i);
                                        int inputSampleIndex = inputSamplePosition;
                                        if (x == 0) {
                                            //srcBuffer.Slice(inputSampleIndex + channels, channels).CopyTo(buffer.Slice(i));
                                            vBuffer[i] = vSrcBuffer[inputSampleIndex + 1];
                                        } else {
                                            ref var values = ref Unsafe.As <Vector2,
                                                ( Vector2 X, Vector2 Y, Vector2 Z, Vector2 W) > (ref vSrcBuffer[inputSampleIndex]);
                                            var cutmullCoeffs = GetCatmullRomCoefficents (ref coeffPtr, x);
                                            var value1 = values.X * cutmullCoeffs.X; //The control point 1.
                                            var value2 = values.Y * cutmullCoeffs.Y; //The control point 2.
                                            var value3 = values.Z * cutmullCoeffs.Z; //The control point 3.
                                            var value4 = values.W * cutmullCoeffs.W; //The control point 4.
                                            vBuffer[i] = value1 + value2 + value3 + value4;
                                        }
                                    }
                                }
                                break;
                            case 3 :
                                {
                                    var vBuffer = Cast < float,
                                        Vector3> (buffer);
                                    var vSrcBuffer = Cast < float,
                                        Vector3> (srcBuffer);
                                    for (int i = 0; i < vBuffer.Length; i++) {
                                        (var inputSamplePosition, var x) = GetConversionPosition (i);
                                        int inputSampleIndex = inputSamplePosition;
                                        if (x == 0) {
                                            //srcBuffer.Slice(inputSampleIndex + channels, channels).CopyTo(buffer.Slice(i));
                                            vBuffer[i] = vSrcBuffer[inputSampleIndex + 1];
                                        } else {
                                            ref var values = ref Unsafe.As <Vector3,
                                                ( Vector3 X, Vector3 Y, Vector3 Z, Vector3 W) > (ref vSrcBuffer[inputSampleIndex]);
                                            var cutmullCoeffs = GetCatmullRomCoefficents (ref coeffPtr, x);
                                            var value1 = values.X * cutmullCoeffs.X; //The control point 1.
                                            var value2 = values.Y * cutmullCoeffs.Y; //The control point 2.
                                            var value3 = values.Z * cutmullCoeffs.Z; //The control point 3.
                                            var value4 = values.W * cutmullCoeffs.W; //The control point 4.
                                            vBuffer[i] = value1 + value2 + value3 + value4;
                                        }
                                    }
                                }
                                break;
                            case 4 :
                                {
                                    var vBuffer = Cast < float,
                                        Vector4> (buffer);
                                    var vSrcBuffer = Cast < float,
                                        Vector4> (srcBuffer);
                                    for (int i = 0; i < vBuffer.Length; i++) {
                                        (var inputSamplePosition, var x) = GetConversionPosition (i);
                                        int inputSampleIndex = inputSamplePosition;
                                        if (x == 0) {
                                            //srcBuffer.Slice(inputSampleIndex + channels, channels).CopyTo(buffer.Slice(i));
                                            vBuffer[i] = vSrcBuffer[inputSampleIndex + 1];
                                        } else {
                                            ref var values = ref Unsafe.As <Vector4,
                                                ( Vector4 X, Vector4 Y, Vector4 Z, Vector4 W) > (ref vSrcBuffer[inputSampleIndex]);
                                            var cutmullCoeffs = GetCatmullRomCoefficents (ref coeffPtr, x);
                                            var value1 = values.X * cutmullCoeffs.X; //The control point 1.
                                            var value2 = values.Y * cutmullCoeffs.Y; //The control point 2.
                                            var value3 = values.Z * cutmullCoeffs.Z; //The control point 3.
                                            var value4 = values.W * cutmullCoeffs.W; //The control point 4.
                                            vBuffer[i] = value1 + value2 + value3 + value4;
                                        }
                                    }
                                }
                                break;

                        #endregion SIMD Optimized Multi-Channel Audio Resampling

                    default:

                        #region Channels that is not SIMD optimized

                        {
                            unsafe {
                                fixed (float * srcBufPtr = srcBuffer) {
                                    for (int i = 0; i < buffer.Length; i += channels) {
                                        (var inputSamplePosition, var x) = GetConversionPosition (outputSamplePosition);
                                        int inputSampleIndex = inputSamplePosition * channels;
                                        if (x == 0) {
                                            srcBuffer.Slice (inputSampleIndex + channels, channels).CopyTo (buffer.Slice (i));
                                        } else {
                                            var cache = srcBufPtr + inputSampleIndex;
                                            var cutmullCoeffs = GetCatmullRomCoefficents (ref coeffPtr, x);
                                            for (int ch = 0; ch < channels; ch++) {
                                                ref var destSample = ref buffer[i + ch]; //Persist the reference in order to eliminate boundary checks.
                                                var value1 = cache[ch] * cutmullCoeffs.X; //The control point 1.
                                                var value2 = cache[channels + ch] * cutmullCoeffs.Y; //The control point 2.
                                                var value3 = cache[channels * 2 + ch] * cutmullCoeffs.Z; //The control point 3.
                                                var value4 = cache[channels * 3 + ch] * cutmullCoeffs.W; //The control point 4.

                                                // Use formula from http://www.mvps.org/directx/articles/catmull/
                                                destSample = value1 + value2 + value3 + value4;
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
        }
        private void ResampleDirect (ref Span<float> buffer, int channels, ref Span<float> srcBuffer) {
            int outputSamplePosition = 0;
            // Use formula from http://www.mvps.org/directx/articles/catmull/

            if (channels == Vector<float>.Count) //SIMD Optimized Multi-Channel Audio Resampling
            {
                var vBuffer = Cast<float, Vector<float>> (buffer);
                var vSrcBuffer = Cast<float, Vector<float>> (srcBuffer);
                for (int i = 0; i < vBuffer.Length; i++) {
                    (var inputSamplePosition, var x) = GetConversionGradient (i);
                    int inputSampleIndex = inputSamplePosition;
                    if (x == 0) {
                        //srcBuffer.Slice(inputSampleIndex + channels, channels).CopyTo(buffer.Slice(i));
                        vBuffer[i] = vSrcBuffer[inputSampleIndex + 1];
                    } else {
                        float xP2 = x * x;
                        float xP3 = xP2 * x;
                        ref var values = ref Unsafe.As < Vector<float>,
                            (Vector<float> X, Vector<float> Y, Vector<float> Z, Vector<float> W) > (ref vSrcBuffer[inputSampleIndex]);
                        var value1 = values.X; //The control point 1.
                        var value2 = values.Y; //The control point 2.
                        var value3 = values.Z; //The control point 3.
                        var value4 = values.W; //The control point 4.

                        // Use formula from http://www.mvps.org/directx/articles/catmull/
                        vBuffer[i] = 0.5f * (
                            2.0f * value2 +
                            (-value1 + value3) * x +
                            (2.0f * value1 - 5.0f * value2 + 4.0f * value3 - value4) * xP2 +
                            (3.0f * value2 - value1 - 3.0f * value3 + value4) * xP3);
                    }
                }
            } else {
                switch (channels) {
                    case 1: //Monaural
                        for (int i = 0; i < buffer.Length; i++) {
                            (var inputSamplePosition, var x) = GetConversionGradient (i);
                            int inputSampleIndex = inputSamplePosition;
                            if (x == 0) {
                                //srcBuffer.Slice(inputSampleIndex + channels, channels).CopyTo(buffer.Slice(i));
                                buffer[i] = srcBuffer[inputSampleIndex + 1];
                            } else {
                                float xP2 = x * x;
                                float xP3 = xP2 * x;
                                var values = Unsafe.As<float, Vector4> (ref srcBuffer[inputSampleIndex]);
                                var value1 = values.X; //The control point 1.
                                var value2 = values.Y; //The control point 2.
                                var value3 = values.Z; //The control point 3.
                                var value4 = values.W; //The control point 4.

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
                            case 2 :
                                {
                                    var vBuffer = Cast < float,
                                        Vector2> (buffer);
                                    var vSrcBuffer = Cast < float,
                                        Vector2> (srcBuffer);
                                    for (int i = 0; i < vBuffer.Length; i++) {
                                        (var inputSamplePosition, var x) = GetConversionGradient (i);
                                        int inputSampleIndex = inputSamplePosition;
                                        if (x == 0) {
                                            //srcBuffer.Slice(inputSampleIndex + channels, channels).CopyTo(buffer.Slice(i));
                                            vBuffer[i] = vSrcBuffer[inputSampleIndex + 1];
                                        } else {
                                            float xP2 = x * x;
                                            float xP3 = xP2 * x;
                                            ref var values = ref Unsafe.As <Vector2,
                                                ( Vector2 X, Vector2 Y, Vector2 Z, Vector2 W) > (ref vSrcBuffer[inputSampleIndex]);
                                            var value1 = values.X; //The control point 1.
                                            var value2 = values.Y; //The control point 2.
                                            var value3 = values.Z; //The control point 3.
                                            var value4 = values.W; //The control point 4.

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
                            case 3 :
                                {
                                    var vBuffer = Cast < float,
                                        Vector3> (buffer);
                                    var vSrcBuffer = Cast < float,
                                        Vector3> (srcBuffer);
                                    for (int i = 0; i < vBuffer.Length; i++) {
                                        (var inputSamplePosition, var x) = GetConversionGradient (i);
                                        int inputSampleIndex = inputSamplePosition;
                                        if (x == 0) {
                                            //srcBuffer.Slice(inputSampleIndex + channels, channels).CopyTo(buffer.Slice(i));
                                            vBuffer[i] = vSrcBuffer[inputSampleIndex + 1];
                                        } else {
                                            float xP2 = x * x;
                                            float xP3 = xP2 * x;
                                            ref var values = ref Unsafe.As <Vector3,
                                                ( Vector3 X, Vector3 Y, Vector3 Z, Vector3 W) > (ref vSrcBuffer[inputSampleIndex]);
                                            var value1 = values.X; //The control point 1.
                                            var value2 = values.Y; //The control point 2.
                                            var value3 = values.Z; //The control point 3.
                                            var value4 = values.W; //The control point 4.

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
                            case 4 :
                                {
                                    var vBuffer = Cast < float,
                                        Vector4> (buffer);
                                    var vSrcBuffer = Cast < float,
                                        Vector4> (srcBuffer);
                                    for (int i = 0; i < vBuffer.Length; i++) {
                                        (var inputSamplePosition, var x) = GetConversionGradient (i);
                                        int inputSampleIndex = inputSamplePosition;
                                        if (x == 0) {
                                            //srcBuffer.Slice(inputSampleIndex + channels, channels).CopyTo(buffer.Slice(i));
                                            vBuffer[i] = vSrcBuffer[inputSampleIndex + 1];
                                        } else {
                                            float xP2 = x * x;
                                            float xP3 = xP2 * x;
                                            ref var values = ref Unsafe.As <Vector4,
                                                ( Vector4 X, Vector4 Y, Vector4 Z, Vector4 W) > (ref vSrcBuffer[inputSampleIndex]);
                                            var value1 = values.X; //The control point 1.
                                            var value2 = values.Y; //The control point 2.
                                            var value3 = values.Z; //The control point 3.
                                            var value4 = values.W; //The control point 4.

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
                            unsafe {
                                fixed (float * srcBufPtr = srcBuffer) {
                                    for (int i = 0; i < buffer.Length; i += channels) {
                                        (var inputSamplePosition, var x) = GetConversionGradient (outputSamplePosition);
                                        int inputSampleIndex = inputSamplePosition * channels;
                                        if (x == 0) {
                                            srcBuffer.Slice (inputSampleIndex + channels, channels).CopyTo (buffer.Slice (i));
                                        } else {
                                            float xP2 = x * x;
                                            float xP3 = xP2 * x;
                                            var cache = srcBufPtr + inputSampleIndex;

                                            for (int ch = 0; ch < channels; ch++) {
                                                ref var destSample = ref buffer[i + ch]; //Persist the reference in order to eliminate boundary checks.
                                                var value1 = cache[ch]; //The control point 1.
                                                var value2 = cache[channels + ch]; //The control point 2.
                                                var value3 = cache[channels * 2 + ch]; //The control point 3.
                                                var value4 = cache[channels * 3 + ch]; //The control point 4.

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
        }
    }
}
