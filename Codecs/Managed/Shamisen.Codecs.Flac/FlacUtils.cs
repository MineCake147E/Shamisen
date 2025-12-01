#region License
/*
 * Ported to C#.
 *
 * libFLAC - Free Lossless Audio Codec library
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
#endregion
using System.Diagnostics;
using System.Runtime.CompilerServices;

#if NET5_0_OR_GREATER



#endif
#if NETCOREAPP3_1_OR_GREATER

using System.Runtime.Intrinsics.X86;

#endif

using Shamisen.Codecs.Flac.Parsing;

namespace Shamisen.Codecs.Flac
{
    /// <summary>
    /// Contains some utility functions about manipulating FLAC audio samples.
    /// </summary>
    public static partial class FlacUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
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
            var encodingParameterLength = (byte)(isRice2 ? 5 : 4);
            var parameterEscape = isRice2 ? 0b11111 : 0b1111;
            Debug.Assert(partitionOrder > 0 ? partitionSamples >= predictorOrder : blockSize >= predictorOrder);
            var sample = 0;
            var bH = buffer;
            for (var partition = 0; partition < partitions; partition++)
            {
                if (!bitReader.ReadBitsUInt32(encodingParameterLength, out var t)) return false;
                var riceParameter = (int)t;
                if (riceParameter < parameterEscape)
                {
                    var u = partition == 0 ? partitionSamples - predictorOrder : partitionSamples;
                    if (!bitReader.ReadRiceCodes(bH.SliceWhile(u), riceParameter)) return false;
                    bH = bH.Slice(u);
                }
                else
                {
                    if (!bitReader.ReadBitsUInt32(5, out t)) return false;
                    var bits = (byte)t;
                    var u = partition == 0 ? partitionSamples - predictorOrder : partitionSamples;
                    for (var i = 0; i < u; i++)
                    {
                        if (!bitReader.ReadBitsUInt32(bits, out t)) return false;
                        bH[i] = (int)t;
                    }
                    bH = bH.Slice(u);
                }
            }
            return true;
        }
    }
}
