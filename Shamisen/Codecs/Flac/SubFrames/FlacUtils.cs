using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Numerics;

#if NET5_0_OR_GREATER

using System.Runtime.Intrinsics.Arm;

#endif
#if NETCOREAPP3_1_OR_GREATER

using System.Runtime.Intrinsics.X86;
using System.Runtime.Intrinsics;

#endif

using Shamisen.Codecs.Flac.Parsing;

namespace Shamisen.Codecs.Flac.SubFrames
{
    /// <summary>
    /// Contains some utility functions about manipulating FLAC audio samples.
    /// </summary>
    public static class FlacUtils
    {
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static int MultiplyNoFlagsIfPossible(int a, int b)
        {
#if NETCOREAPP3_1_OR_GREATER
            if (Bmi2.IsSupported) return (int)Bmi2.MultiplyNoFlags((uint)a, (uint)b);
#endif
            return a * b;
        }

        /// <summary>
        /// Reads the rice encoded residual.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="bitReader">The bit reader.</param>
        /// <param name="predictorOrder">The predictor order.</param>
        /// <param name="partitionOrder">The partition order.</param>
        /// <param name="blockSize">Size of the block.</param>
        /// <param name="isRice2">if set to <c>true</c> [is rice2].</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool ReadRiceEncodedResidual(Span<int> buffer, FlacBitReader bitReader, int predictorOrder, int partitionOrder, int blockSize, bool isRice2)
        {
            //Modified for C# use.

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

            var partitions = 1u << partitionOrder;
            var partitionSamples = blockSize >> partitionOrder;
            byte encodingParameterLength = (byte)(isRice2 ? 5 : 4);
            var parameterEscape = isRice2 ? 0b11111 : 0b1111;
            Debug.Assert(partitionOrder > 0 ? partitionSamples >= predictorOrder : blockSize >= predictorOrder);
            var sample = 0;
            var bH = buffer;
            for (int partition = 0; partition < partitions; partition++)
            {
                var t = bitReader.ReadBitsUInt32(encodingParameterLength);
                if (t is null) return false;
                var riceParameter = (int)t.Value;
                if (riceParameter < parameterEscape)
                {
                    var u = partition == 0 ? partitionSamples - predictorOrder : partitionSamples;
                    if (!bitReader.ReadRiceCodes(bH.SliceWhile(u), riceParameter)) return false;
                    bH = bH.Slice(u);
                }
                else
                {
                    t = bitReader.ReadBitsUInt32(5);
                    if (t is null) return false;
                    byte bits = (byte)t.Value;
                    var u = partition == 0 ? partitionSamples - predictorOrder : partitionSamples;
                    for (int i = 0; i < u; i++)
                    {
                        t = bitReader.ReadBitsUInt32(bits);
                        if (t is null) return false;
                        bH[i] = (int)t.Value;
                    }
                    bH = bH.Slice(u);
                }
            }
            return true;
        }

