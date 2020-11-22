using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MonoAudio.Pipeline
{
    /// <summary>
    /// Defines a base infrastructure of an audio pipeline input.
    /// </summary>
    public interface IAudioPipelineTarget<TSample, TFormat>
        : IAudioPipelineComponent<TSample, TFormat>
        where TSample : unmanaged
        where TFormat : IAudioFormat<TSample>
    {
    }
}
