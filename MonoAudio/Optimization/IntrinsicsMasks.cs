using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoAudio.Optimization
{
    /// <summary>
    /// Defines some <see cref="X86Intrinsics"/> masks.
    /// </summary>
    public enum X86IntrinsicsMask : ulong
    {
        /// <summary>
        /// The none
        /// </summary>
        None = 0,

        /// <summary>
        /// X86Base
        /// </summary>
        X86Base = X86Intrinsics.X86Base,

        /// <summary>
        /// X64 variant of each intrinsics
        /// </summary>
        X64 = X86Intrinsics.X64,

        /// <summary>
        /// BMI1
        /// </summary>
        Bmi1 = X86Intrinsics.Bmi1,

        /// <summary>
        /// BMI2
        /// </summary>
        Bmi2 = X86Intrinsics.Bmi2,

        /// <summary>
        /// LZCNT
        /// </summary>
        Lzcnt = X86Intrinsics.Lzcnt,

        /// <summary>
        /// SSE
        /// </summary>
        Sse = X86Intrinsics.Sse,

        /// <summary>
        /// SSE2
        /// </summary>
        Sse2 = X86Intrinsics.Sse2 | Sse,

        /// <summary>
        /// AES
        /// </summary>
        Aes = X86Intrinsics.Aes | Sse2,

        /// <summary>
        /// PCLMULQDQ
        /// </summary>
        Pclmulqdq = X86Intrinsics.Pclmulqdq | Sse2,

        /// <summary>
        /// SSE3
        /// </summary>
        Sse3 = X86Intrinsics.Sse3 | Sse2,

        /// <summary>
        /// SSSE3
        /// </summary>
        Ssse3 = X86Intrinsics.Ssse3 | Sse3,

        /// <summary>
        /// SSE4.1
        /// </summary>
        Sse41 = X86Intrinsics.Sse41 | Ssse3,

        /// <summary>
        /// SSE4.2
        /// </summary>
        Sse42 = X86Intrinsics.Sse42 | Sse41,

        /// <summary>
        /// AVX
        /// </summary>
        Avx = X86Intrinsics.Avx | Sse42,

        /// <summary>
        /// POPCNT
        /// </summary>
        Popcnt = X86Intrinsics.Popcnt | Sse42,

        /// <summary>
        /// AVX2
        /// </summary>
        Avx2 = X86Intrinsics.Avx2 | Avx,

        /// <summary>
        /// FMA
        /// </summary>
        Fma = X86Intrinsics.Fma | Avx,
    }

    /// <summary>
    /// Defines some <see cref="X86Intrinsics"/> masks.
    /// </summary>
    public enum ArmIntrinsicsMask : ulong
    {
        /// <summary>
        /// Not ARM(e.g. x86-64) or ARM Intrinsics aren't available
        /// </summary>
        None = 0,

        /// <summary>
        /// ArmBase
        /// </summary>
        ArmBase = ArmIntrinsics.ArmBase,

        /// <summary>
        /// Arm64 variant of each intrinsics
        /// </summary>
        Arm64 = ArmIntrinsics.Arm64 | ArmBase,

        /// <summary>
        /// AdvSIMD
        /// </summary>
        AdvSimd = ArmIntrinsics.AdvSimd | ArmBase,

        /// <summary>
        /// AES
        /// </summary>
        Aes = ArmIntrinsics.Aes | ArmBase,

        /// <summary>
        /// CRC32
        /// </summary>
        Crc32 = ArmIntrinsics.Crc32 | ArmBase,

        /// <summary>
        /// SHA1
        /// </summary>
        Sha1 = ArmIntrinsics.Sha1 | ArmBase,

        /// <summary>
        /// SHA256
        /// </summary>
        Sha256 = ArmIntrinsics.Sha256 | ArmBase,

        /// <summary>
        /// DotProduct
        /// </summary>
        Dp = ArmIntrinsics.Dp | AdvSimd,

        /// <summary>
        /// MultiplyRoundedDoubling
        /// </summary>
        Rdm = ArmIntrinsics.Rdm | AdvSimd,
    }
}
