using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Shamisen.Filters
{
    /// <summary>
    /// Represents a BiQuad filter parameters.
    /// </summary>
    public readonly struct BiQuadParameter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BiQuadParameter"/> struct.
        /// </summary>
        /// <param name="b0">The b0.</param>
        /// <param name="b1">The b1.</param>
        /// <param name="b2">The b2.</param>
        /// <param name="a0">The a0.</param>
        /// <param name="a1">The a1.</param>
        /// <param name="a2">The a2.</param>
        private BiQuadParameter(float b0, float b1, float b2, float a0, float a1, float a2)
        {
            (B, A) = (new Vector3(b0, b1, b2) / a0, new Vector2(a1, a2) / -a0);  //Invert in advance
        }

        /// <summary>
        /// The normalized B parameters.
        /// </summary>
        public readonly Vector3 B;

        /// <summary>
        /// The normalized A parameters.
        /// </summary>
        public readonly Vector2 A;

        #region Public Constructor

        //Reference:https://www.g200kg.com/jp/docs/makingeffects/78743dea3f70c8c2f081b7d5187402ec75e6a6b8.html
        //I'm afraid that the reference is written in Japanese...

        /// <summary>
        /// The half Math.Log10(2)/Math.Log10(Math.E) with Bit-Exact representation.
        /// </summary>
        public const double HalfLn2 = 0.34657359027997264;

        /// <summary>
        /// Creates the LPF parameter from specified quality.
        /// </summary>
        /// <param name="samplingFrequency">The sampling frequency.</param>
        /// <param name="cutOffFrequency">The cut off frequency.</param>
        /// <param name="quality">The quality of LPF.</param>
        /// <returns></returns>
        public static BiQuadParameter CreateLPFParameter(double samplingFrequency, double cutOffFrequency, double quality)
        {
            CalculateOmega0RelatedValues(samplingFrequency, cutOffFrequency, out _, out double cosW0, out double sinW0);
            return CreateLPFCoefficients(cosW0, CalculateAlphaFromQuality(quality, sinW0));
        }

        /// <summary>
        /// Creates the HPF parameter from specified quality.
        /// </summary>
        /// <param name="samplingFrequency">The sampling frequency.</param>
        /// <param name="cutOffFrequency">The cut off frequency.</param>
        /// <param name="quality">The quality of LPF.</param>
        /// <returns></returns>
        public static BiQuadParameter CreateHPFParameter(double samplingFrequency, double cutOffFrequency, double quality)
        {
            CalculateOmega0RelatedValues(samplingFrequency, cutOffFrequency, out _, out double cosW0, out double sinW0);
            return CreateHPFCoefficients(cosW0, CalculateAlphaFromQuality(quality, sinW0));
        }

        /// <summary>
        /// Creates the APF parameter from specified quality.
        /// </summary>
        /// <param name="samplingFrequency">The sampling frequency.</param>
        /// <param name="cutOffFrequency">The cut off frequency.</param>
        /// <param name="quality">The quality of LPF.</param>
        /// <returns></returns>
        public static BiQuadParameter CreateAPFParameter(double samplingFrequency, double cutOffFrequency, double quality)
        {
            CalculateOmega0RelatedValues(samplingFrequency, cutOffFrequency, out _, out double cosW0, out double sinW0);
            return CreateAPFCoefficients(cosW0, CalculateAlphaFromQuality(quality, sinW0));
        }

        /// <summary>
        /// Creates the BPF parameter from specified quality.
        /// </summary>
        /// <param name="samplingFrequency">The sampling frequency.</param>
        /// <param name="centerFrequency">The center frequency.</param>
        /// <param name="quality">The quality.</param>
        /// <param name="gainKind">Kind of the gain.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">The kind of gain is invalid! - gainKind</exception>
        public static BiQuadParameter CreateBPFParameterFromQuality(double samplingFrequency, double centerFrequency, double quality, BpfGainKind gainKind)
        {
            CalculateOmega0RelatedValues(samplingFrequency, centerFrequency, out _, out double cosW0, out double sinW0);
            var alpha = CalculateAlphaFromQuality(quality, sinW0);
            return CreateBPFCoefficients(gainKind, cosW0, sinW0, alpha);
        }

        /// <summary>
        /// Creates the BPF parameter from specified bandwidth.
        /// </summary>
        /// <param name="samplingFrequency">The sampling frequency.</param>
        /// <param name="centerFrequency">The center frequency.</param>
        /// <param name="bandWidth">The bandwidth in Octaves.</param>
        /// <param name="gainKind">Kind of the gain.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">The kind of gain is invalid! - gainKind</exception>
        public static BiQuadParameter CreateBPFParameterFromBandWidth(double samplingFrequency, double centerFrequency, double bandWidth, BpfGainKind gainKind)
        {
            CalculateOmega0RelatedValues(samplingFrequency, centerFrequency, out double w0, out double cosW0, out double sinW0);
            var alpha = CalculateAlphaFromBandWidth(bandWidth, w0, sinW0);
            return CreateBPFCoefficients(gainKind, cosW0, sinW0, alpha);
        }

        /// <summary>
        /// Creates the notch filter parameter from specified quality.
        /// </summary>
        /// <param name="samplingFrequency">The sampling frequency.</param>
        /// <param name="centerFrequency">The center frequency.</param>
        /// <param name="quality">The quality.</param>
        /// <returns></returns>
        public static BiQuadParameter CreateNotchFilterParameterFromQuality(double samplingFrequency, double centerFrequency, double quality)
        {
            CalculateOmega0RelatedValues(samplingFrequency, centerFrequency, out _, out double cosW0, out double sinW0);
            var alpha = CalculateAlphaFromQuality(quality, sinW0);
            return CreateNotchFilterCoefficients(cosW0, alpha);
        }

        /// <summary>
        /// Creates the notch filter parameter from specified bandwidth.
        /// </summary>
        /// <param name="samplingFrequency">The sampling frequency.</param>
        /// <param name="centerFrequency">The center frequency.</param>
        /// <param name="bandWidth">The bandwidth.</param>
        /// <returns></returns>
        public static BiQuadParameter CreateNotchFilterParameterFromBandWidth(double samplingFrequency, double centerFrequency, double bandWidth)
        {
            CalculateOmega0RelatedValues(samplingFrequency, centerFrequency, out double w0, out double cosW0, out double sinW0);
            var alpha = CalculateAlphaFromBandWidth(bandWidth, w0, sinW0);
            return CreateNotchFilterCoefficients(cosW0, alpha);
        }

        /// <summary>
        /// Creates the peaking equalizer parameter from specified quality.
        /// </summary>
        /// <param name="samplingFrequency">The sampling frequency.</param>
        /// <param name="centerFrequency">The center frequency.</param>
        /// <param name="quality">The quality.</param>
        /// <param name="dBGain">The peak gain in dB.</param>
        /// <returns></returns>
        public static BiQuadParameter CreatePeakingEqualizerParameterFromQuality(double samplingFrequency, double centerFrequency, double quality, double dBGain)
        {
            var A = CalculateA(dBGain);
            CalculateOmega0RelatedValues(samplingFrequency, centerFrequency, out _, out double cosW0, out double sinW0);
            var alpha = CalculateAlphaFromQuality(quality, sinW0);
            return CreatePeakingEqualizerCoefficients(cosW0, alpha, A);
        }

        /// <summary>
        /// Creates the peaking equalizer parameter from specified bandwidth.
        /// </summary>
        /// <param name="samplingFrequency">The sampling frequency.</param>
        /// <param name="centerFrequency">The center frequency.</param>
        /// <param name="bandWidth">The bandwidth in Octaves.</param>
        /// <param name="dBGain">The peak gain in dB.</param>
        /// <returns></returns>
        public static BiQuadParameter CreatePeakingEqualizerParameterFromBandWidth(double samplingFrequency, double centerFrequency, double bandWidth, double dBGain)
        {
            var A = CalculateA(dBGain);
            CalculateOmega0RelatedValues(samplingFrequency, centerFrequency, out double w0, out double cosW0, out double sinW0);
            var alpha = CalculateAlphaFromBandWidth(bandWidth, w0, sinW0);
            return CreatePeakingEqualizerCoefficients(cosW0, alpha, A);
        }

        /// <summary>
        /// Creates the low shelf filter parameter from specified quality.
        /// </summary>
        /// <param name="samplingFrequency">The sampling frequency.</param>
        /// <param name="centerFrequency">The center frequency.</param>
        /// <param name="quality">The quality.</param>
        /// <param name="dBGain">The peak gain in dB.</param>
        /// <returns></returns>
        public static BiQuadParameter CreateLowShelfFilterParameterFromQuality(double samplingFrequency, double centerFrequency, double quality, double dBGain)
        {
            var A = CalculateA(dBGain);
            CalculateOmega0RelatedValues(samplingFrequency, centerFrequency, out _, out double cosW0, out double sinW0);
            var alpha = CalculateAlphaFromQuality(quality, sinW0);
            return CreateLowShelfCoefficients(cosW0, A, alpha);
        }

        /// <summary>
        /// Creates the low shelf filter parameter from specified slope.
        /// </summary>
        /// <param name="samplingFrequency">The sampling frequency.</param>
        /// <param name="cutOffFrequency">The cut off frequency.</param>
        /// <param name="slope">The slope in dB/Oct.</param>
        /// <param name="dBGain">The peak gain in dB.</param>
        /// <returns></returns>
        public static BiQuadParameter CreateLowShelfFilterParameterFromSlope(double samplingFrequency, double cutOffFrequency, double slope, double dBGain)
        {
            var A = CalculateA(dBGain);
            CalculateOmega0RelatedValues(samplingFrequency, cutOffFrequency, out _, out double cosW0, out double sinW0);
            var alpha = CalculateAlphaFromSlope(slope, A, sinW0);
            return CreateLowShelfCoefficients(cosW0, A, alpha);
        }

        /// <summary>
        /// Creates the High shelf filter parameter from specified quality.
        /// </summary>
        /// <param name="samplingFrequency">The sampling frequency.</param>
        /// <param name="centerFrequency">The center frequency.</param>
        /// <param name="quality">The quality.</param>
        /// <param name="dBGain">The peak gain in dB.</param>
        /// <returns></returns>
        public static BiQuadParameter CreateHighShelfFilterParameterFromQuality(double samplingFrequency, double centerFrequency, double quality, double dBGain)
        {
            var A = CalculateA(dBGain);
            CalculateOmega0RelatedValues(samplingFrequency, centerFrequency, out _, out double cosW0, out double sinW0);
            var alpha = CalculateAlphaFromQuality(quality, sinW0);
            return CreateHighShelfCoefficients(cosW0, A, alpha);
        }

        /// <summary>
        /// Creates the High shelf filter parameter from specified slope.
        /// </summary>
        /// <param name="samplingFrequency">The sampling frequency.</param>
        /// <param name="cutOffFrequency">The cut off frequency.</param>
        /// <param name="slope">The slope in dB/Oct.</param>
        /// <param name="dBGain">The peak gain in dB.</param>
        /// <returns></returns>
        public static BiQuadParameter CreateHighShelfFilterParameterFromSlope(double samplingFrequency, double cutOffFrequency, double slope, double dBGain)
        {
            var A = CalculateA(dBGain);
            CalculateOmega0RelatedValues(samplingFrequency, cutOffFrequency, out _, out double cosW0, out double sinW0);
            var alpha = CalculateAlphaFromSlope(slope, A, sinW0);
            return CreateHighShelfCoefficients(cosW0, A, alpha);
        }

        #endregion Public Constructor

        #region Construction Helper Functions

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double CalculateA(double dBGain) => Math.Pow(10, dBGain / 40.0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double CalculateAlphaFromBandWidth(double bandWidth, double w0, double sinW0) => sinW0 * Math.Sinh(HalfLn2 * bandWidth * w0 / sinW0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double CalculateAlphaFromQuality(double quality, double sinW0) => sinW0 / (2 * quality);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double CalculateAlphaFromSlope(double slope, double a, double sinW0) => 0.5 * sinW0 * Math.Sqrt((a + 1 / a) * (1 / slope - 1) + 2);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CalculateOmega0RelatedValues(double samplingFrequency, double cutOffFrequency, out double w0, out double cosW0, out double sinW0)
        {
            w0 = 2 * Math.PI * cutOffFrequency / samplingFrequency;
            cosW0 = Math.Cos(w0);
            sinW0 = Math.Sin(w0);
        }

        #endregion Construction Helper Functions

        #region Coefficients functions

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static BiQuadParameter CreateLPFCoefficients(double cosW0, double alpha)
        {
            float b1 = (float)(1 - cosW0);
            float b0b2 = 0.5f * b1;
            float a0 = (float)(1 + alpha);
            float a1 = -2f * (float)cosW0;
            float a2 = (float)(1 - alpha);
            return new BiQuadParameter(b0b2, b1, b0b2, a0, a1, a2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static BiQuadParameter CreateHPFCoefficients(double cosW0, double alpha)
        {
            float b1 = -(float)(1 + cosW0);
            float b0b2 = -0.5f * b1;
            float a0 = (float)(1 + alpha);
            float a1 = -2f * (float)cosW0;
            float a2 = (float)(1 - alpha);
            return new BiQuadParameter(b0b2, b1, b0b2, a0, a1, a2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static BiQuadParameter CreateConstSkirtBPFCoefficients(double alpha, double sinW0, double cosW0)
        {
            float b0b2 = 0.5f * (float)sinW0;
            float a0 = (float)(1 + alpha);
            float a1 = -2f * (float)cosW0;
            float a2 = (float)(1 - alpha);
            return new BiQuadParameter(b0b2, 0, -b0b2, a0, a1, a2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static BiQuadParameter CreateZeroDBPeakGainBPFCoefficients(double alpha, double cosW0)
        {
            float a0 = (float)(1 + alpha);
            float a1 = -2f * (float)cosW0;
            float a2 = (float)(1 - alpha);
            float alphaF = (float)alpha;
            return new BiQuadParameter(alphaF, 0, -alphaF, a0, a1, a2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static BiQuadParameter CreateBPFCoefficients(BpfGainKind gainKind, double cosW0, double sinW0, double alpha)
        {
            switch (gainKind)
            {
                case BpfGainKind.ConstSkirt:
                    return CreateConstSkirtBPFCoefficients(alpha, sinW0, cosW0);
                case BpfGainKind.ZeroDBPeakGain:
                    return CreateZeroDBPeakGainBPFCoefficients(alpha, cosW0); ;
                default:
                    throw new ArgumentException("The kind of gain is invalid!", nameof(gainKind));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static BiQuadParameter CreateNotchFilterCoefficients(double cosW0, double alpha)
        {
            float a0 = (float)(1 + alpha);
            float b1a1 = -2f * (float)cosW0;
            float a2 = (float)(1 - alpha);
            return new BiQuadParameter(1, b1a1, 1, a0, b1a1, a2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static BiQuadParameter CreateAPFCoefficients(double cosW0, double alpha)
        {
            float b2a0 = (float)(1 + alpha);
            float b1a1 = -2f * (float)cosW0;
            float b0a2 = (float)(1 - alpha);
            return new BiQuadParameter(b0a2, b1a1, b2a0, b2a0, b1a1, b0a2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static BiQuadParameter CreatePeakingEqualizerCoefficients(double cosW0, double alpha, double a)
        {
            var alphamA = alpha * a;
            var alphadA = alpha / a;
            float b1a1 = -2f * (float)cosW0;
            return new BiQuadParameter((float)(1 + alphamA), b1a1, (float)(1 - alphamA), (float)(1 + alphadA), b1a1, (float)(1 - alphadA));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static BiQuadParameter CreateLowShelfCoefficients(double cosW0, double a, double alpha)
        {
            var TwomSqrtAmAlpha = 2 * Math.Sqrt(a) * alpha;
            var Ap1 = a + 1;
            var As1 = a - 1;
            double As1CosW0 = As1 * cosW0;
            float b0 = (float)(a * (Ap1 - As1CosW0 + TwomSqrtAmAlpha));
            float b1 = (float)(2 * a * (As1 - Ap1 * cosW0));
            float b2 = (float)(a * (Ap1 - As1CosW0 - TwomSqrtAmAlpha));
            float a0 = (float)(Ap1 + As1CosW0 + TwomSqrtAmAlpha);
            float a1 = (float)(-2 * (As1 + Ap1 * cosW0));
            float a2 = (float)(Ap1 + As1CosW0 - TwomSqrtAmAlpha);
            return new BiQuadParameter(b0, b1, b2, a0, a1, a2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static BiQuadParameter CreateHighShelfCoefficients(double cosW0, double a, double alpha)
        {
            var TwomSqrtAmAlpha = 2 * Math.Sqrt(a) * alpha;
            var Ap1 = a + 1;
            var As1 = a - 1;
            double As1CosW0 = As1 * cosW0;
            double Ap1CosW0 = Ap1 * cosW0;
            float b0 = (float)(a * (Ap1 + As1CosW0 + TwomSqrtAmAlpha));
            float b1 = (float)(-2 * a * (As1 + Ap1CosW0));
            float b2 = (float)(a * (Ap1 + As1CosW0 - TwomSqrtAmAlpha));
            float a0 = (float)(Ap1 - As1CosW0 + TwomSqrtAmAlpha);
            float a1 = (float)(2 * (As1 - Ap1CosW0));
            float a2 = (float)(Ap1 - As1CosW0 - TwomSqrtAmAlpha);
            return new BiQuadParameter(b0, b1, b2, a0, a1, a2);
        }

        #endregion Coefficients functions
    }
}
