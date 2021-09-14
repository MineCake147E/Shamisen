using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#if NETCOREAPP3_1_OR_GREATER
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
#endif

using Shamisen.Utils;

namespace Shamisen.Conversion.WaveToSampleConverters
{
    /// <summary>
    /// Converts 8-bit A-Law PCM to Sample.
    /// </summary>
    public sealed class MuLawToSampleConverter : WaveToSampleConverterBase
    {
        private ResizableBufferWrapper<byte> bufferWrapper;
        private const float Multiplier = 1 / 32768.0f;

        /// <summary>
        /// Initializes a new instance of the <see cref="MuLawToSampleConverter"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        public MuLawToSampleConverter(IReadableAudioSource<byte, IWaveFormat> source)
            : base(source, new SampleFormat(source.Format.Channels, source.Format.SampleRate)) => bufferWrapper = new ResizablePooledBufferWrapper<byte>(1);

        /// <summary>
        /// Gets the bytes consumed per sample.
        /// </summary>
        /// <value>
        /// The bytes consumed per sample.
        /// </value>
        protected override int BytesPerSample { get => 1; }

        /// <summary>
        /// Gets the remaining length of the <see cref="IAudioSource{TSample,TFormat}" /> in frames.<br />
        /// The <c>null</c> means that the <see cref="IAudioSource{TSample,TFormat}" /> continues infinitely.
        /// </summary>
        /// <value>
        /// The remaining length of the <see cref="IAudioSource{TSample,TFormat}" /> in frames.
        /// </value>
        public override ulong? Length => Source.Length;

        /// <summary>
        /// Gets the total length of the <see cref="IAudioSource{TSample,TFormat}" /> in frames.<br />
        /// The <c>null</c> means that the <see cref="IAudioSource{TSample,TFormat}" /> continues infinitely.
        /// </summary>
        /// <value>
        /// The total length of the <see cref="IAudioSource{TSample,TFormat}" /> in frames.
        /// </value>
        public override ulong? TotalLength => Source.TotalLength;

        /// <summary>
        /// Gets the position of the <see cref="IAudioSource{TSample,TFormat}" /> in frames.<br />
        /// The <c>null</c> means that the <see cref="IAudioSource{TSample,TFormat}" /> doesn't support this property.
        /// </summary>
        /// <value>
        /// The position of the <see cref="IAudioSource{TSample,TFormat}" /> in frames.
        /// </value>
        public override ulong? Position => Source.TotalLength;

        /// <summary>
        /// Gets the skip support of the <see cref="IAudioSource{TSample,TFormat}" />.
        /// </summary>
        /// <value>
        /// The skip support.
        /// </value>
        public override ISkipSupport? SkipSupport => Source.SkipSupport;

        /// <summary>
        /// Gets the seek support of the <see cref="IAudioSource{TSample,TFormat}" />.
        /// </summary>
        /// <value>
        /// The seek support.
        /// </value>
        public override ISeekSupport? SeekSupport => Source.SeekSupport;

