using System;
using System.Linq;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

using Shamisen.Utils;

#if NETCOREAPP3_1_OR_GREATER
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

using Shamisen.Utils.Intrinsics;

using System.Security.Cryptography;

#endif
#if NET5_0_OR_GREATER
using System.Runtime.Intrinsics.Arm;
#endif
namespace Shamisen.Conversion.WaveToSampleConverters
{
    /// <summary>
    /// Converts 24-bit PCM to Sample.
    /// </summary>
    /// <seealso cref="WaveToSampleConverterBase" />
    public sealed class Pcm24ToSampleConverter : WaveToSampleConverterBase
    {
        private const float Divisor = 8388608.0f;
        private const float Multiplier = 1.0f / Divisor;
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
            var span = readBuffer.Span;
            var cursor = buffer;
            while (cursor.Length > 0)
            {
                var reader = span.SliceWhileIfLongerThan(cursor.Length);
                var rr = Source.Read(MemoryMarshal.AsBytes(reader));
                if (rr.HasNoData)
                {
                    var len = buffer.Length - cursor.Length;
                    return rr.IsEndOfStream && len <= 0 ? rr : len;
                }
                var u = rr.Length / BytesPerSample;
                var wrote = reader.Slice(0, u);
                var dest = cursor.Slice(0, wrote.Length);
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
                    ProcessNormal(wrote, dest);
                }
                cursor = cursor.Slice(u);
                if (u != reader.Length) return buffer.Length - cursor.Length;  //The Source doesn't fill whole reader so return here.
            }
            return buffer.Length;
        }


        private static void ProcessNormal(Span<Int24> wrote, Span<float> dest)
        {
            unchecked
            {
#if NETCOREAPP3_1_OR_GREATER
                if (Avx2.IsSupported)
                {
                    ProcessNormalAvx2(wrote, dest);
                }
#endif
                ProcessNormalStandard(wrote, dest);
            }
        }

        private static void ProcessReversed(Span<Int24> wrote, Span<float> dest)
        {
            unchecked
            {
#if NETCOREAPP3_1_OR_GREATER
                if (Avx2.IsSupported)
                {
                    ProcessReversedAvx2(wrote, dest);
                }
#endif
                ProcessReversedStandard(wrote, dest);
            }
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ProcessNormalStandard(Span<Int24> wrote, Span<float> dest)
        {
            ref var rdi = ref MemoryMarshal.GetReference(dest);
            ref var rsi = ref Unsafe.As<Int24, byte>(ref MemoryMarshal.GetReference(wrote));
            nint i = 0, j = 0, length = MathI.Min(dest.Length, wrote.Length);
            var mul = new Vector4(Multiplier);
            var olen = length - 7;
            for (; i < olen; i += 8, j += 24)
            {
                var g0 = (int)Unsafe.As<byte, ushort>(ref Unsafe.Add(ref rsi, j));
                var h0 = (int)Unsafe.Add(ref rsi, j + 2);
                var g1 = (int)Unsafe.As<byte, ushort>(ref Unsafe.Add(ref rsi, j + 3));
                var h1 = (int)Unsafe.Add(ref rsi, j + 5);
                var g2 = (int)Unsafe.As<byte, ushort>(ref Unsafe.Add(ref rsi, j + 6));
                var h2 = (int)Unsafe.Add(ref rsi, j + 8);
                var g3 = (int)Unsafe.As<byte, ushort>(ref Unsafe.Add(ref rsi, j + 9));
                var h3 = (int)Unsafe.Add(ref rsi, j + 11);
                g0 <<= 8;
                g1 <<= 8;
                g2 <<= 8;
                g3 <<= 8;
                h0 <<= 24;
                h1 <<= 24;
                h2 <<= 24;
                h3 <<= 24;
                g0 |= h0;
                g1 |= h1;
                g2 |= h2;
                g3 |= h3;
                g0 >>= 8;
                g1 >>= 8;
                g2 >>= 8;
                g3 >>= 8;
                var v0_4s = VectorUtils.ConvertAndCreateVector4(g0, g1, g2, g3);
                g0 = Unsafe.As<byte, ushort>(ref Unsafe.Add(ref rsi, j + 12));
                h0 = Unsafe.Add(ref rsi, j + 14);
                g1 = Unsafe.As<byte, ushort>(ref Unsafe.Add(ref rsi, j + 15));
                h1 = Unsafe.Add(ref rsi, j + 17);
                g2 = Unsafe.As<byte, ushort>(ref Unsafe.Add(ref rsi, j + 18));
                h2 = Unsafe.Add(ref rsi, j + 20);
                g3 = Unsafe.As<byte, ushort>(ref Unsafe.Add(ref rsi, j + 21));
                h3 = Unsafe.Add(ref rsi, j + 23);
                g0 <<= 8;
                g1 <<= 8;
                g2 <<= 8;
                g3 <<= 8;
                h0 <<= 24;
                h1 <<= 24;
                h2 <<= 24;
                h3 <<= 24;
                g0 |= h0;
                g1 |= h1;
                g2 |= h2;
                g3 |= h3;
                g0 >>= 8;
                g1 >>= 8;
                g2 >>= 8;
                g3 >>= 8;
                var v1_4s = VectorUtils.ConvertAndCreateVector4(g0, g1, g2, g3);
                v0_4s *= mul;
                v1_4s *= mul;
                Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rdi, i + 0)) = v0_4s;
                Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rdi, i + 4)) = v1_4s;
            }
            for (; i < length; i++, j += 3)
            {
                var g = (int)Unsafe.As<byte, ushort>(ref Unsafe.Add(ref rsi, j));
                var h = (int)Unsafe.Add(ref rsi, j + 2);
                g <<= 8;
                h <<= 24;
                g |= h;
                g >>= 8;
                Unsafe.Add(ref rdi, i) = g * mul.X;
            }
        }

        internal static void ProcessReversedStandard(Span<Int24> wrote, Span<float> dest)
        {
            ref var rdi = ref MemoryMarshal.GetReference(dest);
            ref var rsi = ref MemoryMarshal.GetReference(wrote);
            nint i = 0, length = MathI.Min(dest.Length, wrote.Length);
            var t = Multiplier;
            for (; i < length; i++)
            {
                Unsafe.Add(ref rdi, i) = Int24.ReverseEndianness(Unsafe.Add(ref rsi, i)) * t;
            }
        }
        #region X86
