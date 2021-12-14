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
    /// Converts Sample to G.711 8bit μ-Law PCM.
    /// </summary>
    public sealed class SampleToMuLawConverter : SampleToWaveConverterBase
    {
        private float[] readBuffer;
        /// <summary>
        /// Initializes a new instance of the <see cref="SampleToMuLawConverter"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        public SampleToMuLawConverter(IReadableAudioSource<float, SampleFormat> source)
            : base(source, new WaveFormat(source.Format.SampleRate, 8, source.Format.Channels, AudioEncoding.Mulaw))
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
                ProcessNormal(wrote, dest);
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
                    ProcessAvx2(wrote, dest);
                }
#endif
                ProcessStandardVectorized(wrote, dest);
            }
        }
        #region X86
#if NETCOREAPP3_1_OR_GREATER

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ProcessAvx2(Span<float> wrote, Span<byte> dest)
        {
            var ymm15 = Vector256.Create(0x7fff_ffffu);
            var ymm14 = Vector256.Create(0.0040283203125f);
            var ymm13 = Vector256.Create(0.00390625f).AsUInt32();
            var ymm12 = Vector256.Create(0x03f8_0000u);
            var ymm11 = Vector256.Create(~0u);
            var perm = Vector256.Create(0, 4, 1, 5, 2, 6, 3, 7);
            ref var dst = ref MemoryMarshal.GetReference(dest);
            ref var src = ref MemoryMarshal.GetReference(wrote);
            nint i = 0, length = MathI.Min(dest.Length, wrote.Length);
            var olen = length - 4 * 8 + 1;
            for (; i < olen; i += 4 * 8)
            {
                var ymm0 = Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref src, i + 0 * 8)).AsUInt32();
                var ymm1 = Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref src, i + 1 * 8)).AsUInt32();
                var ymm2 = Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref src, i + 2 * 8)).AsUInt32();
                var ymm3 = Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref src, i + 3 * 8)).AsUInt32();
                var ymm4 = Avx2.And(ymm15, ymm0);
                ymm4 = Avx.Add(ymm4.AsSingle(), ymm14).AsUInt32();
                var ymm5 = Avx2.And(ymm15, ymm1);
                ymm5 = Avx.Add(ymm5.AsSingle(), ymm14).AsUInt32();
                var ymm6 = Avx2.And(ymm15, ymm2);
                ymm6 = Avx.Add(ymm6.AsSingle(), ymm14).AsUInt32();
                var ymm7 = Avx2.And(ymm15, ymm3);
                ymm7 = Avx.Add(ymm7.AsSingle(), ymm14).AsUInt32();
                ymm0 = Avx2.AndNot(ymm15, ymm0);
                ymm1 = Avx2.AndNot(ymm15, ymm1);
                ymm2 = Avx2.AndNot(ymm15, ymm2);
                ymm3 = Avx2.AndNot(ymm15, ymm3);
                ymm4 = Avx2.Subtract(ymm4, ymm13);
                ymm5 = Avx2.Subtract(ymm5, ymm13);
                ymm6 = Avx2.Subtract(ymm6, ymm13);
                ymm7 = Avx2.Subtract(ymm7, ymm13);
                ymm0 = Avx2.ShiftRightLogical(ymm0, 24);
                ymm1 = Avx2.ShiftRightLogical(ymm1, 24);
                ymm2 = Avx2.ShiftRightLogical(ymm2, 24);
                ymm3 = Avx2.ShiftRightLogical(ymm3, 24);
                ymm4 = Avx2.Min(ymm4, ymm12);
                ymm5 = Avx2.Min(ymm5, ymm12);
                ymm6 = Avx2.Min(ymm6, ymm12);
                ymm7 = Avx2.Min(ymm7, ymm12);
                ymm4 = Avx2.ShiftRightLogical(ymm4, 19);
                ymm5 = Avx2.ShiftRightLogical(ymm5, 19);
                ymm6 = Avx2.ShiftRightLogical(ymm6, 19);
                ymm7 = Avx2.ShiftRightLogical(ymm7, 19);
                ymm0 = Avx2.Xor(ymm4, ymm0);
                ymm1 = Avx2.Xor(ymm5, ymm1);
                ymm2 = Avx2.Xor(ymm6, ymm2);
                ymm3 = Avx2.Xor(ymm7, ymm3);
                ymm0 = Avx2.PackUnsignedSaturate(ymm0.AsInt32(), ymm1.AsInt32()).AsUInt32();
                ymm2 = Avx2.PackUnsignedSaturate(ymm2.AsInt32(), ymm3.AsInt32()).AsUInt32();
                ymm0 = Avx2.PackUnsignedSaturate(ymm0.AsInt16(), ymm2.AsInt16()).AsUInt32();
                ymm0 = Avx2.PermuteVar8x32(ymm0.AsInt32(), perm).AsUInt32();
                ymm0 = Avx2.Xor(ymm11, ymm0);
                Unsafe.As<byte, Vector256<byte>>(ref Unsafe.Add(ref dst, i)) = ymm0.AsByte();
            }
            olen = length - 8 + 1;
            for (; i < olen; i += 8)
            {
                var ymm0 = Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref src, i + 0 * 8)).AsUInt32();
                var ymm4 = Avx2.And(ymm15, ymm0);
                ymm4 = Avx.Add(ymm4.AsSingle(), ymm14).AsUInt32();
                ymm0 = Avx2.AndNot(ymm15, ymm0);
                ymm4 = Avx2.Subtract(ymm4, ymm13);
                ymm0 = Avx2.ShiftRightLogical(ymm0, 24);
                ymm4 = Avx2.Min(ymm4, ymm12);
                ymm4 = Avx2.ShiftRightLogical(ymm4, 19);
                ymm0 = Avx2.Xor(ymm4, ymm0);
                ymm0 = Avx2.PackUnsignedSaturate(ymm0.AsInt32(), ymm0.AsInt32()).AsUInt32();
                ymm0 = Avx2.PackUnsignedSaturate(ymm0.AsInt16(), ymm0.AsInt16()).AsUInt32();
                ymm0 = Avx2.PermuteVar8x32(ymm0.AsInt32(), perm).AsUInt32();
                ymm0 = Avx2.Xor(ymm11, ymm0);
                Unsafe.As<byte, long>(ref Unsafe.Add(ref dst, i)) = ymm0.AsInt64().GetElement(0);
            }
            for (; i < length; i++)
            {
                var xmm0 = Vector128.CreateScalarUnsafe(Unsafe.Add(ref src, i)).AsUInt32();
                var xmm4 = Sse2.And(ymm15.GetLower(), xmm0);
                xmm4 = Sse.AddScalar(xmm4.AsSingle(), ymm14.GetLower()).AsUInt32();
                xmm0 = Sse2.AndNot(ymm15.GetLower(), xmm0);
                xmm4 = Sse2.Subtract(xmm4, ymm13.GetLower());
                xmm0 = Sse2.ShiftRightLogical(xmm0, 24);
                xmm4 = Sse41.Min(xmm4, ymm12.GetLower());
                xmm4 = Sse2.ShiftRightLogical(xmm4, 19);
                xmm0 = Sse2.Xor(xmm4, xmm0);
                xmm0 = Sse41.PackUnsignedSaturate(xmm0.AsInt32(), xmm0.AsInt32()).AsUInt32();
                xmm0 = Sse2.PackUnsignedSaturate(xmm0.AsInt16(), xmm0.AsInt16()).AsUInt32();
                xmm0 = Sse2.Xor(ymm11.GetLower(), xmm0);
                Unsafe.Add(ref dst, i) = xmm0.AsByte().GetElement(0);
            }
        }
