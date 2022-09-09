using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Pipeline
{
    /// <summary>
    /// Represents properties of corresponding <see cref="IAudioSource{TSample, TFormat}"/>
    /// </summary>
    /// <typeparam name="TSample">The type of the sample.</typeparam>
    /// <typeparam name="TFormat">The type of the format.</typeparam>
    public sealed class AudioSourceProperties<TSample, TFormat>
        where TSample : unmanaged where TFormat : IAudioFormat<TSample>
    {
        /// <summary>
        /// Gets a value indicating whether the source is dynamic.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the source is dynamic; otherwise, <c>false</c>.
        /// </value>
        public bool IsDynamic { get; }

        /// <summary>
        /// Gets the preferred latency in seconds.<br/>
        /// Only positive non-infinity values are accepted.
        /// </summary>
        /// <value>
        /// The preferred latency.
        /// </value>
        public double PreferredLatency { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioSourceProperties{TSample, TFormat}"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="isDynamic">The value which indicates whether the source is either synthetic or changed in real-time, or not.</param>
        /// <param name="preferredLatency">The preferred latency.</param>
        /// <exception cref="ArgumentNullException">source</exception>
        public AudioSourceProperties(IAudioSource<TSample, TFormat> source, bool isDynamic, double preferredLatency = 1.0)
        {
            ArgumentNullException.ThrowIfNull(source);
            _ = source;
            PreferredLatency = double.IsNaN(preferredLatency) || double.IsInfinity(preferredLatency) || preferredLatency <= 0 ? throw new ArgumentOutOfRangeException(nameof(preferredLatency), "Only positive non-infinity values are accepted!") : preferredLatency;
            IsDynamic = isDynamic;
        }

        /// <summary>
        /// Gets the size of the required buffer.
        /// </summary>
        /// <returns></returns>
        public int GetRequiredBufferSize(IInterleavedAudioFormat<TSample> format) => format.BlockSize * (int)Math.Ceiling(format.SampleRate * PreferredLatency);
    }
}
