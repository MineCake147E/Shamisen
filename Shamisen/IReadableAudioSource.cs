﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen
{
    /// <summary>
    /// Defines a base infrastructure of a readable audio source.
    /// </summary>
    /// <typeparam name="TSample">The type of audio data.</typeparam>
    /// <typeparam name="TFormat">The format of audio data.</typeparam>
    /// <seealso cref="IAudioSource{TSample,TFormat}" />
    public interface IReadableAudioSource<TSample, TFormat> : IAudioSource<TSample, TFormat>
        where TSample : unmanaged
        where TFormat : IAudioFormat<TSample>
    {
        /// <summary>
        /// Reads the audio to the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>The length of the data written.</returns>
        ReadResult Read(Span<TSample> buffer);
    }

    /// <summary>
    /// Defines a base infrastructure of an asynchronously readable audio source.
    /// </summary>
    /// <typeparam name="TSample">The type of audio data.</typeparam>
    /// <typeparam name="TFormat">The format of audio data.</typeparam>
    /// <seealso cref="IAudioSource{TSample,TFormat}" />
    public interface IAsyncReadableAudioSource<TSample, TFormat> : IAudioSource<TSample, TFormat>
        where TSample : unmanaged
        where TFormat : IAudioFormat<TSample>
    {
        /// <summary>
        /// Reads the audio to the specified buffer asynchronously.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>The length of the data written.</returns>
        ValueTask<ReadResult> ReadAsync(Memory<TSample> buffer);
    }
}