using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Shamisen.Utils;

namespace Shamisen.Pipeline
{
    /// <summary>
    /// Constructs a new instance of certain <see cref="IAudioPipelineProducer{TSample, TFormat}"/>.
    /// </summary>
    public interface IAudipPipelineProducerFactory<out TProducer, TSample, TFormat>
        where TProducer: IAudioPipelineProducer<TSample, TFormat>
        where TSample: unmanaged
        where TFormat : IAudioFormat<TSample>
    {
        /// <summary>
        /// Creates a new instance of <typeparamref name="TProducer"/>.
        /// </summary>
        /// <param name="inlet">The setting inlet.</param>
        /// <returns>The new instance of <typeparamref name="TProducer"/>.</returns>
        TProducer Construct(ReadOnceObjectContainer<AudioPipe.Inlet> inlet);
    }
}
