using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using Shamisen.Utils;

#if NETCOREAPP3_1_OR_GREATER
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;


#endif


namespace Shamisen.Synthesis
{
    /// <summary>
    /// Generates a sinusoid wave with specified frequency.
    /// </summary>
    public sealed class SinusoidSource : ISampleSource, IFrequencyGeneratorSource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SinusoidSource"/> class.
        /// </summary>
        /// <param name="format">The output format.</param>
        public SinusoidSource(SampleFormat format)
        {
            SamplingFrequencyInverse = 1.0 / format.SampleRate;
            Format = format;
        }

        /// <inheritdoc/>
        public SampleFormat Format { get; }

        /// <summary>
        /// Gets or sets the frequency.
        /// </summary>
        /// <value>
        /// The frequency.
        /// </value>
        public double Frequency
        {
            get => AngularVelocity.DoubleValue * Format.SampleRate * 0.5;
            set => AngularVelocity = (Fixed64)Math.Abs(2.0 * value / Format.SampleRate);
        }

        private double SamplingFrequencyInverse { get; }

        private Fixed64 Theta { get; set; } = Fixed64.Zero;
        /// <summary>
        /// Gets or sets the angular velocity of this <see cref="SinusoidSource"/>.
        /// </summary>
        public Fixed64 AngularVelocity { get; set; }

        /// <inheritdoc/>
        public ISkipSupport? SkipSupport => null;

        /// <inheritdoc/>
        public ISeekSupport? SeekSupport => null;

        ulong? IAudioSource<float, SampleFormat>.Length => null;

        ulong? IAudioSource<float, SampleFormat>.TotalLength => null;

        ulong? IAudioSource<float, SampleFormat>.Position => null;

