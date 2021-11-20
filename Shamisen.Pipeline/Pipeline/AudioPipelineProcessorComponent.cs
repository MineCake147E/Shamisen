using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Pipeline
{
    /// <summary>
    /// Provides an implementation of simple audio processor in pipeline.
    /// </summary>
    /// <typeparam name="TSample"></typeparam>
    /// <typeparam name="TFormat"></typeparam>
    public sealed class AudioPipelineProcessorComponent<TSample, TFormat>
        where TSample : unmanaged
        where TFormat : IAudioFormat<TSample>
    {

    }
}
