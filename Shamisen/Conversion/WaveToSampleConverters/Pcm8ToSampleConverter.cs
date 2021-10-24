using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

#if NETCOREAPP3_1_OR_GREATER
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
#endif

namespace Shamisen.Conversion.WaveToSampleConverters
{
    /// <summary>
    /// Converts 8-bit PCM to Sample.
    /// </summary>
    /// <seealso cref="WaveToSampleConverterBase" />
    public sealed class Pcm8ToSampleConverter : WaveToSampleConverterBase
    {
        private const int BufferMax = 1024;
        private int ActualBufferMax => BufferMax * Source.Format.Channels;

        private const float Multiplier = 1.0f / 128;
        private Memory<byte> readBuffer;

        /// <summary>
        /// Initializes a new instance of the <see cref="Pcm8ToSampleConverter"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        public Pcm8ToSampleConverter(IReadableAudioSource<byte, IWaveFormat> source)
            : base(source, new SampleFormat(source.Format.Channels, source.Format.SampleRate))
        {
            readBuffer = new byte[ActualBufferMax];
        }

        /// <summary>
        /// Gets the bytes consumed per sample.
        /// </summary>
        /// <value>
        /// The bytes consumed per sample.
        /// </value>
        protected override int BytesPerSample => sizeof(byte);

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override ReadResult Read(Span<float> buffer)
        {
            var span = readBuffer.Span;
            var cursor = buffer;
            while (cursor.Length > 0)
            {
                var reader = span.SliceWhileIfLongerThan(cursor.Length);
                var rr = Source.Read(reader);
                if (rr.HasNoData)
                {
                    var len = buffer.Length - cursor.Length;
                    return rr.IsEndOfStream && len <= 0 ? rr : len;
                }
                var u = rr.Length;
                var wrote = reader.Slice(0, u);
                var dest = cursor.Slice(0, wrote.Length);
                if (wrote.Length != dest.Length)
                {
                    new InvalidOperationException(
                        $"The {nameof(wrote)}'s length and {nameof(dest)}'s length are not equal! This is a bug!").Throw();
                }

                Process(wrote, dest);
                cursor = cursor.Slice(u);
                if (u != reader.Length) return buffer.Length - cursor.Length;  //The Source doesn't fill whole reader so return here.
            }
            return buffer.Length;
        }

        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        private static void Process(Span<byte> wrote, Span<float> dest)
        {
            unchecked
            {
#if NETCOREAPP3_1_OR_GREATER
                if (Avx2.IsSupported)
                {
                    ProcessAvx2A(wrote, dest);
                    return;
                }
#endif
                ProcessStandard(wrote, dest);
            }
        }

