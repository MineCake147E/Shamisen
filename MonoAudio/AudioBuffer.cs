using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace MonoAudio
{
    /// <summary>
    /// Represents an audio buffer.
    /// </summary>
    /// <typeparam name="TSample">The type of the sample.</typeparam>
    /// <typeparam name="TFormat">The type of the format.</typeparam>
    public sealed class AudioBuffer<TSample, TFormat> where TSample : unmanaged
        where TFormat : IAudioFormat<TSample>
    {
        /// <summary>
        /// Gets or sets the element at the specified <paramref name="index"/> in the <see cref="AudioBuffer{TSample, TFormat}"/>.
        /// </summary>
        /// <value>
        /// The element.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public ref TSample this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Memory.Span[index];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioBuffer{TSample, TFormat}" /> struct.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="sizeInFrames">The size in frames.</param>
        public AudioBuffer(TFormat format, int sizeInFrames)
        {
            Format = format;
            Memory = new TSample[format.GetFrameSize() / Unsafe.SizeOf<TSample>() * sizeInFrames];
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
        public Memory<TSample> Memory { get; }

        /// <summary>
        /// Gets the internal memory region.
        /// </summary>
        /// <value>
        /// The span.
        /// </value>
        public Span<TSample> Span => Memory.Span;
    }
}
