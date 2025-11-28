namespace Shamisen.Codecs.Flac.SubFrames
{
    public sealed partial class FlacLinearPredictionSubFrame
    {
        internal readonly ref struct RestoreParameters
        {
            public readonly int PredictorOrder;
            public readonly Span<int> Coefficients;
            public readonly Span<int> Output;
            public readonly Span<int> Residuals;
            public RestoreParameters(int predictorOrder, Span<int> coefficients, Span<int> output, Span<int> residuals)
            {
                PredictorOrder = predictorOrder;
                Coefficients = coefficients;
                Output = output;
                Residuals = residuals;
            }
        }
        internal readonly record struct RestoreConfiguration(byte PredictorOrder, byte MultiplyBitsRequired, byte AccumulatorBitsRequired);
    }
}
