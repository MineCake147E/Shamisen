﻿#region License
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

using DivideSharp;

using Shamisen.Codecs.Flac.Metadata;
using Shamisen.Codecs.Flac.Parsing;
using Shamisen.Data;
using Shamisen.Data.Binary;
using Shamisen.Formats;

namespace Shamisen.Codecs.Flac
{
    /// <summary>
    /// Parses ".flac" files from <see cref="IReadableDataSource{TSample}"/>.
    /// https://xiph.org/flac/format.html
    /// </summary>
    /// <seealso cref="IWaveSource" />
    public sealed class FlacParser : IReadableAudioSource<int, Int32RangedLinearPcmSampleFormat>
    {
        private readonly FlacStreamInfoBlock streamInfoBlock;
        private Int32Divisor channelsDivisor;
        private FlacFrameParser? currentFrame;
        private bool disposedValue;
        private Memory<FlacSeekPoint> seekPoints;

        /// <summary>
        /// Gets the cue sheet.
        /// </summary>
        /// <value>
        /// The cue sheet.
        /// </value>
        public FlacCueSheet? CueSheet { get; }

        /// <summary>
        /// Gets the vorbis comment.
        /// </summary>
        /// <value>
        /// The vorbis comment.
        /// </value>
        public VorbisComment? Comment { get; }

        /// <summary>
        /// Gets the application metadata.
        /// </summary>
        /// <value>
        /// The application metadata.
        /// </value>
        public ReadOnlyMemory<FlacApplicationMetadata> ApplicationMetadata { get; }

        /// <summary>
        /// Gets the pictures.
        /// </summary>
        /// <value>
        /// The pictures.
        /// </value>
        public ReadOnlyMemory<FlacPicture> Pictures { get; }

        /// <summary>
        /// Gets the unused metadata.
        /// </summary>
        /// <value>
        /// The unused metadata.
        /// </value>
        public ReadOnlyMemory<FlacUnusedMetadata> UnusedMetadata { get; }

        /// <summary>
        /// Gets the format.
        /// </summary>
        /// <value>
        /// The format.
        /// </value>
        public Int32RangedLinearPcmSampleFormat Format { get; }

        /// <summary>
        /// Gets the remaining length.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        public ulong Length => TotalLength - Position;

        /// <summary>
        /// Gets the position.
        /// </summary>
        /// <value>
        /// The position.
        /// </value>
        public ulong Position { get; private set; }

        /// <summary>
        /// Gets the total length.
        /// </summary>
        /// <value>
        /// The total length.
        /// </value>
        public ulong TotalLength { get; }

        ulong? IAudioSource<int, Int32RangedLinearPcmSampleFormat>.Length => Length;

        ulong? IAudioSource<int, Int32RangedLinearPcmSampleFormat>.Position => Position;

        ISeekSupport? IAudioSource<int, Int32RangedLinearPcmSampleFormat>.SeekSupport { get; }

        ISkipSupport? IAudioSource<int, Int32RangedLinearPcmSampleFormat>.SkipSupport { get; }

        ulong? IAudioSource<int, Int32RangedLinearPcmSampleFormat>.TotalLength => TotalLength;

        private FlacBitReader BitReader { get; }

