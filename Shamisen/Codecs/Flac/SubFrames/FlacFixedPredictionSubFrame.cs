using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Shamisen.Codecs.Flac.Parsing;

namespace Shamisen.Codecs.Flac.SubFrames
{
    public sealed class FlacFixedPredictionSubFrame : IFlacSubFrame
    {
        private int order;
        private int partition;
        private int[] data;

        public byte SubFrameType { get; }

        public int WastedBits { get; }

        /// <summary>
        ///
        /// </summary>
        /// <param name="bitReader"></param>
        /// <param name="blockSize"></param>
        /// <param name="wastedBits"></param>
        /// <param name="bitsPerSample"></param>
        /// <param name="subFrameType"></param>
        public FlacFixedPredictionSubFrame(FlacBitReader bitReader, int blockSize, int wastedBits, byte bitsPerSample, byte subFrameType)
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

            WastedBits = wastedBits;
            order = subFrameType & 0x7;
            var residual = new int[blockSize - order];
            Span<int> warmup = stackalloc int[order];
            for (int i = 0; i < warmup.Length; i++)
            {
                warmup[i] = (int?)bitReader.ReadBitsUInt64(bitsPerSample) ?? throw new FlacException("Invalid FLAC Stream!");
            }
            order = subFrameType & 0x7;
            //Read residual
            ReadResidualPart(bitReader, blockSize, order, out partition, residual);
            //Restore signal
            data = new int[blockSize];
            warmup.CopyTo(data.AsSpan());
            RestoreSignal(residual, order, data);
        }

        internal static void ReadResidualPart(FlacBitReader bitReader, int blockSize, int order, out int partition, int[] residual)
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

            var (hasValue, result) = bitReader.ReadBitsUInt64(2);
            result &= 0x3;
            if (!hasValue) throw new FlacException("Invalid FLAC Stream!");