        /// <summary>
        /// Reads the audio to the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>
        /// The length of the data written.
        /// </returns>
        public override ReadResult Read(Span<float> buffer)
        {
            int internalBufferLengthRequired = CheckBuffer(buffer.Length);
            var bytebuf = MemoryMarshal.Cast<float, byte>(buffer);
            //Resampling start
            Span<byte> srcBuffer = bytebuf.Slice(bytebuf.Length - internalBufferLengthRequired, internalBufferLengthRequired);
            Span<byte> readBuffer = srcBuffer.SliceAlign(Format.Channels);
            var rr = Source.Read(readBuffer);
            if (rr.HasData)
            {
                unchecked
                {
                    var rb = readBuffer.SliceWhile(rr.Length);
                    var wb = buffer.SliceWhile(rb.Length);
                    Process(rb, wb);
                    return wb.Length;
                }
            }
            else
            {
                return rr;
            }
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static void Process(Span<byte> rb, Span<float> wb)
        {
            unchecked
            {
#if NETCOREAPP3_1_OR_GREATER
                if (rb.Length > 8 && Sse41.IsSupported)
                {
                    ProcessSse41(rb, wb);
                    return;
                }
#endif
                ProcessStandard(rb, wb);
            }
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static void ProcessStandard(Span<byte> rb, Span<float> wb)
        {
            unchecked
            {
                for (int i = 0; i < rb.Length; i++)
                {
                    byte v = rb[i];
                    wb[i] = ConvertMuLawToSingle(v);
                }
            }
        }
#if NETCOREAPP3_1_OR_GREATER

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static void ProcessSse41(Span<byte> rb, Span<float> wb)
        {
            unchecked
            {
                var xmm0 = Vector128.Create(~0u);
                var xmm1 = Vector128.Create(0x8000_0000u);
                var xmm2 = Vector128.Create(0x3b84_0000u);
                var xmm3 = Vector128.Create(0x83f8_0000u);
                nint length = rb.Length;
                nint i;
                ref var rsi = ref MemoryMarshal.GetReference(rb);
                ref var rdi = ref MemoryMarshal.GetReference(wb);
                for (i = 0; i < length - 7; i += 8)
                {
                    var xmm4 = Vector128.CreateScalarUnsafe(Unsafe.As<byte, uint>(ref Unsafe.Add(ref rsi, i)));
                    var xmm5 = Vector128.CreateScalarUnsafe(Unsafe.As<byte, uint>(ref Unsafe.Add(ref rsi, i + 4)));
                    xmm4 = Sse2.Xor(xmm4, xmm0);
                    xmm5 = Sse2.Xor(xmm5, xmm0);
                    xmm4 = Sse41.ConvertToVector128Int32(xmm4.AsSByte()).AsUInt32();
                    xmm5 = Sse41.ConvertToVector128Int32(xmm5.AsSByte()).AsUInt32();
                    xmm4 = Sse2.ShiftLeftLogical(xmm4, 19);
                    xmm5 = Sse2.ShiftLeftLogical(xmm5, 19);
                    var xmm6 = Sse2.And(xmm4, xmm1);
                    var xmm7 = Sse2.And(xmm5, xmm1);
                    xmm6 = Sse2.Or(xmm6, xmm2);
                    xmm7 = Sse2.Or(xmm7, xmm2);
                    xmm4 = Sse2.And(xmm4, xmm3);
                    xmm5 = Sse2.And(xmm5, xmm3);
                    xmm4 = Sse2.Add(xmm4, xmm2);
                    xmm4 = Sse.Subtract(xmm4.AsSingle(), xmm6.AsSingle()).AsUInt32();
                    xmm5 = Sse2.Add(xmm5, xmm2);
                    xmm5 = Sse.Subtract(xmm5.AsSingle(), xmm7.AsSingle()).AsUInt32();
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i)) = xmm4.AsSingle();
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 4)) = xmm5.AsSingle();
                }
                for (; i < length; i++)
                {
                    var g = Unsafe.Add(ref rsi, i);
                    Unsafe.Add(ref rdi, i) = ConvertMuLawToSingle(g);
                }
            }
        }
#endif

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static float ConvertMuLawToSingle(byte value)
        {
            unchecked
            {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
                uint v = (uint)(sbyte)~value;
                v <<= 19;
                v &= 0x83f8_0000;
                var y = v & 0x8000_0000u + 0x3b84_0000u;
                var f = BitConverter.Int32BitsToSingle((int)y);
                v += 0x3b84_0000;
                return BitConverter.Int32BitsToSingle((int)v) - f;
#else
                uint v = (uint)(sbyte)~value;
                v <<= 19;
                v &= 0x83f8_0000;
                var y = v & 0x8000_0000u + 0x3b84_0000u;
                var f = Unsafe.As<uint, float>(ref y);
                v += 0x3b84_0000;
                return Unsafe.As<uint, float>(ref v) - f;
#endif
            }
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private int CheckBuffer(int sampleLengthOut) => sampleLengthOut;

        private void ExpandBuffer(int internalBufferLengthRequired) => bufferWrapper.Resize(internalBufferLengthRequired);

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
