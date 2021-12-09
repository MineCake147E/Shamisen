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
        private const float Multiplier = 1 / 32768.0f;

        /// <summary>
        /// Initializes a new instance of the <see cref="ALawToSampleConverter"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        public ALawToSampleConverter(IReadableAudioSource<byte, IWaveFormat> source)
            : base(source, new SampleFormat(source.Format.Channels, source.Format.SampleRate))
        {
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
            var internalBufferLengthRequired = buffer.Length;
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
                        ProcessAvx2(rb, wb);
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
                nint length = MathI.Min(rb.Length, wb.Length);
                nint i = 0;
                ref var x9 = ref MemoryMarshal.GetReference(rb);
                ref var x10 = ref MemoryMarshal.GetReference(wb);
                for (; i < length; i++)
                {
                    var v = Unsafe.Add(ref x9, i);
                    Unsafe.Add(ref x10, i) = ConvertALawToSingle(v);
                }
            }
        }

#if NETCOREAPP3_1_OR_GREATER

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ProcessAvx2(Span<byte> rb, Span<float> wb)
        {
            unchecked
            {
                nint length = MathI.Min(rb.Length, wb.Length);
                nint i;
                ref var rsi = ref MemoryMarshal.GetReference(rb);
                ref var rdi = ref MemoryMarshal.GetReference(wb);
                var xmm8 = Vector128.CreateScalarUnsafe(0xd5d5_d5d5_d5d5_d5d5ul).AsInt32();
                var ymm9 = Vector256.Create(0x83F8_0000u).AsInt32();
                var ymm2 = Vector256.Create(0x0004_0000);
                var ymm3 = Vector256.Create(0x0080_0000);
                var ymm4 = Vector256.Create(0x3C00_0000);
                var ymm5 = Vector256.Create(0x3B80_0000);
                var olen = length - 31;
                for (i = 0; i < olen; i += 32)
                {
                    var xmm6 = Vector128.CreateScalarUnsafe(Unsafe.As<byte, ulong>(ref Unsafe.Add(ref rsi, i))).AsInt32();
                    var xmm7 = Vector128.CreateScalarUnsafe(Unsafe.As<byte, ulong>(ref Unsafe.Add(ref rsi, i + 8))).AsInt32();
                    var xmm0 = Vector128.CreateScalarUnsafe(Unsafe.As<byte, ulong>(ref Unsafe.Add(ref rsi, i + 16))).AsInt32();
                    var xmm1 = Vector128.CreateScalarUnsafe(Unsafe.As<byte, ulong>(ref Unsafe.Add(ref rsi, i + 24))).AsInt32();
                    xmm6 = Sse2.Xor(xmm8, xmm6);
                    xmm7 = Sse2.Xor(xmm8, xmm7);
                    xmm0 = Sse2.Xor(xmm8, xmm0);
                    xmm1 = Sse2.Xor(xmm8, xmm1);
                    var ymm6 = Avx2.ConvertToVector256Int32(xmm6.AsSByte());
                    var ymm7 = Avx2.ConvertToVector256Int32(xmm7.AsSByte());
                    var ymm0 = Avx2.ConvertToVector256Int32(xmm0.AsSByte());
                    var ymm1 = Avx2.ConvertToVector256Int32(xmm1.AsSByte());
                    ymm6 = Avx2.ShiftLeftLogical(ymm6, 19);
                    ymm7 = Avx2.ShiftLeftLogical(ymm7, 19);
                    ymm0 = Avx2.ShiftLeftLogical(ymm0, 19);
                    ymm1 = Avx2.ShiftLeftLogical(ymm1, 19);
                    ymm6 = Avx2.And(ymm9, ymm6);
                    ymm7 = Avx2.And(ymm9, ymm7);
                    ymm0 = Avx2.And(ymm9, ymm0);
                    ymm1 = Avx2.And(ymm9, ymm1);
                    var ymm10 = Avx2.Or(ymm6, ymm2);
                    var ymm11 = Avx2.Or(ymm7, ymm2);
                    var ymm12 = Avx2.Or(ymm0, ymm2);
                    var ymm13 = Avx2.Or(ymm1, ymm2);
                    var ymm14 = Avx2.Add(ymm6, ymm3);
                    ymm10 = Avx.Add(ymm10.AsSingle(), ymm14.AsSingle()).AsInt32();
                    var ymm15 = Avx2.Add(ymm7, ymm3);
                    ymm11 = Avx.Add(ymm11.AsSingle(), ymm15.AsSingle()).AsInt32();
                    ymm14 = Avx2.Add(ymm0, ymm3);
                    ymm12 = Avx.Add(ymm12.AsSingle(), ymm14.AsSingle()).AsInt32();
                    ymm15 = Avx2.Add(ymm1, ymm3);
                    ymm13 = Avx.Add(ymm13.AsSingle(), ymm15.AsSingle()).AsInt32();
                    ymm6 = Avx2.Or(ymm6, ymm4);
                    ymm7 = Avx2.Or(ymm7, ymm4);
                    ymm0 = Avx2.Or(ymm0, ymm4);
                    ymm1 = Avx2.Or(ymm1, ymm4);
                    ymm10 = Avx2.Add(ymm10, ymm5);
                    ymm6 = Avx.Subtract(ymm10.AsSingle(), ymm6.AsSingle()).AsInt32();
                    ymm15 = Avx2.Add(ymm11, ymm5);
                    ymm7 = Avx.Subtract(ymm15.AsSingle(), ymm7.AsSingle()).AsInt32();
                    ymm10 = Avx2.Add(ymm12, ymm5);
                    ymm0 = Avx.Subtract(ymm10.AsSingle(), ymm0.AsSingle()).AsInt32();
                    ymm15 = Avx2.Add(ymm13, ymm5);
                    ymm1 = Avx.Subtract(ymm15.AsSingle(), ymm1.AsSingle()).AsInt32();
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.AddByteOffset(ref Unsafe.Add(ref rdi, i), 0)) = ymm6.AsSingle();
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.AddByteOffset(ref Unsafe.Add(ref rdi, i), 32)) = ymm7.AsSingle();
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.AddByteOffset(ref Unsafe.Add(ref rdi, i), 64)) = ymm0.AsSingle();
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.AddByteOffset(ref Unsafe.Add(ref rdi, i), 96)) = ymm1.AsSingle();
                }
                olen = length - 7;
                for (; i < olen; i += 8)
                {
                    var xmm6 = Vector128.CreateScalarUnsafe(Unsafe.As<byte, uint>(ref Unsafe.Add(ref rsi, i))).AsInt32();
                    var xmm7 = Vector128.CreateScalarUnsafe(Unsafe.As<byte, uint>(ref Unsafe.Add(ref rsi, i + 4))).AsInt32();
                    xmm6 = Sse2.Xor(xmm8, xmm6);
                    xmm7 = Sse2.Xor(xmm8, xmm7);
                    xmm6 = Sse41.ConvertToVector128Int32(xmm6.AsSByte());
                    xmm7 = Sse41.ConvertToVector128Int32(xmm7.AsSByte());
                    xmm6 = Sse2.ShiftLeftLogical(xmm6, 19);
                    xmm7 = Sse2.ShiftLeftLogical(xmm7, 19);
                    xmm6 = Sse2.And(ymm9.GetLower(), xmm6);
                    xmm7 = Sse2.And(ymm9.GetLower(), xmm7);
                    var xmm10 = Sse2.Or(xmm6, ymm2.GetLower());
                    var xmm11 = Sse2.Or(xmm7, ymm2.GetLower());
                    var xmm14 = Sse2.Add(xmm6, ymm3.GetLower());
                    xmm10 = Sse.Add(xmm10.AsSingle(), xmm14.AsSingle()).AsInt32();
                    var xmm15 = Sse2.Add(xmm7, ymm3.GetLower());
                    xmm11 = Sse.Add(xmm11.AsSingle(), xmm15.AsSingle()).AsInt32();
                    xmm6 = Sse2.Or(xmm6, ymm4.GetLower());
                    xmm7 = Sse2.Or(xmm7, ymm4.GetLower());
                    xmm10 = Sse2.Add(xmm10, ymm5.GetLower());
                    xmm6 = Sse.Subtract(xmm10.AsSingle(), xmm6.AsSingle()).AsInt32();
                    xmm15 = Sse2.Add(xmm11, ymm5.GetLower());
                    xmm7 = Sse.Subtract(xmm15.AsSingle(), xmm7.AsSingle()).AsInt32();
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.AddByteOffset(ref Unsafe.Add(ref rdi, i), 0)) = xmm6.AsSingle();
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.AddByteOffset(ref Unsafe.Add(ref rdi, i), 16)) = xmm7.AsSingle();
                }
                for (; i < length; i++)
                {
                    var g = Unsafe.Add(ref rsi, i);
                    Unsafe.Add(ref rdi, i) = ConvertALawToSingle(g);
                }
            }
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ProcessSse41(Span<byte> rb, Span<float> wb)
        {
            unchecked
            {
                nint length = MathI.Min(rb.Length, wb.Length);
                nint i;
                ref var rsi = ref MemoryMarshal.GetReference(rb);
                ref var rdi = ref MemoryMarshal.GetReference(wb);
                var xmm8 = Vector128.CreateScalarUnsafe(0xd5d5_d5d5_d5d5_d5d5ul).AsInt32();
                var xmm9 = Vector128.Create(0x83F8_0000u).AsInt32();
                var xmm2 = Vector128.Create(0x0004_0000);
                var xmm3 = Vector128.Create(0x0080_0000);
                var xmm4 = Vector128.Create(0x3C00_0000);
                var xmm5 = Vector128.Create(0x3B80_0000);
                var olen = length - 7;
                for (i = 0; i < olen; i += 8)
                {
                    var xmm6 = Vector128.CreateScalarUnsafe(Unsafe.As<byte, uint>(ref Unsafe.Add(ref rsi, i))).AsInt32();
                    var xmm7 = Vector128.CreateScalarUnsafe(Unsafe.As<byte, uint>(ref Unsafe.Add(ref rsi, i + 4))).AsInt32();
                    xmm6 = Sse2.Xor(xmm8, xmm6);
                    xmm7 = Sse2.Xor(xmm8, xmm7);
                    xmm6 = Sse41.ConvertToVector128Int32(xmm6.AsSByte());
                    xmm7 = Sse41.ConvertToVector128Int32(xmm7.AsSByte());
                    xmm6 = Sse2.ShiftLeftLogical(xmm6, 19);
                    xmm7 = Sse2.ShiftLeftLogical(xmm7, 19);
                    xmm6 = Sse2.And(xmm9, xmm6);
                    xmm7 = Sse2.And(xmm9, xmm7);
                    var xmm10 = Sse2.Or(xmm6, xmm2);
                    var xmm11 = Sse2.Or(xmm7, xmm2);
                    var xmm14 = Sse2.Add(xmm6, xmm3);
                    xmm10 = Sse.Add(xmm10.AsSingle(), xmm14.AsSingle()).AsInt32();
                    var xmm15 = Sse2.Add(xmm7, xmm3);
                    xmm11 = Sse.Add(xmm11.AsSingle(), xmm15.AsSingle()).AsInt32();
                    xmm6 = Sse2.Or(xmm6, xmm4);
                    xmm7 = Sse2.Or(xmm7, xmm4);
                    xmm10 = Sse2.Add(xmm10, xmm5);
                    xmm6 = Sse.Subtract(xmm10.AsSingle(), xmm6.AsSingle()).AsInt32();
                    xmm15 = Sse2.Add(xmm11, xmm5);
                    xmm7 = Sse.Subtract(xmm15.AsSingle(), xmm7.AsSingle()).AsInt32();
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.AddByteOffset(ref Unsafe.Add(ref rdi, i), 0)) = xmm6.AsSingle();
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.AddByteOffset(ref Unsafe.Add(ref rdi, i), 16)) = xmm7.AsSingle();
                }
                for (; i < length; i++)
                {
                    var g = Unsafe.Add(ref rsi, i);
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
                nint length = MathI.Min(rb.Length, wb.Length);
                nint i;
                ref var x11 = ref MemoryMarshal.GetReference(rb);
                ref var x10 = ref MemoryMarshal.GetReference(wb);
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
                    var g = Unsafe.Add(ref x11, i);
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
                var v = ((uint)(sbyte)value << 1) | 1;
                var m = 0x3b80_0000u;
                if ((value & 0x70) == 0)
                {
                    var zs = (uint)MathI.LeadingZeroCount(v << 26);
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
            var internalBufferLengthRequired = buffer.Length;
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
                    for (var i = 0; i < rb.Length; i++)
                    {
                        var v = rb[i];
                        var h = ConvertALawToInt16(v);
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
                var v = value;
                var s = v <= 127;
                uint ss = Unsafe.As<bool, byte>(ref s);
                v ^= (byte)0b01010101u;
                var x = (uint)v & 0x7f;
                var j = x > 0xf;
                uint a = Unsafe.As<bool, byte>(ref j);
                var exp = (int)(x >> 4);
                var man = x & 0b1111;
                man += a << 4;
                man = (man << 4) + 8;
                man <<= exp - (int)a;
                return (short)((man ^ (uint)-(int)ss) + ss);
            }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!disposedValue && disposing)
            {
                Source.Dispose();
            }
            disposedValue = true;
        }
    }
}
