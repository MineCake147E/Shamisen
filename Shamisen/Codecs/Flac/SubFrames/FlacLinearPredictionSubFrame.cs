using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Shamisen.Codecs.Flac.Parsing;

namespace Shamisen.Codecs.Flac.SubFrames
{
    public sealed partial class FlacLinearPredictionSubFrame : IFlacSubFrame
    {
        public int WastedBits { get; }

        public byte SubFrameType { get; }

        private int order;
        private int partition;
        private int[] data;

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
            order = subFrameType & 0x1f + 1;
            var residual = new int[blockSize - order];
            Span<int> warmup = stackalloc int[order];
            for (int i = 0; i < warmup.Length; i++)
            {
                warmup[i] = (int?)bitReader.ReadBitsUInt64(bitsPerSample) ?? throw new FlacException("Invalid FLAC Stream!");
            }
            //Read Quantized linear predictor coefficients' precision in bits
            byte quantizedPrecision = (byte?)bitReader.ReadBitsUInt64(4) ?? throw new FlacException("Invalid FLAC Stream!");
            if (quantizedPrecision == 0b1111) throw new FlacException("Invalid FLAC Stream!");
            quantizedPrecision++;
            var shiftsNeeded = (int?)bitReader.ReadBitsUInt64(5) ?? throw new FlacException("Invalid FLAC Stream!");
            Span<int> coeffs = stackalloc int[order];
            for (int i = 0; i < coeffs.Length; i++)
            {
                coeffs[i] = (int?)bitReader.ReadBitsUInt64(quantizedPrecision) ?? throw new FlacException("Invalid FLAC Stream!");
            }
            //Read residual
            FlacFixedPredictionSubFrame.ReadResidualPart(bitReader, blockSize, order, out partition, residual);
            //Restore signal
            data = new int[blockSize];
            warmup.CopyTo(data.AsSpan());
            RestoreSignal(shiftsNeeded, residual, coeffs, data);
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

        internal static unsafe void RestoreSignal(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            //
#pragma warning disable IDE0022
            RestoreSignalStandard(shiftsNeeded, residual, coeffs, output);
#pragma warning restore IDE0022
        }

