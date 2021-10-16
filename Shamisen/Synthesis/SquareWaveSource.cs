using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using Shamisen.Utils;

#if NETCOREAPP3_1_OR_GREATER
using System.Runtime.Intrinsics.X86;
using System.Runtime.Intrinsics;
#endif

using DivideSharp;

namespace Shamisen.Synthesis
{
    /// <summary>
    /// Generates a square wave with specified frequency.
    /// </summary>
    /// <seealso cref="ISampleSource" />
    public sealed class SquareWaveSource : ISampleSource, IFrequencyGeneratorSource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SquareWaveSource"/> class.
        /// </summary>
        /// <param name="format">The output format.</param>
        public SquareWaveSource(SampleFormat format)
        {
            SamplingFrequencyInverse = 1.0 / format.SampleRate;
            Format = format;
            ChannelsDivisor = new(format.Channels);
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
            get => AngularVelocity.DoubleValue * Format.SampleRate * 0.5;
            set => AngularVelocity = (Fixed64)Math.Abs(2.0 * value / Format.SampleRate);
        }

        private double SamplingFrequencyInverse { get; }

        /// <summary>
        /// Gets or sets where the <see cref="IAudioSource{TSample,TFormat}"/> is.
        /// Some implementation could not support this property.
        /// </summary>
        public long Position { get => Theta.Value; set => Theta = new(value); }

        private Fixed64 Theta { get; set; } = Fixed64.Zero;

        /// <summary>
        /// Gets or sets the angular velocity of this <see cref="SquareWaveSource"/>.
        /// </summary>
        public Fixed64 AngularVelocity { get; set; }

        private Int32Divisor ChannelsDivisor { get; set; }

        /// <summary>
        /// Gets the skip support of the <see cref="IAudioSource{TSample,TFormat}"/>.
        /// </summary>
        /// <value>
        /// The skip support.
        /// </value>
        public ISkipSupport? SkipSupport => null;

        /// <summary>
        /// Gets the seek support of the <see cref="IAudioSource{TSample,TFormat}"/>.
        /// </summary>
        /// <value>
        /// The seek support.
        /// </value>
        public ISeekSupport? SeekSupport => null;

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
            int channels = Format.Channels;
            buffer = buffer.SliceAlign(channels);
            var omega = AngularVelocity;
            var theta = Theta;
            var r = Process(buffer, omega, theta, channels, ChannelsDivisor, out var nt);
            Theta = nt;
            return r;
        }

        /// <summary>
        /// Generates the monaural sample.
        /// </summary>
        /// <param name="theta">The theta(from -pi to pi).</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static float GenerateMonauralSample(Fixed64 theta)
        {
            int y = (int)(theta.Value >> 32) & int.MinValue;
            y |= 0x3f80_0000;
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
            return BitConverter.Int32BitsToSingle(y);
#else
            return Unsafe.As<int, float>(ref y);
#endif
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static ulong GetDurationOfSameValue(UInt64Divisor omega, Fixed64 theta)
        {
            ulong r = omega.DivRem(0x8000_0000_0000_0000u - (ulong)theta.Value % 0x8000_0000_0000_0000u, out ulong q);
            return r > 0 ? q + 1 : q;
        }

        internal static ReadResult Process(Span<float> buffer, Fixed64 omega, Fixed64 theta, int channels, Int32Divisor channelsDivisor, out Fixed64 newTheta)
        {
            var bspan = buffer.SliceFromEnd(buffer.Length / channelsDivisor);
            theta = GenerateMonauralBlock(bspan, omega, theta);
            newTheta = theta;
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
            return GenerateMonauralBlockStandard(buffer, omega, theta);
        }

#if NETCOREAPP3_1_OR_GREATER
        #region X86 intrinsics
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static Fixed64 GenerateMonauralBlockAvx2MM256(Span<float> buffer, Fixed64 omega, Fixed64 theta)
        {
            //TODO: AdvSimd and Sse42 variant
            long t = theta.Value;
            long o = omega.Value;
            var ymm12 = Vector256.Create(o * 4);
            var xmm15 = Vector128.Create(0L, o);
            var xmm14 = Sse2.Add(xmm15, ymm12.GetLower());
            var ymm15 = xmm15.ToVector256Unsafe().WithUpper(xmm14).AsSingle();
            ymm12 = Avx2.Add(ymm12, ymm12);
            var ymm0 = Vector256.Create(t);
            var ymm1 = Vector256.Create(t + o * 2);
            ymm0 = Avx2.Add(ymm0, ymm15.AsInt64());
            ymm1 = Avx2.Add(ymm1, ymm15.AsInt64());
            var ymm14 = Vector256.Create(1.0f).AsInt32();
            var ymm13 = Vector256.Create(int.MinValue).AsInt32();
            ref float rdi = ref MemoryMarshal.GetReference(buffer);
            nint i = 0, length = buffer.Length;
            nint olen = length - 31;
            //Everything is in integer, so Haswell will be fine even with 256-bit vectors.
            for (; i < olen; i += 32)
            {
                var ymm2 = Avx2.Add(ymm0, ymm12);
                var ymm3 = Avx2.Add(ymm1, ymm12);
                var ymm5 = Avx2.Add(ymm3, ymm12);
                var ymm4 = Avx2.Add(ymm2, ymm12);
                var ymm6 = Avx2.Add(ymm4, ymm12);
                var ymm7 = Avx2.Add(ymm5, ymm12);
                var ymm8 = Avx.Shuffle(ymm0.AsSingle(), ymm1.AsSingle(), 0b11_01_11_01).AsInt32();
                var ymm9 = Avx.Shuffle(ymm2.AsSingle(), ymm3.AsSingle(), 0b11_01_11_01).AsInt32();
                var ymm10 = Avx.Shuffle(ymm4.AsSingle(), ymm5.AsSingle(), 0b11_01_11_01).AsInt32();
                var ymm11 = Avx.Shuffle(ymm6.AsSingle(), ymm7.AsSingle(), 0b11_01_11_01).AsInt32();
                ymm8 = Avx2.And(ymm8, ymm13);
                ymm9 = Avx2.And(ymm9, ymm13);
                ymm10 = Avx2.And(ymm10, ymm13);
                ymm11 = Avx2.And(ymm11, ymm13);
                ymm8 = Avx2.Or(ymm8, ymm14);
                ymm9 = Avx2.Or(ymm9, ymm14);
                ymm10 = Avx2.Or(ymm10, ymm14);
                ymm11 = Avx2.Or(ymm11, ymm14);
                Unsafe.As<float, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 0)) = ymm8;
                Unsafe.As<float, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 8)) = ymm9;
                Unsafe.As<float, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 16)) = ymm10;
                Unsafe.As<float, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 24)) = ymm11;
                ymm1 = Avx2.Add(ymm7, ymm12);
                ymm0 = Avx2.Add(ymm6, ymm12);
            }
            olen = length - 7;
            for (; i < olen; i += 8)
            {
                var ymm3 = Avx.Shuffle(ymm0.AsSingle(), ymm1.AsSingle(), 0b11_01_11_01).AsInt32();
                var ymm4 = Avx2.And(ymm3, ymm13);
                var ymm5 = Avx2.Or(ymm4, ymm14);
                Unsafe.As<float, Vector256<int>>(ref Unsafe.Add(ref rdi, i)) = ymm5;
                ymm0 = Avx2.Add(ymm0, ymm12);
                ymm1 = Avx2.Add(ymm1, ymm12);
            }
            t = ymm0.GetElement(0);
            if (i < length)
            {
                var ymm3 = Avx.Shuffle(ymm0.AsSingle(), ymm1.AsSingle(), 0b11_01_11_01).AsInt32();
                var ymm4 = Avx2.And(ymm3, ymm13);
                var ymm5 = Avx2.Or(ymm4, ymm14).AsSingle();
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
                    xmm5 = Ssse3.AlignRight(xmm5.AsInt32(), xmm5.AsInt32(), 8).AsSingle();
                    i += 2;
                    t += 2 * o;
                }
                if (i < length)
                {
                    Unsafe.Add(ref rdi, i) = xmm5.GetElement(0);
                    t += o;
                    i++;
                }
            }
            for (; i < length; i++)
            {
                int y = (int)(t >> 32);
                t += o;
                y &= int.MinValue;
                y |= 0x3f80_0000;
                Unsafe.As<float, int>(ref Unsafe.Add(ref rdi, i)) = y;
            }
            return new Fixed64(t);
        }
        #endregion
