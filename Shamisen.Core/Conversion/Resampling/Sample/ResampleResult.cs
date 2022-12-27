#if NETCOREAPP3_1_OR_GREATER
#endif
#if NET5_0_OR_GREATER
#endif


namespace Shamisen.Conversion.Resampling.Sample
{
    internal record struct ResampleResult(int InputSampleIndex, int ConversionGradient, int RearrangedCoeffsIndex = 0, int RearrangedCoeffsDirection = 1)
    {
        public static implicit operator (int inputSampleIndex, int x, int rearrangedCoeffsIndex, int rearrangedCoeffsDirection)(ResampleResult value) => (value.InputSampleIndex, value.ConversionGradient, value.RearrangedCoeffsIndex, value.RearrangedCoeffsDirection);
        public static implicit operator ResampleResult((int inputSampleIndex, int x, int rearrangedCoeffsIndex, int rearrangedCoeffsDirection) value) => new ResampleResult(value.inputSampleIndex, value.x, value.rearrangedCoeffsIndex, value.rearrangedCoeffsDirection);
    }
}
