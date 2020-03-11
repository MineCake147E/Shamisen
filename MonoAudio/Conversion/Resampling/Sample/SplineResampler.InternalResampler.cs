using System;

namespace MonoAudio.Conversion.Resampling.Sample
{
    public sealed partial class SplineResampler
    {
        internal abstract class InternalResampler
        {
            private protected abstract void Resample(Span<float> buffer, ref Span<float> srcBuffer);
        }
    }
}
