using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;


#if NETCOREAPP3_1_OR_GREATER
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

using Shamisen.Optimization;
using Shamisen.Utils.Intrinsics;

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
    public sealed class MuLawToSampleConverter : WaveToSampleConverterBase
    {
        private ResizableBufferWrapper<byte> bufferWrapper;
        private const float Multiplier = 1 / 32768.0f;

        /// <summary>
        /// Initializes a new instance of the <see cref="MuLawToSampleConverter"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        public MuLawToSampleConverter(IReadableAudioSource<byte, IWaveFormat> source)
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
            int internalBufferLengthRequired = CheckBuffer(buffer.Length);
            var bytebuf = MemoryMarshal.Cast<float, byte>(buffer);
            //Resampling start
            var srcBuffer = bytebuf.Slice(bytebuf.Length - internalBufferLengthRequired, internalBufferLengthRequired);
            var readBuffer = srcBuffer.SliceAlign(Format.Channels);
            var rr = Source.Read(readBuffer);
            if (rr.HasData)
            {
                unchecked
                {
                    var rb = readBuffer.SliceWhile(rr.Length);
                    var wb = buffer.SliceWhile(rb.Length);
                    Process(rb, wb);
                    return wb.Length;
                }
            }
            else
            {
                return rr;
            }
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static void Process(Span<byte> rb, Span<float> wb)
        {
            unchecked
            {
#if NET5_0_OR_GREATER
                if (AdvSimd.Arm64.IsSupported)
                {
                    ProcessAdvSimdArm64(rb, wb);
                    return;
                }
                if (AdvSimd.IsSupported)
                {
                    ProcessAdvSimd(rb, wb);
                    return;
                }
#endif
#if NETCOREAPP3_1_OR_GREATER
                if (rb.Length > 64 && !IntrinsicsUtils.AvoidAvxHeavyOperations && Avx2.IsSupported)
                {
                    ProcessAvx2MM256(rb, wb);
                    return;
                }
                if (rb.Length > 32 && Avx2.IsSupported)
                {
                    ProcessAvx2MM128(rb, wb);
                    return;
                }
                if (rb.Length > 8 && Sse41.IsSupported)
                {
                    ProcessSse41(rb, wb);
                    return;
                }
#endif
                ProcessStandard(rb, wb);
            }
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ProcessStandard(Span<byte> rb, Span<float> wb)
        {
            unchecked
            {
                for (int i = 0; i < rb.Length; i++)
                {
                    byte v = rb[i];
                    wb[i] = ConvertMuLawToSingle(v);
                }
            }
        }
#if NET5_0_OR_GREATER
        #region Arm Intrinsics
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ProcessAdvSimd(Span<byte> rb, Span<float> wb)
        {
            unchecked
            {
                var v0_8b = Vector64.Create(~0u);
                var v1_4s = Vector128.Create(0x8000_0000u);
                var v2_4s = Vector128.Create(0x3b84_0000u);
                var v3_4s = Vector128.Create(0x83f8_0000u);
                nint length = rb.Length;
                nint i;
                ref byte rsi = ref MemoryMarshal.GetReference(rb);
                ref float rdi = ref MemoryMarshal.GetReference(wb);
                //This loop is almost directly translated from ProcessSse41, and might be suboptimal on physical ARM chips.
                nint olen = length - 7;
                for (i = 0; i < olen; i += 8)
                {
                    var v4_8b = Vector64.CreateScalarUnsafe(Unsafe.As<byte, uint>(ref Unsafe.Add(ref rsi, i)));
                    var v5_8b = Vector64.CreateScalarUnsafe(Unsafe.As<byte, uint>(ref Unsafe.Add(ref rsi, i + 4)));
                    v4_8b = AdvSimd.Xor(v4_8b, v0_8b);
                    v5_8b = AdvSimd.Xor(v5_8b, v0_8b);
                    var v4_8h = AdvSimd.ShiftLeftLogicalWideningLower(v4_8b.AsSByte(), 0);
                    var v5_8h = AdvSimd.ShiftLeftLogicalWideningLower(v5_8b.AsSByte(), 0);
                    var v4_4s = AdvSimd.ShiftLeftLogicalWideningLower(v4_8h.GetLower(), 0).AsUInt32();
                    var v5_4s = AdvSimd.ShiftLeftLogicalWideningLower(v5_8h.GetLower(), 0).AsUInt32();
                    var v6_4s = AdvSimd.And(v4_4s, v1_4s);
                    var v7_4s = AdvSimd.And(v5_4s, v1_4s);
                    v4_4s = AdvSimd.ShiftLeftLogical(v4_4s, 19);
                    v5_4s = AdvSimd.ShiftLeftLogical(v5_4s, 19);
                    v6_4s = AdvSimd.Or(v6_4s, v2_4s);
                    v7_4s = AdvSimd.Or(v7_4s, v2_4s);
                    v4_4s = AdvSimd.And(v4_4s, v3_4s);
                    v5_4s = AdvSimd.And(v5_4s, v3_4s);
                    v4_4s = AdvSimd.Add(v4_4s, v2_4s);
                    v5_4s = AdvSimd.Add(v5_4s, v2_4s);
                    v4_4s = AdvSimd.Subtract(v4_4s.AsSingle(), v6_4s.AsSingle()).AsUInt32();
                    v5_4s = AdvSimd.Subtract(v5_4s.AsSingle(), v7_4s.AsSingle()).AsUInt32();
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i)) = v4_4s.AsSingle();
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 4)) = v5_4s.AsSingle();
                }
                for (; i < length; i++)
                {
                    byte g = Unsafe.Add(ref rsi, i);
                    Unsafe.Add(ref rdi, i) = ConvertMuLawToSingle(g);
                }
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ProcessAdvSimdArm64(Span<byte> rb, Span<float> wb)
        {
            unchecked
            {
                var v0_8b = Vector64.Create(~0u);
                var v1_4s = Vector128.Create(0x8000_0000u);
                var v2_4s = Vector128.Create(0x3b84_0000u);
                var v3_4s = Vector128.Create(0x83f8_0000u);
                nint length = rb.Length;
                nint i;
                ref byte rsi = ref MemoryMarshal.GetReference(rb);
                ref float rdi = ref MemoryMarshal.GetReference(wb);
                //This loop is almost directly translated from ProcessSse41, and might be suboptimal on physical Armv8 chips.
                nint olen = length - 7;
                for (i = 0; i < olen; i += 8)
                {
                    var v4_8b = Vector64.CreateScalarUnsafe(Unsafe.As<byte, uint>(ref Unsafe.Add(ref rsi, i)));
                    var v5_8b = Vector64.CreateScalarUnsafe(Unsafe.As<byte, uint>(ref Unsafe.Add(ref rsi, i + 4)));
                    v4_8b = AdvSimd.Xor(v4_8b, v0_8b);
                    v5_8b = AdvSimd.Xor(v5_8b, v0_8b);
                    var v4_8h = AdvSimd.ShiftLeftLogicalWideningLower(v4_8b.AsSByte(), 0);
                    var v5_8h = AdvSimd.ShiftLeftLogicalWideningLower(v5_8b.AsSByte(), 0);
                    var v4_4s = AdvSimd.ShiftLeftLogicalWideningLower(v4_8h.GetLower(), 0).AsUInt32();
                    var v5_4s = AdvSimd.ShiftLeftLogicalWideningLower(v5_8h.GetLower(), 0).AsUInt32();
                    var v6_4s = AdvSimd.And(v4_4s, v1_4s);
                    var v7_4s = AdvSimd.And(v5_4s, v1_4s);
                    v4_4s = AdvSimd.ShiftLeftLogical(v4_4s, 19);
                    v5_4s = AdvSimd.ShiftLeftLogical(v5_4s, 19);
                    v6_4s = AdvSimd.Or(v6_4s, v2_4s);
                    v7_4s = AdvSimd.Or(v7_4s, v2_4s);
                    v4_4s = AdvSimd.And(v4_4s, v3_4s);
                    v5_4s = AdvSimd.And(v5_4s, v3_4s);
                    v4_4s = AdvSimd.Add(v4_4s, v2_4s);
                    v5_4s = AdvSimd.Add(v5_4s, v2_4s);
                    v4_4s = AdvSimd.Subtract(v4_4s.AsSingle(), v6_4s.AsSingle()).AsUInt32();
                    v5_4s = AdvSimd.Subtract(v5_4s.AsSingle(), v7_4s.AsSingle()).AsUInt32();
                    AdvSimdUtils.Arm64.StorePair(ref Unsafe.Add(ref rdi, i), v4_4s.AsSingle(), v5_4s.AsSingle());
                }
                for (; i < length; i++)
                {
                    byte g = Unsafe.Add(ref rsi, i);
                    Unsafe.Add(ref rdi, i) = ConvertMuLawToSingle(g);
                }
            }
        }
        #endregion
