
using Shamisen.Utils;

namespace Shamisen.Pipeline
{
    /// <summary>
    /// Constructs a new instance of certain <see cref="IAudioPipelineConsumer{TSample, TFormat}"/>.
    /// </summary>
    public interface IAudipPipelineConsumerFactory<out TConsumer, TSample, TFormat>
        where TConsumer : IAudioPipelineConsumer<TSample, TFormat>
        where TSample : unmanaged
        where TFormat : IAudioFormat<TSample>
    {
        /// <summary>
        /// Creates a new instance of <typeparamref name="TConsumer"/>.
        /// </summary>
        /// <param name="faucet">The setting faucet.</param>
        /// <returns>The new instance of <typeparamref name="TConsumer"/>.</returns>
        TConsumer Construct(ReadOnceObjectContainer<AudioPipe.Faucet> faucet);
    }
}