        #region X86
#if NETCOREAPP3_1_OR_GREATER
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ProcessAvx2M(Span<byte> wrote, Span<float> dest)
        {
            ref var rdi = ref MemoryMarshal.GetReference(dest);
            ref var rsi = ref MemoryMarshal.GetReference(wrote);
            nint i = 0, length = MathI.Min(dest.Length, wrote.Length);
            var xmm0 = Vector128.Create((byte)0x80u).AsSByte();
            var ymm1 = Vector256.Create(Multiplier);
            var olen = length - 63;
            for (; i < olen; i += 64)
            {
                var xmm2 = Vector128.CreateScalarUnsafe(Unsafe.As<byte, ulong>(ref Unsafe.Add(ref rsi, i + 0 * 8))).AsSByte();
                var xmm3 = Vector128.CreateScalarUnsafe(Unsafe.As<byte, ulong>(ref Unsafe.Add(ref rsi, i + 1 * 8))).AsSByte();
                var xmm4 = Vector128.CreateScalarUnsafe(Unsafe.As<byte, ulong>(ref Unsafe.Add(ref rsi, i + 2 * 8))).AsSByte();
                var xmm5 = Vector128.CreateScalarUnsafe(Unsafe.As<byte, ulong>(ref Unsafe.Add(ref rsi, i + 3 * 8))).AsSByte();
                xmm2 = Sse2.Xor(xmm0, xmm2);
                xmm3 = Sse2.Xor(xmm0, xmm3);
                xmm4 = Sse2.Xor(xmm0, xmm4);
                xmm5 = Sse2.Xor(xmm0, xmm5);
                var ymm2 = Avx.ConvertToVector256Single(Avx2.ConvertToVector256Int32(xmm2));
                var ymm3 = Avx.ConvertToVector256Single(Avx2.ConvertToVector256Int32(xmm3));
                var ymm4 = Avx.ConvertToVector256Single(Avx2.ConvertToVector256Int32(xmm4));
                var ymm5 = Avx.ConvertToVector256Single(Avx2.ConvertToVector256Int32(xmm5));
                ymm2 = Avx.Multiply(ymm2, ymm1);
                ymm3 = Avx.Multiply(ymm3, ymm1);
                ymm4 = Avx.Multiply(ymm4, ymm1);
                ymm5 = Avx.Multiply(ymm5, ymm1);
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 0 * 8)) = ymm2;
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 1 * 8)) = ymm3;
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 2 * 8)) = ymm4;
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 3 * 8)) = ymm5;
                xmm2 = Vector128.CreateScalarUnsafe(Unsafe.As<byte, ulong>(ref Unsafe.Add(ref rsi, i + 4 * 8))).AsSByte();
                xmm3 = Vector128.CreateScalarUnsafe(Unsafe.As<byte, ulong>(ref Unsafe.Add(ref rsi, i + 5 * 8))).AsSByte();
                xmm4 = Vector128.CreateScalarUnsafe(Unsafe.As<byte, ulong>(ref Unsafe.Add(ref rsi, i + 6 * 8))).AsSByte();
                xmm5 = Vector128.CreateScalarUnsafe(Unsafe.As<byte, ulong>(ref Unsafe.Add(ref rsi, i + 7 * 8))).AsSByte();
                xmm2 = Sse2.Xor(xmm0, xmm2);
                xmm3 = Sse2.Xor(xmm0, xmm3);
                xmm4 = Sse2.Xor(xmm0, xmm4);
                xmm5 = Sse2.Xor(xmm0, xmm5);
                ymm2 = Avx.ConvertToVector256Single(Avx2.ConvertToVector256Int32(xmm2));
                ymm3 = Avx.ConvertToVector256Single(Avx2.ConvertToVector256Int32(xmm3));
                ymm4 = Avx.ConvertToVector256Single(Avx2.ConvertToVector256Int32(xmm4));
                ymm5 = Avx.ConvertToVector256Single(Avx2.ConvertToVector256Int32(xmm5));
                ymm2 = Avx.Multiply(ymm2, ymm1);
                ymm3 = Avx.Multiply(ymm3, ymm1);
                ymm4 = Avx.Multiply(ymm4, ymm1);
                ymm5 = Avx.Multiply(ymm5, ymm1);
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 4 * 8)) = ymm2;
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 5 * 8)) = ymm3;
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 6 * 8)) = ymm4;
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 7 * 8)) = ymm5;
            }
            olen = length - 15;
            for (; i < olen; i += 16)
            {
                var xmm2 = Vector128.CreateScalarUnsafe(Unsafe.As<byte, ulong>(ref Unsafe.Add(ref rsi, i))).AsSByte();
                var xmm3 = Vector128.CreateScalarUnsafe(Unsafe.As<byte, ulong>(ref Unsafe.Add(ref rsi, i + 8))).AsSByte();
                xmm2 = Sse2.Xor(xmm0, xmm2);
                xmm3 = Sse2.Xor(xmm0, xmm3);
                var ymm2 = Avx.ConvertToVector256Single(Avx2.ConvertToVector256Int32(xmm2));
                var ymm3 = Avx.ConvertToVector256Single(Avx2.ConvertToVector256Int32(xmm3));
                ymm2 = Avx.Multiply(ymm2, ymm1);
                ymm3 = Avx.Multiply(ymm3, ymm1);
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 0 * 8)) = ymm2;
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 1 * 8)) = ymm3;
            }
            for (; i < length; i++)
            {
                Unsafe.Add(ref rdi, i) = (Unsafe.Add(ref rsi, i) - 128) * ymm1.GetElement(0);
            }
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ProcessAvx2A(Span<byte> wrote, Span<float> dest)
        {
            ref var rdi = ref MemoryMarshal.GetReference(dest);
            ref var rsi = ref MemoryMarshal.GetReference(wrote);
            nint i = 0, length = MathI.Min(dest.Length, wrote.Length);
            var ymm0 = Vector256.Create(0x4000_0000);
            var ymm1 = Vector256.Create(-3.0f);
            var olen = length - 63;
            for (; i < olen; i += 64)
            {
                var ymm2 = Avx2.ConvertToVector256Int32(Vector128.CreateScalarUnsafe(Unsafe.As<byte, ulong>(ref Unsafe.Add(ref rsi, i + 0 * 8))).AsByte());
                var ymm3 = Avx2.ConvertToVector256Int32(Vector128.CreateScalarUnsafe(Unsafe.As<byte, ulong>(ref Unsafe.Add(ref rsi, i + 1 * 8))).AsByte());
                var ymm4 = Avx2.ConvertToVector256Int32(Vector128.CreateScalarUnsafe(Unsafe.As<byte, ulong>(ref Unsafe.Add(ref rsi, i + 2 * 8))).AsByte());
                var ymm5 = Avx2.ConvertToVector256Int32(Vector128.CreateScalarUnsafe(Unsafe.As<byte, ulong>(ref Unsafe.Add(ref rsi, i + 3 * 8))).AsByte());
                ymm2 = Avx2.ShiftLeftLogical(ymm2, 15);
                ymm3 = Avx2.ShiftLeftLogical(ymm3, 15);
                ymm4 = Avx2.ShiftLeftLogical(ymm4, 15);
                ymm5 = Avx2.ShiftLeftLogical(ymm5, 15);
                ymm2 = Avx2.Or(ymm2, ymm0);
                ymm3 = Avx2.Or(ymm3, ymm0);
                ymm4 = Avx2.Or(ymm4, ymm0);
                ymm5 = Avx2.Or(ymm5, ymm0);
                ymm2 = Avx.Add(ymm2.AsSingle(), ymm1).AsInt32();
                ymm3 = Avx.Add(ymm3.AsSingle(), ymm1).AsInt32();
                ymm4 = Avx.Add(ymm4.AsSingle(), ymm1).AsInt32();
                ymm5 = Avx.Add(ymm5.AsSingle(), ymm1).AsInt32();
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 0 * 8)) = ymm2.AsSingle();
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 1 * 8)) = ymm3.AsSingle();
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 2 * 8)) = ymm4.AsSingle();
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 3 * 8)) = ymm5.AsSingle();
                ymm2 = Avx2.ConvertToVector256Int32(Vector128.CreateScalarUnsafe(Unsafe.As<byte, ulong>(ref Unsafe.Add(ref rsi, i + 4 * 8))).AsByte());
                ymm3 = Avx2.ConvertToVector256Int32(Vector128.CreateScalarUnsafe(Unsafe.As<byte, ulong>(ref Unsafe.Add(ref rsi, i + 5 * 8))).AsByte());
                ymm4 = Avx2.ConvertToVector256Int32(Vector128.CreateScalarUnsafe(Unsafe.As<byte, ulong>(ref Unsafe.Add(ref rsi, i + 6 * 8))).AsByte());
                ymm5 = Avx2.ConvertToVector256Int32(Vector128.CreateScalarUnsafe(Unsafe.As<byte, ulong>(ref Unsafe.Add(ref rsi, i + 7 * 8))).AsByte());
                ymm2 = Avx2.ShiftLeftLogical(ymm2, 15);
                ymm3 = Avx2.ShiftLeftLogical(ymm3, 15);
                ymm4 = Avx2.ShiftLeftLogical(ymm4, 15);
                ymm5 = Avx2.ShiftLeftLogical(ymm5, 15);
                ymm2 = Avx2.Or(ymm2, ymm0);
                ymm3 = Avx2.Or(ymm3, ymm0);
                ymm4 = Avx2.Or(ymm4, ymm0);
                ymm5 = Avx2.Or(ymm5, ymm0);
                ymm2 = Avx.Add(ymm2.AsSingle(), ymm1).AsInt32();
                ymm3 = Avx.Add(ymm3.AsSingle(), ymm1).AsInt32();
                ymm4 = Avx.Add(ymm4.AsSingle(), ymm1).AsInt32();
                ymm5 = Avx.Add(ymm5.AsSingle(), ymm1).AsInt32();
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 4 * 8)) = ymm2.AsSingle();
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 5 * 8)) = ymm3.AsSingle();
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 6 * 8)) = ymm4.AsSingle();
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 7 * 8)) = ymm5.AsSingle();
            }
            olen = length - 15;
            for (; i < olen; i += 16)
            {
                var ymm2 = Avx2.ConvertToVector256Int32(Vector128.CreateScalarUnsafe(Unsafe.As<byte, ulong>(ref Unsafe.Add(ref rsi, i + 0 * 8))).AsByte());
                var ymm3 = Avx2.ConvertToVector256Int32(Vector128.CreateScalarUnsafe(Unsafe.As<byte, ulong>(ref Unsafe.Add(ref rsi, i + 1 * 8))).AsByte());
                ymm2 = Avx2.ShiftLeftLogical(ymm2, 15);
                ymm3 = Avx2.ShiftLeftLogical(ymm3, 15);
                ymm2 = Avx2.Or(ymm2, ymm0);
                ymm3 = Avx2.Or(ymm3, ymm0);
                ymm2 = Avx.Add(ymm2.AsSingle(), ymm1).AsInt32();
                ymm3 = Avx.Add(ymm3.AsSingle(), ymm1).AsInt32();
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 0 * 8)) = ymm2.AsSingle();
                Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 1 * 8)) = ymm3.AsSingle();
            }
            for (; i < length; i++)
            {
                var v = Unsafe.Add(ref rsi, i) << 15;
                v |= 0x4000_0000;
                var h = Vector128.CreateScalarUnsafe(v).AsSingle();
                Unsafe.Add(ref rdi, i) = Sse.AddScalar(h, ymm1.GetLower()).GetElement(0);
            }
        }
