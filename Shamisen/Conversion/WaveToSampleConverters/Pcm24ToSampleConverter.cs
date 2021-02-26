using Shamisen.Extensions;

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Shamisen.Conversion.WaveToSampleConverters
{
    /// <summary>
    /// Converts 24-bit PCM to Sample.
    /// </summary>
    /// <seealso cref="WaveToSampleConverterBase" />
    public sealed class Pcm24ToSampleConverter : WaveToSampleConverterBase
    {
        private const float Divisor = 8388608.0f;
        private const int ActualBytesPerSample = 3;   //sizeof(Int24)
        private const int BufferMax = 1024;
        private int ActualBufferMax => BufferMax * Source.Format.Channels;

        private Memory<Int24> readBuffer;

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
        public Pcm24ToSampleConverter(IReadableAudioSource<byte, IWaveFormat> source, Endianness endianness = Endianness.Little)
            : base(source, new SampleFormat(source.Format.Channels, source.Format.SampleRate))
        {
            Endianness = endianness;
            readBuffer = new Int24[ActualBufferMax];
        }

        /// <summary>
        /// Gets the bytes consumed per sample.
        /// </summary>
        /// <value>
        /// The bytes consumed per sample.
        /// </value>
        protected override int BytesPerSample => ActualBytesPerSample;

        /// <summary>
        /// Gets the remaining length of the <see cref="IAudioSource{TSample,TFormat}" /> in frames.<br />
        /// The <c>null</c> means that the <see cref="IAudioSource{TSample,TFormat}" /> continues infinitely.
        /// </summary>
        /// <value>
        /// The remaining length of the <see cref="IAudioSource{TSample,TFormat}" /> in frames.
        /// </value>
        public override ulong? Length => Source.Length / ActualBytesPerSample;

        /// <summary>
        /// Gets the total length of the <see cref="IAudioSource{TSample,TFormat}" /> in frames.<br />
        /// The <c>null</c> means that the <see cref="IAudioSource{TSample,TFormat}" /> continues infinitely.
        /// </summary>
        /// <value>
        /// The total length of the <see cref="IAudioSource{TSample,TFormat}" /> in frames.
        /// </value>
        public override ulong? TotalLength => Source.TotalLength / ActualBytesPerSample;

        /// <summary>
        /// Gets the position of the <see cref="IAudioSource{TSample,TFormat}" /> in frames.<br />
        /// The <c>null</c> means that the <see cref="IAudioSource{TSample,TFormat}" /> doesn't support this property.
        /// </summary>
        /// <value>
        /// The position of the <see cref="IAudioSource{TSample,TFormat}" /> in frames.
        /// </value>
        public override ulong? Position => Source.TotalLength / ActualBytesPerSample;

        /// <summary>
        /// Gets the skip support of the <see cref="IAudioSource{TSample,TFormat}" />.
        /// </summary>
        /// <value>
        /// The skip support.
        /// </value>
        public override ISkipSupport? SkipSupport => Source.SkipSupport?.WithFraction(ActualBytesPerSample, 1);

        /// <summary>
        /// Gets the seek support of the <see cref="IAudioSource{TSample,TFormat}" />.
        /// </summary>
        /// <value>
        /// The seek support.
        /// </value>
        public override ISeekSupport? SeekSupport => Source.SeekSupport?.WithFraction(ActualBytesPerSample, 1);

        /// <summary>
        /// Reads the audio to the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>
        /// The length of the data written.
        /// </returns>
        public override ReadResult Read(Span<float> buffer)
        {
            Span<Int24> span = readBuffer.Span;
            var cursor = buffer;
            while (cursor.Length > 0)
            {
                var reader = span.SliceWhileIfLongerThan(cursor.Length);
                var rr = Source.Read(MemoryMarshal.AsBytes(reader));
                if (rr.HasNoData)
                {
                    int len = buffer.Length - cursor.Length;
                    return rr.IsEndOfStream && len <= 0 ? rr : len;
                }
                int u = rr.Length / BytesPerSample;
                var wrote = reader.Slice(0, u);
                var dest = cursor.Slice(0, wrote.Length);
                if (wrote.Length != dest.Length)
                    new InvalidOperationException(
                        $"The {nameof(wrote)}'s length and {nameof(dest)}'s length are not equal! This is a bug!").Throw();
                if (IsEndiannessConversionRequired)
                {
                    for (int i = 0; i < wrote.Length && i < dest.Length; i++)
                    {
                        dest[i] = Int24.ReverseEndianness(wrote[i]) / Divisor;
                    }
                }
                else
                {
                    for (int i = 0; i < wrote.Length && i < dest.Length; i++)
                    {
                        dest[i] = wrote[i] / Divisor;
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
