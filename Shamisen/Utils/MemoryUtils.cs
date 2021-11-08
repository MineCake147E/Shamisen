using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen
{
    /// <summary>
    /// Provides useful methods to interoperate with <see cref="Span{T}"/> and <see cref="Memory{T}"/>.
    /// </summary>
    public static class MemoryUtils
    {
        /// <summary>
        /// Casts and splits the specified span.
        /// </summary>
        /// <typeparam name="TFrom">The type of from.</typeparam>
        /// <typeparam name="TTo">The type of to.</typeparam>
        /// <param name="span">The span.</param>
        /// <param name="residue">The residue part of <paramref name="span"/></param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static Span<TTo> CastSplit<TFrom, TTo>(Span<TFrom> span, out Span<TFrom> residue)
            where TFrom : struct
            where TTo : struct
        {
            var res = MemoryMarshal.Cast<TFrom, TTo>(span);
            var resLen = Unsafe.SizeOf<TTo>() * res.Length / Unsafe.SizeOf<TFrom>();
            residue = span.Slice(resLen);
            return res;
        }

        /// <summary>
        /// Casts and splits the specified span.
        /// </summary>
        /// <typeparam name="TFrom">The type of from.</typeparam>
        /// <typeparam name="TTo">The type of to.</typeparam>
        /// <param name="span">The span.</param>
        /// <param name="residue">The residue part of <paramref name="span"/></param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static ReadOnlySpan<TTo> CastSplit<TFrom, TTo>(ReadOnlySpan<TFrom> span, out ReadOnlySpan<TFrom> residue)
            where TFrom : struct
            where TTo : struct
        {
            var res = MemoryMarshal.Cast<TFrom, TTo>(span);
            var resLen = Unsafe.SizeOf<TTo>() * res.Length / Unsafe.SizeOf<TFrom>();
            residue = span.Slice(resLen);
            return res;
        }

        /// <summary>
        /// Deconstructs the <see cref="Nullable{T}"/> with bool and <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value to deconstruct.</param>
        /// <param name="hasValue">The value which indicates whether the <paramref name="result"/> is valid.</param>
        /// <param name="result">The result.</param>
        public static void Deconstruct<T>(this T? value, out bool hasValue, out T result) where T : struct
        {
            hasValue = value.HasValue;
            result = value.GetValueOrDefault();
        }
    }
}
