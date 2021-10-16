using System;
using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#if NETCOREAPP3_0_OR_GREATER
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
#endif

using Shamisen.Optimization;
using Shamisen.Utils;

namespace Shamisen.Conversion.WaveToSampleConverters
{
    /// <summary>
    /// Converts 32-bit PCM to Sample.
    /// </summary>
    /// <seealso cref="WaveToSampleConverterBase" />
    public sealed class Pcm32ToSampleConverter : WaveToSampleConverterBase
    {
        private const float Divisor = 2147483648.0f;
        private const float Multiplier = 1.0f / Divisor;
        private const int ActualBytesPerSample = sizeof(int);
        private const int BufferMax = 1024;
        private int ActualBufferMax => BufferMax - (BufferMax % Source.Format.Channels);

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
        public Pcm32ToSampleConverter(IReadableAudioSource<byte, IWaveFormat> source, Endianness endianness = Endianness.Little)
            : base(source, new SampleFormat(source.Format.Channels, source.Format.SampleRate))
        {
            Endianness = endianness;
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
            //Since sizeof(float) is the same as sizeof(int), no external buffer is required.
            var rr = Source.Read(MemoryMarshal.Cast<float, byte>(buffer));
            if (rr.HasNoData)
            {
                return rr;
            }
            buffer = buffer.SliceWhile(rr.Length / 4);
            if (IsEndiannessConversionRequired)
            {
                ProcessReversed(buffer);
            }
            else
            {
                ProcessNormal(buffer);
            }
            return buffer.Length;
        }

