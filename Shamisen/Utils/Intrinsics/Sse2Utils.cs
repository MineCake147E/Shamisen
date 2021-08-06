#if NETCOREAPP3_1_OR_GREATER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Utils.Intrinsics
{
    /// <summary>
    /// Contains some utility functions for SSE2.
    /// </summary>
    public static class Sse2Utils
    {
        /// <summary>
        /// Gets the value which indicates whether the <see cref="Sse2Utils"/> can be used in this machine.
        /// </summary>
        public static bool IsSupported => Sse2.IsSupported;

    }
}

#endif