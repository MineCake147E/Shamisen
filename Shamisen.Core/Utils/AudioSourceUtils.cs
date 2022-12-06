using System.Runtime.CompilerServices;

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
        #region Source Conversion
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

        /// <inheritdoc cref="StreamBuffer{TSample, TFormat}.StreamBuffer(IReadableAudioSource{TSample, TFormat}, int, int, bool)"/>
        public static ISampleSource Preload(this ISampleSource source, int initialBlockSize, int internalBufferNumber = 16, bool allowWaitForRead = false)
            => new StreamBuffer<float, SampleFormat>(source, initialBlockSize, internalBufferNumber, allowWaitForRead).EnsureBlocks().ToSampleSource();

        /// <inheritdoc cref="StreamBuffer{TSample, TFormat}.StreamBuffer(IReadableAudioSource{TSample, TFormat}, int, int, bool)"/>
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
        public static ISampleSource? ConvertToSample(this IWaveSource source) => source.Format.BitDepth switch
        {
            8 when source.Format.Encoding == AudioEncoding.LinearPcm => new Pcm8ToSampleConverter(source),
            8 when source.Format.Encoding == AudioEncoding.Alaw => new ALawToSampleConverter(source),
            8 when source.Format.Encoding == AudioEncoding.Mulaw => new MuLawToSampleConverter(source),
            16 when source.Format.Encoding == AudioEncoding.LinearPcm => new Pcm16ToSampleConverter(source),
            32 when source.Format.Encoding == AudioEncoding.LinearPcm => new Pcm32ToSampleConverter(source),
            32 when source.Format.Encoding == AudioEncoding.IeeeFloat => new Float32ToSampleConverter(source),
            24 when source.Format.Encoding == AudioEncoding.LinearPcm => new Pcm24ToSampleConverter(source),
            _ => null,
        };
        #endregion

        #region Source Utils
        /// <summary>
        /// Reads the audio to the specified buffer asynchronously if possible.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination">The buffer.</param>
        /// <returns>The length of the data written.</returns>
        public static async ValueTask<ReadResult> ReadAsAsync<TSample, TFormat>(this IReadableAudioSource<TSample, TFormat> source, Memory<TSample> destination)
            where TSample : unmanaged
            where TFormat : IAudioFormat<TSample>
            => source is IAsyncReadableAudioSource<TSample, TFormat> asource
                ? await asource.ReadAsync(destination)
                : source.Read(destination.Span);
        #endregion
    }
    #region ISampleSource and IWaveSource Implementation

    internal sealed class ReadableSampleSource : ISampleSource
    {
        private readonly IReadableAudioSource<float, SampleFormat> source;

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public ReadableSampleSource(IReadableAudioSource<float, SampleFormat> source)
        {
            ArgumentNullException.ThrowIfNull(source);
            this.source = source;
        }

        public SampleFormat Format
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => source.Format;
        }

        public ulong? Length
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => source.Length;
        }

        public ulong? TotalLength
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => source.TotalLength;
        }

        public ulong? Position
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => source.Position;
        }

        public ISkipSupport? SkipSupport
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => source.SkipSupport;
        }

        public ISeekSupport? SeekSupport
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => source.SeekSupport;
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public void Dispose() => source.Dispose();

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public ReadResult Read(Span<float> buffer) => source.Read(buffer);
    }

    internal sealed class ReadableWaveSource : IWaveSource
    {
        private readonly IReadableAudioSource<byte, IWaveFormat> source;

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public ReadableWaveSource(IReadableAudioSource<byte, IWaveFormat> source)
        {
            ArgumentNullException.ThrowIfNull(source);
            this.source = source;
        }

        public IWaveFormat Format
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => source.Format;
        }

        public ulong? Length
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => source.Length;
        }

        public ulong? TotalLength
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => source.TotalLength;
        }

        public ulong? Position
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => source.Position;
        }

        public ISkipSupport? SkipSupport
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => source.SkipSupport;
        }

        public ISeekSupport? SeekSupport
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => source.SeekSupport;
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public void Dispose() => source.Dispose();

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public ReadResult Read(Span<byte> buffer) => source.Read(buffer);
    }
    #endregion
}
