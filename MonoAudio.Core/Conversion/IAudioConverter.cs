using System;
using System.Collections.Generic;
using System.Text;

namespace MonoAudio.Conversion
{
    /// <summary>
    /// The base definition of some audio converters.
    /// </summary>
    /// <typeparam name="TFrom">The type to convert from.</typeparam>
    /// <typeparam name="TTo">The type to convert data to.</typeparam>
    public interface IAudioConverter<TFrom, TTo> : IReadableAudioSource<TTo>
    {
        /// <summary>
        /// Gets the source.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        IReadableAudioSource<TFrom> Source { get; }
    }
}
