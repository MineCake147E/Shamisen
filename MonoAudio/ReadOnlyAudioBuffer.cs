using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace MonoAudio
{
    /// <summary>
    /// Represents a read-only audio buffer.
    /// </summary>
    /// <typeparam name="TSample">The type of the sample.</typeparam>
    /// <typeparam name="TFormat">The type of the format.</typeparam>
    public sealed class ReadOnlyAudioBuffer<TSample, TFormat> where TSample : unmanaged
        where TFormat : IAudioFormat<TSample>
    {
        /// <summary>
        /// Gets the element at the specified <paramref name="index"/> in the <see cref="AudioBuffer{TSample, TFormat}"/>.
        /// </summary>
        /// <value>
        /// The element.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public TSample this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Memory.Span[index];
        }

        /// <summary>
        /// Gets the format of the audio data.
        /// </summary>
        /// <value>
        /// The format of the audio data.
        /// </value>
        public TFormat Format { get; }

        /// <summary>
        /// Gets the length of this <see cref="AudioBuffer{TSample, TFormat}"/>.
        /// </summary>
        /// <value>
        /// The length of this buffer.
        /// </value>
        public int Length => Memory.Span.Length;

        /// <summary>
        /// Gets the internal memory region.
        /// </summary>
        /// <value>
        /// The memory.
        /// </value>
        public ReadOnlyMemory<TSample> Memory { get; }

        /// <summary>
        /// Gets the internal memory region.
        /// </summary>
        /// <value>
        /// The span.
        /// </value>
        public ReadOnlySpan<TSample> Span => Memory.Span;
    }
}
