using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Codecs.Flac.Metadata
{
    /// <summary>
    /// Represents a CUE-sheet of FLAC files.
    /// </summary>
    public readonly struct FlacCueSheet
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FlacCueSheet"/> struct.
        /// </summary>
        /// <param name="mediaCatalogNumber">The media catalog number.</param>
        /// <param name="leadInSamples">The lead in samples.</param>
        /// <param name="isCD">if set to <c>true</c> [is cd].</param>
        /// <param name="tracks">The tracks.</param>
        public FlacCueSheet(ReadOnlyMemory<byte> mediaCatalogNumber, ulong leadInSamples, bool isCD, ReadOnlyMemory<CueSheetTrack> tracks)
        {
            MediaCatalogNumber = mediaCatalogNumber;
            LeadInSamples = leadInSamples;
            IsCD = isCD;
            Tracks = tracks;
        }

        /// <summary>
        /// The Media catalog number
        /// </summary>
        public ReadOnlyMemory<byte> MediaCatalogNumber { get; }

        /// <summary>
        /// Gets the number of lead-in samples.
        /// </summary>
        /// <value>
        /// The number of lead-in samples.
        /// </value>
        public ulong LeadInSamples { get; }

        /// <summary>
        /// Gets a value indicating whether this cue sheet corresponds to a Compact Disc.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this cue sheet corresponds to a Compact Disc; otherwise, <c>false</c>.
        /// </value>
        public bool IsCD { get; }

        /// <summary>
        /// Gets the tracks.
        /// </summary>
        /// <value>
        /// The tracks.
        /// </value>
        public ReadOnlyMemory<CueSheetTrack> Tracks { get; }

        /// <summary>
        /// Reads the <see cref="FlacCueSheet"/> from the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="bytesConsumed">The bytes consumed.</param>
        /// <returns></returns>
        public static FlacCueSheet ReadFrom(ReadOnlySpan<byte> buffer, out int bytesConsumed)
        {
            if (buffer.Length < Unsafe.SizeOf<RawCueSheet>())
            {
                throw new ArgumentException("The buffer has not enough data!", nameof(buffer));
            }
            var y = MemoryMarshal.Read<RawCueSheet>(buffer);
            var br = buffer.Slice(Unsafe.SizeOf<RawCueSheet>());
            bytesConsumed = Unsafe.SizeOf<RawCueSheet>();
            var catalogNumber = new byte[RawCueSheet.CatalogNumberLength];
            unsafe
            {
                new Span<byte>(y.catalogNumber, RawCueSheet.CatalogNumberLength).CopyTo(catalogNumber.AsSpan());
            }
            if (y.length > 0)
            {
                var tracks = new CueSheetTrack[y.length];
                for (int i = 0; i < tracks.Length; i++)
                {
                    tracks[i] = CueSheetTrack.ReadFrom(br, out var rr);
                    br = br.Slice(rr);
                    bytesConsumed += rr;
                }
                return new(catalogNumber, y.leadInSamples, (y.field3 & 0x80) > 0, tracks);
            }
            throw new FlacException("Invalid Cue Sheet!");
        }

        [StructLayout(LayoutKind.Explicit, Pack = 1, Size = CatalogNumberLength + sizeof(ulong) + 1 + ReservedLength + 1)]
        private unsafe struct RawCueSheet
        {
            internal const int CatalogNumberLength = 128;
            internal const int ReservedLength = 258;

            [FieldOffset(0)]
            internal fixed byte catalogNumber[CatalogNumberLength];

            [FieldOffset(CatalogNumberLength)]
            internal readonly ulong leadInSamples;

            [FieldOffset(CatalogNumberLength + sizeof(ulong))]
            internal readonly byte field3;

            [FieldOffset(CatalogNumberLength + sizeof(ulong) + 1)]
            internal fixed byte reserved[ReservedLength];

            [FieldOffset(CatalogNumberLength + sizeof(ulong) + 1 + ReservedLength)]
            internal readonly byte length;
        }
    }
}
