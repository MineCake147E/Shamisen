using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;


#if NETCOREAPP3_1_OR_GREATER
using System.Runtime.Intrinsics.X86;
using System.Runtime.Intrinsics;
using System.Security.Cryptography;
#endif
#if NET5_0_OR_GREATER
using System.Runtime.Intrinsics.Arm;
#endif

using Shamisen.Utils;

namespace Shamisen.Conversion.WaveToSampleConverters
{
    /// <summary>
    /// Converts 8-bit A-Law PCM to Sample.
    /// </summary>
    public sealed class ALawToSampleConverter : WaveToSampleConverterBase
    {
        private ResizableBufferWrapper<byte> bufferWrapper;
        private const float Multiplier = 1 / 32768.0f;

        /// <summary>
        /// Initializes a new instance of the <see cref="ALawToSampleConverter"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        public ALawToSampleConverter(IReadableAudioSource<byte, IWaveFormat> source)
            : base(source, new SampleFormat(source.Format.Channels, source.Format.SampleRate))
        {
            bufferWrapper = new ResizablePooledBufferWrapper<byte>(1);
        }

        /// <summary>
        /// Gets the bytes consumed per sample.
        /// </summary>
        /// <value>
        /// The bytes consumed per sample.
        /// </value>
        protected override int BytesPerSample => 1;

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
        public override ReadResult Read(Span<float> buffer)
        {
            int internalBufferLengthRequired = buffer.Length;
            var bytebuf = MemoryMarshal.Cast<float, byte>(buffer);
            var srcBuffer = bytebuf.Slice(bytebuf.Length - internalBufferLengthRequired, internalBufferLengthRequired);
            var readBuffer = srcBuffer.SliceAlign(Format.Channels);
            var rr = Source.Read(readBuffer);
            if (rr.HasData)
            {
                unchecked
                {
                    var rb = readBuffer.SliceWhile(rr.Length);
                    var wb = buffer.SliceWhile(rb.Length);
#if NETCOREAPP3_1_OR_GREATER
                    if (rb.Length >= 16 && Avx2.IsSupported)
                    {
                        ProcessAvx2M2(rb, wb);
                        return wb.Length;
                    }
#endif
                    ProcessStandard(rb, wb);
                    return wb.Length;
                }
            }
            else
            {
                return rr;
            }
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ProcessStandard(Span<byte> rb, Span<float> wb)
        {
            unchecked
            {
                if (rb.Length > wb.Length) return;
                for (int i = 0; i < rb.Length; i++)
                {
                    byte v = rb[i];
                    wb[i] = ConvertALawToSingle(v);
                }
            }
        }
#if NETCOREAPP3_1_OR_GREATER

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        [Obsolete("Remaining for benchmark purposes!")]
        internal static void ProcessAvx2(Span<byte> rb, Span<float> wb)
        {
            unchecked
            {
                nint length = rb.Length;
                nint i;
                ref byte rsi = ref MemoryMarshal.GetReference(rb);
                ref float rdi = ref MemoryMarshal.GetReference(wb);
                var xmm9 = Vector128.Create((byte)0xd5).AsUInt32();
                var ymm1 = Vector256.Create(1u);
                var xmm2 = Vector128.Create(0x7070_7070u);
                var xmm3 = Vector128.Create(0u);
                var ymm4 = Vector256.Create(0x1fu);
                var ymm5 = Vector256.Create(0xffu);
                var ymm6 = Vector256.Create(0x84u);
                var ymm7 = Vector256.Create(0x3b80_0000u);
                var ymm8 = Vector256.Create(0x83fc_0000u);
                for (i = 0; i < length - 15; i += 16)
                {
                    var xmm0 = Vector128.CreateScalarUnsafe(Unsafe.As<byte, ulong>(ref Unsafe.Add(ref rsi, i))).AsUInt32();
                    xmm0 = Sse2.Xor(xmm0, xmm9);
                    var ymm10 = Avx2.ConvertToVector256Int32(xmm0.AsSByte()).AsUInt32();
                    ymm10 = Avx2.Add(ymm10, ymm10);
                    ymm10 = Avx2.Or(ymm10, ymm1);
                    xmm0 = Sse2.And(xmm0, xmm2);
                    xmm0 = Sse2.CompareEqual(xmm0.AsByte(), xmm3.AsByte()).AsUInt32();
                    var ymm0 = Avx2.ConvertToVector256Int32(xmm0.AsSByte()).AsUInt32();
                    var ymm11 = Avx2.And(ymm10, ymm4);
                    ymm11 = Avx.ConvertToVector256Single(ymm11.AsInt32()).AsUInt32();
                    ymm11 = Avx2.ShiftRightLogical(ymm11, 23);
                    ymm11 = Avx2.And(ymm11, ymm5);
                    ymm11 = Avx2.Subtract(ymm6, ymm11);
                    var ymm12 = Avx2.ShiftLeftLogical(ymm11, 23);
                    ymm12 = Avx2.Subtract(ymm7, ymm12);
                    ymm11 = Avx2.And(ymm11, ymm0);
                    ymm10 = Avx2.ShiftLeftLogicalVariable(ymm10, ymm11);
                    ymm0 = Avx.BlendVariable(ymm7.AsSingle(), ymm12.AsSingle(), ymm0.AsSingle()).AsUInt32();
                    ymm10 = Avx2.ShiftLeftLogical(ymm10, 18);
                    ymm10 = Avx2.And(ymm10, ymm8);
                    ymm0 = Avx2.Add(ymm10, ymm0);
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i)) = ymm0.AsSingle();

                    xmm0 = Vector128.CreateScalarUnsafe(Unsafe.As<byte, ulong>(ref Unsafe.Add(ref rsi, i + 8))).AsUInt32();
                    xmm0 = Sse2.Xor(xmm0, xmm9);
                    ymm10 = Avx2.ConvertToVector256Int32(xmm0.AsSByte()).AsUInt32();
                    ymm10 = Avx2.Add(ymm10, ymm10);
                    ymm10 = Avx2.Or(ymm10, ymm1);
                    xmm0 = Sse2.And(xmm0, xmm2);
                    xmm0 = Sse2.CompareEqual(xmm0.AsByte(), xmm3.AsByte()).AsUInt32();
                    ymm0 = Avx2.ConvertToVector256Int32(xmm0.AsSByte()).AsUInt32();
                    ymm11 = Avx2.And(ymm10, ymm4);
                    ymm11 = Avx.ConvertToVector256Single(ymm11.AsInt32()).AsUInt32();
                    ymm11 = Avx2.ShiftRightLogical(ymm11, 23);
                    ymm11 = Avx2.And(ymm11, ymm5);
                    ymm11 = Avx2.Subtract(ymm6, ymm11);
                    ymm12 = Avx2.ShiftLeftLogical(ymm11, 23);
                    ymm12 = Avx2.Subtract(ymm7, ymm12);
                    ymm11 = Avx2.And(ymm11, ymm0);
                    ymm10 = Avx2.ShiftLeftLogicalVariable(ymm10, ymm11);
                    ymm0 = Avx.BlendVariable(ymm7.AsSingle(), ymm12.AsSingle(), ymm0.AsSingle()).AsUInt32();
                    ymm10 = Avx2.ShiftLeftLogical(ymm10, 18);
                    ymm10 = Avx2.And(ymm10, ymm8);
                    ymm0 = Avx2.Add(ymm10, ymm0);
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 8)) = ymm0.AsSingle();

                }
                for (; i < length - 7; i += 8)
                {
                    var xmm0 = Vector128.CreateScalarUnsafe(Unsafe.As<byte, ulong>(ref Unsafe.Add(ref rsi, i))).AsUInt32();
                    xmm0 = Sse2.Xor(xmm0, xmm9);
                    var ymm10 = Avx2.ConvertToVector256Int32(xmm0.AsSByte()).AsUInt32();
                    ymm10 = Avx2.Add(ymm10, ymm10);
                    ymm10 = Avx2.Or(ymm10, ymm1);
                    xmm0 = Sse2.And(xmm0, xmm2);
                    xmm0 = Sse2.CompareEqual(xmm0.AsByte(), xmm3.AsByte()).AsUInt32();
                    var ymm0 = Avx2.ConvertToVector256Int32(xmm0.AsSByte()).AsUInt32();
                    var ymm11 = Avx2.And(ymm10, ymm4);
                    ymm11 = Avx.ConvertToVector256Single(ymm11.AsInt32()).AsUInt32();
                    ymm11 = Avx2.ShiftRightLogical(ymm11, 23);
                    ymm11 = Avx2.And(ymm11, ymm5);
                    ymm11 = Avx2.Subtract(ymm6, ymm11);
                    var ymm12 = Avx2.ShiftLeftLogical(ymm11, 23);
                    ymm12 = Avx2.Subtract(ymm7, ymm12);
                    ymm11 = Avx2.And(ymm11, ymm0);
                    ymm10 = Avx2.ShiftLeftLogicalVariable(ymm10, ymm11);
                    ymm0 = Avx.BlendVariable(ymm7.AsSingle(), ymm12.AsSingle(), ymm0.AsSingle()).AsUInt32();
                    ymm10 = Avx2.ShiftLeftLogical(ymm10, 18);
                    ymm10 = Avx2.And(ymm10, ymm8);
                    ymm0 = Avx2.Add(ymm10, ymm0);
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i)) = ymm0.AsSingle();
                }
                for (; i < length; i++)
                {
                    byte g = Unsafe.Add(ref rsi, i);
                    Unsafe.Add(ref rdi, i) = ConvertALawToSingle(g);
                }
            }
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ProcessAvx2M2(Span<byte> rb, Span<float> wb)
        {
            unchecked
            {
                nint length = rb.Length;
                nint i;
                ref byte rsi = ref MemoryMarshal.GetReference(rb);
                ref float rdi = ref MemoryMarshal.GetReference(wb);
                var xmm9 = Vector128.Create((byte)0xd5).AsUInt32();
                var ymm1 = Vector256.Create(1u);
                var xmm2 = Vector128.Create(0x7070_7070u);
                var xmm3 = Vector128.Create(0u);
                var ymm4 = Vector256.Create(0x0fu);
                var ymm7 = Vector256.Create(0x3b80_0000u);
                var ymm8 = Vector256.Create(0x83fc_0000u);
                var ymm15 = Vector256.Create(5, 4, 3, 3, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 1, 5, 4, 3, 3, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 1);
                for (i = 0; i < length - 15; i += 16)
                {
                    var xmm0 = Vector128.CreateScalarUnsafe(Unsafe.As<byte, ulong>(ref Unsafe.Add(ref rsi, i))).AsUInt32();
                    xmm0 = Sse2.Xor(xmm0, xmm9);
                    var ymm10 = Avx2.ConvertToVector256Int32(xmm0.AsSByte()).AsUInt32();
                    var ymm11 = ymm10;
                    ymm10 = Avx2.Add(ymm10, ymm10);
                    ymm10 = Avx2.Or(ymm10, ymm1);
                    xmm0 = Sse2.And(xmm0, xmm2);
                    xmm0 = Sse2.CompareEqual(xmm0.AsByte(), xmm3.AsByte()).AsUInt32();
                    var ymm0 = Avx2.ConvertToVector256Int32(xmm0.AsSByte()).AsUInt32();
                    ymm11 = Avx2.And(ymm11, ymm4);
                    ymm11 = Avx2.Shuffle(ymm15.AsByte(), ymm11.AsByte()).AsUInt32();    //lzcnt and add in one go
                    ymm11 = Avx2.And(ymm11, ymm4);
                    var ymm12 = Avx2.ShiftLeftLogical(ymm11, 23);
                    ymm12 = Avx2.Subtract(ymm7, ymm12);
                    ymm11 = Avx2.And(ymm11, ymm0);
                    ymm10 = Avx2.ShiftLeftLogicalVariable(ymm10, ymm11);
                    ymm0 = Avx.BlendVariable(ymm7.AsSingle(), ymm12.AsSingle(), ymm0.AsSingle()).AsUInt32();
                    ymm10 = Avx2.ShiftLeftLogical(ymm10, 18);
                    ymm10 = Avx2.And(ymm10, ymm8);
                    ymm0 = Avx2.Add(ymm10, ymm0);
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i)) = ymm0.AsSingle();

                    xmm0 = Vector128.CreateScalarUnsafe(Unsafe.As<byte, ulong>(ref Unsafe.Add(ref rsi, i + 8))).AsUInt32();
                    xmm0 = Sse2.Xor(xmm0, xmm9);
                    ymm10 = Avx2.ConvertToVector256Int32(xmm0.AsSByte()).AsUInt32();
                    ymm11 = ymm10;
                    ymm10 = Avx2.Add(ymm10, ymm10);
                    ymm10 = Avx2.Or(ymm10, ymm1);
                    xmm0 = Sse2.And(xmm0, xmm2);
                    xmm0 = Sse2.CompareEqual(xmm0.AsByte(), xmm3.AsByte()).AsUInt32();
                    ymm0 = Avx2.ConvertToVector256Int32(xmm0.AsSByte()).AsUInt32();
                    ymm11 = Avx2.And(ymm11, ymm4);
                    ymm11 = Avx2.Shuffle(ymm15.AsByte(), ymm11.AsByte()).AsUInt32();    //lzcnt and add in one go
                    ymm11 = Avx2.And(ymm11, ymm4);
                    ymm12 = Avx2.ShiftLeftLogical(ymm11, 23);
                    ymm12 = Avx2.Subtract(ymm7, ymm12);
                    ymm11 = Avx2.And(ymm11, ymm0);
                    ymm10 = Avx2.ShiftLeftLogicalVariable(ymm10, ymm11);
                    ymm0 = Avx.BlendVariable(ymm7.AsSingle(), ymm12.AsSingle(), ymm0.AsSingle()).AsUInt32();
                    ymm10 = Avx2.ShiftLeftLogical(ymm10, 18);
                    ymm10 = Avx2.And(ymm10, ymm8);
                    ymm0 = Avx2.Add(ymm10, ymm0);
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 8)) = ymm0.AsSingle();
                }
                for (; i < length - 7; i += 8)
                {
                    var xmm0 = Vector128.CreateScalarUnsafe(Unsafe.As<byte, ulong>(ref Unsafe.Add(ref rsi, i))).AsUInt32();
                    xmm0 = Sse2.Xor(xmm0, xmm9);
                    var ymm10 = Avx2.ConvertToVector256Int32(xmm0.AsSByte()).AsUInt32();
                    var ymm11 = ymm10;
                    ymm10 = Avx2.Add(ymm10, ymm10);
                    ymm10 = Avx2.Or(ymm10, ymm1);
                    xmm0 = Sse2.And(xmm0, xmm2);
                    xmm0 = Sse2.CompareEqual(xmm0.AsByte(), xmm3.AsByte()).AsUInt32();
                    var ymm0 = Avx2.ConvertToVector256Int32(xmm0.AsSByte()).AsUInt32();
                    ymm11 = Avx2.And(ymm11, ymm4);
                    ymm11 = Avx2.Shuffle(ymm15.AsByte(), ymm11.AsByte()).AsUInt32();    //lzcnt and add in one go with lookup table
                    ymm11 = Avx2.And(ymm11, ymm4);
                    var ymm12 = Avx2.ShiftLeftLogical(ymm11, 23);
                    ymm12 = Avx2.Subtract(ymm7, ymm12);
                    ymm11 = Avx2.And(ymm11, ymm0);
                    ymm10 = Avx2.ShiftLeftLogicalVariable(ymm10, ymm11);
                    ymm0 = Avx.BlendVariable(ymm7.AsSingle(), ymm12.AsSingle(), ymm0.AsSingle()).AsUInt32();
                    ymm10 = Avx2.ShiftLeftLogical(ymm10, 18);
                    ymm10 = Avx2.And(ymm10, ymm8);
                    ymm0 = Avx2.Add(ymm10, ymm0);
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i)) = ymm0.AsSingle();
                }
                for (; i < length; i++)
                {
                    byte g = Unsafe.Add(ref rsi, i);
                    Unsafe.Add(ref rdi, i) = ConvertALawToSingle(g);
                }
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ProcessAvx2FP(Span<byte> rb, Span<float> wb)
        {
            unchecked
            {
                nint length = rb.Length;
                nint i;
                ref byte rsi = ref MemoryMarshal.GetReference(rb);
                ref float rdi = ref MemoryMarshal.GetReference(wb);
                var xmm0 = Vector128.CreateScalarUnsafe(0xd5_d5_d5_d5_d5_d5_d5_d5).AsUInt32();
                var ymm1 = Vector256.Create(0x83F80000u).AsSingle();
                var ymm2 = Vector256.Create(262144).AsSingle();
                var ymm3 = Vector256.Create(6.64613997E+35f);
                for (i = 0; i < length - 31; i += 32)
                {
                    var xmm4 = Vector128.CreateScalarUnsafe(Unsafe.As<byte, ulong>(ref Unsafe.Add(ref rsi, i))).AsUInt32();
                    var xmm5 = Vector128.CreateScalarUnsafe(Unsafe.As<byte, ulong>(ref Unsafe.Add(ref rsi, i + 8))).AsUInt32();
                    var xmm6 = Vector128.CreateScalarUnsafe(Unsafe.As<byte, ulong>(ref Unsafe.Add(ref rsi, i + 16))).AsUInt32();
                    var xmm7 = Vector128.CreateScalarUnsafe(Unsafe.As<byte, ulong>(ref Unsafe.Add(ref rsi, i + 24))).AsUInt32();
                    xmm4 = Sse2.Xor(xmm4, xmm0);
                    xmm5 = Sse2.Xor(xmm5, xmm0);
                    xmm6 = Sse2.Xor(xmm6, xmm0);
                    xmm7 = Sse2.Xor(xmm7, xmm0);
                    var ymm4 = Avx2.ConvertToVector256Int32(xmm4.AsSByte()).AsSingle();
                    var ymm5 = Avx2.ConvertToVector256Int32(xmm5.AsSByte()).AsSingle();
                    var ymm6 = Avx2.ConvertToVector256Int32(xmm6.AsSByte()).AsSingle();
                    var ymm7 = Avx2.ConvertToVector256Int32(xmm7.AsSByte()).AsSingle();
                    ymm4 = Avx2.ShiftLeftLogical(ymm4.AsInt32(), 19).AsSingle();
                    ymm5 = Avx2.ShiftLeftLogical(ymm5.AsInt32(), 19).AsSingle();
                    ymm6 = Avx2.ShiftLeftLogical(ymm6.AsInt32(), 19).AsSingle();
                    ymm7 = Avx2.ShiftLeftLogical(ymm7.AsInt32(), 19).AsSingle();
                    ymm4 = Avx.And(ymm4, ymm1);
                    ymm5 = Avx.And(ymm5, ymm1);
                    ymm6 = Avx.And(ymm6, ymm1);
                    ymm7 = Avx.And(ymm7, ymm1);
                    ymm4 = Avx.Or(ymm4, ymm2);
                    ymm5 = Avx.Or(ymm5, ymm2);
                    ymm6 = Avx.Or(ymm6, ymm2);
                    ymm7 = Avx.Or(ymm7, ymm2);
                    ymm4 = Avx.Multiply(ymm4, ymm3);
                    ymm5 = Avx.Multiply(ymm5, ymm3);
                    ymm6 = Avx.Multiply(ymm6, ymm3);
                    ymm7 = Avx.Multiply(ymm7, ymm3);
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 0)) = ymm4;
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 8)) = ymm5;
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 16)) = ymm6;
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 24)) = ymm7;
                }
                for (; i < length; i++)
                {
                    byte g = Unsafe.Add(ref rsi, i);
                    Unsafe.Add(ref rdi, i) = ConvertALawToSingle(g);
                }
            }
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ProcessAvx2M3(Span<byte> rb, Span<float> wb)
        {
            unchecked
            {
                nint length = rb.Length;
                nint i;
                ref byte rsi = ref MemoryMarshal.GetReference(rb);
                ref float rdi = ref MemoryMarshal.GetReference(wb);
                var xmm0 = Vector128.Create((byte)0xd5).AsUInt32();
                var ymm1 = Vector256.Create(1u);
                var xmm2 = Vector128.Create(0x7070_7070u);
                var xmm3 = Vector128.Create(0u);
                var ymm4 = Vector256.Create(0x0fu);
                var ymm5 = Vector256.Create(0x3b80_0000u);
                var ymm6 = Vector256.Create(0x83fc_0000u);
                var ymm7 = Vector256.Create(5, 4, 3, 3, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 1, 5, 4, 3, 3, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 1);
                for (i = 0; i < length - 31; i += 32)
                {
                    var xmm8 = Vector128.CreateScalarUnsafe(Unsafe.As<byte, ulong>(ref Unsafe.Add(ref rsi, i))).AsUInt32();
                    var xmm9 = Vector128.CreateScalarUnsafe(Unsafe.As<byte, ulong>(ref Unsafe.Add(ref rsi, i + 8))).AsUInt32();
                    xmm8 = Sse2.Xor(xmm8, xmm0);
                    xmm9 = Sse2.Xor(xmm9, xmm0);
                    var ymm10 = Avx2.ConvertToVector256Int32(xmm8.AsSByte()).AsUInt32();
                    var ymm11 = Avx2.ConvertToVector256Int32(xmm9.AsSByte()).AsUInt32();
                    var ymm12 = ymm10;
                    var ymm13 = ymm11;
                    ymm10 = Avx2.Add(ymm10, ymm10);
                    ymm11 = Avx2.Add(ymm11, ymm11);
                    ymm10 = Avx2.Or(ymm10, ymm1);
                    ymm11 = Avx2.Or(ymm11, ymm1);
                    xmm8 = Sse2.And(xmm8, xmm2);
                    xmm9 = Sse2.And(xmm9, xmm2);
                    xmm8 = Sse2.CompareEqual(xmm8.AsByte(), xmm3.AsByte()).AsUInt32();
                    xmm9 = Sse2.CompareEqual(xmm9.AsByte(), xmm3.AsByte()).AsUInt32();
                    var ymm8 = Avx2.ConvertToVector256Int32(xmm8.AsSByte()).AsUInt32();
                    var ymm9 = Avx2.ConvertToVector256Int32(xmm9.AsSByte()).AsUInt32();
                    ymm12 = Avx2.And(ymm12, ymm4);
                    ymm13 = Avx2.And(ymm13, ymm4);
                    ymm12 = Avx2.Shuffle(ymm7.AsByte(), ymm12.AsByte()).AsUInt32();
                    ymm13 = Avx2.Shuffle(ymm7.AsByte(), ymm13.AsByte()).AsUInt32();
                    ymm12 = Avx2.And(ymm12, ymm4);
                    ymm13 = Avx2.And(ymm13, ymm4);
                    var ymm14 = Avx2.ShiftLeftLogical(ymm12, 23);
                    var ymm15 = Avx2.ShiftLeftLogical(ymm13, 23);
                    ymm14 = Avx2.Subtract(ymm5, ymm14);
                    ymm15 = Avx2.Subtract(ymm5, ymm15);
                    ymm12 = Avx2.And(ymm12, ymm8);
                    ymm13 = Avx2.And(ymm13, ymm9);
                    ymm10 = Avx2.ShiftLeftLogicalVariable(ymm10, ymm12);
                    ymm11 = Avx2.ShiftLeftLogicalVariable(ymm11, ymm13);
                    ymm8 = Avx.BlendVariable(ymm5.AsSingle(), ymm14.AsSingle(), ymm8.AsSingle()).AsUInt32();
                    ymm9 = Avx.BlendVariable(ymm5.AsSingle(), ymm15.AsSingle(), ymm9.AsSingle()).AsUInt32();
                    ymm10 = Avx2.ShiftLeftLogical(ymm10, 18);
                    ymm11 = Avx2.ShiftLeftLogical(ymm11, 18);
                    ymm10 = Avx2.And(ymm10, ymm6);
                    ymm11 = Avx2.And(ymm11, ymm6);
                    ymm8 = Avx2.Add(ymm10, ymm8);
                    ymm9 = Avx2.Add(ymm11, ymm9);
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i)) = ymm8.AsSingle();
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 8)) = ymm9.AsSingle();

                    xmm8 = Vector128.CreateScalarUnsafe(Unsafe.As<byte, ulong>(ref Unsafe.Add(ref rsi, i + 16))).AsUInt32();
                    xmm9 = Vector128.CreateScalarUnsafe(Unsafe.As<byte, ulong>(ref Unsafe.Add(ref rsi, i + 24))).AsUInt32();
                    xmm8 = Sse2.Xor(xmm8, xmm0);
                    xmm9 = Sse2.Xor(xmm9, xmm0);
                    ymm10 = Avx2.ConvertToVector256Int32(xmm8.AsSByte()).AsUInt32();
                    ymm11 = Avx2.ConvertToVector256Int32(xmm9.AsSByte()).AsUInt32();
                    ymm12 = ymm10;
                    ymm13 = ymm11;
                    ymm10 = Avx2.Add(ymm10, ymm10);
                    ymm11 = Avx2.Add(ymm11, ymm11);
                    ymm10 = Avx2.Or(ymm10, ymm1);
                    ymm11 = Avx2.Or(ymm11, ymm1);
                    xmm8 = Sse2.And(xmm8, xmm2);
                    xmm9 = Sse2.And(xmm9, xmm2);
                    xmm8 = Sse2.CompareEqual(xmm8.AsByte(), xmm3.AsByte()).AsUInt32();
                    xmm9 = Sse2.CompareEqual(xmm9.AsByte(), xmm3.AsByte()).AsUInt32();
                    ymm8 = Avx2.ConvertToVector256Int32(xmm8.AsSByte()).AsUInt32();
                    ymm9 = Avx2.ConvertToVector256Int32(xmm9.AsSByte()).AsUInt32();
                    ymm12 = Avx2.And(ymm12, ymm4);
                    ymm13 = Avx2.And(ymm13, ymm4);
                    ymm12 = Avx2.Shuffle(ymm7.AsByte(), ymm12.AsByte()).AsUInt32();
                    ymm13 = Avx2.Shuffle(ymm7.AsByte(), ymm13.AsByte()).AsUInt32();
                    ymm12 = Avx2.And(ymm12, ymm4);
                    ymm13 = Avx2.And(ymm13, ymm4);
                    ymm14 = Avx2.ShiftLeftLogical(ymm12, 23);
                    ymm15 = Avx2.ShiftLeftLogical(ymm13, 23);
                    ymm14 = Avx2.Subtract(ymm5, ymm14);
                    ymm15 = Avx2.Subtract(ymm5, ymm15);
                    ymm12 = Avx2.And(ymm12, ymm8);
                    ymm13 = Avx2.And(ymm13, ymm9);
                    ymm10 = Avx2.ShiftLeftLogicalVariable(ymm10, ymm12);
                    ymm11 = Avx2.ShiftLeftLogicalVariable(ymm11, ymm13);
                    ymm8 = Avx.BlendVariable(ymm5.AsSingle(), ymm14.AsSingle(), ymm8.AsSingle()).AsUInt32();
                    ymm9 = Avx.BlendVariable(ymm5.AsSingle(), ymm15.AsSingle(), ymm9.AsSingle()).AsUInt32();
                    ymm10 = Avx2.ShiftLeftLogical(ymm10, 18);
                    ymm11 = Avx2.ShiftLeftLogical(ymm11, 18);
                    ymm10 = Avx2.And(ymm10, ymm6);
                    ymm11 = Avx2.And(ymm11, ymm6);
                    ymm8 = Avx2.Add(ymm10, ymm8);
                    ymm9 = Avx2.Add(ymm11, ymm9);
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 16)) = ymm8.AsSingle();
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 24)) = ymm9.AsSingle();
                }
                for (; i < length - 7; i += 8)
                {
                    var xmm8 = Vector128.CreateScalarUnsafe(Unsafe.As<byte, ulong>(ref Unsafe.Add(ref rsi, i))).AsUInt32();
                    xmm8 = Sse2.Xor(xmm8, xmm0);
                    var ymm9 = Avx2.ConvertToVector256Int32(xmm8.AsSByte()).AsUInt32();
                    var ymm10 = ymm9;
                    ymm9 = Avx2.Add(ymm9, ymm9);
                    ymm9 = Avx2.Or(ymm9, ymm1);
                    xmm8 = Sse2.And(xmm8, xmm2);
                    xmm8 = Sse2.CompareEqual(xmm8.AsByte(), xmm3.AsByte()).AsUInt32();
                    var ymm8 = Avx2.ConvertToVector256Int32(xmm8.AsSByte()).AsUInt32();
                    ymm10 = Avx2.And(ymm10, ymm4);
                    ymm10 = Avx2.Shuffle(ymm7.AsByte(), ymm10.AsByte()).AsUInt32();
                    ymm10 = Avx2.And(ymm10, ymm4);
                    var ymm11 = Avx2.ShiftLeftLogical(ymm10, 23);
                    ymm11 = Avx2.Subtract(ymm5, ymm11);
                    ymm10 = Avx2.And(ymm10, ymm8);
                    ymm9 = Avx2.ShiftLeftLogicalVariable(ymm9, ymm10);
                    ymm8 = Avx.BlendVariable(ymm5.AsSingle(), ymm11.AsSingle(), ymm8.AsSingle()).AsUInt32();
                    ymm9 = Avx2.ShiftLeftLogical(ymm9, 18);
                    ymm9 = Avx2.And(ymm9, ymm6);
                    ymm8 = Avx2.Add(ymm9, ymm8);
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i)) = ymm8.AsSingle();
                }
                for (; i < length; i++)
                {
                    byte g = Unsafe.Add(ref rsi, i);
                    Unsafe.Add(ref rdi, i) = ConvertALawToSingle(g);
                }
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ProcessSse41(Span<byte> rb, Span<float> wb)
        {
            unchecked
            {
                nint length = rb.Length;
                nint i;
                ref byte rsi = ref MemoryMarshal.GetReference(rb);
                ref float rdi = ref MemoryMarshal.GetReference(wb);
                var xmm0 = Vector128.Create((byte)0xd5).AsUInt32();
                var xmm1 = Vector128.Create(1u);
                var xmm2 = Vector128.Create(0x0000_0070u);
                var xmm3 = Vector128.Create(0x3f80_0000u);
                var xmm4 = Vector128.Create(0x0fu);
                var xmm5 = Vector128.Create(0x3b80_0000u);
                var xmm6 = Vector128.Create(0x83fc_0000u);
                var xmm7 = Vector128.Create(5, 4, 3, 3, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 1);
                for (i = 0; i < length - 15; i += 16)
                {
                    var xmm8 = Vector128.CreateScalarUnsafe(Unsafe.As<byte, uint>(ref Unsafe.Add(ref rsi, i))).AsUInt32();
                    var xmm9 = Vector128.CreateScalarUnsafe(Unsafe.As<byte, uint>(ref Unsafe.Add(ref rsi, i + 4))).AsUInt32();
                    xmm8 = Sse2.Xor(xmm8, xmm0);
                    xmm9 = Sse2.Xor(xmm9, xmm0);
                    var xmm10 = Sse41.ConvertToVector128Int32(xmm8.AsSByte()).AsUInt32();
                    var xmm11 = Sse41.ConvertToVector128Int32(xmm9.AsSByte()).AsUInt32();
                    var xmm12 = xmm10;
                    var xmm13 = xmm11;
                    xmm8 = Sse2.And(xmm12, xmm2);
                    xmm9 = Sse2.And(xmm13, xmm2);
                    xmm12 = Sse2.And(xmm12, xmm4);
                    xmm13 = Sse2.And(xmm13, xmm4);
                    xmm8 = Sse2.CompareLessThan(xmm8.AsInt32(), xmm1.AsInt32()).AsUInt32();    //(a == 0) == (a < 1) if a is known to be unsigned
                    xmm9 = Sse2.CompareLessThan(xmm9.AsInt32(), xmm1.AsInt32()).AsUInt32();
                    xmm12 = Ssse3.Shuffle(xmm7.AsByte(), xmm12.AsByte()).AsUInt32();
                    xmm13 = Ssse3.Shuffle(xmm7.AsByte(), xmm13.AsByte()).AsUInt32();
                    xmm12 = Sse2.And(xmm12, xmm4);
                    xmm13 = Sse2.And(xmm13, xmm4);
                    xmm10 = Sse2.Add(xmm10, xmm10);
                    xmm11 = Sse2.Add(xmm11, xmm11);
                    xmm12 = Sse2.ShiftLeftLogical(xmm12, 23);
                    xmm13 = Sse2.ShiftLeftLogical(xmm13, 23);
                    var xmm14 = xmm12;
                    var xmm15 = xmm13;
                    xmm12 = Sse2.And(xmm12, xmm8);
                    xmm13 = Sse2.And(xmm13, xmm9);
                    xmm10 = Sse2.Or(xmm10, xmm1);
                    xmm11 = Sse2.Or(xmm11, xmm1);
                    xmm12 = Sse2.Add(xmm12, xmm3);
                    xmm13 = Sse2.Add(xmm13, xmm3);
                    xmm12 = Sse2.ConvertToVector128Int32(xmm12.AsSingle()).AsUInt32();
                    xmm13 = Sse2.ConvertToVector128Int32(xmm13.AsSingle()).AsUInt32();
                    xmm14 = Sse2.Subtract(xmm5, xmm14);
                    xmm15 = Sse2.Subtract(xmm5, xmm15);
                    xmm10 = Sse41.MultiplyLow(xmm10, xmm12);
                    xmm11 = Sse41.MultiplyLow(xmm11, xmm13);
                    xmm8 = Sse41.BlendVariable(xmm5.AsSingle(), xmm14.AsSingle(), xmm8.AsSingle()).AsUInt32();
                    xmm9 = Sse41.BlendVariable(xmm5.AsSingle(), xmm15.AsSingle(), xmm9.AsSingle()).AsUInt32();
                    xmm10 = Sse2.ShiftLeftLogical(xmm10, 18);
                    xmm11 = Sse2.ShiftLeftLogical(xmm11, 18);
                    xmm10 = Sse2.And(xmm10, xmm6);
                    xmm11 = Sse2.And(xmm11, xmm6);
                    xmm8 = Sse2.Add(xmm10, xmm8);
                    xmm9 = Sse2.Add(xmm11, xmm9);
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i)) = xmm8.AsSingle();
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 4)) = xmm9.AsSingle();

                    xmm8 = Vector128.CreateScalarUnsafe(Unsafe.As<byte, uint>(ref Unsafe.Add(ref rsi, i + 8))).AsUInt32();
                    xmm9 = Vector128.CreateScalarUnsafe(Unsafe.As<byte, uint>(ref Unsafe.Add(ref rsi, i + 12))).AsUInt32();
                    xmm8 = Sse2.Xor(xmm8, xmm0);
                    xmm9 = Sse2.Xor(xmm9, xmm0);
                    xmm10 = Sse41.ConvertToVector128Int32(xmm8.AsSByte()).AsUInt32();
                    xmm11 = Sse41.ConvertToVector128Int32(xmm9.AsSByte()).AsUInt32();
                    xmm12 = xmm10;
                    xmm13 = xmm11;
                    xmm8 = Sse2.And(xmm12, xmm2);
                    xmm9 = Sse2.And(xmm13, xmm2);
                    xmm12 = Sse2.And(xmm12, xmm4);
                    xmm13 = Sse2.And(xmm13, xmm4);
                    xmm8 = Sse2.CompareLessThan(xmm8.AsInt32(), xmm1.AsInt32()).AsUInt32();    //(a == 0) == (a < 1) if a is known to be unsigned
                    xmm9 = Sse2.CompareLessThan(xmm9.AsInt32(), xmm1.AsInt32()).AsUInt32();
                    xmm12 = Ssse3.Shuffle(xmm7.AsByte(), xmm12.AsByte()).AsUInt32();
                    xmm13 = Ssse3.Shuffle(xmm7.AsByte(), xmm13.AsByte()).AsUInt32();
                    xmm12 = Sse2.And(xmm12, xmm4);
                    xmm13 = Sse2.And(xmm13, xmm4);
                    xmm10 = Sse2.Add(xmm10, xmm10);
                    xmm11 = Sse2.Add(xmm11, xmm11);
                    xmm12 = Sse2.ShiftLeftLogical(xmm12, 23);
                    xmm13 = Sse2.ShiftLeftLogical(xmm13, 23);
                    xmm14 = xmm12;
                    xmm15 = xmm13;
                    xmm12 = Sse2.And(xmm12, xmm8);
                    xmm13 = Sse2.And(xmm13, xmm9);
                    xmm10 = Sse2.Or(xmm10, xmm1);
                    xmm11 = Sse2.Or(xmm11, xmm1);
                    xmm12 = Sse2.Add(xmm12, xmm3);
                    xmm13 = Sse2.Add(xmm13, xmm3);
                    xmm12 = Sse2.ConvertToVector128Int32(xmm12.AsSingle()).AsUInt32();
                    xmm13 = Sse2.ConvertToVector128Int32(xmm13.AsSingle()).AsUInt32();
                    xmm14 = Sse2.Subtract(xmm5, xmm14);
                    xmm15 = Sse2.Subtract(xmm5, xmm15);
                    xmm10 = Sse41.MultiplyLow(xmm10, xmm12);
                    xmm11 = Sse41.MultiplyLow(xmm11, xmm13);
                    xmm8 = Sse41.BlendVariable(xmm5.AsSingle(), xmm14.AsSingle(), xmm8.AsSingle()).AsUInt32();
                    xmm9 = Sse41.BlendVariable(xmm5.AsSingle(), xmm15.AsSingle(), xmm9.AsSingle()).AsUInt32();
                    xmm10 = Sse2.ShiftLeftLogical(xmm10, 18);
                    xmm11 = Sse2.ShiftLeftLogical(xmm11, 18);
                    xmm10 = Sse2.And(xmm10, xmm6);
                    xmm11 = Sse2.And(xmm11, xmm6);
                    xmm8 = Sse2.Add(xmm10, xmm8);
                    xmm9 = Sse2.Add(xmm11, xmm9);
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 8)) = xmm8.AsSingle();
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 12)) = xmm9.AsSingle();
                }
                for (; i < length; i++)
                {
                    byte g = Unsafe.Add(ref rsi, i);
                    Unsafe.Add(ref rdi, i) = ConvertALawToSingle(g);
                }
            }
        }
