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
        /// <summary>
        /// Slices the <paramref name="span"/> aligned with <paramref name="Align"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="span">The span.</param>
        /// <param name="Align">The align width.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> SliceAlign<T>(this Span<T> span, int Align) => span.Slice(0, MathI.FloorStep(span.Length, Align));
    }
}
