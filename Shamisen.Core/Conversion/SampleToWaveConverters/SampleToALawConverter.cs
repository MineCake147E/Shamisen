using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Shamisen.Utils;

#if NETCOREAPP3_1_OR_GREATER
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

using Shamisen.Utils.Intrinsics;
using Shamisen.Filters;

#endif
#if NET5_0_OR_GREATER
using System.Runtime.Intrinsics.Arm;
#endif

namespace Shamisen.Conversion.SampleToWaveConverters
{
    /// <summary>
    /// Converts Sample to G.711 8bit A-Law PCM.
    /// </summary>
    public sealed class SampleToALawConverter : SampleToWaveConverterBase
    {
        private float[] readBuffer;
        /// <summary>
        /// Initializes a new instance of the <see cref="SampleToALawConverter"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        public SampleToALawConverter(IReadableAudioSource<float, SampleFormat> source)
            : base(source, new WaveFormat(source.Format.SampleRate, 8, source.Format.Channels, AudioEncoding.Alaw))
        {
            readBuffer = new float[1024];
        }

        /// <inheritdoc/>
        protected override int BytesPerSample => sizeof(byte);

        /// <inheritdoc/>
        public override ReadResult Read(Span<byte> buffer)
        {
            var cursor = buffer;
            while (cursor.Length > 0)
            {
                var reader = cursor.Length >= readBuffer.Length ? readBuffer.AsMemory() : readBuffer.AsMemory().Slice(0, cursor.Length);
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
                ConvertSingleToALaw(dest, wrote);
                cursor = cursor.Slice(dest.Length);
                if (u != reader.Length) return buffer.Length - cursor.Length;  //The Source doesn't fill whole reader so return here.
            }
            return buffer.Length;
        }

        /// <summary>
        /// Converts specified <paramref name="value"/> to <see cref="AudioEncoding.Alaw"/> value.
        /// </summary>
        /// <param name="value">The <see cref="float"/> value to convert from.</param>
        /// <returns>The converted <see cref="AudioEncoding.Alaw"/> value.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static byte ConvertSingleToALaw(float value)
        {
            var w = BitConverter.SingleToUInt32Bits(value);
            var s = w & 0x80000000u;
            w &= ~0x80000000u;
            w = MathI.Min(w, 0x3f780000u);
            var q = w < 0x3c000000u;
            w |= s;
            var f = q ? s | 0x3c000000u : 0u;
            w = BitConverter.SingleToUInt32Bits(BitConverter.UInt32BitsToSingle(w) + BitConverter.UInt32BitsToSingle(f));
            w += !q ? 0x00800000 : 0u;
            w -= 0x3c000000u;
            s = w & 0x80000000u;
            w &= ~0x80000000u;
            s >>= 24;
            w >>= 19;
            w |= s;
            return (byte)(w ^ 0xd5);
        }

