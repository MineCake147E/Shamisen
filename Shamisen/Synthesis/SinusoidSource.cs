﻿using System;
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

        /// <summary>
        /// Gets the format of the audio data.
        /// </summary>
        /// <value>
        /// The format of the audio data.
        /// </value>
        public SampleFormat Format { get; }

        /// <summary>
        /// Gets or sets the frequency.
        /// </summary>
        /// <value>
        /// The frequency.
        /// </value>
        public double Frequency
        {
            get => frequency;
            set
            {
                frequency = value;
                Omega = (Fixed64)Math.Abs(2 * value * SamplingFrequencyInverse);
            }
        }

        private double SamplingFrequencyInverse { get; }

        private Fixed64 Theta { get; set; } = Fixed64.Zero;

        private Fixed64 Omega { get; set; }

        /// <summary>
        /// Gets the skip support of the <see cref="IAudioSource{TSample,TFormat}"/>.
        /// </summary>
        /// <value>
        /// The skip support.
        /// </value>
        public ISkipSupport? SkipSupport { get => null; }

        /// <summary>
        /// Gets the seek support of the <see cref="IAudioSource{TSample,TFormat}"/>.
        /// </summary>
        /// <value>
        /// The seek support.
        /// </value>
        public ISeekSupport? SeekSupport { get => null; }

        ulong? IAudioSource<float, SampleFormat>.Length => null;

        ulong? IAudioSource<float, SampleFormat>.TotalLength => null;

        ulong? IAudioSource<float, SampleFormat>.Position => null;

        /// <summary>
        /// Reads the audio to the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>The length of the data written.</returns>
        public ReadResult Read(Span<float> buffer)
        {
            var channels = Format.Channels;
            buffer = buffer.SliceAlign(channels);
            var omega = Omega;
            var theta = Theta;
            switch (channels)   //Unrolling assignations
            {
                case 1:
                    theta = GenerateMonauralBlock(buffer, omega, theta);
                    Theta = theta;
                    return buffer.Length;
                case 2:
                    {
                        var bspan = buffer.SliceFromEnd(buffer.Length / 2);
                        theta = GenerateMonauralBlock(bspan, omega, theta);
                        Theta = theta;
                        var bispan = MemoryMarshal.Cast<float, int>(bspan);
                        AudioUtils.InterleaveStereo(MemoryMarshal.Cast<float, int>(buffer), bispan, bispan);
                        return buffer.Length;
                    }
                case 3:
                    {
                        var bspan = buffer.SliceFromEnd(buffer.Length / 3);
                        theta = GenerateMonauralBlock(bspan, omega, theta);
                        Theta = theta;
                        var bispan = MemoryMarshal.Cast<float, int>(bspan);
                        AudioUtils.InterleaveThree(MemoryMarshal.Cast<float, int>(buffer), bispan, bispan, bispan);
                        return buffer.Length;
                    }
                case 4:
                    {
                        var bspan = buffer.SliceFromEnd(buffer.Length / 4);
                        theta = GenerateMonauralBlock(bspan, omega, theta);
                        Theta = theta;
                        var bispan = MemoryMarshal.Cast<float, int>(bspan);
                        AudioUtils.InterleaveQuad(MemoryMarshal.Cast<float, int>(buffer), bispan, bispan, bispan, bispan);
                        return buffer.Length;
                    }
                case < 8:
                    {
                        var bspan = buffer.SliceFromEnd(buffer.Length / channels);
                        theta = GenerateMonauralBlock(bspan, omega, theta);
                        Theta = theta;
                        int h = 0;
                        for (int i = 0; i < bspan.Length; i++, h += channels)
                        {
                            var value = bspan[i];
                            for (int j = 0; j < channels; j++)
                            {
                                buffer[h + j] = value;
                            }
                        }
                        return buffer.Length;
                    }
                default:
                    {
                        var bspan = buffer.SliceFromEnd(buffer.Length / channels);
                        theta = GenerateMonauralBlock(bspan, omega, theta);
                        Theta = theta;
                        int h = 0;
                        for (int i = 0; i < bspan.Length; i++, h += channels)
                        {
                            var value = bspan[i];
                            buffer.Slice(h, channels).FastFill(value);
                        }
                        return buffer.Length;
                    }
            }
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
        const double PiSquared = Math.PI * Math.PI;
        const double C0d = Math.PI;
        const double C1d = C0d / 2 / 3 * PiSquared;
        const double C2d = C1d / 4 / 5 * PiSquared;
        const double C3d = C2d / 6 / 7 * PiSquared;
        const double C4d = C3d / 8 / 9 * PiSquared;
        const double C5d = C4d / 10 / 11 * PiSquared;
        const float C0 = (float)C0d;
        const float C1 = (float)C1d;
        const float C2 = (float)C2d;
        const float C3 = (float)C3d;
        const float C4 = (float)C4d;
        const float C5 = (float)C5d;
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static Fixed64 GenerateMonauralBlockAvx2FmaMM256(Span<float> buffer, Fixed64 omega, Fixed64 theta)
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
            var ymm14 = Vector256.Create((float)(-1.0 / int.MinValue));
            var ymm13 = Vector256.Create(-C5);
            var ymm12 = Vector256.Create(C4);
            var ymm11 = Vector256.Create(-C3);
            var ymm10 = Vector256.Create(C2);
            var ymm9 = Vector256.Create(-C1);
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
            var t = theta.Value;
            var o = omega.Value;
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
