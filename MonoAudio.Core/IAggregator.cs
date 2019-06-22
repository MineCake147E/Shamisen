using System;
using System.Collections.Generic;
using System.Text;
using MonoAudio.Formats;

namespace MonoAudio
{
    /// <summary>
    /// Defines a base infrastructure of a filter.
    /// </summary>
    /// <typeparam name="TSample"></typeparam>
    /// <typeparam name="TSource">The type of the aggregator.</typeparam>
    /// <typeparam name="TDestinationFormat">The format of output audio.</typeparam>
    public interface IAggregator<TSample, out TSource, out TDestinationFormat>
        where TSource : IReadableAudioSource<TSample, TDestinationFormat> where TDestinationFormat : IAudioFormat<TSample>
    {
        /// <summary>
        /// Gets the base source.
        /// </summary>
        /// <value>
        /// The base source.
        /// </value>
        TSource BaseSource { get; }
    }
}
