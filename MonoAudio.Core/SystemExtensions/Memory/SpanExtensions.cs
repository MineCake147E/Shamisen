using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace System
{
    /// <summary>
    /// Provides some extension functions.
    /// </summary>
    public static partial class SpanExtensions
    {
        #region SliceAlign

        /// <summary>
        /// Slices the <paramref name="span"/> aligned with <paramref name="Align"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="span">The span.</param>
        /// <param name="Align">The align width.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> SliceAlign<T>(this Span<T> span, int Align) => span.Slice(0, MathI.FloorStep(span.Length, Align));

        /// <summary>
        /// Slices the <paramref name="span"/> aligned with <paramref name="Align"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="span">The span.</param>
        /// <param name="Align">The align width.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<T> SliceAlign<T>(this ReadOnlySpan<T> span, int Align) => span.Slice(0, MathI.FloorStep(span.Length, Align));

        /// <summary>
        /// Slices the <paramref name="memory"/> aligned with <paramref name="Align"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="memory">The memory.</param>
        /// <param name="Align">The align width.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Memory<T> SliceAlign<T>(this Memory<T> memory, int Align) => memory.Slice(0, MathI.FloorStep(memory.Length, Align));

        /// <summary>
        /// Slices the <paramref name="memory"/> aligned with <paramref name="Align"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="memory">The memory.</param>
        /// <param name="Align">The align width.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<T> SliceAlign<T>(this ReadOnlyMemory<T> memory, int Align) => memory.Slice(0, MathI.FloorStep(memory.Length, Align));

        #endregion SliceAlign

        #region SliceWhile

        /// <summary>
        /// Slices the <paramref name="span"/> with the specified length.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="span">The span to slice.</param>
        /// <param name="length">The length to read.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> SliceWhile<T>(this Span<T> span, int length) => span.Slice(0, length);

        /// <summary>
        /// Slices the <paramref name="span"/> with the specified length.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="span">The span to slice.</param>
        /// <param name="length">The length to read.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<T> SliceWhile<T>(this ReadOnlySpan<T> span, int length) => span.Slice(0, length);

        #endregion SliceWhile

        /// <summary>
        /// Adds the <paramref name="samplesToAdd"/> to <paramref name="buffer"/>.
        /// </summary>
        /// <param name="samplesToAdd">The samples to add.</param>
        /// <param name="buffer">The buffer.</param>
        /// <exception cref="ArgumentException">samplesToAdd</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FastAdd(ReadOnlySpan<float> samplesToAdd, Span<float> buffer)
        {
            if (samplesToAdd.Length > buffer.Length) throw new ArgumentException("", nameof(samplesToAdd));
            unsafe
            {
                (int newLength, int remainder) = MathI.FloorStepRem(samplesToAdd.Length, Vector<float>.Count);
                if (newLength != 0)
                {
                    var src = MemoryMarshal.Cast<float, Vector<float>>(samplesToAdd);
                    var dst = MemoryMarshal.Cast<float, Vector<float>>(buffer);
                    for (int i = 0; i < src.Length; i++)
                    {
                        dst[i] += src[i];
                    }
                }
                if (remainder != 0)
                {
                    var srcRem = samplesToAdd.Slice(newLength);
                    var dstRem = buffer.Slice(newLength);
                    for (int i = 0; i < srcRem.Length; i++)
                    {
                        dstRem[i] += srcRem[i];
                    }
                }
            }
        }

        /// <summary>
        /// Multiplies the specified samples faster, with the given <paramref name="scale"/>.
        /// </summary>
        /// <param name="span">The span to multiply.</param>
        /// <param name="scale">The value to be multiplied.</param>
        public static void FastScalarMultiply(this Span<float> span, float scale = default)
        {
            if (Vector<float>.Count > span.Length)
            {
                for (int i = 0; i < span.Length; i++)
                {
                    span[i] *= scale;
                }
            }
            else
            {
                var spanV = MemoryMarshal.Cast<float, Vector<float>>(span);
                var scaleV = new Vector<float>(scale);
                for (int i = 0; i < spanV.Length; i++)
                {
                    spanV[i] *= scaleV;
                }
                var spanR = span.Slice(spanV.Length * Vector<float>.Count);
                for (int i = 0; i < spanR.Length; i++)
                {
                    spanR[i] *= scale;
                }
            }
        }

        /// <summary>
        /// Mixes the <paramref name="samplesToMix"/> to <paramref name="buffer"/>.
        /// </summary>
        /// <param name="samplesToMix">The samples to add.</param>
        /// <param name="buffer">The buffer.</param>
        /// <param name="scale">The scale to scale <paramref name="samplesToMix"/>.</param>
        /// <exception cref="ArgumentException">samplesToMix</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FastMix(ReadOnlySpan<float> samplesToMix, Span<float> buffer, float scale)
        {
            if (samplesToMix.Length > buffer.Length) throw new ArgumentException("", nameof(samplesToMix));
            unsafe
            {
                (int newLength, int remainder) = MathI.FloorStepRem(samplesToMix.Length, Vector<float>.Count);
                if (newLength != 0)
                {
                    var scaleV = new Vector<float>(scale);
                    var src = MemoryMarshal.Cast<float, Vector<float>>(samplesToMix);
                    var dst = MemoryMarshal.Cast<float, Vector<float>>(buffer);
                    for (int i = 0; i < src.Length; i++)
                    {
                        dst[i] += scaleV * src[i];
                    }
                }
                if (remainder != 0)
                {
                    var srcRem = samplesToMix.Slice(newLength);
                    var dstRem = buffer.Slice(newLength);
                    for (int i = 0; i < srcRem.Length; i++)
                    {
                        dstRem[i] += srcRem[i] * scale;
                    }
                }
            }
        }
    }
}
