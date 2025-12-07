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

namespace Shamisen.Codecs.Flac
{
    /// <summary>
    /// Represents some options for <see cref="FlacParser"/>
    /// </summary>
    /// <param name="ParseCueSheet"> Gets a value indicating whether to parse cue sheet. </param>
    /// <param name="ParseVorbisComment"> Gets a value indicating whether to parse vorbis comment. </param>
    /// <param name="ParsePictures"> Gets a value indicating whether to parse picture metadata <b>EXCLUDING ACTUAL PICTURE</b>. </param>
    /// <param name="PreserveApplication"> Gets a value indicating whether to preserve application metadata. </param>
    /// <param name="PreservePadding"> Gets a value indicating whether to preserve padding. </param>
    /// <param name="PreserveUnusedMetadata"> Gets a value indicating whether to preserve unused metadata excluding padding and invalid metadata. </param>
    public readonly record struct FlacParserOptions(bool ParseCueSheet, bool ParseVorbisComment, bool ParsePictures, bool PreserveApplication, bool PreservePadding, bool PreserveUnusedMetadata)
    {
        /// <summary>
        /// Gets a value indicating whether an exception should be thrown when stream information is unavailable.
        /// </summary>
        /// <remarks>Set this property to <see langword="true"/> to enforce strict error handling when
        /// stream details are missing. When <see langword="false"/>, the absence of stream information will not result
        /// in an exception.</remarks>
        public bool ThrowWithoutStreamInfo { get; init; } = false;

        /// <summary>
        /// Gets a value indicating whether the first frame of the input should be parsed during initialization.
        /// </summary>
        public bool ParseFirstFrame { get; init; } = true;
    }
}
