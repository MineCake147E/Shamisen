using System;
using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#if NETCOREAPP3_0_OR_GREATER
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
#endif

using Shamisen.Extensions;
using Shamisen.Optimization;

namespace Shamisen.Conversion.WaveToSampleConverters
{
    /// <summary>
    /// Converts 16-bit PCM to Sample.
    /// </summary>
    /// <seealso cref="WaveToSampleConverterBase" />
    public sealed class Pcm16ToSampleConverter : WaveToSampleConverterBase
    {
        private const float Multiplier = 1 / 32768.0f;
        private const int ActualBytesPerSample = sizeof(short);
        private const int BufferMax = 2048;
        private int ActualBufferMax => BufferMax * Source.Format.Channels;

        private Memory<short> readBuffer;

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
        public Pcm16ToSampleConverter(IReadableAudioSource<byte, IWaveFormat> source, Endianness endianness = Endianness.Little)
            : base(source, new SampleFormat(source.Format.Channels, source.Format.SampleRate))
        {
            Endianness = endianness;
            readBuffer = new short[ActualBufferMax];
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
            //Since sizeof(float) is greater than sizeof(short), no external buffer is required.
            var reader = MemoryMarshal.Cast<float, short>(buffer);
            //Source data will be written to later half of buffer.
            reader = reader.Slice(reader.Length - buffer.Length, buffer.Length);
            var rr = Source.Read(MemoryMarshal.AsBytes(reader));
            if (rr.HasNoData)
            {
                int len = 0;
                return rr.IsEndOfStream && len <= 0 ? rr : len;
            }
            int u = rr.Length / ActualBytesPerSample;
            var wrote = reader.Slice(0, u);
            var dest = buffer.Slice(0, wrote.Length);
            if (wrote.Length != dest.Length)
            {
                new InvalidOperationException(
                    $"The {nameof(wrote)}'s length and {nameof(dest)}'s length are not equal! This is a bug!").Throw();
            }

            if (IsEndiannessConversionRequired)
            {
                ProcessReversed(wrote, dest);
            }
            else
            {
                ProcessNormal(dest, wrote);
            }
            if (u != reader.Length) return u;  //The Source doesn't fill whole reader so return here.
            return buffer.Length;
        }



        #region ProcessNormal
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static void ProcessNormal(Span<float> buffer, ReadOnlySpan<short> source)
        {
#if NETCOREAPP3_1_OR_GREATER
            if (buffer.Length >= 128 && IntrinsicsUtils.AvoidAvxHeavyOperations && Avx2.IsSupported)
            {
                ProcessNormalAvx2(buffer, source);
                return;
            }
            if (Sse2.IsSupported)
            {
                ProcessNormalSse41(buffer, source);
                return;
            }
#endif
            ProcessNormalStandard(buffer, source);
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ProcessNormalStandard(Span<float> buffer, ReadOnlySpan<short> source)
        {
            Vector<float> mul = new(Multiplier);
            ref var rdi = ref MemoryMarshal.GetReference(buffer);
            ref var rsi = ref MemoryMarshal.GetReference(source);
            nint i, length = buffer.Length;
            int size = Vector<float>.Count;
            int sizeEpi16 = Vector<short>.Count;
            for (i = 0; i < length - 8 * Vector<float>.Count + 1; i += 8 * Vector<float>.Count)
            {
                Vector.Widen(Unsafe.As<short, Vector<short>>(ref Unsafe.Add(ref rsi, i + sizeEpi16 * 0)), out var v0_nd, out var v1_nd);
                Vector.Widen(Unsafe.As<short, Vector<short>>(ref Unsafe.Add(ref rsi, i + sizeEpi16 * 1)), out var v2_nd, out var v3_nd);
                var v0_ns = Vector.ConvertToSingle(v0_nd);
                var v1_ns = Vector.ConvertToSingle(v1_nd);
                var v2_ns = Vector.ConvertToSingle(v2_nd);
                var v3_ns = Vector.ConvertToSingle(v3_nd);
                v0_ns *= mul;
                v1_ns *= mul;
                v2_ns *= mul;
                v3_ns *= mul;
                Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rdi, i + size * 0)) = v0_ns;
                Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rdi, i + size * 1)) = v1_ns;
                Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rdi, i + size * 2)) = v2_ns;
                Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rdi, i + size * 3)) = v3_ns;
                Vector.Widen(Unsafe.As<short, Vector<short>>(ref Unsafe.Add(ref rsi, i + sizeEpi16 * 2)), out v0_nd, out v1_nd);
                Vector.Widen(Unsafe.As<short, Vector<short>>(ref Unsafe.Add(ref rsi, i + sizeEpi16 * 3)), out v2_nd, out v3_nd);
                v0_ns = Vector.ConvertToSingle(v0_nd);
                v1_ns = Vector.ConvertToSingle(v1_nd);
                v2_ns = Vector.ConvertToSingle(v2_nd);
                v3_ns = Vector.ConvertToSingle(v3_nd);
                v0_ns *= mul;
                v1_ns *= mul;
                v2_ns *= mul;
                v3_ns *= mul;
                Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rdi, i + size * 4)) = v0_ns;
                Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rdi, i + size * 5)) = v1_ns;
                Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rdi, i + size * 6)) = v2_ns;
                Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rdi, i + size * 7)) = v3_ns;
            }
            for (; i < length; i++)
            {
                Unsafe.Add(ref rdi, i) = Unsafe.Add(ref rsi, i) * mul[0];
            }
        }