        #region ProcessNormal
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static void ProcessNormal(Span<float> buffer)
        {
#if NETCOREAPP3_1_OR_GREATER
            if (buffer.Length >= 512 && IntrinsicsUtils.EnableExtremeLoopUnrolling && Avx2.IsSupported)
            {
                ProcessNormalAvx2ExtremeUnroll(buffer);
                return;
            }
            if (buffer.Length >= 128 && !IntrinsicsUtils.AvoidAvxHeavyOperations && Avx2.IsSupported)
            {
                ProcessNormalAvx2(buffer);
                return;
            }
            if (Sse2.IsSupported)
            {
                ProcessNormalSse2(buffer);
                return;
            }
#endif
            ProcessNormalStandard(buffer);
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ProcessNormalStandard(Span<float> buffer)
        {
            Vector<float> mul = new(Multiplier);
            ref float rdi = ref MemoryMarshal.GetReference(buffer);
            ref int rsi = ref Unsafe.As<float, int>(ref rdi);
            nint i, length = buffer.Length;
            int size = Vector<float>.Count;
            for (i = 0; i < length - 8 * Vector<float>.Count + 1; i += 8 * Vector<float>.Count)
            {
                var v0_ns = Vector.ConvertToSingle(Unsafe.As<float, Vector<int>>(ref Unsafe.Add(ref rdi, i + size * 0)));
                var v1_ns = Vector.ConvertToSingle(Unsafe.As<float, Vector<int>>(ref Unsafe.Add(ref rdi, i + size * 1)));
                var v2_ns = Vector.ConvertToSingle(Unsafe.As<float, Vector<int>>(ref Unsafe.Add(ref rdi, i + size * 2)));
                var v3_ns = Vector.ConvertToSingle(Unsafe.As<float, Vector<int>>(ref Unsafe.Add(ref rdi, i + size * 3)));
                v0_ns *= mul;
                v1_ns *= mul;
                v2_ns *= mul;
                v3_ns *= mul;
                Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rdi, i + size * 0)) = v0_ns;
                Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rdi, i + size * 1)) = v1_ns;
                Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rdi, i + size * 2)) = v2_ns;
                Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rdi, i + size * 3)) = v3_ns;
                v0_ns = Vector.ConvertToSingle(Unsafe.As<float, Vector<int>>(ref Unsafe.Add(ref rdi, i + size * 4)));
                v1_ns = Vector.ConvertToSingle(Unsafe.As<float, Vector<int>>(ref Unsafe.Add(ref rdi, i + size * 5)));
                v2_ns = Vector.ConvertToSingle(Unsafe.As<float, Vector<int>>(ref Unsafe.Add(ref rdi, i + size * 6)));
                v3_ns = Vector.ConvertToSingle(Unsafe.As<float, Vector<int>>(ref Unsafe.Add(ref rdi, i + size * 7)));
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
        internal static void ProcessNormalAvx2(Span<float> buffer)
        {
            var mul = Vector256.Create(Multiplier);
            ref float rdi = ref MemoryMarshal.GetReference(buffer);
            nint i, length = buffer.Length;
            //Loop for Intel CPUs in 256-bit AVX2 for better throughput
            for (i = 0; i < length - 63; i += 64)
            {
                var ymm0 = Avx.ConvertToVector256Single(Unsafe.As<float, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 0)));
                var ymm1 = Avx.ConvertToVector256Single(Unsafe.As<float, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 8)));
                var ymm2 = Avx.ConvertToVector256Single(Unsafe.As<float, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 16)));
                var ymm3 = Avx.ConvertToVector256Single(Unsafe.As<float, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 24)));
                ymm0 = Avx.Multiply(ymm0, mul);
                ymm1 = Avx.Multiply(ymm1, mul);
                ymm2 = Avx.Multiply(ymm2, mul);
                ymm3 = Avx.Multiply(ymm3, mul);
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 0)) = ymm0;
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 8)) = ymm1;
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 16)) = ymm2;
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 24)) = ymm3;
                ymm0 = Avx.ConvertToVector256Single(Unsafe.As<float, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 32)));
                ymm1 = Avx.ConvertToVector256Single(Unsafe.As<float, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 40)));
                ymm2 = Avx.ConvertToVector256Single(Unsafe.As<float, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 48)));
                ymm3 = Avx.ConvertToVector256Single(Unsafe.As<float, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 56)));
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
                var xmm0 = Sse2.ConvertToVector128Single(Unsafe.As<float, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 0)));
                var xmm1 = Sse2.ConvertToVector128Single(Unsafe.As<float, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 4)));
                xmm0 = Sse.Multiply(xmm0, mul.GetLower());
                xmm1 = Sse.Multiply(xmm1, mul.GetLower());
                Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 0)) = xmm0;
                Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 4)) = xmm1;
            }
            for (; i < length; i++)
            {
                Unsafe.Add(ref rdi, i) = Unsafe.As<float, int>(ref Unsafe.Add(ref rdi, i)) * mul.GetElement(0);
            }
        }
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        internal static void ProcessNormalAvx2ExtremeUnroll(Span<float> buffer)
        {
            var mul = Vector256.Create(Multiplier);
            ref float rdi = ref MemoryMarshal.GetReference(buffer);
            nint i, length = buffer.Length;
            //Loop for Zen3 in 256-bit AVX2
            //Does Zen3 branch slowly?
            #region Extremely unrolled loop
            for (i = 0; i < length - 255; i += 256)
            {
                var ymm0 = Avx.ConvertToVector256Single(Unsafe.As<float, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 0)));
                var ymm1 = Avx.ConvertToVector256Single(Unsafe.As<float, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 8)));
                var ymm2 = Avx.ConvertToVector256Single(Unsafe.As<float, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 16)));
                var ymm3 = Avx.ConvertToVector256Single(Unsafe.As<float, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 24)));
                ymm0 = Avx.Multiply(ymm0, mul);
                ymm1 = Avx.Multiply(ymm1, mul);
                ymm2 = Avx.Multiply(ymm2, mul);
                ymm3 = Avx.Multiply(ymm3, mul);
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 0)) = ymm0;
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 8)) = ymm1;
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 16)) = ymm2;
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 24)) = ymm3;
                ymm0 = Avx.ConvertToVector256Single(Unsafe.As<float, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 32)));
                ymm1 = Avx.ConvertToVector256Single(Unsafe.As<float, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 40)));
                ymm2 = Avx.ConvertToVector256Single(Unsafe.As<float, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 48)));
                ymm3 = Avx.ConvertToVector256Single(Unsafe.As<float, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 56)));
                ymm0 = Avx.Multiply(ymm0, mul);
                ymm1 = Avx.Multiply(ymm1, mul);
                ymm2 = Avx.Multiply(ymm2, mul);
                ymm3 = Avx.Multiply(ymm3, mul);
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 32)) = ymm0;
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 40)) = ymm1;
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 48)) = ymm2;
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 56)) = ymm3;
                ymm0 = Avx.ConvertToVector256Single(Unsafe.As<float, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 64 + 0)));
                ymm1 = Avx.ConvertToVector256Single(Unsafe.As<float, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 64 + 8)));
                ymm2 = Avx.ConvertToVector256Single(Unsafe.As<float, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 64 + 16)));
                ymm3 = Avx.ConvertToVector256Single(Unsafe.As<float, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 64 + 24)));
                ymm0 = Avx.Multiply(ymm0, mul);
                ymm1 = Avx.Multiply(ymm1, mul);
                ymm2 = Avx.Multiply(ymm2, mul);
                ymm3 = Avx.Multiply(ymm3, mul);
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 64 + 0)) = ymm0;
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 64 + 8)) = ymm1;
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 64 + 16)) = ymm2;
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 64 + 24)) = ymm3;
                ymm0 = Avx.ConvertToVector256Single(Unsafe.As<float, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 64 + 32)));
                ymm1 = Avx.ConvertToVector256Single(Unsafe.As<float, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 64 + 40)));
                ymm2 = Avx.ConvertToVector256Single(Unsafe.As<float, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 64 + 48)));
                ymm3 = Avx.ConvertToVector256Single(Unsafe.As<float, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 64 + 56)));
                ymm0 = Avx.Multiply(ymm0, mul);
                ymm1 = Avx.Multiply(ymm1, mul);
                ymm2 = Avx.Multiply(ymm2, mul);
                ymm3 = Avx.Multiply(ymm3, mul);
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 64 + 32)) = ymm0;
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 64 + 40)) = ymm1;
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 64 + 48)) = ymm2;
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 64 + 56)) = ymm3;
                ymm0 = Avx.ConvertToVector256Single(Unsafe.As<float, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 128 + 0)));
                ymm1 = Avx.ConvertToVector256Single(Unsafe.As<float, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 128 + 8)));
                ymm2 = Avx.ConvertToVector256Single(Unsafe.As<float, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 128 + 16)));
                ymm3 = Avx.ConvertToVector256Single(Unsafe.As<float, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 128 + 24)));
                ymm0 = Avx.Multiply(ymm0, mul);
                ymm1 = Avx.Multiply(ymm1, mul);
                ymm2 = Avx.Multiply(ymm2, mul);
                ymm3 = Avx.Multiply(ymm3, mul);
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 128 + 0)) = ymm0;
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 128 + 8)) = ymm1;
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 128 + 16)) = ymm2;
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 128 + 24)) = ymm3;
                ymm0 = Avx.ConvertToVector256Single(Unsafe.As<float, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 128 + 32)));
                ymm1 = Avx.ConvertToVector256Single(Unsafe.As<float, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 128 + 40)));
                ymm2 = Avx.ConvertToVector256Single(Unsafe.As<float, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 128 + 48)));
                ymm3 = Avx.ConvertToVector256Single(Unsafe.As<float, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 128 + 56)));
                ymm0 = Avx.Multiply(ymm0, mul);
                ymm1 = Avx.Multiply(ymm1, mul);
                ymm2 = Avx.Multiply(ymm2, mul);
                ymm3 = Avx.Multiply(ymm3, mul);
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 128 + 32)) = ymm0;
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 128 + 40)) = ymm1;
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 128 + 48)) = ymm2;
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 128 + 56)) = ymm3;
                ymm0 = Avx.ConvertToVector256Single(Unsafe.As<float, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 192 + 0)));
                ymm1 = Avx.ConvertToVector256Single(Unsafe.As<float, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 192 + 8)));
                ymm2 = Avx.ConvertToVector256Single(Unsafe.As<float, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 192 + 16)));
                ymm3 = Avx.ConvertToVector256Single(Unsafe.As<float, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 192 + 24)));
                ymm0 = Avx.Multiply(ymm0, mul);
                ymm1 = Avx.Multiply(ymm1, mul);
                ymm2 = Avx.Multiply(ymm2, mul);
                ymm3 = Avx.Multiply(ymm3, mul);
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 192 + 0)) = ymm0;
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 192 + 8)) = ymm1;
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 192 + 16)) = ymm2;
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 192 + 24)) = ymm3;
                ymm0 = Avx.ConvertToVector256Single(Unsafe.As<float, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 192 + 32)));
                ymm1 = Avx.ConvertToVector256Single(Unsafe.As<float, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 192 + 40)));
                ymm2 = Avx.ConvertToVector256Single(Unsafe.As<float, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 192 + 48)));
                ymm3 = Avx.ConvertToVector256Single(Unsafe.As<float, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 192 + 56)));
                ymm0 = Avx.Multiply(ymm0, mul);
                ymm1 = Avx.Multiply(ymm1, mul);
                ymm2 = Avx.Multiply(ymm2, mul);
                ymm3 = Avx.Multiply(ymm3, mul);
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 192 + 32)) = ymm0;
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 192 + 40)) = ymm1;
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 192 + 48)) = ymm2;
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 192 + 56)) = ymm3;
            }
            #endregion
            for (; i < length - 31; i += 32)
            {
                var ymm0 = Avx.ConvertToVector256Single(Unsafe.As<float, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 0)));
                var ymm1 = Avx.ConvertToVector256Single(Unsafe.As<float, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 8)));
                var ymm2 = Avx.ConvertToVector256Single(Unsafe.As<float, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 16)));
                var ymm3 = Avx.ConvertToVector256Single(Unsafe.As<float, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 24)));
                ymm0 = Avx.Multiply(ymm0, mul);
                ymm1 = Avx.Multiply(ymm1, mul);
                ymm2 = Avx.Multiply(ymm2, mul);
                ymm3 = Avx.Multiply(ymm3, mul);
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 0)) = ymm0;
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 8)) = ymm1;
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 16)) = ymm2;
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 24)) = ymm3;
            }
            for (; i < length; i++)
            {
                Unsafe.Add(ref rdi, i) = Unsafe.As<float, int>(ref Unsafe.Add(ref rdi, i)) * mul.GetElement(0);
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ProcessNormalSse2(Span<float> buffer)
        {
            var mul = Vector128.Create(Multiplier);
            ref float rdi = ref MemoryMarshal.GetReference(buffer);
            nint i, length = buffer.Length;
            //Loop for Haswell in 128-bit AVX for better frequency behavior
            for (i = 0; i < length - 63; i += 64)
            {
                var xmm0 = Sse2.ConvertToVector128Single(Unsafe.As<float, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 0)));
                var xmm1 = Sse2.ConvertToVector128Single(Unsafe.As<float, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 4)));
                var xmm2 = Sse2.ConvertToVector128Single(Unsafe.As<float, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 8)));
                var xmm3 = Sse2.ConvertToVector128Single(Unsafe.As<float, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 12)));
                var xmm4 = Sse2.ConvertToVector128Single(Unsafe.As<float, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 16)));
                var xmm5 = Sse2.ConvertToVector128Single(Unsafe.As<float, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 20)));
                var xmm6 = Sse2.ConvertToVector128Single(Unsafe.As<float, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 24)));
                var xmm7 = Sse2.ConvertToVector128Single(Unsafe.As<float, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 28)));
                xmm0 = Sse.Multiply(xmm0, mul);
                xmm1 = Sse.Multiply(xmm1, mul);
                xmm2 = Sse.Multiply(xmm2, mul);
                xmm3 = Sse.Multiply(xmm3, mul);
                xmm4 = Sse.Multiply(xmm4, mul);
                xmm5 = Sse.Multiply(xmm5, mul);
                xmm6 = Sse.Multiply(xmm6, mul);
                xmm7 = Sse.Multiply(xmm7, mul);
                Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 0)) = xmm0;
                Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 4)) = xmm1;
                Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 8)) = xmm2;
                Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 12)) = xmm3;
                Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 16)) = xmm4;
                Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 20)) = xmm5;
                Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 24)) = xmm6;
                Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 28)) = xmm7;
                xmm0 = Sse2.ConvertToVector128Single(Unsafe.As<float, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 32 + 0)));
                xmm1 = Sse2.ConvertToVector128Single(Unsafe.As<float, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 32 + 4)));
                xmm2 = Sse2.ConvertToVector128Single(Unsafe.As<float, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 32 + 8)));
                xmm3 = Sse2.ConvertToVector128Single(Unsafe.As<float, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 32 + 12)));
                xmm4 = Sse2.ConvertToVector128Single(Unsafe.As<float, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 32 + 16)));
                xmm5 = Sse2.ConvertToVector128Single(Unsafe.As<float, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 32 + 20)));
                xmm6 = Sse2.ConvertToVector128Single(Unsafe.As<float, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 32 + 24)));
                xmm7 = Sse2.ConvertToVector128Single(Unsafe.As<float, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 32 + 28)));
                xmm0 = Sse.Multiply(xmm0, mul);
                xmm1 = Sse.Multiply(xmm1, mul);
                xmm2 = Sse.Multiply(xmm2, mul);
                xmm3 = Sse.Multiply(xmm3, mul);
                xmm4 = Sse.Multiply(xmm4, mul);
                xmm5 = Sse.Multiply(xmm5, mul);
                xmm6 = Sse.Multiply(xmm6, mul);
                xmm7 = Sse.Multiply(xmm7, mul);
                Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 32 + 0)) = xmm0;
                Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 32 + 4)) = xmm1;
                Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 32 + 8)) = xmm2;
                Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 32 + 12)) = xmm3;
                Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 32 + 16)) = xmm4;
                Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 32 + 20)) = xmm5;
                Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 32 + 24)) = xmm6;
                Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 32 + 28)) = xmm7;
            }
            for (; i < length; i++)
            {
                Unsafe.Add(ref rdi, i) = Unsafe.As<float, int>(ref Unsafe.Add(ref rdi, i)) * mul.GetElement(0);
            }
        }
