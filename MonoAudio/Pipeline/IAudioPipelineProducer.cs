using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoAudio.Pipeline
{
    /// <summary>
    /// Defines a base infrastructure of an audio pipeline output.
    /// </summary>
    /// <typeparam name="TSample">The type of the sample.</typeparam>
    /// <typeparam name="TFormat">The type of the format.</typeparam>
    /// <seealso cref="IAudioPipelineComponent{TSample, TFormat}" />
    public interface IAudioPipelineProducer<TSample, TFormat>
        : IAudioPipelineComponent<TSample, TFormat>
        where TSample : unmanaged
        where TFormat : IAudioFormat<TSample>
    {
        /// <summary>
        /// Gets a value indicating whether this instance can pre-process.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can pre-process; otherwise, <c>false</c>.
        /// </value>
        bool CanPreProcess { get; }
    }
}
