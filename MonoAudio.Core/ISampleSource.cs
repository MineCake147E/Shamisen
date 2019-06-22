using System;
using System.Collections.Generic;
using System.Text;
using MonoAudio.Formats;

namespace MonoAudio
{
    /// <summary>
    /// Defines a base infrastructure of a IEEE 754 Floating-Point PCM audio source.
    /// </summary>
    /// <seealso cref="MonoAudio.IReadableAudioSource{TSample, TFormat}" />
    public interface ISampleSource : IReadableAudioSource<float, SampleFormat>
    {
    }

    /// <summary>
    /// Defines a base infrastructure of a IEEE 754 Floating-Point PCM audio filter.
    /// </summary>
    /// <seealso cref="MonoAudio.IAggregator{TSample, TSource, TDestinationFormat}" />
    public interface ISampleAggregator : IAggregator<float, ISampleSource, SampleFormat>
    {
    }
}
