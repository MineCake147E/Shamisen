using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using Shamisen.Optimization;
#if NET5_0_OR_GREATER

using System.Runtime.Intrinsics.Arm;

#endif
#if NETCOREAPP3_1_OR_GREATER

using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

using Shamisen.Utils.Intrinsics;

#endif

namespace Shamisen.Utils
{
    /// <summary>
    /// Contains <see cref="Math"/>-like functions for <see cref="Vector{T}"/> and its close relatives.
    /// </summary>
    public static class MathV
    {
    }
}
