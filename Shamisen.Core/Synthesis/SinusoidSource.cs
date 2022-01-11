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
        private const double PiSquared = Math.PI * Math.PI;

        private const double C0d = Math.PI;
        private const double C1d = C0d / 2 / 3 * PiSquared;
        private const double C2d = C1d / 4 / 5 * PiSquared;
        private const double C3d = C2d / 6 / 7 * PiSquared;
        private const double C4d = C3d / 8 / 9 * PiSquared;
        private const double C5d = C4d / 10 / 11 * PiSquared;
        private const float C0 = 3.1415926f;
        private const float C1 = -5.1677116f;
        private const float C2 = 2.5501449f;
        private const float C3 = -5.9913243e-1f;
        private const float C4 = 8.1701727e-2f;
        private const float C5 = -6.647926e-3f;
        private const float X2FRatio = (float)(-1.0 / int.MinValue);

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
            set => AngularVelocity = CalculateAngularVelocity(value, Format.SampleRate);
        }
        /// <summary>
        /// Calculates angular velocity from <paramref name="sampleRate"/> and <paramref name="frequency"/>.
        /// </summary>
        /// <param name="frequency">The frequency.</param>
        /// <param name="sampleRate">The sampling rate.</param>
        /// <returns>The angular velocity that can be used for <see cref="IFrequencyGeneratorSource"/>.</returns>
        internal static Fixed64 CalculateAngularVelocity(double frequency, int sampleRate) => (Fixed64)Math.Abs(2.0 * frequency / sampleRate);

        /// <summary>
        /// Gets or sets the value which indicates whether the <see cref="SinusoidSource"/> can utilize fused multiply-adds for calculation, or not.
        /// </summary>
        public bool AllowFma { get; set; } = true;

        private double SamplingFrequencyInverse { get; }

        /// <summary>
        /// Gets or sets the current phase of this <see cref="SinusoidSource"/>.
        /// </summary>
        public Fixed64 Theta { get; set; } = Fixed64.Zero;

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
            var channels = Format.Channels;
            buffer = buffer.SliceAlign(channels);
            var omega = AngularVelocity;
            var theta = Theta;
            var bspan = buffer.SliceFromEnd(buffer.Length / channels);
            theta = GenerateMonauralBlock(bspan, omega, theta, AllowFma);
            Theta = theta;
            if (channels == 1) return buffer.Length;
            AudioUtils.DuplicateMonauralToChannels(buffer, bspan, channels);
            return buffer.Length;
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static Fixed64 GenerateMonauralBlock(Span<float> buffer, Fixed64 omega, Fixed64 theta, bool allowFma = false)
        {
            unchecked
            {
#if NETCOREAPP3_1_OR_GREATER
                if (allowFma && Avx2.IsSupported && Fma.IsSupported)
                {
                    return GenerateMonauralBlockAvx2FmaMM256(buffer, omega, theta);
                }
                if (Avx2.IsSupported)
                {
                    return GenerateMonauralBlockAvx2MM256(buffer, omega, theta);
                }
                if (Sse41.IsSupported)
                {
                    return GenerateMonauralBlockSse41(buffer, omega, theta);
                }
#endif
                return GenerateMonauralBlockStandard(buffer, omega, theta);
            }
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static Fixed64 GenerateMonauralBlockStandard(Span<float> buffer, Fixed64 omega, Fixed64 theta)
        {
            for (var i = 0; i < buffer.Length; i++)
            {
                buffer[i] = GenerateMonauralSample(theta);
                theta += omega;
            }
            return theta;
        }

        #region X86
#if NETCOREAPP3_1_OR_GREATER
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        internal static Fixed64 GenerateMonauralBlockAvx2FmaMM256(Span<float> buffer, Fixed64 omega, Fixed64 theta)
        {
            var t = theta.Value;
            var o = omega.Value;
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
            var ymm14 = Vector256.Create(X2FRatio);
            var ymm13 = Vector256.Create(C5);
            var ymm12 = Vector256.Create(C4);
            var ymm11 = Vector256.Create(C3);
            var ymm10 = Vector256.Create(C2);
            var ymm9 = Vector256.Create(C1);
            var ymm8 = Vector256.Create(C0);
            //TODO: AdvSimd, Avx2FmaMM, and Sse42 variant
            ref var rdi = ref MemoryMarshal.GetReference(buffer);
            nint i = 0, length = buffer.Length;
            var olen = length - 7;
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
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        internal static Fixed64 GenerateMonauralBlockAvx2MM256(Span<float> buffer, Fixed64 omega, Fixed64 theta)
        {
            var t = theta.Value;
            var o = omega.Value;
            var ymm2 = Vector256.Create(o * 8);
            var ymm0 = Vector256.Create(t);
            var ymm1 = Vector256.Create(t + o * 2);
            var ymm15 = Vector256.Create(0L, o * 1, o * 4, o * 5).AsSingle();
            ymm0 = Avx2.Add(ymm0, ymm15.AsInt64());
            ymm1 = Avx2.Add(ymm1, ymm15.AsInt64());
            ymm15 = Vector256.Create(0x8000_0000u).AsSingle();
            var ymm14 = Vector256.Create(X2FRatio);
            var ymm13 = Vector256.Create(C5);
            var ymm12 = Vector256.Create(C4);
            var ymm11 = Vector256.Create(C3);
            var ymm10 = Vector256.Create(C2);
            var ymm9 = Vector256.Create(C1);
            var ymm8 = Vector256.Create(C0);

            ref var rdi = ref MemoryMarshal.GetReference(buffer);
            nint i = 0, length = buffer.Length;
            var olen = length - 7;
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
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        internal static Fixed64 GenerateMonauralBlockSse41(Span<float> buffer, Fixed64 omega, Fixed64 theta)
        {
            var t = theta.Value;
            var o = omega.Value;
            var xmm2 = Vector128.Create(o * 4);
            var xmm0 = Vector128.Create(t);
            var xmm1 = Vector128.Create(t + o * 2);
            var xmm15 = Vector128.Create(0L, o * 1).AsSingle();
            xmm0 = Sse2.Add(xmm0, xmm15.AsInt64());
            xmm1 = Sse2.Add(xmm1, xmm15.AsInt64());
            xmm15 = Vector128.Create(0x8000_0000u).AsSingle();
            var xmm14 = Vector128.Create(X2FRatio);
            var xmm13 = Vector128.Create(C5);
            var xmm12 = Vector128.Create(C4);
            var xmm11 = Vector128.Create(C3);
            var xmm10 = Vector128.Create(C2);
            var xmm9 = Vector128.Create(C1);
            var xmm8 = Vector128.Create(C0);
            ref var rdi = ref MemoryMarshal.GetReference(buffer);
            nint i = 0, length = buffer.Length;
            var olen = length - 3;
            for (; i < olen; i += 4)
            {
                var xmm3 = Sse.Shuffle(xmm0.AsSingle(), xmm1.AsSingle(), 0b11_01_11_01).AsUInt32();
                var xmm4 = Ssse3.Abs(xmm3.AsInt32()).AsSingle();
                var xmm5 = Sse2.Subtract(xmm15.AsUInt32(), xmm4.AsUInt32()).AsSingle();
                xmm4 = Sse41.Min(xmm5.AsUInt32(), xmm4.AsUInt32()).AsSingle();
                xmm4 = Sse2.ConvertToVector128Single(xmm4.AsInt32());
                xmm4 = Sse.Multiply(xmm14, xmm4);
                xmm5 = Sse.Multiply(xmm4, xmm4);
                var xmm6 = xmm13;
                xmm6 = Sse.Multiply(xmm5, xmm6);
                xmm6 = Sse.Add(xmm6, xmm12);
                xmm6 = Sse.Multiply(xmm5, xmm6);
                xmm6 = Sse.Add(xmm6, xmm11);
                xmm6 = Sse.Multiply(xmm5, xmm6);
                xmm6 = Sse.Add(xmm6, xmm10);
                xmm6 = Sse.Multiply(xmm5, xmm6);
                xmm6 = Sse.Add(xmm6, xmm9);
                xmm6 = Sse.Multiply(xmm5, xmm6);
                xmm6 = Sse.Add(xmm6, xmm8);
                xmm5 = Sse.And(xmm3.AsSingle(), xmm15).AsSingle();
                xmm6 = Sse.Multiply(xmm6, xmm4);
                xmm5 = Sse.Xor(xmm6, xmm5);
                Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i)) = xmm5;
                xmm0 = Sse2.Add(xmm0, xmm2);
                xmm1 = Sse2.Add(xmm1, xmm2);
            }
            t = xmm0.GetElement(0);
            if (i < length)
            {
                var xmm3 = Sse.Shuffle(xmm0.AsSingle(), xmm1.AsSingle(), 0b11_01_11_01).AsUInt32();
                var xmm4 = Ssse3.Abs(xmm3.AsInt32()).AsSingle();
                var xmm5 = Sse2.Subtract(xmm15.AsUInt32(), xmm4.AsUInt32()).AsSingle();
                xmm4 = Sse41.Min(xmm5.AsUInt32(), xmm4.AsUInt32()).AsSingle();
                xmm4 = Sse2.ConvertToVector128Single(xmm4.AsInt32());
                xmm4 = Sse.Multiply(xmm14, xmm4);
                xmm5 = Sse.Multiply(xmm4, xmm4);
                var xmm6 = xmm13;
                xmm6 = Sse.Multiply(xmm5, xmm6);
                xmm6 = Sse.Add(xmm6, xmm12);
                xmm6 = Sse.Multiply(xmm5, xmm6);
                xmm6 = Sse.Add(xmm6, xmm11);
                xmm6 = Sse.Multiply(xmm5, xmm6);
                xmm6 = Sse.Add(xmm6, xmm10);
                xmm6 = Sse.Multiply(xmm5, xmm6);
                xmm6 = Sse.Add(xmm6, xmm9);
                xmm6 = Sse.Multiply(xmm5, xmm6);
                xmm6 = Sse.Add(xmm6, xmm8);
                xmm5 = Sse.And(xmm3.AsSingle(), xmm15).AsSingle();
                xmm6 = Sse.Multiply(xmm6, xmm4);
                xmm5 = Sse.Xor(xmm6, xmm5);
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
        #endregion

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static Fixed64 AppendTheta(Fixed64 theta, Fixed64 omega) => theta + omega;

        /// <summary>
        /// Generates the monaural sample.
        /// </summary>
        /// <param name="theta">The theta(from -pi to pi).</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static float GenerateMonauralSample(Fixed64 theta) => Sin(theta);

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static float Sin(Fixed64 theta)
        {
            var f = ConvertFixed64ToSingle(theta);
            var g = f * f;
            var u = C5;
            u = u * g + C4;
            u = u * g + C3;
            u = u * g + C2;
            u = u * g + C1;
            u = u * g + C0;
            u *= f;
            return u;
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static float ConvertFixed64ToSingle(Fixed64 theta)
        {
            var t = (int)(theta.Value >> 32);
            var s = (float)(MathI.ZeroIfFalse(t < 0, -2) + 1);
            t = (int)MathI.Abs(t);
            var y = 0x8000_0000u - (uint)t;
            t = (int)MathI.Min((uint)t, y);
            var f = s * t * X2FRatio;
            return f;
        }

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
