using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using Shamisen.Data;
using Shamisen.Utils;

namespace Shamisen.TestUtils
{
    public static class ErrorUtils
    {
        public delegate void GenerateDelegate<T>(Span<T> values);
        public static ErrorEvaluationResult<float> EvaluateErrors(float start, float end, GenerateDelegate<float> expected, GenerateDelegate<float> actual)
        {
            if (BitConverter.SingleToUInt32Bits(MathI.Xor(start, end)) >> 31 > 0)
            {
                throw new ArgumentException("", nameof(end));
            }
            var s = BitConverter.SingleToUInt32Bits(start);
            var e = BitConverter.SingleToUInt32Bits(end);
            if (e < s) (s, e) = (e, s);
            e++;
            var length = e - s + 1;
            var arrayLength = (int)MathI.Min(65536, length);
            var currentStart = s;
            using var earr = new PooledArray<float>(arrayLength);
            using var aarr = new PooledArray<float>(arrayLength);
            using var darr = new PooledArray<uint>(arrayLength);
            var exp = earr.Span;
            var act = aarr.Span;
            var ulp = darr.Span;
            (uint maxat, uint maxdiff) maxd = (BitConverter.SingleToUInt32Bits(float.NaN), 0);
            var adiff = float.NegativeInfinity;
            ulong zulp = 0, tulp = 0;
            do
            {
                GenerateIndexValuedArraySingle(act, BitConverter.UInt32BitsToSingle(currentStart));
                act.CopyTo(exp);
                ulp.FastFill(0);
                var cexp = exp;
                var cact = act;
                var culp = ulp;
                var v = e - currentStart;
                if (v < cexp.Length)
                {
                    cexp = cexp.SliceWhile((int)v);
                    cact = cact.SliceWhile((int)v);
                    culp = culp.SliceWhile((int)v);
                }
                expected(cexp);
                actual(cact);
                GenerateUlpDifferences(culp, cexp, cact);
                var (maxat, maxdiff) = FindMaximumError(culp);
                if (maxd.maxdiff < maxdiff)
                {
                    maxd = (maxat + currentStart, maxdiff);
                    adiff = cexp[(int)maxat] - cact[(int)maxat];
                }
                var res = AccumulateDifferences(culp);
                zulp += res.zulp;
                tulp += res.tulp;
                currentStart += (uint)cexp.Length;
            } while (currentStart < e);
            return new ErrorEvaluationResult<float>(new(BitConverter.UInt32BitsToSingle(maxd.maxat), maxd.maxdiff, adiff), zulp, Memory<ulong>.Empty, (double)tulp / length, tulp, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        internal static void GenerateUlpDifferences(Span<uint> dst, ReadOnlySpan<float> src0, ReadOnlySpan<float> src1)
        {
            nint i, length = MathI.Min(dst.Length, MathI.Min(src0.Length, src1.Length));
            ref var rsA = ref MemoryMarshal.GetReference(src0);
            ref var rsB = ref MemoryMarshal.GetReference(src1);
            ref var rD = ref MemoryMarshal.GetReference(dst);
            var v15_ns = new Vector<int>(int.MaxValue);
            var olen = length - 4 * Vector<float>.Count + 1;
            for (i = 0; i < olen; i += 4 * Vector<float>.Count)
            {
                ref var x9 = ref Unsafe.Add(ref rsB, i);
                ref var x10 = ref Unsafe.Add(ref rD, i);
                var v0_ns = Unsafe.As<float, Vector<int>>(ref Unsafe.Add(ref rsA, i + 0 * Vector<float>.Count));
                var v1_ns = Unsafe.As<float, Vector<int>>(ref Unsafe.Add(ref rsA, i + 1 * Vector<float>.Count));
                var v2_ns = Unsafe.As<float, Vector<int>>(ref Unsafe.Add(ref rsA, i + 2 * Vector<float>.Count));
                var v3_ns = Unsafe.As<float, Vector<int>>(ref Unsafe.Add(ref rsA, i + 3 * Vector<float>.Count));
                var v4_ns = Unsafe.As<float, Vector<int>>(ref Unsafe.Add(ref x9, 0 * Vector<float>.Count));
                var v5_ns = Unsafe.As<float, Vector<int>>(ref Unsafe.Add(ref x9, 1 * Vector<float>.Count));
                var v6_ns = Unsafe.As<float, Vector<int>>(ref Unsafe.Add(ref x9, 2 * Vector<float>.Count));
                var v7_ns = Unsafe.As<float, Vector<int>>(ref Unsafe.Add(ref x9, 3 * Vector<float>.Count));
                var v8_ns = Vector.Xor(v0_ns, v15_ns);
                var v9_ns = Vector.Xor(v1_ns, v15_ns);
                var v10_ns = Vector.Xor(v2_ns, v15_ns);
                var v11_ns = Vector.Xor(v3_ns, v15_ns);
                v0_ns = VectorUtils.Blend(v0_ns, v0_ns, v8_ns);
                v1_ns = VectorUtils.Blend(v1_ns, v1_ns, v9_ns);
                v2_ns = VectorUtils.Blend(v2_ns, v2_ns, v10_ns);
                v3_ns = VectorUtils.Blend(v3_ns, v3_ns, v11_ns);
                v8_ns = Vector.Xor(v4_ns, v15_ns);
                v9_ns = Vector.Xor(v5_ns, v15_ns);
                v10_ns = Vector.Xor(v6_ns, v15_ns);
                v11_ns = Vector.Xor(v7_ns, v15_ns);
                v4_ns = VectorUtils.Blend(v4_ns, v4_ns, v8_ns);
                v0_ns -= v4_ns;
                v5_ns = VectorUtils.Blend(v5_ns, v5_ns, v9_ns);
                v1_ns -= v5_ns;
                v6_ns = VectorUtils.Blend(v6_ns, v6_ns, v10_ns);
                v2_ns -= v6_ns;
                v7_ns = VectorUtils.Blend(v7_ns, v7_ns, v11_ns);
                v3_ns -= v7_ns;
                v0_ns = Vector.Abs(v0_ns);
                v1_ns = Vector.Abs(v1_ns);
                v2_ns = Vector.Abs(v2_ns);
                v3_ns = Vector.Abs(v3_ns);
                Unsafe.As<uint, Vector<int>>(ref Unsafe.Add(ref x10, 0 * Vector<uint>.Count)) = v0_ns;
                Unsafe.As<uint, Vector<int>>(ref Unsafe.Add(ref x10, 1 * Vector<uint>.Count)) = v1_ns;
                Unsafe.As<uint, Vector<int>>(ref Unsafe.Add(ref x10, 2 * Vector<uint>.Count)) = v2_ns;
                Unsafe.As<uint, Vector<int>>(ref Unsafe.Add(ref x10, 3 * Vector<uint>.Count)) = v3_ns;
            }
            olen = length - Vector<float>.Count + 1;
            for (; i < olen; i += Vector<float>.Count)
            {
                ref var x9 = ref Unsafe.Add(ref rsB, i);
                ref var x10 = ref Unsafe.Add(ref rD, i);
                var v0_ns = Unsafe.As<float, Vector<int>>(ref Unsafe.Add(ref rsA, i));
                var v4_ns = Unsafe.As<float, Vector<int>>(ref x9);
                var v8_ns = Vector.Xor(v0_ns, v15_ns);
                var v9_ns = Vector.Xor(v4_ns, v15_ns);
                v0_ns = VectorUtils.Blend(v0_ns, v0_ns, v8_ns);
                v4_ns = VectorUtils.Blend(v4_ns, v4_ns, v9_ns);
                v0_ns -= v4_ns;
                v0_ns = Vector.Abs(v0_ns);
                Unsafe.As<uint, Vector<int>>(ref x10) = v0_ns;
            }
            for (; i < length; i++)
            {
                ref var x9 = ref Unsafe.Add(ref rsB, i);
                ref var x10 = ref Unsafe.Add(ref rD, i);
                var w0 = Unsafe.As<float, int>(ref Unsafe.Add(ref rsA, i));
                var w1 = Unsafe.As<float, int>(ref x9);
                var w2 = (w0 >> 31) ^ int.MaxValue;
                var w3 = (w1 >> 31) ^ int.MaxValue;
                w0 ^= w2;
                w1 ^= w3;
                w0 -= w1;
                x10 = MathI.Abs(w0);
            }
        }

        public static uint CalculateAbsoluteUlpDifference(float expected, float actual)
        {
            var w0 = BitConverter.SingleToInt32Bits(expected);
            var w1 = BitConverter.SingleToInt32Bits(actual);
            var w2 = (w0 >> 31) ^ int.MaxValue;
            var w3 = (w1 >> 31) ^ int.MaxValue;
            w0 ^= w2;
            w1 ^= w3;
            w0 -= w1;
            return MathI.Abs(w0);
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]

        internal static (ulong zulp, ulong tulp) AccumulateDifferences(ReadOnlySpan<uint> src)
        {
            if (Avx2.IsSupported)
            {
                return AccumulateDifferencesAvx2(src);
            }
            return AccumulateDifferencesFallback(src);
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        internal static (ulong zulp, ulong tulp) AccumulateDifferencesAvx2(ReadOnlySpan<uint> src)
        {
            nint i, length = src.Length;
            ref var rsi = ref MemoryMarshal.GetReference(src);
            var ymm15 = Vector256<ulong>.Zero;
            var ymm14 = Vector256<ulong>.Zero;
            var ymm13 = Vector256<ulong>.Zero;
            var ymm12 = Vector256<ulong>.Zero;
            var ymm11 = Vector256<ulong>.Zero;
            var ymm10 = Vector256<ulong>.Zero;
            var ymm9 = Vector256<ulong>.Zero;
            var ymm8 = Vector256<ulong>.Zero;
            var ymm7 = Vector256<ulong>.Zero;
            var olen = length - 8 * Vector128<uint>.Count + 1;
            for (i = 0; i < olen; i += 8 * Vector128<uint>.Count)
            {
                var ymm0 = Avx2.ConvertToVector256Int64(Unsafe.As<uint, Vector128<uint>>(ref Unsafe.Add(ref rsi, i + 0 * Vector128<uint>.Count))).AsUInt64();
                ymm8 = Avx2.Add(ymm8, ymm0);
                var ymm1 = Avx2.ConvertToVector256Int64(Unsafe.As<uint, Vector128<uint>>(ref Unsafe.Add(ref rsi, i + 1 * Vector128<uint>.Count))).AsUInt64();
                ymm9 = Avx2.Add(ymm9, ymm1);
                var ymm2 = Avx2.ConvertToVector256Int64(Unsafe.As<uint, Vector128<uint>>(ref Unsafe.Add(ref rsi, i + 2 * Vector128<uint>.Count))).AsUInt64();
                ymm10 = Avx2.Add(ymm10, ymm2);
                var ymm3 = Avx2.ConvertToVector256Int64(Unsafe.As<uint, Vector128<uint>>(ref Unsafe.Add(ref rsi, i + 3 * Vector128<uint>.Count))).AsUInt64();
                ymm11 = Avx2.Add(ymm11, ymm3);
                ymm0 = Avx2.CompareEqual(ymm0, ymm7);
                ymm12 = Avx2.Subtract(ymm12, ymm0);
                ymm1 = Avx2.CompareEqual(ymm1, ymm7);
                ymm13 = Avx2.Subtract(ymm13, ymm1);
                ymm2 = Avx2.CompareEqual(ymm2, ymm7);
                ymm14 = Avx2.Subtract(ymm14, ymm2);
                ymm3 = Avx2.CompareEqual(ymm3, ymm7);
                ymm15 = Avx2.Subtract(ymm15, ymm3);
                ymm0 = Avx2.ConvertToVector256Int64(Unsafe.As<uint, Vector128<uint>>(ref Unsafe.Add(ref rsi, i + 4 * Vector128<uint>.Count))).AsUInt64();
                ymm8 = Avx2.Add(ymm8, ymm0);
                ymm1 = Avx2.ConvertToVector256Int64(Unsafe.As<uint, Vector128<uint>>(ref Unsafe.Add(ref rsi, i + 5 * Vector128<uint>.Count))).AsUInt64();
                ymm9 = Avx2.Add(ymm9, ymm1);
                ymm2 = Avx2.ConvertToVector256Int64(Unsafe.As<uint, Vector128<uint>>(ref Unsafe.Add(ref rsi, i + 6 * Vector128<uint>.Count))).AsUInt64();
                ymm10 = Avx2.Add(ymm10, ymm2);
                ymm3 = Avx2.ConvertToVector256Int64(Unsafe.As<uint, Vector128<uint>>(ref Unsafe.Add(ref rsi, i + 7 * Vector128<uint>.Count))).AsUInt64();
                ymm11 = Avx2.Add(ymm11, ymm3);
                ymm0 = Avx2.CompareEqual(ymm0, ymm7);
                ymm12 = Avx2.Subtract(ymm12, ymm0);
                ymm1 = Avx2.CompareEqual(ymm1, ymm7);
                ymm13 = Avx2.Subtract(ymm13, ymm1);
                ymm2 = Avx2.CompareEqual(ymm2, ymm7);
                ymm14 = Avx2.Subtract(ymm14, ymm2);
                ymm3 = Avx2.CompareEqual(ymm3, ymm7);
                ymm15 = Avx2.Subtract(ymm15, ymm3);
            }
            ymm12 = Avx2.Add(ymm12, ymm13);
            ymm13 = Avx2.Add(ymm14, ymm15);
            ymm8 = Avx2.Add(ymm8, ymm9);
            ymm9 = Avx2.Add(ymm10, ymm11);
            olen = length - 2 * Vector128<uint>.Count + 1;
            for (; i < olen; i += 2 * Vector128<uint>.Count)
            {
                var ymm0 = Avx2.ConvertToVector256Int64(Unsafe.As<uint, Vector128<uint>>(ref Unsafe.Add(ref rsi, i + 0 * Vector128<uint>.Count))).AsUInt64();
                ymm8 = Avx2.Add(ymm8, ymm0);
                var ymm1 = Avx2.ConvertToVector256Int64(Unsafe.As<uint, Vector128<uint>>(ref Unsafe.Add(ref rsi, i + 1 * Vector128<uint>.Count))).AsUInt64();
                ymm9 = Avx2.Add(ymm9, ymm1);
                ymm0 = Avx2.CompareEqual(ymm0, ymm7);
                ymm12 = Avx2.Subtract(ymm12, ymm0);
                ymm1 = Avx2.CompareEqual(ymm1, ymm7);
                ymm13 = Avx2.Subtract(ymm13, ymm1);
            }
            ymm12 = Avx2.Add(ymm12, ymm13);
            ymm8 = Avx2.Add(ymm8, ymm9);
            var xmm13 = ymm12.GetUpper();
            var xmm9 = ymm8.GetUpper();
            var xmm12 = Sse2.Add(ymm12.GetLower(), xmm13);
            var xmm8 = Sse2.Add(ymm8.GetLower(), xmm9);
            xmm13 = Sse2.Shuffle(xmm12.AsUInt32(), 0xEE).AsUInt64();
            xmm9 = Sse2.Shuffle(xmm8.AsUInt32(), 0xEE).AsUInt64();
            xmm12 = Sse2.Add(xmm12, xmm13);
            xmm8 = Sse2.Add(xmm8, xmm9);
            var r8 = xmm12.GetElement(0);
            var r9 = xmm8.GetElement(0);
            for (; i < length; i++)
            {
                ulong rax = Unsafe.Add(ref rsi, i);
                var bl = rax == 0;
                ulong rbx = Unsafe.As<bool, byte>(ref bl);
                r9 += rax;
                r8 += rbx;
            }
            return (r8, r9);
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        internal static (ulong zulp, ulong tulp) AccumulateDifferencesFallback(ReadOnlySpan<uint> src)
        {
            nint i, length = src.Length;
            ref var x9 = ref MemoryMarshal.GetReference(src);
            var x0 = 0ul;
            var x1 = 0ul;
            //TODO: Optimization
            for (i = 0; i < length; i++)
            {
                ulong x2 = Unsafe.Add(ref x9, i);
                var b3 = x2 == 0;
                ulong x3 = Unsafe.As<bool, byte>(ref b3);
                x1 += x2;
                x0 += x3;
            }
            return (x0, x1);
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        internal static (uint maxat, uint maxdiff) FindMaximumError(ReadOnlySpan<uint> ulp)
        {
            var v0_ns = VectorUtils.GetIndexVector().AsUInt32();
            var v15_ns = new Vector<uint>((uint)Vector<uint>.Count);
            var v1_ns = Vector<uint>.Zero;
            var v2_ns = Vector<uint>.Zero;
            var v3_ns = Vector<uint>.Zero;
            var v4_ns = Vector<uint>.Zero;
            var v5_ns = Vector<uint>.Zero;
            var v6_ns = Vector<uint>.Zero;
            var v7_ns = Vector<uint>.Zero;
            var v14_ns = v15_ns + v15_ns;
            var v12_ns = v0_ns;
            var v13_ns = v12_ns + v15_ns;
            v0_ns = Vector<uint>.Zero;
            Vector<uint> v8_ns, v9_ns, v10_ns, v11_ns;
            ref var x9 = ref MemoryMarshal.GetReference(ulp);
            nint i, length = ulp.Length;
            var olen = length - 4 * Vector<uint>.Count + 1;
            for (i = 0; i < olen; i += 4 * Vector<uint>.Count)
            {
                v8_ns = Vector.Max(v4_ns, Unsafe.As<uint, Vector<uint>>(ref Unsafe.Add(ref x9, i + 0 * Vector<uint>.Count)));
                v9_ns = Vector.Max(v5_ns, Unsafe.As<uint, Vector<uint>>(ref Unsafe.Add(ref x9, i + 1 * Vector<uint>.Count)));
                v10_ns = Vector.Max(v6_ns, Unsafe.As<uint, Vector<uint>>(ref Unsafe.Add(ref x9, i + 2 * Vector<uint>.Count)));
                v11_ns = Vector.Max(v7_ns, Unsafe.As<uint, Vector<uint>>(ref Unsafe.Add(ref x9, i + 3 * Vector<uint>.Count)));
                v4_ns = Vector.LessThan(v4_ns, v8_ns);
                v5_ns = Vector.LessThan(v5_ns, v9_ns);
                v6_ns = Vector.LessThan(v6_ns, v10_ns);
                v7_ns = Vector.LessThan(v7_ns, v11_ns);
                v0_ns = VectorUtils.Blend(v4_ns, v0_ns, v12_ns);
                v1_ns = VectorUtils.Blend(v5_ns, v1_ns, v13_ns);
                v12_ns += v14_ns;
                v13_ns += v14_ns;
                v4_ns = v8_ns;
                v5_ns = v9_ns;
                v2_ns = VectorUtils.Blend(v6_ns, v2_ns, v12_ns);
                v3_ns = VectorUtils.Blend(v7_ns, v3_ns, v13_ns);
                v6_ns = v10_ns;
                v7_ns = v11_ns;
                v12_ns += v14_ns;
                v13_ns += v14_ns;
            }
            v8_ns = Vector.LessThan(v4_ns, v5_ns);
            v9_ns = Vector.LessThan(v6_ns, v7_ns);
            v4_ns = VectorUtils.Blend(v8_ns, v4_ns, v5_ns);
            v0_ns = VectorUtils.Blend(v8_ns, v0_ns, v1_ns);
            v6_ns = VectorUtils.Blend(v9_ns, v6_ns, v7_ns);
            v2_ns = VectorUtils.Blend(v9_ns, v2_ns, v3_ns);
            v8_ns = Vector.LessThan(v4_ns, v6_ns);
            v4_ns = VectorUtils.Blend(v8_ns, v4_ns, v6_ns);
            v0_ns = VectorUtils.Blend(v8_ns, v0_ns, v2_ns);
            olen = length - Vector<uint>.Count + 1;
            for (; i < olen; i += Vector<uint>.Count)
            {
                v8_ns = Vector.Max(v4_ns, Unsafe.As<uint, Vector<uint>>(ref Unsafe.Add(ref x9, i)));
                v4_ns = Vector.LessThan(v4_ns, v8_ns);
                v12_ns += v15_ns;
                v0_ns = VectorUtils.Blend(v4_ns, v0_ns, v12_ns);
                v4_ns = v8_ns;
            }
            Span<uint> diffs = stackalloc uint[Vector<uint>.Count];
            Span<uint> index = stackalloc uint[Vector<uint>.Count];
            ref var x10 = ref MemoryMarshal.GetReference(index);
            v0_ns.CopyTo(index);
            v4_ns.CopyTo(diffs);
            var maxdiff = 0u;
            uint maxindex = 0;
            for (var j = 0; j < diffs.Length; j++)
            {
                var d = diffs[j];
                if (maxdiff < d)
                {
                    maxdiff = d;
                    maxindex = Unsafe.Add(ref x10, j);
                }
            }
            for (; i < length; i++)
            {
                var d = Unsafe.Add(ref x9, i);
                if (maxdiff < d)
                {
                    maxdiff = d;
                    maxindex = (uint)i;
                }
            }
            return (maxindex, maxdiff);
        }
        public static void GenerateIndexValuedArraySingle(Span<float> dst, float start) => GenerateIndexValuedArrayUInt32(MemoryMarshal.Cast<float, uint>(dst), BitConverter.SingleToUInt32Bits(start));
        public static void GenerateIndexValuedArrayUInt32(Span<uint> dst, uint start)
        {
            var v0_ns = new Vector<uint>(start).AsInt32();
            var v8_ns = VectorUtils.GetIndexVector();
            v0_ns += v8_ns;
            v8_ns = new Vector<int>(Vector<int>.Count);
            var v1_ns = v0_ns + v8_ns;
            v8_ns += v8_ns;
            var v2_ns = v0_ns + v8_ns;
            var v3_ns = v1_ns + v8_ns;
            v8_ns += v8_ns;
            ref var x9 = ref MemoryMarshal.GetReference(dst);
            nint i = 0, length = dst.Length;
            var olen = length - 8 * Vector<int>.Count + 1;
            for (; i < olen; i += 8 * Vector<int>.Count)
            {
                var v4_ns = v0_ns + v8_ns;
                var v5_ns = v1_ns + v8_ns;
                var v6_ns = v2_ns + v8_ns;
                var v7_ns = v3_ns + v8_ns;
                Unsafe.As<uint, Vector<int>>(ref Unsafe.Add(ref x9, i + 0 * Vector<int>.Count)) = v0_ns;
                Unsafe.As<uint, Vector<int>>(ref Unsafe.Add(ref x9, i + 1 * Vector<int>.Count)) = v1_ns;
                Unsafe.As<uint, Vector<int>>(ref Unsafe.Add(ref x9, i + 2 * Vector<int>.Count)) = v2_ns;
                Unsafe.As<uint, Vector<int>>(ref Unsafe.Add(ref x9, i + 3 * Vector<int>.Count)) = v3_ns;
                v0_ns = v4_ns + v8_ns;
                v1_ns = v5_ns + v8_ns;
                v2_ns = v6_ns + v8_ns;
                v3_ns = v7_ns + v8_ns;
                Unsafe.As<uint, Vector<int>>(ref Unsafe.Add(ref x9, i + 4 * Vector<int>.Count)) = v4_ns;
                Unsafe.As<uint, Vector<int>>(ref Unsafe.Add(ref x9, i + 5 * Vector<int>.Count)) = v5_ns;
                Unsafe.As<uint, Vector<int>>(ref Unsafe.Add(ref x9, i + 6 * Vector<int>.Count)) = v6_ns;
                Unsafe.As<uint, Vector<int>>(ref Unsafe.Add(ref x9, i + 7 * Vector<int>.Count)) = v7_ns;
            }
            olen = length - Vector<int>.Count + 1;
            for (; i < olen; i += Vector<int>.Count)
            {
                var v4_ns = v0_ns + v8_ns;
                Unsafe.As<uint, Vector<int>>(ref Unsafe.Add(ref x9, i)) = v0_ns;
                v0_ns = v4_ns;
            }
            var w0 = v0_ns.AsInt32()[0];
            for (; i < length; i++)
            {
                Unsafe.As<uint, int>(ref Unsafe.Add(ref x9, i)) = w0++;
            }
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static void WriteResult(ErrorEvaluationResult<float> res)
        {
            Console.WriteLine($"Values Tested: {res.ValuesTested}");
            var inv = 1.0 / res.ValuesTested;
            Console.WriteLine($"Values with no error: {res.ZeroUlpErrorValues} ({res.ZeroUlpErrorValues * inv:P})");
            Console.WriteLine($"Total Ulp Difference: {res.TotalUlpError} ({res.TotalUlpError * inv:P})");
            Console.WriteLine($"Average Ulp Difference: {res.AverageUlpError}");
            Console.WriteLine($"Maximum Ulp Difference: {res.MaxUlpError.UlpDifference} at {res.MaxUlpError.ParameterAt} ({res.MaxUlpError.AbsoluteDifference})");
        }
    }
}
