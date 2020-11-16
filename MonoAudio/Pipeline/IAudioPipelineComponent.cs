using System.Threading.Tasks;

namespace MonoAudio.Pipeline
{
    /// <summary>
    /// Defines a base infrastructure of an audio pipeline component.
    /// </summary>
    /// <typeparam name="TSample">The type of the sample.</typeparam>
    /// <typeparam name="TFormat">The type of the format.</typeparam>
    /// <seealso cref="IAudioSource{TSample, TFormat}" />
    public interface IAudioPipelineComponent<TSample, TFormat>
        : IAudioSource<TSample, TFormat>
        where TSample : unmanaged
        where TFormat : IAudioFormat<TSample>
    {
        /// <summary>
        /// Processes the <paramref name="input"/> asynchronously.
        /// </summary>
        /// <param name="input">The input buffer.</param>
        ValueTask ProcessAsync(ReadOnlyAudioMemory<TSample, TFormat> input);
    }
}
