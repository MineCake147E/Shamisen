using System;
using System.Collections.Generic;
using System.Text;

namespace MonoAudio
{
    /// <summary>
    /// Defines a base infrastructure of a filter.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TAggregator">The type of the aggregator.</typeparam>
    public interface IAggregator<T, out TAggregator> where TAggregator : IReadableAudioSource<T>
    {
        /// <summary>
        /// Gets the base source.
        /// </summary>
        /// <value>
        /// The base source.
        /// </value>
        TAggregator BaseSource { get; }
    }
}
