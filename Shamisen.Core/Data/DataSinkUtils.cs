using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Data
{
    /// <summary>
    /// Contains some utility functions for <see cref="IDataSink{TSample}"/>.
    /// </summary>
    public static partial class DataSinkUtils
    {
        /// <summary>
        /// Writes the byte to the specified <paramref name="sink"/>.
        /// </summary>
        /// <param name="sink">The sink to write to.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteByte(this IDataSink<byte> sink, byte value)
        {
            Span<byte> buffer = stackalloc byte[] { value };
            sink.Write(buffer);
        }

        /// <summary>
        /// Writes the <see cref="sbyte"/> value to the specified <paramref name="sink"/>.
        /// </summary>
        /// <param name="sink">The sink to write to.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteSByte(this IDataSink<byte> sink, sbyte value)
        {
            Span<byte> buffer = stackalloc byte[] { unchecked((byte)value) };
            sink.Write(buffer);
        }

        /// <summary>
        /// Writes the <see cref="OffsetSByte"/> value to the specified <paramref name="sink"/>.
        /// </summary>
        /// <param name="sink">The sink to write to.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteSByte(this IDataSink<byte> sink, OffsetSByte value)
        {
            Span<byte> buffer = stackalloc byte[] { unchecked((byte)value) };
            sink.Write(buffer);
        }

        /// <summary>
        /// Writes the <see cref="float"/> value to the specified <paramref name="sink"/> with little endian.
        /// </summary>
        /// <param name="sink">The sink to write to.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteSingleLittleEndian(this IDataSink<byte> sink, float value)
            => sink.WriteInt32LittleEndian(BitConverter.SingleToInt32Bits(value));

        /// <summary>
        /// Writes the <see cref="float"/> value to the specified <paramref name="sink"/> with BIG endian.
        /// </summary>
        /// <param name="sink">The sink to write to.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteSingleBigEndian(this IDataSink<byte> sink, float value)
            => sink.WriteInt32BigEndian(BitConverter.SingleToInt32Bits(value));

        /// <summary>
        /// Writes the <see cref="float"/> value to the specified <paramref name="sink"/> with little endian.
        /// </summary>
        /// <param name="sink">The sink to write to.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteDoubleLittleEndian(this IDataSink<byte> sink, double value)
            => sink.WriteInt64LittleEndian(BitConverter.DoubleToInt64Bits(value));

        /// <summary>
        /// Writes the <see cref="float"/> value to the specified <paramref name="sink"/> with BIG endian.
        /// </summary>
        /// <param name="sink">The sink to write to.</param>
        /// <param name="value">The value to write.</param>
        public static void WriteDoubleBigEndian(this IDataSink<byte> sink, double value)
            => sink.WriteInt64BigEndian(BitConverter.DoubleToInt64Bits(value));
    }
}
