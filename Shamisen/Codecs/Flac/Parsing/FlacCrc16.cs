using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Codecs.Flac.Parsing
{
    /// <summary>
    /// Calculates a CRC-16 for FLAC stream. Polynomial: CRC-16-IBM
    /// </summary>
    public readonly partial struct FlacCrc16 : IEquatable<FlacCrc16>
    {
        private readonly ushort state;

        /// <summary>
        /// Initializes a new instance of the <see cref="FlacCrc16"/> struct.
        /// </summary>
        /// <param name="state">The state.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public FlacCrc16(ushort state) => this.state = state;

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
        /// Performs an implicit conversion from <see cref="FlacCrc16"/> to <see cref="UInt16"/>.
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
            for (int i = 0; i < w.Length; i++)
            {
                value *= w[i];
            }
            for (int i = 0; i < rem.Length; i++)
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
        public static FlacCrc16 operator *(FlacCrc16 left, ReadOnlySpan<ulong> right)
        {
            var value = left;
            for (int i = 0; i < right.Length; i++)
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
        public FlacCrc16 GenerateNext(byte value) => new((ushort)(((byte)state << 8) ^ (Table0[value ^ (byte)state])));

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
    }
}
