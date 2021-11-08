using System;
using System.Collections.Generic;
using System.Text;

using Shamisen.Data;
using Shamisen.Data.Binary;

namespace Shamisen.Codecs.Waveform
{
    /// <summary>
    /// Provides a utility function that manipulates chunks in WAVE file.
    /// </summary>
    public static class ChunkUtils
    {
        #region Fmt

        /*
        /// <summary>
        /// Reads and decodes the fmt chunk of WAVE file.
        /// </summary>
        /// <param name="header">The header.</param>
        /// <param name="dataSource">The data source.</param>
        /// <returns>The decoded <see cref="IWaveFormat"/>.</returns>
        internal static IWaveFormat DecodeFormat(RiffChunkHeader header, IDataSource dataSource)
        {
            var formatTag = (AudioEncoding)dataSource.ReadUInt16LittleEndian();
            ushort channels = dataSource.ReadUInt16LittleEndian();
            uint samplesPerSec = dataSource.ReadUInt32LittleEndian();
            uint avgBytesPerSec = dataSource.ReadUInt32LittleEndian();
            ushort blockAlign = dataSource.ReadUInt16LittleEndian();
            ushort bitDepth = dataSource.ReadUInt16LittleEndian();
            switch (header.Length)
            {
                case 18:    //Fake WAVEFORMATEXTENSIBLE that has only cbSize at end of chunk, Actually WAVEFORMAT
                    _ = dataSource.ReadUInt16LittleEndian();
                    return new StandardWaveFormat(formatTag, channels, samplesPerSec, avgBytesPerSec, blockAlign, bitDepth);
                case 16:
                    return new StandardWaveFormat(formatTag, channels, samplesPerSec, avgBytesPerSec, blockAlign, bitDepth);
                case 40:
                    var cbSize = dataSource.ReadUInt16LittleEndian();
                    var numValidBits = dataSource.ReadUInt16LittleEndian();
                    var chMask = dataSource.ReadUInt32LittleEndian();
                    var subFormat = dataSource.ReadStruct<Guid>();
                    if (BinaryExtensions.SystemEndianness == Endianness.Big) subFormat = BinaryExtensions.ReverseEndianness(subFormat);
                    var extSize = cbSize - 22;
                    ReadOnlyMemory<byte> extData = ReadOnlyMemory<byte>.Empty;
                    if (extSize > 0)
                    {
                        var bytes = new byte[extSize];
                        var length = dataSource.Read(bytes.AsSpan());
                        extData = bytes.AsMemory().Slice(0, length);
                    }
                    var exP = new ExtensionPart(cbSize, numValidBits, chMask, subFormat);
                    return new ExtensibleWaveFormat(
                        new StandardWaveFormat(formatTag, channels, samplesPerSec, avgBytesPerSec, blockAlign, bitDepth),
                        cbSize, numValidBits, chMask, subFormat, extData);
                default:
                    throw new NotSupportedException("The given data is invalid WAVE!");
            }
        }
        */

        #endregion Fmt

        /// <summary>
        /// Determines whether the specified <paramref name="chunkId"/> represents JUNK chunk.
        /// </summary>
        /// <param name="chunkId">The chunk identifier.</param>
        /// <returns>
        ///   <c>true</c> if the specified <paramref name="chunkId"/> represents JUNK chunk; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsJunkChunk(this ChunkId chunkId)
        {
            //UPPER to lower conversion
            var ckid = (uint)chunkId | 0x2020_2020u;
            return (ChunkId)ckid == ChunkId.JunkSmall;
        }
    }
}