#if NETCOREAPP3_1_OR_GREATER

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ProcessNormalAvx2(Span<float> buffer, ReadOnlySpan<short> source)
        {
            Vector256<float> mul = Vector256.Create(Multiplier);
            ref var rdi = ref MemoryMarshal.GetReference(buffer);
            ref var rsi = ref MemoryMarshal.GetReference(source);
            nint i, length = buffer.Length;
            //Loop for Intel CPUs in 256-bit AVX2 for better throughput
            for (i = 0; i < length - 63; i += 64)
            {
                var ymm0 = Avx2.ConvertToVector256Int32(Unsafe.As<short, Vector128<short>>(ref Unsafe.Add(ref rsi, i + 0))).AsSingle();
                var ymm1 = Avx2.ConvertToVector256Int32(Unsafe.As<short, Vector128<short>>(ref Unsafe.Add(ref rsi, i + 8))).AsSingle();
                var ymm2 = Avx2.ConvertToVector256Int32(Unsafe.As<short, Vector128<short>>(ref Unsafe.Add(ref rsi, i + 16))).AsSingle();
                var ymm3 = Avx2.ConvertToVector256Int32(Unsafe.As<short, Vector128<short>>(ref Unsafe.Add(ref rsi, i + 24))).AsSingle();
                ymm0 = Avx.ConvertToVector256Single(ymm0.AsInt32());
                ymm1 = Avx.ConvertToVector256Single(ymm1.AsInt32());
                ymm2 = Avx.ConvertToVector256Single(ymm2.AsInt32());
                ymm3 = Avx.ConvertToVector256Single(ymm3.AsInt32());
                ymm0 = Avx.Multiply(ymm0, mul);
                ymm1 = Avx.Multiply(ymm1, mul);
                ymm2 = Avx.Multiply(ymm2, mul);
                ymm3 = Avx.Multiply(ymm3, mul);
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 0)) = ymm0;
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 8)) = ymm1;
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 16)) = ymm2;
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 24)) = ymm3;
                ymm0 = Avx2.ConvertToVector256Int32(Unsafe.As<short, Vector128<short>>(ref Unsafe.Add(ref rsi, i + 32))).AsSingle();
                ymm1 = Avx2.ConvertToVector256Int32(Unsafe.As<short, Vector128<short>>(ref Unsafe.Add(ref rsi, i + 40))).AsSingle();
                ymm2 = Avx2.ConvertToVector256Int32(Unsafe.As<short, Vector128<short>>(ref Unsafe.Add(ref rsi, i + 48))).AsSingle();
                ymm3 = Avx2.ConvertToVector256Int32(Unsafe.As<short, Vector128<short>>(ref Unsafe.Add(ref rsi, i + 56))).AsSingle();
                ymm0 = Avx.ConvertToVector256Single(ymm0.AsInt32());
                ymm1 = Avx.ConvertToVector256Single(ymm1.AsInt32());
                ymm2 = Avx.ConvertToVector256Single(ymm2.AsInt32());
                ymm3 = Avx.ConvertToVector256Single(ymm3.AsInt32());
                ymm0 = Avx.Multiply(ymm0, mul);
                ymm1 = Avx.Multiply(ymm1, mul);
                ymm2 = Avx.Multiply(ymm2, mul);
                ymm3 = Avx.Multiply(ymm3, mul);
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 32)) = ymm0;
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 40)) = ymm1;
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 48)) = ymm2;
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 56)) = ymm3;
            }
            for (; i < length - 7; i += 8)
            {
                var xmm0 = Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(Unsafe.As<short, ulong>(ref Unsafe.Add(ref rsi, i + 0))).AsInt16()).AsSingle();
                var xmm1 = Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(Unsafe.As<short, ulong>(ref Unsafe.Add(ref rsi, i + 4))).AsInt16()).AsSingle();
                xmm0 = Sse2.ConvertToVector128Single(xmm0.AsInt32());
                xmm1 = Sse2.ConvertToVector128Single(xmm1.AsInt32());
                xmm0 = Sse.Multiply(xmm0, mul.GetLower());
                xmm1 = Sse.Multiply(xmm1, mul.GetLower());
                Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 0)) = xmm0;
                Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 4)) = xmm1;
            }
            for (; i < length; i++)
            {
                Unsafe.Add(ref rdi, i) = Unsafe.Add(ref rsi, i) * mul.GetElement(0);
            }
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ProcessNormalSse41(Span<float> buffer, ReadOnlySpan<short> source)
        {
            Vector128<float> mul = Vector128.Create(Multiplier);
            ref var rdi = ref MemoryMarshal.GetReference(buffer);
            ref var rsi = ref MemoryMarshal.GetReference(source);
            nint i, length = buffer.Length;
            //Loop for Haswell in 128-bit AVX for better frequency behaviour
            for (i = 0; i < length - 31; i += 32)
            {
#if DEBUG
                //Suboptimal but GC proof
                var xmm0 = Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(Unsafe.As<short, ulong>(ref Unsafe.Add(ref rsi, i + 0))).AsInt16()).AsSingle();
                var xmm1 = Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(Unsafe.As<short, ulong>(ref Unsafe.Add(ref rsi, i + 4))).AsInt16()).AsSingle();
                var xmm2 = Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(Unsafe.As<short, ulong>(ref Unsafe.Add(ref rsi, i + 8))).AsInt16()).AsSingle();
                var xmm3 = Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(Unsafe.As<short, ulong>(ref Unsafe.Add(ref rsi, i + 12))).AsInt16()).AsSingle();
                xmm0 = Sse2.ConvertToVector128Single(xmm0.AsInt32());
                xmm1 = Sse2.ConvertToVector128Single(xmm1.AsInt32());
                xmm2 = Sse2.ConvertToVector128Single(xmm2.AsInt32());
                xmm3 = Sse2.ConvertToVector128Single(xmm3.AsInt32());
                xmm0 = Sse.Multiply(xmm0, mul);
                xmm1 = Sse.Multiply(xmm1, mul);
                xmm2 = Sse.Multiply(xmm2, mul);
                xmm3 = Sse.Multiply(xmm3, mul);
                Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 0)) = xmm0;
                Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 4)) = xmm1;
                Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 8)) = xmm2;
                Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 12)) = xmm3;
                xmm0 = Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(Unsafe.As<short, ulong>(ref Unsafe.Add(ref rsi, i + 16 + 0))).AsInt16()).AsSingle();
                xmm1 = Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(Unsafe.As<short, ulong>(ref Unsafe.Add(ref rsi, i + 16 + 4))).AsInt16()).AsSingle();
                xmm2 = Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(Unsafe.As<short, ulong>(ref Unsafe.Add(ref rsi, i + 16 + 8))).AsInt16()).AsSingle();
                xmm3 = Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(Unsafe.As<short, ulong>(ref Unsafe.Add(ref rsi, i + 16 + 12))).AsInt16()).AsSingle();
                xmm0 = Sse2.ConvertToVector128Single(xmm0.AsInt32());
                xmm1 = Sse2.ConvertToVector128Single(xmm1.AsInt32());
                xmm2 = Sse2.ConvertToVector128Single(xmm2.AsInt32());
                xmm3 = Sse2.ConvertToVector128Single(xmm3.AsInt32());
                xmm0 = Sse.Multiply(xmm0, mul);
                xmm1 = Sse.Multiply(xmm1, mul);
                xmm2 = Sse.Multiply(xmm2, mul);
                xmm3 = Sse.Multiply(xmm3, mul);
                Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 16 + 0)) = xmm0;
                Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 16 + 4)) = xmm1;
                Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 16 + 8)) = xmm2;
                Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 16 + 12)) = xmm3;
