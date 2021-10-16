using System;
using System.Collections.Generic;
using System.Text;

namespace Shamisen.Pipeline
{
    /// <summary>
    /// Connects two pipeline components.
    /// </summary>
    /// <typeparam name="TSample">The type of the sample.</typeparam>
    /// <typeparam name="TFormat">The type of the format.</typeparam>
    public sealed partial class AudioPipe<TSample, TFormat>
        where TSample : unmanaged
        where TFormat : IAudioFormat<TSample>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="producer"></param>
        /// <param name="consumer"></param>
        public AudioPipe(IAudioPipelineProducer<TSample, TFormat> producer, IAudioPipelineConsumer<TSample, TFormat> consumer)
        {
            Producer = producer ?? throw new ArgumentNullException(nameof(producer));
            Consumer = consumer ?? throw new ArgumentNullException(nameof(consumer));
        }

        /// <summary>
        /// 
        /// </summary>
        public IAudioPipelineProducer<TSample, TFormat> Producer { get; }
        /// <summary>
        /// 
        /// </summary>
        public IAudioPipelineConsumer<TSample, TFormat> Consumer { get; }


    }
}
