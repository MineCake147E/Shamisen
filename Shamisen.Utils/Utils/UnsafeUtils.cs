using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Utils
{
    /// <summary>
    /// Contains some unsafe utility features.
    /// </summary>
    public static class UnsafeUtils
    {
        /// <summary>
        /// Copies data from <paramref name="src"/> to <paramref name="dst"/> with specified <paramref name="length"/>.
        /// </summary>
        /// <param name="dst">The reference to the destination memory region.</param>
        /// <param name="src">The reference to the source memory region.</param>
        /// <param name="length">The length in bytes to copy.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static void MoveMemory(ref byte dst, ref byte src, nuint length)
        {
            //check for overlap
            if (Unsafe.AreSame(ref dst, ref src)) return;
            if (MathR.IsAddressInRange(ref src, length, ref dst))
            {
                CopyFromTail(ref dst, ref src, length);
                return;
            }
            CopyFromHead(ref dst, ref src, length);
        }

        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        internal static void CopyFromHead(ref byte dst, ref byte src, nuint length)
        {
            // src is inside destination: copying from head
            nuint i = 0;
            nuint olen;
            if (Vector.IsHardwareAccelerated)
            {
                olen = length - (nuint)(Vector<byte>.Count * 16) + 1;
                if (olen < length)
                {
                    for (; i < olen; i += (nuint)Vector<byte>.Count * 16)
                    {
                        ref var x10 = ref Unsafe.Add(ref dst, i);
                        var v0_nb = Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref src, i + 0 * (nuint)Vector<byte>.Count));
                        var v1_nb = Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref src, i + 1 * (nuint)Vector<byte>.Count));
                        var v2_nb = Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref src, i + 2 * (nuint)Vector<byte>.Count));
                        var v3_nb = Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref src, i + 3 * (nuint)Vector<byte>.Count));
                        Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref x10, 0 * (nuint)Vector<byte>.Count)) = v0_nb;
                        Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref x10, 1 * (nuint)Vector<byte>.Count)) = v1_nb;
                        Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref x10, 2 * (nuint)Vector<byte>.Count)) = v2_nb;
                        Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref x10, 3 * (nuint)Vector<byte>.Count)) = v3_nb;
                        v0_nb = Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref src, i + 4 * (nuint)Vector<byte>.Count));
                        v1_nb = Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref src, i + 5 * (nuint)Vector<byte>.Count));
                        v2_nb = Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref src, i + 6 * (nuint)Vector<byte>.Count));
                        v3_nb = Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref src, i + 7 * (nuint)Vector<byte>.Count));
                        Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref x10, 4 * (nuint)Vector<byte>.Count)) = v0_nb;
                        Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref x10, 5 * (nuint)Vector<byte>.Count)) = v1_nb;
                        Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref x10, 6 * (nuint)Vector<byte>.Count)) = v2_nb;
                        Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref x10, 7 * (nuint)Vector<byte>.Count)) = v3_nb;
                        v0_nb = Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref src, i + 8 * (nuint)Vector<byte>.Count));
                        v1_nb = Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref src, i + 9 * (nuint)Vector<byte>.Count));
                        v2_nb = Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref src, i + 10 * (nuint)Vector<byte>.Count));
                        v3_nb = Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref src, i + 11 * (nuint)Vector<byte>.Count));
                        Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref x10, 8 * (nuint)Vector<byte>.Count)) = v0_nb;
                        Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref x10, 9 * (nuint)Vector<byte>.Count)) = v1_nb;
                        Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref x10, 10 * (nuint)Vector<byte>.Count)) = v2_nb;
                        Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref x10, 11 * (nuint)Vector<byte>.Count)) = v3_nb;
                        v0_nb = Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref src, i + 12 * (nuint)Vector<byte>.Count));
                        v1_nb = Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref src, i + 13 * (nuint)Vector<byte>.Count));
                        v2_nb = Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref src, i + 14 * (nuint)Vector<byte>.Count));
                        v3_nb = Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref src, i + 15 * (nuint)Vector<byte>.Count));
                        Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref x10, 12 * (nuint)Vector<byte>.Count)) = v0_nb;
                        Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref x10, 13 * (nuint)Vector<byte>.Count)) = v1_nb;
                        Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref x10, 14 * (nuint)Vector<byte>.Count)) = v2_nb;
                        Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref x10, 15 * (nuint)Vector<byte>.Count)) = v3_nb;
                    }
                }
                olen = length - (nuint)(Vector<byte>.Count * 4) + 1;
                if (olen < length)
                {
                    for (; i < olen; i += (nuint)Vector<byte>.Count * 4)
                    {
                        ref var x10 = ref Unsafe.Add(ref dst, i);
                        var v0_nb = Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref src, i + 0 * (nuint)Vector<byte>.Count));
                        var v1_nb = Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref src, i + 1 * (nuint)Vector<byte>.Count));
                        var v2_nb = Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref src, i + 2 * (nuint)Vector<byte>.Count));
                        var v3_nb = Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref src, i + 3 * (nuint)Vector<byte>.Count));
                        Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref x10, 0 * (nuint)Vector<byte>.Count)) = v0_nb;
                        Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref x10, 1 * (nuint)Vector<byte>.Count)) = v1_nb;
                        Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref x10, 2 * (nuint)Vector<byte>.Count)) = v2_nb;
                        Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref x10, 3 * (nuint)Vector<byte>.Count)) = v3_nb;
                    }
                }
            }
            olen = length - sizeof(ulong) * 4 + 1;
            if (olen < length)
            {
                for (; i < olen; i += sizeof(ulong) * 4)
                {
                    ref var x10 = ref Unsafe.Add(ref dst, i);
                    var x11 = Unsafe.As<byte, ulong>(ref Unsafe.Add(ref src, i + 0 * sizeof(ulong)));
                    var x12 = Unsafe.As<byte, ulong>(ref Unsafe.Add(ref src, i + 1 * sizeof(ulong)));
                    var x13 = Unsafe.As<byte, ulong>(ref Unsafe.Add(ref src, i + 2 * sizeof(ulong)));
                    var x14 = Unsafe.As<byte, ulong>(ref Unsafe.Add(ref src, i + 3 * sizeof(ulong)));
                    Unsafe.As<byte, ulong>(ref Unsafe.Add(ref x10, 0 * sizeof(ulong))) = x11;
                    Unsafe.As<byte, ulong>(ref Unsafe.Add(ref x10, 1 * sizeof(ulong))) = x12;
                    Unsafe.As<byte, ulong>(ref Unsafe.Add(ref x10, 2 * sizeof(ulong))) = x13;
                    Unsafe.As<byte, ulong>(ref Unsafe.Add(ref x10, 3 * sizeof(ulong))) = x14;
                }
            }
            olen = length - sizeof(ulong) + 1;
            if (olen < length)
            {
                for (; i < olen; i += sizeof(ulong))
                {
                    ref var x10 = ref Unsafe.Add(ref dst, i);
                    Unsafe.As<byte, ulong>(ref x10) = Unsafe.As<byte, ulong>(ref Unsafe.Add(ref src, i));
                }
            }
            for (; i < length; i++)
            {
                Unsafe.Add(ref dst, i) = Unsafe.Add(ref src, i);
            }
        }

        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        internal static void CopyFromTail(ref byte dst, ref byte src, nuint length)
        {
            // destination.head is inside source: copying from tail
            nuint i = 0, ni = 0;
            nuint olen;
            ref var x8 = ref Unsafe.Add(ref src, length - (nuint)Vector<byte>.Count);
            ref var x9 = ref Unsafe.Add(ref dst, length - (nuint)Vector<byte>.Count);
            if (Vector.IsHardwareAccelerated)
            {
                olen = length - (nuint)(Vector<byte>.Count * 16) + 1;
                if (olen < length)
                {
                    for (; i < olen; i += (nuint)Vector<byte>.Count * 16, ni -= (nuint)Vector<byte>.Count * 16)
                    {
                        ref var x11 = ref Unsafe.Add(ref x9, ni);
                        var v0_nb = Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref x8, ni - 0 * (nuint)Vector<byte>.Count));
                        var v1_nb = Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref x8, ni - 1 * (nuint)Vector<byte>.Count));
                        var v2_nb = Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref x8, ni - 2 * (nuint)Vector<byte>.Count));
                        var v3_nb = Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref x8, ni - 3 * (nuint)Vector<byte>.Count));
                        Unsafe.As<byte, Vector<byte>>(ref Unsafe.Subtract(ref x11, 0 * (nuint)Vector<byte>.Count)) = v0_nb;
                        Unsafe.As<byte, Vector<byte>>(ref Unsafe.Subtract(ref x11, 1 * (nuint)Vector<byte>.Count)) = v1_nb;
                        Unsafe.As<byte, Vector<byte>>(ref Unsafe.Subtract(ref x11, 2 * (nuint)Vector<byte>.Count)) = v2_nb;
                        Unsafe.As<byte, Vector<byte>>(ref Unsafe.Subtract(ref x11, 3 * (nuint)Vector<byte>.Count)) = v3_nb;
                        v0_nb = Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref x8, ni - 4 * (nuint)Vector<byte>.Count));
                        v1_nb = Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref x8, ni - 5 * (nuint)Vector<byte>.Count));
                        v2_nb = Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref x8, ni - 6 * (nuint)Vector<byte>.Count));
                        v3_nb = Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref x8, ni - 7 * (nuint)Vector<byte>.Count));
                        Unsafe.As<byte, Vector<byte>>(ref Unsafe.Subtract(ref x11, 4 * (nuint)Vector<byte>.Count)) = v0_nb;
                        Unsafe.As<byte, Vector<byte>>(ref Unsafe.Subtract(ref x11, 5 * (nuint)Vector<byte>.Count)) = v1_nb;
                        Unsafe.As<byte, Vector<byte>>(ref Unsafe.Subtract(ref x11, 6 * (nuint)Vector<byte>.Count)) = v2_nb;
                        Unsafe.As<byte, Vector<byte>>(ref Unsafe.Subtract(ref x11, 7 * (nuint)Vector<byte>.Count)) = v3_nb;
                        v0_nb = Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref x8, ni - 8 * (nuint)Vector<byte>.Count));
                        v1_nb = Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref x8, ni - 9 * (nuint)Vector<byte>.Count));
                        v2_nb = Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref x8, ni - 10 * (nuint)Vector<byte>.Count));
                        v3_nb = Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref x8, ni - 11 * (nuint)Vector<byte>.Count));
                        Unsafe.As<byte, Vector<byte>>(ref Unsafe.Subtract(ref x11, 8 * (nuint)Vector<byte>.Count)) = v0_nb;
                        Unsafe.As<byte, Vector<byte>>(ref Unsafe.Subtract(ref x11, 9 * (nuint)Vector<byte>.Count)) = v1_nb;
                        Unsafe.As<byte, Vector<byte>>(ref Unsafe.Subtract(ref x11, 10 * (nuint)Vector<byte>.Count)) = v2_nb;
                        Unsafe.As<byte, Vector<byte>>(ref Unsafe.Subtract(ref x11, 11 * (nuint)Vector<byte>.Count)) = v3_nb;
                        v0_nb = Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref x8, ni - 12 * (nuint)Vector<byte>.Count));
                        v1_nb = Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref x8, ni - 13 * (nuint)Vector<byte>.Count));
                        v2_nb = Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref x8, ni - 14 * (nuint)Vector<byte>.Count));
                        v3_nb = Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref x8, ni - 15 * (nuint)Vector<byte>.Count));
                        Unsafe.As<byte, Vector<byte>>(ref Unsafe.Subtract(ref x11, 12 * (nuint)Vector<byte>.Count)) = v0_nb;
                        Unsafe.As<byte, Vector<byte>>(ref Unsafe.Subtract(ref x11, 13 * (nuint)Vector<byte>.Count)) = v1_nb;
                        Unsafe.As<byte, Vector<byte>>(ref Unsafe.Subtract(ref x11, 14 * (nuint)Vector<byte>.Count)) = v2_nb;
                        Unsafe.As<byte, Vector<byte>>(ref Unsafe.Subtract(ref x11, 15 * (nuint)Vector<byte>.Count)) = v3_nb;
                    }
                }
                olen = length - (nuint)(Vector<byte>.Count * 4) + 1;
                if (olen < length)
                {
                    for (; i < olen; i += (nuint)Vector<byte>.Count * 4, ni -= (nuint)Vector<byte>.Count * 4)
                    {
                        ref var x11 = ref Unsafe.Add(ref x9, ni);
                        var v0_nb = Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref x8, ni - 0 * (nuint)Vector<byte>.Count));
                        var v1_nb = Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref x8, ni - 1 * (nuint)Vector<byte>.Count));
                        var v2_nb = Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref x8, ni - 2 * (nuint)Vector<byte>.Count));
                        var v3_nb = Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref x8, ni - 3 * (nuint)Vector<byte>.Count));
                        Unsafe.As<byte, Vector<byte>>(ref Unsafe.Subtract(ref x11, 0 * (nuint)Vector<byte>.Count)) = v0_nb;
                        Unsafe.As<byte, Vector<byte>>(ref Unsafe.Subtract(ref x11, 1 * (nuint)Vector<byte>.Count)) = v1_nb;
                        Unsafe.As<byte, Vector<byte>>(ref Unsafe.Subtract(ref x11, 2 * (nuint)Vector<byte>.Count)) = v2_nb;
                        Unsafe.As<byte, Vector<byte>>(ref Unsafe.Subtract(ref x11, 3 * (nuint)Vector<byte>.Count)) = v3_nb;
                    }
                }
            }
            x8 = ref Unsafe.Add(ref src, length - sizeof(ulong));
            x9 = ref Unsafe.Add(ref dst, length - sizeof(ulong));
            olen = length - 4 * sizeof(ulong) + 1;
            if (olen < length)
            {
                for (; i < olen; i += sizeof(ulong) * 4, ni -= sizeof(ulong) * 4)
                {
                    ref var x11 = ref Unsafe.Add(ref x9, ni);
                    var x12 = Unsafe.As<byte, ulong>(ref Unsafe.Add(ref x8, ni - 0 * sizeof(ulong)));
                    var x13 = Unsafe.As<byte, ulong>(ref Unsafe.Add(ref x8, ni - 1 * sizeof(ulong)));
                    var x14 = Unsafe.As<byte, ulong>(ref Unsafe.Add(ref x8, ni - 2 * sizeof(ulong)));
                    var x15 = Unsafe.As<byte, ulong>(ref Unsafe.Add(ref x8, ni - 3 * sizeof(ulong)));
                    Unsafe.As<byte, ulong>(ref Unsafe.Subtract(ref x11, 0 * sizeof(ulong))) = x12;
                    Unsafe.As<byte, ulong>(ref Unsafe.Subtract(ref x11, 1 * sizeof(ulong))) = x13;
                    Unsafe.As<byte, ulong>(ref Unsafe.Subtract(ref x11, 2 * sizeof(ulong))) = x14;
                    Unsafe.As<byte, ulong>(ref Unsafe.Subtract(ref x11, 3 * sizeof(ulong))) = x15;
                }
            }
            olen = length - sizeof(ulong) + 1;
            if (olen < length)
            {
                for (; i < olen; i += sizeof(ulong), ni -= sizeof(ulong))
                {
                    ref var x10 = ref Unsafe.Add(ref x9, ni);
                    Unsafe.As<byte, ulong>(ref x10) = Unsafe.As<byte, ulong>(ref Unsafe.Add(ref x8, ni));
                }
            }
            x8 = ref Unsafe.Add(ref src, length - 1);
            x9 = ref Unsafe.Add(ref dst, length - 1);
            for (; i < length; i++, ni--)
            {
                Unsafe.Add(ref x9, ni) = Unsafe.Add(ref x8, ni);
            }
        }

        #region Memory Management

        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        internal static bool ValidateAllocationRequest<T>(nint length, out nuint lengthInBytes, byte alignmentExponent = 0, ushort alignmentExponentLimit = 256)
        {
            bool isLengthInRange;
            if (nuint.MaxValue == uint.MaxValue)
            {
                ulong h = (ulong)length * (ulong)Unsafe.SizeOf<T>();
                lengthInBytes = (nuint)h;
                isLengthInRange = h <= uint.MaxValue;
            }
            else
            {
                var hi = Math.BigMul((ulong)length, (ulong)Unsafe.SizeOf<T>(), out var newLength);
                lengthInBytes = (nuint)newLength;
                isLengthInRange = hi == 0;
            }
            return length > 0 && isLengthInRange && alignmentExponent <= alignmentExponentLimit;
        }

        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        internal static unsafe void* AllocateNativeMemoryInternal(nuint lengthInBytes, byte alignmentExponent, bool memoryPressure)
        {
            nuint alignment = (nuint)1 << alignmentExponent;
            if (memoryPressure)
            {
                AddMemoryPressure(lengthInBytes);
            }
            return alignment > 1 ? NativeMemory.AlignedAlloc(lengthInBytes, alignment) : NativeMemory.Alloc(lengthInBytes);
        }

        /// <inheritdoc cref="GC.AddMemoryPressure(long)"/>
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        public static void AddMemoryPressure(nuint bytesAllocated)
        {
            while (bytesAllocated > 0)
            {
                var max = nuint.Min(bytesAllocated, (nuint)nint.MaxValue);
                GC.AddMemoryPressure((long)max);
                bytesAllocated -= max;
            }
        }

        /// <inheritdoc cref="GC.RemoveMemoryPressure(long)"/>
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        public static void RemoveMemoryPressure(nuint bytesAllocated)
        {
            while (bytesAllocated > 0)
            {
                var max = nuint.Min(bytesAllocated, (nuint)nint.MaxValue);
                GC.RemoveMemoryPressure((long)max);
                bytesAllocated -= max;
            }
        }
        #endregion
    }
}
