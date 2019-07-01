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
				Vector<float> filler = new Vector<float>(value);
				var spanV = MemoryMarshal.Cast<float, Vector<float>>(span);
				for (int i = 0; i < spanV.Length; i++)
				{
				    spanV[i] = filler;
				}
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
				Vector<double> filler = new Vector<double>(value);
				var spanV = MemoryMarshal.Cast<double, Vector<double>>(span);
				for (int i = 0; i < spanV.Length; i++)
				{
				    spanV[i] = filler;
				}
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
				Vector<byte> filler = new Vector<byte>(value);
				var spanV = MemoryMarshal.Cast<byte, Vector<byte>>(span);
				for (int i = 0; i < spanV.Length; i++)
				{
				    spanV[i] = filler;
				}
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
				Vector<ushort> filler = new Vector<ushort>(value);
				var spanV = MemoryMarshal.Cast<ushort, Vector<ushort>>(span);
				for (int i = 0; i < spanV.Length; i++)
				{
				    spanV[i] = filler;
				}
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
				Vector<uint> filler = new Vector<uint>(value);
				var spanV = MemoryMarshal.Cast<uint, Vector<uint>>(span);
				for (int i = 0; i < spanV.Length; i++)
				{
				    spanV[i] = filler;
				}
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
				Vector<ulong> filler = new Vector<ulong>(value);
				var spanV = MemoryMarshal.Cast<ulong, Vector<ulong>>(span);
				for (int i = 0; i < spanV.Length; i++)
				{
				    spanV[i] = filler;
				}
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
				Vector<sbyte> filler = new Vector<sbyte>(value);
				var spanV = MemoryMarshal.Cast<sbyte, Vector<sbyte>>(span);
				for (int i = 0; i < spanV.Length; i++)
				{
				    spanV[i] = filler;
				}
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
				Vector<short> filler = new Vector<short>(value);
				var spanV = MemoryMarshal.Cast<short, Vector<short>>(span);
				for (int i = 0; i < spanV.Length; i++)
				{
				    spanV[i] = filler;
				}
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
				Vector<int> filler = new Vector<int>(value);
				var spanV = MemoryMarshal.Cast<int, Vector<int>>(span);
				for (int i = 0; i < spanV.Length; i++)
				{
				    spanV[i] = filler;
				}
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
				Vector<long> filler = new Vector<long>(value);
				var spanV = MemoryMarshal.Cast<long, Vector<long>>(span);
				for (int i = 0; i < spanV.Length; i++)
				{
				    spanV[i] = filler;
				}
				var spanR = span.Slice(spanV.Length * Vector<long>.Count);
				spanR.Fill(value);
			}
        }
    }
}
