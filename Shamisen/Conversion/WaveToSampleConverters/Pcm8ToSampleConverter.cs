using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Shamisen.Conversion.WaveToSampleConverters
{
    /// <summary>
    /// Converts 8-bit PCM to Sample.
    /// </summary>
    /// <seealso cref="Shamisen.Conversion.WaveToSampleConverters.WaveToSampleConverterBase" />
    public sealed class Pcm8ToSampleConverter : WaveToSampleConverterBase
    {
        private const int BufferMax = 4096;
        private int ActualBufferMax => BufferMax - (BufferMax % Source.Format.Channels);

        /// <summary>
        /// Initializes a new instance of the <see cref="Pcm8ToSampleConverter"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        public Pcm8ToSampleConverter(IReadableAudioSource<byte, IWaveFormat> source)
            : base(source, new SampleFormat(source.Format.SampleRate, source.Format.Channels))
        {
        }

        /// <summary>
        /// Gets the bytes consumed per sample.
        /// </summary>
        /// <value>
        /// The bytes consumed per sample.
        /// </value>
        protected override int BytesPerSample => sizeof(byte);

        /// <summary>
        /// Reads the audio to the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>
        /// The length of the data written.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override ReadResult Read(Span<float> buffer)
        {
            Span<byte> span = stackalloc byte[buffer.Length > ActualBufferMax ? ActualBufferMax : buffer.Length];
            var cursor = buffer;
            while (cursor.Length > 0)
            {
                var reader = cursor.Length >= span.Length ? span : span.Slice(0, cursor.Length);
                var rr = Source.Read(reader);
                if (rr.HasNoData) return buffer.Length - cursor.Length;
                int u = rr.Length;
                var wrote = reader.Slice(0, u);
                var dest = cursor.Slice(0, wrote.Length);
                if (wrote.Length != dest.Length)
                    new InvalidOperationException(
                        $"The {nameof(wrote)}'s length and {nameof(dest)}'s length are not equal! This is a bug!").Throw();
                for (int i = 0; i < wrote.Length && i < dest.Length; i++)
                {
                    dest[i] = wrote[i] / 128.0f - 1f;
                }
                cursor = cursor.Slice(u);
                if (u != reader.Length) return buffer.Length - cursor.Length;  //The Source doesn't fill whole reader so return here.
            }
            return buffer.Length;
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Source.Dispose();
                }
            }
            disposedValue = true;
        }
    }
}