#endif
        #endregion
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ProcessStandardVectorized(Span<float> wrote, Span<byte> dest)
        {
            var v15_ns = new Vector<uint>(0x7fff_ffffu);
            var v14_ns = new Vector<float>(0.0040283203125f);
            var v13_ns = new Vector<float>(0.00390625f).AsUInt32();
            var v12_ns = new Vector<uint>(0x03f8_0000u);
            var v11_nb = new Vector<uint>(~0u).AsByte();
            var v10_ns = new Vector<int>(0);
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
                var v4_ns = Vector.BitwiseAnd(v15_ns, v0_ns);
                v4_ns = Vector.Add(v4_ns.AsSingle(), v14_ns).AsUInt32();
                var v5_ns = Vector.BitwiseAnd(v15_ns, v1_ns);
                v5_ns = Vector.Add(v5_ns.AsSingle(), v14_ns).AsUInt32();
                var v6_ns = Vector.BitwiseAnd(v15_ns, v2_ns);
                v6_ns = Vector.Add(v6_ns.AsSingle(), v14_ns).AsUInt32();
                var v7_ns = Vector.BitwiseAnd(v15_ns, v3_ns);
                v7_ns = Vector.Add(v7_ns.AsSingle(), v14_ns).AsUInt32();
                v0_ns = VectorUtils.AndNot(v15_ns, v0_ns);  //Vector.AndNot()'s parameters were reversed compared to Avx2.AndNot()!
                v1_ns = VectorUtils.AndNot(v15_ns, v1_ns);
                v2_ns = VectorUtils.AndNot(v15_ns, v2_ns);
                v3_ns = VectorUtils.AndNot(v15_ns, v3_ns);
                v4_ns = Vector.Subtract(v4_ns, v13_ns);
                v5_ns = Vector.Subtract(v5_ns, v13_ns);
                v6_ns = Vector.Subtract(v6_ns, v13_ns);
                v7_ns = Vector.Subtract(v7_ns, v13_ns);
                v0_ns = VectorUtils.ShiftRightLogical(v0_ns, 24);
                v1_ns = VectorUtils.ShiftRightLogical(v1_ns, 24);
                v2_ns = VectorUtils.ShiftRightLogical(v2_ns, 24);
                v3_ns = VectorUtils.ShiftRightLogical(v3_ns, 24);
                v4_ns = Vector.Min(v4_ns, v12_ns);
                v5_ns = Vector.Min(v5_ns, v12_ns);
                v6_ns = Vector.Min(v6_ns, v12_ns);
                v7_ns = Vector.Min(v7_ns, v12_ns);
                v4_ns = VectorUtils.ShiftRightLogical(v4_ns, 19);
                v5_ns = VectorUtils.ShiftRightLogical(v5_ns, 19);
                v6_ns = VectorUtils.ShiftRightLogical(v6_ns, 19);
                v7_ns = VectorUtils.ShiftRightLogical(v7_ns, 19);
                v4_ns = Vector.Max(v4_ns.AsInt32(), v10_ns).AsUInt32();
                v5_ns = Vector.Max(v5_ns.AsInt32(), v10_ns).AsUInt32();
                v6_ns = Vector.Max(v6_ns.AsInt32(), v10_ns).AsUInt32();
                v7_ns = Vector.Max(v7_ns.AsInt32(), v10_ns).AsUInt32();
                v0_ns = Vector.Xor(v4_ns, v0_ns);
                v1_ns = Vector.Xor(v5_ns, v1_ns);
                v2_ns = Vector.Xor(v6_ns, v2_ns);
                v3_ns = Vector.Xor(v7_ns, v3_ns);
                var v0_nh = Vector.Narrow(v0_ns, v1_ns);
                var v2_nh = Vector.Narrow(v2_ns, v3_ns);
                var v0_nb = Vector.Narrow(v0_nh, v2_nh);
                v0_nb = Vector.Xor(v0_nb, v11_nb);
                Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref dst, i + 0 * Vector<byte>.Count)) = v0_nb;
            }
            for (; i < length; i++)
            {
                var v = Unsafe.Add(ref src, i);
                Unsafe.Add(ref dst, i) = ConvertSingleToMuLaw(v);
            }
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static byte ConvertSingleToMuLaw(float value)
        {
            var a = Math.Abs(value);
            var u = BinaryExtensions.SingleToUInt32Bits(value) & 0x8000_0000u;
            a += 0.0040283203125f;
            u >>= 24;
            a = FastMath.MaxUnsignedInputs(a, 0.00390625f);
            a = FastMath.MinUnsignedInputs(a, 0.96875f);
            var q = BinaryExtensions.SingleToUInt32Bits(a);
            q -= 0x3b80_0000u;
            q >>= 19;
            q |= u;
            return (byte)~q;
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
