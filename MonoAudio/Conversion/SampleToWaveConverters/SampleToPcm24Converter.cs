using MonoAudio.Extensions;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace MonoAudio.Conversion.SampleToWaveConverters
{
    /// <summary>
    /// Converts samples to 24-bit PCM.
    /// </summary>
    /// <seealso cref="SampleToWaveConverterBase" />
    public sealed class SampleToPcm24Converter : SampleToWaveConverterBase
    {
        private const float Multiplier = 8388608.0f;
        private const int ActualBytesPerSample = 3;   //sizeof(Int24)
        private const int BufferMax = 1024;
        private int ActualBufferMax => BufferMax - (BufferMax % Source.Format.Channels);

        private Memory<Int24> dsmLastOutput;
        private Memory<float> dsmAccumulator;
        private int dsmChannelPointer = 0;
        private Memory<float> readBuffer;

        /// <summary>
        /// Initializes a new instance of the <see cref="SampleToPcm24Converter"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="accuracyNeeded">Turns on <see cref="AccuracyMode"/> when <c>true</c>.</param>
        /// <param name="endianness">The destination endianness.</param>
        public SampleToPcm24Converter(IReadableAudioSource<float, SampleFormat> source, bool accuracyNeeded = true, Endianness endianness = Endianness.Little)
            : base(source, new WaveFormat(source.Format.SampleRate, 24, source.Format.Channels, AudioEncoding.LinearPcm))
        {
            if (accuracyNeeded)
            {
                dsmAccumulator = new float[source.Format.Channels];
                dsmLastOutput = new Int24[source.Format.Channels];
            }
            AccuracyMode = accuracyNeeded;
            Endianness = endianness;
            readBuffer = new float[ActualBufferMax];
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="SampleToPcm24Converter"/> does the 24-bit Delta-Sigma modulation.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the accuracy mode is turned on; otherwise, <c>false</c>.
        /// </value>
        public bool AccuracyMode { get; }

        /// <summary>
        /// Gets the endianness.
        /// </summary>
        /// <value>
        /// The endianness.
        /// </value>
        public Endianness Endianness { get; }

        private bool IsEndiannessConversionRequired => Endianness != EndiannessExtensions.EnvironmentEndianness;

        /// <summary>
        /// Gets the bytes consumed per sample.
        /// </summary>
        /// <value>
        /// The bytes consumed per sample.
        /// </value>
        protected override int BytesPerSample => ActualBytesPerSample;

        /// <summary>
        /// Reads the audio to the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>
        /// The length of the data written.
        /// </returns>
        public override ReadResult Read(Span<byte> buffer)
        {
            var cursor = MemoryMarshal.Cast<byte, Int24>(buffer);
            while (cursor.Length > 0)
            {
                var reader = cursor.Length >= readBuffer.Length ? readBuffer : readBuffer.Slice(0, cursor.Length);
                var rr = Source.Read(reader.Span);
                if (rr.HasNoData) return buffer.Length - cursor.Length;
                int u = rr.Length;
                var wrote = reader.Span.Slice(0, u);
                var dest = cursor.Slice(0, wrote.Length);
                if (wrote.Length != dest.Length)
                    new InvalidOperationException(
                        $"The {nameof(wrote)}'s length and {nameof(dest)}'s length are not equal! This is a bug!").Throw();
                if (AccuracyMode)
                {
                    var dsmAcc = dsmAccumulator.Span;
                    var dsmLastOut = dsmLastOutput.Span;
                    dsmChannelPointer %= dsmAcc.Length;
                    for (int i = 0; i < dest.Length; i++)
                    {
                        var diff = wrote[i] - (dsmLastOut[dsmChannelPointer] / Multiplier);
                        dsmAcc[dsmChannelPointer] += diff;
                        var v = dsmLastOut[dsmChannelPointer] = Convert(dsmAcc[dsmChannelPointer]);
                        dest[i] = IsEndiannessConversionRequired ? Int24.ReverseEndianness(v) : v;
                        dsmChannelPointer = ++dsmChannelPointer % dsmAcc.Length;
                    }
                }
                else
                {
                    for (int i = 0; i < dest.Length; i++)
                    {
                        var v = Convert(wrote[i]);
                        dest[i] = IsEndiannessConversionRequired ? Int24.ReverseEndianness(v) : v;
                    }
                }
                cursor = cursor.Slice(dest.Length);
                if (u != reader.Length) return buffer.Length - cursor.Length;  //The Source doesn't fill whole reader so return here.
            }
            return buffer.Length;
        }

        private static Int24 Convert(float srcval)
        {
            return (Int24)Math.Min(Int24.MaxValue, Math.Max(srcval * Multiplier, Int24.MinValue));
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
                dsmLastOutput = default;
                dsmAccumulator = default;
                readBuffer = default;
            }
            disposedValue = true;
        }
    }
}
