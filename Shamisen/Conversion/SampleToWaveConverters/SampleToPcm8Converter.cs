using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

using Shamisen.Utils;

#if NETCOREAPP3_1_OR_GREATER
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

using Shamisen.Utils.Intrinsics;

#endif
#if NET5_0_OR_GREATER
using System.Runtime.Intrinsics.Arm;
#endif
namespace Shamisen.Conversion.SampleToWaveConverters
{
    /// <summary>
    /// Converts samples to 8-bit PCM.
    /// </summary>
    /// <seealso cref="SampleToWaveConverterBase" />
    public sealed class SampleToPcm8Converter : SampleToWaveConverterBase
    {
        private Memory<byte> dsmLastOutput;
        private Memory<float> dsmAccumulator;
        private int dsmChannelPointer = 0;
        private Memory<float> readBuffer;
        private const int BufferMax = 1024; //The bufferMax is fixed to 1024 regardless of the destination type because the buffer is float.
        private int ActualBufferMax => BufferMax - (BufferMax % Source.Format.Channels);

        /// <summary>
        /// Initializes a new instance of the <see cref="SampleToPcm8Converter"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="accuracyNeeded">Turns on <see cref="AccuracyMode"/> when <c>true</c>.</param>
        public SampleToPcm8Converter(IReadableAudioSource<float, SampleFormat> source, bool accuracyNeeded = true)
            : base(source, new WaveFormat(source.Format.SampleRate, 8, source.Format.Channels, AudioEncoding.LinearPcm))
        {
            if (accuracyNeeded)
            {
                dsmAccumulator = new float[source.Format.Channels];
                dsmLastOutput = new byte[source.Format.Channels];
            }
            AccuracyMode = accuracyNeeded;
            readBuffer = new float[ActualBufferMax];
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="SampleToPcm8Converter"/> does the 8-bit Delta-Sigma modulation.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the accuracy mode is turned on; otherwise, <c>false</c>.
        /// </value>
        public bool AccuracyMode { get; }

        /// <summary>
        /// Gets the bytes consumed per sample.
        /// </summary>
        /// <value>
        /// The bytes consumed per sample.
        /// </value>
        protected override int BytesPerSample => sizeof(byte);

        /// <summary>
        /// Reads the audio to the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>
        /// The length of the data written.
        /// </returns>
        public override ReadResult Read(Span<byte> buffer)
        {
            var cursor = buffer;
            while (cursor.Length > 0)
            {
                var reader = cursor.Length >= readBuffer.Length ? readBuffer : readBuffer.Slice(0, cursor.Length);
                var rr = Source.Read(reader.Span);
                if (rr.IsEndOfStream && buffer.Length == cursor.Length) return rr;
                if (rr.HasNoData) return buffer.Length - cursor.Length;
                var u = rr.Length;
                var wrote = reader.Span.Slice(0, u);
                var dest = cursor.Slice(0, wrote.Length);
                if (wrote.Length != dest.Length)
                {
                    new InvalidOperationException(
                        $"The {nameof(wrote)}'s length and {nameof(dest)}'s length are not equal! This is a bug!").Throw();
                }

                if (AccuracyMode)
                {
                    var dsmAcc = dsmAccumulator.Span;
                    var dsmLastOut = dsmLastOutput.Span;
                    dsmChannelPointer %= dsmAcc.Length;
                    for (var i = 0; i < dest.Length; i++)
                    {
                        var diff = wrote[i] - (dsmLastOut[dsmChannelPointer] / 128.0f - 1);
                        dsmAcc[dsmChannelPointer] += diff;
                        dest[i] = dsmLastOut[dsmChannelPointer] = (byte)Math.Min(byte.MaxValue, Math.Max(dsmAcc[dsmChannelPointer] * 128 + 128, byte.MinValue));
                        dsmChannelPointer = ++dsmChannelPointer % dsmAcc.Length;
                    }
                }
                else
                {
                    ProcessNormal(wrote, dest);
                }
                cursor = cursor.Slice(dest.Length);
                if (u != reader.Length) return buffer.Length - cursor.Length;  //The Source doesn't fill whole reader so return here.
            }
            return buffer.Length;
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static void ProcessNormal(Span<float> wrote, Span<byte> dest)
        {
            unchecked
            {
#if NETCOREAPP3_1_OR_GREATER
                if (Avx2.IsSupported)
                {
                    ProcessNormalAvx2A(wrote, dest);
                }
#endif
                ProcessNormalStandard(wrote, dest);
            }
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ProcessNormalStandard(Span<float> wrote, Span<byte> dest)
        {
            var mul = new Vector<float>(128.0f);
            var sign = Vector.AsVectorSingle(new Vector<int>(int.MinValue));
            var reciprocalEpsilon = new Vector<float>(16777216f);
            var min = new Vector<float>(-128.0f);
            var max = new Vector<float>(127.0f);
            var mask = new Vector<byte>(0x80);
            ref var dst = ref MemoryMarshal.GetReference(dest);
            ref var src = ref MemoryMarshal.GetReference(wrote);
            nint i = 0, length = MathI.Min(dest.Length, wrote.Length);
            var olen = length - Vector<float>.Count * 4 + 1;
            for (; i < olen; i += Vector<float>.Count * 4)
            {
                var v0_ns = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref src, i + 0 * Vector<float>.Count));
                v0_ns *= mul;
                var v1_ns = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref src, i + 1 * Vector<float>.Count));
                v1_ns *= mul;
                var v2_ns = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref src, i + 2 * Vector<float>.Count));
                v2_ns *= mul;
                var v3_ns = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref src, i + 3 * Vector<float>.Count));
                v3_ns *= mul;
                v0_ns = VectorUtils.RoundInLoop(v0_ns, sign, reciprocalEpsilon);
                v1_ns = VectorUtils.RoundInLoop(v1_ns, sign, reciprocalEpsilon);
                v2_ns = VectorUtils.RoundInLoop(v2_ns, sign, reciprocalEpsilon);
                v3_ns = VectorUtils.RoundInLoop(v3_ns, sign, reciprocalEpsilon);
                v0_ns = Vector.Min(max, v0_ns);
                v1_ns = Vector.Min(max, v1_ns);
                v2_ns = Vector.Min(max, v2_ns);
                v3_ns = Vector.Min(max, v3_ns);
                v0_ns = Vector.Max(min, v0_ns);
                v1_ns = Vector.Max(min, v1_ns);
                v2_ns = Vector.Max(min, v2_ns);
                v3_ns = Vector.Max(min, v3_ns);
                var v0_nh = Vector.AsVectorUInt16(Vector.Narrow(Vector.ConvertToInt32(v0_ns), Vector.ConvertToInt32(v1_ns)));
                var v2_nh = Vector.AsVectorUInt16(Vector.Narrow(Vector.ConvertToInt32(v2_ns), Vector.ConvertToInt32(v3_ns)));
                var v0_nb = Vector.Narrow(v0_nh, v2_nh);
                v0_nb = Vector.Xor(v0_nb, mask);
                Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref dst, i + 0 * Vector<byte>.Count)) = v0_nb;
            }
            for (; i < length; i++)
            {
                var v = Unsafe.Add(ref src, i);
                Unsafe.Add(ref dst, i) = (byte)((byte)Math.Round(Math.Min(sbyte.MaxValue, Math.Max(v * 128, sbyte.MinValue))) + 128);
            }
        }

        #region X86
