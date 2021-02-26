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
        /// <summary>
        /// Converts the specified <paramref name="systemEndianedValue"/> to little endian.
        /// </summary>
        /// <param name="systemEndianedValue">The value in system endian.</param>
        /// <returns>The little endianed value.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static ushort ConvertToLittleEndian(ushort systemEndianedValue)
            => BitConverter.IsLittleEndian ? systemEndianedValue : BinaryPrimitives.ReverseEndianness(systemEndianedValue);

        /// <summary>
        /// Converts the specified <paramref name="systemEndianedValue"/> to little endian.
        /// </summary>
        /// <param name="systemEndianedValue">The value in system endian.</param>
        /// <returns>The little endianed value.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static short ConvertToLittleEndian(short systemEndianedValue)
            => BitConverter.IsLittleEndian ? systemEndianedValue : BinaryPrimitives.ReverseEndianness(systemEndianedValue);

        /// <summary>
        /// Converts the specified <paramref name="systemEndianedValue"/> to little endian.
        /// </summary>
        /// <param name="systemEndianedValue">The value in system endian.</param>
        /// <returns>The little endianed value.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static uint ConvertToLittleEndian(uint systemEndianedValue)
            => BitConverter.IsLittleEndian ? systemEndianedValue : BinaryPrimitives.ReverseEndianness(systemEndianedValue);

        /// <summary>
        /// Converts the specified <paramref name="systemEndianedValue"/> to little endian.
        /// </summary>
        /// <param name="systemEndianedValue">The value in system endian.</param>
        /// <returns>The little endianed value.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static int ConvertToLittleEndian(int systemEndianedValue)
            => BitConverter.IsLittleEndian ? systemEndianedValue : BinaryPrimitives.ReverseEndianness(systemEndianedValue);

        /// <summary>
        /// Converts the specified <paramref name="systemEndianedValue"/> to little endian.
        /// </summary>
        /// <param name="systemEndianedValue">The value in system endian.</param>
        /// <returns>The little endianed value.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static ulong ConvertToLittleEndian(ulong systemEndianedValue)
            => BitConverter.IsLittleEndian ? systemEndianedValue : BinaryPrimitives.ReverseEndianness(systemEndianedValue);

        /// <summary>
        /// Converts the specified <paramref name="systemEndianedValue"/> to little endian.
        /// </summary>
        /// <param name="systemEndianedValue">The value in system endian.</param>
        /// <returns>The little endianed value.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static long ConvertToLittleEndian(long systemEndianedValue)
            => BitConverter.IsLittleEndian ? systemEndianedValue : BinaryPrimitives.ReverseEndianness(systemEndianedValue);


        /// <summary>
        /// Converts the specified <paramref name="systemEndianedValue"/> to little endian.
        /// </summary>
        /// <param name="systemEndianedValue">The value in system endian.</param>
        /// <returns>The little endianed value.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static ushort ConvertToBigEndian(ushort systemEndianedValue)
            => !BitConverter.IsLittleEndian ? systemEndianedValue : BinaryPrimitives.ReverseEndianness(systemEndianedValue);

        /// <summary>
        /// Converts the specified <paramref name="systemEndianedValue"/> to little endian.
        /// </summary>
        /// <param name="systemEndianedValue">The value in system endian.</param>
        /// <returns>The little endianed value.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static short ConvertToBigEndian(short systemEndianedValue)
            => !BitConverter.IsLittleEndian ? systemEndianedValue : BinaryPrimitives.ReverseEndianness(systemEndianedValue);

        /// <summary>
        /// Converts the specified <paramref name="systemEndianedValue"/> to little endian.
        /// </summary>
        /// <param name="systemEndianedValue">The value in system endian.</param>
        /// <returns>The little endianed value.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static uint ConvertToBigEndian(uint systemEndianedValue)
            => !BitConverter.IsLittleEndian ? systemEndianedValue : BinaryPrimitives.ReverseEndianness(systemEndianedValue);

        /// <summary>
        /// Converts the specified <paramref name="systemEndianedValue"/> to little endian.
        /// </summary>
        /// <param name="systemEndianedValue">The value in system endian.</param>
        /// <returns>The little endianed value.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static int ConvertToBigEndian(int systemEndianedValue)
            => !BitConverter.IsLittleEndian ? systemEndianedValue : BinaryPrimitives.ReverseEndianness(systemEndianedValue);

        /// <summary>
        /// Converts the specified <paramref name="systemEndianedValue"/> to little endian.
        /// </summary>
        /// <param name="systemEndianedValue">The value in system endian.</param>
        /// <returns>The little endianed value.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static ulong ConvertToBigEndian(ulong systemEndianedValue)
            => !BitConverter.IsLittleEndian ? systemEndianedValue : BinaryPrimitives.ReverseEndianness(systemEndianedValue);

        /// <summary>
        /// Converts the specified <paramref name="systemEndianedValue"/> to little endian.
        /// </summary>
        /// <param name="systemEndianedValue">The value in system endian.</param>
        /// <returns>The little endianed value.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static long ConvertToBigEndian(long systemEndianedValue)
            => !BitConverter.IsLittleEndian ? systemEndianedValue : BinaryPrimitives.ReverseEndianness(systemEndianedValue);

    }
}
