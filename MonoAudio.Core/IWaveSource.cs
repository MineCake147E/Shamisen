using System;
using System.Collections.Generic;
using System.Text;

namespace MonoAudio
{
    /// <summary>
    /// Defines a base infrastructure of a raw audio data source.
    /// </summary>
    /// <seealso cref="MonoAudio.IReadableAudioSource{T}" />
    public interface IWaveSource : IReadableAudioSource<byte>
    {
    }

    /// <summary>
    /// Defines a base infrastructure of a raw audio data filter.
    /// </summary>
    /// <seealso cref="MonoAudio.IAggregator{T,TAggregator}" />
    public interface IWaveAggregator : IAggregator<byte, IWaveSource>
    {
    }
}
