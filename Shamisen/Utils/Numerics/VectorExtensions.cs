using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

#if NETCOREAPP3_1_OR_GREATER

using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

#endif
#if NET5_0_OR_GREATER

using System.Runtime.Intrinsics.Arm;

#endif

namespace Shamisen
{
    /// <summary>
    /// Contains some utility functions for <see cref="Vector{T}" /> and some its close relatives.
    /// </summary>
    public static class VectorExtensions
    {
#if NETCOREAPP3_1_OR_GREATER

        /// <summary>
        ///
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <param name="v3"></param>
        public static void Deconstruct(this Vector256<ulong> vector, out ulong v0, out ulong v1, out ulong v2, out ulong v3)
        {
            v0 = vector.GetElement(0);
            v1 = vector.GetElement(1);
            v2 = vector.GetElement(2);
            v3 = vector.GetElement(3);
        }

#endif
    }
}
