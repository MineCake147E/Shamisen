using System;
using System.Collections.Generic;
using System.Text;

namespace Shamisen
{
    /// <summary>
    /// Defines a base infrastructure of a IEEE 754 Floating-Point PCM audio source.
    /// </summary>
    /// <seealso cref="Shamisen.IReadableAudioSource{TSample, TFormat}" />
    public interface ISampleSource : IReadableAudioSource<float, SampleFormat>
    {
    }

    /// <summary>
    /// Defines a base infrastructure of a IEEE 754 Floating-Point PCM audio filter.
    /// </summary>
    /// <seealso cref="Shamisen.IAggregator{TSample, TSource, TDestinationFormat}" />
    public interface ISampleAggregator : IAggregator<float, ISampleSource, SampleFormat>
    {
    }
}