        /// <summary>
        /// Shifts the values in specified <paramref name="span"/> left with specified <paramref name="shift"/>.
        /// </summary>
        /// <param name="span">The span.</param>
        /// <param name="shift">The shift.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static void ShiftLeft(Span<int> span, int shift)
        {
#if NET5_0_OR_GREATER
            if (ShiftLeftArm(span, shift)) return;
#endif
#if NETCOREAPP3_1_OR_GREATER
            if (ShiftLeftX86(span, shift)) return;
#endif
            //fallback
            if (ShiftLeftStandard(shift, span)) return;
            ShiftLeftSimple(shift, span);
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static bool ShiftLeftStandard(int shift, Span<int> span)
        {
            unsafe
            {
                ref var head = ref MemoryMarshal.GetReference(span);
                nint length = span.Length;
                nint vlen = length - length % 4;
                nint i;
                for (i = 0; i < vlen; i += 4)
                {
                    var v0 = Unsafe.Add(ref head, i + 0);
                    var v1 = Unsafe.Add(ref head, i + 1);
                    var v2 = Unsafe.Add(ref head, i + 2);
                    var v3 = Unsafe.Add(ref head, i + 3);
                    v0 <<= shift;
                    v1 <<= shift;
                    v2 <<= shift;
                    v3 <<= shift;
                    Unsafe.Add(ref head, i + 0) = v0;
                    Unsafe.Add(ref head, i + 1) = v1;
                    Unsafe.Add(ref head, i + 2) = v2;
                    Unsafe.Add(ref head, i + 3) = v3;
                }
                for (; i < length; i++)
                {
                    var value = Unsafe.Add(ref head, i);
                    value <<= shift;
                    Unsafe.Add(ref head, i) = value;
                }
            }
            return true;
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ShiftLeftSimple(int shift, Span<int> span)
        {
            for (int i = 0; i < span.Length; i++)
            {
                span[i] = span[i] << shift;
            }
        }

#if NET5_0_OR_GREATER

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static bool ShiftLeftArm(Span<int> span, int shift)
        {
            if (!AdvSimd.IsSupported) return false;
            unsafe
            {
                ref var head = ref MemoryMarshal.GetReference(span);
                nint length = span.Length;
                nint vlen = length - length % Vector128<int>.Count;
                nint avlen = (vlen - vlen % (8 * Vector128<int>.Count)) * sizeof(int);
                var v0 = Vector128.Create(shift);
                ref var vhead = ref Unsafe.As<int, Vector128<int>>(ref head);
                nint i;
                const int Size = sizeof(int);
                for (i = 0; i < avlen; i += 8 * Vector128<int>.Count * sizeof(int))
                {
                    //Changing ways to offset to suppress RyuJIT allocating more registers for storing offset addresses.
                    var v1 = Unsafe.AddByteOffset(ref vhead, i + 16 * 0);
                    var v2 = Unsafe.AddByteOffset(ref vhead, i + 16 * 1);
                    var v3 = Unsafe.AddByteOffset(ref vhead, i + 16 * 2);
                    var v4 = Unsafe.AddByteOffset(ref vhead, i + 16 * 3);

                    v1 = AdvSimd.ShiftLogical(v1, v0);
                    v2 = AdvSimd.ShiftLogical(v2, v0);
                    v3 = AdvSimd.ShiftLogical(v3, v0);
                    v4 = AdvSimd.ShiftLogical(v4, v0);

                    Unsafe.AddByteOffset(ref vhead, i) = v1;
                    Unsafe.Add(ref Unsafe.AddByteOffset(ref vhead, i), 1) = v2;
                    Unsafe.Add(ref Unsafe.AddByteOffset(ref vhead, i), 2) = v3;
                    Unsafe.Add(ref Unsafe.AddByteOffset(ref vhead, i), 3) = v4;

                    v1 = Unsafe.AddByteOffset(ref vhead, i + 16 * 4);
                    v2 = Unsafe.AddByteOffset(ref vhead, i + 16 * 5);
                    v3 = Unsafe.AddByteOffset(ref vhead, i + 16 * 6);
                    v4 = Unsafe.AddByteOffset(ref vhead, i + 16 * 7);

                    v1 = AdvSimd.ShiftLogical(v1, v0);
                    v2 = AdvSimd.ShiftLogical(v2, v0);
                    v3 = AdvSimd.ShiftLogical(v3, v0);
                    v4 = AdvSimd.ShiftLogical(v4, v0);

                    Unsafe.Add(ref Unsafe.AddByteOffset(ref vhead, i), 4) = v1;
                    Unsafe.Add(ref Unsafe.AddByteOffset(ref vhead, i), 5) = v2;
                    Unsafe.Add(ref Unsafe.AddByteOffset(ref vhead, i), 6) = v3;
                    Unsafe.Add(ref Unsafe.AddByteOffset(ref vhead, i), 7) = v4;
                }
                for (; i < vlen; i += Vector128<int>.Count)
                {
                    var v1 = Unsafe.AddByteOffset(ref vhead, Size * i);
                    v1 = AdvSimd.ShiftLogical(v1, v0);
                    Unsafe.AddByteOffset(ref vhead, Size * i) = v1;
                }

                for (; i < length; i++)
                {
                    var w15 = Unsafe.Add(ref head, i);
                    w15 <<= shift;
                    Unsafe.Add(ref head, i) = w15;
                }
            }
            return true;
        }

#endif
#if NETCOREAPP3_1_OR_GREATER

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static bool ShiftLeftX86(Span<int> span, int shift)
        {
            if (Avx2.IsSupported)
            {
                ShiftLeftAvx2(span, shift);
                return true;
            }
            if (Sse2.IsSupported)
            {
                ShiftLeftSse2(span, shift);
                return true;
            }
            return false;
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ShiftLeftAvx2(Span<int> span, int shift)
        {
            unsafe
            {
                ref var head = ref MemoryMarshal.GetReference(span);
                nint length = span.Length;
                nint vlen = length - length % Vector256<int>.Count;
                nint avlen = (vlen - vlen % (8 * Vector256<int>.Count)) * sizeof(int);
                var ymm0 = Vector256.Create((uint)shift);
                ref var vhead = ref Unsafe.As<int, Vector256<int>>(ref head);
                nint i;
                const int Size = sizeof(int);
                for (i = 0; i < avlen; i += 8 * Vector256<int>.Count * sizeof(int))
                {
                    //Changing ways to offset to suppress RyuJIT allocating more registers for storing offset addresses.
                    var ymm1 = Unsafe.AddByteOffset(ref vhead, i + 32 * 0);
                    var ymm2 = Unsafe.AddByteOffset(ref vhead, i + 32 * 1);
                    var ymm3 = Unsafe.AddByteOffset(ref vhead, i + 32 * 2);
                    var ymm4 = Unsafe.AddByteOffset(ref vhead, i + 32 * 3);

                    ymm1 = Avx2.ShiftLeftLogicalVariable(ymm1, ymm0);
                    ymm2 = Avx2.ShiftLeftLogicalVariable(ymm2, ymm0);
                    ymm3 = Avx2.ShiftLeftLogicalVariable(ymm3, ymm0);
                    ymm4 = Avx2.ShiftLeftLogicalVariable(ymm4, ymm0);

                    Unsafe.AddByteOffset(ref vhead, i) = ymm1;
                    Unsafe.Add(ref Unsafe.AddByteOffset(ref vhead, i), 1) = ymm2;
                    Unsafe.Add(ref Unsafe.AddByteOffset(ref vhead, i), 2) = ymm3;
                    Unsafe.Add(ref Unsafe.AddByteOffset(ref vhead, i), 3) = ymm4;

                    ymm1 = Unsafe.AddByteOffset(ref vhead, i + 32 * 4);
                    ymm2 = Unsafe.AddByteOffset(ref vhead, i + 32 * 5);
                    ymm3 = Unsafe.AddByteOffset(ref vhead, i + 32 * 6);
                    ymm4 = Unsafe.AddByteOffset(ref vhead, i + 32 * 7);

                    ymm1 = Avx2.ShiftLeftLogicalVariable(ymm1, ymm0);
                    ymm2 = Avx2.ShiftLeftLogicalVariable(ymm2, ymm0);
                    ymm3 = Avx2.ShiftLeftLogicalVariable(ymm3, ymm0);
                    ymm4 = Avx2.ShiftLeftLogicalVariable(ymm4, ymm0);

                    Unsafe.Add(ref Unsafe.AddByteOffset(ref vhead, i), 4) = ymm1;
                    Unsafe.Add(ref Unsafe.AddByteOffset(ref vhead, i), 5) = ymm2;
                    Unsafe.Add(ref Unsafe.AddByteOffset(ref vhead, i), 6) = ymm3;
                    Unsafe.Add(ref Unsafe.AddByteOffset(ref vhead, i), 7) = ymm4;
                }
                for (; i < vlen; i += Vector256<int>.Count)
                {
                    var ymm1 = Unsafe.AddByteOffset(ref vhead, Size * i);
                    ymm1 = Avx2.ShiftLeftLogicalVariable(ymm1, ymm0);
                    Unsafe.AddByteOffset(ref vhead, Size * i) = ymm1;
                }

                for (; i < length; i++)
                {
                    var r15d = Unsafe.Add(ref head, i);
                    r15d <<= shift;
                    Unsafe.Add(ref head, i) = r15d;
                }
            }
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ShiftLeftSse2(Span<int> span, int shift)
        {
            unsafe
            {
                ref var head = ref MemoryMarshal.GetReference(span);
                nint length = span.Length;
                nint vlen = length - length % Vector128<int>.Count;
                nint avlen = (vlen - vlen % (8 * Vector128<int>.Count)) * sizeof(int);
                var xmm0 = Vector128.Create(shift);
                ref var vhead = ref Unsafe.As<int, Vector128<int>>(ref head);
                nint i;
                const int Size = sizeof(int);
                for (i = 0; i < avlen; i += 8 * Vector128<int>.Count * sizeof(int))
                {
                    //Changing ways to offset to suppress RyuJIT allocating more registers for storing offset addresses.
                    var xmm1 = Unsafe.AddByteOffset(ref vhead, i + 16 * 0);
                    var xmm2 = Unsafe.AddByteOffset(ref vhead, i + 16 * 1);
                    var xmm3 = Unsafe.AddByteOffset(ref vhead, i + 16 * 2);
                    var xmm4 = Unsafe.AddByteOffset(ref vhead, i + 16 * 3);

                    xmm1 = Sse2.ShiftLeftLogical(xmm1, xmm0);
                    xmm2 = Sse2.ShiftLeftLogical(xmm2, xmm0);
                    xmm3 = Sse2.ShiftLeftLogical(xmm3, xmm0);
                    xmm4 = Sse2.ShiftLeftLogical(xmm4, xmm0);

                    Unsafe.AddByteOffset(ref vhead, i) = xmm1;
                    Unsafe.Add(ref Unsafe.AddByteOffset(ref vhead, i), 1) = xmm2;
                    Unsafe.Add(ref Unsafe.AddByteOffset(ref vhead, i), 2) = xmm3;
                    Unsafe.Add(ref Unsafe.AddByteOffset(ref vhead, i), 3) = xmm4;

                    xmm1 = Unsafe.AddByteOffset(ref vhead, i + 16 * 4);
                    xmm2 = Unsafe.AddByteOffset(ref vhead, i + 16 * 5);
                    xmm3 = Unsafe.AddByteOffset(ref vhead, i + 16 * 6);
                    xmm4 = Unsafe.AddByteOffset(ref vhead, i + 16 * 7);

                    xmm1 = Sse2.ShiftLeftLogical(xmm1, xmm0);
                    xmm2 = Sse2.ShiftLeftLogical(xmm2, xmm0);
                    xmm3 = Sse2.ShiftLeftLogical(xmm3, xmm0);
                    xmm4 = Sse2.ShiftLeftLogical(xmm4, xmm0);

                    Unsafe.Add(ref Unsafe.AddByteOffset(ref vhead, i), 4) = xmm1;
                    Unsafe.Add(ref Unsafe.AddByteOffset(ref vhead, i), 5) = xmm2;
                    Unsafe.Add(ref Unsafe.AddByteOffset(ref vhead, i), 6) = xmm3;
                    Unsafe.Add(ref Unsafe.AddByteOffset(ref vhead, i), 7) = xmm4;
                }
                for (; i < vlen; i += Vector128<int>.Count)
                {
                    var xmm1 = Unsafe.AddByteOffset(ref vhead, Size * i);
                    xmm1 = Sse2.ShiftLeftLogical(xmm1, xmm0);
                    Unsafe.AddByteOffset(ref vhead, Size * i) = xmm1;
                }

                for (; i < length; i++)
                {
                    var r15d = Unsafe.Add(ref head, i);
                    r15d <<= shift;
                    Unsafe.Add(ref head, i) = r15d;
                }
            }
        }

#endif
    }
}
