#if NETCOREAPP3_1_OR_GREATER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Utils.Intrinsics
{
    /// <summary>
    /// Contains some utility functions for <see cref="Avx2"/>.
    /// </summary>
    public static class Avx2Utils
    {
        /// <summary>
        /// Gets the value which indicates whether the <see cref="Avx2Utils"/> can be used in this machine.
        /// </summary>
        public static bool IsSupported => Avx2.IsSupported;

        /// <inheritdoc cref="Avx.BlendVariable(Vector256{float}, Vector256{float}, Vector256{float})"/>
        public static Vector256<int> BlendVariable(Vector256<int> left, Vector256<int> right, Vector256<int> mask) => Avx.BlendVariable(left.AsSingle(), right.AsSingle(), mask.AsSingle()).AsInt32();
    }
}
#endif