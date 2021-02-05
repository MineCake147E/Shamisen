using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

using Shamisen.Utils;

namespace Shamisen.Conversion.WaveToSampleConverters
{
    /// <summary>
    /// Converts 8-bit A-Law PCM to Sample.
    /// </summary>
    public sealed class ALawToSampleConverter : WaveToSampleConverterBase
    {
        private ResizableBufferWrapper<byte> bufferWrapper;
        private const float Multiplier = 1 / 32768.0f;

        /// <summary>
        /// Initializes a new instance of the <see cref="ALawToSampleConverter"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="format">The format.</param>
        public ALawToSampleConverter(IReadableAudioSource<byte, IWaveFormat> source, SampleFormat format) : base(source, new SampleFormat(source.Format.SampleRate, source.Format.Channels))
        {
            bufferWrapper = new ResizablePooledBufferWrapper<byte>(1);
        }

        /// <summary>
        /// Gets the bytes consumed per sample.
        /// </summary>
        /// <value>
        /// The bytes consumed per sample.
        /// </value>
        protected override int BytesPerSample { get => 1; }

        /// <summary>
        /// Reads the audio to the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>
        /// The length of the data written.
        /// </returns>
        public override ReadResult Read(Span<float> buffer)
        {
            buffer = buffer.SliceAlign(Format.Channels);
            int internalBufferLengthRequired = CheckBuffer(buffer.Length);

            //Resampling start
            Span<byte> srcBuffer = bufferWrapper.Buffer.Slice(0, internalBufferLengthRequired);
            Span<byte> readBuffer = srcBuffer.SliceAlign(Format.Channels);
            var rr = Source.Read(readBuffer);
            if (rr.HasData)
            {
                unchecked
                {
                    var rb = readBuffer.Slice(rr.Length);
                    var wb = buffer.Slice(rb.Length);
                    for (int i = 0; i < rb.Length; i++)
                    {
                        byte v = rb[i];
                        var hj = ConvertALawToInt16(v);
                        wb[i] = hj * Multiplier;
                    }
                    return wb.Length;
                }
            }
            else
            {
                return ReadResult.WaitingForSource;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static short ConvertALawToInt16(byte value)
        {
            unchecked
            {
                byte v = value;
                bool s = v <= 127;
                uint ss = Unsafe.As<bool, byte>(ref s);
                v ^= (byte)0b01010101u;
                uint x = (uint)v & 0x7f;
                bool j = x > 0xf;
                uint a = Unsafe.As<bool, byte>(ref j);
                int exp = (int)(x >> 4);
                var man = x & 0b1111;
                man += a << 4;
                man = (man << 4) + 8;
                man <<= exp - (int)a;
                return (short)((man ^ (uint)-(int)ss) + ss);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int CheckBuffer(int sampleLengthOut)
        {
            int v = sampleLengthOut;
            int samplesRequired = v;
            int internalBufferLengthRequired = samplesRequired;
            if (internalBufferLengthRequired > bufferWrapper.Buffer.Length)
            {
                ExpandBuffer(internalBufferLengthRequired);
            }

            return internalBufferLengthRequired;
        }

        private void ExpandBuffer(int internalBufferLengthRequired)
        {
            bufferWrapper.Resize(internalBufferLengthRequired);
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
                    bufferWrapper.Dispose();
                }
                //bufferWrapper = null;
            }
            disposedValue = true;
        }
    }
}
