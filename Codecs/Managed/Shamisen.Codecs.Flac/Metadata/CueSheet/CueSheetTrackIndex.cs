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
    /// Represents a track index of a <see cref="CueSheetTrack"/>.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 12)]
    public readonly struct CueSheetTrackIndex
    {
        [FieldOffset(0)]
        private readonly ulong offset;

        [FieldOffset(8)]
        private readonly byte index;

        [FieldOffset(9)]
        private readonly UInt24 reserved;

        /// <summary>
        /// Initializes a new instance of the <see cref="CueSheetTrackIndex"/> struct.
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <param name="index">The index.</param>
        /// <param name="reserved">The reserved.</param>
        public CueSheetTrackIndex(ulong offset, byte index, UInt24 reserved)
        {
            this.offset = offset;
            this.index = index;
            this.reserved = reserved;
        }

        /// <summary>
        /// Gets the offset.
        /// </summary>
        /// <value>
        /// The offset.
        /// </value>
        public ulong Offset => offset;

        /// <summary>
        /// Gets the index point number.
        /// </summary>
        /// <value>
        /// The index point number.
        /// </value>
        public byte IndexPointNumber => index;

        /// <summary>
        /// Gets the reserved space.
        /// </summary>
        /// <value>
        /// The reserved space.
        /// </value>
        public UInt24 Reserved => reserved;

        /// <summary>
        /// Converts the specified <paramref name="systemEndianedValue"/> to/from BIG ENDIAN.
        /// </summary>
        /// <param name="systemEndianedValue">The value in system endian.</param>
        /// <returns>The endian-reversed value if the system is little-endian, otherwise, <paramref name="systemEndianedValue"/>.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static CueSheetTrackIndex ConvertToBigEndian(CueSheetTrackIndex systemEndianedValue)
            => new(BinaryExtensions.ConvertToBigEndian(systemEndianedValue.offset), systemEndianedValue.index, BinaryExtensions.ConvertToBigEndian(systemEndianedValue.reserved));

        /// <summary>
        /// Reads the <see cref="CueSheetTrackIndex"/> from the specified <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static CueSheetTrackIndex ReadFrom(ReadOnlySpan<byte> buffer)
        {
            if (buffer.Length < Unsafe.SizeOf<CueSheetTrackIndex>())
                throw new ArgumentException("The buffer has not enough data!", nameof(buffer));
            var h = MemoryMarshal.Read<CueSheetTrackIndex>(buffer);
            return ConvertToBigEndian(h);
        }

        /// <summary>
        /// Reads the <see cref="CueSheetTrackIndex"/> from the specified <paramref name="dataSource"/>.
        /// </summary>
        /// <param name="dataSource">The data source.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static CueSheetTrackIndex ReadFrom(IReadableDataSource<byte> dataSource)
        {
            Span<byte> a = stackalloc byte[12];
            var rr = dataSource.Read(a);
            return rr >= a.Length ? ReadFrom(a) : throw new ArgumentException("The dataSource has not enough data!", nameof(dataSource));
        }
    }
}
