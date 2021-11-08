#if NETCOREAPP3_1_OR_GREATER
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

using Shamisen.Optimization;

namespace Shamisen.Conversion.WaveToSampleConverters
{
    public sealed partial class RangedPcm32ToSampleConverter
    {
        internal static class X86
        {
            public static bool IsSupported
            {
                [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
                get => Sse2.IsSupported;
            }

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void Process(Span<float> buffer, int effectiveBitDepth)
            {
                switch (effectiveBitDepth)
                {
                    case >= 32:
                        Pcm32ToSampleConverter.ProcessNormal(buffer);
                        return;
                    case 16 when Avx2.IsSupported:
                        ProcessE16Avx2(buffer);
                        return;
                    case 16 when Sse2.IsSupported:
                        ProcessE16Sse2(buffer);
                        return;
                    case 8 when Avx2.IsSupported:
                        ProcessE8Avx2(buffer);
                        return;
                    case 8 when Sse2.IsSupported:
                        ProcessE8Sse2(buffer);
                        return;
                    case 24 when Avx2.IsSupported:
                        ProcessE24Avx2(buffer);
                        return;
                    case 24 when Ssse3.IsSupported:
                        ProcessE24Ssse3(buffer);
                        return;
                    case < 24 when Avx2.IsSupported && !IntrinsicsUtils.PreferShiftVariable:
                        ProcessELessThan24Avx2(buffer, effectiveBitDepth);
                        return;
                    case < 24 when Avx2.IsSupported && IntrinsicsUtils.PreferShiftVariable:
                        ProcessELessThan24Avx2PSV(buffer, effectiveBitDepth);
                        return;
                    case < 24 when Sse2.IsSupported:
                        ProcessELessThan24Sse2(buffer, effectiveBitDepth);
                        return;
                    case > 24 when Avx2.IsSupported:
                        ProcessEMoreThan24Avx2(buffer, effectiveBitDepth);
                        return;
                    default:
                        ProcessEMoreThan24Standard(buffer, effectiveBitDepth);
                        return;
                }
            }

            #region 24 Effective Bits
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void ProcessE24Avx2(Span<float> dest)
            {
                ref var rdi = ref MemoryMarshal.GetReference(dest);
                ref var rsi = ref Unsafe.As<float, int>(ref rdi);
                nint i = 0, length = dest.Length;
                var ymm15 = Vector256.Create(1.0f);
                var ymm8 = Vector256.Create(int.MinValue);
                var olen = length - 31;
                for (; i < olen; i += 32)
                {
                    var ymm0 = Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rsi, i + 0 * 8));
                    var ymm1 = Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rsi, i + 1 * 8));
                    var ymm2 = Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rsi, i + 2 * 8));
                    var ymm3 = Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rsi, i + 3 * 8));
                    var ymm4 = Avx2.And(ymm8, ymm0);
                    var ymm5 = Avx2.And(ymm8, ymm1);
                    var ymm6 = Avx2.And(ymm8, ymm2);
                    var ymm7 = Avx2.And(ymm8, ymm3);
                    ymm0 = Avx2.Abs(ymm0).AsInt32();
                    ymm1 = Avx2.Abs(ymm1).AsInt32();
                    ymm2 = Avx2.Abs(ymm2).AsInt32();
                    ymm3 = Avx2.Abs(ymm3).AsInt32();
                    ymm0 = Avx2.Add(ymm0, ymm15.AsInt32());
                    ymm1 = Avx2.Add(ymm1, ymm15.AsInt32());
                    ymm2 = Avx2.Add(ymm2, ymm15.AsInt32());
                    ymm3 = Avx2.Add(ymm3, ymm15.AsInt32());
                    ymm0 = Avx2.Or(ymm0, ymm4);
                    ymm4 = Avx2.Or(ymm4, ymm15.AsInt32());
                    ymm1 = Avx2.Or(ymm1, ymm5);
                    ymm5 = Avx2.Or(ymm5, ymm15.AsInt32());
                    ymm2 = Avx2.Or(ymm2, ymm6);
                    ymm6 = Avx2.Or(ymm6, ymm15.AsInt32());
                    ymm3 = Avx2.Or(ymm3, ymm7);
                    ymm7 = Avx2.Or(ymm7, ymm15.AsInt32());
                    ymm0 = Avx.Subtract(ymm0.AsSingle(), ymm4.AsSingle()).AsInt32();
                    ymm1 = Avx.Subtract(ymm1.AsSingle(), ymm5.AsSingle()).AsInt32();
                    ymm2 = Avx.Subtract(ymm2.AsSingle(), ymm6.AsSingle()).AsInt32();
                    ymm3 = Avx.Subtract(ymm3.AsSingle(), ymm7.AsSingle()).AsInt32();
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 0)) = ymm0.AsSingle();
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 8)) = ymm1.AsSingle();
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 16)) = ymm2.AsSingle();
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 24)) = ymm3.AsSingle();
                }
                olen = length - 7;
                for (; i < olen; i += 8)
                {
                    var xmm0 = Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rsi, i + 0 * 4));
                    var xmm1 = Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rsi, i + 1 * 4));
                    var xmm4 = Sse2.And(ymm8.GetLower(), xmm0);
                    var xmm5 = Sse2.And(ymm8.GetLower(), xmm1);
                    xmm0 = Ssse3.Abs(xmm0).AsInt32();
                    xmm1 = Ssse3.Abs(xmm1).AsInt32();
                    xmm0 = Sse2.Add(xmm0, ymm15.AsInt32().GetLower());
                    xmm1 = Sse2.Add(xmm1, ymm15.AsInt32().GetLower());
                    xmm0 = Sse2.Or(xmm0, xmm4);
                    xmm4 = Sse2.Or(xmm4, ymm15.AsInt32().GetLower());
                    xmm1 = Sse2.Or(xmm1, xmm5);
                    xmm5 = Sse2.Or(xmm5, ymm15.AsInt32().GetLower());
                    xmm0 = Sse.Subtract(xmm0.AsSingle(), xmm4.AsSingle()).AsInt32();
                    xmm1 = Sse.Subtract(xmm1.AsSingle(), xmm5.AsSingle()).AsInt32();
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 0)) = xmm0.AsSingle();
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 4)) = xmm1.AsSingle();
                }
                for (; i < length; i++)
                {
                    var xmm0 = Vector128.CreateScalarUnsafe(Unsafe.Add(ref rsi, i));
                    var xmm4 = Sse2.And(ymm8.GetLower(), xmm0);
                    xmm0 = Ssse3.Abs(xmm0).AsInt32();
                    xmm0 = Sse2.Add(xmm0, ymm15.AsInt32().GetLower());
                    xmm0 = Sse2.Or(xmm0, xmm4);
                    xmm4 = Sse2.Or(xmm4, ymm15.AsInt32().GetLower());
                    xmm0 = Sse.SubtractScalar(xmm0.AsSingle(), xmm4.AsSingle()).AsInt32();
                    Unsafe.Add(ref rdi, i + 0) = xmm0.GetElement(0);
                }
            }

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void ProcessE24Ssse3(Span<float> dest)
            {
                ref var rdi = ref MemoryMarshal.GetReference(dest);
                ref var rsi = ref Unsafe.As<float, int>(ref rdi);
                nint i = 0, length = dest.Length;
                var xmm15 = Vector128.Create(1.0f);
                var xmm8 = Vector128.Create(int.MinValue);
                var olen = length - 7;
                for (; i < olen; i += 8)
                {
                    var xmm0 = Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rsi, i + 0 * 4));
                    var xmm1 = Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rsi, i + 1 * 4));
                    var xmm4 = Sse2.And(xmm8, xmm0);
                    var xmm5 = Sse2.And(xmm8, xmm1);
                    xmm0 = Ssse3.Abs(xmm0).AsInt32();
                    xmm1 = Ssse3.Abs(xmm1).AsInt32();
                    xmm0 = Sse2.Add(xmm0, xmm15.AsInt32());
                    xmm1 = Sse2.Add(xmm1, xmm15.AsInt32());
                    xmm0 = Sse2.Or(xmm0, xmm4);
                    xmm4 = Sse2.Or(xmm4, xmm15.AsInt32());
                    xmm1 = Sse2.Or(xmm1, xmm5);
                    xmm5 = Sse2.Or(xmm5, xmm15.AsInt32());
                    xmm0 = Sse.Subtract(xmm0.AsSingle(), xmm4.AsSingle()).AsInt32();
                    xmm1 = Sse.Subtract(xmm1.AsSingle(), xmm5.AsSingle()).AsInt32();
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 0)) = xmm0.AsSingle();
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 4)) = xmm1.AsSingle();
                }
                for (; i < length; i++)
                {
                    var xmm0 = Vector128.CreateScalarUnsafe(Unsafe.Add(ref rsi, i));
                    var xmm4 = Sse2.And(xmm8, xmm0);
                    xmm0 = Ssse3.Abs(xmm0).AsInt32();
                    xmm0 = Sse2.Add(xmm0, xmm15.AsInt32());
                    xmm0 = Sse2.Or(xmm0, xmm4);
                    xmm4 = Sse2.Or(xmm4, xmm15.AsInt32());
                    xmm0 = Sse.SubtractScalar(xmm0.AsSingle(), xmm4.AsSingle()).AsInt32();
                    Unsafe.Add(ref rdi, i + 0) = xmm0.GetElement(0);
                }
            }
            #endregion

            #region 16 Effective Bits

            /// <summary>
            /// Tricky convert-and-subtract approach, witch the conversion is also done with trick.
            /// </summary>
            /// <param name="buffer"></param>
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void ProcessE16Avx2(Span<float> buffer)
            {
                ref var rdi = ref MemoryMarshal.GetReference(buffer);
                ref var rsi = ref Unsafe.As<float, int>(ref rdi);
                nint i = 0, length = buffer.Length;
                var ymm0 = Vector256.Create(0x4040_0000);
                var ymm1 = Vector256.Create(-3.0f);
                var ymm15 = Vector256.Create(0x0000_ffffu).AsInt32();
                var olen = length - 63;
                for (; i < olen; i += 64)
                {
                    var ymm2 = Avx2.And(ymm15, Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rsi, i + 0 * 8)));
                    var ymm3 = Avx2.And(ymm15, Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rsi, i + 1 * 8)));
                    var ymm4 = Avx2.And(ymm15, Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rsi, i + 2 * 8)));
                    var ymm5 = Avx2.And(ymm15, Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rsi, i + 3 * 8)));
                    ymm2 = Avx2.ShiftLeftLogical(ymm2, 7);
                    ymm3 = Avx2.ShiftLeftLogical(ymm3, 7);
                    ymm4 = Avx2.ShiftLeftLogical(ymm4, 7);
                    ymm5 = Avx2.ShiftLeftLogical(ymm5, 7);
                    ymm2 = Avx2.Xor(ymm2, ymm0);
                    ymm3 = Avx2.Xor(ymm3, ymm0);
                    ymm4 = Avx2.Xor(ymm4, ymm0);
                    ymm5 = Avx2.Xor(ymm5, ymm0);
                    ymm2 = Avx.Add(ymm2.AsSingle(), ymm1).AsInt32();
                    ymm3 = Avx.Add(ymm3.AsSingle(), ymm1).AsInt32();
                    ymm4 = Avx.Add(ymm4.AsSingle(), ymm1).AsInt32();
                    ymm5 = Avx.Add(ymm5.AsSingle(), ymm1).AsInt32();
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 0 * 8)) = ymm2.AsSingle();
                    ymm2 = Avx2.And(ymm15, Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rsi, i + 4 * 8)));
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 1 * 8)) = ymm3.AsSingle();
                    ymm3 = Avx2.And(ymm15, Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rsi, i + 5 * 8)));
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 2 * 8)) = ymm4.AsSingle();
                    ymm4 = Avx2.And(ymm15, Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rsi, i + 6 * 8)));
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 3 * 8)) = ymm5.AsSingle();
                    ymm5 = Avx2.And(ymm15, Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rsi, i + 7 * 8)));
                    ymm2 = Avx2.ShiftLeftLogical(ymm2, 7);
                    ymm3 = Avx2.ShiftLeftLogical(ymm3, 7);
                    ymm4 = Avx2.ShiftLeftLogical(ymm4, 7);
                    ymm5 = Avx2.ShiftLeftLogical(ymm5, 7);
                    ymm2 = Avx2.Xor(ymm2, ymm0);
                    ymm3 = Avx2.Xor(ymm3, ymm0);
                    ymm4 = Avx2.Xor(ymm4, ymm0);
                    ymm5 = Avx2.Xor(ymm5, ymm0);
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
                    var ymm2 = Avx2.And(ymm15, Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rsi, i + 0 * 8)));
                    var ymm3 = Avx2.And(ymm15, Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rsi, i + 1 * 8)));
                    ymm2 = Avx2.ShiftLeftLogical(ymm2, 7);
                    ymm3 = Avx2.ShiftLeftLogical(ymm3, 7);
                    ymm2 = Avx2.Xor(ymm2, ymm0);
                    ymm3 = Avx2.Xor(ymm3, ymm0);
                    ymm2 = Avx.Add(ymm2.AsSingle(), ymm1).AsInt32();
                    ymm3 = Avx.Add(ymm3.AsSingle(), ymm1).AsInt32();
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 0 * 8)) = ymm2.AsSingle();
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 1 * 8)) = ymm3.AsSingle();
                }
                for (; i < length; i++)
                {
                    var v = ((ushort)Unsafe.Add(ref rsi, i)) << 7;
                    v ^= 0x4040_0000;
                    var h = Vector128.CreateScalarUnsafe(v).AsSingle();
                    Unsafe.Add(ref rdi, i) = Sse.AddScalar(h, ymm1.GetLower()).GetElement(0);
                }
            }

            /// <summary>
            /// Tricky convert-and-subtract approach, witch the conversion is also done with trick.
            /// </summary>
            /// <param name="buffer"></param>
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void ProcessE16Sse2(Span<float> buffer)
            {
                ref var rdi = ref MemoryMarshal.GetReference(buffer);
                ref var rsi = ref Unsafe.As<float, int>(ref rdi);
                nint i = 0, length = buffer.Length;
                var xmm0 = Vector128.Create(0x4040_0000);
                var xmm1 = Vector128.Create(-3.0f);
                var xmm15 = Vector128.Create(0x0000_ffffu).AsInt32();
                var olen = length - 31;
                for (; i < olen; i += 32)
                {
                    var xmm2 = Sse2.And(xmm15, Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rsi, i + 0 * 4)));
                    var xmm3 = Sse2.And(xmm15, Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rsi, i + 1 * 4)));
                    var xmm4 = Sse2.And(xmm15, Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rsi, i + 2 * 4)));
                    var xmm5 = Sse2.And(xmm15, Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rsi, i + 3 * 4)));
                    xmm2 = Sse2.ShiftLeftLogical(xmm2, 7);
                    xmm3 = Sse2.ShiftLeftLogical(xmm3, 7);
                    xmm4 = Sse2.ShiftLeftLogical(xmm4, 7);
                    xmm5 = Sse2.ShiftLeftLogical(xmm5, 7);
                    xmm2 = Sse2.Xor(xmm2, xmm0);
                    xmm3 = Sse2.Xor(xmm3, xmm0);
                    xmm4 = Sse2.Xor(xmm4, xmm0);
                    xmm5 = Sse2.Xor(xmm5, xmm0);
                    xmm2 = Sse.Add(xmm2.AsSingle(), xmm1).AsInt32();
                    xmm3 = Sse.Add(xmm3.AsSingle(), xmm1).AsInt32();
                    xmm4 = Sse.Add(xmm4.AsSingle(), xmm1).AsInt32();
                    xmm5 = Sse.Add(xmm5.AsSingle(), xmm1).AsInt32();
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 0 * 4)) = xmm2.AsSingle();
                    xmm2 = Sse2.And(xmm15, Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rsi, i + 4 * 4)));
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 1 * 4)) = xmm3.AsSingle();
                    xmm3 = Sse2.And(xmm15, Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rsi, i + 5 * 4)));
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 2 * 4)) = xmm4.AsSingle();
                    xmm4 = Sse2.And(xmm15, Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rsi, i + 6 * 4)));
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 3 * 4)) = xmm5.AsSingle();
                    xmm5 = Sse2.And(xmm15, Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rsi, i + 7 * 4)));
                    xmm2 = Sse2.ShiftLeftLogical(xmm2, 7);
                    xmm3 = Sse2.ShiftLeftLogical(xmm3, 7);
                    xmm4 = Sse2.ShiftLeftLogical(xmm4, 7);
                    xmm5 = Sse2.ShiftLeftLogical(xmm5, 7);
                    xmm2 = Sse2.Xor(xmm2, xmm0);
                    xmm3 = Sse2.Xor(xmm3, xmm0);
                    xmm4 = Sse2.Xor(xmm4, xmm0);
                    xmm5 = Sse2.Xor(xmm5, xmm0);
                    xmm2 = Sse.Add(xmm2.AsSingle(), xmm1).AsInt32();
                    xmm3 = Sse.Add(xmm3.AsSingle(), xmm1).AsInt32();
                    xmm4 = Sse.Add(xmm4.AsSingle(), xmm1).AsInt32();
                    xmm5 = Sse.Add(xmm5.AsSingle(), xmm1).AsInt32();
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 4 * 4)) = xmm2.AsSingle();
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 5 * 4)) = xmm3.AsSingle();
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 6 * 4)) = xmm4.AsSingle();
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 7 * 4)) = xmm5.AsSingle();
                }
                olen = length - 7;
                for (; i < olen; i += 8)
                {
                    var xmm2 = Sse2.And(xmm15, Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rsi, i + 0 * 4)));
                    var xmm3 = Sse2.And(xmm15, Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rsi, i + 1 * 4)));
                    xmm2 = Sse2.ShiftLeftLogical(xmm2, 7);
                    xmm3 = Sse2.ShiftLeftLogical(xmm3, 7);
                    xmm2 = Sse2.Xor(xmm2, xmm0);
                    xmm3 = Sse2.Xor(xmm3, xmm0);
                    xmm2 = Sse.Add(xmm2.AsSingle(), xmm1).AsInt32();
                    xmm3 = Sse.Add(xmm3.AsSingle(), xmm1).AsInt32();
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 0 * 4)) = xmm2.AsSingle();
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 1 * 4)) = xmm3.AsSingle();
                }
                for (; i < length; i++)
                {
                    var v = ((ushort)Unsafe.Add(ref rsi, i)) << 7;
                    v ^= 0x4040_0000;
                    var h = Vector128.CreateScalarUnsafe(v).AsSingle();
                    Unsafe.Add(ref rdi, i) = Sse.AddScalar(h, xmm1).GetElement(0);
                }
            }
            #endregion

            #region 8 Effective Bits

            /// <summary>
            /// Tricky convert-and-subtract approach, witch the conversion is also done with trick.
            /// </summary>
            /// <param name="buffer"></param>
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void ProcessE8Avx2(Span<float> buffer)
            {
                ref var rdi = ref MemoryMarshal.GetReference(buffer);
                ref var rsi = ref Unsafe.As<float, int>(ref rdi);
                nint i = 0, length = buffer.Length;
                var ymm0 = Vector256.Create(0x4040_0000);
                var ymm1 = Vector256.Create(-3.0f);
                var ymm15 = Vector256.Create(0x0000_00ffu).AsInt32();
                var olen = length - 63;
                const int Shift = 15;
                for (; i < olen; i += 64)
                {
                    var ymm2 = Avx2.And(ymm15, Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rsi, i + 0 * 8)));
                    var ymm3 = Avx2.And(ymm15, Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rsi, i + 1 * 8)));
                    var ymm4 = Avx2.And(ymm15, Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rsi, i + 2 * 8)));
                    var ymm5 = Avx2.And(ymm15, Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rsi, i + 3 * 8)));
                    ymm2 = Avx2.ShiftLeftLogical(ymm2, Shift);
                    ymm3 = Avx2.ShiftLeftLogical(ymm3, Shift);
                    ymm4 = Avx2.ShiftLeftLogical(ymm4, Shift);
                    ymm5 = Avx2.ShiftLeftLogical(ymm5, Shift);
                    ymm2 = Avx2.Xor(ymm2, ymm0);
                    ymm3 = Avx2.Xor(ymm3, ymm0);
                    ymm4 = Avx2.Xor(ymm4, ymm0);
                    ymm5 = Avx2.Xor(ymm5, ymm0);
                    ymm2 = Avx.Add(ymm2.AsSingle(), ymm1).AsInt32();
                    ymm3 = Avx.Add(ymm3.AsSingle(), ymm1).AsInt32();
                    ymm4 = Avx.Add(ymm4.AsSingle(), ymm1).AsInt32();
                    ymm5 = Avx.Add(ymm5.AsSingle(), ymm1).AsInt32();
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 0 * 8)) = ymm2.AsSingle();
                    ymm2 = Avx2.And(ymm15, Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rsi, i + 4 * 8)));
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 1 * 8)) = ymm3.AsSingle();
                    ymm3 = Avx2.And(ymm15, Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rsi, i + 5 * 8)));
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 2 * 8)) = ymm4.AsSingle();
                    ymm4 = Avx2.And(ymm15, Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rsi, i + 6 * 8)));
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 3 * 8)) = ymm5.AsSingle();
                    ymm5 = Avx2.And(ymm15, Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rsi, i + 7 * 8)));
                    ymm2 = Avx2.ShiftLeftLogical(ymm2, Shift);
                    ymm3 = Avx2.ShiftLeftLogical(ymm3, Shift);
                    ymm4 = Avx2.ShiftLeftLogical(ymm4, Shift);
                    ymm5 = Avx2.ShiftLeftLogical(ymm5, Shift);
                    ymm2 = Avx2.Xor(ymm2, ymm0);
                    ymm3 = Avx2.Xor(ymm3, ymm0);
                    ymm4 = Avx2.Xor(ymm4, ymm0);
                    ymm5 = Avx2.Xor(ymm5, ymm0);
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
                    var ymm2 = Avx2.And(ymm15, Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rsi, i + 0 * 8)));
                    var ymm3 = Avx2.And(ymm15, Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rsi, i + 1 * 8)));
                    ymm2 = Avx2.ShiftLeftLogical(ymm2, Shift);
                    ymm3 = Avx2.ShiftLeftLogical(ymm3, Shift);
                    ymm2 = Avx2.Xor(ymm2, ymm0);
                    ymm3 = Avx2.Xor(ymm3, ymm0);
                    ymm2 = Avx.Add(ymm2.AsSingle(), ymm1).AsInt32();
                    ymm3 = Avx.Add(ymm3.AsSingle(), ymm1).AsInt32();
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 0 * 8)) = ymm2.AsSingle();
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 1 * 8)) = ymm3.AsSingle();
                }
                for (; i < length; i++)
                {
                    var v = ((byte)Unsafe.Add(ref rsi, i)) << Shift;
                    v ^= 0x4040_0000;
                    var h = Vector128.CreateScalarUnsafe(v).AsSingle();
                    Unsafe.Add(ref rdi, i) = Sse.AddScalar(h, ymm1.GetLower()).GetElement(0);
                }
            }

            /// <summary>
            /// Tricky convert-and-subtract approach, witch the conversion is also done with trick.
            /// </summary>
            /// <param name="buffer"></param>
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void ProcessE8Sse2(Span<float> buffer)
            {
                ref var rdi = ref MemoryMarshal.GetReference(buffer);
                ref var rsi = ref Unsafe.As<float, int>(ref rdi);
                nint i = 0, length = buffer.Length;
                var xmm0 = Vector128.Create(0x4040_0000);
                var xmm1 = Vector128.Create(-3.0f);
                var xmm15 = Vector128.Create(0x0000_00ffu).AsInt32();
                const int Shift = 15;
                var olen = length - 31;
                for (; i < olen; i += 32)
                {
                    var xmm2 = Sse2.And(xmm15, Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rsi, i + 0 * 4)));
                    var xmm3 = Sse2.And(xmm15, Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rsi, i + 1 * 4)));
                    var xmm4 = Sse2.And(xmm15, Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rsi, i + 2 * 4)));
                    var xmm5 = Sse2.And(xmm15, Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rsi, i + 3 * 4)));
                    xmm2 = Sse2.ShiftLeftLogical(xmm2, Shift);
                    xmm3 = Sse2.ShiftLeftLogical(xmm3, Shift);
                    xmm4 = Sse2.ShiftLeftLogical(xmm4, Shift);
                    xmm5 = Sse2.ShiftLeftLogical(xmm5, Shift);
                    xmm2 = Sse2.Xor(xmm2, xmm0);
                    xmm3 = Sse2.Xor(xmm3, xmm0);
                    xmm4 = Sse2.Xor(xmm4, xmm0);
                    xmm5 = Sse2.Xor(xmm5, xmm0);
                    xmm2 = Sse.Add(xmm2.AsSingle(), xmm1).AsInt32();
                    xmm3 = Sse.Add(xmm3.AsSingle(), xmm1).AsInt32();
                    xmm4 = Sse.Add(xmm4.AsSingle(), xmm1).AsInt32();
                    xmm5 = Sse.Add(xmm5.AsSingle(), xmm1).AsInt32();
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 0 * 4)) = xmm2.AsSingle();
                    xmm2 = Sse2.And(xmm15, Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rsi, i + 4 * 4)));
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 1 * 4)) = xmm3.AsSingle();
                    xmm3 = Sse2.And(xmm15, Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rsi, i + 5 * 4)));
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 2 * 4)) = xmm4.AsSingle();
                    xmm4 = Sse2.And(xmm15, Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rsi, i + 6 * 4)));
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 3 * 4)) = xmm5.AsSingle();
                    xmm5 = Sse2.And(xmm15, Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rsi, i + 7 * 4)));
                    xmm2 = Sse2.ShiftLeftLogical(xmm2, Shift);
                    xmm3 = Sse2.ShiftLeftLogical(xmm3, Shift);
                    xmm4 = Sse2.ShiftLeftLogical(xmm4, Shift);
                    xmm5 = Sse2.ShiftLeftLogical(xmm5, Shift);
                    xmm2 = Sse2.Xor(xmm2, xmm0);
                    xmm3 = Sse2.Xor(xmm3, xmm0);
                    xmm4 = Sse2.Xor(xmm4, xmm0);
                    xmm5 = Sse2.Xor(xmm5, xmm0);
                    xmm2 = Sse.Add(xmm2.AsSingle(), xmm1).AsInt32();
                    xmm3 = Sse.Add(xmm3.AsSingle(), xmm1).AsInt32();
                    xmm4 = Sse.Add(xmm4.AsSingle(), xmm1).AsInt32();
                    xmm5 = Sse.Add(xmm5.AsSingle(), xmm1).AsInt32();
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 4 * 4)) = xmm2.AsSingle();
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 5 * 4)) = xmm3.AsSingle();
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 6 * 4)) = xmm4.AsSingle();
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 7 * 4)) = xmm5.AsSingle();
                }
                olen = length - 7;
                for (; i < olen; i += 8)
                {
                    var xmm2 = Sse2.And(xmm15, Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rsi, i + 0 * 4)));
                    var xmm3 = Sse2.And(xmm15, Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rsi, i + 1 * 4)));
                    xmm2 = Sse2.ShiftLeftLogical(xmm2, Shift);
                    xmm3 = Sse2.ShiftLeftLogical(xmm3, Shift);
                    xmm2 = Sse2.Xor(xmm2, xmm0);
                    xmm3 = Sse2.Xor(xmm3, xmm0);
                    xmm2 = Sse.Add(xmm2.AsSingle(), xmm1).AsInt32();
                    xmm3 = Sse.Add(xmm3.AsSingle(), xmm1).AsInt32();
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 0 * 4)) = xmm2.AsSingle();
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 1 * 4)) = xmm3.AsSingle();
                }
                for (; i < length; i++)
                {
                    var v = ((byte)Unsafe.Add(ref rsi, i)) << Shift;
                    v ^= 0x4040_0000;
                    var h = Vector128.CreateScalarUnsafe(v).AsSingle();
                    Unsafe.Add(ref rdi, i) = Sse.AddScalar(h, xmm1).GetElement(0);
                }
            }
            #endregion

            #region Variable Effective Bit Depth
            #region <24
            /// <summary>
            /// Tricky convert-and-subtract approach, witch the conversion is also done with trick.
            /// </summary>
            /// <param name="buffer"></param>
            /// <param name="effectiveBitDepth"></param>
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void ProcessELessThan24Avx2(Span<float> buffer, int effectiveBitDepth)
            {
                ref var rdi = ref MemoryMarshal.GetReference(buffer);
                ref var rsi = ref Unsafe.As<float, int>(ref rdi);
                nint i = 0, length = buffer.Length;
                var ymm0 = Vector256.Create(0x4040_0000);
                var ymm1 = Vector256.Create(-3.0f);
                var mask = (int)MathI.ZeroHighBits(effectiveBitDepth, ~0u);
                var ymm15 = Vector256.Create(mask);
                var shift = 23 - effectiveBitDepth;
                var xmm14 = Vector128.CreateScalarUnsafe((long)shift).AsInt32();
                var olen = length - 63;
                for (; i < olen; i += 64)
                {
                    var ymm2 = Avx2.And(ymm15, Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rsi, i + 0 * 8)));
                    var ymm3 = Avx2.And(ymm15, Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rsi, i + 1 * 8)));
                    var ymm4 = Avx2.And(ymm15, Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rsi, i + 2 * 8)));
                    var ymm5 = Avx2.And(ymm15, Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rsi, i + 3 * 8)));
                    ymm2 = Avx2.ShiftLeftLogical(ymm2, xmm14);
                    ymm3 = Avx2.ShiftLeftLogical(ymm3, xmm14);
                    ymm4 = Avx2.ShiftLeftLogical(ymm4, xmm14);
                    ymm5 = Avx2.ShiftLeftLogical(ymm5, xmm14);
                    ymm2 = Avx2.Xor(ymm2, ymm0);
                    ymm3 = Avx2.Xor(ymm3, ymm0);
                    ymm4 = Avx2.Xor(ymm4, ymm0);
                    ymm5 = Avx2.Xor(ymm5, ymm0);
                    ymm2 = Avx.Add(ymm2.AsSingle(), ymm1).AsInt32();
                    ymm3 = Avx.Add(ymm3.AsSingle(), ymm1).AsInt32();
                    ymm4 = Avx.Add(ymm4.AsSingle(), ymm1).AsInt32();
                    ymm5 = Avx.Add(ymm5.AsSingle(), ymm1).AsInt32();
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 0 * 8)) = ymm2.AsSingle();
                    ymm2 = Avx2.And(ymm15, Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rsi, i + 4 * 8)));
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 1 * 8)) = ymm3.AsSingle();
                    ymm3 = Avx2.And(ymm15, Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rsi, i + 5 * 8)));
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 2 * 8)) = ymm4.AsSingle();
                    ymm4 = Avx2.And(ymm15, Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rsi, i + 6 * 8)));
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 3 * 8)) = ymm5.AsSingle();
                    ymm5 = Avx2.And(ymm15, Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rsi, i + 7 * 8)));
                    ymm2 = Avx2.ShiftLeftLogical(ymm2, xmm14);
                    ymm3 = Avx2.ShiftLeftLogical(ymm3, xmm14);
                    ymm4 = Avx2.ShiftLeftLogical(ymm4, xmm14);
                    ymm5 = Avx2.ShiftLeftLogical(ymm5, xmm14);
                    ymm2 = Avx2.Xor(ymm2, ymm0);
                    ymm3 = Avx2.Xor(ymm3, ymm0);
                    ymm4 = Avx2.Xor(ymm4, ymm0);
                    ymm5 = Avx2.Xor(ymm5, ymm0);
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
                    var ymm2 = Avx2.And(ymm15, Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rsi, i + 0 * 8)));
                    var ymm3 = Avx2.And(ymm15, Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rsi, i + 1 * 8)));
                    ymm2 = Avx2.ShiftLeftLogical(ymm2, xmm14);
                    ymm3 = Avx2.ShiftLeftLogical(ymm3, xmm14);
                    ymm2 = Avx2.Xor(ymm2, ymm0);
                    ymm3 = Avx2.Xor(ymm3, ymm0);
                    ymm2 = Avx.Add(ymm2.AsSingle(), ymm1).AsInt32();
                    ymm3 = Avx.Add(ymm3.AsSingle(), ymm1).AsInt32();
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 0 * 8)) = ymm2.AsSingle();
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 1 * 8)) = ymm3.AsSingle();
                }
                for (; i < length; i++)
                {
                    var v = (mask & Unsafe.Add(ref rsi, i)) << shift;
                    v ^= 0x4040_0000;
                    var h = Vector128.CreateScalarUnsafe(v).AsSingle();
                    Unsafe.Add(ref rdi, i) = Sse.AddScalar(h, ymm1.GetLower()).GetElement(0);
                }
            }

            /// <summary>
            /// Tricky convert-and-subtract approach, witch the conversion is also done with trick.<br/>
            /// Optimized for environments where <see cref="IntrinsicsUtils.PreferShiftVariable"/> is <c>true</c>.
            /// </summary>
            /// <param name="buffer"></param>
            /// <param name="effectiveBitDepth"></param>
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void ProcessELessThan24Avx2PSV(Span<float> buffer, int effectiveBitDepth)
            {
                ref var rdi = ref MemoryMarshal.GetReference(buffer);
                ref var rsi = ref Unsafe.As<float, int>(ref rdi);
                nint i = 0, length = buffer.Length;
                var ymm0 = Vector256.Create(0x4040_0000);
                var ymm1 = Vector256.Create(-3.0f);
                var mask = (int)MathI.ZeroHighBits(effectiveBitDepth, ~0u);
                var ymm15 = Vector256.Create(mask);
                var shift = 23 - effectiveBitDepth;
                var ymm14 = Vector256.Create(shift).AsUInt32();
                var olen = length - 63;
                for (; i < olen; i += 64)
                {
                    var ymm2 = Avx2.And(ymm15, Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rsi, i + 0 * 8)));
                    var ymm3 = Avx2.And(ymm15, Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rsi, i + 1 * 8)));
                    var ymm4 = Avx2.And(ymm15, Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rsi, i + 2 * 8)));
                    var ymm5 = Avx2.And(ymm15, Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rsi, i + 3 * 8)));
                    ymm2 = Avx2.ShiftLeftLogicalVariable(ymm2, ymm14);
                    ymm3 = Avx2.ShiftLeftLogicalVariable(ymm3, ymm14);
                    ymm4 = Avx2.ShiftLeftLogicalVariable(ymm4, ymm14);
                    ymm5 = Avx2.ShiftLeftLogicalVariable(ymm5, ymm14);
                    ymm2 = Avx2.Xor(ymm2, ymm0);
                    ymm3 = Avx2.Xor(ymm3, ymm0);
                    ymm4 = Avx2.Xor(ymm4, ymm0);
                    ymm5 = Avx2.Xor(ymm5, ymm0);
                    ymm2 = Avx.Add(ymm2.AsSingle(), ymm1).AsInt32();
                    ymm3 = Avx.Add(ymm3.AsSingle(), ymm1).AsInt32();
                    ymm4 = Avx.Add(ymm4.AsSingle(), ymm1).AsInt32();
                    ymm5 = Avx.Add(ymm5.AsSingle(), ymm1).AsInt32();
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 0 * 8)) = ymm2.AsSingle();
                    ymm2 = Avx2.And(ymm15, Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rsi, i + 4 * 8)));
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 1 * 8)) = ymm3.AsSingle();
                    ymm3 = Avx2.And(ymm15, Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rsi, i + 5 * 8)));
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 2 * 8)) = ymm4.AsSingle();
                    ymm4 = Avx2.And(ymm15, Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rsi, i + 6 * 8)));
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 3 * 8)) = ymm5.AsSingle();
                    ymm5 = Avx2.And(ymm15, Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rsi, i + 7 * 8)));
                    ymm2 = Avx2.ShiftLeftLogicalVariable(ymm2, ymm14);
                    ymm3 = Avx2.ShiftLeftLogicalVariable(ymm3, ymm14);
                    ymm4 = Avx2.ShiftLeftLogicalVariable(ymm4, ymm14);
                    ymm5 = Avx2.ShiftLeftLogicalVariable(ymm5, ymm14);
                    ymm2 = Avx2.Xor(ymm2, ymm0);
                    ymm3 = Avx2.Xor(ymm3, ymm0);
                    ymm4 = Avx2.Xor(ymm4, ymm0);
                    ymm5 = Avx2.Xor(ymm5, ymm0);
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
                    var ymm2 = Avx2.And(ymm15, Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rsi, i + 0 * 8)));
                    var ymm3 = Avx2.And(ymm15, Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref rsi, i + 1 * 8)));
                    ymm2 = Avx2.ShiftLeftLogicalVariable(ymm2, ymm14);
                    ymm3 = Avx2.ShiftLeftLogicalVariable(ymm3, ymm14);
                    ymm2 = Avx2.Xor(ymm2, ymm0);
                    ymm3 = Avx2.Xor(ymm3, ymm0);
                    ymm2 = Avx.Add(ymm2.AsSingle(), ymm1).AsInt32();
                    ymm3 = Avx.Add(ymm3.AsSingle(), ymm1).AsInt32();
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 0 * 8)) = ymm2.AsSingle();
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 1 * 8)) = ymm3.AsSingle();
                }
                for (; i < length; i++)
                {
                    var v = (mask & Unsafe.Add(ref rsi, i)) << shift;
                    v ^= 0x4040_0000;
                    var h = Vector128.CreateScalarUnsafe(v).AsSingle();
                    Unsafe.Add(ref rdi, i) = Sse.AddScalar(h, ymm1.GetLower()).GetElement(0);
                }
            }

            /// <summary>
            /// Tricky convert-and-subtract approach, witch the conversion is also done with trick.
            /// </summary>
            /// <param name="buffer"></param>
            /// <param name="effectiveBitDepth"></param>
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void ProcessELessThan24Sse2(Span<float> buffer, int effectiveBitDepth)
            {
                ref var rdi = ref MemoryMarshal.GetReference(buffer);
                ref var rsi = ref Unsafe.As<float, int>(ref rdi);
                nint i = 0, length = buffer.Length;
                var xmm0 = Vector128.Create(0x4040_0000);
                var xmm1 = Vector128.Create(-3.0f);
                var mask = (int)MathI.ZeroHighBits(effectiveBitDepth, ~0u);
                var xmm15 = Vector128.Create(mask);
                var shift = 23 - effectiveBitDepth;
                var xmm14 = Vector128.CreateScalarUnsafe((long)shift).AsInt32();
                var olen = length - 31;
                for (; i < olen; i += 32)
                {
                    var xmm2 = Sse2.And(xmm15, Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rsi, i + 0 * 4)));
                    var xmm3 = Sse2.And(xmm15, Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rsi, i + 1 * 4)));
                    var xmm4 = Sse2.And(xmm15, Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rsi, i + 2 * 4)));
                    var xmm5 = Sse2.And(xmm15, Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rsi, i + 3 * 4)));
                    xmm2 = Sse2.ShiftLeftLogical(xmm2, xmm14);
                    xmm3 = Sse2.ShiftLeftLogical(xmm3, xmm14);
                    xmm4 = Sse2.ShiftLeftLogical(xmm4, xmm14);
                    xmm5 = Sse2.ShiftLeftLogical(xmm5, xmm14);
                    xmm2 = Sse2.Xor(xmm2, xmm0);
                    xmm3 = Sse2.Xor(xmm3, xmm0);
                    xmm4 = Sse2.Xor(xmm4, xmm0);
                    xmm5 = Sse2.Xor(xmm5, xmm0);
                    xmm2 = Sse.Add(xmm2.AsSingle(), xmm1).AsInt32();
                    xmm3 = Sse.Add(xmm3.AsSingle(), xmm1).AsInt32();
                    xmm4 = Sse.Add(xmm4.AsSingle(), xmm1).AsInt32();
                    xmm5 = Sse.Add(xmm5.AsSingle(), xmm1).AsInt32();
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 0 * 4)) = xmm2.AsSingle();
                    xmm2 = Sse2.And(xmm15, Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rsi, i + 4 * 4)));
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 1 * 4)) = xmm3.AsSingle();
                    xmm3 = Sse2.And(xmm15, Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rsi, i + 5 * 4)));
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 2 * 4)) = xmm4.AsSingle();
                    xmm4 = Sse2.And(xmm15, Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rsi, i + 6 * 4)));
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 3 * 4)) = xmm5.AsSingle();
                    xmm5 = Sse2.And(xmm15, Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rsi, i + 7 * 4)));
                    xmm2 = Sse2.ShiftLeftLogical(xmm2, xmm14);
                    xmm3 = Sse2.ShiftLeftLogical(xmm3, xmm14);
                    xmm4 = Sse2.ShiftLeftLogical(xmm4, xmm14);
                    xmm5 = Sse2.ShiftLeftLogical(xmm5, xmm14);
                    xmm2 = Sse2.Xor(xmm2, xmm0);
                    xmm3 = Sse2.Xor(xmm3, xmm0);
                    xmm4 = Sse2.Xor(xmm4, xmm0);
                    xmm5 = Sse2.Xor(xmm5, xmm0);
                    xmm2 = Sse.Add(xmm2.AsSingle(), xmm1).AsInt32();
                    xmm3 = Sse.Add(xmm3.AsSingle(), xmm1).AsInt32();
                    xmm4 = Sse.Add(xmm4.AsSingle(), xmm1).AsInt32();
                    xmm5 = Sse.Add(xmm5.AsSingle(), xmm1).AsInt32();
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 4 * 4)) = xmm2.AsSingle();
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 5 * 4)) = xmm3.AsSingle();
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 6 * 4)) = xmm4.AsSingle();
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 7 * 4)) = xmm5.AsSingle();
                }
                olen = length - 7;
                for (; i < olen; i += 8)
                {
                    var xmm2 = Sse2.And(xmm15, Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rsi, i + 0 * 4)));
                    var xmm3 = Sse2.And(xmm15, Unsafe.As<int, Vector128<int>>(ref Unsafe.Add(ref rsi, i + 1 * 4)));
                    xmm2 = Sse2.ShiftLeftLogical(xmm2, xmm14);
                    xmm3 = Sse2.ShiftLeftLogical(xmm3, xmm14);
                    xmm2 = Sse2.Xor(xmm2, xmm0);
                    xmm3 = Sse2.Xor(xmm3, xmm0);
                    xmm2 = Sse.Add(xmm2.AsSingle(), xmm1).AsInt32();
                    xmm3 = Sse.Add(xmm3.AsSingle(), xmm1).AsInt32();
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 0 * 4)) = xmm2.AsSingle();
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 1 * 4)) = xmm3.AsSingle();
                }
                for (; i < length; i++)
                {
                    var v = (mask & Unsafe.Add(ref rsi, i)) << shift;
                    v ^= 0x4040_0000;
                    var h = Vector128.CreateScalarUnsafe(v).AsSingle();
                    Unsafe.Add(ref rdi, i) = Sse.AddScalar(h, xmm1).GetElement(0);
                }
            }

            #endregion
            #region >24
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void ProcessEMoreThan24Avx2(Span<float> buffer, int effectiveBitDepth)
            {
                var mul = Vector256.Create(2.0f / (1 << effectiveBitDepth));
                ref var rdi = ref MemoryMarshal.GetReference(buffer);
                nint i, length = buffer.Length;
                //Loop for Intel CPUs in 256-bit AVX2 for better throughput
                for (i = 0; i < length - 63; i += 64)
                {
                    var ymm0 = Avx.ConvertToVector256Single(Unsafe.As<float, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 0)));
                    var ymm1 = Avx.ConvertToVector256Single(Unsafe.As<float, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 8)));
                    var ymm2 = Avx.ConvertToVector256Single(Unsafe.As<float, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 16)));
                    var ymm3 = Avx.ConvertToVector256Single(Unsafe.As<float, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 24)));
                    ymm0 = Avx.Multiply(ymm0, mul);
                    ymm1 = Avx.Multiply(ymm1, mul);
                    ymm2 = Avx.Multiply(ymm2, mul);
                    ymm3 = Avx.Multiply(ymm3, mul);
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 0)) = ymm0;
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 8)) = ymm1;
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 16)) = ymm2;
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 24)) = ymm3;
                    ymm0 = Avx.ConvertToVector256Single(Unsafe.As<float, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 32)));
                    ymm1 = Avx.ConvertToVector256Single(Unsafe.As<float, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 40)));
                    ymm2 = Avx.ConvertToVector256Single(Unsafe.As<float, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 48)));
                    ymm3 = Avx.ConvertToVector256Single(Unsafe.As<float, Vector256<int>>(ref Unsafe.Add(ref rdi, i + 56)));
                    ymm0 = Avx.Multiply(ymm0, mul);
                    ymm1 = Avx.Multiply(ymm1, mul);
                    ymm2 = Avx.Multiply(ymm2, mul);
                    ymm3 = Avx.Multiply(ymm3, mul);
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 32)) = ymm0;
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 40)) = ymm1;
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 48)) = ymm2;
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref rdi, i + 56)) = ymm3;
                }
                for (; i < length - 7; i += 8)
                {
                    var xmm0 = Sse2.ConvertToVector128Single(Unsafe.As<float, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 0)));
                    var xmm1 = Sse2.ConvertToVector128Single(Unsafe.As<float, Vector128<int>>(ref Unsafe.Add(ref rdi, i + 4)));
                    xmm0 = Sse.Multiply(xmm0, mul.GetLower());
                    xmm1 = Sse.Multiply(xmm1, mul.GetLower());
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 0)) = xmm0;
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rdi, i + 4)) = xmm1;
                }
                for (; i < length; i++)
                {
                    Unsafe.Add(ref rdi, i) = Unsafe.As<float, int>(ref Unsafe.Add(ref rdi, i)) * mul.GetElement(0);
                }
            }
            #endregion
            #endregion
        }
    }
}
#endif