        /// <summary>
        /// Implemented for comparison against libFLAC.
        /// </summary>
        /// <param name="shiftsNeeded"></param>
        /// <param name="residual"></param>
        /// <param name="coeffs"></param>
        /// <param name="output"></param>
        internal static unsafe void RestoreSignalLibFlac(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            //Modified for C# port.

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

            var order = coeffs.Length;
            int sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + order;
            int i;
            int dataLength = output.Length - order;
            var c = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(coeffs));
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));

            #region Large Code Segments

            if (order <= 12)
            {
                if (order > 8)
                {
                    if (order > 10)
                    {
                        if (order == 12)
                        {
                            for (i = 0; i < dataLength; i += 1)
                            {
                                sum = 0;
                                sum += c[11] * d[i - 12];
                                sum += c[10] * d[i - 11];
                                sum += c[9] * d[i - 10];
                                sum += c[8] * d[i - 9];
                                sum += c[7] * d[i - 8];
                                sum += c[6] * d[i - 7];
                                sum += c[5] * d[i - 6];
                                sum += c[4] * d[i - 5];
                                sum += c[3] * d[i - 4];
                                sum += c[2] * d[i - 3];
                                sum += c[1] * d[i - 2];
                                sum += c[0] * d[i - 1];
                                d[i] = r[i] + (sum >> shiftsNeeded);
                            }
                        }
                        else
                        { /* order == 11 */
                            for (i = 0; i < dataLength; i += 1)
                            {
                                sum = 0;
                                sum += c[10] * d[i - 11];
                                sum += c[9] * d[i - 10];
                                sum += c[8] * d[i - 9];
                                sum += c[7] * d[i - 8];
                                sum += c[6] * d[i - 7];
                                sum += c[5] * d[i - 6];
                                sum += c[4] * d[i - 5];
                                sum += c[3] * d[i - 4];
                                sum += c[2] * d[i - 3];
                                sum += c[1] * d[i - 2];
                                sum += c[0] * d[i - 1];
                                d[i] = r[i] + (sum >> shiftsNeeded);
                            }
                        }
                    }
                    else
                    {
                        if (order == 10)
                        {
                            for (i = 0; i < dataLength; i += 1)
                            {
                                sum = 0;
                                sum += c[9] * d[i - 10];
                                sum += c[8] * d[i - 9];
                                sum += c[7] * d[i - 8];
                                sum += c[6] * d[i - 7];
                                sum += c[5] * d[i - 6];
                                sum += c[4] * d[i - 5];
                                sum += c[3] * d[i - 4];
                                sum += c[2] * d[i - 3];
                                sum += c[1] * d[i - 2];
                                sum += c[0] * d[i - 1];
                                d[i] = r[i] + (sum >> shiftsNeeded);
                            }
                        }
                        else
                        { /* order == 9 */
                            for (i = 0; i < dataLength; i += 1)
                            {
                                sum = 0;
                                sum += c[8] * d[i - 9];
                                sum += c[7] * d[i - 8];
                                sum += c[6] * d[i - 7];
                                sum += c[5] * d[i - 6];
                                sum += c[4] * d[i - 5];
                                sum += c[3] * d[i - 4];
                                sum += c[2] * d[i - 3];
                                sum += c[1] * d[i - 2];
                                sum += c[0] * d[i - 1];
                                d[i] = r[i] + (sum >> shiftsNeeded);
                            }
                        }
                    }
                }
                else if (order > 4)
                {
                    if (order > 6)
                    {
                        if (order == 8)
                        {
                            for (i = 0; i < dataLength; i += 1)
                            {
                                sum = 0;
                                sum += c[7] * d[i - 8];
                                sum += c[6] * d[i - 7];
                                sum += c[5] * d[i - 6];
                                sum += c[4] * d[i - 5];
                                sum += c[3] * d[i - 4];
                                sum += c[2] * d[i - 3];
                                sum += c[1] * d[i - 2];
                                sum += c[0] * d[i - 1];
                                d[i] = r[i] + (sum >> shiftsNeeded);
                            }
                        }
                        else
                        { /* order == 7 */
                            for (i = 0; i < dataLength; i += 1)
                            {
                                sum = 0;
                                sum += c[6] * d[i - 7];
                                sum += c[5] * d[i - 6];
                                sum += c[4] * d[i - 5];
                                sum += c[3] * d[i - 4];
                                sum += c[2] * d[i - 3];
                                sum += c[1] * d[i - 2];
                                sum += c[0] * d[i - 1];
                                d[i] = r[i] + (sum >> shiftsNeeded);
                            }
                        }
                    }
                    else
                    {
                        if (order == 6)
                        {
                            for (i = 0; i < dataLength; i += 1)
                            {
                                sum = 0;
                                sum += c[5] * d[i - 6];
                                sum += c[4] * d[i - 5];
                                sum += c[3] * d[i - 4];
                                sum += c[2] * d[i - 3];
                                sum += c[1] * d[i - 2];
                                sum += c[0] * d[i - 1];
                                d[i] = r[i] + (sum >> shiftsNeeded);
                            }
                        }
                        else
                        { /* order == 5 */
                            for (i = 0; i < dataLength; i += 1)
                            {
                                sum = 0;
                                sum += c[4] * d[i - 5];
                                sum += c[3] * d[i - 4];
                                sum += c[2] * d[i - 3];
                                sum += c[1] * d[i - 2];
                                sum += c[0] * d[i - 1];
                                d[i] = r[i] + (sum >> shiftsNeeded);
                            }
                        }
                    }
                }
                else
                {
                    if (order > 2)
                    {
                        if (order == 4)
                        {
                            for (i = 0; i < dataLength; i += 1)
                            {
                                sum = 0;
                                sum += c[3] * d[i - 4];
                                sum += c[2] * d[i - 3];
                                sum += c[1] * d[i - 2];
                                sum += c[0] * d[i - 1];
                                d[i] = r[i] + (sum >> shiftsNeeded);
                            }
                        }
                        else
                        { /* order == 3 */
                            for (i = 0; i < dataLength; i += 1)
                            {
                                sum = 0;
                                sum += c[2] * d[i - 3];
                                sum += c[1] * d[i - 2];
                                sum += c[0] * d[i - 1];
                                d[i] = r[i] + (sum >> shiftsNeeded);
                            }
                        }
                    }
                    else
                    {
                        if (order == 2)
                        {
                            for (i = 0; i < dataLength; i += 1)
                            {
                                sum = 0;
                                sum += c[1] * d[i - 2];
                                sum += c[0] * d[i - 1];
                                d[i] = r[i] + (sum >> shiftsNeeded);
                            }
                        }
                        else
                        { /* order == 1 */
                            for (i = 0; i < dataLength; i += 1)
                                d[i] = r[i] + ((c[0] * d[i - 1]) >> shiftsNeeded);
                        }
                    }
                }
            }
            else
            { /* order > 12 */
                for (i = 0; i < dataLength; i += 1)
                {
                    sum = 0;
                    switch (order)
                    {
#pragma warning disable S907 // "goto" statement should not be used
                        case 32:
                            sum += c[31] * d[i - 32];
                            goto case 31;
                        case 31:
                            sum += c[30] * d[i - 31];
                            goto case 30;
                        case 30:
                            sum += c[29] * d[i - 30];
                            goto case 29;
                        case 29:
                            sum += c[28] * d[i - 29];
                            goto case 28;
                        case 28:
                            sum += c[27] * d[i - 28];
                            goto case 27;
                        case 27:
                            sum += c[26] * d[i - 27];
                            goto case 26;
                        case 26:
                            sum += c[25] * d[i - 26];
                            goto case 25;
                        case 25:
                            sum += c[24] * d[i - 25];
                            goto case 24;
                        case 24:
                            sum += c[23] * d[i - 24];
                            goto case 23;
                        case 23:
                            sum += c[22] * d[i - 23];
                            goto case 22;
                        case 22:
                            sum += c[21] * d[i - 22];
                            goto case 21;
                        case 21:
                            sum += c[20] * d[i - 21];
                            goto case 20;
                        case 20:
                            sum += c[19] * d[i - 20];
                            goto case 19;
                        case 19:
                            sum += c[18] * d[i - 19];
                            goto case 18;
                        case 18:
                            sum += c[17] * d[i - 18];
                            goto case 17;
                        case 17:
                            sum += c[16] * d[i - 17];
                            goto case 16;
                        case 16:
                            sum += c[15] * d[i - 16];
                            goto case 15;
                        case 15:
                            sum += c[14] * d[i - 15];
                            goto case 14;
                        case 14:
                            sum += c[13] * d[i - 14];
                            goto default;
#pragma warning restore S907 // "goto" statement should not be used
                        default:
                            sum += c[12] * d[i - 13];
                            sum += c[11] * d[i - 12];
                            sum += c[10] * d[i - 11];
                            sum += c[9] * d[i - 10];
                            sum += c[8] * d[i - 9];
                            sum += c[7] * d[i - 8];
                            sum += c[6] * d[i - 7];
                            sum += c[5] * d[i - 6];
                            sum += c[4] * d[i - 5];
                            sum += c[3] * d[i - 4];
                            sum += c[2] * d[i - 3];
                            sum += c[1] * d[i - 2];
                            sum += c[0] * d[i - 1];
                            break;
                    }
                    d[i] = r[i] + (sum >> shiftsNeeded);
                }
            }

            #endregion Large Code Segments
        }

        /// <summary>
        /// Implemented for comparison against obvious way.
        /// </summary>
        /// <param name="shiftsNeeded"></param>
        /// <param name="residual"></param>
        /// <param name="coeffs"></param>
        /// <param name="output"></param>
        internal static unsafe void RestoreSignalObvious(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            Span<int> a = stackalloc int[coeffs.Length];
            output.SliceWhile(a.Length).CopyTo(a);
            output = output.Slice(a.Length);
            if (residual.Length < output.Length) throw new FlacException("Invalid FLAC Stream!");
            for (int i = 0; i < output.Length && i < residual.Length; i++)
            {
                int sum = 0;
                for (int j = 0; j < a.Length && j < coeffs.Length; j++)
                {
                    sum += a[i] * coeffs[i];
                }
                sum >>= shiftsNeeded;
                sum += residual[i];
                a.SliceWhile(a.Length - 1).CopyTo(a.Slice(1));
                output[i] = a[0] = sum;
            }
        }

        #endregion RestoreSignal

        public ReadResult Read(Span<int> buffer)
        {
            if (data.Length > buffer.Length) return ReadResult.EndOfStream;
            data.AsSpan().CopyTo(buffer);
            return data.Length;
        }
    }
}
