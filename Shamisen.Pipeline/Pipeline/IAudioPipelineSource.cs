using System;
using System.Collections.Generic;
using System.Text;

namespace Shamisen.Pipeline
{
    /// <summary>
    /// Defines a base infrastructure of a readable audio pipeline component.
    /// </summary>
    /// <typeparam name="TSample">The type of the sample.</typeparam>
    /// <typeparam name="TFormat">The type of the format.</typeparam>
    /// <seealso cref="IAudioPipelineComponent{TSample, TFormat}" />
    /// <seealso cref="IAsyncReadableAudioSource{TSample, TFormat}" />
    /// <seealso cref="IReadableAudioSource{TSample, TFormat}" />
    public interface IAudioPipelineSource<TSample, TFormat>
        : IAudioPipelineComponent<TSample, TFormat>,
        IAsyncReadableAudioSource<TSample, TFormat>,
        IReadableAudioSource<TSample, TFormat>
        where TSample : unmanaged
        where TFormat : IAudioFormat<TSample>
    {
    }
}
