using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#if NETCOREAPP3_1_OR_GREATER
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;


#endif
using Shamisen.Utils;

namespace Shamisen.Synthesis
{
    /// <summary>
    /// Generates a triangular wave with specified frequency.
    /// </summary>
    public sealed class TriangleWaveSource : ISampleSource, IFrequencyGeneratorSource
    {
        private bool disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="TriangleWaveSource"/> class.
        /// </summary>
        /// <param name="format">The output format.</param>
        public TriangleWaveSource(SampleFormat format)
        {
            Format = format;
        }

        /// <inheritdoc/>
        public double Frequency
        {
            get => AngularVelocity.DoubleValue * Format.SampleRate * 0.5;
            set => AngularVelocity = (Fixed64)Math.Abs(2.0 * value / Format.SampleRate);
        }
        /// <summary>
        /// Gets or sets the angular velocity of this <see cref="TriangleWaveSource"/>.
        /// </summary>
        public Fixed64 AngularVelocity { get; set; }

        /// <summary>
        /// Gets or sets the current phase of this <see cref="TriangleWaveSource"/>.
        /// </summary>
        public Fixed64 Theta { get; set; } = Fixed64.Zero;
        /// <inheritdoc/>
        public SampleFormat Format { get; }
        /// <inheritdoc/>
        public ulong? Length { get; }
        /// <inheritdoc/>
        public ulong? TotalLength { get; }
        /// <inheritdoc/>
        public ulong? Position { get; }
        /// <inheritdoc/>
        public ISkipSupport? SkipSupport { get; }
        /// <inheritdoc/>
        public ISeekSupport? SeekSupport { get; }

