using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Shamisen.Data.Binary
{
    /// <summary>
    /// Provides some functions that helps you to analyze data input binary.
    /// </summary>
    /// <seealso cref="IDataSource{TSample}" />
    public static class DataReaderExtensions
    {
        #region Non-Try Functions

        #region byte and sbyte

        /// <summary>
        /// Reads single <see cref="byte"/> from <paramref name="dataReader"/>.
        /// </summary>
        /// <param name="dataReader">The data reader.</param>
        /// <returns>The value read.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte ReadByte(this IDataSource<byte> dataReader)
        {
            unsafe
            {
                byte al;
                Span<byte> span = new Span<byte>(&al, sizeof(byte));
                dataReader.CheckRead(span);
                return al;
            }
        }

        /// <summary>
        /// Reads single <see cref="sbyte"/> from <paramref name="dataReader"/>.
        /// </summary>
        /// <param name="dataReader">The data reader.</param>
        /// <returns>The value read.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sbyte ReadSignedByte(this IDataSource<byte> dataReader)
        {
            unsafe
            {
                sbyte al;
                Span<byte> span = new Span<byte>(&al, sizeof(sbyte));
                dataReader.CheckRead(span);
                return al;
            }
        }

        #endregion byte and sbyte

        #region ushort and short

        /// <summary>
        /// Reads single little-endianed <see cref="ushort"/> from <paramref name="dataReader"/>.
        /// </summary>
        /// <param name="dataReader">The data reader.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort ReadUInt16LittleEndian(this IDataSource<byte> dataReader)
        {
            unsafe
            {
                ushort ax;
                Span<byte> span = new Span<byte>(&ax, sizeof(ushort));
                dataReader.CheckRead(span);
                return ax;
            }
        }

        /// <summary>
        /// Reads single big-endianed <see cref="ushort"/> from <paramref name="dataReader"/>.
        /// </summary>
        /// <param name="dataReader">The data reader.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort ReadUInt16BigEndian(this IDataSource<byte> dataReader)
        {
            unsafe
            {
                ushort rawUInt16;
                Span<byte> span = new Span<byte>(&rawUInt16, sizeof(ushort));
                dataReader.CheckRead(span);
                return BinaryExtensions.ConvertToBigEndian(rawUInt16);
            }
        }

        /// <summary>
        /// Reads single little-endianed <see cref="short"/> from <paramref name="dataReader"/>.
        /// </summary>
        /// <param name="dataReader">The data reader.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short ReadInt16LittleEndian(this IDataSource<byte> dataReader)
        {
            unsafe
            {
                short rawInt16;
                Span<byte> span = new Span<byte>(&rawInt16, sizeof(short));
                dataReader.CheckRead(span);
                return BinaryExtensions.ConvertToLittleEndian(rawInt16);
            }
        }

        /// <summary>
        /// Reads single big-endianed <see cref="short"/> from <paramref name="dataReader"/>.
        /// </summary>
        /// <param name="dataReader">The data reader.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short ReadInt16BigEndian(this IDataSource<byte> dataReader)
        {
            unsafe
            {
                short rawInt16;
                Span<byte> span = new Span<byte>(&rawInt16, sizeof(short));
                dataReader.CheckRead(span);
                return BinaryExtensions.ConvertToBigEndian(rawInt16);
            }
        }

        #endregion ushort and short

        #region uint and int

        /// <summary>
        /// Reads single little-endianed <see cref="uint"/> from <paramref name="dataReader"/>.
        /// </summary>
        /// <param name="dataReader">The data reader.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ReadUInt32LittleEndian(this IDataSource<byte> dataReader)
        {
            unsafe
            {
                uint rawUInt32;
                Span<byte> span = new Span<byte>(&rawUInt32, sizeof(uint));
                dataReader.CheckRead(span);
                return BinaryExtensions.ConvertToLittleEndian(rawUInt32);
            }
        }

        /// <summary>
        /// Reads single big-endianed <see cref="uint"/> from <paramref name="dataReader"/>.
        /// </summary>
        /// <param name="dataReader">The data reader.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ReadUInt32BigEndian(this IDataSource<byte> dataReader)
        {
            unsafe
            {
                uint rawUInt32;
                Span<byte> span = new Span<byte>(&rawUInt32, sizeof(uint));
                dataReader.CheckRead(span);
                return BinaryExtensions.ConvertToBigEndian(rawUInt32);
            }
        }

        /// <summary>
        /// Reads single little-endianed <see cref="int"/> from <paramref name="dataReader"/>.
        /// </summary>
        /// <param name="dataReader">The data reader.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReadInt32LittleEndian(this IDataSource<byte> dataReader)
        {
            unsafe
            {
                int rawInt32;
                Span<byte> span = new Span<byte>(&rawInt32, sizeof(int));
                dataReader.CheckRead(span);
                return BinaryExtensions.ConvertToLittleEndian(rawInt32);
            }
        }

        /// <summary>
        /// Reads single big-endianed <see cref="int"/> from <paramref name="dataReader"/>.
        /// </summary>
        /// <param name="dataReader">The data reader.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReadInt32BigEndian(this IDataSource<byte> dataReader)
        {
            unsafe
            {
                int rawInt32;
                Span<byte> span = new Span<byte>(&rawInt32, sizeof(int));
                dataReader.CheckRead(span);
                return BinaryExtensions.ConvertToBigEndian(rawInt32);
            }
        }

        #endregion uint and int

        #region ulong and long

        /// <summary>
        /// Reads single little-endianed <see cref="ulong"/> from <paramref name="dataReader"/>.
        /// </summary>
        /// <param name="dataReader">The data reader.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong ReadUInt64LittleEndian(this IDataSource<byte> dataReader)
        {
            unsafe
            {
                ulong rawUInt64;
                Span<byte> span = new Span<byte>(&rawUInt64, sizeof(ulong));
                dataReader.CheckRead(span);
                return BinaryExtensions.ConvertToLittleEndian(rawUInt64);
            }
        }

        /// <summary>
        /// Reads single big-endianed <see cref="ulong"/> from <paramref name="dataReader"/>.
        /// </summary>
        /// <param name="dataReader">The data reader.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong ReadUInt64BigEndian(this IDataSource<byte> dataReader)
        {
            unsafe
            {
                ulong rawUInt64;
                Span<byte> span = new Span<byte>(&rawUInt64, sizeof(ulong));
                dataReader.CheckRead(span);
                return BinaryExtensions.ConvertToBigEndian(rawUInt64);
            }
        }

        /// <summary>
        /// Reads single little-endianed <see cref="long"/> from <paramref name="dataReader"/>.
        /// </summary>
        /// <param name="dataReader">The data reader.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long ReadInt64LittleEndian(this IDataSource<byte> dataReader)
        {
            unsafe
            {
                long rawInt64;
                Span<byte> span = new Span<byte>(&rawInt64, sizeof(long));
                dataReader.CheckRead(span);
                return BinaryExtensions.ConvertToLittleEndian(rawInt64);
            }
        }

        /// <summary>
        /// Reads single big-endianed <see cref="long"/> from <paramref name="dataReader"/>.
        /// </summary>
        /// <param name="dataReader">The data reader.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long ReadInt64BigEndian(this IDataSource<byte> dataReader)
        {
            unsafe
            {
                long rawInt64;
                Span<byte> span = new Span<byte>(&rawInt64, sizeof(long));
                dataReader.CheckRead(span);
                return BinaryExtensions.ConvertToBigEndian(rawInt64);
            }
        }

        #endregion ulong and long

        #region Structural

        /// <summary>
        /// Reads single <typeparamref name="TStruct"/> value from <paramref name="dataReader"/>.
        /// </summary>
        /// <typeparam name="TStruct">The type to read.</typeparam>
        /// <param name="dataReader">The data reader.</param>
        /// <returns>The deserialized <typeparamref name="TStruct"/> value.</returns>
        public static TStruct ReadStruct<TStruct>(this IDataSource<byte> dataReader) where TStruct : unmanaged
        {
            Span<byte> buffer = stackalloc byte[Unsafe.SizeOf<TStruct>()];
            dataReader.CheckRead(buffer);
            return MemoryMarshal.Read<TStruct>(buffer);
        }

        #endregion Structural

        #endregion Non-Try Functions

        #region TryFunctions

        #region byte and sbyte

        /// <summary>
        /// Reads single <see cref="byte"/> from <paramref name="dataReader"/>.
        /// </summary>
        /// <param name="dataReader">The data reader.</param>
        /// <returns>The value read.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadResult TryReadByte(this IDataSource<byte> dataReader, out byte read)
        {
            Span<byte> span = stackalloc byte[1];
            var result = dataReader.Read(span);
            read = span[0];
            return result;
        }

        /// <summary>
        /// Reads single <see cref="sbyte"/> from <paramref name="dataReader"/>.
        /// </summary>
        /// <param name="dataReader">The data reader.</param>
        /// <returns>The value read.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadResult TryReadSignedByte(this IDataSource<byte> dataReader, out sbyte read)
        {
            Span<byte> span = stackalloc byte[1];
            var result = dataReader.Read(span);
            read = unchecked((sbyte)span[0]);
            return result;
        }

        #endregion byte and sbyte

        #region ushort and short

        /// <summary>
        /// Reads single little-endianed <see cref="ushort"/> from <paramref name="dataReader"/>.
        /// </summary>
        /// <param name="dataReader">The data reader.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadResult TryReadUInt16LittleEndian(this IDataSource<byte> dataReader, out ushort read)
        {
            Span<byte> span = stackalloc byte[2];
            var result = dataReader.Read(span);
            read = BinaryPrimitives.ReadUInt16LittleEndian(span);
            return result;
        }

        /// <summary>
        /// Reads single big-endianed <see cref="ushort"/> from <paramref name="dataReader"/>.
        /// </summary>
        /// <param name="dataReader">The data reader.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadResult TryReadUInt16BigEndian(this IDataSource<byte> dataReader, out ushort read)
        {
            Span<byte> span = stackalloc byte[2];
            var result = dataReader.Read(span);
            read = BinaryPrimitives.ReadUInt16BigEndian(span);
            return result;
        }

        /// <summary>
        /// Reads single little-endianed <see cref="short"/> from <paramref name="dataReader"/>.
        /// </summary>
        /// <param name="dataReader">The data reader.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadResult TryReadInt16LittleEndian(this IDataSource<byte> dataReader, out short read)
        {
            Span<byte> span = stackalloc byte[2];
            var result = dataReader.Read(span);
            read = BinaryPrimitives.ReadInt16LittleEndian(span);
            return result;
        }

        /// <summary>
        /// Reads single big-endianed <see cref="short"/> from <paramref name="dataReader"/>.
        /// </summary>
        /// <param name="dataReader">The data reader.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadResult TryReadInt16BigEndian(this IDataSource<byte> dataReader, out short read)
        {
            Span<byte> span = stackalloc byte[2];
            var result = dataReader.Read(span);
            read = BinaryPrimitives.ReadInt16BigEndian(span);
            return result;
        }

        #endregion ushort and short

        #region uint and int

        /// <summary>
        /// Reads single little-endianed <see cref="uint"/> from <paramref name="dataReader"/>.
        /// </summary>
        /// <param name="dataReader">The data reader.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadResult TryReadUInt32LittleEndian(this IDataSource<byte> dataReader, out uint read)
        {
            Span<byte> span = stackalloc byte[4];
            var result = dataReader.Read(span);
            read = BinaryPrimitives.ReadUInt32LittleEndian(span);
            return result;
        }

        /// <summary>
        /// Reads single big-endianed <see cref="uint"/> from <paramref name="dataReader"/>.
        /// </summary>
        /// <param name="dataReader">The data reader.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadResult TryReadUInt32BigEndian(this IDataSource<byte> dataReader, out uint read)
        {
            Span<byte> span = stackalloc byte[4];
            var result = dataReader.Read(span);
            read = BinaryPrimitives.ReadUInt32BigEndian(span);
            return result;
        }

        /// <summary>
        /// Reads single little-endianed <see cref="int"/> from <paramref name="dataReader"/>.
        /// </summary>
        /// <param name="dataReader">The data reader.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadResult TryReadInt32LittleEndian(this IDataSource<byte> dataReader, out int read)
        {
            Span<byte> span = stackalloc byte[4];
            var result = dataReader.Read(span);
            read = BinaryPrimitives.ReadInt32LittleEndian(span);
            return result;
        }

        /// <summary>
        /// Reads single big-endianed <see cref="int"/> from <paramref name="dataReader"/>.
        /// </summary>
        /// <param name="dataReader">The data reader.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadResult TryReadInt32BigEndian(this IDataSource<byte> dataReader, out int read)
        {
            Span<byte> span = stackalloc byte[4];
            var result = dataReader.Read(span);
            read = BinaryPrimitives.ReadInt32BigEndian(span);
            return result;
        }

        #endregion uint and int

        #region ulong and long

        /// <summary>
        /// Reads single little-endianed <see cref="ulong"/> from <paramref name="dataReader"/>.
        /// </summary>
        /// <param name="dataReader">The data reader.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadResult TryReadUInt64LittleEndian(this IDataSource<byte> dataReader, out ulong read)
        {
            Span<byte> span = stackalloc byte[8];
            var result = dataReader.Read(span);
            read = BinaryPrimitives.ReadUInt64LittleEndian(span);
            return result;
        }

        /// <summary>
        /// Reads single big-endianed <see cref="ulong"/> from <paramref name="dataReader"/>.
        /// </summary>
        /// <param name="dataReader">The data reader.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadResult TryReadUInt64BigEndian(this IDataSource<byte> dataReader, out ulong read)
        {
            Span<byte> span = stackalloc byte[8];
            var result = dataReader.Read(span);
            read = BinaryPrimitives.ReadUInt64BigEndian(span);
            return result;
        }

        /// <summary>
        /// Reads single little-endianed <see cref="long"/> from <paramref name="dataReader"/>.
        /// </summary>
        /// <param name="dataReader">The data reader.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadResult TryReadInt64LittleEndian(this IDataSource<byte> dataReader, out long read)
        {
            Span<byte> span = stackalloc byte[8];
            var result = dataReader.Read(span);
            read = BinaryPrimitives.ReadInt64LittleEndian(span);
            return result;
        }

        /// <summary>
        /// Reads single big-endianed <see cref="long"/> from <paramref name="dataReader"/>.
        /// </summary>
        /// <param name="dataReader">The data reader.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadResult TryReadInt64BigEndian(this IDataSource<byte> dataReader, out long read)
        {
            Span<byte> span = stackalloc byte[8];
            var result = dataReader.Read(span);
            read = BinaryPrimitives.ReadInt64BigEndian(span);
            return result;
        }

        #endregion ulong and long

        #endregion TryFunctions

        #region Others

        /// <summary>
        /// Reads some data from <paramref name="dataSource"/> to fill <paramref name="buffer"/>.
        /// </summary>
        /// <typeparam name="T">Type of buffer data.</typeparam>
        /// <param name="dataSource">The data source.</param>
        /// <param name="buffer">The buffer.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ReadAll<T>(this IDataSource<T> dataSource, Span<T> buffer) where T : unmanaged
        {
            var bufRemain = buffer;
            ReadResult lastResult = 0;
            do
            {
                var result = dataSource.Read(bufRemain);
                if (result.IsEndOfStream) ThrowEndOfStreamException("filling buffer");
                if (result.HasData)
                {
                    if (result.Length == bufRemain.Length) return;
                    bufRemain = bufRemain.Slice(result.Length);
                }
                else if (lastResult.HasNoData)
                {
                    ThrowBufferingException("filling buffer");
                }
                lastResult = result;
            } while (!bufRemain.IsEmpty);
        }

        /// <summary>
        /// Tries to read some data from <paramref name="dataSource"/> to fill <paramref name="buffer"/> while the <paramref name="dataSource"/> doesn't run out of data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataSource">The data source.</param>
        /// <param name="buffer">The buffer.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadResult TryReadAll<T>(this IDataSource<T> dataSource, Span<T> buffer) where T : unmanaged
        {
            var bufRemain = buffer;
            ReadResult lastResult = 0;
            do
            {
                var result = dataSource.Read(bufRemain);
                if (result.IsEndOfStream) return result;
                if (result.HasData)
                {
                    if (result.Length == bufRemain.Length) return buffer.Length;
                    bufRemain = bufRemain.Slice(result.Length);
                }
                else if (lastResult.HasNoData)
                {
                    return buffer.Length - bufRemain.Length;
                }
                lastResult = result;
            } while (!bufRemain.IsEmpty);
            return buffer.Length;
        }

        /// <summary>
        /// Reads some data from <paramref name="dataSource"/> to <paramref name="buffer"/>.
        /// </summary>
        /// <typeparam name="T">Type of buffer data.</typeparam>
        /// <param name="dataSource">The data source.</param>
        /// <param name="buffer">The buffer.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CheckRead<T>(this IDataSource<T> dataSource, Span<T> buffer) where T : unmanaged
        {
            var bufRemain = buffer;
            var result = dataSource.Read(bufRemain);
            result.ThrowWhenInsufficient(bufRemain.Length, "filling buffer");
        }

        /// <summary>
        /// Throws an exception when the read data is insufficient.
        /// </summary>
        /// <param name="readResult">The read result.</param>
        /// <param name="lengthRequired">The least length required.</param>
        /// <param name="situation">The situation of reading some required data.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerNonUserCode]
        public static void ThrowWhenInsufficient(this ReadResult readResult, int lengthRequired = 1, string situation = "")
        {
            if (readResult.IsEndOfStream)
            {
                ThrowEndOfStreamException(situation);
            }
            if (readResult.Length < lengthRequired)
            {
                ThrowBufferingException(situation);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [DebuggerNonUserCode]
        private static void ThrowBufferingException(string situation)
            => throw new BufferingException($"The dataSource ran out of buffered data{(string.IsNullOrWhiteSpace(situation) ? "" : $" while {situation}")}!");

        [MethodImpl(MethodImplOptions.NoInlining)]
        [DebuggerNonUserCode]
        private static void ThrowEndOfStreamException(string situation)
            => throw new EndOfStreamException($"The dataSource ran out of data{(string.IsNullOrWhiteSpace(situation) ? "" : $" while {situation}")}!");

        #endregion Others
    }
}