        /// <inheritdoc/>
        public ReadResult Read(Span<float> buffer)
        {
            int channels = Format.Channels;
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
            if (Avx2.IsSupported && Fma.IsSupported)
            {
                return GenerateMonauralBlockAvx2FmaMM256(buffer, omega, theta);
            }
            if (Avx2.IsSupported)
            {
                return GenerateMonauralBlockAvx2MM256(buffer, omega, theta);
            }
#endif
            return GenerateMonauralBlockStandard(buffer, omega, ref theta);
        }

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
#if NETCOREAPP3_1_OR_GREATER
        private const double PiSquared = Math.PI * Math.PI;
        private const double C0d = Math.PI;
        private const double C1d = C0d / 2 / 3 * PiSquared;
        private const double C2d = C1d / 4 / 5 * PiSquared;
        private const double C3d = C2d / 6 / 7 * PiSquared;
        private const double C4d = C3d / 8 / 9 * PiSquared;
        private const double C5d = C4d / 10 / 11 * PiSquared;
        private const float C0 = (float)C0d;
        private const float C1 = (float)C1d;
        private const float C2 = (float)C2d;
        private const float C3 = (float)C3d;
        private const float C4 = (float)C4d;
        private const float C5 = (float)C5d;
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static Fixed64 GenerateMonauralBlockAvx2FmaMM256(Span<float> buffer, Fixed64 omega, Fixed64 theta)
        {
            long t = theta.Value;
            long o = omega.Value;
            var ymm2 = Vector256.Create(o * 4);
            var xmm15 = Vector128.Create(0L, o);
            var xmm14 = Sse2.Add(xmm15, ymm2.GetLower());
            var ymm15 = xmm15.ToVector256Unsafe().WithUpper(xmm14).AsSingle();
            ymm2 = Avx2.Add(ymm2, ymm2);
            var ymm0 = Vector256.Create(t);
            var ymm1 = Vector256.Create(t + o * 2);
            ymm0 = Avx2.Add(ymm0, ymm15.AsInt64());
            ymm1 = Avx2.Add(ymm1, ymm15.AsInt64());
            ymm15 = Vector256.Create(0x8000_0000u).AsSingle();
            var ymm14 = Vector256.Create((float)(-1.0 / int.MinValue));
            var ymm13 = Vector256.Create(-C5);
            var ymm12 = Vector256.Create(C4);
            var ymm11 = Vector256.Create(-C3);
            var ymm10 = Vector256.Create(C2);
            var ymm9 = Vector256.Create(-C1);
            var ymm8 = Vector256.Create(C0);
            //TODO: AdvSimd, Avx2FmaMM, and Sse42 variant
            ref float rdi = ref MemoryMarshal.GetReference(buffer);
            nint i = 0, length = buffer.Length;
            nint olen = length - 7;
            for (; i < olen; i += 8)
            {
                var ymm3 = Avx.Shuffle(ymm0.AsSingle(), ymm1.AsSingle(), 0b11_01_11_01).AsUInt32();
                var ymm4 = Avx2.Abs(ymm3.AsInt32()).AsSingle();
                var ymm5 = Avx2.Subtract(ymm15.AsUInt32(), ymm4.AsUInt32()).AsSingle();
                ymm4 = Avx2.Min(ymm5.AsUInt32(), ymm4.AsUInt32()).AsSingle();
                ymm4 = Avx.ConvertToVector256Single(ymm4.AsInt32());
                ymm4 = Avx.Multiply(ymm14, ymm4);
                //Calculate SinPi(x)
                ymm5 = Avx.Multiply(ymm4, ymm4);
                var ymm6 = ymm13;
                ymm6 = Fma.MultiplyAdd(ymm5, ymm6, ymm12);
                ymm6 = Fma.MultiplyAdd(ymm5, ymm6, ymm11);
                ymm6 = Fma.MultiplyAdd(ymm5, ymm6, ymm10);
                ymm6 = Fma.MultiplyAdd(ymm5, ymm6, ymm9);
                ymm6 = Fma.MultiplyAdd(ymm5, ymm6, ymm8);
                ymm5 = Avx.And(ymm3.AsSingle(), ymm15).AsSingle();
                ymm6 = Avx.Multiply(ymm6, ymm4);
                ymm5 = Avx.Xor(ymm6, ymm5);
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i)) = ymm5;
                ymm0 = Avx2.Add(ymm0, ymm2);
                ymm1 = Avx2.Add(ymm1, ymm2);
            }
            t = ymm0.GetElement(0);
            if (i < length)
            {
                var ymm3 = Avx.Shuffle(ymm0.AsSingle(), ymm1.AsSingle(), 0b11_01_11_01).AsUInt32();
                var ymm4 = Avx2.Abs(ymm3.AsInt32()).AsSingle();
                var ymm5 = Avx2.Subtract(ymm15.AsUInt32(), ymm4.AsUInt32()).AsSingle();
                ymm4 = Avx2.Min(ymm5.AsUInt32(), ymm4.AsUInt32()).AsSingle();
                ymm4 = Avx.ConvertToVector256Single(ymm4.AsInt32());
                ymm4 = Avx.Multiply(ymm14, ymm4);
                ymm5 = Avx.Multiply(ymm4, ymm4);
                var ymm6 = ymm13;
                ymm6 = Fma.MultiplyAdd(ymm5, ymm6, ymm12);
                ymm6 = Fma.MultiplyAdd(ymm5, ymm6, ymm11);
                ymm6 = Fma.MultiplyAdd(ymm5, ymm6, ymm10);
                ymm6 = Fma.MultiplyAdd(ymm5, ymm6, ymm9);
                ymm6 = Fma.MultiplyAdd(ymm5, ymm6, ymm8);
                ymm5 = Avx.And(ymm3.AsSingle(), ymm15).AsSingle();
                ymm6 = Avx.Multiply(ymm6, ymm4);
                ymm5 = Avx.Xor(ymm6, ymm5);
                var xmm6 = ymm5.GetUpper();
                var xmm5 = ymm5.GetLower();
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
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static Fixed64 GenerateMonauralBlockAvx2MM256(Span<float> buffer, Fixed64 omega, Fixed64 theta)
        {
            long t = theta.Value;
            long o = omega.Value;
            var ymm2 = Vector256.Create(o * 8);
            var ymm0 = Vector256.Create(t);
            var ymm1 = Vector256.Create(t + o * 2);
            var ymm15 = Vector256.Create(0L, o * 1, o * 4, o * 5).AsSingle();
            ymm0 = Avx2.Add(ymm0, ymm15.AsInt64());
            ymm1 = Avx2.Add(ymm1, ymm15.AsInt64());
            ymm15 = Vector256.Create(0x8000_0000u).AsSingle();
            var ymm14 = Vector256.Create((float)(-1.0 / int.MinValue));
            var ymm13 = Vector256.Create(-C5);
            var ymm12 = Vector256.Create(C4);
            var ymm11 = Vector256.Create(-C3);
            var ymm10 = Vector256.Create(C2);
            var ymm9 = Vector256.Create(-C1);
            var ymm8 = Vector256.Create(C0);

            ref float rdi = ref MemoryMarshal.GetReference(buffer);
            nint i = 0, length = buffer.Length;
            nint olen = length - 7;
            for (; i < olen; i += 8)
            {
                var ymm3 = Avx.Shuffle(ymm0.AsSingle(), ymm1.AsSingle(), 0b11_01_11_01).AsUInt32();
                var ymm4 = Avx2.Abs(ymm3.AsInt32()).AsSingle();
                var ymm5 = Avx2.Subtract(ymm15.AsUInt32(), ymm4.AsUInt32()).AsSingle();
                ymm4 = Avx2.Min(ymm5.AsUInt32(), ymm4.AsUInt32()).AsSingle();
                ymm4 = Avx.ConvertToVector256Single(ymm4.AsInt32());
                ymm4 = Avx.Multiply(ymm14, ymm4);
                ymm5 = Avx.Multiply(ymm4, ymm4);
                var ymm6 = ymm13;
                ymm6 = Avx.Multiply(ymm5, ymm6);
                ymm6 = Avx.Add(ymm6, ymm12);
                ymm6 = Avx.Multiply(ymm5, ymm6);
                ymm6 = Avx.Add(ymm6, ymm11);
                ymm6 = Avx.Multiply(ymm5, ymm6);
                ymm6 = Avx.Add(ymm6, ymm10);
                ymm6 = Avx.Multiply(ymm5, ymm6);
                ymm6 = Avx.Add(ymm6, ymm9);
                ymm6 = Avx.Multiply(ymm5, ymm6);
                ymm6 = Avx.Add(ymm6, ymm8);
                ymm5 = Avx.And(ymm3.AsSingle(), ymm15).AsSingle();
                ymm6 = Avx.Multiply(ymm6, ymm4);
                ymm5 = Avx.Xor(ymm6, ymm5);
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i)) = ymm5;
                ymm0 = Avx2.Add(ymm0, ymm2);
                ymm1 = Avx2.Add(ymm1, ymm2);
            }
            t = ymm0.GetElement(0);
            if (i < length)
            {
                var ymm3 = Avx.Shuffle(ymm0.AsSingle(), ymm1.AsSingle(), 0b11_01_11_01).AsUInt32();
                var ymm4 = Avx2.Abs(ymm3.AsInt32()).AsSingle();
                var ymm5 = Avx2.Subtract(ymm15.AsUInt32(), ymm4.AsUInt32()).AsSingle();
                ymm4 = Avx2.Min(ymm5.AsUInt32(), ymm4.AsUInt32()).AsSingle();
                ymm4 = Avx.ConvertToVector256Single(ymm4.AsInt32());
                ymm4 = Avx.Multiply(ymm14, ymm4);
                ymm5 = Avx.Multiply(ymm4, ymm4);
                var ymm6 = ymm13;
                ymm6 = Avx.Multiply(ymm5, ymm6);
                ymm6 = Avx.Add(ymm6, ymm12);
                ymm6 = Avx.Multiply(ymm5, ymm6);
                ymm6 = Avx.Add(ymm6, ymm11);
                ymm6 = Avx.Multiply(ymm5, ymm6);
                ymm6 = Avx.Add(ymm6, ymm10);
                ymm6 = Avx.Multiply(ymm5, ymm6);
                ymm6 = Avx.Add(ymm6, ymm9);
                ymm6 = Avx.Multiply(ymm5, ymm6);
                ymm6 = Avx.Add(ymm6, ymm8);
                ymm5 = Avx.And(ymm3.AsSingle(), ymm15).AsSingle();
                ymm6 = Avx.Multiply(ymm6, ymm4);
                ymm5 = Avx.Xor(ymm6, ymm5);
                var xmm6 = ymm5.GetUpper();
                var xmm5 = ymm5.GetLower();
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
        private static Fixed64 AppendTheta(Fixed64 theta, Fixed64 omega) => theta + omega;

        /// <summary>
        /// Generates the monaural sample.
        /// </summary>
        /// <param name="theta">The theta(from -pi to pi).</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static float GenerateMonauralSample(Fixed64 theta) => MathX.SinF(theta);

        #region IDisposable Support

        private bool disposedValue = false; //
        private double frequency;

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
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

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}