#endif
#if NETCOREAPP3_1_OR_GREATER
        #region X86 Intrinsics

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ProcessAvx2MM128(Span<byte> rb, Span<float> wb)
        {
            unchecked
            {
                var xmm0 = Vector128.Create(~0u);
                var xmm1 = Vector128.Create(0x8000_0000u);
                var xmm2 = Vector128.Create(0x3b84_0000u);
                var xmm3 = Vector128.Create(0x83f8_0000u);
                nint length = rb.Length;
                nint i;
                ref byte rsi = ref MemoryMarshal.GetReference(rb);
                ref float rdi = ref MemoryMarshal.GetReference(wb);
                nint olen = length - 15;
                for (i = 0; i < olen; i += 16)
                {
                    var xmm4 = Vector128.CreateScalarUnsafe(Unsafe.As<byte, uint>(ref Unsafe.Add(ref rsi, i)));
                    var xmm5 = Vector128.CreateScalarUnsafe(Unsafe.As<byte, uint>(ref Unsafe.Add(ref rsi, i + 4)));
                    var xmm6 = Vector128.CreateScalarUnsafe(Unsafe.As<byte, uint>(ref Unsafe.Add(ref rsi, i + 8)));
                    var xmm7 = Vector128.CreateScalarUnsafe(Unsafe.As<byte, uint>(ref Unsafe.Add(ref rsi, i + 12)));
                    xmm4 = Sse2.Xor(xmm4, xmm0);
                    xmm5 = Sse2.Xor(xmm5, xmm0);
                    xmm6 = Sse2.Xor(xmm6, xmm0);
                    xmm7 = Sse2.Xor(xmm7, xmm0);
                    xmm4 = Sse41.ConvertToVector128Int32(xmm4.AsSByte()).AsUInt32();
                    xmm5 = Sse41.ConvertToVector128Int32(xmm5.AsSByte()).AsUInt32();
                    xmm6 = Sse41.ConvertToVector128Int32(xmm6.AsSByte()).AsUInt32();
                    xmm7 = Sse41.ConvertToVector128Int32(xmm7.AsSByte()).AsUInt32();
                    xmm4 = Sse2.ShiftLeftLogical(xmm4, 19);
                    xmm5 = Sse2.ShiftLeftLogical(xmm5, 19);
                    xmm6 = Sse2.ShiftLeftLogical(xmm6, 19);
                    xmm7 = Sse2.ShiftLeftLogical(xmm7, 19);
                    var xmm8 = Sse2.And(xmm4, xmm1);
                    var xmm9 = Sse2.And(xmm5, xmm1);
                    var xmm10 = Sse2.And(xmm6, xmm1);
                    var xmm11 = Sse2.And(xmm7, xmm1);
                    xmm8 = Sse2.Or(xmm8, xmm2);
                    xmm9 = Sse2.Or(xmm9, xmm2);
                    xmm10 = Sse2.Or(xmm10, xmm2);
                    xmm11 = Sse2.Or(xmm11, xmm2);
                    xmm4 = Sse2.And(xmm4, xmm3);
                    xmm5 = Sse2.And(xmm5, xmm3);
                    xmm6 = Sse2.And(xmm6, xmm3);
                    xmm7 = Sse2.And(xmm7, xmm3);
                    xmm4 = Sse2.Add(xmm4, xmm2);
                    xmm4 = Sse.Subtract(xmm4.AsSingle(), xmm8.AsSingle()).AsUInt32();
                    xmm5 = Sse2.Add(xmm5, xmm2);
                    xmm5 = Sse.Subtract(xmm5.AsSingle(), xmm9.AsSingle()).AsUInt32();
                    xmm6 = Sse2.Add(xmm6, xmm2);
                    xmm6 = Sse.Subtract(xmm6.AsSingle(), xmm10.AsSingle()).AsUInt32();
                    xmm7 = Sse2.Add(xmm7, xmm2);
                    xmm7 = Sse.Subtract(xmm7.AsSingle(), xmm11.AsSingle()).AsUInt32();
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i)) = xmm4.AsSingle();
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 4)) = xmm5.AsSingle();
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 8)) = xmm6.AsSingle();
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 12)) = xmm7.AsSingle();
                }
                for (; i < length; i++)
                {
                    byte g = Unsafe.Add(ref rsi, i);
                    Unsafe.Add(ref rdi, i) = ConvertMuLawToSingle(g);
                }
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ProcessAvx2MM256(Span<byte> rb, Span<float> wb)
        {
            unchecked
            {
                var xmm0 = Vector128.CreateScalarUnsafe(~0ul).AsUInt32();
                var ymm1 = Vector256.Create(0x8000_0000u);
                var ymm2 = Vector256.Create(0x3b84_0000u);
                var ymm3 = Vector256.Create(0x83f8_0000u);
                nint length = rb.Length;
                nint i;
                ref byte rsi = ref MemoryMarshal.GetReference(rb);
                ref float rdi = ref MemoryMarshal.GetReference(wb);
                nint olen = length - 31;
                for (i = 0; i < olen; i += 32)
                {
                    var xmm4 = Vector128.CreateScalarUnsafe(Unsafe.As<byte, ulong>(ref Unsafe.Add(ref rsi, i))).AsUInt32();
                    var xmm5 = Vector128.CreateScalarUnsafe(Unsafe.As<byte, ulong>(ref Unsafe.Add(ref rsi, i + 8))).AsUInt32();
                    var xmm6 = Vector128.CreateScalarUnsafe(Unsafe.As<byte, ulong>(ref Unsafe.Add(ref rsi, i + 16))).AsUInt32();
                    var xmm7 = Vector128.CreateScalarUnsafe(Unsafe.As<byte, ulong>(ref Unsafe.Add(ref rsi, i + 24))).AsUInt32();
                    xmm4 = Sse2.Xor(xmm4, xmm0);
                    xmm5 = Sse2.Xor(xmm5, xmm0);
                    xmm6 = Sse2.Xor(xmm6, xmm0);
                    xmm7 = Sse2.Xor(xmm7, xmm0);
                    var ymm4 = Avx2.ConvertToVector256Int32(xmm4.AsSByte()).AsUInt32();
                    var ymm5 = Avx2.ConvertToVector256Int32(xmm5.AsSByte()).AsUInt32();
                    var ymm6 = Avx2.ConvertToVector256Int32(xmm6.AsSByte()).AsUInt32();
                    var ymm7 = Avx2.ConvertToVector256Int32(xmm7.AsSByte()).AsUInt32();
                    ymm4 = Avx2.ShiftLeftLogical(ymm4, 19);
                    ymm5 = Avx2.ShiftLeftLogical(ymm5, 19);
                    ymm6 = Avx2.ShiftLeftLogical(ymm6, 19);
                    ymm7 = Avx2.ShiftLeftLogical(ymm7, 19);
                    var ymm8 = Avx2.And(ymm4, ymm1);
                    var ymm9 = Avx2.And(ymm5, ymm1);
                    var ymm10 = Avx2.And(ymm6, ymm1);
                    var ymm11 = Avx2.And(ymm7, ymm1);
                    ymm8 = Avx2.Or(ymm8, ymm2);
                    ymm9 = Avx2.Or(ymm9, ymm2);
                    ymm10 = Avx2.Or(ymm10, ymm2);
                    ymm11 = Avx2.Or(ymm11, ymm2);
                    ymm4 = Avx2.And(ymm4, ymm3);
                    ymm5 = Avx2.And(ymm5, ymm3);
                    ymm6 = Avx2.And(ymm6, ymm3);
                    ymm7 = Avx2.And(ymm7, ymm3);
                    ymm4 = Avx2.Add(ymm4, ymm2);
                    ymm4 = Avx.Subtract(ymm4.AsSingle(), ymm8.AsSingle()).AsUInt32();
                    ymm5 = Avx2.Add(ymm5, ymm2);
                    ymm5 = Avx.Subtract(ymm5.AsSingle(), ymm9.AsSingle()).AsUInt32();
                    ymm6 = Avx2.Add(ymm6, ymm2);
                    ymm6 = Avx.Subtract(ymm6.AsSingle(), ymm10.AsSingle()).AsUInt32();
                    ymm7 = Avx2.Add(ymm7, ymm2);
                    ymm7 = Avx.Subtract(ymm7.AsSingle(), ymm11.AsSingle()).AsUInt32();
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i)) = ymm4.AsSingle();
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 8)) = ymm5.AsSingle();
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 16)) = ymm6.AsSingle();
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 24)) = ymm7.AsSingle();
                }
                olen = length - 3;
                for (; i < olen; i += 4)
                {
                    var xmm4 = Vector128.CreateScalarUnsafe(Unsafe.As<byte, uint>(ref Unsafe.Add(ref rsi, i)));
                    xmm4 = Sse2.Xor(xmm4, xmm0);
                    xmm4 = Sse41.ConvertToVector128Int32(xmm4.AsSByte()).AsUInt32();
                    xmm4 = Sse2.ShiftLeftLogical(xmm4, 19);
                    var xmm6 = Sse2.And(xmm4, ymm1.GetLower());
                    xmm6 = Sse2.Or(xmm6, ymm2.GetLower());
                    xmm4 = Sse2.And(xmm4, ymm3.GetLower());
                    xmm4 = Sse2.Add(xmm4, ymm2.GetLower());
                    xmm4 = Sse.Subtract(xmm4.AsSingle(), xmm6.AsSingle()).AsUInt32();
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i)) = xmm4.AsSingle();
                }
                for (; i < length; i++)
                {
                    byte g = Unsafe.Add(ref rsi, i);
                    Unsafe.Add(ref rdi, i) = ConvertMuLawToSingle(g);
                }
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ProcessSse41(Span<byte> rb, Span<float> wb)
        {
            unchecked
            {
                var xmm0 = Vector128.Create(~0u);
                var xmm1 = Vector128.Create(0x8000_0000u);
                var xmm2 = Vector128.Create(0x3b84_0000u);
                var xmm3 = Vector128.Create(0x83f8_0000u);
                nint length = rb.Length;
                nint i;
                ref byte rsi = ref MemoryMarshal.GetReference(rb);
                ref float rdi = ref MemoryMarshal.GetReference(wb);
                nint olen = length - 7;
                for (i = 0; i < olen; i += 8)
                {
                    var xmm4 = Vector128.CreateScalarUnsafe(Unsafe.As<byte, uint>(ref Unsafe.Add(ref rsi, i)));
                    var xmm5 = Vector128.CreateScalarUnsafe(Unsafe.As<byte, uint>(ref Unsafe.Add(ref rsi, i + 4)));
                    xmm4 = Sse2.Xor(xmm4, xmm0);
                    xmm5 = Sse2.Xor(xmm5, xmm0);
                    xmm4 = Sse41.ConvertToVector128Int32(xmm4.AsSByte()).AsUInt32();
                    xmm5 = Sse41.ConvertToVector128Int32(xmm5.AsSByte()).AsUInt32();
                    var xmm6 = Sse2.And(xmm4, xmm1);
                    var xmm7 = Sse2.And(xmm5, xmm1);
                    xmm4 = Sse2.ShiftLeftLogical(xmm4, 19);
                    xmm5 = Sse2.ShiftLeftLogical(xmm5, 19);
                    xmm6 = Sse2.Or(xmm6, xmm2);
                    xmm7 = Sse2.Or(xmm7, xmm2);
                    xmm4 = Sse2.And(xmm4, xmm3);
                    xmm5 = Sse2.And(xmm5, xmm3);
                    xmm4 = Sse2.Add(xmm4, xmm2);
                    xmm4 = Sse.Subtract(xmm4.AsSingle(), xmm6.AsSingle()).AsUInt32();
                    xmm5 = Sse2.Add(xmm5, xmm2);
                    xmm5 = Sse.Subtract(xmm5.AsSingle(), xmm7.AsSingle()).AsUInt32();
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i)) = xmm4.AsSingle();
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 4)) = xmm5.AsSingle();
                }
                for (; i < length; i++)
                {
                    byte g = Unsafe.Add(ref rsi, i);
                    Unsafe.Add(ref rdi, i) = ConvertMuLawToSingle(g);
                }
            }
        }
        #endregion
#endif

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static float ConvertMuLawToSingle(byte value)
        {
            unchecked
            {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
                uint v = (uint)(sbyte)~value;
                uint y = (v & 0x8000_0000u) + 0x3b84_0000u;
                float f = BitConverter.Int32BitsToSingle((int)y);
                v <<= 19;
                v &= 0x83f8_0000;
                v += 0x3b84_0000;
                return BitConverter.Int32BitsToSingle((int)v) - f;
#else
                uint v = (uint)(sbyte)~value;
                var y = (v & 0x8000_0000u) + 0x3b84_0000u;
                var f = Unsafe.As<uint, float>(ref y);
                v <<= 19;
                v &= 0x83f8_0000;
                v += 0x3b84_0000;
                return Unsafe.As<uint, float>(ref v) - f;
#endif
            }
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private int CheckBuffer(int sampleLengthOut) => sampleLengthOut;

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