#endif
        #endregion
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ProcessStandard(Span<byte> wrote, Span<float> dest)
        {
            Vector<float> mul = new(Multiplier);
            Vector<sbyte> sign = new(sbyte.MinValue);
            ref var rdi = ref MemoryMarshal.GetReference(dest);
            ref var rsi = ref MemoryMarshal.GetReference(wrote);
            nint i = 0, length = MathI.Min(dest.Length, wrote.Length);
            var sizeF = Vector<float>.Count;
            var sizeI = Vector<byte>.Count;
            var olen = length - sizeF * 8 + 1;
            for (; i < olen; i += sizeF * 8)
            {
                var v3_nb = Vector.Xor(sign, Unsafe.As<byte, Vector<sbyte>>(ref Unsafe.Add(ref rsi, i + sizeI * 0)));
                Vector.Widen(v3_nb, out var v1_nh, out var v3_nh);
                Vector.Widen(v1_nh, out var v0_nd, out var v1_nd);
                Vector.Widen(v3_nh, out var v2_nd, out var v3_nd);
                var v0_ns = Vector.ConvertToSingle(v0_nd);
                var v1_ns = Vector.ConvertToSingle(v1_nd);
                var v2_ns = Vector.ConvertToSingle(v2_nd);
                var v3_ns = Vector.ConvertToSingle(v3_nd);
                v0_ns *= mul;
                v1_ns *= mul;
                v2_ns *= mul;
                v3_ns *= mul;
                Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rdi, i + sizeF * 0)) = v0_ns;
                Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rdi, i + sizeF * 1)) = v1_ns;
                Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rdi, i + sizeF * 2)) = v2_ns;
                Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rdi, i + sizeF * 3)) = v3_ns;
                v3_nb = Vector.Xor(sign, Unsafe.As<byte, Vector<sbyte>>(ref Unsafe.Add(ref rsi, i + sizeI * 1)));
                Vector.Widen(v3_nb, out v1_nh, out v3_nh);
                Vector.Widen(v1_nh, out v0_nd, out v1_nd);
                Vector.Widen(v3_nh, out v2_nd, out v3_nd);
                v0_ns = Vector.ConvertToSingle(v0_nd);
                v1_ns = Vector.ConvertToSingle(v1_nd);
                v2_ns = Vector.ConvertToSingle(v2_nd);
                v3_ns = Vector.ConvertToSingle(v3_nd);
                v0_ns *= mul;
                v1_ns *= mul;
                v2_ns *= mul;
                v3_ns *= mul;
                Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rdi, i + sizeF * 4)) = v0_ns;
                Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rdi, i + sizeF * 5)) = v1_ns;
                Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rdi, i + sizeF * 6)) = v2_ns;
                Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rdi, i + sizeF * 7)) = v3_ns;
            }
            for (; i < length; i++)
            {
                Unsafe.Add(ref rdi, i) = (Unsafe.Add(ref rsi, i) - 128) * mul[0];
            }
        }

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
