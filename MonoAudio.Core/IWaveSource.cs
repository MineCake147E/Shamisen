using System;
using System.Collections.Generic;
using System.Text;
using MonoAudio.Formats;

namespace MonoAudio
{
    /// <summary>
    /// Defines a base infrastructure of a raw audio data source.
    /// </summary>
    /// <seealso cref="MonoAudio.IReadableAudioSource{TSample, TFormat}" />
    public interface IWaveSource : IReadableAudioSource<byte, IWaveFormat>
    {
    }

    /// <summary>
    /// Defines a base infrastructure of a raw audio data filter.
    /// </summary>
    /// <seealso cref="MonoAudio.IAggregator{TSample, TSource, TDestinationFormat}" />
    public interface IWaveAggregator : IAggregator<byte, IWaveSource, IWaveFormat>
    {
    }
}
