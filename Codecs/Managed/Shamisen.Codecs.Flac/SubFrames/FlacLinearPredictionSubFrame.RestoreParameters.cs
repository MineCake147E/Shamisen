namespace Shamisen.Codecs.Flac.SubFrames
{
    public sealed partial class FlacLinearPredictionSubFrame
    {
        internal readonly ref struct RestoreParameters
        {
            private readonly RestoreConfiguration configuration;
            private readonly byte shiftsNeeded;
            public readonly Span<int> Coefficients;
            public readonly Span<int> Output;

            public byte PredictorOrder => configuration.PredictorOrder;
            public byte MultiplyBitsRequired => configuration.MultiplyBitsRequired;
            public byte AccumulatorBitsRequired => configuration.AccumulatorBitsRequired;
            public int ShiftsNeeded => shiftsNeeded;

            public RestoreParameters(RestoreConfiguration configuration, int shiftsNeeded, Span<int> coefficients, Span<int> output)
            {
                this.configuration = configuration;
                this.shiftsNeeded = (byte)shiftsNeeded;
                Coefficients = coefficients;
                Output = output;
                ArgumentOutOfRangeException.ThrowIfLessThan(coefficients.Length, 32);
            }
        }
        internal readonly record struct RestoreConfiguration(byte PredictorOrder, byte MultiplyBitsRequired, byte AccumulatorBitsRequired);
    }
}
