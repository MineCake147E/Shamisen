using System.Collections.Generic;

namespace Shamisen.Filters
{
    /// <summary>
    /// Defines a base infrastructure of an audio filter with multiple inputs.
    /// </summary>
    /// <typeparam name="TSample">The type of the sample.</typeparam>
    /// <typeparam name="TFormat">The type of the format.</typeparam>
    public interface IMultipleInputAudioFilter<TSample, TFormat> : IReadableAudioSource<TSample, TFormat>
        where TSample : unmanaged
        where TFormat : IAudioFormat<TSample>
    {
        /// <summary>
        /// Gets the sources.
        /// </summary>
        /// <value>
        /// The sources.
        /// </value>
        IEnumerable<IReadableAudioSource<TSample, TFormat>> Sources { get; }
    }
}
