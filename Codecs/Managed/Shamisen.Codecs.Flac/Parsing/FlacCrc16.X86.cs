using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Text;

using Shamisen.Utils;

namespace Shamisen.Codecs.Flac.Parsing
{
    public readonly partial struct FlacCrc16
    {
        internal static partial class X86
        {
            public static bool IsSupported => Pclmulqdq.IsSupported && Sse2.IsSupported && Ssse3.IsSupported;

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

            #region Constants
            // Rk1: Modulus for order 64
            private const ulong Rk1 = 0x8663_0000_0000_0000;
            // Rk2: Modulus for order 128
            private const ulong Rk2 = 0x8617_0000_0000_0000;
            // Rk3: Modulus for order 960
            private const ulong Rk3 = 0x8665_0000_0000_0000;
            // Rk4: Modulus for order 1024
            private const ulong Rk4 = 0x8077_0000_0000_0000;
            // Rk6: Modulus for order 32
            private const ulong Rk6 = 0x807b_0000_0000_0000;
            // Rk7: Quotient for order 64
            private const ulong FloorX64OverP = 0x0000_0001_fffb_ffe7;
            // Rk8: Polynomial x^16 + x^15 + x^2 + 1 shifted left 32 bits
            private const ulong Polynomial = 0x0000_0001_8005_0000;
            // Rk9: Modulus for order 832
            private const ulong Rk9 = 0x6a7a_0000_0000_0000;
            // Rk10: Modulus for order 896
            private const ulong Rk10 = 0x5ccb_0000_0000_0000;
            // Rk11: Modulus for order 704
            private const ulong Rk11 = 0x006b_0000_0000_0000;
            // Rk12: Modulus for order 768
            private const ulong Rk12 = 0xedb3_0000_0000_0000;
            // Rk13: Modulus for order 576
            private const ulong Rk13 = 0xf997_0000_0000_0000;
            // Rk14: Modulus for order 640
            private const ulong Rk14 = 0x8c47_0000_0000_0000;
            // Rk15: Modulus for order 448
            private const ulong Rk15 = 0xbffa_0000_0000_0000;
            // Rk16: Modulus for order 512
            private const ulong Rk16 = 0x861b_0000_0000_0000;
            // Rk17: Modulus for order 320
            private const ulong Rk17 = 0xeac3_0000_0000_0000;
            // Rk18: Modulus for order 384
            private const ulong Rk18 = 0xed6b_0000_0000_0000;
            // Rk19: Modulus for order 192
            private const ulong Rk19 = 0xf557_0000_0000_0000;
            // Rk20: Modulus for order 256
            private const ulong Rk20 = 0x806f_0000_0000_0000;
            #endregion

            private static ReadOnlySpan<byte> ShuffleTable => [0, 129, 130, 131, 132, 133, 134, 135, 136, 137, 138, 139, 140, 141, 142, 143, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 0];
            internal static FlacCrc16 CalculateCrc16UInt64BigEndianPclmulqdq(FlacCrc16 left, ReadOnlySpan<ulong> right)
            {
                ref var rsi = ref Unsafe.As<ulong, byte>(ref MemoryMarshal.GetReference(right));
                nuint i = 0, length = (nuint)right.Length * sizeof(ulong);
                if (length == 0) return left;
                Vector128<ulong> xmm0 = Vector128.CreateScalar((uint)left.State << 16).AsUInt64(), xmm1, xmm2, xmm3, xmm4, xmm5, xmm6, xmm7;
                xmm0 = Sse2.ShiftLeftLogical128BitLane(xmm0, 12);
                Vector128<ulong> xmm8, xmm9, xmm10, xmm11, xmm12, xmm13, xmm14;
                var xmm15 = Vector128.Create(Rk3, Rk4);
                if (length >= (nuint)Vector128<byte>.Count)
                {
                    xmm0 ^= Sse2.Shuffle(Vector128.LoadUnsafe(ref rsi, 0 * 16).AsUInt32(), 0b01_00_11_10).AsUInt64();
                    i += 16;
                    var olen = length - 127;
                    if (olen < length)
                    {
                        // Load the initial 128 bytes except the first 16 bytes.
                        // We don't have to reverse endianness because we had done it before.
                        xmm1 = Sse2.Shuffle(Vector128.LoadUnsafe(ref rsi, 1 * 16).AsUInt32(), 0b01_00_11_10).AsUInt64();
                        xmm2 = Sse2.Shuffle(Vector128.LoadUnsafe(ref rsi, 2 * 16).AsUInt32(), 0b01_00_11_10).AsUInt64();
                        xmm3 = Sse2.Shuffle(Vector128.LoadUnsafe(ref rsi, 3 * 16).AsUInt32(), 0b01_00_11_10).AsUInt64();
                        xmm4 = Sse2.Shuffle(Vector128.LoadUnsafe(ref rsi, 4 * 16).AsUInt32(), 0b01_00_11_10).AsUInt64();
                        xmm5 = Sse2.Shuffle(Vector128.LoadUnsafe(ref rsi, 5 * 16).AsUInt32(), 0b01_00_11_10).AsUInt64();
                        xmm6 = Sse2.Shuffle(Vector128.LoadUnsafe(ref rsi, 6 * 16).AsUInt32(), 0b01_00_11_10).AsUInt64();
                        xmm7 = Sse2.Shuffle(Vector128.LoadUnsafe(ref rsi, 7 * 16).AsUInt32(), 0b01_00_11_10).AsUInt64();
                        i += 128 - 16;
                        for (; i < olen; i += 128)
                        {
                            //Since we have no access to alignments, we can't use prefetch.
                            xmm8 = Pclmulqdq.CarrylessMultiply(xmm0, xmm15, 0x11);
                            xmm0 = Pclmulqdq.CarrylessMultiply(xmm0, xmm15, 0x00);
                            xmm9 = Pclmulqdq.CarrylessMultiply(xmm1, xmm15, 0x11);
                            xmm1 = Pclmulqdq.CarrylessMultiply(xmm1, xmm15, 0x00);
                            xmm10 = Pclmulqdq.CarrylessMultiply(xmm2, xmm15, 0x11);
                            xmm2 = Pclmulqdq.CarrylessMultiply(xmm2, xmm15, 0x00);
                            xmm11 = Pclmulqdq.CarrylessMultiply(xmm3, xmm15, 0x11);
                            xmm3 = Pclmulqdq.CarrylessMultiply(xmm3, xmm15, 0x00);
                            xmm0 ^= xmm8;
                            xmm12 = Sse2.Shuffle(Vector128.LoadUnsafe(ref rsi, i + 0 * 16).AsUInt32(), 0b01_00_11_10).AsUInt64();
                            xmm1 ^= xmm9;
                            xmm13 = Sse2.Shuffle(Vector128.LoadUnsafe(ref rsi, i + 1 * 16).AsUInt32(), 0b01_00_11_10).AsUInt64();
                            xmm2 ^= xmm10;
                            xmm8 = Sse2.Shuffle(Vector128.LoadUnsafe(ref rsi, i + 2 * 16).AsUInt32(), 0b01_00_11_10).AsUInt64();
                            xmm3 ^= xmm11;
                            xmm9 = Sse2.Shuffle(Vector128.LoadUnsafe(ref rsi, i + 3 * 16).AsUInt32(), 0b01_00_11_10).AsUInt64();
                            xmm0 ^= xmm12;
                            xmm1 ^= xmm13;
                            xmm2 ^= xmm8;
                            xmm3 ^= xmm9;
                            xmm8 = Pclmulqdq.CarrylessMultiply(xmm4, xmm15, 0x11);
                            xmm4 = Pclmulqdq.CarrylessMultiply(xmm4, xmm15, 0x00);
                            xmm9 = Pclmulqdq.CarrylessMultiply(xmm5, xmm15, 0x11);
                            xmm5 = Pclmulqdq.CarrylessMultiply(xmm5, xmm15, 0x00);
                            xmm10 = Pclmulqdq.CarrylessMultiply(xmm6, xmm15, 0x11);
                            xmm6 = Pclmulqdq.CarrylessMultiply(xmm6, xmm15, 0x00);
                            xmm11 = Pclmulqdq.CarrylessMultiply(xmm7, xmm15, 0x11);
                            xmm7 = Pclmulqdq.CarrylessMultiply(xmm7, xmm15, 0x00);
                            xmm4 ^= xmm8;
                            xmm12 = Sse2.Shuffle(Vector128.LoadUnsafe(ref rsi, i + 4 * 16).AsUInt32(), 0b01_00_11_10).AsUInt64();
                            xmm5 ^= xmm9;
                            xmm13 = Sse2.Shuffle(Vector128.LoadUnsafe(ref rsi, i + 5 * 16).AsUInt32(), 0b01_00_11_10).AsUInt64();
                            xmm6 ^= xmm10;
                            xmm8 = Sse2.Shuffle(Vector128.LoadUnsafe(ref rsi, i + 6 * 16).AsUInt32(), 0b01_00_11_10).AsUInt64();
                            xmm7 ^= xmm11;
                            xmm9 = Sse2.Shuffle(Vector128.LoadUnsafe(ref rsi, i + 7 * 16).AsUInt32(), 0b01_00_11_10).AsUInt64();
                            xmm4 ^= xmm12;
                            xmm5 ^= xmm13;
                            xmm6 ^= xmm8;
                            xmm7 ^= xmm9;
                        }
                        //Reduce intermediate values to 128 bits.
                        xmm12 = Vector128.Create(Rk9, Rk10);
                        xmm13 = Vector128.Create(Rk11, Rk12);
                        xmm14 = Vector128.Create(Rk13, Rk14);
                        xmm15 = Vector128.Create(Rk15, Rk16);
                        xmm8 = Pclmulqdq.CarrylessMultiply(xmm0, xmm12, 0x11);
                        xmm0 = Pclmulqdq.CarrylessMultiply(xmm0, xmm12, 0x00);
                        xmm9 = Pclmulqdq.CarrylessMultiply(xmm1, xmm13, 0x11);
                        xmm1 = Pclmulqdq.CarrylessMultiply(xmm1, xmm13, 0x00);
                        xmm10 = Pclmulqdq.CarrylessMultiply(xmm2, xmm14, 0x11);
                        xmm2 = Pclmulqdq.CarrylessMultiply(xmm2, xmm14, 0x00);
                        xmm11 = Pclmulqdq.CarrylessMultiply(xmm3, xmm15, 0x11);
                        xmm3 = Pclmulqdq.CarrylessMultiply(xmm3, xmm15, 0x00);
                        xmm0 ^= xmm8;
                        xmm12 = Vector128.Create(Rk17, Rk18);
                        xmm1 ^= xmm9;
                        xmm13 = Vector128.Create(Rk19, Rk20);
                        xmm2 ^= xmm10;
                        xmm15 = Vector128.Create(Rk1, Rk2);
                        xmm3 ^= xmm11;
                        xmm8 = Pclmulqdq.CarrylessMultiply(xmm4, xmm12, 0x11);
                        xmm4 = Pclmulqdq.CarrylessMultiply(xmm4, xmm12, 0x00);
                        xmm9 = Pclmulqdq.CarrylessMultiply(xmm5, xmm13, 0x11);
                        xmm5 = Pclmulqdq.CarrylessMultiply(xmm5, xmm13, 0x00);
                        xmm10 = Pclmulqdq.CarrylessMultiply(xmm6, xmm15, 0x11);
                        xmm6 = Pclmulqdq.CarrylessMultiply(xmm6, xmm15, 0x00);
                        xmm4 ^= xmm8;
                        xmm5 ^= xmm9;
                        xmm6 ^= xmm10;
                        xmm0 ^= xmm1;
                        xmm2 ^= xmm3;
                        xmm4 ^= xmm5;
                        xmm6 ^= xmm7;
                        xmm0 ^= xmm2;
                        xmm4 ^= xmm6;
                        xmm0 ^= xmm4;
                    }
                    xmm15 = Vector128.Create(Rk1, Rk2);
                    olen = length - 15;
                    if (olen < length)
                    {
                        for (; i < olen; i += 16)
                        {
                            xmm8 = Pclmulqdq.CarrylessMultiply(xmm0, xmm15, 0x11);
                            xmm0 = Pclmulqdq.CarrylessMultiply(xmm0, xmm15, 0x00);
                            xmm12 = Sse2.Shuffle(Vector128.LoadUnsafe(ref rsi, i).AsUInt32(), 0b01_00_11_10).AsUInt64();
                            xmm0 ^= xmm8;
                            xmm0 ^= xmm12;
                        }
                    }
                }
                if (i < length)
                {
                    xmm8 = VectorUtils.LoadTail128((int)(i / sizeof(ulong)), right);
                    xmm0 ^= Sse2.Shuffle(xmm8.AsUInt32(), 0b01_00_11_10).AsUInt64();
                }
                // 128-bit segments are done.
                // Now we have to reduce xmm0 to 32bits.
                xmm15 = Vector128.Create(Rk1, Rk6);
                xmm7 = Pclmulqdq.CarrylessMultiply(xmm0, xmm15, 0x01);
                xmm0 = Sse2.ShiftLeftLogical128BitLane(xmm0, 8);
                xmm7 ^= xmm0;
                xmm0 = Sse2.And(xmm7, Vector128.Create(0xFFFFFFFFFFFFFFFF, 0x00000000FFFFFFFF));
                xmm7 = Sse2.ShiftRightLogical128BitLane(xmm7, 12);
                xmm7 = Pclmulqdq.CarrylessMultiply(xmm7, xmm15, 0x10);
                xmm15 = Vector128.Create(FloorX64OverP, Polynomial);
                xmm0 = xmm7 ^ xmm0;
                xmm7 = Pclmulqdq.CarrylessMultiply(xmm0, xmm15, 0x01);
                xmm7 = Sse2.ShiftLeftLogical128BitLane(xmm7, 4);
                xmm7 = Pclmulqdq.CarrylessMultiply(xmm7, xmm15, 0x11);
                xmm7 = Sse2.ShiftLeftLogical128BitLane(xmm7, 4);
                xmm0 = xmm7 ^ xmm0;
                left = new((ushort)(xmm0.GetElement(0) >> 48));
                return left;
            }

            internal static FlacCrc16 CalculateCrc16BytePclmulqdq(FlacCrc16 left, ReadOnlySpan<byte> right)
            {
                ref var rsi = ref MemoryMarshal.GetReference(right);
                nuint i = 0, length = (nuint)right.Length;
                if (length == 0) return left;
                Vector128<ulong> xmm0 = Vector128.CreateScalar((uint)left.State << 16).AsUInt64(), xmm1, xmm2, xmm3, xmm4, xmm5, xmm6, xmm7;
                xmm0 = Sse2.ShiftLeftLogical128BitLane(xmm0, 12);
                Vector128<ulong> xmm8, xmm9, xmm10, xmm11, xmm12, xmm13;
                var xmm14 = Vector128.Create((byte)15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0);
                var xmm15 = Vector128.Create(Rk3, Rk4);
                if (length >= (nuint)Vector128<byte>.Count)
                {
                    xmm0 ^= Ssse3.Shuffle(Vector128.LoadUnsafe(ref rsi, 0 * 16).AsByte(), xmm14).AsUInt64();
                    i += 16;
                    var olen = length - 127;
                    if (olen < length)
                    {
                        // Load the initial 128 bytes except the first 16 bytes.
                        xmm1 = Ssse3.Shuffle(Vector128.LoadUnsafe(ref rsi, 1 * 16).AsByte(), xmm14).AsUInt64();
                        xmm2 = Ssse3.Shuffle(Vector128.LoadUnsafe(ref rsi, 2 * 16).AsByte(), xmm14).AsUInt64();
                        xmm3 = Ssse3.Shuffle(Vector128.LoadUnsafe(ref rsi, 3 * 16).AsByte(), xmm14).AsUInt64();
                        xmm4 = Ssse3.Shuffle(Vector128.LoadUnsafe(ref rsi, 4 * 16).AsByte(), xmm14).AsUInt64();
                        xmm5 = Ssse3.Shuffle(Vector128.LoadUnsafe(ref rsi, 5 * 16).AsByte(), xmm14).AsUInt64();
                        xmm6 = Ssse3.Shuffle(Vector128.LoadUnsafe(ref rsi, 6 * 16).AsByte(), xmm14).AsUInt64();
                        xmm7 = Ssse3.Shuffle(Vector128.LoadUnsafe(ref rsi, 7 * 16).AsByte(), xmm14).AsUInt64();
                        i += 128 - 16;
                        for (; i < olen; i += 128)
                        {
                            //Since we have no access to alignments, we can't use prefetch.
                            xmm8 = Pclmulqdq.CarrylessMultiply(xmm0, xmm15, 0x11);
                            xmm0 = Pclmulqdq.CarrylessMultiply(xmm0, xmm15, 0x00);
                            xmm9 = Pclmulqdq.CarrylessMultiply(xmm1, xmm15, 0x11);
                            xmm1 = Pclmulqdq.CarrylessMultiply(xmm1, xmm15, 0x00);
                            xmm10 = Pclmulqdq.CarrylessMultiply(xmm2, xmm15, 0x11);
                            xmm2 = Pclmulqdq.CarrylessMultiply(xmm2, xmm15, 0x00);
                            xmm11 = Pclmulqdq.CarrylessMultiply(xmm3, xmm15, 0x11);
                            xmm3 = Pclmulqdq.CarrylessMultiply(xmm3, xmm15, 0x00);
                            xmm0 ^= xmm8;
                            xmm12 = Ssse3.Shuffle(Vector128.LoadUnsafe(ref rsi, i + 0 * 16).AsByte(), xmm14).AsUInt64();
                            xmm1 ^= xmm9;
                            xmm13 = Ssse3.Shuffle(Vector128.LoadUnsafe(ref rsi, i + 1 * 16).AsByte(), xmm14).AsUInt64();
                            xmm2 ^= xmm10;
                            xmm8 = Ssse3.Shuffle(Vector128.LoadUnsafe(ref rsi, i + 2 * 16).AsByte(), xmm14).AsUInt64();
                            xmm3 ^= xmm11;
                            xmm9 = Ssse3.Shuffle(Vector128.LoadUnsafe(ref rsi, i + 3 * 16).AsByte(), xmm14).AsUInt64();
                            xmm0 ^= xmm12;
                            xmm1 ^= xmm13;
                            xmm2 ^= xmm8;
                            xmm3 ^= xmm9;
                            xmm8 = Pclmulqdq.CarrylessMultiply(xmm4, xmm15, 0x11);
                            xmm4 = Pclmulqdq.CarrylessMultiply(xmm4, xmm15, 0x00);
                            xmm9 = Pclmulqdq.CarrylessMultiply(xmm5, xmm15, 0x11);
                            xmm5 = Pclmulqdq.CarrylessMultiply(xmm5, xmm15, 0x00);
                            xmm10 = Pclmulqdq.CarrylessMultiply(xmm6, xmm15, 0x11);
                            xmm6 = Pclmulqdq.CarrylessMultiply(xmm6, xmm15, 0x00);
                            xmm11 = Pclmulqdq.CarrylessMultiply(xmm7, xmm15, 0x11);
                            xmm7 = Pclmulqdq.CarrylessMultiply(xmm7, xmm15, 0x00);
                            xmm4 ^= xmm8;
                            xmm12 = Ssse3.Shuffle(Vector128.LoadUnsafe(ref rsi, i + 4 * 16).AsByte(), xmm14).AsUInt64();
                            xmm5 ^= xmm9;
                            xmm13 = Ssse3.Shuffle(Vector128.LoadUnsafe(ref rsi, i + 5 * 16).AsByte(), xmm14).AsUInt64();
                            xmm6 ^= xmm10;
                            xmm8 = Ssse3.Shuffle(Vector128.LoadUnsafe(ref rsi, i + 6 * 16).AsByte(), xmm14).AsUInt64();
                            xmm7 ^= xmm11;
                            xmm9 = Ssse3.Shuffle(Vector128.LoadUnsafe(ref rsi, i + 7 * 16).AsByte(), xmm14).AsUInt64();
                            xmm4 ^= xmm12;
                            xmm5 ^= xmm13;
                            xmm6 ^= xmm8;
                            xmm7 ^= xmm9;
                        }
                        //Reduce intermediate values to 128 bits.
                        xmm12 = Vector128.Create(Rk9, Rk10);
                        xmm13 = Vector128.Create(Rk11, Rk12);
                        xmm8 = Pclmulqdq.CarrylessMultiply(xmm0, xmm12, 0x11);
                        xmm0 = Pclmulqdq.CarrylessMultiply(xmm0, xmm12, 0x00);
                        xmm12 = Vector128.Create(Rk13, Rk14);
                        xmm9 = Pclmulqdq.CarrylessMultiply(xmm1, xmm13, 0x11);
                        xmm1 = Pclmulqdq.CarrylessMultiply(xmm1, xmm13, 0x00);
                        xmm13 = Vector128.Create(Rk15, Rk16);
                        xmm10 = Pclmulqdq.CarrylessMultiply(xmm2, xmm12, 0x11);
                        xmm2 = Pclmulqdq.CarrylessMultiply(xmm2, xmm12, 0x00);
                        xmm12 = Vector128.Create(Rk17, Rk18);
                        xmm11 = Pclmulqdq.CarrylessMultiply(xmm3, xmm13, 0x11);
                        xmm3 = Pclmulqdq.CarrylessMultiply(xmm3, xmm13, 0x00);
                        xmm13 = Vector128.Create(Rk19, Rk20);
                        xmm0 ^= xmm8;
                        xmm1 ^= xmm9;
                        xmm2 ^= xmm10;
                        xmm3 ^= xmm11;
                        xmm8 = Pclmulqdq.CarrylessMultiply(xmm4, xmm12, 0x11);
                        xmm4 = Pclmulqdq.CarrylessMultiply(xmm4, xmm12, 0x00);
                        xmm12 = Vector128.Create(Rk1, Rk2);
                        xmm9 = Pclmulqdq.CarrylessMultiply(xmm5, xmm13, 0x11);
                        xmm5 = Pclmulqdq.CarrylessMultiply(xmm5, xmm13, 0x00);
                        xmm10 = Pclmulqdq.CarrylessMultiply(xmm6, xmm12, 0x11);
                        xmm6 = Pclmulqdq.CarrylessMultiply(xmm6, xmm12, 0x00);
                        xmm4 ^= xmm8;
                        xmm5 ^= xmm9;
                        xmm6 ^= xmm10;
                        xmm0 ^= xmm1;
                        xmm2 ^= xmm3;
                        xmm4 ^= xmm5;
                        xmm6 ^= xmm7;
                        xmm0 ^= xmm2;
                        xmm4 ^= xmm6;
                        xmm0 ^= xmm4;
                    }
                    xmm15 = Vector128.Create(Rk1, Rk2);
                    olen = length - 15;
                    if (olen < length)
                    {
                        for (; i < olen; i += 16)
                        {
                            xmm8 = Pclmulqdq.CarrylessMultiply(xmm0, xmm15, 0x11);
                            xmm0 = Pclmulqdq.CarrylessMultiply(xmm0, xmm15, 0x00);
                            xmm12 = Ssse3.Shuffle(Vector128.LoadUnsafe(ref rsi, i).AsByte(), xmm14).AsUInt64();
                            xmm0 ^= xmm8;
                            xmm0 ^= xmm12;
                        }
                    }
                }
                xmm15 = Vector128.Create(Rk1, Rk2);
                var mask = (nuint)(-Unsafe.BitCast<bool, byte>(i > 0)) & 15;
                if (i < length)
                {
                    var elementsOffset = (byte)((i - length) & mask);
                    var remainingElements = length - i;
                    xmm2 = Vector128.Create(elementsOffset).AsUInt64();
                    xmm3 = (xmm14 - xmm2.AsByte()).AsUInt64();
                    xmm8 = VectorUtils.LoadTail128((int)i, right).AsUInt64();
                    xmm8 = Ssse3.Shuffle(xmm8.AsByte(), xmm3.AsByte()).AsUInt64();
                    xmm14 = Vector128.LoadUnsafe(ref MemoryMarshal.GetReference(ShuffleTable), 16u - (length - i));
                    xmm13 = (xmm14 ^ Vector128.Create((byte)128)).AsUInt64();
                    if (i > 0)
                    {
                        xmm9 = Ssse3.Shuffle(xmm0.AsByte(), xmm14).AsUInt64();
                        xmm0 = Ssse3.Shuffle(xmm0.AsByte(), xmm13.AsByte()).AsUInt64();
                        xmm8 = Sse41.BlendVariable(xmm8.AsByte(), xmm9.AsByte(), xmm13.AsByte()).AsUInt64();
                        xmm7 = Pclmulqdq.CarrylessMultiply(xmm0, xmm15, 0x11);
                        xmm0 = Pclmulqdq.CarrylessMultiply(xmm0, xmm15, 0x00);
                        xmm0 ^= xmm7;
                        xmm0 ^= xmm8;
                    }
                    else
                    {
                        xmm0 ^= xmm8;
                        if (length >= 4)
                        {
                            xmm0 = Ssse3.Shuffle(xmm0.AsByte(), xmm13.AsByte()).AsUInt64();
                            i++;
                        }
                        else
                        {
                            xmm14 = Vector128<byte>.Indices + Vector128.Create((byte)8) - Vector128.Create((byte)remainingElements);
                            xmm0 = Ssse3.Shuffle(xmm0.AsByte(), xmm14).AsUInt64();
                        }
                    }
                }
                xmm12 = Vector128.Create(FloorX64OverP, Polynomial);
                // 128-bit segments are done.
                // Now we have to reduce xmm0 to 32bits.
                if (i > 0)
                {
                    xmm15 = Vector128.Create(Rk1, Rk6);
                    xmm7 = Pclmulqdq.CarrylessMultiply(xmm0, xmm15, 0x01);
                    xmm0 = Sse2.ShiftLeftLogical128BitLane(xmm0, 8);
                    xmm7 ^= xmm0;
                    xmm0 = Sse2.And(xmm7, Vector128.Create(0xFFFFFFFFFFFFFFFF, 0x00000000FFFFFFFF));
                    xmm7 = Sse2.ShiftRightLogical128BitLane(xmm7, 12);
                    xmm7 = Pclmulqdq.CarrylessMultiply(xmm7, xmm15, 0x10);
                    xmm0 = xmm7 ^ xmm0;
                }
                // Barrett reduction
                xmm7 = Pclmulqdq.CarrylessMultiply(xmm0, xmm12, 0x01);
                xmm7 = Sse2.ShiftLeftLogical128BitLane(xmm7, 4);
                xmm7 = Pclmulqdq.CarrylessMultiply(xmm7, xmm12, 0x11);
                xmm7 = Sse2.ShiftLeftLogical128BitLane(xmm7, 4);
                xmm0 = xmm7 ^ xmm0;
                left = new((ushort)(xmm0.GetElement(0) >> 48));
                //for (; i < length; i++)
                //{
                //    left *= Unsafe.Add(ref rsi, i);
                //}
                return left;
            }

        }
    }
}
