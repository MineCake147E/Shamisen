﻿#if NET || NETSTANDARD || NETCOREAPP
// <auto-generated />
using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Shamisen.Utils;
using Shamisen.Utils.Numerics;
#endif
#if NETCOREAPP3_1_OR_GREATER
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

#endif
#if NET5_0_OR_GREATER
using System.Runtime.Intrinsics.Arm;
#endif
namespace Shamisen.Conversion.Resampling.Sample
{
    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct CachedResampleArgs
    {
        readonly Vector4Int32 values;
        public int ConversionGradient
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get
            {
                return values.X;
            }
        }

        public int RateMul
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get
            {
                return values.Y;
            }
        }

        public int GradientIncrement
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get
            {
                return values.Z;
            }
        }

        public int IndexIncrement
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get
            {
                return values.W;
            }
        }
        public CachedResampleArgs(int x, int ram, int acc, int facc)
        {
            values = new(x, ram, acc, facc);
        }
    }

    [StructLayout(LayoutKind.Explicit, Size = 4 * sizeof(ulong))]
    internal readonly struct UnifiedResampleArgs
    {
        [FieldOffset(0)]
        private readonly ulong xrci;
        [FieldOffset(1 * sizeof(ulong))]
        private readonly ulong ramrcd;
        [FieldOffset(2 * sizeof(ulong))]
        private readonly ulong accch;
        [FieldOffset(3 * sizeof(ulong))]
        private readonly int facc;
        [FieldOffset(7 * sizeof(float))]
        private readonly float rmi;

        /// <summary>
        /// First member of <see cref="UnifiedResampleArgs"/>.
        /// </summary>
        public int ConversionGradient
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => (int)xrci;
        }
        /// <summary>
        /// Second member of <see cref="UnifiedResampleArgs"/>. Tied with <see cref="ConversionGradient"/>.
        /// </summary>
        public int RearrangedCoeffsIndex
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => (int)(xrci >> 32);
        }
        /// <summary>
        /// Third member of <see cref="UnifiedResampleArgs"/>.
        /// </summary>
        public int RateMul
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => (int)ramrcd;
        }
        /// <summary>
        /// 4th member of <see cref="UnifiedResampleArgs"/>. Tied with <see cref="RateMul"/>.
        /// </summary>
        public int RearrangedCoeffsDirection
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => (int)(ramrcd >> 32);
        }
        /// <summary>
        /// 5th member of <see cref="UnifiedResampleArgs"/>.
        /// </summary>
        public int GradientIncrement
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => (int)accch;
        }
        /// <summary>
        /// 6th member of <see cref="UnifiedResampleArgs"/>. Tied with <see cref="GradientIncrement"/>.
        /// </summary>
        public int Channels
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => (int)(accch >> 32);
        }
        /// <summary>
        /// 7th member of <see cref="UnifiedResampleArgs"/>.
        /// </summary>
        public int IndexIncrement
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => facc;
        }
        /// <summary>
        /// 8th member of <see cref="UnifiedResampleArgs"/>.
        /// </summary>
        public float RateMulInverse
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => rmi;
        }

        [SkipLocalsInit]
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public UnifiedResampleArgs(ulong xrci, ulong ramrcd, ulong accch, int facc, float rmi)
        {
            Unsafe.SkipInit(out this);
            this.xrci = xrci;
            this.ramrcd = ramrcd;
            this.accch = accch;
            this.facc = facc;
            this.rmi = rmi;
        }

        [SkipLocalsInit]
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public UnifiedResampleArgs(float rateMulInverse, int conversionGradient, int rateMul, int gradientIncrement, int indexIncrement, int channels, int rearrangedCoeffsIndex, int rearrangedCoeffsDirection)
        {
            Unsafe.SkipInit(out this);
            ulong xrci = (uint)rearrangedCoeffsIndex;
            ulong ramrcd = (uint)rearrangedCoeffsDirection;
            ulong accch = (uint)channels;
            xrci = (xrci << 32) | (uint)conversionGradient;
            ramrcd = (ramrcd << 32) | (uint)rateMul;
            accch = (accch << 32) | (uint)gradientIncrement;
            this.xrci = xrci;
            this.ramrcd = ramrcd;
            this.accch = accch;
            this.facc = indexIncrement;
            this.rmi = rateMulInverse;
        }

    }
}
