﻿using System;
using System.Collections.Generic;
using System.Text;
using static System.Runtime.InteropServices.MemoryMarshal;
using System.Numerics;
using System.Runtime.CompilerServices;
using Shamisen.Filters;
using Shamisen.Formats;
using Shamisen.Utils;

namespace Shamisen.Conversion.Resampling.Sample {
    public sealed partial class SplineResampler {
        private const int RemSampleOffset = 4;
        private int ResampleCachedDirect (Span<float> buffer, int channels, Span<float> srcBuffer) {
            ref var coeffPtr = ref GetReference (preCalculatedCatmullRomCoefficents.AsSpan ());
            int outputSamplePosition = 0;
            // Use formula from http://www.mvps.org/directx/articles/catmull/
            int inputSampleIndex = 0, x = conversionGradient;
            if (channels == Vector<float>.Count) //SIMD Optimized Multi-Channel Audio Resampling
            {
                var vBuffer = Cast<float, Vector<float>> (buffer);
                var vSrcBuffer = Cast<float, Vector<float>> (srcBuffer);
                for (int i = 0; i < vBuffer.Length; i++) {
                    ref var values = ref Unsafe.As < Vector<float>,
                        (Vector<float> X, Vector<float> Y, Vector<float> Z, Vector<float> W) > (ref vSrcBuffer[inputSampleIndex]);
                    var cutmullCoeffs = Unsafe.Add (ref coeffPtr, x);
                    var value1 = values.X * cutmullCoeffs.X; //The control point 1.
                    var value2 = values.Y * cutmullCoeffs.Y; //The control point 2.
                    var value3 = values.Z * cutmullCoeffs.Z; //The control point 3.
                    var value4 = values.W * cutmullCoeffs.W; //The control point 4.

                    // Use formula from http://www.mvps.org/directx/articles/catmull/
                    vBuffer[i] = value1 + value2 + value3 + value4;
                    inputSampleIndex += AdvanceConversionGradient(ref x);
                }
            } else {
                switch (channels) {
                    case 1: //Monaural
                        for (int i = 0; i < buffer.Length; i++) {
                            var values = Unsafe.As<float, Vector4> (ref srcBuffer[inputSampleIndex]);
                            var cutmullCoeffs = Unsafe.Add (ref coeffPtr, x);
                            // Use formula from http://www.mvps.org/directx/articles/catmull/
                            buffer[i] = Vector4.Dot (values, cutmullCoeffs);
                            inputSampleIndex += AdvanceConversionGradient(ref x);
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
                                        ref var values = ref Unsafe.As <Vector2,
                                            ( Vector2 X, Vector2 Y, Vector2 Z, Vector2 W) > (ref vSrcBuffer[inputSampleIndex]);
                                        var cutmullCoeffs = Unsafe.Add (ref coeffPtr, x);
                                        var value1 = values.X * cutmullCoeffs.X; //The control point 1.
                                        var value2 = values.Y * cutmullCoeffs.Y; //The control point 2.
                                        var value3 = values.Z * cutmullCoeffs.Z; //The control point 3.
                                        var value4 = values.W * cutmullCoeffs.W; //The control point 4.
                                        vBuffer[i] = value1 + value2 + value3 + value4;
                                        inputSampleIndex += AdvanceConversionGradient(ref x);
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
                                        ref var values = ref Unsafe.As <Vector3,
                                            ( Vector3 X, Vector3 Y, Vector3 Z, Vector3 W) > (ref vSrcBuffer[inputSampleIndex]);
                                        var cutmullCoeffs = Unsafe.Add (ref coeffPtr, x);
                                        var value1 = values.X * cutmullCoeffs.X; //The control point 1.
                                        var value2 = values.Y * cutmullCoeffs.Y; //The control point 2.
                                        var value3 = values.Z * cutmullCoeffs.Z; //The control point 3.
                                        var value4 = values.W * cutmullCoeffs.W; //The control point 4.
                                        vBuffer[i] = value1 + value2 + value3 + value4;
                                        inputSampleIndex += AdvanceConversionGradient(ref x);
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
                                        ref var values = ref Unsafe.As <Vector4,
                                            ( Vector4 X, Vector4 Y, Vector4 Z, Vector4 W) > (ref vSrcBuffer[inputSampleIndex]);
                                        var cutmullCoeffs = Unsafe.Add (ref coeffPtr, x);
                                        var value1 = values.X * cutmullCoeffs.X; //The control point 1.
                                        var value2 = values.Y * cutmullCoeffs.Y; //The control point 2.
                                        var value3 = values.Z * cutmullCoeffs.Z; //The control point 3.
                                        var value4 = values.W * cutmullCoeffs.W; //The control point 4.
                                        vBuffer[i] = value1 + value2 + value3 + value4;
                                        inputSampleIndex += AdvanceConversionGradient(ref x);
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
                                        var cache = srcBufPtr + inputSampleIndex;
                                        var cutmullCoeffs = Unsafe.Add (ref coeffPtr, x);
                                        for (int ch = 0; ch < channels; ch++) {
                                            ref var destSample = ref buffer[i + ch]; //Persist the reference in order to eliminate boundary checks.
                                            var values = new Vector4(
                                                cache[ch], cache[channels + ch], cache[channels * 2 + ch], cache[channels * 3 + ch]);
                                            // Use formula from http://www.mvps.org/directx/articles/catmull/
                                            destSample = Vector4.Dot(values, cutmullCoeffs);
                                        }
                                        inputSampleIndex += AdvanceConversionGradient(ref x);
                                        outputSamplePosition++;
                                    }
                                }
                            }
                        }

