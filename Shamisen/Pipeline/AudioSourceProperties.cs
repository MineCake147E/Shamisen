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
        /// Initializes a new instance of the <see cref="AudioSourceProperties{TSample, TFormat}"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="preferredLatency">The preferred latency.</param>
        /// <exception cref="ArgumentNullException">source</exception>
        public AudioSourceProperties(IAudioSource<TSample, TFormat> source, double preferredLatency = 1.0)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            PreferredLatency = preferredLatency;
        }

        private IAudioSource<TSample, TFormat> Source { get; }

        /// <summary>
        /// Gets the preferred latency in seconds.<br/>
        /// 0, negative values(including <see cref="double.NegativeInfinity"/>), and <see cref="double.NaN"/> means the source is dynamic.<br/>
        /// <see cref="double.PositiveInfinity"/> means the source is available locally(inside RAM).<br/>
        /// Otherwise, the source is either available online (over the network), decoding or processing another source, or loading data from a high-latency storage(like HDD).
        /// </summary>
        /// <value>
        /// The preferred latency.
        /// </value>
        public double PreferredLatency { get; }

        /// <summary>
        /// Gets a value indicating whether the source is static.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the source is static; otherwise, <c>false</c>.
        /// </value>
        public bool IsStatic => double.IsPositiveInfinity(PreferredLatency);

        /// <summary>
        /// Gets a value indicating whether the source is dynamic.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the source is dynamic; otherwise, <c>false</c>.
        /// </value>
        public bool IsDynamic => PreferredLatency <= 0 || double.IsNaN(PreferredLatency);
    }
}
