#if NET5_0_OR_GREATER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Utils.Intrinsics
{
    /// <summary>
    /// Contains some utility functions for <see cref="AdvSimd"/>.
    /// </summary>
    public static partial class AdvSimdUtils
    {
        /// <inheritdoc cref="AdvSimd.IsSupported"/>
        public static bool IsSupported => AdvSimd.IsSupported;
        /// <summary>
        /// Contains some utility functions for <see cref="AdvSimd.Arm64"/>.
        /// </summary>
        public static partial class Arm64
        {
            /// <inheritdoc cref="AdvSimd.Arm64.IsSupported"/>
            public static bool IsSupported => AdvSimd.Arm64.IsSupported;
        }
    }
}

#endif