#else
                unsafe
                {
                    var xmm0 = Sse41.ConvertToVector128Int32((short*)Unsafe.AsPointer(ref Unsafe.Add(ref rsi, i + 0))).AsSingle();
                    var xmm1 = Sse41.ConvertToVector128Int32((short*)Unsafe.AsPointer(ref Unsafe.Add(ref rsi, i + 4))).AsSingle();
                    var xmm2 = Sse41.ConvertToVector128Int32((short*)Unsafe.AsPointer(ref Unsafe.Add(ref rsi, i + 8))).AsSingle();
                    var xmm3 = Sse41.ConvertToVector128Int32((short*)Unsafe.AsPointer(ref Unsafe.Add(ref rsi, i + 12))).AsSingle();
                    xmm0 = Sse2.ConvertToVector128Single(xmm0.AsInt32());
                    xmm1 = Sse2.ConvertToVector128Single(xmm1.AsInt32());
                    xmm2 = Sse2.ConvertToVector128Single(xmm2.AsInt32());
                    xmm3 = Sse2.ConvertToVector128Single(xmm3.AsInt32());
                    xmm0 = Sse.Multiply(xmm0, mul);
                    xmm1 = Sse.Multiply(xmm1, mul);
                    xmm2 = Sse.Multiply(xmm2, mul);
                    xmm3 = Sse.Multiply(xmm3, mul);
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 0)) = xmm0;
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 4)) = xmm1;
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 8)) = xmm2;
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 12)) = xmm3;
                    xmm0 = Sse41.ConvertToVector128Int32((short*)Unsafe.AsPointer(ref Unsafe.Add(ref rsi, i + 16 + 0))).AsSingle();
                    xmm1 = Sse41.ConvertToVector128Int32((short*)Unsafe.AsPointer(ref Unsafe.Add(ref rsi, i + 16 + 4))).AsSingle();
                    xmm2 = Sse41.ConvertToVector128Int32((short*)Unsafe.AsPointer(ref Unsafe.Add(ref rsi, i + 16 + 8))).AsSingle();
                    xmm3 = Sse41.ConvertToVector128Int32((short*)Unsafe.AsPointer(ref Unsafe.Add(ref rsi, i + 16 + 12))).AsSingle();
                    xmm0 = Sse2.ConvertToVector128Single(xmm0.AsInt32());
                    xmm1 = Sse2.ConvertToVector128Single(xmm1.AsInt32());
                    xmm2 = Sse2.ConvertToVector128Single(xmm2.AsInt32());
                    xmm3 = Sse2.ConvertToVector128Single(xmm3.AsInt32());
                    xmm0 = Sse.Multiply(xmm0, mul);
                    xmm1 = Sse.Multiply(xmm1, mul);
                    xmm2 = Sse.Multiply(xmm2, mul);
                    xmm3 = Sse.Multiply(xmm3, mul);
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 16 + 0)) = xmm0;
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 16 + 4)) = xmm1;
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 16 + 8)) = xmm2;
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 16 + 12)) = xmm3;
                }
#endif
            }
            for (; i < length; i++)
            {
                Unsafe.Add(ref rdi, i) = Unsafe.Add(ref rsi, i) * mul.GetElement(0);
            }
        }
#endif
        #endregion

        #region ProcessReversed
        internal static void ProcessReversed(Span<short> wrote, Span<float> dest)
        {
            for (int i = 0; i < wrote.Length && i < dest.Length; i++)
            {
                dest[i] = BinaryPrimitives.ReverseEndianness(wrote[i]) * Multiplier;
            }
        }
        #endregion
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
