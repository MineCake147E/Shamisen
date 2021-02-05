using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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
        public static Span<TTo> CastSplit<TFrom, TTo>(Span<TFrom> span, out Span<TFrom> residue)
            where TFrom : struct
            where TTo : struct
        {
            var res = MemoryMarshal.Cast<TFrom, TTo>(span);
            var resLen = Unsafe.SizeOf<TTo>() * res.Length / Unsafe.SizeOf<TFrom>();
            residue = span.Slice(resLen);
            return res;
        }
    }
}