#endif
        #endregion

        #region ProcessReversed
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static void ProcessReversed(Span<float> buffer)
        {
#if NETCOREAPP3_1_OR_GREATER
            if (buffer.Length >= 128 && IntrinsicsUtils.AvoidAvxHeavyOperations && Avx2.IsSupported)
            {
                ProcessReversedAvx2(buffer);
                return;
            }
            if (Ssse3.IsSupported)
            {
                ProcessReversedSsse3(buffer);
                return;
            }
#endif
            ProcessReversedStandard(buffer);
        }
#if NETCOREAPP3_1_OR_GREATER
        internal static void ProcessReversedSsse3(Span<float> buffer)
        {
            var mul = Vector128.Create(Multiplier);
            var shuf = Vector128.Create(3, 2, 1, 0, 7, 6, 5, 4, 11, 10, 9, 8, 15, 14, 13, 12).AsByte();
            ref float rdi = ref MemoryMarshal.GetReference(buffer);
            nint i, length = buffer.Length;
            //Loop for Haswell in 128-bit AVX for better frequency behaviour
            for (i = 0; i < length - 31; i += 32)
            {
                var xmm0 = Unsafe.As<float, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 0));
                var xmm1 = Unsafe.As<float, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 4));
                var xmm2 = Unsafe.As<float, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 8));
                var xmm3 = Unsafe.As<float, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 12));
                xmm0 = Ssse3.Shuffle(xmm0.AsByte(), shuf).AsInt32();
                xmm1 = Ssse3.Shuffle(xmm1.AsByte(), shuf).AsInt32();
                xmm2 = Ssse3.Shuffle(xmm2.AsByte(), shuf).AsInt32();
                xmm3 = Ssse3.Shuffle(xmm3.AsByte(), shuf).AsInt32();
                xmm0 = Sse2.ConvertToVector128Single(xmm0).AsInt32();
                xmm1 = Sse2.ConvertToVector128Single(xmm1).AsInt32();
                xmm2 = Sse2.ConvertToVector128Single(xmm2).AsInt32();
                xmm3 = Sse2.ConvertToVector128Single(xmm3).AsInt32();
                xmm0 = Sse.Multiply(xmm0.AsSingle(), mul).AsInt32();
                xmm1 = Sse.Multiply(xmm1.AsSingle(), mul).AsInt32();
                xmm2 = Sse.Multiply(xmm2.AsSingle(), mul).AsInt32();
                xmm3 = Sse.Multiply(xmm3.AsSingle(), mul).AsInt32();
                Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 0)) = xmm0.AsSingle();
                Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 4)) = xmm1.AsSingle();
                Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 8)) = xmm2.AsSingle();
                Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 12)) = xmm3.AsSingle();
                xmm0 = Unsafe.As<float, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 16));
                xmm1 = Unsafe.As<float, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 20));
                xmm2 = Unsafe.As<float, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 24));
                xmm3 = Unsafe.As<float, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 28));
                xmm0 = Ssse3.Shuffle(xmm0.AsByte(), shuf).AsInt32();
                xmm1 = Ssse3.Shuffle(xmm1.AsByte(), shuf).AsInt32();
                xmm2 = Ssse3.Shuffle(xmm2.AsByte(), shuf).AsInt32();
                xmm3 = Ssse3.Shuffle(xmm3.AsByte(), shuf).AsInt32();
                xmm0 = Sse2.ConvertToVector128Single(xmm0).AsInt32();
                xmm1 = Sse2.ConvertToVector128Single(xmm1).AsInt32();
                xmm2 = Sse2.ConvertToVector128Single(xmm2).AsInt32();
                xmm3 = Sse2.ConvertToVector128Single(xmm3).AsInt32();
                xmm0 = Sse.Multiply(xmm0.AsSingle(), mul).AsInt32();
                xmm1 = Sse.Multiply(xmm1.AsSingle(), mul).AsInt32();
                xmm2 = Sse.Multiply(xmm2.AsSingle(), mul).AsInt32();
                xmm3 = Sse.Multiply(xmm3.AsSingle(), mul).AsInt32();
                Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 16)) = xmm0.AsSingle();
                Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 20)) = xmm1.AsSingle();
                Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 24)) = xmm2.AsSingle();
                Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 28)) = xmm3.AsSingle();
            }
            for (; i < length - 7; i += 8)
            {
                var xmm0 = Unsafe.As<float, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 0));
                var xmm1 = Unsafe.As<float, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 4));
                xmm0 = Ssse3.Shuffle(xmm0.AsByte(), shuf).AsInt32();
                xmm1 = Ssse3.Shuffle(xmm1.AsByte(), shuf).AsInt32();
                xmm0 = Sse2.ConvertToVector128Single(xmm0).AsInt32();
                xmm1 = Sse2.ConvertToVector128Single(xmm1).AsInt32();
                xmm0 = Sse.Multiply(xmm0.AsSingle(), mul).AsInt32();
                xmm1 = Sse.Multiply(xmm1.AsSingle(), mul).AsInt32();
                Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 0)) = xmm0.AsSingle();
                Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 4)) = xmm1.AsSingle();
            }
            for (; i < length; i++)
            {
                Unsafe.Add(ref rdi, i) = BinaryPrimitives.ReverseEndianness(Unsafe.As<float, int>(ref Unsafe.Add(ref rdi, i))) * mul.GetElement(0);
            }
        }
        internal static void ProcessReversedAvx2(Span<float> buffer)
        {
            var mul = Vector256.Create(Multiplier);
            var shuf = Vector256.Create(3, 2, 1, 0, 7, 6, 5, 4, 11, 10, 9, 8, 15, 14, 13, 12, 3, 2, 1, 0, 7, 6, 5, 4, 11, 10, 9, 8, 15, 14, 13, 12).AsByte();
            ref float rdi = ref MemoryMarshal.GetReference(buffer);
            nint i, length = buffer.Length;
            //Loop for Intel CPUs in 256-bit AVX2 for better throughput
            for (i = 0; i < length - 63; i += 64)
            {
                var ymm0 = Unsafe.As<float, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 0));
                var ymm1 = Unsafe.As<float, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 8));
                var ymm2 = Unsafe.As<float, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 16));
                var ymm3 = Unsafe.As<float, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 24));
                ymm0 = Avx2.Shuffle(ymm0.AsByte(), shuf).AsInt32();
                ymm1 = Avx2.Shuffle(ymm1.AsByte(), shuf).AsInt32();
                ymm2 = Avx2.Shuffle(ymm2.AsByte(), shuf).AsInt32();
                ymm3 = Avx2.Shuffle(ymm3.AsByte(), shuf).AsInt32();
                ymm0 = Avx.ConvertToVector256Single(ymm0).AsInt32();
                ymm1 = Avx.ConvertToVector256Single(ymm1).AsInt32();
                ymm2 = Avx.ConvertToVector256Single(ymm2).AsInt32();
                ymm3 = Avx.ConvertToVector256Single(ymm3).AsInt32();
                ymm0 = Avx.Multiply(ymm0.AsSingle(), mul).AsInt32();
                ymm1 = Avx.Multiply(ymm1.AsSingle(), mul).AsInt32();
                ymm2 = Avx.Multiply(ymm2.AsSingle(), mul).AsInt32();
                ymm3 = Avx.Multiply(ymm3.AsSingle(), mul).AsInt32();
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 0)) = ymm0.AsSingle();
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 8)) = ymm1.AsSingle();
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 16)) = ymm2.AsSingle();
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 24)) = ymm3.AsSingle();
                ymm0 = Unsafe.As<float, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 32));
                ymm1 = Unsafe.As<float, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 40));
                ymm2 = Unsafe.As<float, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 48));
                ymm3 = Unsafe.As<float, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 56));
                ymm0 = Avx2.Shuffle(ymm0.AsByte(), shuf).AsInt32();
                ymm1 = Avx2.Shuffle(ymm1.AsByte(), shuf).AsInt32();
                ymm2 = Avx2.Shuffle(ymm2.AsByte(), shuf).AsInt32();
                ymm3 = Avx2.Shuffle(ymm3.AsByte(), shuf).AsInt32();
                ymm0 = Avx.ConvertToVector256Single(ymm0).AsInt32();
                ymm1 = Avx.ConvertToVector256Single(ymm1).AsInt32();
                ymm2 = Avx.ConvertToVector256Single(ymm2).AsInt32();
                ymm3 = Avx.ConvertToVector256Single(ymm3).AsInt32();
                ymm0 = Avx.Multiply(ymm0.AsSingle(), mul).AsInt32();
                ymm1 = Avx.Multiply(ymm1.AsSingle(), mul).AsInt32();
                ymm2 = Avx.Multiply(ymm2.AsSingle(), mul).AsInt32();
                ymm3 = Avx.Multiply(ymm3.AsSingle(), mul).AsInt32();
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 32)) = ymm0.AsSingle();
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 40)) = ymm1.AsSingle();
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 48)) = ymm2.AsSingle();
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 56)) = ymm3.AsSingle();
            }
            for (; i < length - 7; i += 8)
            {
                var xmm0 = Unsafe.As<float, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 0));
                var xmm1 = Unsafe.As<float, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 4));
                xmm0 = Ssse3.Shuffle(xmm0.AsByte(), shuf.GetLower()).AsInt32();
                xmm1 = Ssse3.Shuffle(xmm1.AsByte(), shuf.GetLower()).AsInt32();
                xmm0 = Sse2.ConvertToVector128Single(xmm0).AsInt32();
                xmm1 = Sse2.ConvertToVector128Single(xmm1).AsInt32();
                xmm0 = Sse.Multiply(xmm0.AsSingle(), mul.GetLower()).AsInt32();
                xmm1 = Sse.Multiply(xmm1.AsSingle(), mul.GetLower()).AsInt32();
                Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 0)) = xmm0.AsSingle();
                Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 4)) = xmm1.AsSingle();
            }
            for (; i < length; i++)
            {
                Unsafe.Add(ref rdi, i) = BinaryPrimitives.ReverseEndianness(Unsafe.As<float, int>(ref Unsafe.Add(ref rdi, i))) * mul.GetElement(0);
            }
        }