        private IReadableDataSource<byte> Source { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FlacParser"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="options">The options.</param>
        /// <exception cref="ArgumentNullException">
        /// nameof(source)
        /// or
        /// nameof(source)
        /// or
        /// nameof(source)
        /// </exception>
        public FlacParser(IReadableDataSource<byte> source, FlacParserOptions options = default)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            var fLaC = source.ReadUInt32LittleEndian();
            if (fLaC != BinaryExtensions.ConvertToBigEndian(0x664C_6143))
                throw new ArgumentException("The FLAC Stream is invalid!", nameof(source));
            var streamInfoHeader = ReadMetadataHeader(source);
            if (streamInfoHeader.MetadataBlockType != FlacMetadataBlockType.StreamInfo)
                throw new ArgumentException("The FLAC Stream is invalid!", nameof(source));
            var streamInfo = streamInfoBlock = FlacStreamInfoBlock.ToReadableValue(Read<FlacStreamInfoBlock>(source));
            Format = new(streamInfo.Channels, (int)streamInfo.SampleRate, streamInfo.BitDepth);
            channelsDivisor = new(Format.Channels);
            TotalLength = streamInfo.TotalSamples;
            var currentHeader = streamInfoHeader;
            var appl = new List<FlacApplicationMetadata>();
            var pics = new List<FlacPicture>();
            var unused = new List<FlacUnusedMetadata>();
            while (!currentHeader.IsLastMetadataBlock)
            {
                currentHeader = ReadMetadataHeader(source);
                switch (currentHeader.MetadataBlockType)
                {
                    case FlacMetadataBlockType.Application when options.PreserveApplication:
                        {
                            var h = source.ReadStruct<VectorB4>();
                            var u = new byte[currentHeader.Size - 4];
                            source.ReadAll(u);
                            appl.Add(new FlacApplicationMetadata(h, u));
                        }
                        break;
                    case FlacMetadataBlockType.Picture when options.ParsePictures:
                        {
                            var u = new byte[currentHeader.Size];
                            source.ReadAll(u);
                            pics.Add(FlacPicture.ReadFrom(u));
                        }
                        break;
                    case FlacMetadataBlockType.Padding when options.PreservePadding:
                        {
                            var u = new byte[currentHeader.Size];
                            source.ReadAll(u);
                            unused.Add(new FlacUnusedMetadata(currentHeader.MetadataBlockType, u));
                        }
                        break;
                    case FlacMetadataBlockType.VorbisComment when options.ParseVorbisComment && Comment is null:
                        Comment = ReadVorbisComment(currentHeader, source);
                        break;
                    case FlacMetadataBlockType.CueSheet when options.ParseCueSheet && CueSheet is null:
                        CueSheet = ReadCueSheet(currentHeader, source);
                        break;
                    case FlacMetadataBlockType.SeekTable when seekPoints.IsEmpty:
                        ReadSeekTable(currentHeader, source);
                        break;
                    case FlacMetadataBlockType.SeekTable when !seekPoints.IsEmpty:
                    case FlacMetadataBlockType.CueSheet when CueSheet is not null:
                    case FlacMetadataBlockType.VorbisComment when Comment is not null:
                    case FlacMetadataBlockType.StreamInfo:  //StreamInfo already specified
                    case FlacMetadataBlockType.Invalid: //Invalid chunks here
                        throw new ArgumentException("The FLAC Stream is invalid!", nameof(source));
                    default:    //Reserved
                        if (options.PreserveUnusedMetadata && currentHeader.MetadataBlockType != FlacMetadataBlockType.Padding)
                        {
                            var u = new byte[currentHeader.Size];
                            source.ReadAll(u);
                            unused.Add(new FlacUnusedMetadata(currentHeader.MetadataBlockType, u));
                        }
                        else
                        {
                            source.SkipWithFallback(currentHeader.Size);
                        }
                        break;
                }
            }
            ApplicationMetadata = appl.ToArray();
            Pictures = pics.ToArray();
            UnusedMetadata = unused.ToArray();
            BitReader = new FlacBitReader(source);
            currentFrame = FindNextFrame() ?? throw new FlacException("The FLAC file has no data!", BitReader);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Reads the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns></returns>
        public ReadResult Read(Span<int> buffer)
        {
            var bb = buffer.SliceAlign(channelsDivisor);
            var bbr = bb;
            if (Length == 0) return ReadResult.EndOfStream;
            while (!bbr.IsEmpty && Length > 0)
            {
                while (currentFrame?.Length is null || currentFrame.Length == 0)
                {
                    var g = FindNextFrame();
                    if (g is null)
                    {
                        Position = TotalLength;
                        return (bb.Length - bbr.Length);
                    }
                    currentFrame = g;
                }
                var rr = currentFrame.Read(bbr);
                while (rr.HasNoData)
                {
                    while (currentFrame?.Length is null || currentFrame.Length == 0)
                    {
                        var g = FindNextFrame();
                        if (g is null)
                        {
                            Position = TotalLength;
                            return (bb.Length - bbr.Length);
                        }
                        currentFrame = g;
                    }
                    rr = currentFrame.Read(bbr);
                }
                Position += (ulong)(rr.Length / channelsDivisor);
                if (rr.Length == bbr.Length)
                    return bb.Length;
                bbr = bbr.Slice(rr.Length);
            }
            return (bb.Length - bbr.Length);
        }

        /// <summary>
        /// Reads the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns></returns>
        public ReadResult Read(Span<byte> buffer)
        {
            var bb = MemoryMarshal.Cast<byte, int>(buffer).SliceAlign(channelsDivisor);
            return Read(bb) * sizeof(int);
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static T Read<T>(IReadableDataSource<byte> source) where T : unmanaged
        {
            Span<byte> buffer = stackalloc byte[Unsafe.SizeOf<T>()];
            source.ReadAll(buffer);
            return MemoryMarshal.Read<T>(buffer);
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static FlacMetadataBlockHeader ReadMetadataHeader(IReadableDataSource<byte> source)
        {
            Span<byte> buffer = stackalloc byte[4];
            source.ReadAll(buffer);
            return MemoryMarshal.Read<FlacMetadataBlockHeader>(buffer);
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //
                }
                BitReader.Dispose();
                disposedValue = true;
            }
        }

        private FlacFrameParser? FindNextFrame()
        {
            var frameParser = FlacFrameParser.ParseNextFrame(BitReader, streamInfoBlock);

            return frameParser;
        }

        private static FlacCueSheet ReadCueSheet(FlacMetadataBlockHeader header, IReadableDataSource<byte> source)
        {
            using var h = new PooledArray<byte>((int)header.Size);
            source.ReadAll(h.Span);
            return FlacCueSheet.ReadFrom(h.Span, out _);
        }

        private static VorbisComment ReadVorbisComment(FlacMetadataBlockHeader header, IReadableDataSource<byte> source)
        {
            using var h = new PooledArray<byte>((int)header.Size);
            source.ReadAll(h.Span);
            return VorbisComment.ReadFrom(h.Span);
        }

        private void ReadSeekTable(FlacMetadataBlockHeader header, IReadableDataSource<byte> source)
        {
            using var h = new PooledArray<byte>((int)header.Size);
            source.ReadAll(h.Span);
            seekPoints = MemoryMarshal.Cast<byte, FlacSeekPoint>(h.Span).ToArray();
        }
    }
}
