using System;
using System.Collections.Generic;
using System.Text;

namespace MonoAudio
{
    /// <summary>
    /// Defines a base infrastructure of a IEEE 754 Floating-Point PCM audio source.
    /// </summary>
    /// <seealso cref="MonoAudio.IReadableAudioSource{T}" />
    public interface ISampleSource : IReadableAudioSource<float>
    {
    }

    /// <summary>
    /// Defines a base infrastructure of a IEEE 754 Floating-Point PCM audio filter.
    /// </summary>
    /// <seealso cref="MonoAudio.IAggregator{T,TAggregator}" />
    public interface ISampleAggregator : IAggregator<float, ISampleSource>
    {
    }
}