#if NETCOREAPP3_1_OR_GREATER
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ProcessNormalAvx2M(Span<float> wrote, Span<byte> dest)
        {
            var madd = Vector256.Create(128.0f);
            var sign = Vector256.Create((byte)128);
            var perm = Vector256.Create(0, 4, 1, 5, 2, 6, 3, 7);
            ref var dst = ref MemoryMarshal.GetReference(dest);
            ref var src = ref MemoryMarshal.GetReference(wrote);
            nint i = 0, length = MathI.Min(dest.Length, wrote.Length);
            var olen = length - 8 * 16 + 1;
            for (; i < olen; i += 8 * 16)
            {
                var ymm0 = Avx.Multiply(madd, Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref src, i + 0 * 8)));
                var ymm1 = Avx.Multiply(madd, Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref src, i + 1 * 8)));
                var ymm2 = Avx.Multiply(madd, Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref src, i + 2 * 8)));
                var ymm3 = Avx.Multiply(madd, Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref src, i + 3 * 8)));
                var ymm4 = Avx.Multiply(madd, Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref src, i + 4 * 8)));
                var ymm5 = Avx.Multiply(madd, Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref src, i + 5 * 8)));
                var ymm6 = Avx.Multiply(madd, Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref src, i + 6 * 8)));
                var ymm7 = Avx.Multiply(madd, Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref src, i + 7 * 8)));
                ymm0 = Avx.ConvertToVector256Int32(ymm0).AsSingle();
                ymm1 = Avx.ConvertToVector256Int32(ymm1).AsSingle();
                ymm2 = Avx.ConvertToVector256Int32(ymm2).AsSingle();
                ymm3 = Avx.ConvertToVector256Int32(ymm3).AsSingle();
                ymm4 = Avx.ConvertToVector256Int32(ymm4).AsSingle();
                ymm5 = Avx.ConvertToVector256Int32(ymm5).AsSingle();
                ymm6 = Avx.ConvertToVector256Int32(ymm6).AsSingle();
                ymm7 = Avx.ConvertToVector256Int32(ymm7).AsSingle();
                var ymm8 = Avx2.PackSignedSaturate(ymm0.AsInt32(), ymm1.AsInt32()).AsUInt32();
                var ymm9 = Avx2.PackSignedSaturate(ymm2.AsInt32(), ymm3.AsInt32()).AsUInt32();
                var ymm10 = Avx2.PackSignedSaturate(ymm4.AsInt32(), ymm5.AsInt32()).AsUInt32();
                var ymm11 = Avx2.PackSignedSaturate(ymm6.AsInt32(), ymm7.AsInt32()).AsUInt32();
                ymm8 = Avx2.PackSignedSaturate(ymm8.AsInt16(), ymm9.AsInt16()).AsUInt32();
                ymm10 = Avx2.PackSignedSaturate(ymm10.AsInt16(), ymm11.AsInt16()).AsUInt32();
                ymm8 = Avx2.PermuteVar8x32(ymm8.AsInt32(), perm).AsUInt32();
                ymm10 = Avx2.PermuteVar8x32(ymm10.AsInt32(), perm).AsUInt32();
                ymm8 = Avx2.Xor(sign, ymm8.AsByte()).AsUInt32();
                ymm10 = Avx2.Xor(sign, ymm10.AsByte()).AsUInt32();
                Unsafe.As<byte, Vector256<byte>>(ref Unsafe.Add(ref dst, i + 0 * Vector256<byte>.Count)) = ymm8.AsByte();
                Unsafe.As<byte, Vector256<byte>>(ref Unsafe.Add(ref dst, i + 1 * Vector256<byte>.Count)) = ymm10.AsByte();
                ymm0 = Avx.Multiply(madd, Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref src, i + 8 * 8)));
                ymm1 = Avx.Multiply(madd, Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref src, i + 9 * 8)));
                ymm2 = Avx.Multiply(madd, Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref src, i + 10 * 8)));
                ymm3 = Avx.Multiply(madd, Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref src, i + 11 * 8)));
                ymm4 = Avx.Multiply(madd, Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref src, i + 12 * 8)));
                ymm5 = Avx.Multiply(madd, Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref src, i + 13 * 8)));
                ymm6 = Avx.Multiply(madd, Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref src, i + 14 * 8)));
                ymm7 = Avx.Multiply(madd, Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref src, i + 15 * 8)));
                ymm0 = Avx.ConvertToVector256Int32(ymm0).AsSingle();
                ymm1 = Avx.ConvertToVector256Int32(ymm1).AsSingle();
                ymm2 = Avx.ConvertToVector256Int32(ymm2).AsSingle();
                ymm3 = Avx.ConvertToVector256Int32(ymm3).AsSingle();
                ymm4 = Avx.ConvertToVector256Int32(ymm4).AsSingle();
                ymm5 = Avx.ConvertToVector256Int32(ymm5).AsSingle();
                ymm6 = Avx.ConvertToVector256Int32(ymm6).AsSingle();
                ymm7 = Avx.ConvertToVector256Int32(ymm7).AsSingle();
                ymm8 = Avx2.PackSignedSaturate(ymm0.AsInt32(), ymm1.AsInt32()).AsUInt32();
                ymm9 = Avx2.PackSignedSaturate(ymm2.AsInt32(), ymm3.AsInt32()).AsUInt32();
                ymm10 = Avx2.PackSignedSaturate(ymm4.AsInt32(), ymm5.AsInt32()).AsUInt32();
                ymm11 = Avx2.PackSignedSaturate(ymm6.AsInt32(), ymm7.AsInt32()).AsUInt32();
                ymm8 = Avx2.PackSignedSaturate(ymm8.AsInt16(), ymm9.AsInt16()).AsUInt32();
                ymm10 = Avx2.PackSignedSaturate(ymm10.AsInt16(), ymm11.AsInt16()).AsUInt32();
                ymm8 = Avx2.PermuteVar8x32(ymm8.AsInt32(), perm).AsUInt32();
                ymm10 = Avx2.PermuteVar8x32(ymm10.AsInt32(), perm).AsUInt32();
                ymm8 = Avx2.Xor(sign, ymm8.AsByte()).AsUInt32();
                ymm10 = Avx2.Xor(sign, ymm10.AsByte()).AsUInt32();
                Unsafe.As<byte, Vector256<byte>>(ref Unsafe.Add(ref dst, i + 2 * Vector256<byte>.Count)) = ymm8.AsByte();
                Unsafe.As<byte, Vector256<byte>>(ref Unsafe.Add(ref dst, i + 3 * Vector256<byte>.Count)) = ymm10.AsByte();
            }
            olen = length - 32 + 1;
            for (; i < olen; i += 32)
            {
                var ymm0 = Avx.Multiply(madd, Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref src, i + 0 * 8)));
                var ymm1 = Avx.Multiply(madd, Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref src, i + 1 * 8)));
                var ymm2 = Avx.Multiply(madd, Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref src, i + 2 * 8)));
                var ymm3 = Avx.Multiply(madd, Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref src, i + 3 * 8)));
                ymm0 = Avx.ConvertToVector256Int32(ymm0).AsSingle();
                ymm1 = Avx.ConvertToVector256Int32(ymm1).AsSingle();
                ymm2 = Avx.ConvertToVector256Int32(ymm2).AsSingle();
                ymm3 = Avx.ConvertToVector256Int32(ymm3).AsSingle();
                var ymm8 = Avx2.PackSignedSaturate(ymm0.AsInt32(), ymm1.AsInt32()).AsUInt32();
                var ymm9 = Avx2.PackSignedSaturate(ymm2.AsInt32(), ymm3.AsInt32()).AsUInt32();
                ymm8 = Avx2.PackSignedSaturate(ymm8.AsInt16(), ymm9.AsInt16()).AsUInt32();
                ymm8 = Avx2.PermuteVar8x32(ymm8.AsInt32(), perm).AsUInt32();
                ymm8 = Avx2.Xor(sign, ymm8.AsByte()).AsUInt32();
                Unsafe.As<byte, Vector256<byte>>(ref Unsafe.Add(ref dst, i + 0 * Vector256<byte>.Count)) = ymm8.AsByte();
            }
            olen = length - 8 + 1;
            for (; i < olen; i += 8)
            {
                var ymm0 = Avx.Multiply(madd, Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref src, i + 0 * 8)));
                ymm0 = Avx.ConvertToVector256Int32(ymm0).AsSingle();
                var xmm4 = ymm0.GetUpper().AsInt32();
                var xmm0 = Sse2.PackSignedSaturate(ymm0.GetLower().AsInt32(), xmm4);
                xmm0 = Sse2.PackSignedSaturate(xmm0, xmm0).AsInt16();
                xmm0 = Sse2.Xor(xmm0, sign.GetLower().AsInt16());
                Unsafe.As<byte, long>(ref Unsafe.Add(ref dst, i)) = xmm0.AsInt64().GetElement(0);
            }
            for (; i < length; i++)
            {
                var xmm0 = Sse.MultiplyScalar(madd.GetLower(), Vector128.CreateScalarUnsafe(Unsafe.Add(ref src, i)));
                var xmm1 = Sse2.ConvertToVector128Int32(xmm0);
                var xmm2 = Sse2.PackSignedSaturate(xmm1, xmm1);
                var xmm3 = Sse2.PackSignedSaturate(xmm2, xmm2).AsByte();
                xmm3 = Sse2.Xor(xmm3, sign.GetLower());
                Unsafe.Add(ref dst, i) = xmm3.GetElement(0);
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ProcessNormalAvx2A(Span<float> wrote, Span<byte> dest)
        {
            var add = Vector256.Create(3.0f);
            var offset = Vector256.Create(16384u);
            var mask = Vector256.Create(0x007f_ffffu);
            var min = Vector256.Create(-1.0f);
            var max = Vector256.Create(127.0f / 128.0f);
            var perm = Vector256.Create(0, 4, 1, 5, 2, 6, 3, 7);
            ref var dst = ref MemoryMarshal.GetReference(dest);
            ref var src = ref MemoryMarshal.GetReference(wrote);
            nint i = 0, length = MathI.Min(dest.Length, wrote.Length);
            var olen = length - 8 * 8 + 1;
            for (; i < olen; i += 8 * 8)
            {
                var ymm0 = Avx.Min(max, Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref src, i + 0 * 8))).AsUInt32();
                var ymm1 = Avx.Min(max, Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref src, i + 1 * 8))).AsUInt32();
                var ymm2 = Avx.Min(max, Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref src, i + 2 * 8))).AsUInt32();
                var ymm3 = Avx.Min(max, Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref src, i + 3 * 8))).AsUInt32();
                ymm0 = Avx.Max(min, ymm0.AsSingle()).AsUInt32();
                ymm1 = Avx.Max(min, ymm1.AsSingle()).AsUInt32();
                ymm2 = Avx.Max(min, ymm2.AsSingle()).AsUInt32();
                ymm3 = Avx.Max(min, ymm3.AsSingle()).AsUInt32();
                ymm0 = Avx.Add(add, ymm0.AsSingle()).AsUInt32();
                ymm1 = Avx.Add(add, ymm1.AsSingle()).AsUInt32();
                ymm2 = Avx.Add(add, ymm2.AsSingle()).AsUInt32();
                ymm3 = Avx.Add(add, ymm3.AsSingle()).AsUInt32();
                ymm0 = Avx2.And(mask, ymm0);
                ymm1 = Avx2.And(mask, ymm1);
                ymm2 = Avx2.And(mask, ymm2);
                ymm3 = Avx2.And(mask, ymm3);
                ymm0 = Avx2.Add(offset, ymm0);
                ymm1 = Avx2.Add(offset, ymm1);
                ymm2 = Avx2.Add(offset, ymm2);
                ymm3 = Avx2.Add(offset, ymm3);
                ymm0 = Avx2.ShiftRightLogical(ymm0, 15);
                ymm1 = Avx2.ShiftRightLogical(ymm1, 15);
                ymm2 = Avx2.ShiftRightLogical(ymm2, 15);
                ymm3 = Avx2.ShiftRightLogical(ymm3, 15);
                var ymm4 = Avx2.PackUnsignedSaturate(ymm0.AsInt32(), ymm1.AsInt32());
                var ymm5 = Avx2.PackUnsignedSaturate(ymm2.AsInt32(), ymm3.AsInt32());
                var ymm6 = Avx2.PackUnsignedSaturate(ymm4.AsInt16(), ymm5.AsInt16());
                ymm6 = Avx2.PermuteVar8x32(ymm6.AsInt32(), perm).AsByte();
                Unsafe.As<byte, Vector256<byte>>(ref Unsafe.Add(ref dst, i + 0 * Vector256<byte>.Count)) = ymm6;
                ymm0 = Avx.Min(max, Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref src, i + 4 * 8))).AsUInt32();
                ymm1 = Avx.Min(max, Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref src, i + 5 * 8))).AsUInt32();
                ymm2 = Avx.Min(max, Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref src, i + 6 * 8))).AsUInt32();
                ymm3 = Avx.Min(max, Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref src, i + 7 * 8))).AsUInt32();
                ymm0 = Avx.Max(min, ymm0.AsSingle()).AsUInt32();
                ymm1 = Avx.Max(min, ymm1.AsSingle()).AsUInt32();
                ymm2 = Avx.Max(min, ymm2.AsSingle()).AsUInt32();
                ymm3 = Avx.Max(min, ymm3.AsSingle()).AsUInt32();
                ymm0 = Avx.Add(add, ymm0.AsSingle()).AsUInt32();
                ymm1 = Avx.Add(add, ymm1.AsSingle()).AsUInt32();
                ymm2 = Avx.Add(add, ymm2.AsSingle()).AsUInt32();
                ymm3 = Avx.Add(add, ymm3.AsSingle()).AsUInt32();
                ymm0 = Avx2.And(mask, ymm0);
                ymm1 = Avx2.And(mask, ymm1);
                ymm2 = Avx2.And(mask, ymm2);
                ymm3 = Avx2.And(mask, ymm3);
                ymm0 = Avx2.Add(offset, ymm0);
                ymm1 = Avx2.Add(offset, ymm1);
                ymm2 = Avx2.Add(offset, ymm2);
                ymm3 = Avx2.Add(offset, ymm3);
                ymm0 = Avx2.ShiftRightLogical(ymm0, 15);
                ymm1 = Avx2.ShiftRightLogical(ymm1, 15);
                ymm2 = Avx2.ShiftRightLogical(ymm2, 15);
                ymm3 = Avx2.ShiftRightLogical(ymm3, 15);
                ymm4 = Avx2.PackUnsignedSaturate(ymm0.AsInt32(), ymm1.AsInt32());
                ymm5 = Avx2.PackUnsignedSaturate(ymm2.AsInt32(), ymm3.AsInt32());
                ymm6 = Avx2.PackUnsignedSaturate(ymm4.AsInt16(), ymm5.AsInt16());
                ymm6 = Avx2.PermuteVar8x32(ymm6.AsInt32(), perm).AsByte();
                Unsafe.As<byte, Vector256<byte>>(ref Unsafe.Add(ref dst, i + 1 * Vector256<byte>.Count)) = ymm6.AsByte();
            }
            olen = length - 8 + 1;
            for (; i < olen; i += 8)
            {
                var ymm0 = Avx.Min(max, Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref src, i + 0 * 8))).AsUInt32();
                ymm0 = Avx.Max(min, ymm0.AsSingle()).AsUInt32();
                ymm0 = Avx.Add(add, ymm0.AsSingle()).AsUInt32();
                ymm0 = Avx2.And(mask, ymm0);
                ymm0 = Avx2.Add(offset, ymm0);
                ymm0 = Avx2.ShiftRightLogical(ymm0, 15);
                ymm0 = Avx2.PackUnsignedSaturate(ymm0.AsInt32(), ymm0.AsInt32()).AsUInt32();
                ymm0 = Avx2.PackUnsignedSaturate(ymm0.AsInt16(), ymm0.AsInt16()).AsUInt32();
                ymm0 = Avx2.PermuteVar8x32(ymm0.AsInt32(), perm).AsUInt32();
                Unsafe.As<byte, long>(ref Unsafe.Add(ref dst, i)) = ymm0.AsInt64().GetElement(0);
            }
            for (; i < length; i++)
            {
                var xmm0 = Sse.MinScalar(max.GetLower(), Vector128.CreateScalarUnsafe(Unsafe.Add(ref src, i))).AsUInt32();
                xmm0 = Sse.MaxScalar(min.GetLower(), xmm0.AsSingle()).AsUInt32();
                xmm0 = Sse.AddScalar(add.GetLower(), xmm0.AsSingle()).AsUInt32();
                xmm0 = Sse2.And(mask.GetLower(), xmm0);
                xmm0 = Sse2.Add(offset.GetLower(), xmm0);
                xmm0 = Sse2.ShiftRightLogical(xmm0, 15);
                xmm0 = Sse2.PackUnsignedSaturate(xmm0.AsInt16(), xmm0.AsInt16()).AsUInt32();
                Unsafe.Add(ref dst, i) = xmm0.AsByte().GetElement(0);
            }
        }
#endif
        #endregion

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
                dsmLastOutput = default;
                dsmAccumulator = default;
                readBuffer = default;
            }
            disposedValue = true;
        }
    }
}
