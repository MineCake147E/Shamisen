using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;


#if NETCOREAPP3_1_OR_GREATER
using System.Runtime.Intrinsics.X86;
using System.Runtime.Intrinsics;
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
            : base(source, new SampleFormat(source.Format.Channels, source.Format.SampleRate)) => bufferWrapper = new ResizablePooledBufferWrapper<byte>(1);

        /// <summary>
        /// Gets the bytes consumed per sample.
        /// </summary>
        /// <value>
        /// The bytes consumed per sample.
        /// </value>
        protected override int BytesPerSample { get => 1; }

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
            Span<byte> srcBuffer = bytebuf.Slice(bytebuf.Length - internalBufferLengthRequired, internalBufferLengthRequired);
            Span<byte> readBuffer = srcBuffer.SliceAlign(Format.Channels);
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
                    for (int i = 0; i < rb.Length; i++)
                    {
                        byte v = rb[i];
                        wb[i] = ConvertALawToSingle(v);
                    }
                    return wb.Length;
                }
            }
            else
            {
                return rr;
            }
        }
#if NETCOREAPP3_1_OR_GREATER

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static void ProcessAvx2(Span<byte> rb, Span<float> wb)
        {
            unchecked
            {
                nint length = rb.Length;
                nint i;
                ref var rsi = ref MemoryMarshal.GetReference(rb);
                ref var rdi = ref MemoryMarshal.GetReference(wb);
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
                    var g = Unsafe.Add(ref rsi, i);
                    Unsafe.Add(ref rdi, i) = ConvertALawToSingle(g);
                }
            }
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static void ProcessAvx2M2(Span<byte> rb, Span<float> wb)
        {
            unchecked
            {
                nint length = rb.Length;
                nint i;
                ref var rsi = ref MemoryMarshal.GetReference(rb);
                ref var rdi = ref MemoryMarshal.GetReference(wb);
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
        private static void ProcessAdvSimd64(Span<byte> rb, Span<float> wb)
        {
            unchecked
            {
                nint length = rb.Length;
                nint i;
                ref var x0 = ref MemoryMarshal.GetReference(rb);
                ref var x1 = ref MemoryMarshal.GetReference(wb);
                var v0_4h = Vector64.Create((ushort)0xd5);
                var v1_4h = Vector64.Create((ushort)0x70);
                var v2_4h = Vector64.Create((ushort)0xd5);
                var v3_4s = Vector128.Create(~0x1au);
                var v4_4s = Vector128.Create(0x3b80_0000u);
                var v5_4s = Vector128.Create(0x83fc_0000u);
                for (i = 0; i < length - 3; i += 4)
                {
                    var s6 = Vector64.CreateScalarUnsafe(Unsafe.As<byte, uint>(ref Unsafe.Add(ref x0, i)));
                    var v6_8h = AdvSimd.ShiftLeftLogicalWideningLower(s6.AsByte(), 0);
                    v6_8h = AdvSimd.Xor(v6_8h.GetLower().AsByte(), v0_4h.AsByte()).AsUInt16().ToVector128Unsafe();
                    //v0_8b = AdvSimd.Xor(v0_8b, v9_8b);  //Sse2.Xor
                    //var v10_8h = AdvSimd.ShiftLeftLogicalWideningLower(v0_8b.AsSByte(), 0);

                    //var v10_4s = AdvSimd.ShiftLeftLogicalWideningLower(v10_8h.GetLower(), 0).AsUInt32();//Avx2.ConvertToVector256Int32
                    //var v11_4s = v10_4s;
                    //v10_4s = AdvSimd.ShiftLeftLogical(v10_4s, 1);
                    //v10_4s = AdvSimd.Or(v10_4s, v1_4s);
                    //v0_8b = AdvSimd.And(v0_8b, v9_8b);
                    //v0_8b = AdvSimd.CompareEqual(v0_8b, v3_2s);

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
            int internalBufferLengthRequired = buffer.Length;
            var bytebuf = MemoryMarshal.Cast<float, byte>(buffer);
            Span<byte> srcBuffer = bytebuf.Slice(bytebuf.Length - internalBufferLengthRequired);
            Span<byte> readBuffer = srcBuffer.SliceAlign(Format.Channels);
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
                byte v = value;
                bool s = v <= 127;
                uint ss = Unsafe.As<bool, byte>(ref s);
                v ^= (byte)0b01010101u;
                uint x = (uint)v & 0x7f;
                bool j = x > 0xf;
                uint a = Unsafe.As<bool, byte>(ref j);
                int exp = (int)(x >> 4);
                var man = x & 0b1111;
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
