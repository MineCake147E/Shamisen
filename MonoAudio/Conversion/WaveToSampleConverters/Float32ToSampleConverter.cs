using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace MonoAudio.Conversion.WaveToSampleConverters
{
    /// <summary>
    /// Converts 32-bit IeeeFloat PCM to Sample.
    /// </summary>
    /// <seealso cref="MonoAudio.Conversion.WaveToSampleConverters.WaveToSampleConverterBase" />
    public sealed class Float32ToSampleConverter : WaveToSampleConverterBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Float32ToSampleConverter"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        public Float32ToSampleConverter(IReadableAudioSource<byte, IWaveFormat> source)
            : base(source, new SampleFormat(source.Format.SampleRate, source.Format.Channels))
        {
        }

        /// <summary>
        /// Gets the bytes consumed per sample.
        /// </summary>
        /// <value>
        /// The bytes consumed per sample.
        /// </value>
        protected override int BytesPerSample => 4;

        /// <summary>
        /// Reads the audio to the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>
        /// The length of the data written.
        /// </returns>
        public override ReadResult Read(Span<float> buffer)
        {
            var g = Source.Read(MemoryMarshal.Cast<float, byte>(buffer));
            return g.HasData ? g.Length / 4 : g;
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
                //
            }
            disposedValue = true;
        }
    }
}
