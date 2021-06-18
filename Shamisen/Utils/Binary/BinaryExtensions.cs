using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Buffers.Binary;

namespace Shamisen
{
    /// <summary>
    /// Provides some functions that helps you about binary things.
    /// </summary>
    public static partial class BinaryExtensions
    {
        #region ReverseEndianness

        /// <summary>
        /// Gets the system endianness.
        /// </summary>
        /// <value>
        /// The system endianness.
        /// </value>
        public static Endianness SystemEndianness => BitConverter.IsLittleEndian ? Endianness.Little : Endianness.Big;

        /// <summary>
        /// Reverses internal primitive values by performing an endianness swap of the specified <see cref="Guid"/> <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value to reverse.</param>
        /// <returns>The reversed value.</returns>
        public static Guid ReverseEndianness(Guid value)
        {
            var q = Unsafe.As<Guid, (uint, ushort, ushort, byte, byte, byte, byte, byte, byte, byte, byte)>(ref value);
            q.Item1 = BinaryPrimitives.ReverseEndianness(q.Item1);
            q.Item2 = BinaryPrimitives.ReverseEndianness(q.Item2);
            q.Item3 = BinaryPrimitives.ReverseEndianness(q.Item3);
            return new Guid(q.Item1, q.Item2, q.Item3, q.Item4, q.Item5, q.Item6, q.Item7, q.Item8, q.Item9, q.Item10, q.Item11);
        }

        /// <summary>
        /// Reverses internal primitive values by performing an endianness swap of the specified <see cref="Int24" /> value.
        /// </summary>
        /// <param name="value">The value to reverse.</param>
        /// <returns>
        /// The reversed value.
        /// </returns>
        public static Int24 ReverseEndianness(Int24 value) => Int24.ReverseEndianness(value);

        /// <summary>
        /// Reverses internal primitive values by performing an endianness swap of the specified <see cref="UInt24" /> value.
        /// </summary>
        /// <param name="value">The value to reverse.</param>
        /// <returns>
        /// The reversed value.
        /// </returns>
        public static UInt24 ReverseEndianness(UInt24 value) => UInt24.ReverseEndianness(value);

        #endregion ReverseEndianness

        #region ConvertToLittleEndian

        /// <summary>
        /// Converts the specified <paramref name="systemEndianedValue"/> to/from little endian.
        /// </summary>
        /// <param name="systemEndianedValue">The value in system endian.</param>
        /// <returns>The endian-reversed value if the system is BIG-ENDIAN, otherwise, <paramref name="systemEndianedValue"/>.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static Int24 ConvertToLittleEndian(Int24 systemEndianedValue) => BitConverter.IsLittleEndian ? systemEndianedValue : ReverseEndianness(systemEndianedValue);

        /// <summary>
        /// Converts the specified <paramref name="systemEndianedValue"/> to/from little endian.
        /// </summary>
        /// <param name="systemEndianedValue">The value in system endian.</param>
        /// <returns>The endian-reversed value if the system is BIG-ENDIAN, otherwise, <paramref name="systemEndianedValue"/>.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static UInt24 ConvertToLittleEndian(UInt24 systemEndianedValue) => BitConverter.IsLittleEndian ? systemEndianedValue : ReverseEndianness(systemEndianedValue);

        #endregion ConvertToLittleEndian

        #region ConvertToBigEndian

        /// <summary>
        /// Converts the specified <paramref name="systemEndianedValue"/> to/from BIG ENDIAN.
        /// </summary>
        /// <param name="systemEndianedValue">The value in system endian.</param>
        /// <returns>The endian-reversed value if the system is little-endian, otherwise, <paramref name="systemEndianedValue"/>.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static Int24 ConvertToBigEndian(Int24 systemEndianedValue) => !BitConverter.IsLittleEndian ? systemEndianedValue : ReverseEndianness(systemEndianedValue);

        /// <summary>
        /// Converts the specified <paramref name="systemEndianedValue"/> to/from BIG ENDIAN.
        /// </summary>
        /// <param name="systemEndianedValue">The value in system endian.</param>
        /// <returns>The endian-reversed value if the system is little-endian, otherwise, <paramref name="systemEndianedValue"/>.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static UInt24 ConvertToBigEndian(UInt24 systemEndianedValue) => !BitConverter.IsLittleEndian ? systemEndianedValue : ReverseEndianness(systemEndianedValue);

