using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen
{
    /// <summary>
    /// Contains debug utilities.
    /// </summary>
    internal static partial class DebugUtils
    {
        /// <summary>
        /// Equivalent to <see cref="Console.WriteLine(string?)"/> but executed only in Debug mode.
        /// </summary>
        /// <param name="a"></param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void WriteLine(string a)
        {
            unchecked
            {
                //
#if DEBUG
                Console.WriteLine(a);
#endif
            }
        }

        internal static string DumpBinary(ReadOnlySpan<byte> data)
        {
            var sb = new StringBuilder();
            int i;
            var a = new byte[16];
            for (i = 0; i < data.Length - 15; i += 16)
            {
                _ = sb.Append($"{i:X16}: ");
                data.Slice(i, 16).CopyTo(a);
                _ = sb.AppendLine(string.Join(" ", a.Select(g => $"{g:X02}")));
            }
            if (i < data.Length)
            {
                _ = sb.Append($"{i:X16}: ");
                a.AsSpan().Clear();
                data.Slice(i).CopyTo(a);
                _ = sb.AppendLine(string.Join(" ", a.Take(data.Length - i).Select(g => $"{g:X02}")));
            }
            return sb.ToString();
        }
    }
}
