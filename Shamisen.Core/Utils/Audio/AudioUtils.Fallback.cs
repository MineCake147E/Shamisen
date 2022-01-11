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
                    ref var rL = ref MemoryMarshal.GetReference(left);
                    ref var rR = ref MemoryMarshal.GetReference(right);
                    ref var rB = ref MemoryMarshal.GetReference(buffer);
                    nint length = MathI.Min(MathI.Min(left.Length, right.Length), buffer.Length / 2);
                    var u8Length = length & ~1;
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
                    ref var rL = ref MemoryMarshal.GetReference(left);
                    ref var rR = ref MemoryMarshal.GetReference(right);
                    ref var rB = ref MemoryMarshal.GetReference(buffer);
                    nint length = MathI.Min(MathI.Min(left.Length, right.Length), buffer.Length / 2);
                    var u8Length = length & ~1;
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
                    ref var rL = ref MemoryMarshal.GetReference(left);
                    ref var rR = ref MemoryMarshal.GetReference(right);
                    ref var rC = ref MemoryMarshal.GetReference(center);
                    ref var rB = ref MemoryMarshal.GetReference(buffer);
                    nint length = MathI.Min(MathI.Min(left.Length, right.Length), MathI.Min(buffer.Length / 3, center.Length));
                    var u8Length = length & ~1;
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
                    ref var rFL = ref MemoryMarshal.GetReference(frontLeft);
                    ref var rFR = ref MemoryMarshal.GetReference(frontRight);
                    ref var rRL = ref MemoryMarshal.GetReference(rearLeft);
                    ref var rRR = ref MemoryMarshal.GetReference(rearRight);
                    ref var rB = ref MemoryMarshal.GetReference(buffer);
                    nint length = MathI.Min(buffer.Length / 4, MathI.Min(MathI.Min(frontLeft.Length, frontRight.Length), MathI.Min(rearLeft.Length, rearRight.Length)));
                    var u8Length = length & ~1;
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

            #region DuplicateMonauralToChannels
            #region Stereo

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
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

            #region Three

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void DuplicateMonauralTo3Channels(Span<float> destination, ReadOnlySpan<float> source)
            {
                ref var src = ref MemoryMarshal.GetReference(source);
                ref var dst = ref MemoryMarshal.GetReference(destination);
                nint i = 0, length = MathI.Min(source.Length, destination.Length / 3);
                var olen = length - 3;
                for (; i < olen; i += 4)
                {
                    var v0_4s = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref src, i));
                    var s1 = v0_4s.X;
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
                    var s1 = Unsafe.Add(ref src, i);
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
                ref var x0 = ref MemoryMarshal.GetReference(source);
                ref var x1 = ref MemoryMarshal.GetReference(destination);
                nint i = 0, length = MathI.Min(source.Length * (nint)4, destination.Length);
                var olen = length - 3 * 4;
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
            #region Deinterleave
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void DeinterleaveStereoSingle(ReadOnlySpan<float> buffer, Span<float> left, Span<float> right)
            {
                nint i, length = MathI.Min(MathI.Min(left.Length, right.Length), buffer.Length / 2);
                ref var rsi = ref MemoryMarshal.GetReference(buffer);
                ref var r8 = ref MemoryMarshal.GetReference(left);
                ref var r9 = ref MemoryMarshal.GetReference(right);
                var olen = length - 3;
                for (i = 0; i < olen; i += 4)
                {
                    Unsafe.Add(ref r8, i + 0) = Unsafe.Add(ref rsi, 2 * i + 0);
                    Unsafe.Add(ref r9, i + 0) = Unsafe.Add(ref rsi, 2 * i + 1);
                    Unsafe.Add(ref r8, i + 1) = Unsafe.Add(ref rsi, 2 * i + 2);
                    Unsafe.Add(ref r9, i + 1) = Unsafe.Add(ref rsi, 2 * i + 3);
                    Unsafe.Add(ref r8, i + 2) = Unsafe.Add(ref rsi, 2 * i + 4);
                    Unsafe.Add(ref r9, i + 2) = Unsafe.Add(ref rsi, 2 * i + 5);
                    Unsafe.Add(ref r8, i + 3) = Unsafe.Add(ref rsi, 2 * i + 6);
                    Unsafe.Add(ref r9, i + 3) = Unsafe.Add(ref rsi, 2 * i + 7);
                }
                for (; i < length; i++)
                {
                    var l = Unsafe.Add(ref rsi, 2 * i);
                    var r = Unsafe.Add(ref rsi, 2 * i + 1);
                    Unsafe.Add(ref r8, i) = l;
                    Unsafe.Add(ref r9, i) = r;
                }
            }
            #endregion

            #region FastLog2
            internal static void FastLog2Order5Fallback(Span<float> destination, ReadOnlySpan<float> source)
            {
                ref var x9 = ref MemoryMarshal.GetReference(source);
                ref var x10 = ref MemoryMarshal.GetReference(destination);
                nint i = 0, length = MathI.Min(destination.Length, source.Length);
                const float C0 = 4.6385369e-2f;
                const float C1 = -1.9626966e-1f;
                const float C2 = 4.175958e-1f;
                const float C3 = -7.0966283e-1f;
                const float C4 = 1.4419656f;
                var v15_ns = new Vector<uint>(0x7f00_0000u);
                var v14_ns = new Vector<uint>(0x3f80_0000u);
                var v13_ns = new Vector<float>(C0);
                var v12_ns = new Vector<float>(C1);
                var v11_ns = new Vector<float>(C2);
                var v10_ns = new Vector<float>(C3);
                var v9_ns = new Vector<float>(C4);
                var v8_ns = new Vector<uint>(0x7f80_0000u);
                var v7_ns = new Vector<float>(5.9604645E-08f);
                var olen = length - 2 * Vector<float>.Count + 1;
                for (; i < olen; i += 2 * Vector<float>.Count)
                {
                    ref var x11 = ref Unsafe.Add(ref x10, i);
                    var v0_ns = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref x9, i + 0 * Vector<float>.Count));
                    var v1_ns = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref x9, i + 1 * Vector<float>.Count));
                    var v2_ns = VectorUtils.AndNot(v8_ns, v0_ns.AsUInt32()).AsSingle();
                    var v3_ns = VectorUtils.AndNot(v8_ns, v1_ns.AsUInt32()).AsSingle();
                    v0_ns = Vector.BitwiseAnd(v0_ns.AsUInt32(), v8_ns).AsSingle();
                    v1_ns = Vector.BitwiseAnd(v1_ns.AsUInt32(), v8_ns).AsSingle();
                    v0_ns = (v0_ns.AsUInt32() + v0_ns.AsUInt32()).AsSingle();
                    v1_ns = (v1_ns.AsUInt32() + v1_ns.AsUInt32()).AsSingle();
                    v0_ns = (v0_ns.AsUInt32() - v15_ns).AsSingle();
                    v1_ns = (v1_ns.AsUInt32() - v15_ns).AsSingle();
                    v0_ns = Vector.ConvertToSingle(v0_ns.AsInt32());
                    v0_ns *= v7_ns;
                    v1_ns = Vector.ConvertToSingle(v1_ns.AsInt32());
                    v1_ns *= v7_ns;
                    var v4_ns = v13_ns;
                    v2_ns = (v14_ns + v2_ns.AsUInt32()).AsSingle();
                    v2_ns -= v14_ns.AsSingle();
                    v3_ns = (v14_ns + v3_ns.AsUInt32()).AsSingle();
                    v3_ns -= v14_ns.AsSingle();
                    var v5_ns = v4_ns;
                    v4_ns *= v2_ns;
                    v4_ns += v12_ns;
                    v5_ns *= v3_ns;
                    v5_ns += v12_ns;
                    v4_ns *= v2_ns;
                    v4_ns += v11_ns;
                    v5_ns *= v3_ns;
                    v5_ns += v11_ns;
                    v4_ns *= v2_ns;
                    v4_ns += v10_ns;
                    v5_ns *= v3_ns;
                    v5_ns += v10_ns;
                    v4_ns *= v2_ns;
                    v4_ns += v9_ns;
                    v5_ns *= v3_ns;
                    v5_ns += v9_ns;
                    v4_ns *= v2_ns;
                    v4_ns += v0_ns;
                    v5_ns *= v3_ns;
                    v5_ns += v1_ns;
                    Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref x11, 0 * Vector<float>.Count)) = v4_ns;
                    Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref x11, 1 * Vector<float>.Count)) = v5_ns;
                }
                unchecked
                {
                    olen = length - Vector<float>.Count + 1;
                    for (; i < olen; i += Vector<float>.Count)
                    {
                        ref var x11 = ref Unsafe.Add(ref x10, i);
                        var v0_ns = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref x9, i));
                        var v2_ns = VectorUtils.AndNot(v8_ns, v0_ns.AsUInt32()).AsSingle();
                        v0_ns = Vector.BitwiseAnd(v0_ns.AsUInt32(), v8_ns).AsSingle();
                        v0_ns = (v0_ns.AsUInt32() + v0_ns.AsUInt32()).AsSingle();
                        v0_ns = (v0_ns.AsUInt32() - v15_ns).AsSingle();
                        v0_ns = Vector.ConvertToSingle(v0_ns.AsInt32());
                        v0_ns *= v7_ns;
                        var v4_ns = v13_ns;
                        v2_ns = (v14_ns + v2_ns.AsUInt32()).AsSingle();
                        v2_ns -= v14_ns.AsSingle();
                        v4_ns *= v2_ns;
                        v4_ns += v12_ns;
                        v4_ns *= v2_ns;
                        v4_ns += v11_ns;
                        v4_ns *= v2_ns;
                        v4_ns += v10_ns;
                        v4_ns *= v2_ns;
                        v4_ns += v9_ns;
                        v4_ns *= v2_ns;
                        v4_ns += v0_ns;
                        Unsafe.As<float, Vector<float>>(ref x11) = v4_ns;
                    }
                    for (; i < length; i++)
                    {
                        ref var x11 = ref Unsafe.Add(ref x10, i);
                        var w0 = Unsafe.As<float, uint>(ref Unsafe.Add(ref x9, i)) << 1;
                        var w1 = w0 << 8;
                        w0 -= 0x7f00_0000u;
                        w1 >>= 9;
                        w0 >>= 24;
                        var s0 = (float)w0;
                        w1 = 0x3f80_0000u + w1;
                        var s1 = BinaryExtensions.UInt32BitsToSingle(w1) - 1.0f;
                        var s2 = v13_ns[0];
                        s2 *= s1;
                        s2 += v12_ns[0];
                        s2 *= s1;
                        s2 += v11_ns[0];
                        s2 *= s1;
                        s2 += v10_ns[0];
                        s2 *= s1;
                        s2 += v9_ns[0];
                        s2 *= s1;
                        s2 += s0;
                        x11 = s2;
                    }
                }
            }
            #endregion
        }
    }
}
