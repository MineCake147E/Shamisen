using System;
using System.Collections.Generic;
using System.Linq;
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
using Shamisen;

#endif
#if NET5_0_OR_GREATER
using System.Runtime.Intrinsics.Arm;
#endif

namespace Shamisen.Conversion.SampleToWaveConverters
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
        private int ActualBufferMax => BufferMax - BufferMax % Source.Format.Channels;

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
                if (rr.IsEndOfStream && buffer.Length == cursor.Length) return rr;
                if (rr.HasNoData) return buffer.Length - cursor.Length;
                var u = rr.Length;
                var wrote = reader.Span.Slice(0, u);
                var dest = cursor.Slice(0, wrote.Length);
                if (wrote.Length != dest.Length)
                {
                    new InvalidOperationException(
                        $"The {nameof(wrote)}'s length and {nameof(dest)}'s length are not equal! This is a bug!").Throw();
                }

                if (AccuracyMode)
                {
                    var dsmAcc = dsmAccumulator.Span;
                    var dsmLastOut = dsmLastOutput.Span;
                    dsmChannelPointer %= dsmAcc.Length;
                    for (var i = 0; i < dest.Length; i++)
                    {
                        var diff = wrote[i] - dsmLastOut[dsmChannelPointer] / Multiplier;
                        dsmAcc[dsmChannelPointer] += diff;
                        var v = dsmLastOut[dsmChannelPointer] = Convert(dsmAcc[dsmChannelPointer]);
                        dest[i] = v;
                        dsmChannelPointer = ++dsmChannelPointer % dsmAcc.Length;
                    }
                }
                else
                {
                    ProcessNormal(wrote, dest);
                }
                if (IsEndiannessConversionRequired)
                {
                    dest.ReverseEndianness();
                }
                cursor = cursor.Slice(dest.Length);
                if (u != reader.Length) return buffer.Length - cursor.Length;  //The Source doesn't fill whole reader so return here.
            }
            return buffer.Length;
        }

        private static void ProcessNormal(Span<float> wrote, Span<Int24> dest)
        {
            unchecked
            {
#if NETCOREAPP3_1_OR_GREATER
                if (Avx2.IsSupported)
                {
                    ProcessNormalAvx2(wrote, dest);
                    return;
                }
#endif
                ProcessNormalStandard(wrote, dest);
            }
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ProcessNormalStandard(Span<float> wrote, Span<Int24> dest)
        {
            ref var rsi = ref MemoryMarshal.GetReference(wrote);
            ref var rdi = ref Unsafe.As<Int24, byte>(ref MemoryMarshal.GetReference(dest));
            nint i = 0, j = 0, length = MathI.Min(dest.Length, wrote.Length);
            Vector4 mul = new(Multiplier);
            Vector4 max = new(8388607.0f / 8388608.0f);
            Vector4 min = new(-1.0f);
            var olen = length - 7;
            for (; i < olen; i += 8, j += 24)
            {
                var v0_4s = Vector4.Min(max, Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rsi, i + 0)));
                var v1_4s = Vector4.Min(max, Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rsi, i + 4)));
                v0_4s = Vector4.Max(min, v0_4s);
                v1_4s = Vector4.Max(min, v1_4s);
                v0_4s *= mul;
                v1_4s *= mul;
                v0_4s = VectorUtils.Round(v0_4s);
                v1_4s = VectorUtils.Round(v1_4s);
                var w0 = (uint)(int)v0_4s.X;
                var w1 = (uint)(int)v0_4s.Y;
                var w2 = (uint)(int)v0_4s.Z;
                var w3 = (uint)(int)v0_4s.W;
                Unsafe.As<byte, ushort>(ref Unsafe.Add(ref rdi, j + 0)) = (ushort)w0;
                Unsafe.As<byte, ushort>(ref Unsafe.Add(ref rdi, j + 3)) = (ushort)w1;
                Unsafe.As<byte, ushort>(ref Unsafe.Add(ref rdi, j + 6)) = (ushort)w2;
                Unsafe.As<byte, ushort>(ref Unsafe.Add(ref rdi, j + 9)) = (ushort)w3;
                w0 >>= 16;
                w1 >>= 16;
                w2 >>= 16;
                w3 >>= 16;
                Unsafe.Add(ref rdi, j + 2) = (byte)w0;
                Unsafe.Add(ref rdi, j + 5) = (byte)w1;
                Unsafe.Add(ref rdi, j + 8) = (byte)w2;
                Unsafe.Add(ref rdi, j + 11) = (byte)w3;
                w0 = (uint)(int)v1_4s.X;
                w1 = (uint)(int)v1_4s.Y;
                w2 = (uint)(int)v1_4s.Z;
                w3 = (uint)(int)v1_4s.W;
                Unsafe.As<byte, ushort>(ref Unsafe.Add(ref rdi, j + 12)) = (ushort)w0;
                Unsafe.As<byte, ushort>(ref Unsafe.Add(ref rdi, j + 15)) = (ushort)w1;
                Unsafe.As<byte, ushort>(ref Unsafe.Add(ref rdi, j + 18)) = (ushort)w2;
                Unsafe.As<byte, ushort>(ref Unsafe.Add(ref rdi, j + 21)) = (ushort)w3;
                w0 >>= 16;
                w1 >>= 16;
                w2 >>= 16;
                w3 >>= 16;
                Unsafe.Add(ref rdi, j + 14) = (byte)w0;
                Unsafe.Add(ref rdi, j + 17) = (byte)w1;
                Unsafe.Add(ref rdi, j + 20) = (byte)w2;
                Unsafe.Add(ref rdi, j + 23) = (byte)w3;
            }
            for (; i < length; i++, j += 3)
            {
                var s0 = FastMath.Min(max.X, Unsafe.Add(ref rsi, i));
                s0 = FastMath.Max(min.X, s0);
                s0 *= mul.X;
                s0 = FastMath.Round(s0);
                var h = (uint)(int)s0;
                Unsafe.As<byte, ushort>(ref Unsafe.Add(ref rdi, j)) = (ushort)h;
                Unsafe.Add(ref rdi, j + 2) = (byte)(h >> 16);
            }
        }
        #region X86
