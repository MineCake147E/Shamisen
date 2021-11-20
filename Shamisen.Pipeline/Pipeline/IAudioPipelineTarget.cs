using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Pipeline
{
    /// <summary>
    /// Defines a base infrastructure of an audio pipeline consumer.
    /// </summary>
    public interface IAudioPipelineConsumer<TSample, TFormat>
        : IAudioPipelineComponent<TSample, TFormat>
        where TSample : unmanaged
        where TFormat : IAudioFormat<TSample>
    {
    }
}