        #endregion ConvertToBigEndian

#if NET5_0_OR_GREATER

        /// <summary>
        /// Reads a <see cref="double"/> from the beginning of a read-only span of bytes, as little endian.<br/>
        /// This method poly-fills the <see cref="BinaryPrimitives.ReadDoubleLittleEndian(ReadOnlySpan{byte})"/> method for non-supported frameworks.
        /// </summary>
        /// <param name="source">The read-only span of bytes to read.</param>
        /// <param name="value">When this method returns, contains the value read out of the read-only span of bytes, as little endian.</param>
        /// <returns><c>true</c> if the span is large enough to contain a <see cref="double"/>; otherwise, <c>false</c>.</returns>
        /// <seealso cref="BinaryPrimitives.TryReadDoubleLittleEndian(ReadOnlySpan{byte}, out double)"/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool TryReadDoubleLittleEndian(ReadOnlySpan<byte> source, out double value)
            => BinaryPrimitives.TryReadDoubleLittleEndian(source, out value);

#else
        /// <summary>
        /// Reads a <see cref="double"/> from the beginning of a read-only span of bytes, as little endian.<br/>
        /// This method poly-fills the <see cref="BinaryPrimitives"/>.ReadDoubleLittleEndian(ReadOnlySpan{byte}) method for non-supported frameworks.
        /// </summary>
        /// <param name="source">The read-only span of bytes to read.</param>
        /// <param name="value">When this method returns, contains the value read out of the read-only span of bytes, as little endian.</param>
        /// <returns><c>true</c> if the span is large enough to contain a <see cref="double"/>; otherwise, <c>false</c>.</returns>
        /// /// <seealso cref="BinaryPrimitives"/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool TryReadDoubleLittleEndian(ReadOnlySpan<byte> source, out double value)
        {
            var cl = BinaryPrimitives.TryReadUInt64LittleEndian(source, out var rax);   //we can assign ref T value as out parameter...
            value = Unsafe.As<ulong, double>(ref rax);  //assuming ulong and double is stored as same endianness.
            return cl;
        }
#endif

#if NET5_0_OR_GREATER

        /// <summary>
        /// Reads a <see cref="double"/> from the beginning of a read-only span of bytes, as big endian.<br/>
        /// This method poly-fills the <see cref="BinaryPrimitives.ReadDoubleLittleEndian(ReadOnlySpan{byte})"/> method for non-supported frameworks.
        /// </summary>
        /// <param name="source">The read-only span of bytes to read.</param>
        /// <param name="value">When this method returns, contains the value read out of the read-only span of bytes, as Big endian.</param>
        /// <returns><c>true</c> if the span is large enough to contain a <see cref="double"/>; otherwise, <c>false</c>.</returns>
        /// <seealso cref="BinaryPrimitives.TryReadDoubleLittleEndian(ReadOnlySpan{byte}, out double)"/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool TryReadDoubleBigEndian(ReadOnlySpan<byte> source, out double value)
            => BinaryPrimitives.TryReadDoubleBigEndian(source, out value);

#else
        /// <summary>
        /// Reads a <see cref="double"/> from the beginning of a read-only span of bytes, as big endian.<br/>
        /// This method poly-fills the <see cref="BinaryPrimitives"/>.ReadDoubleLittleEndian(ReadOnlySpan{byte}) method for non-supported frameworks.
        /// </summary>
        /// <param name="source">The read-only span of bytes to read.</param>
        /// <param name="value">When this method returns, contains the value read out of the read-only span of bytes, as big endian.</param>
        /// <returns><c>true</c> if the span is large enough to contain a <see cref="double"/>; otherwise, <c>false</c>.</returns>
        /// /// <seealso cref="BinaryPrimitives"/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool TryReadDoubleBigEndian(ReadOnlySpan<byte> source, out double value)
        {
            var cl = BinaryPrimitives.TryReadUInt64BigEndian(source, out var rax);   //we can assign ref T value as out parameter...
            value = Unsafe.As<ulong, double>(ref rax);  //assuming ulong and double is stored as same endianness.
            return cl;
        }
#endif

#if NET5_0_OR_GREATER

