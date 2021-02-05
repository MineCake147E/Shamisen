using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Optimization
{
    /// <summary>
    /// Defines some X86 intrinsics.
    /// </summary>
    [Flags]
    public enum X86Intrinsics : ulong
    {
        /// <summary>
        /// Not X86(e.g. ARMv8-A) or X86 Intrinsics aren't available
        /// </summary>
        None = 0,

        /// <summary>
        /// X86Base
        /// </summary>
        X86Base = 0x1,

        /// <summary>
        /// X64 variant of each intrinsics
        /// </summary>
        X64 = 0x2,

        /// <summary>
        /// BMI1
        /// </summary>
        Bmi1 = 0x4,

        /// <summary>
        /// BMI2
        /// </summary>
        Bmi2 = 0x8,

        /// <summary>
        /// LZCNT
        /// </summary>
        Lzcnt = 0x10,

        /// <summary>
        /// SSE
        /// </summary>
        Sse = 0x20,

        /// <summary>
        /// SSE2
        /// </summary>
        Sse2 = 0x40,

        /// <summary>
        /// AES
        /// </summary>
        Aes = 0x80,

        /// <summary>
        /// PCLMULQDQ
        /// </summary>
        Pclmulqdq = 0x100,

        /// <summary>
        /// SSE3
        /// </summary>
        Sse3 = 0x200,

        /// <summary>
        /// SSSE3
        /// </summary>
        Ssse3 = 0x400,

        /// <summary>
        /// SSE4.1
        /// </summary>
        Sse41 = 0x800,

        /// <summary>
        /// SSE4.2
        /// </summary>
        Sse42 = 0x1000,

        /// <summary>
        /// AVX
        /// </summary>
        Avx = 0x2000,

        /// <summary>
        /// POPCNT
        /// </summary>
        Popcnt = 0x4000,

        /// <summary>
        /// AVX2
        /// </summary>
        Avx2 = 0x8000,

        /// <summary>
        /// FMA
        /// </summary>
        Fma = 0x1_0000,
    }

    /// <summary>
    /// Defines some ARM intrinsics.
    /// </summary>
    [Flags]
    public enum ArmIntrinsics : ulong
    {
        /// <summary>
        /// Not ARM(e.g. x86-64) or ARM Intrinsics aren't available
        /// </summary>
        None = 0,

        /// <summary>
        /// ArmBase
        /// </summary>
        ArmBase = 0x1,

        /// <summary>
        /// Arm64 variant of each intrinsics
        /// </summary>
        Arm64 = 0x2,

        /// <summary>
        /// AdvSIMD
        /// </summary>
        AdvSimd = 0x4,

        /// <summary>
        /// AES
        /// </summary>
        Aes = 0x8,

        /// <summary>
        /// CRC32
        /// </summary>
        Crc32 = 0x10,

        /// <summary>
        /// SHA1
        /// </summary>
        Sha1 = 0x20,

        /// <summary>
        /// SHA256
        /// </summary>
        Sha256 = 0x40,

        /// <summary>
        /// DotProduct
        /// </summary>
        Dp = 0x80,

        /// <summary>
        /// MultiplyRoundedDoubling
        /// </summary>
        Rdm = 0x100,
    }
}