#endif
#if NET5_0_OR_GREATER
        internal static void ProcessAdvSimd64(Span<byte> rb, Span<float> wb)
        {
            unchecked
            {
                nint length = rb.Length;
                nint i;
                ref byte x11 = ref MemoryMarshal.GetReference(rb);
                ref float x10 = ref MemoryMarshal.GetReference(wb);
                var v0_4h = Vector64.Create((ushort)0xd5);
                var v1_4s = Vector128.Create(0x70u);
                var v2_4s = Vector128.Create(~0u >> 5);
                var v3_4s = Vector128.Create(0x3b80_0000);
                var v4_4s = Vector128.Create(0x83fc_0000u);
                for (i = 0; i < length - 3; i += 4)
                {
                    //ldr     s6, [x11], #4
                    var s6 = Vector64.CreateScalarUnsafe(Unsafe.As<byte, uint>(ref Unsafe.Add(ref x11, i)));
                    //ushll   v6.8h, v6.8b, #0
                    var v6_8h = AdvSimd.ShiftLeftLogicalWideningLower(s6.AsByte(), 0);
                    //eor v6.8b, v6.8b, v0.8b
                    v6_8h = AdvSimd.Xor(v6_8h.GetLower(), v0_4h).ToVector128Unsafe();
                    //ushll v6.4s, v6.4h, #0
                    var v6_4s = AdvSimd.ShiftLeftLogicalWideningLower(v6_8h.GetLower(), 0);
                    //shl     v7.4s, v6.4s, #24
                    var v7_4s = AdvSimd.ShiftLeftLogical(v6_4s, 24);
                    //sshr    v7.4s, v7.4s, #24
                    v7_4s = AdvSimd.ShiftRightArithmetic(v7_4s.AsInt32(), 24).AsUInt32();
                    //shl     v7.4s, v7.4s, #19
                    v7_4s = AdvSimd.ShiftLeftLogical(v7_4s, 19);
                    //orr     v7.4s, #4, lsl #16
                    v7_4s = AdvSimd.Or(v7_4s, Vector128.Create(4u << 16));
                    //and     v16.16b, v6.16b, v1.16b
                    var v16_4s = AdvSimd.And(v6_4s, v1_4s);
                    //cmeq    v16.4s, v16.4s, #0
                    v16_4s = AdvSimd.CompareEqual(v16_4s, Vector128<uint>.Zero);
                    //shl     v6.4s, v6.4s, #27
                    v6_4s = AdvSimd.ShiftLeftLogical(v6_4s, 27);
                    //orr     v6.16b, v6.16b, v2.16b
                    v6_4s = AdvSimd.Or(v6_4s, v2_4s);
                    //clz     v6.4s, v6.4s
                    v6_4s = AdvSimd.LeadingZeroCount(v6_4s);
                    //shl     v17.4s, v6.4s, #23
                    var v17_4s = AdvSimd.ShiftRightLogical(v6_4s, 23).AsInt32();
                    //sub     v17.4s, v3.4s, v17.4s
                    v17_4s = AdvSimd.Subtract(v3_4s, v17_4s);
                    //bif     v17.16b, v3.16b, v16.16b
                    v17_4s = AdvSimd.BitwiseSelect(v16_4s.AsInt32(), v17_4s, v3_4s);
                    //and     v6.16b, v6.16b, v16.16b
                    v6_4s = AdvSimd.And(v6_4s, v16_4s);
                    //ushl    v6.4s, v7.4s, v6.4s
                    v6_4s = AdvSimd.ShiftLogical(v7_4s, v6_4s.AsInt32());
                    //and     v6.16b, v6.16b, v4.16b
                    v6_4s = AdvSimd.And(v6_4s, v4_4s);
                    //add     v6.4s, v6.4s, v17.4s
                    v6_4s = AdvSimd.Add(v6_4s, v17_4s.AsUInt32());
                    //str     q6, [x10], #16
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref x10, i)) = v6_4s.AsSingle();
                }
                for (; i < length; i++)
                {
                    byte g = Unsafe.Add(ref x11, i);
                    Unsafe.Add(ref x10, i) = ConvertALawToSingle(g);
                }
            }
        }