        /// <summary>
        /// Reads a <see cref="float"/> from the beginning of a read-only span of bytes, as little endian.
        /// </summary>
        /// <param name="source">The read-only span of bytes to read.</param>
        /// <param name="value">When this method returns, contains the value read out of the read-only span of bytes, as little endian.</param>
        /// <returns><c>true</c> if the span is large enough to contain a <see cref="float"/>; otherwise, <c>false</c>.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool TryReadSingleLittleEndian(ReadOnlySpan<byte> source, out float value)
            => BinaryPrimitives.TryReadSingleLittleEndian(source, out value);

#else
        /// <summary>
        /// Reads a <see cref="float"/> from the beginning of a read-only span of bytes, as little endian.
        /// </summary>
        /// <param name="source">The read-only span of bytes to read.</param>
        /// <param name="value">When this method returns, contains the value read out of the read-only span of bytes, as little endian.</param>
        /// <returns><c>true</c> if the span is large enough to contain a <see cref="float"/>; otherwise, <c>false</c>.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool TryReadSingleLittleEndian(ReadOnlySpan<byte> source, out float value)
        {
            var cl = BinaryPrimitives.TryReadUInt32LittleEndian(source, out var eax);
            value = Unsafe.As<uint, float>(ref eax);  //assuming ulong and double is stored as same endianness.
            return cl;
        }
#endif

#if NET5_0_OR_GREATER

        /// <summary>
        /// Reads a <see cref="float"/> from the beginning of a read-only span of bytes, as big endian.
        /// </summary>
        /// <param name="source">The read-only span of bytes to read.</param>
        /// <param name="value">When this method returns, contains the value read out of the read-only span of bytes, as big endian.</param>
        /// <returns><c>true</c> if the span is large enough to contain a <see cref="float"/>; otherwise, <c>false</c>.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool TryReadSingleBigEndian(ReadOnlySpan<byte> source, out float value)
            => BinaryPrimitives.TryReadSingleBigEndian(source, out value);

#else
        /// <summary>
        /// Reads a <see cref="float"/> from the beginning of a read-only span of bytes, as big endian.
        /// </summary>
        /// <param name="source">The read-only span of bytes to read.</param>
        /// <param name="value">When this method returns, contains the value read out of the read-only span of bytes, as big endian.</param>
        /// <returns><c>true</c> if the span is large enough to contain a <see cref="float"/>; otherwise, <c>false</c>.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool TryReadSingleBigEndian(ReadOnlySpan<byte> source, out float value)
        {
            var cl = BinaryPrimitives.TryReadUInt32BigEndian(source, out var eax);
            value = Unsafe.As<uint, float>(ref eax);  //assuming ulong and double is stored as same endianness.
            return cl;
        }
#endif
#if NETCOREAPP3_1_OR_GREATER

        /// <summary>
        /// Converts a single-precision floating-point value into an integer.
        /// </summary>
        /// <param name="value">The single-precision floating-point value to convert.</param>
        /// <returns>An integer representing the converted single-precision floating-point value.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static int SingleToInt32Bits(float value) => BitConverter.SingleToInt32Bits(value);

        /// <summary>
        /// Reinterprets the specified 32-bit integer as a single-precision floating-point value.
        /// </summary>
        /// <param name="value">The integer to convert.</param>
        /// <returns>A single-precision floating-point value that represents the converted integer.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static float Int32BitsToSingle(int value) => BitConverter.Int32BitsToSingle(value);

#else
        /// <summary>
        /// Converts a single-precision floating-point value into an integer.
        /// </summary>
        /// <param name="value">The single-precision floating-point value to convert.</param>
        /// <returns>An integer representing the converted single-precision floating-point value.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static int SingleToInt32Bits(float value)
        {
            var v = value;
            return Unsafe.As<float, int>(ref v);  //assuming ulong and double is stored as same endianness.
        }

        /// <summary>
        /// Reinterprets the specified 32-bit integer as a single-precision floating-point value.
        /// </summary>
        /// <param name="value">The integer to convert.</param>
        /// <returns>A single-precision floating-point value that represents the converted integer.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static float Int32BitsToSingle(int value)
        {
            var v = value;
            return Unsafe.As<int, float>(ref v);  //assuming ulong and double is stored as same endianness.
        }
#endif
    }
}
