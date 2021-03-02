using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Shamisen.Conversion.WaveToSampleConverters;
using Shamisen.Filters;
using Shamisen.Filters.Buffering;

namespace Shamisen
{
    /// <summary>
    /// Contains some utility functions for <see cref="IAudioSource{TSample, TFormat}"/>.
    /// </summary>
    public static class AudioSourceUtils
    {
        /// <summary>
        /// Ensures the blocks are aligned for the <paramref name="source"/>.
        /// </summary>
        /// <typeparam name="TSample">The type of the sample.</typeparam>
        /// <typeparam name="TFormat">The type of the format.</typeparam>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static IReadableAudioSource<TSample, TFormat> EnsureBlocks<TSample, TFormat>(this IReadableAudioSource<TSample, TFormat> source)
            where TSample : unmanaged where TFormat : IInterleavedAudioFormat<TSample>
            => new BlockSizeReservoir<TSample, TFormat>(source);

        /// <summary>
        /// Converts the <paramref name="source"/> to <see cref="ISampleSource"/>.
        /// </summary>
        /// <param name="source">The source to convert.</param>
        /// <returns></returns>
        public static ISampleSource ToSampleSource(this IReadableAudioSource<float, SampleFormat> source) => new ReadableSampleSource(source);

        /// <summary>
        /// Converts the <paramref name="source"/> to <see cref="IWaveSource"/>.
        /// </summary>
        /// <param name="source">The source to convert.</param>
        /// <returns></returns>
        public static IWaveSource ToWaveSource(this IReadableAudioSource<byte, IWaveFormat> source) => new ReadableWaveSource(source);

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamBuffer{TSample, TFormat}"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="initialBlockSize">
        /// The size of initial buffer in Frames(independent on the number of channel and the type of sample).<br/>
        /// The buffer is automatically extended if the internal buffer is smaller than the size of reading buffers.
        /// </param>
        /// <param name="internalBufferNumber">The number of internal buffer.</param>
        /// <param name="allowWaitForRead">The value which indicates whether the <see cref="StreamBuffer{TSample, TFormat}"/> should wait for another sample block or not.</param>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> should not be <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="initialBlockSize"/> should be larger than or equals to 2048.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="internalBufferNumber"/> should be larger than or equals to 16.</exception>
        public static ISampleSource Preload(this ISampleSource source, int initialBlockSize, int internalBufferNumber = 16, bool allowWaitForRead = false)
            => new StreamBuffer<float, SampleFormat>(source, initialBlockSize, internalBufferNumber, allowWaitForRead).EnsureBlocks().ToSampleSource();

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamBuffer{TSample, TFormat}"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="initialBlockSize">
        /// The size of initial buffer in Frames(independent on the number of channel and the type of sample).<br/>
        /// The buffer is automatically extended if the internal buffer is smaller than the size of reading buffers.
        /// </param>
        /// <param name="internalBufferNumber">The number of internal buffer.</param>
        /// <param name="allowWaitForRead">The value which indicates whether the <see cref="StreamBuffer{TSample, TFormat}"/> should wait for another sample block or not.</param>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> should not be <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="initialBlockSize"/> should be larger than or equals to 2048.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="internalBufferNumber"/> should be larger than or equals to 16.</exception>
        public static IWaveSource Preload(this IWaveSource source, int initialBlockSize, int internalBufferNumber = 16, bool allowWaitForRead = false)
            => new StreamBuffer<byte, IWaveFormat>(source, initialBlockSize, internalBufferNumber, allowWaitForRead).EnsureBlocks().ToWaveSource();

        /// <summary>
        /// Loops the specified source.
        /// </summary>
        /// <typeparam name="TSample">The type of the sample.</typeparam>
        /// <typeparam name="TFormat">The type of the format.</typeparam>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static IReadableAudioSource<TSample, TFormat> Loop<TSample, TFormat>(this IReadableAudioSource<TSample, TFormat> source)
            where TSample : unmanaged where TFormat : IAudioFormat<TSample>
            => new LoopSource<TSample, TFormat>(source);

        /// <summary>
        /// Converts the <paramref name="source"/> to <see cref="ISampleSource"/>.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static ISampleSource? ConvertToSample(this IWaveSource source)
        {
            return source.Format.BitDepth switch
            {
                8 when source.Format.Encoding == AudioEncoding.LinearPcm => new Pcm8ToSampleConverter(source),
                8 when source.Format.Encoding == AudioEncoding.Alaw => new ALawToSampleConverter(source),
                16 when source.Format.Encoding == AudioEncoding.LinearPcm => new Pcm16ToSampleConverter(source),
                32 when source.Format.Encoding == AudioEncoding.LinearPcm => new Pcm32ToSampleConverter(source),
                32 when source.Format.Encoding == AudioEncoding.IeeeFloat => new Float32ToSampleConverter(source),
                24 when source.Format.Encoding == AudioEncoding.LinearPcm => new Pcm24ToSampleConverter(source),
                _ => null,
            };
        }
    }

    internal sealed class ReadableSampleSource : ISampleSource
    {
        private readonly IReadableAudioSource<float, SampleFormat> source;

        public ReadableSampleSource(IReadableAudioSource<float, SampleFormat> source) => this.source = source ?? throw new ArgumentNullException(nameof(source));

        public SampleFormat Format => source.Format;

        public ulong? Length => source.Length;

        public ulong? TotalLength => source.TotalLength;

        public ulong? Position => source.Position;

        public ISkipSupport? SkipSupport => source.SkipSupport;

        public ISeekSupport? SeekSupport => source.SeekSupport;

        public void Dispose() => source.Dispose();

        public ReadResult Read(Span<float> buffer) => source.Read(buffer);
    }

    internal sealed class ReadableWaveSource : IWaveSource
    {
        private readonly IReadableAudioSource<byte, IWaveFormat> source;

        public ReadableWaveSource(IReadableAudioSource<byte, IWaveFormat> source) => this.source = source ?? throw new ArgumentNullException(nameof(source));

        public IWaveFormat Format => source.Format;

        public ulong? Length => source.Length;

        public ulong? TotalLength => source.TotalLength;

        public ulong? Position => source.Position;

        public ISkipSupport? SkipSupport => source.SkipSupport;

        public ISeekSupport? SeekSupport => source.SeekSupport;

        public void Dispose() => source.Dispose();

        public ReadResult Read(Span<byte> buffer) => source.Read(buffer);
    }
}