#endif

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static Fixed64 GenerateMonauralBlockStandard(Span<float> buffer, Fixed64 omega, Fixed64 theta)
        {
            long t = theta.Value;
            long o = omega.Value;
            ref int rdi = ref Unsafe.As<float, int>(ref MemoryMarshal.GetReference(buffer));
            nint i = 0, length = buffer.Length;
            nint olen = length - 7;
            for (; i < olen; i += 8)
            {
                int y = ((int)(t >> 32) & int.MinValue) | 0x3f80_0000;
                t += o;
                Unsafe.Add(ref rdi, i + 0) = y;
                y = ((int)(t >> 32) & int.MinValue) | 0x3f80_0000;
                t += o;
                Unsafe.Add(ref rdi, i + 1) = y;
                y = ((int)(t >> 32) & int.MinValue) | 0x3f80_0000;
                t += o;
                Unsafe.Add(ref rdi, i + 2) = y;
                y = ((int)(t >> 32) & int.MinValue) | 0x3f80_0000;
                t += o;
                Unsafe.Add(ref rdi, i + 3) = y;
                y = ((int)(t >> 32) & int.MinValue) | 0x3f80_0000;
                t += o;
                Unsafe.Add(ref rdi, i + 4) = y;
                y = ((int)(t >> 32) & int.MinValue) | 0x3f80_0000;
                t += o;
                Unsafe.Add(ref rdi, i + 5) = y;
                y = ((int)(t >> 32) & int.MinValue) | 0x3f80_0000;
                t += o;
                Unsafe.Add(ref rdi, i + 6) = y;
                y = ((int)(t >> 32) & int.MinValue) | 0x3f80_0000;
                t += o;
                Unsafe.Add(ref rdi, i + 7) = y;
            }
            for (; i < length; i++)
            {
                int y = (int)(t >> 32);
                t += o;
                y &= int.MinValue;
                y |= 0x3f80_0000;
                Unsafe.Add(ref rdi, i) = y;
            }
            return new Fixed64(t);
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