#if NETCOREAPP3_1_OR_GREATER

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ProcessNormalAvx2(Span<float> wrote, Span<Int24> dest)
        {
            ref var rsi = ref MemoryMarshal.GetReference(wrote);
            ref var rdi = ref Unsafe.As<Int24, byte>(ref MemoryMarshal.GetReference(dest));
            nint i = 0, j = 0, length = MathI.Min(dest.Length, wrote.Length);
            var ymm15 = Vector256.Create(-1.0f);
            var ymm14 = Vector256.Create(8388607.0f / 8388608.0f);
            var ymm13 = Vector256.Create(0x0b800000);
            var ymm12 = Vector256.Create(0, 1, 2, 4, 5, 6, 8, 9, 10, 12, 13, 14, 128, 128, 128, 128, 0, 1, 2, 4, 5, 6, 8, 9, 10, 12, 13, 14, 128, 128, 128, 128);
            var ymm11 = Vector256.Create(2, 4, 3, 0, 5, 6, 7, 1);
            var ymm10 = Vector256.Create(0, 1, 3, 5, 2, 4, 7, 6);
            var ymm9 = Vector256.Create(1, 2, 3, 5, 6, 7, 0, 4);
            var ymm8 = Vector256.Create(0, 4, 1, 2, 3, 5, 6, 7);
            var olen = length - 31;
            for (; i < olen; i += 32, j += 96)
            {
                var ymm0 = Avx.Min(ymm14, Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rsi, i + 0))).AsInt32();
                var ymm1 = Avx.Min(ymm14, Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rsi, i + 8))).AsInt32();
                var ymm2 = Avx.Min(ymm14, Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rsi, i + 16))).AsInt32();
                var ymm3 = Avx.Min(ymm14, Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rsi, i + 24))).AsInt32();
                ymm0 = Avx.Max(ymm15, ymm0.AsSingle()).AsInt32();
                ymm1 = Avx.Max(ymm15, ymm1.AsSingle()).AsInt32();
                ymm2 = Avx.Max(ymm15, ymm2.AsSingle()).AsInt32();
                ymm3 = Avx.Max(ymm15, ymm3.AsSingle()).AsInt32();
                ymm0 = Avx2.Add(ymm13, ymm0);
                ymm1 = Avx2.Add(ymm13, ymm1);
                ymm2 = Avx2.Add(ymm13, ymm2);
                ymm3 = Avx2.Add(ymm13, ymm3);
                ymm0 = Avx.ConvertToVector256Int32(ymm0.AsSingle());
                ymm1 = Avx.ConvertToVector256Int32(ymm1.AsSingle());
                ymm2 = Avx.ConvertToVector256Int32(ymm2.AsSingle());
                ymm3 = Avx.ConvertToVector256Int32(ymm3.AsSingle());
                ymm0 = Avx2.Shuffle(ymm0.AsByte(), ymm12).AsInt32();
                ymm1 = Avx2.Shuffle(ymm1.AsByte(), ymm12).AsInt32();
                ymm2 = Avx2.Shuffle(ymm2.AsByte(), ymm12).AsInt32();
                ymm3 = Avx2.Shuffle(ymm3.AsByte(), ymm12).AsInt32();
                ymm1 = Avx2.PermuteVar8x32(ymm1, ymm11);
                ymm2 = Avx2.PermuteVar8x32(ymm2, ymm10);
                ymm0 = Avx2.AlignRight(ymm0, ymm1, 12);
                ymm3 = Avx2.AlignRight(ymm3, ymm2, 12);
                ymm1 = Avx2.UnpackLow(ymm1.AsInt64(), ymm2.AsInt64()).AsInt32();
                ymm0 = Avx2.PermuteVar8x32(ymm0, ymm9);
                ymm1 = Avx2.Permute4x64(ymm1.AsInt64(), 0b11_01_10_00).AsInt32();
                ymm3 = Avx2.PermuteVar8x32(ymm3, ymm8);
                Unsafe.As<byte, Vector256<int>>(ref Unsafe.Add(ref rdi, j + 0)) = ymm0;
                Unsafe.As<byte, Vector256<int>>(ref Unsafe.Add(ref rdi, j + 32)) = ymm1;
                Unsafe.As<byte, Vector256<int>>(ref Unsafe.Add(ref rdi, j + 64)) = ymm3;
            }
            for (; i < length; i++, j += 3)
            {
                var xmm0 = Sse.MinScalar(ymm14.GetLower(), Vector128.CreateScalarUnsafe(Unsafe.Add(ref rsi, i)));
                xmm0 = Sse.MaxScalar(ymm15.GetLower(), xmm0);
                xmm0 = Sse2.Add(ymm13.GetLower(), xmm0.AsInt32()).AsSingle();
                var h = (uint)Sse.ConvertToInt32(xmm0);
                Unsafe.As<byte, ushort>(ref Unsafe.Add(ref rdi, j)) = (ushort)h;
                Unsafe.Add(ref rdi, j + 2) = (byte)(h >> 16);
            }
        }

#endif
        #endregion
        private static Int24 Convert(float srcval) => (Int24)Math.Round(Math.Min(Int24.MaxValue, Math.Max(srcval * Multiplier, Int24.MinValue)));

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
