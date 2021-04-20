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
        /// Gets or resets the current CRC16.
        /// </summary>
        /// <value>
        /// The CRC16.
        /// </value>
        public FlacCrc16 Crc16
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get
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
                return crc16;
            }
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set
            {
                crc16 = value;
                crcOffset = consumedWords;
                crcAlignBits = consumedBits;
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

        /// <summary>
        /// Gets the source.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        /// <exception cref="ObjectDisposedException">FlacBitReader</exception>
        public IReadableDataSource<byte> Source => source ?? throw new ObjectDisposedException(nameof(FlacBitReader));

        private ulong[] Buffer
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => buffer ?? throw new ObjectDisposedException(nameof(FlacBitReader));
        }

        int Capacity => Buffer.Length;

        private Span<ulong> FullSpan
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Buffer;
        }

        private Span<ulong> AvailableSpan
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => FullSpan.SliceWhile(words);
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
        /// Reads the number with specified <paramref name="bits"/>.
        /// </summary>
        /// <param name="bits">The bits to read. must be &lt;=64.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public ulong? ReadBitsUInt64(byte bits)
        {
            if (bits <= 32) return ReadBitsUInt32(bits);
            uint? v = ReadBitsUInt32((byte)(bits - 32));
            if (!v.HasValue) return null;
            uint? w = ReadBitsUInt32(32);
            return !w.HasValue ? null : ((ulong)v.Value << 32) | w.Value;
        }

        /// <summary>
        /// Reads the number with specified <paramref name="bits"/>.
        /// </summary>
        /// <param name="bits">The bits to read. must be &lt;=32.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public uint? ReadBitsUInt32(byte bits)
        {
            var buf = AvailableSpan;
            if (bits > 32) throw new ArgumentOutOfRangeException(nameof(bits), "The bits must be less than 33!");
            if (Capacity * 2 < bits) throw new ArgumentOutOfRangeException(nameof(bits), "");
            if (consumedWords > buf.Length) throw new FlacException("");
            if (bits == 0) return 0;
            bits %= 33;
            int bitsToRead = bits;
            while (UnconsumedInputBits < bitsToRead)
            {
                if (!ReadFromSource()) return null;
            }
            buf = AvailableSpan;
            if (consumedWords < buf.Length)
            {
                if (consumedBits > 0)
                {
                    var n = BitsPerWord - consumedBits;
                    var word = buf[consumedWords];
                    var mask = consumedBits < BitsPerWord ? ~0ul >> consumedBits : 0;
                    if (bitsToRead < n)
                    {
                        var shift = n - bitsToRead;
                        var res = shift < BitsPerWord ? (uint)((word & mask) >> shift) : 0u;
                        consumedBits += bitsToRead;
                        return res;
                    }
                    var result = (uint)(word & mask);
                    bitsToRead -= n;
                    consumedBits = 0;
                    consumedWords++;
                    if (bitsToRead > 0)
                    {
                        var shift = BitsPerWord - bitsToRead;
                        result = bitsToRead < 32 ? result << shift : 0;
                        result |= shift < BitsPerWord ? (uint)((buf[consumedWords] & mask) >> shift) : 0u;
                        consumedBits = bitsToRead;
                    }
                    return result;
                }
                else
                {
                    var word = buf[consumedWords];
                    if (bitsToRead < BitsPerWord)
                    {
                        var result = (uint)(word >> (BitsPerWord - bitsToRead));
                        consumedBits = bitsToRead;
                        return result;
                    }
                    consumedWords++;
                    return (uint)word;
                }
            }
            else
            {
                var s2 = FullSpan;
                if (consumedBits > 0)
                {
                    if (consumedBits + bitsToRead > bytesOfIncompleteWord * 8) throw new FlacException("This is a bug!");
                    var result = (uint)((s2[consumedWords] & (~0ul >> consumedBits)) >> (BitsPerWord - consumedBits - bitsToRead));
                    consumedBits += bitsToRead;
                    return result;
                }
                else
                {
                    var result = (uint)(s2[consumedWords] >> (BitsPerWord - bitsToRead));
                    consumedBits += bitsToRead;
                    return result;
                }
            }
        }

        /// <summary>
        ///  Reads the number with specified <paramref name="bits"/>.
        /// </summary>
        /// <param name="bits">The bits to read. must be &lt;=32.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public int? ReadBitsInt32(byte bits)
        {
            if (bits < 1) return null;
            var g = ReadBitsUInt32(bits);
            if (!g.HasValue) return null;
            var gg = g.Value;
            gg <<= 32 - bits;
            return (int)gg >> (32 - bits);
        }

        /// <summary>
        /// Reads the <see cref="uint"/> value in little-endian.
        /// </summary>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public uint? ReadUInt32LittleEndian()
        {
            var v = ReadBitsUInt32(32);
            return !v.HasValue ? null : BinaryPrimitives.ReverseEndianness(v.Value);
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
                    if (ReadBitsUInt32((byte)m) is null) return false;
                    bits -= m;
                }
                m = bits / 8u;
                if (m > 0)
                {
                    if (!SkipByteBlockWithoutCrc(m)) return false;
                    bits %= 8;
                }
                if (bits > 0 && ReadBitsUInt32((byte)bits) is null)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Reads the data to the specified <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>The length of the data written.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public ReadResult ReadAligned(Span<byte> buffer)
        {
            var bhead = buffer;
            for (int i = 0; i < bhead.Length; i++)
            {
                if (!bhead.IsEmpty && consumedBits > 0)
                {
                    var r = ReadBitsUInt32(8);
                    if (!r.HasValue) return ReadResult.WaitingForSource;
                    bhead[i] = (byte)r.Value;
                }
                else
                {
                    bhead = bhead.Slice(i);
                    break;
                }
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
            for (int i = 0; i < bhead.Length; i++)
            {
                var r = ReadBitsUInt32(8);
                if (!r.HasValue) return ReadResult.WaitingForSource;
                bhead[i] = (byte)r.Value;
            }
            return buffer.Length;
        }

        /// <summary>
        /// Skips the byte block without calculating CRC.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal bool SkipByteBlockWithoutCrc(uint values)
        {
            if (!IsByteAligned) throw new FlacException("This is a bug!");
            while (values > 0 && consumedBits > 0)
            {
                if (ReadBitsUInt32(8) is null) return false;
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
                if (ReadBitsUInt32(8) is null) return false;
                values--;
            }
            return true;
        }

        /// <summary>
        /// Reads an 1-bit value from this <see cref="FlacBitReader"/>.
        /// </summary>
        /// <param name="doCrc">if set to <c>true</c> this instance does CRC.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public bool? ReadBool(bool doCrc = true)
        {
            var h = ReadBitsUInt32(1);
            return h.HasValue ? h.Value > 0 : null;
        }

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
            var ngg = ReadBitsUInt32((byte)parameter);
            if (ngg is null) return null;
            var gg = ngg.Value;
            gg |= g << parameter;
            gg = (gg >> 1) ^ (uint)-(int)(gg & 1);
            return (int)gg;
        }

        /// <summary>
        /// Reads the UTF8 encoded number.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="rawData"></param>
        /// <returns></returns>
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
                    var res = MathI.ExtractBitField(first, 0, (byte)(7 - locnt)) << (6 * bytesToRead);
                    Span<byte> q = stackalloc byte[bytesToRead];
                    if (ReadAligned(q).Length < q.Length) return false;
                    for (int i = 0; i < q.Length; i++)
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
        /// <returns></returns>
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
                    if (ReadAligned(q).Length < q.Length) return false;
                    for (int i = 0; i < q.Length; i++)
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
                int cwords, words;
                int ucbits;
                ulong b;
                ref var val = ref MemoryMarshal.GetReference(span);
                var end = span.Length;

                if (parameter == 0)
                {
                    for (int i = 0; i < end; i++)
                    {
                        if (!ReadUnaryUnsigned(out msbs)) return false;
                        Unsafe.Add(ref val, i) = (int)(msbs >> 1) ^ -(int)(msbs & 1);
                    }
                    return true;
                }
                else
                {
                    cwords = consumedWords;
                    words = this.words;
                    bool Q = cwords >= words;
                    var i = IntPtr.Zero;
                    var len = ((IntPtr)end).ToPointer();
                    ucbits = BitsPerWord - consumedBits;
                    ref var fsp = ref MemoryMarshal.GetReference(FullSpan);
                    b = Unsafe.Add(ref fsp, cwords) << consumedBits;
                    int shift = BitsPerWord - parameter;
                    x = 0;
                    msbs = 0;
                    for (; i.ToPointer() < len; i += 1)
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
                                if (cwords >= words)
                                    goto IncompleteMsbs;
                                b = Unsafe.Add(ref fsp, cwords);
                                y = (uint)MathI.LeadingZeroCount(b);
                                x += y;
                            } while (y == BitsPerWord);
                        }
                        b <<= (int)y;
                        b <<= 1;
                        ucbits = (ucbits - (int)x - 1) % BitsPerWord;
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
                            if (cwords >= words)
                                goto IncompleteLsbs;
                            b = Unsafe.Add(ref fsp, cwords);
                            ucbits += shift;
                            x |= (uint)(b >> ucbits);
                            b <<= BitsPerWord - ucbits;
                        }
                        lsbs = x;

                        x = (msbs << parameter) | lsbs;
                        Unsafe.Add(ref val, i) = (int)(x >> 1) ^ -(int)(x & 1);
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
                        var j = ReadBitsUInt32((byte)(parameter - ucbits));
                        if (!j.HasValue) return false;
                        lsbs = x;
                        lsbs |= j.Value;
                        x = (msbs << parameter) | lsbs;
                        Unsafe.Add(ref val, i) = (int)(x >> 1) ^ -(int)(x & 1);
                        i += 1;
                        cwords = consumedWords;
                        words = this.words;
                        ucbits = BitsPerWord - consumedBits;
                        b = cwords < Capacity ? Unsafe.Add(ref fsp, cwords) << consumedBits : 0;
                        if (cwords >= words && i.ToPointer() < len) goto ProcessTail;