#endif

        /// <summary>
        /// Converts A-law value to <see cref="float"/> value.<br/>
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static float ConvertALawToSingle(byte value)
        {
            unchecked
            {
                value ^= 0b1101_0101;
                uint v = ((uint)(sbyte)value << 1) | 1;
                uint m = 0x3b80_0000u;
                if ((value & 0x70) == 0)
                {
                    uint zs = (uint)MathI.LeadingZeroCount(v << 26);
                    v <<= (int)zs;
                    m -= zs << 23;
                }
                v <<= 18;
                v &= 0x83fc_0000;
                v += m;
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
                return BitConverter.Int32BitsToSingle((int)v);
#else
                return Unsafe.As<uint, float>(ref v);
#endif
            }
        }
        internal ReadResult ReadOld(Span<float> buffer)
        {
            buffer = buffer.SliceAlign(2);
            int internalBufferLengthRequired = buffer.Length;
            var bytebuf = MemoryMarshal.Cast<float, byte>(buffer);
            var srcBuffer = bytebuf.Slice(bytebuf.Length - internalBufferLengthRequired);
            var readBuffer = srcBuffer.SliceAlign(Format.Channels);
            var rr = Source.Read(readBuffer);
            if (rr.HasData)
            {
                unchecked
                {
                    var rb = readBuffer.SliceWhile(rr.Length);
                    var wb = buffer.SliceWhile(rb.Length);
                    for (int i = 0; i < rb.Length; i++)
                    {
                        byte v = rb[i];
                        short h = ConvertALawToInt16(v);
                        wb[i] = h * Multiplier;
                    }
                    return wb.Length;
                }
            }
            else
            {
                return rr;
            }
        }

        /// <summary>
        /// Converts A-law value to <see cref="short"/> value.<br/>
        /// Remains only for test uses.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        [Obsolete($"Use {nameof(ConvertALawToSingle)} instead!")]
        internal static short ConvertALawToInt16(byte value)
        {
            unchecked
            {
                byte v = value;
                bool s = v <= 127;
                uint ss = Unsafe.As<bool, byte>(ref s);
                v ^= (byte)0b01010101u;
                uint x = (uint)v & 0x7f;
                bool j = x > 0xf;
                uint a = Unsafe.As<bool, byte>(ref j);
                int exp = (int)(x >> 4);
                uint man = x & 0b1111;
                man += a << 4;
                man = (man << 4) + 8;
                man <<= exp - (int)a;
                return (short)((man ^ (uint)-(int)ss) + ss);
            }
        }



        private void ExpandBuffer(int internalBufferLengthRequired) => bufferWrapper.Resize(internalBufferLengthRequired);

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
                    bufferWrapper.Dispose();
                }
                //bufferWrapper = null;
            }
            disposedValue = true;
        }
    }
}
