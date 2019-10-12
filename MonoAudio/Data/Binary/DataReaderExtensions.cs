using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace MonoAudio.Data.Binary
{
    /// <summary>
    /// Provides some functions that helps you to analyze data input binary.
    /// </summary>
    /// <seealso cref="ISynchronizedDataReader{TSample}" />
    public static class DataReaderExtensions
    {
        #region byte and sbyte

        /// <summary>
        /// Reads single <see cref="byte"/> from <paramref name="dataReader"/>.
        /// </summary>
        /// <param name="dataReader">The data reader.</param>
        /// <returns>The value read.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte ReadByte(this ISynchronizedDataReader<byte> dataReader)
        {
            Span<byte> span = stackalloc byte[1];
            _ = dataReader.Read(span);
            return span[0];
        }

        /// <summary>
        /// Reads single <see cref="sbyte"/> from <paramref name="dataReader"/>.
        /// </summary>
        /// <param name="dataReader">The data reader.</param>
        /// <returns>The value read.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sbyte ReadSignedByte(this ISynchronizedDataReader<byte> dataReader)
        {
            Span<byte> span = stackalloc byte[1];
            _ = dataReader.Read(span);
            return unchecked((sbyte)span[0]);
        }

        #endregion byte and sbyte

        #region ushort and short

        /// <summary>
        /// Reads single little-endianed <see cref="ushort"/> from <paramref name="dataReader"/>.
        /// </summary>
        /// <param name="dataReader">The data reader.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort ReadUInt16LittleEndian(this ISynchronizedDataReader<byte> dataReader)
        {
            Span<byte> span = stackalloc byte[2];
            _ = dataReader.Read(span);
            return BinaryPrimitives.ReadUInt16LittleEndian(span);
        }

        /// <summary>
        /// Reads single big-endianed <see cref="ushort"/> from <paramref name="dataReader"/>.
        /// </summary>
        /// <param name="dataReader">The data reader.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort ReadUInt16BigEndian(this ISynchronizedDataReader<byte> dataReader)
        {
            Span<byte> span = stackalloc byte[2];
            _ = dataReader.Read(span);
            return BinaryPrimitives.ReadUInt16BigEndian(span);
        }

        /// <summary>
        /// Reads single little-endianed <see cref="short"/> from <paramref name="dataReader"/>.
        /// </summary>
        /// <param name="dataReader">The data reader.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short ReadInt16LittleEndian(this ISynchronizedDataReader<byte> dataReader)
        {
            Span<byte> span = stackalloc byte[2];
            _ = dataReader.Read(span);
            return BinaryPrimitives.ReadInt16LittleEndian(span);
        }

        /// <summary>
        /// Reads single big-endianed <see cref="short"/> from <paramref name="dataReader"/>.
        /// </summary>
        /// <param name="dataReader">The data reader.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short ReadInt16BigEndian(this ISynchronizedDataReader<byte> dataReader)
        {
            Span<byte> span = stackalloc byte[2];
            _ = dataReader.Read(span);
            return BinaryPrimitives.ReadInt16BigEndian(span);
        }

        #endregion ushort and short

        #region uint and int

        /// <summary>
        /// Reads single little-endianed <see cref="uint"/> from <paramref name="dataReader"/>.
        /// </summary>
        /// <param name="dataReader">The data reader.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ReadUInt32LittleEndian(this ISynchronizedDataReader<byte> dataReader)
        {
            Span<byte> span = stackalloc byte[4];
            _ = dataReader.Read(span);
            return BinaryPrimitives.ReadUInt32LittleEndian(span);
        }

        /// <summary>
        /// Reads single big-endianed <see cref="uint"/> from <paramref name="dataReader"/>.
        /// </summary>
        /// <param name="dataReader">The data reader.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ReadUInt32BigEndian(this ISynchronizedDataReader<byte> dataReader)
        {
            Span<byte> span = stackalloc byte[4];
            _ = dataReader.Read(span);
            return BinaryPrimitives.ReadUInt32BigEndian(span);
        }

        /// <summary>
        /// Reads single little-endianed <see cref="int"/> from <paramref name="dataReader"/>.
        /// </summary>
        /// <param name="dataReader">The data reader.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReadInt32LittleEndian(this ISynchronizedDataReader<byte> dataReader)
        {
            Span<byte> span = stackalloc byte[4];
            _ = dataReader.Read(span);
            return BinaryPrimitives.ReadInt32LittleEndian(span);
        }

        /// <summary>
        /// Reads single big-endianed <see cref="int"/> from <paramref name="dataReader"/>.
        /// </summary>
        /// <param name="dataReader">The data reader.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReadInt32BigEndian(this ISynchronizedDataReader<byte> dataReader)
        {
            Span<byte> span = stackalloc byte[4];
            _ = dataReader.Read(span);
            return BinaryPrimitives.ReadInt32BigEndian(span);
        }

        #endregion uint and int

        #region ulong and long

        /// <summary>
        /// Reads single little-endianed <see cref="ulong"/> from <paramref name="dataReader"/>.
        /// </summary>
        /// <param name="dataReader">The data reader.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong ReadUInt64LittleEndian(this ISynchronizedDataReader<byte> dataReader)
        {
            Span<byte> span = stackalloc byte[8];
            _ = dataReader.Read(span);
            return BinaryPrimitives.ReadUInt64LittleEndian(span);
        }

        /// <summary>
        /// Reads single big-endianed <see cref="ulong"/> from <paramref name="dataReader"/>.
        /// </summary>
        /// <param name="dataReader">The data reader.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong ReadUInt64BigEndian(this ISynchronizedDataReader<byte> dataReader)
        {
            Span<byte> span = stackalloc byte[8];
            _ = dataReader.Read(span);
            return BinaryPrimitives.ReadUInt64BigEndian(span);
        }

        /// <summary>
        /// Reads single little-endianed <see cref="long"/> from <paramref name="dataReader"/>.
        /// </summary>
        /// <param name="dataReader">The data reader.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long ReadInt64LittleEndian(this ISynchronizedDataReader<byte> dataReader)
        {
            Span<byte> span = stackalloc byte[8];
            _ = dataReader.Read(span);
            return BinaryPrimitives.ReadInt64LittleEndian(span);
        }

        /// <summary>
        /// Reads single big-endianed <see cref="long"/> from <paramref name="dataReader"/>.
        /// </summary>
        /// <param name="dataReader">The data reader.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long ReadInt64BigEndian(this ISynchronizedDataReader<byte> dataReader)
        {
            Span<byte> span = stackalloc byte[8];
            _ = dataReader.Read(span);
            return BinaryPrimitives.ReadInt64BigEndian(span);
        }

        #endregion ulong and long

        #region Structural

        /// <summary>
        /// Reads single <typeparamref name="TStruct"/> value from <paramref name="dataReader"/>.
        /// </summary>
        /// <typeparam name="TStruct">The type to read.</typeparam>
        /// <param name="dataReader">The data reader.</param>
        /// <returns>The deserialized <typeparamref name="TStruct"/> value.</returns>
        public static TStruct ReadStruct<TStruct>(this ISynchronizedDataReader<byte> dataReader) where TStruct : unmanaged
        {
            Span<byte> buffer = stackalloc byte[Unsafe.SizeOf<TStruct>()];
            if (dataReader.Read(buffer) != buffer.Length)
                throw new ArgumentException($"Not enough data are available in {nameof(dataReader)}!", nameof(dataReader));
            return MemoryMarshal.Read<TStruct>(buffer);
        }

        #endregion Structural
    }
}