            switch ((FlacEntropyCodingMethod)result)
            {
                case FlacEntropyCodingMethod.PartitionedRice:
                case FlacEntropyCodingMethod.PartitionedRice2:
                    //Read partition order
                    var g = bitReader.ReadBitsUInt64(4) ?? throw new FlacException("Invalid FLAC Stream!");
                    if (blockSize >> (int)g < order) throw new FlacException("Invalid FLAC Stream!");
                    partition = (int)g;
                    break;
                default:
                    throw new FlacException("Invalid FLAC Stream!");
            }
            switch ((FlacEntropyCodingMethod)result)
            {
                case FlacEntropyCodingMethod.PartitionedRice:
                    if (!ResidualUtils.ReadRiceEncodedResidual(residual.AsSpan(), bitReader, order, partition, blockSize, false))
                        throw new FlacException("Invalid FLAC Stream!");
                    break;
                case FlacEntropyCodingMethod.PartitionedRice2:
                    if (!ResidualUtils.ReadRiceEncodedResidual(residual.AsSpan(), bitReader, order, partition, blockSize, true))
                        throw new FlacException("Invalid FLAC Stream!");
                    break;
                default:
                    throw new FlacException("Invalid FLAC Stream!");
            }
        }

        private static unsafe void RestoreSignal(ReadOnlySpan<int> residual, int order, Span<int> output)
        {
            //Refereed https://github.com/xiph/flac/blob/master/src/libFLAC/fixed.c and written for C# use.

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

#pragma warning disable S907 // "goto" statement should not be used
            switch (order)
            {
                case 0:
                    residual.CopyTo(output);
                    return;
                case 1:
                    {
                        RestoreOneSimple(residual, output);
                    }
                    return;
                case 2:
                    {
                        RestoreTwoSimple(residual, output);
                    }
                    return;
                case 3:
                    {
                        RestoreThreeSimple(residual, output);
                    }
                    return;
                case 4:
                    {
                        RestoreFourSimple(residual, output);
                    }
                    return;
                default:
                    throw new FlacException("Invalid FLAC Stream!");
            }
        }

        private static unsafe void RestoreOneSimple(ReadOnlySpan<int> residual, Span<int> output)
        {
            switch (0)
            {
                case 0:
                    if (output.Length < 1) goto default;
                    var data = output.Slice(1);
                    var prev = output[0];
                    if (residual.Length < data.Length) goto default;
                    ref var R = ref MemoryMarshal.GetReference(residual);
                    ref var D = ref MemoryMarshal.GetReference(data);
                    var F = ((IntPtr)data.Length).ToPointer();
                    for (var i = IntPtr.Zero; i.ToPointer() < F; i += 1)
                    {
                        Unsafe.Add(ref D, i) = prev = Unsafe.Add(ref R, i) + prev;
                    }
                    break;
                default:
                    throw new FlacException("Invalid FLAC Stream!");
            }
        }

        private static unsafe void RestoreTwoSimple(ReadOnlySpan<int> residual, Span<int> output)
        {
            switch (0)
            {
                case 0:
                    if (output.Length < 2) goto default;
                    var data = output.Slice(2);
                    (var prev0, var prev1) = Unsafe.As<int, (int, int)>(ref output[0]);
                    if (residual.Length < data.Length) goto default;
                    ref var R = ref MemoryMarshal.GetReference(residual);
                    ref var D = ref MemoryMarshal.GetReference(data);
                    var F = ((IntPtr)data.Length).ToPointer();
                    for (var i = IntPtr.Zero; i.ToPointer() < F; i += 1)
                    {
                        var p = prev0;
                        prev0 = prev1;
                        Unsafe.Add(ref D, i) = prev1 = Unsafe.Add(ref R, i) + 2 * prev1 - p;
                    }
                    break;
                default:
                    throw new FlacException("Invalid FLAC Stream!");
            }
        }

        private static unsafe void RestoreThreeSimple(ReadOnlySpan<int> residual, Span<int> output)
        {
            switch (0)
            {
                case 0:
                    if (output.Length < 3) goto default;
                    var data = output.Slice(3);
                    (var prev0, var prev1, var prev2) = Unsafe.As<int, (int, int, int)>(ref output[0]);
                    if (residual.Length < data.Length) goto default;
                    ref var R = ref MemoryMarshal.GetReference(residual);
                    ref var D = ref MemoryMarshal.GetReference(data);
                    var F = ((IntPtr)data.Length).ToPointer();
                    for (var i = IntPtr.Zero; i.ToPointer() < F; i += 1)
                    {
                        (var p0, var p1, var p2) = (prev0, prev1, prev2);
                        var n = Unsafe.Add(ref R, i) + 3 * p2 - 3 * p1 + p0;
                        (prev0, prev1, prev2) = (p1, p2, n);
                        Unsafe.Add(ref D, i) = n;
                    }
                    break;
                default:
                    throw new FlacException("Invalid FLAC Stream!");
            }
        }

        private static unsafe void RestoreFourSimple(ReadOnlySpan<int> residual, Span<int> output)
        {
            switch (0)
            {
                case 0:
                    if (output.Length < 4) goto default;
                    var data = output.Slice(4);
                    (var prev0, var prev1, var prev2, var prev3) = Unsafe.As<int, (int, int, int, int)>(ref output[0]);
                    if (residual.Length < data.Length) goto default;
                    ref var R = ref MemoryMarshal.GetReference(residual);
                    ref var D = ref MemoryMarshal.GetReference(data);
                    var F = ((IntPtr)data.Length).ToPointer();
                    for (var i = IntPtr.Zero; i.ToPointer() < F; i += 1)
                    {
                        (var p0, var p1, var p2, var p3) = (prev0, prev1, prev2, prev3);
                        var n = Unsafe.Add(ref R, i) + 4 * p3 - 6 * p2 + 4 * p1 - p0;
                        (prev0, prev1, prev2, prev3) = (p1, p2, p3, n);
                        Unsafe.Add(ref D, i) = n;
                    }
                    break;
                default:
                    throw new FlacException("Invalid FLAC Stream!");
            }
        }

#pragma warning restore S907 // "goto" statement should not be used

        public ReadResult Read(Span<int> buffer)
        {
            if (data.Length > buffer.Length) return ReadResult.EndOfStream;
            data.AsSpan().CopyTo(buffer);
            return data.Length;
        }
    }
}
