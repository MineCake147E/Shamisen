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
    }
}
