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
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
#if NETCOREAPP3_1_OR_GREATER
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
#endif

namespace Shamisen.Codecs.Flac.Parsing
{
    /// <summary>
    /// Calculates a CRC-16 for FLAC stream. Polynomial: CRC-16-IBM
    /// </summary>
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public readonly partial struct FlacCrc16 : IEquatable<FlacCrc16>
    {
        private readonly ushort state;

        /// <summary>
        /// Initializes a new instance of the <see cref="FlacCrc16"/> struct.
        /// </summary>
        /// <param name="state">The state.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public FlacCrc16(ushort state)
        {
            this.state = state;
        }

        /// <summary>
        /// Gets the state.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        public ushort State
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => state;
        }

        #region Operator overloads

        /// <summary>
        /// Performs an implicit conversion from <see cref="FlacCrc16"/> to <see cref="ushort"/>.
        /// </summary>
        /// <param name="crc16">The CRC16.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static implicit operator ushort(FlacCrc16 crc16) => crc16.state;

        /// <summary>
        /// Calculates the next value of CRC-16-IBM with <paramref name="right"/>.
        /// </summary>
        /// <param name="left">The current CRC-16-IBM value.</param>
        /// <param name="right">The new byte.</param>
        /// <returns>
        /// The next value of CRC16.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static FlacCrc16 operator *(FlacCrc16 left, byte right) => left.GenerateNext(right);

        /// <summary>
        /// Calculates the next value of CRC-16-IBM with <paramref name="right"/>.
        /// </summary>
        /// <param name="left">The current CRC-16-IBM value.</param>
        /// <param name="right">The new bytes in little endian.</param>
        /// <returns>
        /// The next value of CRC16.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static FlacCrc16 operator *(FlacCrc16 left, ulong right) => GenerateNext(left, right);

        /// <summary>
        /// Calculates the next value of CRC-16-IBM with <paramref name="right"/>.
        /// </summary>
        /// <param name="left">The current CRC-16-IBM value.</param>
        /// <param name="right">The new bytes.</param>
        /// <returns>
        /// The next value of CRC16.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static FlacCrc16 operator *(FlacCrc16 left, ReadOnlySpan<byte> right)
        {
            var value = left;
            var w = MemoryUtils.CastSplit<byte, ulong>(right, out var rem);
            for (var i = 0; i < w.Length; i++)
            {
                value *= w[i];
            }
            for (var i = 0; i < rem.Length; i++)
            {
                value *= rem[i];
            }
            return value;
        }

        /// <summary>
        /// Calculates the next value of CRC-16-IBM with <paramref name="right"/>.
        /// </summary>
        /// <param name="left">The current CRC-16-IBM value.</param>
        /// <param name="right">The new bytes.</param>
        /// <returns>
        /// The next value of CRC16.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static FlacCrc16 operator *(FlacCrc16 left, ReadOnlySpan<ulong> right) => CalculateCrc16(left, right);

        private static FlacCrc16 CalculateCrc16(FlacCrc16 left, ReadOnlySpan<ulong> right)
        {
            unchecked
            {
#if NETCOREAPP3_1_OR_GREATER
                if (Pclmulqdq.IsSupported) return CalculateCrc16Pclmulqdq(left, right);
#endif
                return CalculateCrc16Standard(left, right);
            }
        }

        internal static FlacCrc16 CalculateCrc16Standard(FlacCrc16 left, ReadOnlySpan<ulong> right)
        {
            var value = left;
            for (var i = 0; i < right.Length; i++)
            {
                value *= right[i];
            }
            return value;
        }

        /// <summary>
        /// Calculates the next value of CRC-16-IBM with <paramref name="right"/>.
        /// </summary>
        /// <param name="left">The current CRC-16-IBM value.</param>
        /// <param name="right">The new bytes.</param>
        /// <returns>
        /// The next value of CRC16.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static FlacCrc16 operator *(FlacCrc16 left, ushort right) => left * (byte)(right >> 8) * (byte)right;

        /// <summary>
        /// Calculates the next value of CRC-16-IBM with <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public FlacCrc16 GenerateNext(byte value) => new((ushort)(((byte)state << 8) ^ (GetTable0At(value ^ (byte)(state >> 8)))));

