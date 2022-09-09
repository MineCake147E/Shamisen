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

#if NETCOREAPP3_1_OR_GREATER

using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

#endif
#if NET5_0_OR_GREATER

using System.Runtime.Intrinsics.Arm;

#endif

using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Buffers.Binary;
using System.ComponentModel;

using Shamisen.Conversion;

namespace Shamisen.Codecs.Flac.Parsing
{
    /// <summary>
    /// Provides a functionality for reading bit stream data.
    /// </summary>
    public sealed partial class FlacBitReader
    {
        private const int BitsPerWord = 8 * BytesPerWord;
        private const int BytesPerWord = sizeof(ulong);
        private const int DefaultCapacity = 131072 / BitsPerWord;

        private ulong[]? buffer;
        private int bytesOfIncompleteWord;
        private int consumedBits;
        private int consumedWords;
        private FlacCrc16 crc16;
        private int crcAlignBits;
        private int crcOffset;
        private bool disposedValue;
        private volatile bool isEndOfStream = false;
        private IReadableDataSource<byte>? source;
        private int words;

        /// <summary>
        /// Gets the consumed bits in a word.
        /// </summary>
        /// <value>
        /// The consumed bits.
        /// </value>
        public int ConsumedBits => consumedBits;

        /// <summary>
        /// Gets the consumed words.
        /// </summary>
        /// <value>
        /// The consumed words.
        /// </value>
        public int ConsumedWords => consumedWords;

        /// <summary>
        /// Gets or resets the current CRC16.
        /// </summary>
        /// <value>
        /// The CRC16.
        /// </value>
        public FlacCrc16 Crc16
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => crc16;
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set
            {
                crc16 = value;
                crcOffset = consumedWords;
                crcAlignBits = consumedBits;
            }
        }

        /// <summary>
        /// Gets the debug dump of current word.
        /// </summary>
        /// <value>
        /// The debug dump current word.
        /// </value>
        public string DebugDumpCurrentWord
        {
            get
            {
                var sb = new StringBuilder();
                DumpWord(sb, consumedWords, FullSpan[consumedWords]);
                return sb.ToString();
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is byte aligned.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is byte aligned; otherwise, <c>false</c>.
        /// </value>
        public bool IsByteAligned
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => (consumedBits & 7) == 0;
        }

        /// <summary>
        /// Gets the remaining unaligned bits.
        /// </summary>
        /// <value>
        /// The remaining unaligned bits.
        /// </value>
        public int RemainingUnalignedBits
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => 8 - (consumedBits & 7);
        }

        /// <inheritdoc cref="IAudioConverter{TFrom, TFromFormat, TTo, TToFormat}.Source"/>
        /// <exception cref="ObjectDisposedException">FlacBitReader</exception>
        public IReadableDataSource<byte> Source => source ?? throw new ObjectDisposedException(nameof(FlacBitReader));

