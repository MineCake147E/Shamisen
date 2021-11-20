
using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

#if NETCOREAPP3_1_OR_GREATER

using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Shamisen.Conversion.SampleToWaveConverters
{
    public sealed partial class SampleToPcm16Converter
    {
        #region Normal
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ProcessNormalSse2(Span<float> wrote, Span<short> dest)
        {
            var max = Vector128.Create(32767.0f / 32768.0f);
            var min = Vector128.Create(-1.0f);
            var mul = Vector128.Create(0x0780_0000u);
            ref var rdi = ref MemoryMarshal.GetReference(dest);
            ref var rsi = ref MemoryMarshal.GetReference(wrote);
            nint i = 0, length = MathI.Min(wrote.Length, dest.Length);
            var olen = length - 15;
            for (; i < olen; i += 16)
            {
                var xmm0 = Sse.Min(max, Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rsi, i)));
                var xmm1 = Sse.Min(max, Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rsi, i + 4)));
                xmm0 = Sse.Max(xmm0, min);
                xmm1 = Sse.Max(xmm1, min);
                xmm0 = Sse2.Add(xmm0.AsUInt32(), mul).AsSingle();
                xmm1 = Sse2.Add(xmm1.AsUInt32(), mul).AsSingle();
                xmm0 = Sse2.ConvertToVector128Int32(xmm0).AsSingle();
                xmm1 = Sse2.ConvertToVector128Int32(xmm1).AsSingle();
                Unsafe.As<short, Vector128<short>>(ref Unsafe.Add(ref rdi, i)) = Sse2.PackSignedSaturate(xmm0.AsInt32(), xmm1.AsInt32());
                xmm0 = Sse.Min(max, Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rsi, i + 8)));
                xmm1 = Sse.Min(max, Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rsi, i + 12)));
                xmm0 = Sse.Max(xmm0, min);
                xmm1 = Sse.Max(xmm1, min);
                xmm0 = Sse2.Add(xmm0.AsUInt32(), mul).AsSingle();
                xmm1 = Sse2.Add(xmm1.AsUInt32(), mul).AsSingle();
                xmm0 = Sse2.ConvertToVector128Int32(xmm0).AsSingle();
                xmm1 = Sse2.ConvertToVector128Int32(xmm1).AsSingle();
                Unsafe.As<short, Vector128<short>>(ref Unsafe.Add(ref rdi, i + 8)) = Sse2.PackSignedSaturate(xmm0.AsInt32(), xmm1.AsInt32());
            }
            for (; i < length; i++)
            {
                var xmm0 = Vector128.CreateScalarUnsafe(Unsafe.Add(ref rsi, i));
                xmm0 = Sse.MinScalar(xmm0, max);
                xmm0 = Sse.MaxScalar(xmm0, min);
                xmm0 = Sse2.Add(xmm0.AsUInt32(), mul).AsSingle();
                var r8 = Sse.ConvertToInt32(xmm0);
                Unsafe.Add(ref rdi, i) = (short)r8;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ProcessNormalAvx2(Span<float> wrote, Span<short> dest)
        {
            var max = Vector256.Create(32767.0f / 32768.0f);
            var min = Vector256.Create(-1.0f);
            var offset = Vector256.Create(0x0780_0000u);
            ref var rdi = ref MemoryMarshal.GetReference(dest);
            ref var rsi = ref MemoryMarshal.GetReference(wrote);
            nint i = 0, length = MathI.Min(wrote.Length, dest.Length);
            var olen = length - 31;
            for (; i < olen; i += 32)
            {
                var ymm0 = Avx.Max(min, Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rsi, i + 0)));
                var ymm1 = Avx.Max(min, Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rsi, i + 8)));
                var ymm2 = Avx.Max(min, Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rsi, i + 16)));
                var ymm3 = Avx.Max(min, Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rsi, i + 24)));
                ymm0 = Avx.Min(ymm0, max);
                ymm1 = Avx.Min(ymm1, max);
                ymm2 = Avx.Min(ymm2, max);
                ymm3 = Avx.Min(ymm3, max);
                ymm0 = Avx2.Add(offset, ymm0.AsUInt32()).AsSingle();
                ymm1 = Avx2.Add(offset, ymm1.AsUInt32()).AsSingle();
                ymm2 = Avx2.Add(offset, ymm2.AsUInt32()).AsSingle();
                ymm3 = Avx2.Add(offset, ymm3.AsUInt32()).AsSingle();
                ymm0 = Avx.ConvertToVector256Int32(ymm0).AsSingle();
                var xmm0 = Sse2.PackSignedSaturate(ymm0.AsInt32().GetLower(), ymm0.AsInt32().GetUpper());
                ymm1 = Avx.ConvertToVector256Int32(ymm1).AsSingle();
                var xmm1 = Sse2.PackSignedSaturate(ymm1.AsInt32().GetLower(), ymm1.AsInt32().GetUpper());
                ymm2 = Avx.ConvertToVector256Int32(ymm2).AsSingle();
                var xmm2 = Sse2.PackSignedSaturate(ymm2.AsInt32().GetLower(), ymm2.AsInt32().GetUpper());
                ymm3 = Avx.ConvertToVector256Int32(ymm3).AsSingle();
                var xmm3 = Sse2.PackSignedSaturate(ymm3.AsInt32().GetLower(), ymm3.AsInt32().GetUpper());
                Unsafe.As<short, Vector128<short>>(ref Unsafe.Add(ref rdi, i)) = xmm0;
                Unsafe.As<short, Vector128<short>>(ref Unsafe.Add(ref rdi, i + 8)) = xmm1;
                Unsafe.As<short, Vector128<short>>(ref Unsafe.Add(ref rdi, i + 16)) = xmm2;
                Unsafe.As<short, Vector128<short>>(ref Unsafe.Add(ref rdi, i + 24)) = xmm3;
            }
            olen = length - 7;
            for (; i < olen; i += 8)
            {
                var xmm0 = Sse.Max(min.GetLower(), Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rsi, i)));
                var xmm1 = Sse.Max(min.GetLower(), Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rsi, i + 4)));
                xmm0 = Sse.Min(xmm0, max.GetLower());
                xmm1 = Sse.Min(xmm1, max.GetLower());
                xmm0 = Sse2.Add(offset.GetLower(), xmm0.AsUInt32()).AsSingle();
                xmm1 = Sse2.Add(offset.GetLower(), xmm1.AsUInt32()).AsSingle();
                xmm0 = Sse2.ConvertToVector128Int32(xmm0).AsSingle();
                xmm1 = Sse2.ConvertToVector128Int32(xmm1).AsSingle();
                Unsafe.As<short, Vector128<short>>(ref Unsafe.Add(ref rdi, i)) = Sse2.PackSignedSaturate(xmm0.AsInt32(), xmm1.AsInt32());
            }
            for (; i < length; i++)
            {
                var xmm0 = Vector128.CreateScalarUnsafe(Unsafe.Add(ref rsi, i));
                xmm0 = Sse.MaxScalar(xmm0, min.GetLower());
                xmm0 = Sse.MinScalar(xmm0, max.GetLower());
                xmm0 = Sse2.Add(offset.GetLower(), xmm0.AsUInt32()).AsSingle();
                var r8 = Sse.ConvertToInt32(xmm0);
                Unsafe.Add(ref rdi, i) = (short)r8;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ProcessReversedSsse3(Span<float> wrote, Span<short> dest)
        {
            var max = Vector128.Create(32767.0f / 32768.0f);
            var min = Vector128.Create(-1.0f);
            var mul = Vector128.Create(0x0780_0000u);
            ref var rdi = ref MemoryMarshal.GetReference(dest);
            ref var rsi = ref MemoryMarshal.GetReference(wrote);
            nint i = 0, length = MathI.Min(wrote.Length, dest.Length);
            var olen = length - 15;
            var shuffle = Vector128.Create(1, 0, 3, 2, 5, 4, 7, 6, 9, 8, 11, 10, 13, 12, 15, 14).AsByte();
            for (; i < olen; i += 16)
            {
                var xmm0 = Sse.Min(max, Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rsi, i)));
                var xmm1 = Sse.Min(max, Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rsi, i + 4)));
                xmm0 = Sse.Max(xmm0, min);
                xmm1 = Sse.Max(xmm1, min);
                xmm0 = Sse2.Add(xmm0.AsUInt32(), mul).AsSingle();
                xmm1 = Sse2.Add(xmm1.AsUInt32(), mul).AsSingle();
                xmm0 = Sse2.ConvertToVector128Int32(xmm0).AsSingle();
                xmm1 = Sse2.ConvertToVector128Int32(xmm1).AsSingle();
                var xmm2 = Sse2.PackSignedSaturate(xmm0.AsInt32(), xmm1.AsInt32());
                Unsafe.As<short, Vector128<short>>(ref Unsafe.Add(ref rdi, i)) = Ssse3.Shuffle(xmm2.AsByte(), shuffle).AsInt16();
                xmm0 = Sse.Min(max, Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rsi, i + 8)));
                xmm1 = Sse.Min(max, Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rsi, i + 12)));
                xmm0 = Sse.Max(xmm0, min);
                xmm1 = Sse.Max(xmm1, min);
                xmm0 = Sse2.Add(xmm0.AsUInt32(), mul).AsSingle();
                xmm1 = Sse2.Add(xmm1.AsUInt32(), mul).AsSingle();
                xmm0 = Sse2.ConvertToVector128Int32(xmm0).AsSingle();
                xmm1 = Sse2.ConvertToVector128Int32(xmm1).AsSingle();
                xmm2 = Sse2.PackSignedSaturate(xmm0.AsInt32(), xmm1.AsInt32());
                Unsafe.As<short, Vector128<short>>(ref Unsafe.Add(ref rdi, i + 8)) = Ssse3.Shuffle(xmm2.AsByte(), shuffle).AsInt16();
            }
            for (; i < length; i++)
            {
                var xmm0 = Vector128.CreateScalarUnsafe(Unsafe.Add(ref rsi, i));
                xmm0 = Sse.MinScalar(xmm0, max);
                xmm0 = Sse.MaxScalar(xmm0, min);
                xmm0 = Sse2.Add(xmm0.AsUInt32(), mul).AsSingle();
                var r8 = Sse.ConvertToInt32(xmm0);
                Unsafe.Add(ref rdi, i) = BinaryPrimitives.ReverseEndianness((short)r8);
            }
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ProcessReversedAvx2(Span<float> wrote, Span<short> dest)
        {
            var max = Vector256.Create(32767.0f);
            var min = Vector256.Create(-32768.0f);
            var mul = Vector256.Create(32768.0f);
            var shuffle = Vector128.Create(1, 0, 3, 2, 5, 4, 7, 6, 9, 8, 11, 10, 13, 12, 15, 14).AsByte();
            ref var rdi = ref MemoryMarshal.GetReference(dest);
            ref var rsi = ref MemoryMarshal.GetReference(wrote);
            nint i = 0, length = MathI.Min(wrote.Length, dest.Length);
            var olen = length - 31;
            for (; i < olen; i += 32)
            {
                var ymm0 = Avx.Multiply(mul, Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rsi, i + 0)));
                var ymm1 = Avx.Multiply(mul, Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rsi, i + 8)));
                var ymm2 = Avx.Multiply(mul, Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rsi, i + 16)));
                var ymm3 = Avx.Multiply(mul, Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rsi, i + 24)));
                ymm0 = Avx.Max(ymm0, min);
                ymm1 = Avx.Max(ymm1, min);
                ymm2 = Avx.Max(ymm2, min);
                ymm3 = Avx.Max(ymm3, min);
                ymm0 = Avx.Min(ymm0, max);
                ymm1 = Avx.Min(ymm1, max);
                ymm2 = Avx.Min(ymm2, max);
                ymm3 = Avx.Min(ymm3, max);
                ymm0 = Avx.ConvertToVector256Int32(ymm0).AsSingle();
                var xmm0 = Sse2.PackSignedSaturate(ymm0.AsInt32().GetLower(), ymm0.AsInt32().GetUpper());
                ymm1 = Avx.ConvertToVector256Int32(ymm1).AsSingle();
                var xmm1 = Sse2.PackSignedSaturate(ymm1.AsInt32().GetLower(), ymm1.AsInt32().GetUpper());
                ymm2 = Avx.ConvertToVector256Int32(ymm2).AsSingle();
                var xmm2 = Sse2.PackSignedSaturate(ymm2.AsInt32().GetLower(), ymm2.AsInt32().GetUpper());
                ymm3 = Avx.ConvertToVector256Int32(ymm3).AsSingle();
                var xmm3 = Sse2.PackSignedSaturate(ymm3.AsInt32().GetLower(), ymm3.AsInt32().GetUpper());
                xmm0 = Ssse3.Shuffle(xmm0.AsByte(), shuffle).AsInt16();
                xmm1 = Ssse3.Shuffle(xmm1.AsByte(), shuffle).AsInt16();
                xmm2 = Ssse3.Shuffle(xmm2.AsByte(), shuffle).AsInt16();
                xmm3 = Ssse3.Shuffle(xmm3.AsByte(), shuffle).AsInt16();
                Unsafe.As<short, Vector128<short>>(ref Unsafe.Add(ref rdi, i)) = xmm0;
                Unsafe.As<short, Vector128<short>>(ref Unsafe.Add(ref rdi, i + 8)) = xmm1;
                Unsafe.As<short, Vector128<short>>(ref Unsafe.Add(ref rdi, i + 16)) = xmm2;
                Unsafe.As<short, Vector128<short>>(ref Unsafe.Add(ref rdi, i + 24)) = xmm3;
            }
            olen = length - 7;
            for (; i < olen; i += 8)
            {
                var xmm0 = Sse.Multiply(mul.GetLower(), Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rsi, i)));
                var xmm1 = Sse.Multiply(mul.GetLower(), Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rsi, i + 4)));
                xmm0 = Sse.Min(xmm0, max.GetLower());
                xmm1 = Sse.Min(xmm1, max.GetLower());
                xmm0 = Sse.Max(xmm0, min.GetLower());
                xmm1 = Sse.Max(xmm1, min.GetLower());
                xmm0 = Sse2.ConvertToVector128Int32(xmm0).AsSingle();
                xmm1 = Sse2.ConvertToVector128Int32(xmm1).AsSingle();
                var xmm2 = Sse2.PackSignedSaturate(xmm0.AsInt32(), xmm1.AsInt32());
                Unsafe.As<short, Vector128<short>>(ref Unsafe.Add(ref rdi, i)) = Ssse3.Shuffle(xmm2.AsByte(), shuffle).AsInt16();
            }
            for (; i < length; i++)
            {
                var xmm0 = Vector128.CreateScalarUnsafe(Unsafe.Add(ref rsi, i));
                xmm0 = Sse.MultiplyScalar(xmm0, mul.GetLower());
                xmm0 = Sse.MinScalar(xmm0, max.GetLower());
                xmm0 = Sse.MaxScalar(xmm0, min.GetLower());
                var r8 = Sse.ConvertToInt32(xmm0);
                Unsafe.Add(ref rdi, i) = BinaryPrimitives.ReverseEndianness((short)r8);
            }
        }

        #endregion

        #region Accurate
        private static void ProcessAccurateStereoSse41(Span<float> wrote, Span<short> dest, Span<float> dsmAccumulator, Span<short> dsmLastOutput)
        {
            var max = Vector128.Create(32767.0f);
            var min = Vector128.Create(-32768.0f);
            var mul = Vector128.Create(32768.0f);
            var acc = Vector128.CreateScalar(Unsafe.As<float, double>(ref MemoryMarshal.GetReference(dsmAccumulator))).AsSingle();
            var lov = Vector128.CreateScalar(Unsafe.As<short, float>(ref MemoryMarshal.GetReference(dsmLastOutput))).AsInt16();
            ref var wh = ref Unsafe.As<float, double>(ref MemoryMarshal.GetReference(wrote));
            ref var dh = ref MemoryMarshal.GetReference(dest);
            nint length = wrote.Length / 2;
            nint i;
            for (i = 0; i < length - 1; i += 2)
            {
                var lo = Sse2.ConvertToVector128Single(Sse41.ConvertToVector128Int32(lov));
                var xmm5 = Vector128.CreateScalar(Unsafe.Add(ref wh, i)).AsSingle();
                xmm5 = Sse.Multiply(xmm5, mul);
                lo = Sse.Subtract(xmm5, lo);
                acc = Sse.Add(acc, lo);
                acc = Sse.Max(acc, min);
                acc = Sse.Min(acc, max);
                var xmm1 = Sse2.ConvertToVector128Int32WithTruncation(acc);
                Unsafe.Add(ref dh, 2 * i) = xmm1.AsInt16().GetElement(0);
                var xmm5epi16 = Sse2.PackSignedSaturate(xmm1, xmm1);
                Unsafe.Add(ref dh, 2 * i + 1) = xmm1.AsInt16().GetElement(2);
                lo = Sse2.ConvertToVector128Single(Sse41.ConvertToVector128Int32(xmm5epi16));
                xmm5 = Vector128.CreateScalar(Unsafe.Add(ref wh, i + 1)).AsSingle();
                xmm5 = Sse.Multiply(xmm5, mul);
                lo = Sse.Subtract(xmm5, lo);
                acc = Sse.Add(acc, lo);
                acc = Sse.Max(acc, min);
                acc = Sse.Min(acc, max);
                xmm1 = Sse2.ConvertToVector128Int32WithTruncation(acc);
                Unsafe.Add(ref dh, 2 * i + 2) = xmm1.AsInt16().GetElement(0);
                Unsafe.Add(ref dh, 2 * i + 3) = xmm1.AsInt16().GetElement(2);
                lov = Sse2.PackSignedSaturate(xmm1, xmm1);
            }
            if (i < length)
            {
                var lo = Sse2.ConvertToVector128Single(Sse41.ConvertToVector128Int32(lov));
                var xmm5 = Vector128.CreateScalar(Unsafe.Add(ref wh, i)).AsSingle();
                xmm5 = Sse.Multiply(xmm5, mul);
                lo = Sse.Subtract(xmm5, lo);
                acc = Sse.Add(acc, lo);
                acc = Sse.Max(acc, min);
                acc = Sse.Min(acc, max);
                var xmm1 = Sse2.ConvertToVector128Int32WithTruncation(acc);
                Unsafe.Add(ref dh, 2 * i) = xmm1.AsInt16().GetElement(0);
                Unsafe.Add(ref dh, 2 * i + 1) = xmm1.AsInt16().GetElement(2);
                lov = Sse2.PackSignedSaturate(xmm1, xmm1);
            }
            Unsafe.As<float, double>(ref MemoryMarshal.GetReference(dsmAccumulator)) = acc.AsDouble().GetElement(0);
            Unsafe.As<short, float>(ref MemoryMarshal.GetReference(dsmLastOutput)) = lov.AsSingle().GetElement(0);
        }

        private static void ProcessAccurateMonauralSse41(Span<float> wrote, Span<short> dest, Span<float> dsmAccumulator, Span<short> dsmLastOutput)
        {
            var max = Vector128.CreateScalar(32767.0f);
            var min = Vector128.CreateScalar(-32768.0f);
            var mul = Vector128.CreateScalar(32768.0f);
            var acc = Vector128.CreateScalar(MemoryMarshal.GetReference(dsmAccumulator));
            var lov = Vector128.CreateScalar(MemoryMarshal.GetReference(dsmLastOutput));
            var lo = Sse2.ConvertToVector128Single(Sse41.ConvertToVector128Int32(lov));
            ref var wh = ref MemoryMarshal.GetReference(wrote);
            ref var dh = ref MemoryMarshal.GetReference(dest);
            nint length = wrote.Length;
            nint i;
            for (i = 0; i < length - 1; i += 2)
            {
                var xmm5 = Vector128.CreateScalar(Unsafe.Add(ref wh, i));
                xmm5 = Sse.MultiplyScalar(xmm5, mul);
                lo = Sse.SubtractScalar(xmm5, lo);
                acc = Sse.AddScalar(acc, lo);
                acc = Sse.MaxScalar(acc, min);
                acc = Sse.MinScalar(acc, max);
                var eax = Sse.ConvertToInt32WithTruncation(acc);
                Unsafe.Add(ref dh, 2 * i) = (short)eax;
                lo = Sse41.RoundToZeroScalar(acc);
                xmm5 = Vector128.CreateScalar(Unsafe.Add(ref wh, i + 1));
                xmm5 = Sse.MultiplyScalar(xmm5, mul);
                lo = Sse.SubtractScalar(xmm5, lo);
                acc = Sse.AddScalar(acc, lo);
                acc = Sse.MaxScalar(acc, min);
                acc = Sse.MinScalar(acc, max);
                eax = Sse.ConvertToInt32WithTruncation(acc);
                Unsafe.Add(ref dh, 2 * i + 1) = (short)eax;
                lo = Sse41.RoundToZeroScalar(acc);
            }
            if (i < length)
            {
                var xmm5 = Vector128.CreateScalar(Unsafe.Add(ref wh, i));
                xmm5 = Sse.MultiplyScalar(xmm5, mul);
                lo = Sse.SubtractScalar(xmm5, lo);
                acc = Sse.AddScalar(acc, lo);
                acc = Sse.MaxScalar(acc, min);
                acc = Sse.MinScalar(acc, max);
                var eax = Sse.ConvertToInt32WithTruncation(acc);
                Unsafe.Add(ref dh, 2 * i) = (short)eax;
                lo = Sse41.RoundToZeroScalar(acc);
            }
            MemoryMarshal.GetReference(dsmAccumulator) = acc.GetElement(0);
            MemoryMarshal.GetReference(dsmLastOutput) = (short)lo.GetElement(0);
        }
        #endregion
        #region AccurateFused
        private static void ProcessAccurateFusedStereoFma(Span<float> wrote, Span<short> dest, Span<float> dsmAccumulator, Span<short> dsmLastOutput)
        {
            var max = Vector128.Create(32767.0f);
            var min = Vector128.Create(-32768.0f);
            var mul = Vector128.Create(32768.0f);
            var acc = Vector128.CreateScalar(Unsafe.As<float, double>(ref MemoryMarshal.GetReference(dsmAccumulator))).AsSingle();
            var lov = Vector128.CreateScalar(Unsafe.As<short, float>(ref MemoryMarshal.GetReference(dsmLastOutput))).AsInt16();
            ref var wh = ref Unsafe.As<float, double>(ref MemoryMarshal.GetReference(wrote));
            ref var dh = ref MemoryMarshal.GetReference(dest);
            nint length = wrote.Length / 2;
            nint i;
            for (i = 0; i < length - 1; i += 2)
            {
                var lo = Sse2.ConvertToVector128Single(Sse41.ConvertToVector128Int32(lov));
                var xmm5 = Vector128.CreateScalarUnsafe(Unsafe.Add(ref wh, i)).AsSingle();
                xmm5 = Fma.MultiplySubtract(xmm5, mul, lo);   //C# prefers to use vfmsub213ps+vmovaps rather than vfmsub231ps
                acc = Sse.Add(acc, xmm5);
                acc = Sse.Max(acc, min);
                acc = Sse.Min(acc, max);
                var xmm1 = Sse2.ConvertToVector128Int32WithTruncation(acc);
                Unsafe.Add(ref dh, 2 * i) = xmm1.AsInt16().GetElement(0);
                var xmm5epi16 = Sse2.PackSignedSaturate(xmm1, xmm1);
                Unsafe.Add(ref dh, 2 * i + 1) = xmm1.AsInt16().GetElement(2);
                lo = Sse2.ConvertToVector128Single(Sse41.ConvertToVector128Int32(xmm5epi16));
                xmm5 = Vector128.CreateScalarUnsafe(Unsafe.Add(ref wh, i + 1)).AsSingle();
                xmm5 = Fma.MultiplySubtract(xmm5, mul, lo);
                acc = Sse.Add(acc, xmm5);
                acc = Sse.Max(acc, min);
                acc = Sse.Min(acc, max);
                xmm1 = Sse2.ConvertToVector128Int32WithTruncation(acc);
                Unsafe.Add(ref dh, 2 * i + 2) = xmm1.AsInt16().GetElement(0);
                Unsafe.Add(ref dh, 2 * i + 3) = xmm1.AsInt16().GetElement(2);
                lov = Sse2.PackSignedSaturate(xmm1, xmm1);
            }
            if (i < length)
            {
                var lo = Sse2.ConvertToVector128Single(Sse41.ConvertToVector128Int32(lov));
                var xmm5 = Vector128.CreateScalarUnsafe(Unsafe.Add(ref wh, i)).AsSingle();
                xmm5 = Sse.Multiply(xmm5, mul);
                lo = Fma.MultiplySubtract(xmm5, mul, lo);
                acc = Sse.Add(acc, lo);
                acc = Sse.Max(acc, min);
                acc = Sse.Min(acc, max);
                var xmm1 = Sse2.ConvertToVector128Int32WithTruncation(acc);
                Unsafe.Add(ref dh, 2 * i) = xmm1.AsInt16().GetElement(0);
                Unsafe.Add(ref dh, 2 * i + 1) = xmm1.AsInt16().GetElement(2);
                lov = Sse2.PackSignedSaturate(xmm1, xmm1);
            }
            Unsafe.As<float, double>(ref MemoryMarshal.GetReference(dsmAccumulator)) = acc.AsDouble().GetElement(0);
            Unsafe.As<short, float>(ref MemoryMarshal.GetReference(dsmLastOutput)) = lov.AsSingle().GetElement(0);
        }
        private static void ProcessAccurateFusedMonauralFma(Span<float> wrote, Span<short> dest, Span<float> dsmAccumulator, Span<short> dsmLastOutput)
        {
            var max = Vector128.CreateScalar(32767.0f);
            var min = Vector128.CreateScalar(-32768.0f);
            var mul = Vector128.CreateScalar(32768.0f);
            var acc = Vector128.CreateScalar(MemoryMarshal.GetReference(dsmAccumulator));
            var lov = Vector128.CreateScalar(MemoryMarshal.GetReference(dsmLastOutput));
            var lo = Sse2.ConvertToVector128Single(Sse41.ConvertToVector128Int32(lov));
            ref var wh = ref MemoryMarshal.GetReference(wrote);
            ref var dh = ref MemoryMarshal.GetReference(dest);
            nint length = wrote.Length;
            nint i;
            for (i = 0; i < length - 1; i += 2)
            {
                var xmm5 = Vector128.CreateScalar(Unsafe.Add(ref wh, i));
                xmm5 = Fma.MultiplySubtractScalar(mul, xmm5, lo);   //C# prefers to use vfmsub213ss rather than vfmsub231ss
                acc = Sse.AddScalar(acc, xmm5);
                acc = Sse.MaxScalar(acc, min);
                acc = Sse.MinScalar(acc, max);
                var eax = Sse.ConvertToInt32WithTruncation(acc);
                Unsafe.Add(ref dh, 2 * i) = (short)eax;
                lo = Sse41.RoundToZeroScalar(acc);
                xmm5 = Vector128.CreateScalar(Unsafe.Add(ref wh, i + 1));
                xmm5 = Fma.MultiplySubtractScalar(mul, xmm5, lo);
                acc = Sse.AddScalar(acc, xmm5);
                acc = Sse.MaxScalar(acc, min);
                acc = Sse.MinScalar(acc, max);
                eax = Sse.ConvertToInt32WithTruncation(acc);
                Unsafe.Add(ref dh, 2 * i + 1) = (short)eax;
                lo = Sse41.RoundToZeroScalar(acc);
            }
            if (i < length)
            {
                var xmm5 = Vector128.CreateScalar(Unsafe.Add(ref wh, i));
                xmm5 = Fma.MultiplySubtractScalar(mul, xmm5, lo);
                acc = Sse.AddScalar(acc, xmm5);
                acc = Sse.MaxScalar(acc, min);
                acc = Sse.MinScalar(acc, max);
                var eax = Sse.ConvertToInt32WithTruncation(acc);
                Unsafe.Add(ref dh, 2 * i) = (short)eax;
                lo = Sse41.RoundToZeroScalar(acc);
            }
            MemoryMarshal.GetReference(dsmAccumulator) = acc.GetElement(0);
            MemoryMarshal.GetReference(dsmLastOutput) = (short)lo.GetElement(0);
        }

        #endregion
    }
}

#endif