        /// <summary>
        /// Calculates the next value of CRC-16-IBM with <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public FlacCrc16 GenerateNext(ulong value) => GenerateNext(this, value);

        /// <summary>
        /// Calculates the next value of CRC-16-IBM with <paramref name="value"/>.
        /// </summary>
        /// <param name="left">The CRC16 to calculate the next value.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static FlacCrc16 GenerateNext(FlacCrc16 left, ulong value)
        {
            //The libFLAC code is used as a reference.

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

            var t = left.state;
            t ^= (ushort)(value >> 48);
            t = (ushort)(
                GetTable7At((byte)(t >> 8)) ^ GetTable6At((byte)t) ^
                GetTable5At((byte)(value >> 40)) ^ GetTable4At((byte)(value >> 32)) ^
                GetTable3At((byte)(value >> 24)) ^ GetTable2At((byte)(value >> 16)) ^
                GetTable1At((byte)(value >> 8)) ^ GetTable0At((byte)value));
            return new(t);
        }

        #endregion Operator overloads

        #region Fast CRC Calculation using Pclmulqdq
#if NETCOREAPP3_1_OR_GREATER
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        internal static FlacCrc16 CalculateCrc16Pclmulqdq(FlacCrc16 left, ReadOnlySpan<ulong> right)
        {
            #region License Notice
            //  Translated from x86 Assembly to C#, and modified for our use.

            /*******************************************************************************
            ;  Copyright(c) 2011-2015 Intel Corporation All rights reserved.
            ;
            ;  Redistribution and use in source and binary forms, with or without
            ;  modification, are permitted provided that the following conditions
            ;  are met:
            ;    * Redistributions of source code must retain the above copyright
            ;      notice, this list of conditions and the following disclaimer.
            ;    * Redistributions in binary form must reproduce the above copyright
            ;      notice, this list of conditions and the following disclaimer in
            ;      the documentation and/or other materials provided with the
            ;      distribution.
            ;    * Neither the name of Intel Corporation nor the names of its
            ;      contributors may be used to endorse or promote products derived
            ;      from this software without specific prior written permission.
            ;
            ;  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
            ;  "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
            ;  LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
            ;  A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
            ;  OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
            ;  SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
            ;  LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
            ;  DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
            ;  THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
            ;  (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
            ;  OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
            *******************************************************************************/
            #endregion
            //We don't have to completely get rid of table-based one.

            ref var rsi = ref Unsafe.As<ulong, byte>(ref MemoryMarshal.GetReference(right));
            nint i = 0, length = right.Length * sizeof(ulong);
            if (length < 256) return CalculateCrc16Standard(left, right);
            Vector128<ulong> xmm0 = Vector128.CreateScalar((uint)left.State << 16).AsUInt64(), xmm1, xmm2, xmm3, xmm4, xmm5, xmm6, xmm7;
            xmm0 = Sse2.ShiftLeftLogical128BitLane(xmm0, 12);
            #region Constants
            const ulong Rk1 = 0x8663_0000_0000_0000;
            const ulong Rk2 = 0x8617_0000_0000_0000;
            const ulong Rk3 = 0x8665_0000_0000_0000;
            const ulong Rk4 = 0x8077_0000_0000_0000;
            const ulong Rk5 = 0x8663_0000_0000_0000;
            const ulong Rk6 = 0x807b_0000_0000_0000;
            const ulong Rk7 = 0x0000_0001_fffb_ffe7;
            const ulong Rk8 = 0x0000_0001_8005_0000;
            const ulong Rk9 = 0x6a7a_0000_0000_0000;
            const ulong Rk10 = 0x5ccb_0000_0000_0000;
            const ulong Rk11 = 0x006b_0000_0000_0000;
            const ulong Rk12 = 0xedb3_0000_0000_0000;
            const ulong Rk13 = 0xf997_0000_0000_0000;
            const ulong Rk14 = 0x8c47_0000_0000_0000;
            const ulong Rk15 = 0xbffa_0000_0000_0000;
            const ulong Rk16 = 0x861b_0000_0000_0000;
            const ulong Rk17 = 0xeac3_0000_0000_0000;
            const ulong Rk18 = 0xed6b_0000_0000_0000;
            const ulong Rk19 = 0xf557_0000_0000_0000;
            const ulong Rk20 = 0x806f_0000_0000_0000;
            #endregion
            //Load the initial 128 bytes
            //We don't have to reverse endianness because we had done it before.
            xmm0 = Sse2.Xor(xmm0, Sse2.Shuffle(Unsafe.As<byte, Vector128<uint>>(ref Unsafe.Add(ref rsi, 0 * 16)), 0b01_00_11_10).AsUInt64());
            xmm1 = Sse2.Shuffle(Unsafe.As<byte, Vector128<uint>>(ref Unsafe.Add(ref rsi, 1 * 16)), 0b01_00_11_10).AsUInt64();
            xmm2 = Sse2.Shuffle(Unsafe.As<byte, Vector128<uint>>(ref Unsafe.Add(ref rsi, 2 * 16)), 0b01_00_11_10).AsUInt64();
            xmm3 = Sse2.Shuffle(Unsafe.As<byte, Vector128<uint>>(ref Unsafe.Add(ref rsi, 3 * 16)), 0b01_00_11_10).AsUInt64();
            xmm4 = Sse2.Shuffle(Unsafe.As<byte, Vector128<uint>>(ref Unsafe.Add(ref rsi, 4 * 16)), 0b01_00_11_10).AsUInt64();
            xmm5 = Sse2.Shuffle(Unsafe.As<byte, Vector128<uint>>(ref Unsafe.Add(ref rsi, 5 * 16)), 0b01_00_11_10).AsUInt64();
            xmm6 = Sse2.Shuffle(Unsafe.As<byte, Vector128<uint>>(ref Unsafe.Add(ref rsi, 6 * 16)), 0b01_00_11_10).AsUInt64();
            xmm7 = Sse2.Shuffle(Unsafe.As<byte, Vector128<uint>>(ref Unsafe.Add(ref rsi, 7 * 16)), 0b01_00_11_10).AsUInt64();
            var xmm15 = Vector128.Create(Rk3, Rk4);
            i += 128;
            var olen = length - 127;
            for (; i < olen; i += 128)
            {
                //Since we has no access to alignments, we can't use prefetch.
                var xmm8 = Pclmulqdq.CarrylessMultiply(xmm0, xmm15, 0x11);
                xmm0 = Pclmulqdq.CarrylessMultiply(xmm0, xmm15, 0x00);
                var xmm9 = Pclmulqdq.CarrylessMultiply(xmm1, xmm15, 0x11);
                xmm1 = Pclmulqdq.CarrylessMultiply(xmm1, xmm15, 0x00);
                var xmm10 = Pclmulqdq.CarrylessMultiply(xmm2, xmm15, 0x11);
                xmm2 = Pclmulqdq.CarrylessMultiply(xmm2, xmm15, 0x00);
                var xmm11 = Pclmulqdq.CarrylessMultiply(xmm3, xmm15, 0x11);
                xmm3 = Pclmulqdq.CarrylessMultiply(xmm3, xmm15, 0x00);
                xmm0 = Sse2.Xor(xmm0, xmm8);
                var xmm12 = Sse2.Shuffle(Unsafe.As<byte, Vector128<uint>>(ref Unsafe.Add(ref rsi, i + 0 * 16)), 0b01_00_11_10).AsUInt64();
                xmm1 = Sse2.Xor(xmm1, xmm9);
                var xmm13 = Sse2.Shuffle(Unsafe.As<byte, Vector128<uint>>(ref Unsafe.Add(ref rsi, i + 1 * 16)), 0b01_00_11_10).AsUInt64();
                xmm2 = Sse2.Xor(xmm2, xmm10);
                xmm8 = Sse2.Shuffle(Unsafe.As<byte, Vector128<uint>>(ref Unsafe.Add(ref rsi, i + 2 * 16)), 0b01_00_11_10).AsUInt64();
                xmm3 = Sse2.Xor(xmm3, xmm11);
                xmm9 = Sse2.Shuffle(Unsafe.As<byte, Vector128<uint>>(ref Unsafe.Add(ref rsi, i + 3 * 16)), 0b01_00_11_10).AsUInt64();
                xmm0 = Sse2.Xor(xmm0, xmm12);
                xmm1 = Sse2.Xor(xmm1, xmm13);
                xmm2 = Sse2.Xor(xmm2, xmm8);
                xmm3 = Sse2.Xor(xmm3, xmm9);
                xmm8 = Pclmulqdq.CarrylessMultiply(xmm4, xmm15, 0x11);
                xmm4 = Pclmulqdq.CarrylessMultiply(xmm4, xmm15, 0x00);
                xmm9 = Pclmulqdq.CarrylessMultiply(xmm5, xmm15, 0x11);
                xmm5 = Pclmulqdq.CarrylessMultiply(xmm5, xmm15, 0x00);
                xmm10 = Pclmulqdq.CarrylessMultiply(xmm6, xmm15, 0x11);
                xmm6 = Pclmulqdq.CarrylessMultiply(xmm6, xmm15, 0x00);
                xmm11 = Pclmulqdq.CarrylessMultiply(xmm7, xmm15, 0x11);
                xmm7 = Pclmulqdq.CarrylessMultiply(xmm7, xmm15, 0x00);
                xmm4 = Sse2.Xor(xmm4, xmm8);
                xmm12 = Sse2.Shuffle(Unsafe.As<byte, Vector128<uint>>(ref Unsafe.Add(ref rsi, i + 4 * 16)), 0b01_00_11_10).AsUInt64();
                xmm5 = Sse2.Xor(xmm5, xmm9);
                xmm13 = Sse2.Shuffle(Unsafe.As<byte, Vector128<uint>>(ref Unsafe.Add(ref rsi, i + 5 * 16)), 0b01_00_11_10).AsUInt64();
                xmm6 = Sse2.Xor(xmm6, xmm10);
                xmm8 = Sse2.Shuffle(Unsafe.As<byte, Vector128<uint>>(ref Unsafe.Add(ref rsi, i + 6 * 16)), 0b01_00_11_10).AsUInt64();
                xmm7 = Sse2.Xor(xmm7, xmm11);
                xmm9 = Sse2.Shuffle(Unsafe.As<byte, Vector128<uint>>(ref Unsafe.Add(ref rsi, i + 7 * 16)), 0b01_00_11_10).AsUInt64();
                xmm4 = Sse2.Xor(xmm4, xmm12);
                xmm5 = Sse2.Xor(xmm5, xmm13);
                xmm6 = Sse2.Xor(xmm6, xmm8);
                xmm7 = Sse2.Xor(xmm7, xmm9);
            }
            //Reduce intermediate CRC-ish values to 128 bits.
            {
                var xmm12 = Vector128.Create(Rk9, Rk10);
                var xmm13 = Vector128.Create(Rk11, Rk12);
                var xmm14 = Vector128.Create(Rk13, Rk14);
                xmm15 = Vector128.Create(Rk15, Rk16);
                var xmm8 = Pclmulqdq.CarrylessMultiply(xmm0, xmm12, 0x11);
                xmm0 = Pclmulqdq.CarrylessMultiply(xmm0, xmm12, 0x00);
                var xmm9 = Pclmulqdq.CarrylessMultiply(xmm1, xmm13, 0x11);
                xmm1 = Pclmulqdq.CarrylessMultiply(xmm1, xmm13, 0x00);
                var xmm10 = Pclmulqdq.CarrylessMultiply(xmm2, xmm14, 0x11);
                xmm2 = Pclmulqdq.CarrylessMultiply(xmm2, xmm14, 0x00);
                var xmm11 = Pclmulqdq.CarrylessMultiply(xmm3, xmm15, 0x11);
                xmm3 = Pclmulqdq.CarrylessMultiply(xmm3, xmm15, 0x00);
                xmm0 = Sse2.Xor(xmm0, xmm8);
                xmm12 = Vector128.Create(Rk17, Rk18);
                xmm1 = Sse2.Xor(xmm1, xmm9);
                xmm13 = Vector128.Create(Rk19, Rk20);
                xmm2 = Sse2.Xor(xmm2, xmm10);
                xmm15 = Vector128.Create(Rk1, Rk2);
                xmm3 = Sse2.Xor(xmm3, xmm11);
                xmm8 = Pclmulqdq.CarrylessMultiply(xmm4, xmm12, 0x11);
                xmm4 = Pclmulqdq.CarrylessMultiply(xmm4, xmm12, 0x00);
                xmm9 = Pclmulqdq.CarrylessMultiply(xmm5, xmm13, 0x11);
                xmm5 = Pclmulqdq.CarrylessMultiply(xmm5, xmm13, 0x00);
                xmm10 = Pclmulqdq.CarrylessMultiply(xmm6, xmm15, 0x11);
                xmm6 = Pclmulqdq.CarrylessMultiply(xmm6, xmm15, 0x00);
                xmm4 = Sse2.Xor(xmm4, xmm8);
                xmm5 = Sse2.Xor(xmm5, xmm9);
                xmm6 = Sse2.Xor(xmm6, xmm10);
                xmm0 = Sse2.Xor(xmm0, xmm1);
                xmm2 = Sse2.Xor(xmm2, xmm3);
                xmm4 = Sse2.Xor(xmm4, xmm5);
                xmm6 = Sse2.Xor(xmm6, xmm7);
                xmm0 = Sse2.Xor(xmm0, xmm2);
                xmm4 = Sse2.Xor(xmm4, xmm6);
                xmm0 = Sse2.Xor(xmm0, xmm4);
            }
            //xmm15 = Vector128.Create(Rk1, Rk2);
            olen = length - 15;
            for (; i < olen; i += 16)
            {
                var xmm8 = Pclmulqdq.CarrylessMultiply(xmm0, xmm15, 0x11);
                xmm0 = Pclmulqdq.CarrylessMultiply(xmm0, xmm15, 0x00);
                var xmm12 = Sse2.Shuffle(Unsafe.As<byte, Vector128<uint>>(ref Unsafe.Add(ref rsi, i)), 0b01_00_11_10).AsUInt64();
                xmm0 = Sse2.Xor(xmm0, xmm8);
                xmm0 = Sse2.Xor(xmm0, xmm12);
            }
            //128-bit segments are done.
            //Now we have to reduce xmm0 to 32bits.
            //Final word(ulong) can be calculated by existing method.
            xmm15 = Vector128.Create(Rk5, Rk6);
            xmm7 = Pclmulqdq.CarrylessMultiply(xmm0, xmm15, 0x01);
            xmm0 = Sse2.ShiftLeftLogical128BitLane(xmm0, 8);
            xmm7 = Sse2.Xor(xmm7, xmm0);
            xmm0 = Sse2.And(xmm7, Vector128.Create(0xFFFFFFFFFFFFFFFF, 0x00000000FFFFFFFF));
            xmm7 = Sse2.ShiftRightLogical128BitLane(xmm7, 12);
            xmm7 = Pclmulqdq.CarrylessMultiply(xmm7, xmm15, 0x10);
            xmm15 = Vector128.Create(Rk7, Rk8);
            xmm0 = Sse2.Xor(xmm7, xmm0);
            xmm7 = Pclmulqdq.CarrylessMultiply(xmm0, xmm15, 0x01);
            xmm7 = Sse2.ShiftLeftLogical128BitLane(xmm7, 4);
            xmm7 = Pclmulqdq.CarrylessMultiply(xmm7, xmm15, 0x11);
            xmm7 = Sse2.ShiftLeftLogical128BitLane(xmm7, 4);
            xmm0 = Sse2.Xor(xmm7, xmm0);
            left = new((ushort)(xmm0.AsUInt32().GetElement(1) >> 16));
            if (i < length)
            {
                left *= Unsafe.As<byte, ulong>(ref Unsafe.Add(ref rsi, i));
            }
            return left;
        }
#endif
        #endregion

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the obj parameter; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public override bool Equals(object? obj) => obj is FlacCrc16 crc && Equals(crc);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public bool Equals(FlacCrc16 other) => state == other.state;

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public override int GetHashCode() => HashCode.Combine(state);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="FlacCrc16"/> objects are equal.
        /// </summary>
        /// <param name="left">The first <see cref="FlacCrc16"/> to compare.</param>
        /// <param name="right">The second <see cref="FlacCrc16"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if the left is the same as the right; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool operator ==(FlacCrc16 left, FlacCrc16 right) => left.Equals(right);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="FlacCrc16"/> objects are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="FlacCrc16"/> to compare.</param>
        /// <param name="right">The second  <see cref="FlacCrc16"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if left and right are not equal; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool operator !=(FlacCrc16 left, FlacCrc16 right) => !(left == right);

        private string GetDebuggerDisplay() => $"{state}";

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// The converted <see cref="string"/> instance.
        /// </returns>
        public override string? ToString() => GetDebuggerDisplay();
    }
}