#endif
        internal static void ProcessReversedStandard(Span<float> buffer)
        {
            float mul = Multiplier;
            ref float rdi = ref MemoryMarshal.GetReference(buffer);
            ref int rsi = ref Unsafe.As<float, int>(ref rdi);
            nint i, length = buffer.Length;
            for (i = 0; i < length - 3; i += 4)
            {
                //ReverseEndianness can't be done in vectorized way in .NET Standard.
                Unsafe.Add(ref rdi, i + 0) = BinaryPrimitives.ReverseEndianness(Unsafe.Add(ref rsi, i + 0)) * mul;
                Unsafe.Add(ref rdi, i + 1) = BinaryPrimitives.ReverseEndianness(Unsafe.Add(ref rsi, i + 1)) * mul;
                Unsafe.Add(ref rdi, i + 2) = BinaryPrimitives.ReverseEndianness(Unsafe.Add(ref rsi, i + 2)) * mul;
                Unsafe.Add(ref rdi, i + 3) = BinaryPrimitives.ReverseEndianness(Unsafe.Add(ref rsi, i + 3)) * mul;
            }
            for (; i < length; i++)
            {
                Unsafe.Add(ref rdi, i) = BinaryPrimitives.ReverseEndianness(Unsafe.Add(ref rsi, i)) * mul;
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
