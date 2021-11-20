using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen
{
    /// <summary>
    /// Defines a base infrastructure of a readable audio source.
    /// </summary>
    /// <typeparam name="TSample">The type of audio data.</typeparam>
    /// <typeparam name="TFormat">The format of audio data.</typeparam>
    /// <seealso cref="IAudioSource{TSample,TFormat}" />
    public interface IReadableAudioSource<TSample, TFormat> : IAudioSource<TSample, TFormat>, IReadSupport<TSample>
        where TSample : unmanaged
        where TFormat : IAudioFormat<TSample>
    {
    }

    /// <summary>
    /// Defines a base infrastructure of an asynchronously readable audio source.
    /// </summary>
    /// <typeparam name="TSample">The type of audio data.</typeparam>
    /// <typeparam name="TFormat">The format of audio data.</typeparam>
    /// <seealso cref="IAudioSource{TSample,TFormat}" />
    public interface IAsyncReadableAudioSource<TSample, TFormat> : IAudioSource<TSample, TFormat>, IAsyncReadSupport<TSample>
        where TSample : unmanaged
        where TFormat : IAudioFormat<TSample>
    {
    }
}
