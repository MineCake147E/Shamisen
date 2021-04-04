using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

using System.Text;
using System.Threading.Tasks;

#if NET5_0_OR_GREATER

using System.Runtime.Intrinsics.Arm;

#endif
#if NETCOREAPP3_1_OR_GREATER

using System.Runtime.Intrinsics.X86;

#endif

using Shamisen.Codecs.Flac.Parsing;

namespace Shamisen.Codecs.Flac.SubFrames
{
    /// <summary>
    /// The base class for <see cref="FlacFixedPredictionSubFrame"/>.
    /// </summary>
    public static class ResidualUtils
    {
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static int MultiplyNoFlagsIfPossible(int a, int b)
        {
#if NETCOREAPP3_1_OR_GREATER
            if (Bmi2.IsSupported) return (int)Bmi2.MultiplyNoFlags((uint)a, (uint)b);
#endif
            return a * b;
        }

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
            Trace.Assert(partitionOrder > 0 ? partitionSamples >= predictorOrder : blockSize >= predictorOrder);
            Trace.Assert(buffer.Length >= blockSize);
            var sample = 0;
            var bH = buffer;
            for (int partition = 0; partition < partitions; partition++)
            {
                var t = bitReader.ReadBitsUInt64(encodingParameterLength);
                if (t is null) return false;
                var riceParameter = (int)t.Value;
                if (riceParameter < parameterEscape)
                {
                    var u = partition == 0 ? partitionSamples - predictorOrder : partitionSamples;
                    if (!bitReader.ReadRiceCodes(bH, riceParameter)) return false;
                    bH = bH.Slice(u);
                }
                else
                {
                    t = bitReader.ReadBitsUInt64(5);
                    if (t is null) return false;
                    byte bits = (byte)t.Value;
                    var u = partition == 0 ? partitionSamples - predictorOrder : partitionSamples;
                    for (int i = 0; i < u; i++)
                    {
                        t = bitReader.ReadBitsUInt64(bits);
                        if (t is null) return false;
                        bH[i] = (int)t.Value;
                    }
                }
            }
            return true;
        }
    }
}
