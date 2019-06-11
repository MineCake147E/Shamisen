using MonoAudio.Extensions;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace MonoAudio.Conversion.WaveToSampleConverters
{
    /// <summary>
    /// Converts 32-bit PCM to Sample.
    /// </summary>
    /// <seealso cref="MonoAudio.Conversion.WaveToSampleConverters.WaveToSampleConverterBase" />
    public sealed class Pcm32ToSampleConverter : WaveToSampleConverterBase
    {
        private const float divisor = 2147483648.0f;
        private const int bytesPerSample = sizeof(int);
        private const int bufferMax = 1024;
        private int ActualBufferMax => bufferMax - (bufferMax % Source.Format.Channels);

        /// <summary>
        /// Gets the endianness of <see cref="WaveToSampleConverterBase.Source"/>.
        /// </summary>
        /// <value>
        /// The endianness of <see cref="WaveToSampleConverterBase.Source"/>.
        /// </value>
        public Endianness Endianness { get; }

        private bool IsEndiannessConversionRequired => Endianness != EndiannessExtensions.EnvironmentEndianness;

        /// <summary>
        /// Initializes a new instance of the <see cref="Pcm16ToSampleConverter"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="endianness">The endianness of <paramref name="source"/>.</param>
        public Pcm32ToSampleConverter(IReadableAudioSource<byte> source, Endianness endianness = Endianness.Little)
            : base(source, new WaveFormat(source.Format.SampleRate, 32, source.Format.Channels, AudioEncoding.IeeeFloat))
        {
            Endianness = endianness;
        }

        /// <summary>
        /// Gets the bytes consumed per sample.
        /// </summary>
        /// <value>
        /// The bytes consumed per sample.
        /// </value>
        protected override int BytesPerSample => bytesPerSample;

        /// <summary>
        /// Reads the audio to the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>
        /// The length of the data written.
        /// </returns>
        public override int Read(Span<float> buffer)
        {
            Span<int> span = stackalloc int[buffer.Length > ActualBufferMax ? ActualBufferMax : buffer.Length];
            var cursor = buffer;
            while (cursor.Length > 0)
            {
                var reader = cursor.Length >= span.Length ? span : span.Slice(0, cursor.Length);
                int u = Source.Read(MemoryMarshal.AsBytes(span)) / bytesPerSample;
                var wrote = reader.Slice(0, u);
                var dest = cursor.Slice(0, wrote.Length);
                if (wrote.Length != dest.Length)
                    new InvalidOperationException(
                        $"The {nameof(wrote)}'s length and {nameof(dest)}'s length are not equal! This is a bug!").Throw();
                if (IsEndiannessConversionRequired)
                {
                    for (int i = 0; i < wrote.Length && i < dest.Length; i++)
                    {
                        dest[i] = BinaryPrimitives.ReverseEndianness(wrote[i]) / divisor;
                    }
                }
                else
                {
                    for (int i = 0; i < wrote.Length && i < dest.Length; i++)
                    {
                        dest[i] = wrote[i] / divisor;
                    }
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