        /// <inheritdoc/>
        public ReadResult Read(Span<float> buffer)
        {
            var channels = Format.Channels;
            buffer = buffer.SliceAlign(channels);
            var omega = AngularVelocity;
            var theta = Theta;
            var bspan = buffer.SliceFromEnd(buffer.Length / channels);
            theta = GenerateMonauralBlock(bspan, omega, theta);
            Theta = theta;
            if (channels == 1) return buffer.Length;
            AudioUtils.DuplicateMonauralToChannels(buffer, bspan, channels);
            return buffer.Length;
        }


        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static Fixed64 GenerateMonauralBlock(Span<float> buffer, Fixed64 omega, Fixed64 theta)
        {
#if NETCOREAPP3_1_OR_GREATER
            if (Avx2.IsSupported)
            {
                return GenerateMonauralBlockAvx2MM256(buffer, omega, theta);
            }
#endif
            return GenerateMonauralBlockStandard(buffer, omega, ref theta);
        }

#if NETCOREAPP3_1_OR_GREATER
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static Fixed64 GenerateMonauralBlockAvx2MM256(Span<float> buffer, Fixed64 omega, Fixed64 theta)
        {
            var t = theta.Value;
            var o = omega.Value;
            var ymm13 = Vector256.Create(o * 4);
            var xmm15 = Vector128.Create(0L, o);
            var xmm14 = Sse2.Add(xmm15, ymm13.GetLower());
            var ymm15 = xmm15.ToVector256Unsafe().WithUpper(xmm14).AsInt32();
            ymm13 = Avx2.Add(ymm13, ymm13);
            var ymm0 = Vector256.Create(t);
            var ymm1 = Vector256.Create(t + o * 2);
            ymm0 = Avx2.Add(ymm0, ymm15.AsInt64());
            ymm1 = Avx2.Add(ymm1, ymm15.AsInt64());
            ymm15 = Vector256.Create(0x8000_0000u).AsInt32();
            var ymm14 = Vector256.Create((float)(1.0 / 0x4000_0000));

            ref var rdi = ref MemoryMarshal.GetReference(buffer);
            nint i = 0, length = buffer.Length;
            var olen = length - 7;
            for (; i < olen; i += 32)
            {
                var ymm2 = Avx2.Add(ymm0, ymm13).AsInt32();
                var ymm3 = Avx2.Add(ymm1, ymm13).AsInt32();
                var ymm5 = Avx2.Add(ymm3.AsInt64(), ymm13).AsInt32();
                var ymm4 = Avx2.Add(ymm2.AsInt64(), ymm13).AsInt32();
                var ymm6 = Avx2.Add(ymm4.AsInt64(), ymm13).AsInt32();
                var ymm7 = Avx2.Add(ymm5.AsInt64(), ymm13).AsInt32();
                var ymm8 = Avx.Shuffle(ymm0.AsSingle(), ymm1.AsSingle(), 0b11_01_11_01).AsInt32();
                ymm1 = Avx2.Add(ymm7.AsInt64(), ymm13);
                var ymm9 = Avx.Shuffle(ymm2.AsSingle(), ymm3.AsSingle(), 0b11_01_11_01).AsInt32();
                ymm0 = Avx2.Add(ymm6.AsInt64(), ymm13);
                var ymm10 = Avx.Shuffle(ymm4.AsSingle(), ymm5.AsSingle(), 0b11_01_11_01).AsInt32();
                var ymm11 = Avx.Shuffle(ymm6.AsSingle(), ymm7.AsSingle(), 0b11_01_11_01).AsInt32();
                ymm2 = Avx2.And(ymm8, ymm15.AsInt32());
                ymm3 = Avx2.And(ymm9, ymm15.AsInt32());
                ymm4 = Avx2.And(ymm10, ymm15.AsInt32());
                ymm5 = Avx2.And(ymm11, ymm15.AsInt32());
                ymm8 = Avx2.Abs(ymm8).AsInt32();
                ymm9 = Avx2.Abs(ymm9).AsInt32();
                ymm10 = Avx2.Abs(ymm10).AsInt32();
                ymm11 = Avx2.Abs(ymm11).AsInt32();
                ymm6 = Avx2.Subtract(ymm15, ymm8);
                ymm7 = Avx2.Subtract(ymm15, ymm9);
                ymm8 = Avx2.Min(ymm8.AsUInt32(), ymm6.AsUInt32()).AsInt32();
                ymm6 = Avx2.Subtract(ymm15, ymm10);
                ymm9 = Avx2.Min(ymm9.AsUInt32(), ymm7.AsUInt32()).AsInt32();
                ymm7 = Avx2.Subtract(ymm15, ymm11);
                ymm10 = Avx2.Min(ymm10.AsUInt32(), ymm6.AsUInt32()).AsInt32();
                ymm11 = Avx2.Min(ymm11.AsUInt32(), ymm7.AsUInt32()).AsInt32();
                ymm8 = Avx.ConvertToVector256Single(ymm8).AsInt32();
                ymm9 = Avx.ConvertToVector256Single(ymm9).AsInt32();
                ymm10 = Avx.ConvertToVector256Single(ymm10).AsInt32();
                ymm11 = Avx.ConvertToVector256Single(ymm11).AsInt32();
                ymm8 = Avx.Multiply(ymm8.AsSingle(), ymm14).AsInt32();
                ymm9 = Avx.Multiply(ymm9.AsSingle(), ymm14).AsInt32();
                ymm10 = Avx.Multiply(ymm10.AsSingle(), ymm14).AsInt32();
                ymm11 = Avx.Multiply(ymm11.AsSingle(), ymm14).AsInt32();
                ymm8 = Avx2.Xor(ymm2, ymm8);
                ymm9 = Avx2.Xor(ymm3, ymm9);
                ymm10 = Avx2.Xor(ymm4, ymm10);
                ymm11 = Avx2.Xor(ymm5, ymm11);
                Unsafe.As<float, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 0)) = ymm8;
                Unsafe.As<float, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 8)) = ymm9;
                Unsafe.As<float, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 16)) = ymm10;
                Unsafe.As<float, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 24)) = ymm11;
            }
            t = ymm0.GetElement(0);
            if (i < length)
            {
                var ymm3 = Avx.Shuffle(ymm0.AsSingle(), ymm1.AsSingle(), 0b11_01_11_01).AsUInt32();
                var ymm4 = Avx2.Abs(ymm3.AsInt32()).AsSingle();
                var ymm5 = Avx2.Subtract(ymm15.AsUInt32(), ymm4.AsUInt32()).AsSingle();
                ymm3 = Avx2.And(ymm3, ymm15.AsUInt32());
                ymm4 = Avx2.Min(ymm5.AsUInt32(), ymm4.AsUInt32()).AsSingle();
                ymm4 = Avx.ConvertToVector256Single(ymm4.AsInt32());
                ymm4 = Avx.Multiply(ymm14, ymm4);
                ymm4 = Avx2.Xor(ymm4.AsUInt32(), ymm3).AsSingle();
                var xmm6 = ymm4.GetUpper();
                var xmm5 = ymm4.GetLower();
                if (i < length - 3)
                {
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i)) = xmm5;
                    xmm5 = xmm6;
                    i += 4;
                    t += 4 * o;
                }
                if (i < length - 1)
                {
                    Unsafe.As<float, double>(ref Unsafe.Add(ref rdi, i)) = xmm5.AsDouble().GetElement(0);
                    xmm5 = Ssse3.AlignRight(xmm5.AsByte(), xmm5.AsByte(), 8).AsSingle();
                    i += 2;
                    t += 2 * o;
                }
                for (; i < length; i++)
                {
                    Unsafe.Add(ref rdi, i) = xmm5.GetElement(0);
                    xmm5 = Ssse3.AlignRight(xmm5.AsByte(), xmm5.AsByte(), 4).AsSingle();
                    t += o;
                }
            }
            return new Fixed64(t);
        }
#endif


        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static Fixed64 GenerateMonauralBlockStandard(Span<float> buffer, Fixed64 omega, ref Fixed64 theta)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = GenerateMonauralSample(theta);
                theta = AppendTheta(theta, omega);
            }

            return theta;
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static Fixed64 AppendTheta(Fixed64 theta, Fixed64 omega) => theta + omega;


        /// <summary>
        /// Generates the monaural sample.
        /// </summary>
        /// <param name="theta">The theta(from -pi to pi).</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static float GenerateMonauralSample(Fixed64 theta)
        {
            const float Multiplier = 1.0f / 0x4000_0000;
            var t = (int)(theta.Value >> 32);
            var s = -(t >> 31);
            var n = MathI.Abs(t);
            n = MathI.Min(n, 0x8000_0000u - n);
            n += (uint)s;
            n ^= (uint)s;
            var f = n * Multiplier;
            return f;
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //
                }

                disposedValue = true;
            }
        }


        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
