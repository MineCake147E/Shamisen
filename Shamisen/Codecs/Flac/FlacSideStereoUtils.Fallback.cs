using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Shamisen.Codecs.Flac
{
    public static partial class FlacSideStereoUtils
    {
        internal static partial class Fallback
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void DecodeAndInterleaveLeftSideStereoInt32(Span<int> buffer, ReadOnlySpan<int> left, ReadOnlySpan<int> right)
            {
                unsafe
                {
                    if (left.Length > right.Length) throw new ArgumentException("right must be as long as left!", nameof(right));
                    if (buffer.Length < left.Length * 2) throw new ArgumentException("buffer must be twice as long as left!");
                    ref int rL = ref MemoryMarshal.GetReference(left);
                    ref int rR = ref MemoryMarshal.GetReference(right);
                    ref int rB = ref MemoryMarshal.GetReference(buffer);
                    nint length = left.Length;
                    nint u8Length = length & ~1;
                    nint j = 0;
                    nint i = 0;
                    for (; i < u8Length; i += 2)
                    {
                        //The Unsafe.Add(ref Unsafe.Add(ref T, IntPtr), int) pattern avoids extra lea instructions.
                        int a = Unsafe.Add(ref rL, i);
                        Unsafe.Add(ref rB, j) = a;
                        int b = Unsafe.Add(ref rR, i);
                        Unsafe.Add(ref Unsafe.Add(ref rB, j), 1) = a - b;
                        a = Unsafe.Add(ref Unsafe.Add(ref rL, i), 1);
                        Unsafe.Add(ref Unsafe.Add(ref rB, j), 2) = a;
                        b = Unsafe.Add(ref Unsafe.Add(ref rR, i), 1);
                        Unsafe.Add(ref Unsafe.Add(ref rB, j), 3) = a - b;
                        j += 2 * 2;
                    }
                    for (; i < length; i += 1)
                    {
                        int a = Unsafe.Add(ref rL, i);
                        Unsafe.Add(ref rB, j) = a;
                        int b = Unsafe.Add(ref rR, i);
                        Unsafe.Add(ref Unsafe.Add(ref rB, j), 1) = a - b;
                        j += 2;
                    }
                }
            }

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void DecodeAndInterleaveRightSideStereoInt32(Span<int> buffer, ReadOnlySpan<int> left, ReadOnlySpan<int> right)
            {
                unsafe
                {
                    if (left.Length > right.Length) throw new ArgumentException("right must be as long as left!", nameof(right));
                    if (buffer.Length < left.Length * 2) throw new ArgumentException("buffer must be twice as long as left!");
                    ref int rL = ref MemoryMarshal.GetReference(left);
                    ref int rR = ref MemoryMarshal.GetReference(right);
                    ref int rB = ref MemoryMarshal.GetReference(buffer);
                    nint length = left.Length;
                    nint u8Length = length & ~1;
                    nint j = 0;
                    nint i = 0;
                    for (; i < u8Length; i += 2)
                    {
                        //The Unsafe.Add(ref Unsafe.Add(ref T, IntPtr), int) pattern avoids extra lea instructions.
                        int a = Unsafe.Add(ref rL, i);
                        int b = Unsafe.Add(ref rR, i);
                        Unsafe.Add(ref rB, j) = a + b;
                        Unsafe.Add(ref Unsafe.Add(ref rB, j), 1) = b;
                        a = Unsafe.Add(ref Unsafe.Add(ref rL, i), 1);
                        b = Unsafe.Add(ref Unsafe.Add(ref rR, i), 1);
                        Unsafe.Add(ref Unsafe.Add(ref rB, j), 2) = a + b;
                        Unsafe.Add(ref Unsafe.Add(ref rB, j), 3) = b;
                        j += 2 * 2;
                    }
                    for (; i < length; i += 1)
                    {
                        int a = Unsafe.Add(ref rL, i);
                        int b = Unsafe.Add(ref rR, i);
                        Unsafe.Add(ref rB, j) = a + b;
                        Unsafe.Add(ref Unsafe.Add(ref rB, j), 1) = b;
                        j += 2;
                    }
                }
            }

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void DecodeAndInterleaveMidSideStereoInt32(Span<int> buffer, ReadOnlySpan<int> left, ReadOnlySpan<int> right)
            {
                unsafe
                {
                    if (left.Length > right.Length) throw new ArgumentException("right must be as long as left!", nameof(right));
                    if (buffer.Length < left.Length * 2) throw new ArgumentException("buffer must be twice as long as left!");
                    ref int rL = ref MemoryMarshal.GetReference(left);
                    ref int rR = ref MemoryMarshal.GetReference(right);
                    ref int rB = ref MemoryMarshal.GetReference(buffer);
                    nint length = left.Length;
                    nint u8Length = length & ~1;
                    nint j = 0;
                    nint i = 0;
                    for (; i < u8Length; i += 2)
                    {
                        //The Unsafe.Add(ref Unsafe.Add(ref T, IntPtr), int) pattern avoids extra lea instructions.
                        int a = Unsafe.Add(ref rL, i);
                        int b = Unsafe.Add(ref rR, i);
                        a <<= 1;
                        a |= b & 1;
                        Unsafe.Add(ref rB, j) = (a + b) >> 1;
                        Unsafe.Add(ref Unsafe.Add(ref rB, j), 1) = (a - b) >> 1;
                        a = Unsafe.Add(ref Unsafe.Add(ref rL, i), 1);
                        b = Unsafe.Add(ref Unsafe.Add(ref rR, i), 1);
                        a <<= 1;
                        a |= b & 1;
                        Unsafe.Add(ref Unsafe.Add(ref rB, j), 2) = (a + b) >> 1;
                        Unsafe.Add(ref Unsafe.Add(ref rB, j), 3) = (a - b) >> 1;
                        j += 2 * 2;
                    }
                    for (; i < length; i += 1)
                    {
                        int a = Unsafe.Add(ref rL, i);
                        int b = Unsafe.Add(ref rR, i);
                        a <<= 1;
                        a |= b & 1;
                        Unsafe.Add(ref rB, j) = (a + b) >> 1;
                        Unsafe.Add(ref Unsafe.Add(ref rB, j), 1) = (a - b) >> 1;
                        j += 2;
                    }
                }
            }
        }
    }
}
