using System;

#if NETCOREAPP3_1_OR_GREATER

using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using AesX86 = System.Runtime.Intrinsics.X86.Aes;

#endif
#if NET5_0_OR_GREATER

using System.Runtime.Intrinsics.Arm;
using AesArm = System.Runtime.Intrinsics.Arm.Aes;

#endif

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace Shamisen.Optimization
{
    /// <summary>
    /// Contains some utilities about Hardware Intrinsics
    /// </summary>
    public static class IntrinsicsUtils
    {
        static IntrinsicsUtils()
        {
#if NETCOREAPP3_1_OR_GREATER
            if (Avx.IsSupported)
            {
                if (!Avx2.IsSupported)  //before Haswell
                {
                    avoidAvxFloatingPoint = true;
                }
                else //after Haswell
                {
#if NET5_0_OR_GREATER
                    var (Eax, Ebx, Ecx, Edx) = X86Base.CpuId(1, 0);
                    if((Eax & 0x0f00) != 0x600)
                    {
                        //Maybe AMD or something
                        avoidAvxFloatingPoint = false;
                    }
                    else
                    {
                        avoidAvxFloatingPoint = (Eax & 0x000f_00f0) switch
                        {
                            0x60040 or 0x50040 or 0xc0030 or 0xf0030 => true,   //Haswell
                            _ => false, //post-Broadwell Intel CPUs
                        };
                    }
#else
                    //There is no way to determine the CPU is post-haswell, so
                    avoidAvxFloatingPoint = false;
#endif
                }
            }
#endif
                }
        /// <summary>
        /// Gets the X86 intrinsics available on this CPU.
        /// </summary>
        /// <value>
        /// The X86 intrinsics.
        /// </value>
        public static X86Intrinsics X86Intrinsics
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get;
        } = GetAvailableX86Intrinsics();

        /// <summary>
        /// Gets the ARM intrinsics available on this CPU.
        /// </summary>
        /// <value>
        /// The arm intrinsics.
        /// </value>
        public static ArmIntrinsics ArmIntrinsics
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get;
        } = GetAvailableArmIntrinsics();

        /// <summary>
        /// Gets the available X86 intrinsics.
        /// </summary>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static X86Intrinsics GetAvailableX86Intrinsics()
        {
            unchecked
            {
#if NETCOREAPP3_1_OR_GREATER
                var res = X86Intrinsics.None;
#if NET5_0_OR_GREATER
                res |= X86Base.IsSupported ? X86Intrinsics.X86Base : 0;
                res |= X86Base.X64.IsSupported ? X86Intrinsics.X64 : 0;
#else
                res |= (Bmi1.X64.IsSupported ||
                        Bmi2.X64.IsSupported ||
                        Lzcnt.X64.IsSupported ||
                        Sse.X64.IsSupported ||
                        Sse2.X64.IsSupported ||
                        Aes.X64.IsSupported ||
                        Pclmulqdq.X64.IsSupported ||
                        Sse3.X64.IsSupported ||
                        Ssse3.X64.IsSupported ||
                        Sse41.X64.IsSupported ||
                        Sse42.X64.IsSupported ||
                        Avx.X64.IsSupported ||
                        Popcnt.X64.IsSupported ||
                        Avx2.X64.IsSupported ||
                        Fma.X64.IsSupported) ? X86Intrinsics.X64 : 0;
#endif
                res |= Bmi1.IsSupported ? X86Intrinsics.Bmi1 : 0;
                res |= Bmi2.IsSupported ? X86Intrinsics.Bmi2 : 0;
                res |= Lzcnt.IsSupported ? X86Intrinsics.Lzcnt : 0;
                res |= Sse.IsSupported ? X86Intrinsics.Sse : 0;
                res |= Sse2.IsSupported ? X86Intrinsics.Sse2 : 0;
                res |= AesX86.IsSupported ? X86Intrinsics.Aes : 0;
                res |= Pclmulqdq.IsSupported ? X86Intrinsics.Pclmulqdq : 0;
                res |= Sse3.IsSupported ? X86Intrinsics.Sse3 : 0;
                res |= Ssse3.IsSupported ? X86Intrinsics.Ssse3 : 0;
                res |= Sse41.IsSupported ? X86Intrinsics.Sse41 : 0;
                res |= Sse42.IsSupported ? X86Intrinsics.Sse42 : 0;
                res |= Avx.IsSupported ? X86Intrinsics.Avx : 0;
                res |= Popcnt.IsSupported ? X86Intrinsics.Popcnt : 0;
                res |= Avx2.IsSupported ? X86Intrinsics.Avx2 : 0;
                res |= Fma.IsSupported ? X86Intrinsics.Fma : 0;
                return res;
#else
                return X86Intrinsics.None;
#endif
            }
        }

        /// <summary>
        /// Gets the available arm intrinsics.
        /// </summary>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static ArmIntrinsics GetAvailableArmIntrinsics()
        {
#if NET5_0_OR_GREATER
            var res = ArmIntrinsics.None;
            res |= ArmBase.IsSupported ? ArmIntrinsics.ArmBase : 0;
            res |= ArmBase.Arm64.IsSupported ? ArmIntrinsics.Arm64 : 0;
            res |= AdvSimd.IsSupported ? ArmIntrinsics.AdvSimd : 0;
            res |= AesArm.IsSupported ? ArmIntrinsics.Aes : 0;
            res |= Crc32.IsSupported ? ArmIntrinsics.Crc32 : 0;
            res |= Sha1.IsSupported ? ArmIntrinsics.Sha1 : 0;
            res |= Sha256.IsSupported ? ArmIntrinsics.Sha256 : 0;
            res |= Dp.IsSupported ? ArmIntrinsics.Dp : 0;
            res |= Rdm.IsSupported ? ArmIntrinsics.Rdm : 0;
            return res;
#else
            return ArmIntrinsics.None;
#endif
        }
        static bool avoidAvxFloatingPoint = false;
        /// <summary>
        /// Gets the value which indicates whether the Shamisen should avoid heavy floating-point operations in 256-bits-wide vectors.
        /// </summary>
        public static bool AvoidAvxFloatingPointOperations => avoidAvxFloatingPoint;

        /// <summary>
        /// Determines whether the specified value has features specified by mask.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="mask">The mask.</param>
        /// <returns>
        ///   <c>true</c> if the specified mask has feature; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool HasAllFeatures(this X86Intrinsics value, X86IntrinsicsMask mask) => (X86IntrinsicsMask)(value & (X86Intrinsics)mask) == mask;

        /// <summary>
        /// Determines whether the specified value has features specified by mask.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="mask">The mask.</param>
        /// <returns>
        ///   <c>true</c> if the specified mask has feature; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool HasAllFeatures(this X86Intrinsics value, X86Intrinsics mask) => (value & mask) == mask;

        /// <summary>
        /// Determines whether the specified value has features specified by mask.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="mask">The mask.</param>
        /// <returns>
        ///   <c>true</c> if the specified mask has feature; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool HasAllFeatures(this ArmIntrinsics value, ArmIntrinsicsMask mask) => (ArmIntrinsicsMask)(value & (ArmIntrinsics)mask) == mask;

        /// <summary>
        /// Determines whether the specified value has features specified by mask.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="mask">The mask.</param>
        /// <returns>
        ///   <c>true</c> if the specified mask has feature; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool HasAllFeatures(this ArmIntrinsics value, ArmIntrinsics mask) => (value & mask) == mask;

        /// <summary>
        /// Determines whether the specified value has features specified by mask.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="mask">The mask.</param>
        /// <returns>
        ///   <c>true</c> if the specified mask has feature; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool HasAtLeaseOneFeature(this X86Intrinsics value, X86IntrinsicsMask mask) => (value & (X86Intrinsics)mask) > 0;

        /// <summary>
        /// Determines whether the specified value has features specified by mask.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="mask">The mask.</param>
        /// <returns>
        ///   <c>true</c> if the specified mask has feature; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool HasAtLeaseOneFeature(this X86Intrinsics value, X86Intrinsics mask) => (value & mask) > 0;

        /// <summary>
        /// Determines whether the specified value has features specified by mask.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="mask">The mask.</param>
        /// <returns>
        ///   <c>true</c> if the specified mask has feature; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool HasAtLeaseOneFeature(this ArmIntrinsics value, ArmIntrinsicsMask mask) => (value & (ArmIntrinsics)mask) > 0;

        /// <summary>
        /// Determines whether the specified value has features specified by mask.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="mask">The mask.</param>
        /// <returns>
        ///   <c>true</c> if the specified mask has feature; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static bool HasAtLeaseOneFeature(this ArmIntrinsics value, ArmIntrinsics mask) => (value & mask) > 0;
    }
}
