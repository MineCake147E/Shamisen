using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System.Runtime.InteropServices;

namespace System
{
    /// <summary>
    /// Provides some extension functions.
    /// </summary>
    public static partial class SpanExtensions
    {
		/// <summary>
        /// Fills the specified memory region faster, with the given <paramref name="value"/>.
        /// </summary>
        /// <param name="span">The span to fill.</param>
        /// <param name="value">The value to be filled.</param>
        public static void FastFill(this Span<float> span, float value = default)
        {
            if(Vector<float>.Count > span.Length)
			{
				span.Fill(value);
			}
			else
			{
				var spanV = MemoryMarshal.Cast<float, Vector<float>>(span);
				spanV.Fill(new Vector<float>(value));
				var spanR = span.Slice(spanV.Length * Vector<float>.Count);
				spanR.Fill(value);
			}
        }
		/// <summary>
        /// Fills the specified memory region faster, with the given <paramref name="value"/>.
        /// </summary>
        /// <param name="span">The span to fill.</param>
        /// <param name="value">The value to be filled.</param>
        public static void FastFill(this Span<double> span, double value = default)
        {
            if(Vector<double>.Count > span.Length)
			{
				span.Fill(value);
			}
			else
			{
				var spanV = MemoryMarshal.Cast<double, Vector<double>>(span);
				spanV.Fill(new Vector<double>(value));
				var spanR = span.Slice(spanV.Length * Vector<double>.Count);
				spanR.Fill(value);
			}
        }
		/// <summary>
        /// Fills the specified memory region faster, with the given <paramref name="value"/>.
        /// </summary>
        /// <param name="span">The span to fill.</param>
        /// <param name="value">The value to be filled.</param>
        public static void FastFill(this Span<byte> span, byte value = default)
        {
            if(Vector<byte>.Count > span.Length)
			{
				span.Fill(value);
			}
			else
			{
				var spanV = MemoryMarshal.Cast<byte, Vector<byte>>(span);
				spanV.Fill(new Vector<byte>(value));
				var spanR = span.Slice(spanV.Length * Vector<byte>.Count);
				spanR.Fill(value);
			}
        }
		/// <summary>
        /// Fills the specified memory region faster, with the given <paramref name="value"/>.
        /// </summary>
        /// <param name="span">The span to fill.</param>
        /// <param name="value">The value to be filled.</param>
        public static void FastFill(this Span<ushort> span, ushort value = default)
        {
            if(Vector<ushort>.Count > span.Length)
			{
				span.Fill(value);
			}
			else
			{
				var spanV = MemoryMarshal.Cast<ushort, Vector<ushort>>(span);
				spanV.Fill(new Vector<ushort>(value));
				var spanR = span.Slice(spanV.Length * Vector<ushort>.Count);
				spanR.Fill(value);
			}
        }
		/// <summary>
        /// Fills the specified memory region faster, with the given <paramref name="value"/>.
        /// </summary>
        /// <param name="span">The span to fill.</param>
        /// <param name="value">The value to be filled.</param>
        public static void FastFill(this Span<uint> span, uint value = default)
        {
            if(Vector<uint>.Count > span.Length)
			{
				span.Fill(value);
			}
			else
			{
				var spanV = MemoryMarshal.Cast<uint, Vector<uint>>(span);
				spanV.Fill(new Vector<uint>(value));
				var spanR = span.Slice(spanV.Length * Vector<uint>.Count);
				spanR.Fill(value);
			}
        }
		/// <summary>
        /// Fills the specified memory region faster, with the given <paramref name="value"/>.
        /// </summary>
        /// <param name="span">The span to fill.</param>
        /// <param name="value">The value to be filled.</param>
        public static void FastFill(this Span<ulong> span, ulong value = default)
        {
            if(Vector<ulong>.Count > span.Length)
			{
				span.Fill(value);
			}
			else
			{
				var spanV = MemoryMarshal.Cast<ulong, Vector<ulong>>(span);
				spanV.Fill(new Vector<ulong>(value));
				var spanR = span.Slice(spanV.Length * Vector<ulong>.Count);
				spanR.Fill(value);
			}
        }
		/// <summary>
        /// Fills the specified memory region faster, with the given <paramref name="value"/>.
        /// </summary>
        /// <param name="span">The span to fill.</param>
        /// <param name="value">The value to be filled.</param>
        public static void FastFill(this Span<sbyte> span, sbyte value = default)
        {
            if(Vector<sbyte>.Count > span.Length)
			{
				span.Fill(value);
			}
			else
			{
				var spanV = MemoryMarshal.Cast<sbyte, Vector<sbyte>>(span);
				spanV.Fill(new Vector<sbyte>(value));
				var spanR = span.Slice(spanV.Length * Vector<sbyte>.Count);
				spanR.Fill(value);
			}
        }
		/// <summary>
        /// Fills the specified memory region faster, with the given <paramref name="value"/>.
        /// </summary>
        /// <param name="span">The span to fill.</param>
        /// <param name="value">The value to be filled.</param>
        public static void FastFill(this Span<short> span, short value = default)
        {
            if(Vector<short>.Count > span.Length)
			{
				span.Fill(value);
			}
			else
			{
				var spanV = MemoryMarshal.Cast<short, Vector<short>>(span);
				spanV.Fill(new Vector<short>(value));
				var spanR = span.Slice(spanV.Length * Vector<short>.Count);
				spanR.Fill(value);
			}
        }
		/// <summary>
        /// Fills the specified memory region faster, with the given <paramref name="value"/>.
        /// </summary>
        /// <param name="span">The span to fill.</param>
        /// <param name="value">The value to be filled.</param>
        public static void FastFill(this Span<int> span, int value = default)
        {
            if(Vector<int>.Count > span.Length)
			{
				span.Fill(value);
			}
			else
			{
				var spanV = MemoryMarshal.Cast<int, Vector<int>>(span);
				spanV.Fill(new Vector<int>(value));
				var spanR = span.Slice(spanV.Length * Vector<int>.Count);
				spanR.Fill(value);
			}
        }
		/// <summary>
        /// Fills the specified memory region faster, with the given <paramref name="value"/>.
        /// </summary>
        /// <param name="span">The span to fill.</param>
        /// <param name="value">The value to be filled.</param>
        public static void FastFill(this Span<long> span, long value = default)
        {
            if(Vector<long>.Count > span.Length)
			{
				span.Fill(value);
			}
			else
			{
				var spanV = MemoryMarshal.Cast<long, Vector<long>>(span);
				spanV.Fill(new Vector<long>(value));
				var spanR = span.Slice(spanV.Length * Vector<long>.Count);
				spanR.Fill(value);
			}
        }
    }
}