        /// <summary>
        /// Converts <see cref="float"/> values to <see cref="AudioEncoding.Alaw"/> values.
        /// </summary>
        /// <param name="destination">The place to store resulting <see cref="AudioEncoding.Alaw"/> values.</param>
        /// <param name="source">The <see cref="float"/> values to convert from.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static void ConvertSingleToALaw(Span<byte> destination, ReadOnlySpan<float> source)
        {
            unchecked
            {
                if (Avx2.IsSupported)
                {
                    ProcessAvx2(destination, source);
                }
                ProcessStandardVectorized(destination, source);
            }
        }
        #region X86

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ProcessAvx2(Span<byte> dest, ReadOnlySpan<float> wrote)
        {
            ref var dst = ref MemoryMarshal.GetReference(dest);
            ref var src = ref MemoryMarshal.GetReference(wrote);
            nint i = 0, length = MathI.Min(dest.Length, wrote.Length);
            var olen = length - 4 * 8 + 1;
            var ymm15 = Vector256.Create(0xd5d5_d5d5u);
            var ymm14 = Vector256.Create(0, 4, 1, 5, 2, 6, 3, 7);
            var ymm13 = Vector256.Create(0x3c00_0000u);
            var ymm12 = Vector256.Create(0x8000_0000u);
            var ymm11 = Vector256.Create(0x3f78_0000u);
            var ymm10 = Vector256.Create(0x0080_0000u);
            Vector256<uint> ymm9, ymm8;
            for (; i < olen; i += 4 * 8)
            {
                var ymm0 = Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref src, i + 0 * 8)).AsUInt32();
                var ymm1 = Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref src, i + 1 * 8)).AsUInt32();
                var ymm2 = Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref src, i + 2 * 8)).AsUInt32();
                var ymm3 = Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref src, i + 3 * 8)).AsUInt32();
                var ymm4 = Avx2.AndNot(ymm12, ymm0);
                var ymm5 = Avx2.AndNot(ymm12, ymm1);
                ymm0 = Avx2.And(ymm0, ymm12);
                ymm1 = Avx2.And(ymm1, ymm12);
                ymm4 = Avx2.Min(ymm11, ymm4);
                ymm5 = Avx2.Min(ymm11, ymm5);
                var ymm6 = Avx2.Or(ymm0, ymm4);
                var ymm7 = Avx2.Or(ymm1, ymm5);
                ymm8 = Avx.Compare(ymm4.AsSingle(), ymm13.AsSingle(), FloatComparisonMode.OrderedLessThanNonSignaling).AsUInt32();
                ymm9 = Avx.Compare(ymm5.AsSingle(), ymm13.AsSingle(), FloatComparisonMode.OrderedLessThanNonSignaling).AsUInt32();
                ymm4 = Avx2.And(ymm8, ymm13);
                ymm5 = Avx2.And(ymm9, ymm13);
                ymm8 = Avx2.AndNot(ymm8, ymm10);
                ymm9 = Avx2.AndNot(ymm9, ymm10);
                ymm4 = Avx2.Or(ymm0, ymm4);
                ymm0 = Avx.Add(ymm4.AsSingle(), ymm6.AsSingle()).AsUInt32();
                ymm6 = Avx2.AndNot(ymm12, ymm2);
                ymm5 = Avx2.Or(ymm1, ymm5);
                ymm1 = Avx.Add(ymm5.AsSingle(), ymm7.AsSingle()).AsUInt32();
                ymm7 = Avx2.AndNot(ymm12, ymm3);
                ymm0 = Avx2.Add(ymm0, ymm8);
                ymm2 = Avx2.And(ymm2, ymm12);
                ymm1 = Avx2.Add(ymm1, ymm9);
                ymm3 = Avx2.And(ymm3, ymm12);
                ymm6 = Avx2.Min(ymm11, ymm6);
                ymm7 = Avx2.Min(ymm11, ymm7);
                ymm4 = Avx2.Or(ymm2, ymm6);
                ymm5 = Avx2.Or(ymm3, ymm7);
                ymm8 = Avx.Compare(ymm6.AsSingle(), ymm13.AsSingle(), FloatComparisonMode.OrderedLessThanNonSignaling).AsUInt32();
                ymm9 = Avx.Compare(ymm7.AsSingle(), ymm13.AsSingle(), FloatComparisonMode.OrderedLessThanNonSignaling).AsUInt32();
                ymm6 = Avx2.And(ymm8, ymm13);
                ymm7 = Avx2.And(ymm9, ymm13);
                ymm8 = Avx2.AndNot(ymm8, ymm10);
                ymm9 = Avx2.AndNot(ymm9, ymm10);
                ymm6 = Avx2.Or(ymm2, ymm6);
                ymm2 = Avx.Add(ymm4.AsSingle(), ymm6.AsSingle()).AsUInt32();
                ymm7 = Avx2.Or(ymm3, ymm7);
                ymm3 = Avx.Add(ymm5.AsSingle(), ymm7.AsSingle()).AsUInt32();
                ymm2 = Avx2.Add(ymm2, ymm8);
                ymm3 = Avx2.Add(ymm3, ymm9);
                ymm0 = Avx2.Subtract(ymm0, ymm13);
                ymm1 = Avx2.Subtract(ymm1, ymm13);
                ymm2 = Avx2.Subtract(ymm2, ymm13);
                ymm3 = Avx2.Subtract(ymm3, ymm13);
                ymm4 = Avx2.And(ymm12, ymm0);
                ymm5 = Avx2.And(ymm12, ymm1);
                ymm6 = Avx2.And(ymm12, ymm2);
                ymm7 = Avx2.And(ymm12, ymm3);
                ymm0 = Avx2.AndNot(ymm12, ymm0);
                ymm1 = Avx2.AndNot(ymm12, ymm1);
                ymm2 = Avx2.AndNot(ymm12, ymm2);
                ymm3 = Avx2.AndNot(ymm12, ymm3);
                ymm4 = Avx2.ShiftRightLogical(ymm4, 24);
                ymm5 = Avx2.ShiftRightLogical(ymm5, 24);
                ymm6 = Avx2.ShiftRightLogical(ymm6, 24);
                ymm7 = Avx2.ShiftRightLogical(ymm7, 24);
                ymm0 = Avx2.ShiftRightLogical(ymm0, 19);
                ymm1 = Avx2.ShiftRightLogical(ymm1, 19);
                ymm2 = Avx2.ShiftRightLogical(ymm2, 19);
                ymm3 = Avx2.ShiftRightLogical(ymm3, 19);
                ymm0 = Avx2.Or(ymm4, ymm0);
                ymm1 = Avx2.Or(ymm5, ymm1);
                ymm2 = Avx2.Or(ymm6, ymm2);
                ymm3 = Avx2.Or(ymm7, ymm3);
                ymm0 = Avx2.PackUnsignedSaturate(ymm0.AsInt32(), ymm1.AsInt32()).AsUInt32();
                ymm2 = Avx2.PackUnsignedSaturate(ymm2.AsInt32(), ymm3.AsInt32()).AsUInt32();
                ymm0 = Avx2.PackUnsignedSaturate(ymm0.AsInt16(), ymm2.AsInt16()).AsUInt32();
                ymm0 = Avx2.PermuteVar8x32(ymm0.AsInt32(), ymm14).AsUInt32();
                ymm0 = Avx2.Xor(ymm15, ymm0);
                Unsafe.As<byte, Vector256<byte>>(ref Unsafe.Add(ref dst, i)) = ymm0.AsByte();
            }
            olen = length - 8 + 1;
            for (; i < olen; i += 8)
            {
                var ymm0 = Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref src, i + 0 * 8)).AsUInt32();
                var ymm4 = Avx2.AndNot(ymm12, ymm0);
                ymm0 = Avx2.And(ymm0, ymm12);
                ymm4 = Avx2.Min(ymm11, ymm4);
                var ymm6 = Avx2.Or(ymm0, ymm4);
                ymm8 = Avx.Compare(ymm4.AsSingle(), ymm13.AsSingle(), FloatComparisonMode.OrderedLessThanNonSignaling).AsUInt32();
                ymm4 = Avx2.And(ymm8, ymm13);
                ymm8 = Avx2.AndNot(ymm8, ymm10);
                ymm4 = Avx2.Or(ymm0, ymm4);
                ymm0 = Avx.Add(ymm4.AsSingle(), ymm6.AsSingle()).AsUInt32();
                ymm0 = Avx2.Add(ymm0, ymm8);
                ymm0 = Avx2.Subtract(ymm0, ymm13);
                ymm4 = Avx2.And(ymm12, ymm0);
                ymm0 = Avx2.AndNot(ymm12, ymm0);
                ymm4 = Avx2.ShiftRightLogical(ymm4, 24);
                ymm0 = Avx2.ShiftRightLogical(ymm0, 19);
                ymm0 = Avx2.Or(ymm4, ymm0);
                ymm0 = Avx2.PackUnsignedSaturate(ymm0.AsInt32(), ymm0.AsInt32()).AsUInt32();
                ymm0 = Avx2.PackUnsignedSaturate(ymm0.AsInt16(), ymm0.AsInt16()).AsUInt32();
                ymm0 = Avx2.PermuteVar8x32(ymm0.AsInt32(), ymm14).AsUInt32();
                ymm0 = Avx2.Xor(ymm15, ymm0);
                Unsafe.As<byte, long>(ref Unsafe.Add(ref dst, i)) = ymm0.AsInt64().GetElement(0);
            }
            for (; i < length; i++)
            {
                var xmm0 = Vector128.CreateScalarUnsafe(Unsafe.Add(ref src, i)).AsUInt32();
                var xmm4 = Sse2.AndNot(ymm12.GetLower(), xmm0);
                xmm0 = Sse2.And(xmm0, ymm12.GetLower());
                xmm4 = Sse41.Min(ymm11.GetLower(), xmm4);
                var xmm6 = Sse2.Or(xmm0, xmm4);
                var xmm8 = Avx.CompareScalar(xmm4.AsSingle(), ymm13.GetLower().AsSingle(), FloatComparisonMode.OrderedLessThanNonSignaling).AsUInt32();
                xmm4 = Sse2.And(xmm8, ymm13.GetLower());
                xmm8 = Sse2.AndNot(xmm8, ymm10.GetLower());
                xmm4 = Sse2.Or(xmm0, xmm4);
                xmm0 = Sse.AddScalar(xmm4.AsSingle(), xmm6.AsSingle()).AsUInt32();
                xmm0 = Sse2.Add(xmm0, xmm8);
                xmm0 = Sse2.Subtract(xmm0, ymm13.GetLower());
                xmm4 = Sse2.And(ymm12.GetLower(), xmm0);
                xmm0 = Sse2.AndNot(ymm12.GetLower(), xmm0);
                xmm4 = Sse2.ShiftRightLogical(xmm4, 24);
                xmm0 = Sse2.ShiftRightLogical(xmm0, 19);
                xmm0 = Sse2.Or(xmm4, xmm0);
                xmm0 = Sse41.PackUnsignedSaturate(xmm0.AsInt32(), xmm0.AsInt32()).AsUInt32();
                xmm0 = Sse2.PackUnsignedSaturate(xmm0.AsInt16(), xmm0.AsInt16()).AsUInt32();
                xmm0 = Sse2.Xor(ymm15.GetLower(), xmm0);
                Unsafe.Add(ref dst, i) = xmm0.AsByte().GetElement(0);
            }
        }
        #endregion
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ProcessStandardVectorized(Span<byte> dest, ReadOnlySpan<float> wrote)
        {
            var v15_nb = new Vector<uint>(0xd5d5_d5d5u).AsByte();
            var v13_ns = new Vector<uint>(0x3c00_0000u);
            var v12_ns = new Vector<uint>(0x8000_0000u);
            var v11_ns = new Vector<uint>(0x3f78_0000u);
            var v10_ns = new Vector<uint>(0x0080_0000u);
            Vector<uint> v9_ns, v8_ns;
            ref var dst = ref MemoryMarshal.GetReference(dest);
            ref var src = ref MemoryMarshal.GetReference(wrote);
            nint i = 0, length = MathI.Min(dest.Length, wrote.Length);
            var olen = length - Vector<float>.Count * 4 + 1;
            for (; i < olen; i += Vector<float>.Count * 4)
            {
                var v0_ns = Unsafe.As<float, Vector<uint>>(ref Unsafe.Add(ref src, i + 0 * Vector<float>.Count));
                var v1_ns = Unsafe.As<float, Vector<uint>>(ref Unsafe.Add(ref src, i + 1 * Vector<float>.Count));
                var v2_ns = Unsafe.As<float, Vector<uint>>(ref Unsafe.Add(ref src, i + 2 * Vector<float>.Count));
                var v3_ns = Unsafe.As<float, Vector<uint>>(ref Unsafe.Add(ref src, i + 3 * Vector<float>.Count));
                var v4_ns = VectorUtils.AndNot(v12_ns, v0_ns);
                var v5_ns = VectorUtils.AndNot(v12_ns, v1_ns);
                v0_ns = Vector.BitwiseAnd(v0_ns, v12_ns);
                v1_ns = Vector.BitwiseAnd(v1_ns, v12_ns);
                v4_ns = Vector.Min(v11_ns, v4_ns);
                v5_ns = Vector.Min(v11_ns, v5_ns);
                var v6_ns = Vector.BitwiseOr(v0_ns, v4_ns);
                var v7_ns = Vector.BitwiseOr(v1_ns, v5_ns);
                v8_ns = Vector.LessThan(v4_ns.AsSingle(), v13_ns.AsSingle()).AsUInt32();
                v9_ns = Vector.LessThan(v5_ns.AsSingle(), v13_ns.AsSingle()).AsUInt32();
                v4_ns = Vector.BitwiseAnd(v8_ns, v13_ns);
                v5_ns = Vector.BitwiseAnd(v9_ns, v13_ns);
                v8_ns = VectorUtils.AndNot(v8_ns, v10_ns);
                v9_ns = VectorUtils.AndNot(v9_ns, v10_ns);
                v4_ns = Vector.BitwiseOr(v0_ns, v4_ns);
                v0_ns = Vector.Add(v4_ns.AsSingle(), v6_ns.AsSingle()).AsUInt32();
                v6_ns = VectorUtils.AndNot(v12_ns, v2_ns);
                v5_ns = Vector.BitwiseOr(v1_ns, v5_ns);
                v1_ns = Vector.Add(v5_ns.AsSingle(), v7_ns.AsSingle()).AsUInt32();
                v7_ns = VectorUtils.AndNot(v12_ns, v3_ns);
                v0_ns = Vector.Add(v0_ns, v8_ns);
                v2_ns = Vector.BitwiseAnd(v2_ns, v12_ns);
                v1_ns = Vector.Add(v1_ns, v9_ns);
                v3_ns = Vector.BitwiseAnd(v3_ns, v12_ns);
                v6_ns = Vector.Min(v11_ns, v6_ns);
                v7_ns = Vector.Min(v11_ns, v7_ns);
                v4_ns = Vector.BitwiseOr(v2_ns, v6_ns);
                v5_ns = Vector.BitwiseOr(v3_ns, v7_ns);
                v8_ns = Vector.LessThan(v6_ns.AsSingle(), v13_ns.AsSingle()).AsUInt32();
                v9_ns = Vector.LessThan(v7_ns.AsSingle(), v13_ns.AsSingle()).AsUInt32();
                v6_ns = Vector.BitwiseAnd(v8_ns, v13_ns);
                v7_ns = Vector.BitwiseAnd(v9_ns, v13_ns);
                v8_ns = VectorUtils.AndNot(v8_ns, v10_ns);
                v9_ns = VectorUtils.AndNot(v9_ns, v10_ns);
                v6_ns = Vector.BitwiseOr(v2_ns, v6_ns);
                v2_ns = Vector.Add(v4_ns.AsSingle(), v6_ns.AsSingle()).AsUInt32();
                v7_ns = Vector.BitwiseOr(v3_ns, v7_ns);
                v3_ns = Vector.Add(v5_ns.AsSingle(), v7_ns.AsSingle()).AsUInt32();
                v2_ns = Vector.Add(v2_ns, v8_ns);
                v3_ns = Vector.Add(v3_ns, v9_ns);
                v0_ns = Vector.Subtract(v0_ns, v13_ns);
                v1_ns = Vector.Subtract(v1_ns, v13_ns);
                v2_ns = Vector.Subtract(v2_ns, v13_ns);
                v3_ns = Vector.Subtract(v3_ns, v13_ns);
                v4_ns = Vector.BitwiseAnd(v12_ns, v0_ns);
                v5_ns = Vector.BitwiseAnd(v12_ns, v1_ns);
                v6_ns = Vector.BitwiseAnd(v12_ns, v2_ns);
                v7_ns = Vector.BitwiseAnd(v12_ns, v3_ns);
                v0_ns = VectorUtils.AndNot(v12_ns, v0_ns);
                v1_ns = VectorUtils.AndNot(v12_ns, v1_ns);
                v2_ns = VectorUtils.AndNot(v12_ns, v2_ns);
                v3_ns = VectorUtils.AndNot(v12_ns, v3_ns);
                v4_ns = Vector.ShiftRightLogical(v4_ns, 24);
                v5_ns = Vector.ShiftRightLogical(v5_ns, 24);
                v6_ns = Vector.ShiftRightLogical(v6_ns, 24);
                v7_ns = Vector.ShiftRightLogical(v7_ns, 24);
                v0_ns = Vector.ShiftRightLogical(v0_ns, 19);
                v1_ns = Vector.ShiftRightLogical(v1_ns, 19);
                v2_ns = Vector.ShiftRightLogical(v2_ns, 19);
                v3_ns = Vector.ShiftRightLogical(v3_ns, 19);
                v0_ns = Vector.BitwiseOr(v4_ns, v0_ns);
                v1_ns = Vector.BitwiseOr(v5_ns, v1_ns);
                v2_ns = Vector.BitwiseOr(v6_ns, v2_ns);
                v3_ns = Vector.BitwiseOr(v7_ns, v3_ns);
                var v0_nh = Vector.Narrow(v0_ns, v1_ns);
                var v2_nh = Vector.Narrow(v2_ns, v3_ns);
                var v0_nb = Vector.Narrow(v0_nh, v2_nh);
                v0_nb = Vector.Xor(v0_nb, v15_nb);
                Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref dst, i + 0 * Vector<byte>.Count)) = v0_nb;
            }
            for (; i < length; i++)
            {
                var v = Unsafe.Add(ref src, i);
                Unsafe.Add(ref dst, i) = ConvertSingleToALaw(v);
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