#pragma warning restore S907 // "goto" statement should not be used
                    }
                    return true;
                }
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
                var q = ReadBitsUInt64((byte)RemainingUnalignedBits);
                if (q is null || !IsByteAligned) return false;
            }
            return true;
        }

        /// <summary>
        /// Reads the unary code with sign bit.
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
                    ulong q = consumedBits < BitsPerWord ? aspan[cw] << consumedBits : 0;
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
                            consumedWords = cw;
                        }
                        return true;
                    }
                    else
                    {
                        value = (uint)(BitsPerWord - consumedBits);
                        cw++;
                        consumedBits = 0;
                    }
                }
                if (bytesOfIncompleteWord * 8 > consumedBits)
                {
                    int end = bytesOfIncompleteWord * 8;
                    ulong b = (FullSpan[consumedWords] & (~0ul << (BitsPerWord - end))) << consumedBits;
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
            }
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal bool ReadFromSource()
        {
            var q = FullSpan;
            if (consumedWords > 0)
            {
                UpdateCrcByBlock();
                var start = q.Slice(consumedWords, words - consumedWords + (bytesOfIncompleteWord > 0 ? 1 : 0));
                start.CopyTo(q);
            }
            var bytes = (q.Length - words) * BytesPerWord - bytesOfIncompleteWord;
            if (bytes == 0) return false;   //no space left
            var target = MemoryMarshal.AsBytes(q).Slice(words * BytesPerWord + bytesOfIncompleteWord);
            if (BitConverter.IsLittleEndian && bytesOfIncompleteWord > 0)
            {
                q[words] = BinaryPrimitives.ReverseEndianness(q[words]);
            }
            var rr = Source.Read(target);
            if (rr.IsEndOfStream)
            {
                isEndOfStream = true;
            }
            if (rr.HasNoData) return false;
            var size = (bytesOfIncompleteWord + rr.Length + (BytesPerWord - 1)) / BytesPerWord;
            MemoryMarshal.Cast<byte, ulong>(MemoryMarshal.AsBytes(q).Slice(words * BytesPerWord)).SliceWhile(size).ReverseEndianness();
            var d = words * BytesPerWord + bytesOfIncompleteWord + rr.Length;
            words = d / BytesPerWord;
            bytesOfIncompleteWord = d % BytesPerWord;
            return true;
        }

        /// <summary>
        /// Reads a byte from this <see cref="FlacBitReader"/>.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool ReadByte(out byte data)
        {
            var h = ReadBitsUInt32(8);
            data = (byte)h.GetValueOrDefault();
            return h.HasValue;
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
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal void UpdateCrcByWord(ulong word)
        {
            var crc = crc16;
            var cAl = crcAlignBits;
            for (; cAl < BitsPerWord; cAl += 8)
            {
                int shift = BitsPerWord - 8 - cAl;
                crc *= (byte)(shift < BitsPerWord ? (word >> shift) : 0);
            }
            crc16 = crc;
            crcAlignBits = 0;
        }
    }

    #region IDisposable

    sealed partial class FlacBitReader : IDisposable
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
