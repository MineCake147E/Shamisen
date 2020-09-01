using System;
using System.Runtime.CompilerServices;

using DivideSharp;

namespace MonoAudio.Conversion.Resampling.Sample
{
    public sealed partial class SplineResampler
    {
        internal abstract class InternalResampler
        {
            protected int conversionGradient = 0;

            protected InternalResampler(UInt32Divisor channelsDivisor, UInt32Divisor rateDivDivisor, int rateDiv, float rateDivInverse, UInt32Divisor rateMulDivisor, int rateMul, float rateMulInverse)
            {
                ChannelsDivisor = channelsDivisor;
                RateDivDivisor = rateDivDivisor;
                RateDiv = rateDiv;
                RateDivInverse = rateDivInverse;
                RateMulDivisor = rateMulDivisor;
                RateMul = rateMul;
                RateMulInverse = rateMulInverse;
            }

            /// <summary>
            /// Gets the divisor for dividing a number by Channels.
            /// </summary>
            /// <value>
            /// The divisor object.
            /// </value>
            protected UInt32Divisor ChannelsDivisor { get; }

            /// <summary>
            /// Gets the divisor for converting source sample rate to destination sample rate.
            /// </summary>
            /// <value>
            /// The divisor object.
            /// </value>
            protected UInt32Divisor RateDivDivisor { get; }

            /// <summary>
            /// Gets the rate source sample rate.
            /// </summary>
            /// <value>
            /// The number to divide with.
            /// </value>
            protected int RateDiv { get; }

            /// <summary>
            /// Gets the divisor source sample rate.
            /// </summary>
            /// <value>
            /// The number to multiply.
            /// </value>
            protected float RateDivInverse { get; }

            /// <summary>
            /// Gets the divisor for converting destination sample rate to source sample rate.
            /// </summary>
            /// <value>
            /// The divisor object.
            /// </value>
            protected UInt32Divisor RateMulDivisor { get; }

            /// <summary>
            /// Gets the rate destination sample rate.
            /// </summary>
            /// <value>
            /// The number to multiply.
            /// </value>
            protected int RateMul { get; }

            /// <summary>
            /// Gets the divisor destination sample rate.
            /// </summary>
            /// <value>
            /// The number to multiply.
            /// </value>
            protected float RateMulInverse { get; }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            protected int AdvanceConversionGradient(ref int conversionGradient)
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

            private protected abstract int Resample(Span<float> buffer, ref Span<float> srcBuffer);
        }
    }
}
