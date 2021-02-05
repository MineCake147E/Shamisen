using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

using DivideSharp;

using static System.Runtime.InteropServices.MemoryMarshal;

namespace Shamisen.Conversion.Resampling.Sample
{
    public sealed partial class SplineResampler
    {
        internal sealed class CachedDirectStereoResampler : InternalResampler
        {
            /// <summary>
            /// The pre calculated catmull-rom coefficents.<br/>
            /// X: The coefficent for value1 ((-xP3 + 2 * xP2 - x) * 0.5f)<br/>
            /// Y: The coefficent for value2 (((3 * xP3) - (5 * xP2) + 2) * 0.5f)<br/>
            /// Z: The coefficent for value3 ((-(3 * xP3) + 4 * xP2 + x) * 0.5f)<br/>
            /// W: The coefficent for value4 ((xP3 - xP2) * 0.5f)<br/>
            /// </summary>
            private readonly Vector4[] preCalculatedCatmullRomCoefficents;

            public CachedDirectStereoResampler(UInt32Divisor channelsDivisor, UInt32Divisor rateDivDivisor, int rateDiv, float rateDivInverse, UInt32Divisor rateMulDivisor, int rateMul, float rateMulInverse) : base(channelsDivisor, rateDivDivisor, rateDiv, rateDivInverse, rateMulDivisor, rateMul, rateMulInverse)
            {
                preCalculatedCatmullRomCoefficents = new Vector4[RateMul];
                for (int i = 0; i < preCalculatedCatmullRomCoefficents.Length; i++)
                {
                    var x = i * RateMulInverse;
                    preCalculatedCatmullRomCoefficents[i] = CalculateCatmullRomCoeffs(x);
                }
            }

            private protected override int Resample(Span<float> buffer, ref Span<float> srcBuffer)
            {
                ref var coeffPtr = ref GetReference(preCalculatedCatmullRomCoefficents.AsSpan());
                // Use formula from http://www.mvps.org/directx/articles/catmull/
                int inputSampleIndex = 0, x = conversionGradient;
                var vBuffer = Cast<float,
                                    Vector2>(buffer);
                var vSrcBuffer = Cast<float,
                    Vector2>(srcBuffer);
                ref var values = ref Unsafe.As<Vector2,
                        (Vector2 X, Vector2 Y, Vector2 Z, Vector2 W)>(ref vSrcBuffer[inputSampleIndex]);
                for (int i = 0; i < vBuffer.Length; i++)
                {
                    var cutmullCoeffs = Unsafe.Add(ref coeffPtr, x);
                    var value1 = values.X * cutmullCoeffs.X; //The control point 1.
                    var value2 = values.Y * cutmullCoeffs.Y; //The control point 2.
                    var value3 = values.Z * cutmullCoeffs.Z; //The control point 3.
                    var value4 = values.W * cutmullCoeffs.W; //The control point 4.
                    vBuffer[i] = value1 + value2 + value3 + value4;
                    var advance = AdvanceConversionGradient(ref x);
                    if (advance > 0)
                    {
                        inputSampleIndex += advance;
                        values = ref Unsafe.As<Vector2,
                        (Vector2 X, Vector2 Y, Vector2 Z, Vector2 W)>(ref vSrcBuffer[inputSampleIndex]);
                    }
                }
                conversionGradient = x;
                return inputSampleIndex;
            }
        }
    }
}
