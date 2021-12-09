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
using System.Text;
using System.Threading.Tasks;

using Shamisen.Codecs.Flac.Parsing;
using Shamisen.Data;

namespace Shamisen.Codecs.Flac.SubFrames
{
    /// <summary>
    /// Represents a verbatim FLAC sub-frame.
    /// </summary>
    public sealed class FlacVerbatimSubFrame : IFlacSubFrame
    {
        /// <summary>
        /// Gets the number of wasted LSBs.
        /// </summary>
        public int WastedBits { get; private set; }

        /// <summary>
        /// Gets the type of the sub-frame.
        /// </summary>
        public byte SubFrameType => 1;

        private PooledArray<int> values;
        private bool disposedValue;

        /// <summary>
        /// Initializes a new instance of <see cref="FlacVerbatimSubFrame"/>.
        /// </summary>
        public FlacVerbatimSubFrame(Span<int> values, int wastedBits)
        {
            this.values = new(values.Length);
            values.CopyTo(this.values.Array.AsSpan());
            WastedBits = wastedBits;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="FlacVerbatimSubFrame"/>.
        /// </summary>
        public FlacVerbatimSubFrame(FlacBitReader bitReader, int blockSize, int wastedBits, byte bitsPerSample)
        {
            values = new(blockSize);
            var vs = values.Span;
            for (var i = 0; i < vs.Length; i++)
            {
                if (!bitReader.ReadBitsInt32(bitsPerSample, out var result)) throw new FlacException("Invalid FLAC Stream!", bitReader);
                vs[i] = result << wastedBits;
            }
        }

        /// <summary>
        /// Reads the data to the specified <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>The length of the data written.</returns>
        public ReadResult Read(Span<int> buffer)
        {
            if (values.Length > buffer.Length) return ReadResult.EndOfStream;
            values.Span.CopyTo(buffer);
            return values.Length;
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //
                }
                values.Dispose();
                disposedValue = true;
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="FlacVerbatimSubFrame"/> class.
        /// </summary>
        ~FlacVerbatimSubFrame()
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