#if NETCOREAPP3_1_OR_GREATER
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ProcessNormalAvx2(Span<Int24> wrote, Span<float> dest)
        {
            ref var rdi = ref MemoryMarshal.GetReference(dest);
            ref var rsi = ref Unsafe.As<Int24, byte>(ref MemoryMarshal.GetReference(wrote));
            nint i = 0, j = 0, length = MathI.Min(dest.Length, wrote.Length);
            var ymm15 = Vector256.Create(1.0f);
            var ymm14 = Vector256.Create(0, 1, 2, 6, 3, 4, 5, 7);
            var ymm13 = Vector256.Create(0, 2, 3, 4, 1, 5, 6, 7);
            var ymm12 = Vector256.Create(0, 4, 5, 6, 1, 2, 3, 7);
            var ymm11 = Vector256.Create(0, 4, 1, 1, 5, 6, 7, 0);
            var ymm10 = Vector256.Create(0, 1, 2, 0, 6, 3, 7, 0);
            var ymm9 = Vector256.Create(128, 0, 1, 2, 128, 3, 4, 5, 128, 6, 7, 8, 128, 9, 10, 11, 128, 0, 1, 2, 128, 3, 4, 5, 128, 6, 7, 8, 128, 9, 10, 11);
            var ymm8 = Vector256.Create(int.MinValue);
            var olen = length - 31;
            for (; i < olen; i += 32, j += 96)
            {
                var ymm0 = Unsafe.As<byte, Vector256<int>>(ref Unsafe.Add(ref rsi, j));
                var ymm2 = Unsafe.As<byte, Vector256<int>>(ref Unsafe.Add(ref rsi, j + 32));
                var ymm3 = Unsafe.As<byte, Vector256<int>>(ref Unsafe.Add(ref rsi, j + 64));
                ymm0 = Avx2.PermuteVar8x32(ymm0, ymm14);
                ymm3 = Avx2.PermuteVar8x32(ymm3, ymm13);
                var ymm1 = Avx2.PermuteVar8x32(ymm2, ymm12);
                ymm2 = Avx2.AlignRight(ymm3, ymm1, 4);  //12, 13, 14, 16, 10, 11, 15, 17
                ymm1 = Avx2.AlignRight(ymm1, ymm0, 12); //6, 8, 12, 13, 7, 9, 10, 11
                ymm3 = Avx2.AlignRight(ymm3, ymm3, 4);
                ymm1 = Avx2.PermuteVar8x32(ymm1, ymm11);
                ymm2 = Avx2.PermuteVar8x32(ymm2, ymm10);
                ymm0 = Avx2.Shuffle(ymm0.AsByte(), ymm9).AsInt32();
                ymm1 = Avx2.Shuffle(ymm1.AsByte(), ymm9).AsInt32();
                ymm2 = Avx2.Shuffle(ymm2.AsByte(), ymm9).AsInt32();
                ymm3 = Avx2.Shuffle(ymm3.AsByte(), ymm9).AsInt32();
                var ymm4 = Avx2.And(ymm8, ymm0);
                ymm0 = Avx2.Abs(ymm0).AsInt32();
                var ymm5 = Avx2.And(ymm8, ymm1);
                ymm1 = Avx2.Abs(ymm1).AsInt32();
                var ymm6 = Avx2.And(ymm8, ymm2);
                ymm2 = Avx2.Abs(ymm2).AsInt32();
                var ymm7 = Avx2.And(ymm8, ymm3);
                ymm3 = Avx2.Abs(ymm3).AsInt32();
                ymm0 = Avx2.ShiftRightLogical(ymm0, 8);
                ymm1 = Avx2.ShiftRightLogical(ymm1, 8);
                ymm2 = Avx2.ShiftRightLogical(ymm2, 8);
                ymm3 = Avx2.ShiftRightLogical(ymm3, 8);
                ymm0 = Avx2.Add(ymm0, ymm15.AsInt32());
                ymm1 = Avx2.Add(ymm1, ymm15.AsInt32());
                ymm2 = Avx2.Add(ymm2, ymm15.AsInt32());
                ymm3 = Avx2.Add(ymm3, ymm15.AsInt32());
                ymm0 = Avx2.Or(ymm0, ymm4);
                ymm4 = Avx2.Or(ymm4, ymm15.AsInt32());
                ymm1 = Avx2.Or(ymm1, ymm5);
                ymm5 = Avx2.Or(ymm5, ymm15.AsInt32());
                ymm2 = Avx2.Or(ymm2, ymm6);
                ymm6 = Avx2.Or(ymm6, ymm15.AsInt32());
                ymm3 = Avx2.Or(ymm3, ymm7);
                ymm7 = Avx2.Or(ymm7, ymm15.AsInt32());
                ymm0 = Avx.Subtract(ymm0.AsSingle(), ymm4.AsSingle()).AsInt32();
                ymm1 = Avx.Subtract(ymm1.AsSingle(), ymm5.AsSingle()).AsInt32();
                ymm2 = Avx.Subtract(ymm2.AsSingle(), ymm6.AsSingle()).AsInt32();
                ymm3 = Avx.Subtract(ymm3.AsSingle(), ymm7.AsSingle()).AsInt32();
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 0)) = ymm0.AsSingle();
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 8)) = ymm1.AsSingle();
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 16)) = ymm2.AsSingle();
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 24)) = ymm3.AsSingle();
            }
            for (; i < length; i++, j += 3)
            {
                var xmm0 = Vector128.CreateScalarUnsafe((uint)Unsafe.As<byte, ushort>(ref Unsafe.Add(ref rsi, j))).AsInt32();
                var xmm1 = Vector128.CreateScalarUnsafe((uint)Unsafe.Add(ref rsi, j + 2)).AsInt32();
                xmm0 = Sse2.ShiftLeftLogical(xmm0, 8);
                xmm1 = Sse2.ShiftLeftLogical(xmm1, 24);
                xmm1 = Sse2.Or(xmm1, xmm0);
                var xmm4 = Sse2.And(ymm8.GetLower(), xmm1);
                xmm1 = Ssse3.Abs(xmm1).AsInt32();
                xmm1 = Sse2.ShiftRightLogical(xmm1, 8);
                xmm1 = Sse2.Add(xmm1, ymm15.GetLower().AsInt32());
                xmm1 = Sse2.Or(xmm1, xmm4);
                xmm4 = Sse2.Or(xmm4, ymm15.GetLower().AsInt32());
                xmm1 = Sse.SubtractScalar(xmm1.AsSingle(), xmm4.AsSingle()).AsInt32();
                Unsafe.Add(ref rdi, i + 0) = xmm1.AsSingle().GetElement(0);
            }
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ProcessReversedAvx2(Span<Int24> wrote, Span<float> dest)
        {
            ref var rdi = ref MemoryMarshal.GetReference(dest);
            ref var rsi = ref Unsafe.As<Int24, byte>(ref MemoryMarshal.GetReference(wrote));
            nint i = 0, j = 0, length = MathI.Min(dest.Length, wrote.Length);
            var ymm15 = Vector256.Create(1.0f);
            var ymm14 = Vector256.Create(0, 1, 2, 6, 3, 4, 5, 7);
            var ymm13 = Vector256.Create(0, 2, 3, 4, 1, 5, 6, 7);
            var ymm12 = Vector256.Create(0, 4, 5, 6, 1, 2, 3, 7);
            var ymm11 = Vector256.Create(0, 4, 1, 1, 5, 6, 7, 0);
            var ymm10 = Vector256.Create(0, 1, 2, 0, 6, 3, 7, 0);
            var ymm9 = Vector256.Create(128, 2, 1, 0, 128, 5, 4, 3, 128, 8, 7, 6, 128, 11, 10, 9, 128, 2, 1, 0, 128, 5, 4, 3, 128, 8, 7, 6, 128, 11, 10, 9);
            var ymm8 = Vector256.Create(int.MinValue);
            var olen = length - 31;
            for (; i < olen; i += 32, j += 96)
            {
                var ymm0 = Unsafe.As<byte, Vector256<int>>(ref Unsafe.Add(ref rsi, j));
                var ymm2 = Unsafe.As<byte, Vector256<int>>(ref Unsafe.Add(ref rsi, j + 32));
                var ymm3 = Unsafe.As<byte, Vector256<int>>(ref Unsafe.Add(ref rsi, j + 64));
                ymm0 = Avx2.PermuteVar8x32(ymm0, ymm14);
                ymm3 = Avx2.PermuteVar8x32(ymm3, ymm13);
                var ymm1 = Avx2.PermuteVar8x32(ymm2, ymm12);
                ymm2 = Avx2.AlignRight(ymm3, ymm1, 4);  //12, 13, 14, 16, 10, 11, 15, 17
                ymm1 = Avx2.AlignRight(ymm1, ymm0, 12); //6, 8, 12, 13, 7, 9, 10, 11
                ymm3 = Avx2.AlignRight(ymm3, ymm3, 4);
                ymm1 = Avx2.PermuteVar8x32(ymm1, ymm11);
                ymm2 = Avx2.PermuteVar8x32(ymm2, ymm10);
                ymm0 = Avx2.Shuffle(ymm0.AsByte(), ymm9).AsInt32();
                ymm1 = Avx2.Shuffle(ymm1.AsByte(), ymm9).AsInt32();
                ymm2 = Avx2.Shuffle(ymm2.AsByte(), ymm9).AsInt32();
                ymm3 = Avx2.Shuffle(ymm3.AsByte(), ymm9).AsInt32();
                var ymm4 = Avx2.And(ymm8, ymm0);
                ymm0 = Avx2.Abs(ymm0).AsInt32();
                var ymm5 = Avx2.And(ymm8, ymm1);
                ymm1 = Avx2.Abs(ymm1).AsInt32();
                var ymm6 = Avx2.And(ymm8, ymm2);
                ymm2 = Avx2.Abs(ymm2).AsInt32();
                var ymm7 = Avx2.And(ymm8, ymm3);
                ymm3 = Avx2.Abs(ymm3).AsInt32();
                ymm0 = Avx2.ShiftRightLogical(ymm0, 8);
                ymm1 = Avx2.ShiftRightLogical(ymm1, 8);
                ymm2 = Avx2.ShiftRightLogical(ymm2, 8);
                ymm3 = Avx2.ShiftRightLogical(ymm3, 8);
                ymm0 = Avx2.Add(ymm0, ymm15.AsInt32());
                ymm1 = Avx2.Add(ymm1, ymm15.AsInt32());
                ymm2 = Avx2.Add(ymm2, ymm15.AsInt32());
                ymm3 = Avx2.Add(ymm3, ymm15.AsInt32());
                ymm0 = Avx2.Or(ymm0, ymm4);
                ymm4 = Avx2.Or(ymm4, ymm15.AsInt32());
                ymm1 = Avx2.Or(ymm1, ymm5);
                ymm5 = Avx2.Or(ymm5, ymm15.AsInt32());
                ymm2 = Avx2.Or(ymm2, ymm6);
                ymm6 = Avx2.Or(ymm6, ymm15.AsInt32());
                ymm3 = Avx2.Or(ymm3, ymm7);
                ymm7 = Avx2.Or(ymm7, ymm15.AsInt32());
                ymm0 = Avx.Subtract(ymm0.AsSingle(), ymm4.AsSingle()).AsInt32();
                ymm1 = Avx.Subtract(ymm1.AsSingle(), ymm5.AsSingle()).AsInt32();
                ymm2 = Avx.Subtract(ymm2.AsSingle(), ymm6.AsSingle()).AsInt32();
                ymm3 = Avx.Subtract(ymm3.AsSingle(), ymm7.AsSingle()).AsInt32();
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 0)) = ymm0.AsSingle();
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 8)) = ymm1.AsSingle();
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 16)) = ymm2.AsSingle();
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 24)) = ymm3.AsSingle();
            }
            for (; i < length; i++, j += 3)
            {
                var xmm0 = Vector128.CreateScalarUnsafe((uint)Unsafe.As<byte, ushort>(ref Unsafe.Add(ref rsi, j))).AsInt32();
                var xmm1 = Vector128.CreateScalarUnsafe((uint)Unsafe.Add(ref rsi, j + 2)).AsInt32();
                xmm1 = Sse2.ShiftLeftLogical(xmm1, 16);
                xmm1 = Sse2.Or(xmm1, xmm0);
                xmm1 = Ssse3.Shuffle(xmm1.AsByte(), ymm9.GetLower()).AsInt32();
                var xmm4 = Sse2.And(ymm8.GetLower(), xmm1);
                xmm1 = Ssse3.Abs(xmm1).AsInt32();
                xmm1 = Sse2.ShiftRightLogical(xmm1, 8);
                xmm1 = Sse2.Add(xmm1, ymm15.GetLower().AsInt32());
                xmm1 = Sse2.Or(xmm1, xmm4);
                xmm4 = Sse2.Or(xmm4, ymm15.GetLower().AsInt32());
                xmm1 = Sse.SubtractScalar(xmm1.AsSingle(), xmm4.AsSingle()).AsInt32();
                Unsafe.Add(ref rdi, i + 0) = xmm1.AsSingle().GetElement(0);
            }
        }
#endif
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
