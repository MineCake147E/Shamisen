﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Shamisen.Conversion
{
    /// <summary>
    /// The base definition of some audio converters.
    /// </summary>
    /// <typeparam name="TFrom">The type to convert from.</typeparam>
    /// <typeparam name="TFromFormat"></typeparam>
    /// <typeparam name="TTo">The type to convert data to.</typeparam>
    /// <typeparam name="TToFormat"></typeparam>
    public interface IAudioConverter<TFrom, TFromFormat, TTo, TToFormat> : IReadableAudioSource<TTo, TToFormat>
        where TFrom : unmanaged
        where TTo : unmanaged
        where TFromFormat : IAudioFormat<TFrom>
        where TToFormat : IAudioFormat<TTo>
    {
        /// <summary>
        /// Gets the source to read the samples from.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        IReadableAudioSource<TFrom, TFromFormat>? Source { get; }
    }
}
