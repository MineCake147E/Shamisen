#if NETCOREAPP3_1_OR_GREATER

#region License notice

/* libFLAC - Free Lossless Audio Codec library
 * Copyright (C) 2000-2009  Josh Coalson
 * Copyright (C) 2011-2018  Xiph.Org Foundation
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 *
 * - Redistributions of source code must retain the above copyright
 * notice, this list of conditions and the following disclaimer.
 *
 * - Redistributions in binary form must reproduce the above copyright
 * notice, this list of conditions and the following disclaimer in the
 * documentation and/or other materials provided with the distribution.
 *
 * - Neither the name of the Xiph.org Foundation nor the names of its
 * contributors may be used to endorse or promote products derived from
 * this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * ``AS IS'' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 * LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
 * A PARTICULAR PURPOSE ARE DISCLAIMED.  IN NO EVENT SHALL THE FOUNDATION OR
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
 * EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
 * PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 * LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

#endregion License notice

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Codecs.Flac.SubFrames
{
    public sealed partial class FlacLinearPredictionSubFrame
    {
        internal static partial class X86
        {
            internal static bool IsSupported =>
#if NET5_0_OR_GREATER
                X86Base.IsSupported;

#else
                Sse.IsSupported;
#endif

            #region Order2

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static bool RestoreSignalOrder2(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
            {
                if (Sse41.IsSupported)
                {
                    RestoreSignalOrder2Sse41(shiftsNeeded, residual, coeffs, output);
                    return true;
                }
                return false;
            }

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static unsafe void RestoreSignalOrder2Sse41(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
            {
                const int Order = 2;
                if (coeffs.Length < Order) return;
                _ = coeffs[Order - 1];
                ref var o = ref MemoryMarshal.GetReference(output);
                ref var d = ref Unsafe.Add(ref o, Order);
                ref var r = ref MemoryMarshal.GetReference(residual);
                ref var c = ref MemoryMarshal.GetReference(coeffs);
                var vcoeff = Sse2.LoadScalarVector128((ulong*)Unsafe.AsPointer(ref c)).AsInt32();
                var vshift = Vector128.CreateScalar(shiftsNeeded);
                var vzero = Vector128.Create(0);
                var vprev = Sse2.LoadScalarVector128((ulong*)Unsafe.AsPointer(ref o)).AsInt32();
                vprev = Sse2.Shuffle(vprev, 0b00_01_10_11);
                nint dataLength = output.Length - Order;
                for (nint i = 0; i < dataLength;)
                {
                    var res = Vector128.CreateScalar(Unsafe.Add(ref r, i));
                    var y = Sse41.MultiplyLow(vcoeff, vprev);
                    y = Ssse3.HorizontalAdd(y, vzero);
                    y = Sse2.ShiftRightArithmetic(y, vshift);   //C# shift operator handling sucks so SSE2 is used instead(same latency, same throughput).
                    y = Sse2.Add(y, res);   //Avoids extract and insert
#if NET5_0_OR_GREATER
                    Sse2.StoreScalar((int*)Unsafe.AsPointer(ref Unsafe.Add(ref d, i)), y);
#else
                    Unsafe.Add(ref d, i) = y.GetElement(0);
#endif
                    i++;
                    vprev = Sse2.Shuffle(vprev, 0b11_11_00_00);
                    vprev = Sse41.Blend(vprev.AsSingle(), y.AsSingle(), 0b0000_1101).AsInt32();
                }
            }

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static bool RestoreSignalOrder2Wide(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
            {
                if (Sse41.IsSupported)
                {
                    RestoreSignalOrder2WideSse41(shiftsNeeded, residual, coeffs, output);
                    return true;
                }
                return false;
            }

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static unsafe void RestoreSignalOrder2WideSse41(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
            {
                const int Order = 2;
                if (coeffs.Length < Order) return;
                _ = coeffs[Order - 1];
                ref var o = ref MemoryMarshal.GetReference(output);
                ref var d = ref Unsafe.Add(ref o, Order);
                ref var r = ref MemoryMarshal.GetReference(residual);
                ref var c = ref MemoryMarshal.GetReference(coeffs);
                var vcoeff = Sse41.ConvertToVector128Int64(Sse2.LoadScalarVector128((ulong*)Unsafe.AsPointer(ref c)).AsUInt32()).AsInt32();
                var vprev = Sse2.LoadScalarVector128((ulong*)Unsafe.AsPointer(ref o)).AsInt32();
                vprev = Sse2.Shuffle(vprev, 0b11_00_11_01);
                var vshift = Vector128.CreateScalar((long)shiftsNeeded);
                nint dataLength = output.Length - Order;
                for (nint i = 0; i < dataLength;)
                {
                    var res = Vector128.CreateScalar(Unsafe.Add(ref r, i));
                    var y = Sse41.Multiply(vcoeff, vprev);
                    y = Sse2.Add(y, Sse2.ShiftRightLogical128BitLane(y, 8));
                    y = Sse2.ShiftRightLogical(y, vshift);
                    var yy = Sse2.Add(y.AsInt32(), res);
#if NET5_0_OR_GREATER
                    Sse2.StoreScalar((int*)Unsafe.AsPointer(ref Unsafe.Add(ref d, i)), yy);
#else
                    Unsafe.Add(ref d, i) = yy.GetElement(0);
#endif
                    i++;
                    vprev = Sse2.Shuffle(vprev, 0b11_00_11_00);
                    vprev = Sse41.Blend(vprev.AsSingle(), yy.AsSingle(), 0b0000_0001).AsInt32();
                }
            }

            #endregion Order2

            #region Order3

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static bool RestoreSignalOrder3(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
            {
                if (Avx.IsSupported)
                {
                    RestoreSignalOrder3Avx(shiftsNeeded, residual, coeffs, output);
                    return true;
                }
                if (Sse41.IsSupported)
                {
                    RestoreSignalOrder3Sse41(shiftsNeeded, residual, coeffs, output);
                    return true;
                }
                return false;
            }

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static unsafe void RestoreSignalOrder3Sse41(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
            {
                const int Order = 3;
                if (coeffs.Length < Order) return;
                _ = coeffs[Order - 1];
                ref var o = ref MemoryMarshal.GetReference(output);
                ref var d = ref Unsafe.Add(ref o, Order);
                ref var r = ref MemoryMarshal.GetReference(residual);
                ref var c = ref MemoryMarshal.GetReference(coeffs);
                var vcoeff = Vector128.Create(c, Unsafe.Add(ref c, 1), Unsafe.Add(ref c, 2), 0);
                var vshift = Vector128.CreateScalar(shiftsNeeded);
                var vzero = Vector128.Create(0);
                var vprev = Vector128.Create(o, Unsafe.Add(ref o, 1), Unsafe.Add(ref o, 2), 0);
                vprev = Sse2.Shuffle(vprev, 0b00_00_01_10);
                nint dataLength = output.Length - Order;
                for (nint i = 0; i < dataLength;)
                {
                    var res = Vector128.CreateScalar(Unsafe.Add(ref r, i));
                    var y = Sse41.MultiplyLow(vcoeff, vprev);
                    y = Ssse3.HorizontalAdd(y, vzero);
                    y = Ssse3.HorizontalAdd(y, vzero);
                    y = Sse2.ShiftRightArithmetic(y, vshift);   //C# shift operator handling sucks so SSE2 is used instead(same latency, same throughput).
                    y = Sse2.Add(y, res);   //Avoids extract and insert
#if NET5_0_OR_GREATER
                    Sse2.StoreScalar((int*)Unsafe.AsPointer(ref Unsafe.Add(ref d, i)), y);
#else
                    Unsafe.Add(ref d, i) = y.GetElement(0);
#endif
                    i++;
                    vprev = Sse2.Shuffle(vprev, 0b00_01_00_00);
                    vprev = Sse41.Blend(vprev.AsSingle(), y.AsSingle(), 0b0000_1001).AsInt32();
                }
            }

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static unsafe void RestoreSignalOrder3Avx(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
            {
                const int Order = 3;
                if (coeffs.Length < Order) return;
                _ = coeffs[Order - 1];
                ref var o = ref MemoryMarshal.GetReference(output);
                ref var d = ref Unsafe.Add(ref o, Order);
                ref var r = ref MemoryMarshal.GetReference(residual);
                ref var c = ref MemoryMarshal.GetReference(coeffs);
                var mask = Vector128.Create(~0, ~0, ~0, 0).AsSingle();
                var vcoeff = Avx.MaskLoad((float*)Unsafe.AsPointer(ref c), mask).AsInt32();
                var vshift = Vector128.CreateScalar(shiftsNeeded);
                var vzero = Vector128.Create(0);
                var vprev = Avx.MaskLoad((float*)Unsafe.AsPointer(ref o), mask).AsInt32();
                vprev = Sse2.Shuffle(vprev, 0b00_00_01_10);
                nint dataLength = output.Length - Order;
                for (nint i = 0; i < dataLength;)
                {
                    var res = Vector128.CreateScalar(Unsafe.Add(ref r, i));
                    var y = Sse41.MultiplyLow(vcoeff, vprev);
                    y = Ssse3.HorizontalAdd(y, vzero);
                    y = Ssse3.HorizontalAdd(y, vzero);
                    y = Sse2.ShiftRightArithmetic(y, vshift);   //C# shift operator handling sucks so SSE2 is used instead(same latency, same throughput).
                    y = Sse2.Add(y, res);   //Avoiding extract and insert reduces overall latency and increase throughput.
#if NET5_0_OR_GREATER
                    Sse2.StoreScalar((int*)Unsafe.AsPointer(ref Unsafe.Add(ref d, i)), y);
#else
                    Unsafe.Add(ref d, i) = y.GetElement(0); //netcoreapp3.1 needs 1 more operation
#endif
                    i++;
                    vprev = Sse2.Shuffle(vprev, 0b00_01_00_00);
                    vprev = Sse41.Blend(vprev.AsSingle(), y.AsSingle(), 0b0000_1001).AsInt32();
                }
            }

            #endregion Order3

            #region Order4

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static bool RestoreSignalOrder4(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
            {
                if (Sse41.IsSupported)
                {
                    RestoreSignalOrder4Sse41(shiftsNeeded, residual, coeffs, output);
                    return true;
                }
                return false;
            }

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static unsafe void RestoreSignalOrder4Sse41(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
            {
                const int Order = 4;
                if (coeffs.Length < Order) return;
                _ = coeffs[Order - 1];
                ref var o = ref MemoryMarshal.GetReference(output);
                ref var d = ref Unsafe.Add(ref o, Order);
                ref var r = ref MemoryMarshal.GetReference(residual);
                var vcoeff = Unsafe.As<int, Vector128<int>>(ref MemoryMarshal.GetReference(coeffs));
                var vshift = Vector128.CreateScalar(shiftsNeeded);
                var vzero = Vector128.Create(0);
                var vprev = Unsafe.As<int, Vector128<int>>(ref o);
                vprev = Sse2.Shuffle(vprev, 0b00_01_10_11);
                nint dataLength = output.Length - Order;
                for (nint i = 0; i < dataLength; i++)
                {
                    var res = Vector128.CreateScalar(Unsafe.Add(ref r, i));
                    var y = Sse41.MultiplyLow(vcoeff, vprev);
                    y = Ssse3.HorizontalAdd(y, vzero);
                    y = Ssse3.HorizontalAdd(y, vzero);
                    y = Sse2.ShiftRightArithmetic(y, vshift);   //C# shift operator handling sucks so SSE2 is used instead(same latency, same throughput).
                    y = Sse2.Add(y, res);   //Avoids extract and insert
#if NET5_0_OR_GREATER
                    Sse2.StoreScalar((int*)Unsafe.AsPointer(ref Unsafe.Add(ref d, i)), y);
#else
                    Unsafe.Add(ref d, i) = y.GetElement(0);
#endif
                    vprev = Sse2.Shuffle(vprev, 0b10_01_00_00);
                    vprev = Sse41.Blend(vprev.AsSingle(), y.AsSingle(), 0b0000_0001).AsInt32();
                }
            }

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static bool RestoreSignalOrder4Wide(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
            {
                if (Avx2.IsSupported)
                {
                    RestoreSignalOrder4WideAvx2(shiftsNeeded, residual, coeffs, output);
                    return true;
                }
                if (Sse41.IsSupported)
                {
                    RestoreSignalOrder4WideSse41(shiftsNeeded, residual, coeffs, output);
                    return true;
                }
                return false;
            }

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static unsafe void RestoreSignalOrder4WideSse41(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
            {
                const int Order = 4;
                if (coeffs.Length < Order) return;
                _ = coeffs[Order - 1];
                ref var o = ref MemoryMarshal.GetReference(output);
                ref var d = ref Unsafe.Add(ref o, Order);
                ref var r = ref MemoryMarshal.GetReference(residual);
                ref var c = ref MemoryMarshal.GetReference(coeffs);
                var vcoeff0 = Sse41.ConvertToVector128Int64(Sse2.LoadScalarVector128((ulong*)Unsafe.AsPointer(ref c)).AsUInt32()).AsInt32();
                var vcoeff1 = Sse41.ConvertToVector128Int64(Sse2.LoadScalarVector128((ulong*)Unsafe.AsPointer(ref Unsafe.Add(ref c, 2))).AsUInt32()).AsInt32();
                var vprev0 = Sse2.Shuffle(Sse2.LoadScalarVector128((ulong*)Unsafe.AsPointer(ref Unsafe.Add(ref o, 2))).AsInt32(), 0b11_00_11_01);
                var vprev1 = Sse2.Shuffle(Sse2.LoadScalarVector128((ulong*)Unsafe.AsPointer(ref Unsafe.Add(ref o, 0))).AsInt32(), 0b11_00_11_01);
                var vshift = Vector128.CreateScalar((long)shiftsNeeded);
                nint dataLength = output.Length - Order;
                for (nint i = 0; i < dataLength;)
                {
                    var res = Vector128.CreateScalar(Unsafe.Add(ref r, i));
                    var z0 = Sse41.Multiply(vcoeff0, vprev0);
                    var z1 = Sse41.Multiply(vcoeff1, vprev1);
                    var y = Sse2.Add(z0, z1);
                    y = Sse2.Add(y, Sse2.ShiftRightLogical128BitLane(y, 8));
                    y = Sse2.ShiftRightLogical(y, vshift);
                    var yy = Sse2.Add(y.AsInt32(), res);
#if NET5_0_OR_GREATER
                    Sse2.StoreScalar((int*)Unsafe.AsPointer(ref Unsafe.Add(ref d, i)), yy);
#else
                    Unsafe.Add(ref d, i) = yy.GetElement(0);
#endif
                    i++;
                    yy = Sse2.ShiftLeftLogical128BitLane(yy, 8);
                    vprev1 = Ssse3.AlignRight(vprev1, vprev0, 8);
                    vprev0 = Ssse3.AlignRight(vprev0, yy, 8);
                }
            }

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static unsafe void RestoreSignalOrder4WideAvx2(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
            {
                const int Order = 4;
                if (coeffs.Length < Order) return;
                _ = coeffs[Order - 1];
                ref var o = ref MemoryMarshal.GetReference(output);
                ref var d = ref Unsafe.Add(ref o, Order);
                ref var r = ref MemoryMarshal.GetReference(residual);
                ref var c = ref MemoryMarshal.GetReference(coeffs);
                var vcoeff0 = Avx2.ConvertToVector256Int64(Sse2.LoadVector128((int*)Unsafe.AsPointer(ref c)).AsUInt32()).AsInt32();
                var vprev0 = Avx2.ConvertToVector256Int64(Sse2.Shuffle(Sse2.LoadVector128((int*)Unsafe.AsPointer(ref Unsafe.Add(ref o, 0))).AsInt32(), 0b00_01_10_11).AsUInt32()).AsInt32();
                var vshift = Vector128.CreateScalarUnsafe((long)shiftsNeeded);
                nint dataLength = output.Length - Order;
                for (nint i = 0; i < dataLength;)
                {
                    var res = Vector128.CreateScalar(Unsafe.Add(ref r, i));
                    var z0 = Avx2.Multiply(vcoeff0, vprev0);
                    var y = Sse2.Add(z0.GetLower(), z0.GetUpper());
                    y = Sse2.Add(y, Sse2.ShiftRightLogical128BitLane(y, 8));
                    y = Sse2.ShiftRightLogical(y, vshift);
                    var yy = Sse2.Add(y.AsInt32(), res);
#if NET5_0_OR_GREATER
                    Sse2.StoreScalar((int*)Unsafe.AsPointer(ref Unsafe.Add(ref d, i)), yy);
#else
                    Unsafe.Add(ref d, i) = yy.GetElement(0);
#endif
                    i++;
                    var yu = yy.ToVector256();
                    yu = Avx2.Permute4x64(yu.AsUInt64(), 0b00_00_00_00).AsInt32();
                    vprev0 = Avx2.Permute4x64(vprev0.AsInt64(), 0b10_01_00_11).AsInt32();
                    vprev0 = Avx2.Blend(vprev0, yu, 0b0000_0001);
                }
            }

            #endregion Order4
        }
    }
}

#endif
