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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Shamisen.Codecs.Flac.Parsing;
using Shamisen.Data;

namespace Shamisen.Codecs.Flac.SubFrames
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="IFlacSubFrame" />
    public sealed partial class FlacLinearPredictionSubFrame : IFlacSubFrame
    {
        /// <summary>
        /// Gets the number of wasted LSBs.
        /// </summary>
        public int WastedBits { get; }

        /// <summary>
        /// Gets the type of the sub-frame.
        /// </summary>
        public byte SubFrameType { get; }

        private int order;
        private int partition;
        private PooledArray<int> data;
        private bool disposedValue;

        /// <summary>
        ///
        /// </summary>
        /// <param name="bitReader"></param>
        /// <param name="blockSize"></param>
        /// <param name="wastedBits"></param>
        /// <param name="bitsPerSample"></param>
        /// <param name="subFrameType"></param>
        public FlacLinearPredictionSubFrame(FlacBitReader bitReader, int blockSize, int wastedBits, byte bitsPerSample, byte subFrameType)
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
            order = (subFrameType & 0x1f) + 1;
            using var residual = new PooledArray<int>(blockSize - order);
            Span<int> warmup = stackalloc int[order];
            for (var i = 0; i < warmup.Length; i++)
            {
                warmup[i] = bitReader.ReadBitsInt32(bitsPerSample, out var v2) ? v2 : throw new FlacException("Invalid FLAC Stream!", bitReader);
            }
            //Read Quantized linear predictor coefficients' precision in bits
            var quantizedPrecision = bitReader.ReadBitsUInt32(4, out var v) ? (byte)v : throw new FlacException("Invalid FLAC Stream!", bitReader);
            if (quantizedPrecision == 0b1111) throw new FlacException("Invalid FLAC Stream!", bitReader);
            quantizedPrecision++;
            if (!bitReader.ReadBitsInt32(5, out var shiftsNeeded))
            {
                throw new FlacException("Invalid FLAC Stream!", bitReader);
            }
            Span<int> coeffs = stackalloc int[order];
            for (var i = 0; i < coeffs.Length; i++)
            {
                if (!bitReader.ReadBitsInt32(quantizedPrecision, out var value)) throw new FlacException("Invalid FLAC Stream!", bitReader);
                coeffs[i] = value;
            }
            //Read residual
            var residualSpan = residual.Span;
            FlacFixedPredictionSubFrame.ReadResidualPart(bitReader, blockSize, order, out partition, residualSpan);
            //Restore signal
            data = new(blockSize);
            warmup.CopyTo(data.Span);
            if (bitsPerSample + quantizedPrecision + MathI.LogBase2((uint)order) <= 32)
            {
                RestoreSignal(shiftsNeeded, residualSpan, coeffs, data.Span);
            }
            else
            {
                RestoreSignalWide(shiftsNeeded, residualSpan, coeffs, data.Span);
            }
            if (wastedBits > 0)
                FlacUtils.ShiftLeft(data.Span, wastedBits);
        }

        #region RestoreSignal

        //Refereed https://github.com/xiph/flac/blob/master/src/libFLAC/lpc.c and written for C# use.

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

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignal(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            //
#pragma warning disable IDE0022
            RestoreSignalStandard(shiftsNeeded, residual, coeffs, output);
#pragma warning restore IDE0022
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalWide(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            //
#pragma warning disable IDE0022
            RestoreSignalStandardWide(shiftsNeeded, residual, coeffs, output);
#pragma warning restore IDE0022
        }

        #endregion RestoreSignal

        /// <summary>
        /// Reads the data to the specified <paramref name="buffer" />.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>
        /// The length of the data written.
        /// </returns>
        public ReadResult Read(Span<int> buffer)
        {
            if (data.Length > buffer.Length) return ReadResult.EndOfStream;
            data.Span.CopyTo(buffer);
            return data.Length;
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //
                }
                data?.Dispose();
                disposedValue = true;
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="FlacLinearPredictionSubFrame"/> class.
        /// </summary>
        ~FlacLinearPredictionSubFrame()
        {
            Dispose(disposing: false);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
