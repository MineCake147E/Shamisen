using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen
{
    /// <summary>
    /// Contains some utility functions for <see cref="IAudioSource{TSample, TFormat}"/>.
    /// </summary>
    public static class AudioSourceUtils
    {
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
        public static IWaveSource ToWaveSource(this IReadableAudioSource<byte, WaveFormat> source) => new ReadableWaveSource(source);
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
        private readonly IReadableAudioSource<byte, WaveFormat> source;

        public ReadableWaveSource(IReadableAudioSource<byte, WaveFormat> source) => this.source = source ?? throw new ArgumentNullException(nameof(source));

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