                        #endregion Channels that is not SIMD optimized

                        break;
                }
            }
            conversionGradient = x;
            return inputSampleIndex;
        }

        private int ResampleCachedWrappedOdd (Span<float> buffer, int channels, Span<float> srcBuffer) {
            ref var coeffPtr = ref GetReference (preCalculatedCatmullRomCoefficents.AsSpan ());
            int outputSamplePosition = 0;
            // Use formula from http://www.mvps.org/directx/articles/catmull/
            Vector4 GetCatmullRomCoefficents (ref Vector4 coeffs, int i)
            {
                int x = i;
                if (i <= RateMul / 2) return Unsafe.Add(ref coeffs, x);
                x = RateMul - i;
                var q = Unsafe.Add(ref coeffs, x);
                return new Vector4(q.W, q.Z, q.Y, q.X);
            }
            int inputSampleIndex = 0, x = conversionGradient;
            if (channels == Vector<float>.Count) //SIMD Optimized Multi-Channel Audio Resampling
            {
                var vBuffer = Cast<float, Vector<float>> (buffer);
                var vSrcBuffer = Cast<float, Vector<float>> (srcBuffer);
                for (int i = 0; i < vBuffer.Length; i++) {
                    ref var values = ref Unsafe.As < Vector<float>,
                        (Vector<float> X, Vector<float> Y, Vector<float> Z, Vector<float> W) > (ref vSrcBuffer[inputSampleIndex]);
                    var cutmullCoeffs = GetCatmullRomCoefficents (ref coeffPtr, x);
                    var value1 = values.X * cutmullCoeffs.X; //The control point 1.
                    var value2 = values.Y * cutmullCoeffs.Y; //The control point 2.
                    var value3 = values.Z * cutmullCoeffs.Z; //The control point 3.
                    var value4 = values.W * cutmullCoeffs.W; //The control point 4.

                    // Use formula from http://www.mvps.org/directx/articles/catmull/
                    vBuffer[i] = value1 + value2 + value3 + value4;
                    inputSampleIndex += AdvanceConversionGradient(ref x);
                }
            } else {
                switch (channels) {
                    case 1: //Monaural
                        for (int i = 0; i < buffer.Length; i++) {
                            var values = Unsafe.As<float, Vector4> (ref srcBuffer[inputSampleIndex]);
                            var cutmullCoeffs = GetCatmullRomCoefficents (ref coeffPtr, x);

                            // Use formula from http://www.mvps.org/directx/articles/catmull/
                            buffer[i] = Vector4.Dot (values, cutmullCoeffs);
                            inputSampleIndex += AdvanceConversionGradient(ref x);
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
                                        ref var values = ref Unsafe.As <Vector2,
                                            ( Vector2 X, Vector2 Y, Vector2 Z, Vector2 W) > (ref vSrcBuffer[inputSampleIndex]);
                                        var cutmullCoeffs = GetCatmullRomCoefficents (ref coeffPtr, x);
                                        var value1 = values.X * cutmullCoeffs.X; //The control point 1.
                                        var value2 = values.Y * cutmullCoeffs.Y; //The control point 2.
                                        var value3 = values.Z * cutmullCoeffs.Z; //The control point 3.
                                        var value4 = values.W * cutmullCoeffs.W; //The control point 4.
                                        vBuffer[i] = value1 + value2 + value3 + value4;
                                        inputSampleIndex += AdvanceConversionGradient(ref x);
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
                                        ref var values = ref Unsafe.As <Vector3,
                                            ( Vector3 X, Vector3 Y, Vector3 Z, Vector3 W) > (ref vSrcBuffer[inputSampleIndex]);
                                        var cutmullCoeffs = GetCatmullRomCoefficents (ref coeffPtr, x);
                                        var value1 = values.X * cutmullCoeffs.X; //The control point 1.
                                        var value2 = values.Y * cutmullCoeffs.Y; //The control point 2.
                                        var value3 = values.Z * cutmullCoeffs.Z; //The control point 3.
                                        var value4 = values.W * cutmullCoeffs.W; //The control point 4.
                                        vBuffer[i] = value1 + value2 + value3 + value4;
                                        inputSampleIndex += AdvanceConversionGradient(ref x);
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
                                        ref var values = ref Unsafe.As <Vector4,
                                            ( Vector4 X, Vector4 Y, Vector4 Z, Vector4 W) > (ref vSrcBuffer[inputSampleIndex]);
                                        var cutmullCoeffs = GetCatmullRomCoefficents (ref coeffPtr, x);
                                        var value1 = values.X * cutmullCoeffs.X; //The control point 1.
                                        var value2 = values.Y * cutmullCoeffs.Y; //The control point 2.
                                        var value3 = values.Z * cutmullCoeffs.Z; //The control point 3.
                                        var value4 = values.W * cutmullCoeffs.W; //The control point 4.
                                        vBuffer[i] = value1 + value2 + value3 + value4;
                                        inputSampleIndex += AdvanceConversionGradient(ref x);
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
                                        inputSampleIndex += AdvanceConversionGradient(ref x);
                                        outputSamplePosition++;
                                    }
                                }
                            }
                        }

                        #endregion Channels that is not SIMD optimized

                        break;
                }
            }
            conversionGradient = x;
            return inputSampleIndex;
        }
        private int ResampleCachedWrappedEven (Span<float> buffer, int channels, Span<float> srcBuffer) {
            ref var coeffPtr = ref GetReference (preCalculatedCatmullRomCoefficents.AsSpan ());
            int outputSamplePosition = 0;
            int rmul = RateMul;
            // Use formula from http://www.mvps.org/directx/articles/catmull/
            Vector4 GetCatmullRomCoefficents (ref Vector4 coeffs, int i)
            {
                int x = i;
                if (i < RateMul / 2) return Unsafe.Add(ref coeffs, x);
                x = RateMul - i - 1;
                var q = Unsafe.Add(ref coeffs, x);
                return new Vector4(q.W, q.Z, q.Y, q.X);
            }
            int inputSampleIndex = 0, x = conversionGradient;
            if (channels == Vector<float>.Count) //SIMD Optimized Multi-Channel Audio Resampling
            {
                var vBuffer = Cast<float, Vector<float>> (buffer);
                var vSrcBuffer = Cast<float, Vector<float>> (srcBuffer);
                for (int i = 0; i < vBuffer.Length; i++) {
                    ref var values = ref Unsafe.As < Vector<float>,
                        (Vector<float> X, Vector<float> Y, Vector<float> Z, Vector<float> W) > (ref vSrcBuffer[inputSampleIndex]);
                    var cutmullCoeffs = GetCatmullRomCoefficents (ref coeffPtr, x);
                    var value1 = values.X * cutmullCoeffs.X; //The control point 1.
                    var value2 = values.Y * cutmullCoeffs.Y; //The control point 2.
                    var value3 = values.Z * cutmullCoeffs.Z; //The control point 3.
                    var value4 = values.W * cutmullCoeffs.W; //The control point 4.

                    // Use formula from http://www.mvps.org/directx/articles/catmull/
                    vBuffer[i] = value1 + value2 + value3 + value4;
                    inputSampleIndex += AdvanceConversionGradient(ref x);
                }
            } else {
                switch (channels) {
                    case 1: //Monaural
                        for (int i = 0; i < buffer.Length; i++) {
                            var values = Unsafe.As<float, Vector4> (ref srcBuffer[inputSampleIndex]);
                            var cutmullCoeffs = GetCatmullRomCoefficents (ref coeffPtr, x);

                            // Use formula from http://www.mvps.org/directx/articles/catmull/
                            buffer[i] = Vector4.Dot (values, cutmullCoeffs);
                            inputSampleIndex += AdvanceConversionGradient(ref x);
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
                                        ref var values = ref Unsafe.As <Vector2,
                                            ( Vector2 X, Vector2 Y, Vector2 Z, Vector2 W) > (ref vSrcBuffer[inputSampleIndex]);
                                        var cutmullCoeffs = GetCatmullRomCoefficents (ref coeffPtr, x);
                                        var value1 = values.X * cutmullCoeffs.X; //The control point 1.
                                        var value2 = values.Y * cutmullCoeffs.Y; //The control point 2.
                                        var value3 = values.Z * cutmullCoeffs.Z; //The control point 3.
                                        var value4 = values.W * cutmullCoeffs.W; //The control point 4.
                                        vBuffer[i] = value1 + value2 + value3 + value4;
                                        inputSampleIndex += AdvanceConversionGradient(ref x);
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
                                        ref var values = ref Unsafe.As <Vector3,
                                            ( Vector3 X, Vector3 Y, Vector3 Z, Vector3 W) > (ref vSrcBuffer[inputSampleIndex]);
                                        var cutmullCoeffs = GetCatmullRomCoefficents (ref coeffPtr, x);
                                        var value1 = values.X * cutmullCoeffs.X; //The control point 1.
                                        var value2 = values.Y * cutmullCoeffs.Y; //The control point 2.
                                        var value3 = values.Z * cutmullCoeffs.Z; //The control point 3.
                                        var value4 = values.W * cutmullCoeffs.W; //The control point 4.
                                        vBuffer[i] = value1 + value2 + value3 + value4;
                                        inputSampleIndex += AdvanceConversionGradient(ref x);
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
                                        ref var values = ref Unsafe.As <Vector4,
                                            ( Vector4 X, Vector4 Y, Vector4 Z, Vector4 W) > (ref vSrcBuffer[inputSampleIndex]);
                                        var cutmullCoeffs = GetCatmullRomCoefficents (ref coeffPtr, x);
                                        var value1 = values.X * cutmullCoeffs.X; //The control point 1.
                                        var value2 = values.Y * cutmullCoeffs.Y; //The control point 2.
                                        var value3 = values.Z * cutmullCoeffs.Z; //The control point 3.
                                        var value4 = values.W * cutmullCoeffs.W; //The control point 4.
                                        vBuffer[i] = value1 + value2 + value3 + value4;
                                        inputSampleIndex += AdvanceConversionGradient(ref x);
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
                                        inputSampleIndex += AdvanceConversionGradient(ref x);
                                        outputSamplePosition++;
                                    }
                                }
                            }
                        }

                        #endregion Channels that is not SIMD optimized

                        break;
                }
            }
            conversionGradient = x;
            return inputSampleIndex;
        }
        private int ResampleDirect (Span<float> buffer, int channels, Span<float> srcBuffer) {
            int outputSamplePosition = 0;
            // Use formula from http://www.mvps.org/directx/articles/catmull/
            int inputSampleIndex = 0, cG = conversionGradient;
            if (channels == Vector<float>.Count) //SIMD Optimized Multi-Channel Audio Resampling
            {
                var vBuffer = Cast<float, Vector<float>> (buffer);
                var vSrcBuffer = Cast<float, Vector<float>> (srcBuffer);
                for (int i = 0; i < vBuffer.Length; i++) {
                    float x = cG * RateMulInverse;
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
                    inputSampleIndex += AdvanceConversionGradient(ref cG);
                }
            } else {
                switch (channels) {
                    case 1: //Monaural
                        for (int i = 0; i < buffer.Length; i++) {
                            float x = cG * RateMulInverse;
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
                            inputSampleIndex += AdvanceConversionGradient(ref cG);
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
                                        float x = cG * RateMulInverse;
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
                                        inputSampleIndex += AdvanceConversionGradient(ref cG);
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
                                        float x = cG * RateMulInverse;
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
                                        inputSampleIndex += AdvanceConversionGradient(ref cG);
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
                                        float x = cG * RateMulInverse;
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
                                        inputSampleIndex += AdvanceConversionGradient(ref cG);
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
                                        var cache = srcBufPtr + inputSampleIndex;
                                        var m = CalculateCatmullRomCoeffs(cG * RateMulInverse);
                                        for (int ch = 0; ch < channels; ch++) {
                                            ref var destSample = ref buffer[i + ch]; //Persist the reference in order to reduce boundary checks.
                                            var values = new Vector4(
                                                cache[ch], cache[channels + ch], cache[channels * 2 + ch], cache[channels * 3 + ch]);
                                            // Use formula from http://www.mvps.org/directx/articles/catmull/
                                            destSample = Vector4.Dot(values, m);
                                        }
                                        inputSampleIndex += AdvanceConversionGradient(ref cG);
                                        outputSamplePosition++;
                                    }
                                }
                            }
                        }

                        #endregion Channels that is not SIMD optimized

                        break;
                }
            }
            conversionGradient = cG;
            return inputSampleIndex;
        }
    }
}
