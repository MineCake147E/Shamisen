using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Shamisen.Data;

namespace Shamisen.Codecs.Flac.Metadata
{
    /// <summary>
    /// Represents a track in FLAC cue-sheet.
    /// </summary>
    public readonly struct CueSheetTrack
    {
        private readonly byte field4;

        /// <summary>
        /// Initializes a new instance of the <see cref="CueSheetTrack"/> struct.
        /// </summary>
        /// <param name="trackOffsetInFrames">The track offset in frames.</param>
        /// <param name="trackNumber">The track number.</param>
        /// <param name="trackISRC">The track ISRC.</param>
        /// <param name="field4">The field4 containing <see cref="IsAudio"/> and <see cref="PreEmphasis"/>.</param>
        /// <param name="trackIndexes">The track indexes.</param>
        public CueSheetTrack(ulong trackOffsetInFrames, byte trackNumber, ReadOnlyMemory<byte> trackISRC, byte field4, ReadOnlyMemory<CueSheetTrackIndex> trackIndexes)
        {
            TrackOffsetInFrames = trackOffsetInFrames;
            TrackNumber = trackNumber;
            TrackISRC = trackISRC;
            this.field4 = field4;
            TrackIndexes = trackIndexes;
        }

        /// <summary>
        /// Gets the track offset in frames.
        /// </summary>
        /// <value>
        /// The track offset in frames.
        /// </value>
        public ulong TrackOffsetInFrames { get; }

        /// <summary>
        /// Gets the track number of this track.
        /// </summary>
        /// <value>
        /// The track number.
        /// </value>
        public byte TrackNumber { get; }

        /// <summary>
        /// Gets the track International Standard Recording Code.
        /// </summary>
        /// <value>
        /// The track International Standard Recording Code.
        /// </value>
        public ReadOnlyMemory<byte> TrackISRC { get; }

        /// <summary>
        /// Gets a value indicating whether this track is audio track.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this track is audio track; otherwise, <c>false</c>.
        /// </value>
        public bool IsAudio => (field4 & 0x80) > 0;

        /// <summary>
        /// Gets the pre-emphasis flag.
        /// </summary>
        /// <value>
        /// The pre-emphasis flag.
        /// </value>
        public bool PreEmphasis => (field4 & 0x40) > 0;

        /// <summary>
        /// Gets the track indexes.
        /// </summary>
        /// <value>
        /// The track indexes.
        /// </value>
        public ReadOnlyMemory<CueSheetTrackIndex> TrackIndexes { get; }

        /// <summary>
        /// Reads the <see cref="CueSheetTrack"/> from the specified data source.
        /// </summary>
        /// <param name="dataSource">The data source.</param>
        /// <returns></returns>
        public static CueSheetTrack ReadFrom(IReadableDataSource<byte> dataSource)
        {
            Span<byte> a = stackalloc byte[Unsafe.SizeOf<RawCueSheetTrack>()];
            var rr = dataSource.Read(a);
            if (rr < a.Length)
            {
                throw new ArgumentException("The dataSource has not enough data!", nameof(dataSource));
            }
            var y = MemoryMarshal.Read<RawCueSheetTrack>(a);
            if (y.indexes > 0)
            {
                var m = new CueSheetTrackIndex[y.indexes];
                var g = MemoryMarshal.AsBytes(m.AsSpan());
                var rr2 = dataSource.Read(g);
                if (rr2 < g.Length)
                {
                    throw new ArgumentException("The dataSource has not enough data!", nameof(dataSource));
                }
                for (int i = 0; i < m.Length; i++)
                {
                    m[i] = CueSheetTrackIndex.ConvertToBigEndian(m[i]);
                }
                unsafe
                {
                    byte[]? isrc = new byte[RawCueSheetTrack.ISRCLength];
                    new Span<byte>(y.isrc, 12).CopyTo(isrc.AsSpan());
                    return new(y.trackOffset, y.trackNumber, isrc, y.field4, m);
                }
            }
            else
            {
                unsafe
                {
                    byte[]? isrc = new byte[RawCueSheetTrack.ISRCLength];
                    new Span<byte>(y.isrc, 12).CopyTo(isrc.AsSpan());
                    return new(y.trackOffset, y.trackNumber, isrc, y.field4, ReadOnlyMemory<CueSheetTrackIndex>.Empty);
                }
            }
        }

        /// <summary>
        /// Reads the <see cref="CueSheetTrack" /> from the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="bytesConsumed">The bytes consumed.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">The buffer has not enough data! - buffer
        /// or
        /// The buffer has not enough data! - buffer</exception>
        public static CueSheetTrack ReadFrom(ReadOnlySpan<byte> buffer, out int bytesConsumed)
        {
            if (buffer.Length < Unsafe.SizeOf<RawCueSheetTrack>())
            {
                throw new ArgumentException("The buffer has not enough data!", nameof(buffer));
            }
            var y = MemoryMarshal.Read<RawCueSheetTrack>(buffer);
            var br = buffer.Slice(Unsafe.SizeOf<RawCueSheetTrack>());
            if (y.indexes > 0)
            {
                var m = new CueSheetTrackIndex[y.indexes];
                var g = MemoryMarshal.AsBytes(m.AsSpan());
                if (br.Length < g.Length)
                {
                    throw new ArgumentException("The buffer has not enough data!", nameof(buffer));
                }
                br.SliceWhile(g.Length).CopyTo(g);
                for (int i = 0; i < m.Length; i++)
                {
                    m[i] = CueSheetTrackIndex.ConvertToBigEndian(m[i]);
                }
                unsafe
                {
                    byte[]? isrc = new byte[RawCueSheetTrack.ISRCLength];
                    new Span<byte>(y.isrc, 12).CopyTo(isrc.AsSpan());
                    bytesConsumed = Unsafe.SizeOf<RawCueSheetTrack>() + g.Length;
                    return new(y.trackOffset, y.trackNumber, isrc, y.field4, m);
                }
            }
            else
            {
                unsafe
                {
                    byte[]? isrc = new byte[RawCueSheetTrack.ISRCLength];
                    new Span<byte>(y.isrc, 12).CopyTo(isrc.AsSpan());
                    bytesConsumed = Unsafe.SizeOf<RawCueSheetTrack>();
                    return new(y.trackOffset, y.trackNumber, isrc, y.field4, ReadOnlyMemory<CueSheetTrackIndex>.Empty);
                }
            }
        }

        [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 36)]
        private unsafe struct RawCueSheetTrack
        {
            internal const int ISRCLength = 12;
            internal const int ReservedLength = 13;

            [FieldOffset(0)]
            internal readonly ulong trackOffset;

            [FieldOffset(8)]
            internal readonly byte trackNumber;

            [FieldOffset(9)]
            internal fixed byte isrc[ISRCLength];

            [FieldOffset(21)]
            internal readonly byte field4;

            [FieldOffset(22)]
            internal fixed byte reserved[ReservedLength];

            [FieldOffset(35)]
            internal readonly byte indexes;
        }
    }
}
