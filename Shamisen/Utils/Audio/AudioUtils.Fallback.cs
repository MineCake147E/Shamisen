using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Shamisen.Utils
{
    public static partial class AudioUtils
    {
        internal static partial class Fallback
        {
            #region Interleave


            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void InterleaveStereoInt32(Span<int> buffer, ReadOnlySpan<int> left, ReadOnlySpan<int> right)
            {
                unsafe
                {
                    if (left.Length > right.Length) throw new ArgumentException("right must be as long as left!", nameof(right));
                    if (buffer.Length < left.Length * 2) throw new ArgumentException("buffer must be twice as long as left!");
                    ref var rL = ref MemoryMarshal.GetReference(left);
                    ref var rR = ref MemoryMarshal.GetReference(right);
                    ref var rB = ref MemoryMarshal.GetReference(buffer);
                    nint length = left.Length;
                    nint u8Length = length & ~1;
                    nint j = 0;
                    nint i = 0;
                    for (; i < u8Length; i += 2)
                    {
                        //The Unsafe.Add(ref Unsafe.Add(ref T, IntPtr), int) pattern avoids extra lea instructions.
                        var a = Unsafe.Add(ref rL, i);
                        Unsafe.Add(ref rB, j) = a;
                        a = Unsafe.Add(ref rR, i);
                        Unsafe.Add(ref Unsafe.Add(ref rB, j), 1) = a;
                        a = Unsafe.Add(ref Unsafe.Add(ref rL, i), 1);
                        Unsafe.Add(ref Unsafe.Add(ref rB, j), 2) = a;
                        a = Unsafe.Add(ref Unsafe.Add(ref rR, i), 1);
                        Unsafe.Add(ref Unsafe.Add(ref rB, j), 3) = a;
                        j += 2 * 2;
                    }
                    for (; i < length; i += 1)
                    {
                        var a = Unsafe.Add(ref rL, i);
                        Unsafe.Add(ref rB, j) = a;
                        a = Unsafe.Add(ref rR, i);
                        Unsafe.Add(ref Unsafe.Add(ref rB, j), 1) = a;
                        j += 2;
                    }
                }
            }

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void InterleaveStereoSingle(Span<float> buffer, ReadOnlySpan<float> left, ReadOnlySpan<float> right)
            {
                unsafe
                {
                    if (left.Length > right.Length) throw new ArgumentException("right must be as long as left!", nameof(right));
                    if (buffer.Length < left.Length * 2) throw new ArgumentException("buffer must be twice as long as left!");
                    ref var rL = ref MemoryMarshal.GetReference(left);
                    ref var rR = ref MemoryMarshal.GetReference(right);
                    ref var rB = ref MemoryMarshal.GetReference(buffer);
                    nint length = left.Length;
                    nint u8Length = length & ~1;
                    nint j = 0;
                    nint i = 0;
                    for (; i < u8Length; i += 2)
                    {
                        //The Unsafe.Add(ref Unsafe.Add(ref T, IntPtr), int) pattern avoids extra lea instructions.
                        var a = Unsafe.Add(ref rL, i);
                        Unsafe.Add(ref rB, j) = a;
                        a = Unsafe.Add(ref rR, i);
                        Unsafe.Add(ref Unsafe.Add(ref rB, j), 1) = a;
                        a = Unsafe.Add(ref Unsafe.Add(ref rL, i), 1);
                        Unsafe.Add(ref Unsafe.Add(ref rB, j), 2) = a;
                        a = Unsafe.Add(ref Unsafe.Add(ref rR, i), 1);
                        Unsafe.Add(ref Unsafe.Add(ref rB, j), 3) = a;
                        j += 2 * 2;
                    }
                    for (; i < length; i += 1)
                    {
                        var a = Unsafe.Add(ref rL, i);
                        Unsafe.Add(ref rB, j) = a;
                        a = Unsafe.Add(ref rR, i);
                        Unsafe.Add(ref Unsafe.Add(ref rB, j), 1) = a;
                        j += 2;
                    }
                }
            }

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void InterleaveThreeInt32(Span<int> buffer, ReadOnlySpan<int> left, ReadOnlySpan<int> right, ReadOnlySpan<int> center)
            {
                unsafe
                {
                    const int Channels = 3;
                    if (left.Length > right.Length) throw new ArgumentException("right must be as long as left!", nameof(right));
                    if (left.Length > center.Length) throw new ArgumentException("center must be as long as left!", nameof(center));
                    if (buffer.Length < left.Length * Channels) throw new ArgumentException("buffer must be three times as long as left!");
                    //These pre-touches may avoid some range checks
                    _ = right[left.Length - 1];
                    _ = center[left.Length - 1];
                    _ = buffer[left.Length * Channels - 1];
                    ref var rL = ref MemoryMarshal.GetReference(left);
                    ref var rR = ref MemoryMarshal.GetReference(right);
                    ref var rC = ref MemoryMarshal.GetReference(center);
                    ref var rB = ref MemoryMarshal.GetReference(buffer);
                    nint length = left.Length;
                    nint u8Length = length & ~1;
                    nint j = 0;
                    nint i = 0;
                    for (; i < u8Length; i += 2)
                    {
                        //The Unsafe.Add(ref Unsafe.Add(ref T, IntPtr), int) pattern avoids extra lea instructions.
                        var a = Unsafe.Add(ref rL, i);
                        Unsafe.Add(ref rB, j) = a;
                        a = Unsafe.Add(ref rR, i);
                        Unsafe.Add(ref Unsafe.Add(ref rB, j), 1) = a;
                        a = Unsafe.Add(ref rC, i);
                        Unsafe.Add(ref Unsafe.Add(ref rB, j), 2) = a;
                        a = Unsafe.Add(ref Unsafe.Add(ref rL, i), 1);
                        Unsafe.Add(ref Unsafe.Add(ref rB, j), 3) = a;
                        a = Unsafe.Add(ref Unsafe.Add(ref rR, i), 1);
                        Unsafe.Add(ref Unsafe.Add(ref rB, j), 4) = a;
                        a = Unsafe.Add(ref Unsafe.Add(ref rC, i), 1);
                        Unsafe.Add(ref Unsafe.Add(ref rB, j), 5) = a;
                        j += Channels * 2;
                    }
                    for (; i < length; i += 1)
                    {
                        var a = Unsafe.Add(ref rL, i);
                        Unsafe.Add(ref rB, j) = a;
                        a = Unsafe.Add(ref rR, i);
                        Unsafe.Add(ref Unsafe.Add(ref rB, j), 1) = a;
                        a = Unsafe.Add(ref rC, i);
                        Unsafe.Add(ref Unsafe.Add(ref rB, j), 2) = a;
                        j += Channels;
                    }
                }
            }

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void InterleaveQuadInt32(Span<int> buffer, ReadOnlySpan<int> frontLeft, ReadOnlySpan<int> frontRight, ReadOnlySpan<int> rearLeft, ReadOnlySpan<int> rearRight)
            {
                unsafe
                {
                    const int Channels = 4;
                    if (frontLeft.Length > frontRight.Length) throw new ArgumentException("right must be as long as left!", nameof(frontRight));
                    if (frontLeft.Length > rearLeft.Length) throw new ArgumentException("center must be as long as left!", nameof(rearLeft));
                    if (frontLeft.Length > rearRight.Length) throw new ArgumentException("center must be as long as left!", nameof(rearRight));
                    if (buffer.Length < frontLeft.Length * Channels) throw new ArgumentException("buffer must be three times as long as left!");
                    //These pre-touches may avoid some range checks
                    _ = frontRight[frontLeft.Length - 1];
                    _ = rearLeft[frontLeft.Length - 1];
                    _ = rearRight[frontLeft.Length - 1];
                    _ = buffer[frontLeft.Length * Channels - 1];
                    ref var rFL = ref MemoryMarshal.GetReference(frontLeft);
                    ref var rFR = ref MemoryMarshal.GetReference(frontRight);
                    ref var rRL = ref MemoryMarshal.GetReference(rearLeft);
                    ref var rRR = ref MemoryMarshal.GetReference(rearRight);
                    ref var rB = ref MemoryMarshal.GetReference(buffer);
                    nint length = frontLeft.Length;
                    nint u8Length = length & ~1;
                    nint j = 0;
                    nint i = 0;
                    for (; i < u8Length; i += 2)
                    {
                        //The Unsafe.Add(ref Unsafe.Add(ref T, IntPtr), int) pattern avoids extra lea instructions.
                        var a = Unsafe.Add(ref rFL, i);
                        Unsafe.Add(ref rB, j) = a;
                        a = Unsafe.Add(ref rFR, i);
                        Unsafe.Add(ref Unsafe.Add(ref rB, j), 1) = a;
                        a = Unsafe.Add(ref rRL, i);
                        Unsafe.Add(ref Unsafe.Add(ref rB, j), 2) = a;
                        a = Unsafe.Add(ref rRR, i);
                        Unsafe.Add(ref Unsafe.Add(ref rB, j), 3) = a;
                        a = Unsafe.Add(ref Unsafe.Add(ref rFL, i), 1);
                        Unsafe.Add(ref Unsafe.Add(ref rB, j), 4) = a;
                        a = Unsafe.Add(ref Unsafe.Add(ref rFR, i), 1);
                        Unsafe.Add(ref Unsafe.Add(ref rB, j), 5) = a;
                        a = Unsafe.Add(ref Unsafe.Add(ref rRL, i), 1);
                        Unsafe.Add(ref Unsafe.Add(ref rB, j), 6) = a;
                        a = Unsafe.Add(ref Unsafe.Add(ref rRR, i), 1);
                        Unsafe.Add(ref Unsafe.Add(ref rB, j), 7) = a;
                        j += Channels * 2;
                    }
                    for (; i < length; i += 1)
                    {
                        var a = Unsafe.Add(ref rFL, i);
                        Unsafe.Add(ref rB, j) = a;
                        a = Unsafe.Add(ref rFR, i);
                        Unsafe.Add(ref Unsafe.Add(ref rB, j), 1) = a;
                        a = Unsafe.Add(ref rRL, i);
                        Unsafe.Add(ref Unsafe.Add(ref rB, j), 2) = a;
                        a = Unsafe.Add(ref rRR, i);
                        Unsafe.Add(ref Unsafe.Add(ref rB, j), 3) = a;
                        j += Channels;
                    }
                }
            }
            #endregion

            #region DuplicateMonauralToStereo

            internal static void DuplicateMonauralToStereo(Span<float> destination, ReadOnlySpan<float> source)
            {
                ref var src = ref MemoryMarshal.GetReference(source);
                ref var dst = ref MemoryMarshal.GetReference(destination);
                nint i = 0, length = Math.Min(source.Length, destination.Length / 2);
                var olen = length - 3;
                for (; i < olen; i += 4)
                {
                    var h = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref src, i));
                    Unsafe.As<float, Vector2>(ref Unsafe.Add(ref dst, i * 2 + 0)) = new Vector2(h.X);
                    Unsafe.As<float, Vector2>(ref Unsafe.Add(ref dst, i * 2 + 2)) = new Vector2(h.Y);
                    Unsafe.As<float, Vector2>(ref Unsafe.Add(ref dst, i * 2 + 4)) = new Vector2(h.Z);
                    Unsafe.As<float, Vector2>(ref Unsafe.Add(ref dst, i * 2 + 6)) = new Vector2(h.W);
                }
                for (; i < length; i++)
                {
                    var h = Unsafe.Add(ref src, i);
                    Unsafe.As<float, Vector2>(ref Unsafe.Add(ref dst, i * 2)) = new Vector2(h);
                }
            }
            #endregion
        }
    }
}