        /// <summary>
        /// Gets the unconsumed input bits.
        /// </summary>
        /// <value>
        /// The unconsumed input bits.
        /// </value>
        public int UnconsumedInputBits
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => (words - consumedWords) * BitsPerWord + bytesOfIncompleteWord * 8 - consumedBits;
        }

        private Span<ulong> AvailableSpan
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => FullSpan.SliceWhile(words);
        }

        private ulong[] Buffer
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => buffer ?? throw new ObjectDisposedException(nameof(FlacBitReader));
        }

        private int Capacity => Buffer.Length;

        private Span<ulong> FullSpan
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Buffer;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FlacBitReader"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="crc16">The CRC16.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public FlacBitReader(IReadableDataSource<byte>? source, FlacCrc16 crc16 = default)
        {
            this.source = source;
            words = bytesOfIncompleteWord = 0;
            consumedBits = consumedWords = 0;
            buffer = new ulong[DefaultCapacity];
        }

        /// <summary>
        /// Clears this <see cref="FlacBitReader"/>.
        /// </summary>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public void Clear()
        {
            words = bytesOfIncompleteWord = 0;
            consumedWords = consumedBits = 0;
        }

        /// <summary>
        /// Reads the data to the specified <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>The length of the data written.</returns>
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        public ReadResult ReadAlignedLarge(Span<byte> buffer)
        {
            var bhead = buffer;
            if (consumedBits % 8 != 0)
            {
                return ReadBytes(bhead);
            }
            else
            {
                var bbal = bhead.SliceWhile(RemainingUnalignedBits / 8);
                if (ReadBytes(bbal).Length < bbal.Length) return ReadResult.WaitingForSource;

            }
            if (bhead.IsEmpty)
            {
                return buffer.Length;
            }
            while (bhead.Length >= sizeof(ulong))
            {
                var u = MemoryMarshal.Cast<byte, ulong>(bhead.SliceWhileIfLongerThan(AvailableSpan.Length));
                bhead = bhead.Slice(u.Length * sizeof(ulong));
                AvailableSpan.SliceWhileIfLongerThan(u.Length).CopyTo(u);
                u.ReverseEndianness();
                if (bhead.Length > sizeof(ulong) && !ReadFromSource()) return ReadResult.WaitingForSource;
            }
            return ReadBytes(bhead).Length < bhead.Length ? ReadResult.WaitingForSource : buffer.Length;
        }

        /// <summary>
        /// Reads the data to the specified <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>The length of the data written.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public ReadResult ReadBytes(Span<byte> buffer)
        {
            ref var bhead = ref MemoryMarshal.GetReference(buffer);
            nint i = 0;
            nint length = buffer.Length;
            var olen = length - 7;
            for (; i < olen; i += 8)
            {
                if (!ReadBitsUInt64(64, out var u)) return (int)i;
                Unsafe.As<byte, ulong>(ref Unsafe.Add(ref bhead, i)) = BinaryExtensions.ConvertToBigEndian(u);
            }
            if (i < length)
            {
                var rlen = 8 * (length - i);
                if (!ReadBitsUInt64((byte)rlen, out var u)) return (int)i;
                u = BinaryExtensions.ConvertToBigEndian(u << (int)-rlen);
                if (i < length - 3)
                {
                    Unsafe.As<byte, uint>(ref Unsafe.Add(ref bhead, i)) = (uint)u;
                    u >>= 32;
                    i += 4;
                }
                if (i < length - 1)
                {
                    Unsafe.As<byte, ushort>(ref Unsafe.Add(ref bhead, i)) = (ushort)u;
                    u >>= 16;
                    i += 2;
                }
                for (; i < length; i++)
                {
                    Unsafe.Add(ref bhead, i) = (byte)u;
                    u >>= 8;
                }
            }
            return (int)i;
        }

        /// <summary>
        ///  Reads the number with specified <paramref name="bits"/>.
        /// </summary>
        /// <param name="bits">The bits to read. must be &lt;=32.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        [Obsolete("Use out parameterized one instead!")]
        public int? ReadBitsInt32(byte bits) => !ReadBitsInt32(bits, out var i) ? null : i;

        /// <summary>
        /// Reads the number with specified <paramref name="bits"/>.
        /// </summary>
        /// <param name="bits">The bits.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public bool ReadBitsInt32(byte bits, out int value)
        {
            value = 0;
            if (bits < 1 || !ReadBitsUInt32(bits, out var h))
            {
                return false;
            }
            var gg = h;
            gg <<= 32 - bits;
            value = (int)gg >> (32 - bits);
            return true;
        }

        /// <summary>
        /// Reads the number with specified <paramref name="bits"/>.
        /// </summary>
        /// <param name="bits">The bits to read. must be &lt;=32.</param>
        /// <returns></returns>
        [Obsolete("Use ReadBitsUInt32(byte, out int) instead!")]
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public uint? ReadBitsUInt32(byte bits) => ReadBitsUInt32(bits, out var value) ? value : null;

        /// <summary>
        /// Reads the number with specified <paramref name="bits" />.
        /// </summary>
        /// <param name="bits">The bits to read. must be &lt;=32.</param>
        /// <param name="value">The value read.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// bits - The bits must be less than 33!
        /// or
        /// bits
        /// </exception>
        /// <exception cref="FlacException">
        /// This is a bug!
        /// </exception>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public bool ReadBitsUInt32(byte bits, out uint value)
        {
            var buf = AvailableSpan;
            if (bits > 32) throw new ArgumentOutOfRangeException(nameof(bits), "The bits must be less than 33!");
            if (Capacity * 2 < bits) throw new ArgumentOutOfRangeException(nameof(bits), "");
            var cw = consumedWords;

            if (cw > buf.Length) throw new FlacException("", this);
            if (bits == 0)
            {
                value = 0;
                return false;
            }
            bits = (byte)(((bits - 1) & 31) + 1);
            int bitsToRead = bits;
            while (UnconsumedInputBits < bitsToRead)
            {
                if (!ReadFromSource())
                {
                    value = 0;
                    return false;
                }
            }
            buf = FullSpan;
            ref var bufHead = ref MemoryMarshal.GetReference(buf);
            cw = consumedWords;
            var cb = consumedBits;
            if (cw < words)
            {
                if (cb > 0)
                {
                    var n = BitsPerWord - cb;
                    var word = Unsafe.Add(ref bufHead, cw);
                    word = MathI.ZeroHighBitsFromHigh(cb, word);
                    if (bitsToRead < n)
                    {
                        var shift = n - bitsToRead;
                        word >>= shift;
                        var res = (uint)word;
                        cb += bitsToRead;
                        value = res;
                        cw += cb >> 6;
                        cb &= 0x3f;
                        consumedBits = cb;
                        consumedWords = cw;
                        return true;
                    }
                    var result = (uint)word;
                    bitsToRead -= n;
                    cb = 0;
                    cw++;
                    if (bitsToRead > 0)
                    {
                        word = Unsafe.Add(ref bufHead, cw);
                        var shift = BitsPerWord - bitsToRead;
                        var v = result << bitsToRead;
                        result = v;
                        word >>= shift;
                        result |= (uint)word;
                        cb = bitsToRead;
                    }
                    value = result;
                    cw += cb >> 6;
                    cb &= 0x3f;
                    consumedBits = cb;
                    consumedWords = cw;
                    return true;
                }
                else
                {
                    var word = Unsafe.Add(ref bufHead, cw);
                    var result = (uint)(word >> -bitsToRead);
                    cb = bitsToRead;
                    value = result;
                    cw += cb >> 6;
                    cb &= 0x3f;
                    consumedBits = cb;
                    consumedWords = cw;
                    return true;
                }
            }
            else
            {
                var word = Unsafe.Add(ref bufHead, cw);
                var result = word << cb;
                cb += bitsToRead;
                value = (uint)(result >> -bitsToRead);
                cw += cb >> 6;
                cb &= 0x3f;
                consumedBits = cb;
                consumedWords = cw;
                return true;
            }
        }

        /// <summary>
        /// Reads the number with specified <paramref name="bits"/>.
        /// </summary>
        /// <param name="bits">The bits to read. must be &lt;=64.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        [Obsolete("Use out parameterized one instead!")]
        public ulong? ReadBitsUInt64(byte bits) => ReadBitsUInt64(bits, out var value) ? value : null;

        /// <summary>
        /// Reads the number with specified <paramref name="bits" />.
        /// </summary>
        /// <param name="bits">The bits to read. must be &lt;=64.</param>
        /// <param name="value">The value read.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public bool ReadBitsUInt64(byte bits, out ulong value)
        {
            var buf = AvailableSpan;
            if (bits > 64) throw new ArgumentOutOfRangeException(nameof(bits), "The bits must be less than 65!");
            if (Capacity * 2 < bits) throw new ArgumentOutOfRangeException(nameof(bits), "");
            var cw = consumedWords;

            if (cw > buf.Length) throw new FlacException("", this);
            if (bits == 0)
            {
                value = 0;
                return false;
            }
            bits = (byte)(((bits - 1) & 63) + 1);
            int bitsToRead = bits;
            while (UnconsumedInputBits < bitsToRead)
            {
                if (!ReadFromSource())
                {
                    value = 0;
                    return false;
                }
            }
            buf = FullSpan;
            ref var bufHead = ref MemoryMarshal.GetReference(buf);
            cw = consumedWords;
            var cb = consumedBits;
            if (cw < words)
            {
                if (cb > 0)
                {
                    var n = BitsPerWord - cb;
                    var word = Unsafe.Add(ref bufHead, cw);
                    word = MathI.ZeroHighBitsFromHigh(cb, word);
                    if (bitsToRead < n)
                    {
                        var shift = n - bitsToRead;
                        word >>= shift;
                        var res = word;
                        cb += bitsToRead;
                        value = res;
                        cw += cb >> 6;
                        cb &= 0x3f;
                        consumedBits = cb;
                        consumedWords = cw;
                        return true;
                    }
                    var result = word;
                    bitsToRead -= n;
                    cb = 0;
                    cw++;
                    if (bitsToRead > 0)
                    {
                        word = Unsafe.Add(ref bufHead, cw);
                        var shift = BitsPerWord - bitsToRead;
                        result <<= bitsToRead;
                        word >>= shift;
                        result |= word;
                        cb = bitsToRead;
                    }
                    value = result;
                    cw += cb >> 6;
                    cb &= 0x3f;
                    consumedBits = cb;
                    consumedWords = cw;
                    return true;
                }
                else
                {
                    var word = Unsafe.Add(ref bufHead, cw);
                    var result = word >> -bitsToRead;
                    cb = bitsToRead;
                    value = result;
                    cw += cb >> 6;
                    cb &= 0x3f;
                    consumedBits = cb;
                    consumedWords = cw;
                    return true;
                }
            }
            else
            {
                var v = Unsafe.Add(ref bufHead, cw);
                var result = (v << cb) >> -bitsToRead;
                cb += bitsToRead;
                value = result;
                cw += cb >> 6;
                cb &= 0x3f;
                consumedBits = cb;
                consumedWords = cw;
                return true;
            }
        }

        /// <summary>
        /// Reads a byte from this <see cref="FlacBitReader"/>.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public bool ReadByte(out byte data) => Read8Bits(out data);

        /// <summary>
        /// Reads the rice code from this <see cref="FlacBitReader"/>.
        /// </summary>
        /// <param name="parameter">The Rice parameter.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public int? ReadRiceCode(int parameter)
        {
            //As of the libFLAC's code, the Rice coding used in FLAC is:
            //0-continuing unary code of quotient,
            //end 1,
            //remainder,
            //flag bit to calculate NOT of result value
            //The operation -(value) - 1 is equivalent to ~value.
            var result = ReadUnaryUnsigned(out var g);
            if (!result) return null;
            if (!ReadBitsUInt32((byte)parameter, out var ngg)) return null;
            var gg = ngg;
            gg |= g << parameter;
            gg = (gg >> 1) ^ (uint)-(int)(gg & 1);
            return (int)gg;
        }

        /// <summary>
        /// Reads the multiple rice code from this <see cref="FlacBitReader"/>.
        /// </summary>
        /// <param name="span">The span to write the data to.</param>
        /// <param name="parameter">The Rice parameter.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public bool ReadRiceCodes(Span<int> span, int parameter)
        {
            unsafe
            {
                uint lsbs, msbs, x, y;
                int cwords, twords;
                int ucbits;
                ulong b;
                ref var val = ref MemoryMarshal.GetReference(span);
                var end = span.Length;

                if (parameter == 0)
                {
                    for (var i = 0; i < end; i++)
                    {
                        if (!ReadUnaryUnsigned(out msbs)) return false;
                        Unsafe.Add(ref val, i) = (int)(msbs >> 1) ^ -(int)(msbs & 1);
                    }
                    return true;
                }
                else
                {
                    cwords = consumedWords;
                    twords = words;
                    var Q = cwords >= twords;
                    nint i = 0;
                    nint len = end;
                    ucbits = BitsPerWord - consumedBits;
                    ref var fsp = ref MemoryMarshal.GetReference(FullSpan);
                    b = Unsafe.Add(ref fsp, cwords) << consumedBits;
                    var shift = BitsPerWord - parameter;
                    x = 0;
                    lsbs = msbs = 0;
                    while (i < len)
                    {
#pragma warning disable S907 // "goto" statement should not be used
                        if (Q)
                        {
                            Q = false;
                            x = 0;
                            goto ProcessTail;
                        }
                        x = y = (uint)MathI.LeadingZeroCount(b);
                        if (x == BitsPerWord)
                        {
                            x = (uint)ucbits;
                            do
                            {
                                cwords++;
                                if (cwords >= twords)
                                    goto IncompleteMsbs;
                                b = Unsafe.Add(ref fsp, cwords);
                                y = (uint)MathI.LeadingZeroCount(b);
                                x += y;
                            } while (y == BitsPerWord);
                        }
                        b <<= (int)y;
                        b <<= 1;
                        ucbits = (int)(((uint)ucbits - x - 1) % BitsPerWord);
                        msbs = x;

                        x = (uint)(b >> shift);
                        if (parameter <= ucbits)
                        {
                            ucbits -= parameter;
                            b <<= parameter;
                        }
                        else
                        {
                            cwords++;
                            if (cwords >= twords)
                                goto IncompleteLsbs;
                            b = Unsafe.Add(ref fsp, cwords);
                            ucbits += shift;
                            x |= (uint)(b >> ucbits);
                            b <<= BitsPerWord - ucbits;
                        }
                        lsbs = x;

                        x = (msbs << parameter) | lsbs;
                        var value = (int)(x >> 1) ^ -(int)(x & 1);
                        Unsafe.Add(ref val, i) = value;
                        i++;
                        continue;

                    ProcessTail: //process_tail
                        goto ProcessMsbs;
                    IncompleteMsbs: //incomplete_msbs
                        consumedBits = 0;
                        consumedWords = cwords;
                    ProcessMsbs:
                        if (!ReadUnaryUnsigned(out msbs)) return false;
                        msbs += x;
                        ucbits = 0;
                        x = 0;
                        goto ProcessLsbs;
                    IncompleteLsbs:
                        consumedBits = 0;
                        consumedWords = cwords;
                    ProcessLsbs:
                        if (!ReadBitsUInt32((byte)(parameter - ucbits), out var j)) return false;
                        lsbs = x;
                        lsbs |= j;
                        x = (msbs << parameter) | lsbs;
                        Unsafe.Add(ref val, i) = (int)(x >> 1) ^ -(int)(x & 1);
                        x = 0;
                        cwords = consumedWords;
                        twords = words;
                        ucbits = BitsPerWord - consumedBits;
                        b = cwords < Capacity ? Unsafe.Add(ref fsp, cwords) << consumedBits : 0;
                        i++;
                        if (cwords >= twords && i < len) goto ProcessTail;
#pragma warning restore S907 // "goto" statement should not be used
                    }

                    if (ucbits == 0 && cwords < twords)
                    {
                        cwords++;
                        ucbits = BitsPerWord;
                    }

                    consumedBits = BitsPerWord - ucbits;
                    consumedWords = cwords;

                    return true;
                }
            }
        }

        /// <summary>
        /// Reads the <see cref="uint"/> value in little-endian.
        /// </summary>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public uint? ReadUInt32LittleEndian() => !ReadBitsUInt32(32, out var v) ? null : BinaryPrimitives.ReverseEndianness(v);

        /// <summary>
        /// Reads the unary code without sign bit.
        /// </summary>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public bool ReadUnaryUnsigned(out uint value)
        {
            int i;
            value = 0;
            var aspan = AvailableSpan;
            while (true)
            {
                var cw = consumedWords;
                while (cw < aspan.Length)
                {
                    var q = consumedBits < BitsPerWord ? aspan[cw] << consumedBits : 0;
                    if (q > 0)
                    {
                        i = MathI.LeadingZeroCount(q);
                        value += (uint)i;
                        i++;
                        consumedBits += i;
                        if (consumedBits >= BitsPerWord)
                        {
                            cw++;
                            consumedBits = 0;
                        }
                        consumedWords = cw;
                        return true;
                    }
                    else
                    {
                        value += (uint)(BitsPerWord - consumedBits);
                        cw++;
                        consumedBits = 0;
                    }
                }
                consumedWords = cw;
                if (bytesOfIncompleteWord * 8 > consumedBits)
                {
                    var end = bytesOfIncompleteWord * 8;
                    var b = (FullSpan[consumedWords] & (~0ul << (BitsPerWord - end))) << consumedBits;
                    if (b > 0)
                    {
                        i = MathI.LeadingZeroCount(b);
                        value += (uint)i;
                        i++;
                        consumedBits += i;
                        return true;
                    }
                    else
                    {
                        value += (uint)(end - consumedBits);
                        consumedBits = end;
                    }
                }
                if (!ReadFromSource()) return false;
                aspan = AvailableSpan;
            }
        }

        /// <summary>
        /// Reads the UTF8 encoded number.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="rawData"></param>
        /// <param name="bytesRead"></param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        public bool ReadUtf8UInt32(out uint value, Span<byte> rawData, out int bytesRead)
        {
            value = 0;
            bytesRead = 0;
            if (!ReadByte(out var first)) return false;
            if (rawData.Length > 1) rawData[0] = first;
            bytesRead = 1;
            var locnt = MathI.LeadingZeroCount(~((uint)first << 24));
            switch (locnt)
            {
                case 0:
                    value = first;
                    return true;
                case < 8:
                    var bytesToRead = locnt - 1;
                    var res = MathI.ZeroHighBits((byte)(7 - locnt), first) << (6 * bytesToRead);
                    Span<byte> q = stackalloc byte[bytesToRead];
                    if (ReadBytes(q).Length < q.Length) return false;
                    for (var i = 0; i < q.Length; i++)
                    {
                        res |= ((uint)q[i] & 0x3f) << (6 * (bytesToRead - i - 1));
                    }
                    if (rawData.Length - 1 >= q.Length)
                    {
                        q.CopyTo(rawData.Slice(1));
                        bytesRead += q.Length;
                    }
                    value = res;
                    return true;
                default:
                    value = ~0u;
                    return true;
            }
        }

        /// <summary>
        /// Reads the UTF8 encoded number.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="rawData"></param>
        /// <param name="bytesRead"></param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        public bool ReadUtf8UInt64(out ulong value, Span<byte> rawData, out int bytesRead)
        {
            value = 0;
            bytesRead = 0;
            if (!ReadByte(out var first)) return false;
            if (rawData.Length > 1) rawData[0] = first;
            bytesRead = 1;
            var locnt = MathI.LeadingZeroCount(~((uint)first << 24));
            switch (locnt)
            {
                case 0:
                    value = first;
                    return true;
                case < 8:
                    var bytesToRead = locnt - 1;
                    var res = (ulong)MathI.ExtractBitField(first, 0, (byte)(7 - locnt)) << (6 * bytesToRead);
                    Span<byte> q = stackalloc byte[bytesToRead];
                    if (ReadBytes(q).Length < q.Length) return false;
                    for (var i = 0; i < q.Length; i++)
                    {
                        res |= ((ulong)q[i] & 0x3f) << (6 * (bytesToRead - i - 1));
                    }
                    if (rawData.Length - 1 >= q.Length)
                    {
                        q.CopyTo(rawData.Slice(1));
                        bytesRead += q.Length;
                    }
                    value = res;
                    return true;
                default:
                    value = ~0u;
                    return true;
            }
        }

        /// <summary>
        /// Reads the zero padding.
        /// </summary>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public bool ReadZeroPadding()
        {
            if (!IsByteAligned)
            {
                if (!ReadBitsUInt64((byte)RemainingUnalignedBits, out _) || !IsByteAligned) return false;
            }
            return true;
        }

        /// <summary>
        /// Skips the bits without calculating CRC.
        /// </summary>
        /// <param name="bits">The bits.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public bool SkipBitsWithoutCrc(uint bits)
        {
            if (bits > 0)
            {
                var n = (uint)consumedBits & 7u;
                uint m;
                if (n != 0)
                {
                    m = Math.Min(8u - n, bits);
                    if (!ReadBitsUInt32((byte)m, out _)) return false;
                    bits -= m;
                }
                m = bits / 8u;
                if (m > 0)
                {
                    if (!SkipByteBlockWithoutCrc(m)) return false;
                    bits %= 8;
                }
                if (bits > 0 && !ReadBitsUInt32((byte)bits, out _))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Updates the CRC16.
        /// </summary>
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        public void UpdateCrc16()
        {
            UpdateCrcByBlock();
            if (consumedBits > 0)
            {
                var tail = FullSpan[consumedWords];
                for (; crcAlignBits < consumedBits; crcAlignBits += 8)
                {
                    crc16 *= (byte)(tail >> (BitsPerWord - 8 - crcAlignBits));
                }
            }
        }

        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        internal bool ReadFromSource()
        {
            var q = FullSpan;
            if (consumedWords > 0)
            {
                UpdateCrcByBlock();
                var start = q.Slice(consumedWords, words - consumedWords + (bytesOfIncompleteWord > 0 ? 1 : 0));
                start.CopyTo(q);
                words -= consumedWords;
                consumedWords = 0;
            }
            var bytes = (q.Length - words) * BytesPerWord - bytesOfIncompleteWord;
            if (bytes == 0) return false;   //no space left
            var target = MemoryMarshal.AsBytes(q).Slice(words * BytesPerWord + bytesOfIncompleteWord);
            var preswap = q[words];
            if (BitConverter.IsLittleEndian && bytesOfIncompleteWord > 0)
            {
                q[words] = BinaryPrimitives.ReverseEndianness(preswap);
            }
            var rr = Source.Read(target);
            if (rr.IsEndOfStream)
            {
                isEndOfStream = true;
            }
            if (rr.HasNoData)
            {
                q[words] = preswap;
                return false;
            }

            var size = (bytesOfIncompleteWord + rr.Length + (BytesPerWord - 1)) / BytesPerWord;
            MemoryMarshal.Cast<byte, ulong>(MemoryMarshal.AsBytes(q).Slice(words * BytesPerWord)).SliceWhile(size).ReverseEndianness();
            var d = words * BytesPerWord + bytesOfIncompleteWord + rr.Length;
            words = d / BytesPerWord;
            bytesOfIncompleteWord = d % BytesPerWord;
            return true;
        }

        /// <summary>
        /// Skips the byte block without calculating CRC.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal bool SkipByteBlockWithoutCrc(uint values)
        {
            if (!IsByteAligned) throw new FlacException("This is a bug!", this);
            while (values > 0 && consumedBits > 0)
            {
                if (!Read8Bits(out _)) return false;
                values--;
            }
            if (values == 0) return true;
            while (values >= BytesPerWord)
            {
                if (consumedWords < words)
                {
                    consumedWords++;
                    values -= BytesPerWord;
                }
                else if (!ReadFromSource())
                {
                    return false;
                }
            }
            while (values > 0)
            {
                if (!Read8Bits(out _)) return false;
                values--;
            }
            return true;
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal void UpdateCrcByBlock()
        {
            var q = FullSpan;
            if (consumedWords > crcOffset && crcAlignBits > 0)
            {
                UpdateCrcByWord(q[crcOffset++]);
            }
            if (consumedWords > crcOffset)
            {
                crc16 *= q.Slice(crcOffset, consumedWords - crcOffset);
            }
            crcOffset = 0;
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal void UpdateCrcByWord(ulong word)
        {
            var crc = crc16;
            var cAl = crcAlignBits;
            for (; cAl < BitsPerWord; cAl += 8)
            {
                var shift = BitsPerWord - 8 - cAl;
                crc *= (byte)(shift < BitsPerWord ? (word >> shift) : 0);
            }
            crc16 = crc;
            crcAlignBits = 0;
        }

        #region ReadNBits

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        [Obsolete("Use out parameterized one instead!")]
        private uint? Read8Bits() => !Read8Bits(out var r) ? null : r;

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private bool Read8Bits(out byte value)
        {
            var buf = AvailableSpan;
            if (Capacity * 2 < 8) throw new FlacException("", this);
            var cw = consumedWords;
            if (cw > buf.Length) throw new FlacException("", this);
            var bitsToRead = 8;
            while (UnconsumedInputBits < bitsToRead)
            {
                if (!ReadFromSource())
                {
                    value = 0;
                    return false;
                }
            }
            buf = FullSpan;
            ref var bufHead = ref MemoryMarshal.GetReference(buf);
            cw = consumedWords;
            var cb = consumedBits;
            if (cw < words)
            {
                if (cb > 0)
                {
                    var n = BitsPerWord - cb;
                    var word = Unsafe.Add(ref bufHead, cw);
                    word = MathI.ZeroHighBitsFromHigh(cb, word);
                    if (bitsToRead < n)
                    {
                        var shift = n - bitsToRead;
                        word >>= shift;
                        var res = MathI.ZeroIfFalse(shift < BitsPerWord, (uint)word);
                        cb += bitsToRead;
                        value = (byte)res;
                        cw += cb >> 6;
                        cb &= 0x3f;
                        consumedBits = cb;
                        consumedWords = cw;
                        return true;
                    }
                    var result = (uint)word;
                    bitsToRead -= n;
                    cb = 0;
                    cw++;
                    if (bitsToRead > 0)
                    {
                        word = Unsafe.Add(ref bufHead, cw);
                        var shift = BitsPerWord - bitsToRead;
                        result = bitsToRead < 32 ? result << bitsToRead : 0;
                        word >>= shift;
                        result |= MathI.ZeroIfFalse(shift < BitsPerWord, (uint)word);
                        cb = bitsToRead;
                    }
                    value = (byte)result;
                    cw += cb >> 6;
                    cb &= 0x3f;
                    consumedBits = cb;
                    consumedWords = cw;
                    return true;
                }
                else
                {
                    var word = Unsafe.Add(ref bufHead, cw);
                    var result = (uint)(word >> (BitsPerWord - bitsToRead));
                    cb = bitsToRead;
                    value = (byte)result;
                    cw += cb >> 6;
                    cb &= 0x3f;
                    consumedBits = cb;
                    consumedWords = cw;
                    return true;
                }
            }
            else
            {
                if (cb + bitsToRead > bytesOfIncompleteWord * 8) throw new FlacException("This is a bug!", this);
                var result = (uint)((Unsafe.Add(ref bufHead, cw) & (~0ul >> cb)) >> (BitsPerWord - cb - bitsToRead));
                cb += bitsToRead;
                value = (byte)result;
                cw += cb >> 6;
                cb &= 0x3f;
                consumedBits = cb;
                consumedWords = cw;
                return true;
            }
        }

        #endregion ReadNBits

        #region Debug Functions

        /// <summary>
        /// Dumps this instance.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string Dump()
        {
            if (buffer is null) return "";
            var sb = new StringBuilder();
            var buf = FullSpan;
            _ = sb.AppendLine();
            for (var i = 0; i < buf.Length; i++)
            {
                _ = sb.Append($"{i * 8:X8} ");
                _ = i == consumedWords ? sb.Append("# ") : sb.Append("  ");
                var value = buf[i];
                DumpWord(sb, i, value);
                _ = sb.AppendLine();
            }
            _ = sb.AppendLine();
            return sb.ToString();
        }

        private void DumpWord(StringBuilder sb, int i, ulong value)
        {
            var mask = 0x8000_0000_0000_0000ul;
            var j = 0;
            while (mask > 0)
            {
                switch (((value & mask) > 0, i == consumedWords && consumedBits == j))
                {
                    case (true, true):
                        _ = sb.Append('I');
                        break;
                    case (false, true):
                        _ = sb.Append('O');
                        break;
                    case (true, false):
                        _ = sb.Append('1');
                        break;
                    case (false, false):
                        _ = sb.Append('0');
                        break;
                }
                if (j % 8 == 7 && j < 63)
                {
                    _ = sb.Append('_');
                }
                mask >>= 1;
                j++;
            }
        }

        #endregion Debug Functions
    }

    #region IDisposable

    public sealed partial class FlacBitReader : IDisposable
    {
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //
                }
                buffer = null;
                source = null;
                disposedValue = true;
            }
        }
    }

    #endregion IDisposable
}
