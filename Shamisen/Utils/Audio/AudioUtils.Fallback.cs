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
                        int a = Unsafe.Add(ref rL, i);
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
                    ref float rL = ref MemoryMarshal.GetReference(left);
                    ref float rR = ref MemoryMarshal.GetReference(right);
                    ref float rB = ref MemoryMarshal.GetReference(buffer);
                    nint length = left.Length;
                    nint u8Length = length & ~1;
                    nint j = 0;
                    nint i = 0;
                    for (; i < u8Length; i += 2)
                    {
                        //The Unsafe.Add(ref Unsafe.Add(ref T, IntPtr), int) pattern avoids extra lea instructions.
                        float a = Unsafe.Add(ref rL, i);
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
                        float a = Unsafe.Add(ref rL, i);
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
                    ref int rL = ref MemoryMarshal.GetReference(left);
                    ref int rR = ref MemoryMarshal.GetReference(right);
                    ref int rC = ref MemoryMarshal.GetReference(center);
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
                        int a = Unsafe.Add(ref rL, i);
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
                    ref int rFL = ref MemoryMarshal.GetReference(frontLeft);
                    ref int rFR = ref MemoryMarshal.GetReference(frontRight);
                    ref int rRL = ref MemoryMarshal.GetReference(rearLeft);
                    ref int rRR = ref MemoryMarshal.GetReference(rearRight);
                    ref int rB = ref MemoryMarshal.GetReference(buffer);
                    nint length = MathI.Min(buffer.Length / 4, MathI.Min(MathI.Min(frontLeft.Length, frontRight.Length), MathI.Min(rearLeft.Length, rearRight.Length)));
                    nint u8Length = length & ~1;
                    nint j = 0;
                    nint i = 0;
                    for (; i < u8Length; i += 2)
                    {
                        //The Unsafe.Add(ref Unsafe.Add(ref T, IntPtr), int) pattern avoids extra lea instructions.
                        int a = Unsafe.Add(ref rFL, i);
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
                        int a = Unsafe.Add(ref rFL, i);
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

            #region DuplicateMonauralToChannels
            #region Stereo

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void DuplicateMonauralToStereo(Span<float> destination, ReadOnlySpan<float> source)
            {
                ref float src = ref MemoryMarshal.GetReference(source);
                ref float dst = ref MemoryMarshal.GetReference(destination);
                nint i = 0, length = Math.Min(source.Length, destination.Length / 2);
                nint olen = length - 3;
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
                    float h = Unsafe.Add(ref src, i);
                    Unsafe.As<float, Vector2>(ref Unsafe.Add(ref dst, i * 2)) = new Vector2(h);
                }
            }
            #endregion

            #region Three

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void DuplicateMonauralTo3Channels(Span<float> destination, ReadOnlySpan<float> source)
            {
                ref float src = ref MemoryMarshal.GetReference(source);
                ref float dst = ref MemoryMarshal.GetReference(destination);
                nint i = 0, length = MathI.Min(source.Length, destination.Length / 3);
                nint olen = length - 3;
                for (; i < olen; i += 4)
                {
                    var v0_4s = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref src, i));
                    float s1 = v0_4s.X;
                    Unsafe.Add(ref dst, i * 3 + 0) = s1;
                    Unsafe.Add(ref dst, i * 3 + 1) = s1;
                    Unsafe.Add(ref dst, i * 3 + 2) = s1;
                    s1 = v0_4s.Y;
                    Unsafe.Add(ref dst, i * 3 + 3) = s1;
                    Unsafe.Add(ref dst, i * 3 + 4) = s1;
                    Unsafe.Add(ref dst, i * 3 + 5) = s1;
                    s1 = v0_4s.Z;
                    Unsafe.Add(ref dst, i * 3 + 6) = s1;
                    Unsafe.Add(ref dst, i * 3 + 7) = s1;
                    Unsafe.Add(ref dst, i * 3 + 8) = s1;
                    s1 = v0_4s.W;
                    Unsafe.Add(ref dst, i * 3 + 9) = s1;
                    Unsafe.Add(ref dst, i * 3 + 10) = s1;
                    Unsafe.Add(ref dst, i * 3 + 11) = s1;
                }
                for (; i < length; i++)
                {
                    float s1 = Unsafe.Add(ref src, i);
                    Unsafe.Add(ref dst, i * 3 + 0) = s1;
                    Unsafe.Add(ref dst, i * 3 + 1) = s1;
                    Unsafe.Add(ref dst, i * 3 + 2) = s1;
                }
            }
            #endregion

            #region Quad
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void DuplicateMonauralTo4Channels(Span<float> destination, ReadOnlySpan<float> source)
            {
                ref float x0 = ref MemoryMarshal.GetReference(source);
                ref float x1 = ref MemoryMarshal.GetReference(destination);
                nint i = 0, length = MathI.Min(source.Length * (nint)4, destination.Length);
                nint olen = length - 3 * 4;
                for (; i < olen; i += 4 * 4)
                {
                    var v0_4s = new Vector4(Unsafe.AddByteOffset(ref x0, i + 0));
                    var v1_4s = new Vector4(Unsafe.AddByteOffset(ref x0, i + 4));
                    var v2_4s = new Vector4(Unsafe.AddByteOffset(ref x0, i + 8));
                    var v3_4s = new Vector4(Unsafe.AddByteOffset(ref x0, i + 12));
                    Unsafe.As<float, Vector4>(ref Unsafe.Add(ref x1, i + 0)) = v0_4s;
                    Unsafe.As<float, Vector4>(ref Unsafe.Add(ref x1, i + 4)) = v1_4s;
                    Unsafe.As<float, Vector4>(ref Unsafe.Add(ref x1, i + 8)) = v2_4s;
                    Unsafe.As<float, Vector4>(ref Unsafe.Add(ref x1, i + 12)) = v3_4s;
                }
                for (; i < length; i += 4)
                {
                    var v0_4s = new Vector4(Unsafe.AddByteOffset(ref x0, i));
                    Unsafe.As<float, Vector4>(ref Unsafe.Add(ref x1, i)) = v0_4s;
                }
            }
            #endregion

            #endregion
        }
    }
}
