#if NETCOREAPP3_1_OR_GREATER

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Shamisen.Utils
{
    public static partial class AudioUtils
    {
        internal static partial class X86
        {
            internal static bool IsSupported =>
#if NET5_0_OR_GREATER
                X86Base.IsSupported;

#else
                Sse.IsSupported;
#endif

            #region Stereo

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void InterleaveStereoInt32(Span<int> buffer, ReadOnlySpan<int> left, ReadOnlySpan<int> right)
            {
                if (Avx2.IsSupported)
                {
                    InterleaveStereoInt32Avx2(buffer, left, right);
                    return;
                }
                if (Avx.IsSupported)
                {
                    InterleaveStereoInt32Avx(buffer, left, right);
                    return;
                }
                Fallback.InterleaveStereoInt32(buffer, left, right);
            }

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void InterleaveStereoInt32Avx2(Span<int> buffer, ReadOnlySpan<int> left, ReadOnlySpan<int> right)
            {
                unsafe
                {
                    if (left.Length > right.Length) throw new ArgumentException("right must be as long as left!", nameof(right));
                    if (buffer.Length < left.Length * 2) throw new ArgumentException("buffer must be twice as long as left!");
                    if (left.Length < Vector256<int>.Count)
                    {
                        Fallback.InterleaveStereoInt32(buffer, left, right);
                        return;
                    }
                    right = right.SliceWhile(left.Length);
                    buffer = buffer.SliceWhile(left.Length * 2);
                    //These pre-touches may avoid some range checks
                    _ = right[left.Length - 1];
                    _ = buffer[left.Length * 2 - 1];
                    _ = MemoryUtils.CastSplit<int, (Vector256<int>, Vector256<int>)>(buffer, out var rbuffer);
                    var vL = MemoryUtils.CastSplit<int, Vector256<int>>(left, out var rleft);
                    _ = MemoryUtils.CastSplit<int, Vector256<int>>(right, out var rright);
                    ref var rL = ref Unsafe.As<int, Vector256<int>>(ref MemoryMarshal.GetReference(left));
                    ref var rR = ref Unsafe.As<int, Vector256<int>>(ref MemoryMarshal.GetReference(right));
                    ref var rB = ref Unsafe.As<int, (Vector256<int>, Vector256<int>)>(ref MemoryMarshal.GetReference(buffer));
                    var length = ((IntPtr)(vL.Length * sizeof(Vector256<int>))).ToPointer();
                    var j = IntPtr.Zero;
                    for (var i = IntPtr.Zero; i.ToPointer() < length; i += sizeof(Vector256<int>))
                    {
                        var ymm0 = Unsafe.AddByteOffset(ref rL, i);
                        var ymm1 = Unsafe.AddByteOffset(ref rR, i);
                        var x = Avx2.UnpackLow(ymm0, ymm1);
                        var y = Avx2.UnpackHigh(ymm0, ymm1);
                        var d = Avx2.Permute2x128(x, y, 0x20);
                        var q = Avx2.Permute2x128(x, y, 0x31);
                        Unsafe.AddByteOffset(ref rB, j) = (d, q);
                        j += 2 * sizeof(Vector256<int>);
                    }
                    Fallback.InterleaveStereoInt32(rbuffer, rleft, rright);
                }
            }

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void InterleaveStereoInt32Avx(Span<int> buffer, ReadOnlySpan<int> left, ReadOnlySpan<int> right)
            {
                unsafe
                {
                    if (left.Length > right.Length) throw new ArgumentException("right must be as long as left!", nameof(right));
                    if (buffer.Length < left.Length * 2) throw new ArgumentException("buffer must be twice as long as left!");
                    if (left.Length < Vector256<int>.Count)
                    {
                        Fallback.InterleaveStereoInt32(buffer, left, right);
                        return;
                    }
                    right = right.SliceWhile(left.Length);
                    buffer = buffer.SliceWhile(left.Length * 2);
                    //These pre-touches may avoid some range checks
                    _ = right[left.Length - 1];
                    _ = buffer[left.Length * 2 - 1];
                    _ = MemoryUtils.CastSplit<int, (Vector256<float>, Vector256<float>)>(buffer, out var rbuffer);
                    var vL = MemoryUtils.CastSplit<int, Vector256<float>>(left, out var rleft);
                    _ = MemoryUtils.CastSplit<int, Vector256<float>>(right, out var rright);
                    ref var rL = ref Unsafe.As<int, Vector256<float>>(ref MemoryMarshal.GetReference(left));
                    ref var rR = ref Unsafe.As<int, Vector256<float>>(ref MemoryMarshal.GetReference(right));
                    ref var rB = ref Unsafe.As<int, (Vector256<float>, Vector256<float>)>(ref MemoryMarshal.GetReference(buffer));
                    var length = ((IntPtr)(vL.Length * sizeof(Vector256<float>))).ToPointer();
                    var j = IntPtr.Zero;
                    for (var i = IntPtr.Zero; i.ToPointer() < length; i += sizeof(Vector256<float>))
                    {
                        var ymm0 = Unsafe.AddByteOffset(ref rL, i);
                        var ymm1 = Unsafe.AddByteOffset(ref rR, i);
                        var x = Avx.UnpackLow(ymm0, ymm1);
                        var y = Avx.UnpackHigh(ymm0, ymm1);
                        var d = Avx.Permute2x128(x, y, 0b00100000);
                        var q = Avx.Permute2x128(x, y, 0b00110001);
                        Unsafe.AddByteOffset(ref rB, j) = (d, q);
                        j += 2 * sizeof(Vector256<float>);
                    }
                    Fallback.InterleaveStereoInt32(rbuffer, rleft, rright);
                }
            }

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void InterleaveStereoSingleAvx(Span<float> buffer, ReadOnlySpan<float> left, ReadOnlySpan<float> right)
            {
                unsafe
                {
                    if (left.Length > right.Length) throw new ArgumentException("right must be as long as left!", nameof(right));
                    if (buffer.Length < left.Length * 2) throw new ArgumentException("buffer must be twice as long as left!");
                    if (left.Length < Vector256<int>.Count)
                    {
                        Fallback.InterleaveStereoSingle(buffer, left, right);
                        return;
                    }
                    right = right.SliceWhile(left.Length);
                    buffer = buffer.SliceWhile(left.Length * 2);
                    //These pre-touches may avoid some range checks
                    _ = right[left.Length - 1];
                    _ = buffer[left.Length * 2 - 1];
                    _ = MemoryUtils.CastSplit<float, (Vector256<float>, Vector256<float>)>(buffer, out var rbuffer);
                    var vL = MemoryUtils.CastSplit<float, Vector256<float>>(left, out var rleft);
                    _ = MemoryUtils.CastSplit<float, Vector256<float>>(right, out var rright);
                    ref var rL = ref Unsafe.As<float, Vector256<float>>(ref MemoryMarshal.GetReference(left));
                    ref var rR = ref Unsafe.As<float, Vector256<float>>(ref MemoryMarshal.GetReference(right));
                    ref var rB = ref Unsafe.As<float, (Vector256<float>, Vector256<float>)>(ref MemoryMarshal.GetReference(buffer));
                    var length = ((IntPtr)(vL.Length * sizeof(Vector256<float>))).ToPointer();
                    var j = IntPtr.Zero;
                    for (var i = IntPtr.Zero; i.ToPointer() < length; i += sizeof(Vector256<float>))
                    {
                        var ymm0 = Unsafe.AddByteOffset(ref rL, i);
                        var ymm1 = Unsafe.AddByteOffset(ref rR, i);
                        var x = Avx.UnpackLow(ymm0, ymm1);
                        var y = Avx.UnpackHigh(ymm0, ymm1);
                        var d = Avx.Permute2x128(x, y, 0b00100000);
                        var q = Avx.Permute2x128(x, y, 0b00110001);
                        Unsafe.AddByteOffset(ref rB, j) = (d, q);
                        j += 2 * sizeof(Vector256<float>);
                    }
                    Fallback.InterleaveStereoSingle(rbuffer, rleft, rright);
                }
            }

            #endregion Stereo

            #region Three

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void InterleaveThreeInt32(Span<int> buffer, ReadOnlySpan<int> left, ReadOnlySpan<int> right, ReadOnlySpan<int> center)
            {
                if (Avx2.IsSupported)
                {
                    InterleaveThreeInt32Avx2(buffer, left, right, center);
                    return;
                }
                Fallback.InterleaveThreeInt32(buffer, left, right, center);
            }

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void InterleaveThreeInt32Avx2(Span<int> buffer, ReadOnlySpan<int> left, ReadOnlySpan<int> right, ReadOnlySpan<int> center)
            {
                unsafe
                {
                    if (left.Length > right.Length) throw new ArgumentException("right must be as long as left!", nameof(right));
                    if (left.Length > center.Length) throw new ArgumentException("center must be as long as left!", nameof(center));
                    if (buffer.Length < left.Length * 3) throw new ArgumentException("buffer must be 3 times as long as left!");
                    if (left.Length < Vector256<int>.Count)
                    {
                        Fallback.InterleaveThreeInt32(buffer, left, right, center);
                        return;
                    }
                    right = right.SliceWhile(left.Length);
                    center = center.SliceWhile(left.Length);
                    buffer = buffer.SliceWhile(left.Length * 3);
                    //These pre-touches may avoid some range checks
                    _ = MemoryUtils.CastSplit<int, (Vector256<int>, Vector256<int>, Vector256<int>)>(buffer, out var rbuffer);
                    var vL = MemoryUtils.CastSplit<int, Vector256<int>>(left, out var rleft);
                    _ = MemoryUtils.CastSplit<int, Vector256<int>>(right, out var rright);
                    _ = MemoryUtils.CastSplit<int, Vector256<int>>(center, out var rcenter);
                    ref var rL = ref Unsafe.As<int, Vector256<int>>(ref MemoryMarshal.GetReference(left));
                    ref var rR = ref Unsafe.As<int, Vector256<int>>(ref MemoryMarshal.GetReference(right));
                    ref var rC = ref Unsafe.As<int, Vector256<int>>(ref MemoryMarshal.GetReference(center));
                    ref var rB = ref Unsafe.As<int, (Vector256<int>, Vector256<int>, Vector256<int>)>(ref MemoryMarshal.GetReference(buffer));
                    var length = ((IntPtr)(vL.Length * sizeof(Vector256<int>))).ToPointer();
                    var j = IntPtr.Zero;
                    var ymm13 = Vector256.Create(0, 0, 0, 1, 1, 1, 2, 2);
                    var ymm14 = Vector256.Create(2, 3, 3, 3, 4, 4, 4, 5);
                    var ymm15 = Vector256.Create(5, 5, 6, 6, 6, 7, 7, 7);

                    for (var i = IntPtr.Zero; i.ToPointer() < length; i += sizeof(Vector256<int>))
                    {
                        var ymm2 = Unsafe.AddByteOffset(ref rL, i);
                        var ymm5 = Unsafe.AddByteOffset(ref rR, i);
                        var ymm8 = Unsafe.AddByteOffset(ref rC, i);
                        var ymm0 = Avx2.PermuteVar8x32(ymm2, ymm13);    //00011122
                        var ymm1 = Avx2.PermuteVar8x32(ymm2, ymm14);    //23334445
                        ymm2 = Avx2.PermuteVar8x32(ymm2, ymm15);        //55666777
                        var ymm3 = Avx2.PermuteVar8x32(ymm5, ymm13);    //888999aa
                        var ymm4 = Avx2.PermuteVar8x32(ymm5, ymm14);    //abbbcccd
                        ymm5 = Avx2.PermuteVar8x32(ymm5, ymm15);        //ddeeefff
                        var ymm6 = Avx2.PermuteVar8x32(ymm8, ymm13);    //ggghhhii
                        var ymm7 = Avx2.PermuteVar8x32(ymm8, ymm14);    //ijjjkkkl
                        ymm8 = Avx2.PermuteVar8x32(ymm8, ymm15);        //llmmmnnn
                        ymm0 = Avx2.Blend(ymm0, ymm3, 0b10110110);
                        ymm1 = Avx2.Blend(ymm1, ymm4, 0b00100100);
                        ymm2 = Avx2.Blend(ymm2, ymm5, 0b11011011);
                        ymm0 = Avx2.Blend(ymm0, ymm6, 0b00100100);
                        ymm1 = Avx2.Blend(ymm1, ymm7, 0b01001001);
                        ymm2 = Avx2.Blend(ymm2, ymm8, 0b10010010);
                        Unsafe.AddByteOffset(ref rB, j) = (ymm0, ymm1, ymm2);
                        j += 3 * sizeof(Vector256<int>);
                    }
                    if (rleft.Length > 0)
                    {
                        Fallback.InterleaveThreeInt32(rbuffer, rleft, rright, rcenter);
                    }
                }
            }

            #endregion Three

            #region Quad

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void InterleaveQuadInt32(Span<int> buffer, ReadOnlySpan<int> frontLeft, ReadOnlySpan<int> frontRight, ReadOnlySpan<int> rearLeft, ReadOnlySpan<int> rearRight)
            {
                if (Avx2.IsSupported)
                {
                    InterleaveQuadInt32Avx2(buffer, frontLeft, frontRight, rearLeft, rearRight);
                    return;
                }
                Fallback.InterleaveQuadInt32(buffer, frontLeft, frontRight, rearLeft, rearRight);
            }

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void InterleaveQuadInt32Avx2(Span<int> buffer, ReadOnlySpan<int> frontLeft, ReadOnlySpan<int> frontRight, ReadOnlySpan<int> rearLeft, ReadOnlySpan<int> rearRight)
            {
                unsafe
                {
                    const int Channels = 4;
                    if (frontLeft.Length > frontRight.Length) throw new ArgumentException("right must be as long as left!", nameof(frontRight));
                    if (frontLeft.Length > rearLeft.Length) throw new ArgumentException("center must be as long as left!", nameof(rearLeft));
                    if (frontLeft.Length > rearRight.Length) throw new ArgumentException("center must be as long as left!", nameof(rearRight));
                    if (buffer.Length < frontLeft.Length * Channels) throw new ArgumentException("buffer must be 4 times as long as left!");
                    //These pre-touches may avoid some range checks
                    _ = frontRight[frontLeft.Length - 1];
                    _ = rearLeft[frontLeft.Length - 1];
                    _ = rearRight[frontLeft.Length - 1];
                    _ = buffer[frontLeft.Length * Channels - 1];
                    frontRight = frontRight.SliceWhile(frontLeft.Length);
                    rearLeft = rearLeft.SliceWhile(frontLeft.Length);
                    rearRight = rearRight.SliceWhile(frontLeft.Length);
                    buffer = buffer.SliceWhile(frontLeft.Length * Channels);
                    //These pre-touches may avoid some range checks
                    _ = frontRight[frontLeft.Length - 1];
                    _ = rearLeft[frontLeft.Length - 1];
                    _ = rearRight[frontLeft.Length - 1];
                    _ = buffer[frontLeft.Length * 2 - 1];
                    _ = MemoryUtils.CastSplit<int, (Vector256<int>, Vector256<int>, Vector256<int>, Vector256<int>)>(buffer, out var rbuffer);
                    var vL = MemoryUtils.CastSplit<int, Vector256<int>>(frontLeft, out var rfleft);
                    _ = MemoryUtils.CastSplit<int, Vector256<int>>(frontRight, out var rfright);
                    _ = MemoryUtils.CastSplit<int, Vector256<int>>(rearLeft, out var rrleft);
                    _ = MemoryUtils.CastSplit<int, Vector256<int>>(rearRight, out var rrright);
                    ref var rFL = ref Unsafe.As<int, Vector256<int>>(ref MemoryMarshal.GetReference(frontLeft));
                    ref var rFR = ref Unsafe.As<int, Vector256<int>>(ref MemoryMarshal.GetReference(frontRight));
                    ref var rRL = ref Unsafe.As<int, Vector256<int>>(ref MemoryMarshal.GetReference(rearLeft));
                    ref var rRR = ref Unsafe.As<int, Vector256<int>>(ref MemoryMarshal.GetReference(rearRight));
                    ref var rB = ref Unsafe.As<int, Vector256<int>>(ref MemoryMarshal.GetReference(buffer));
                    nint length = vL.Length;
                    nint j = 0;
                    for (nint i = 0; i < length; i++)
                    {
                        var ymm0 = Unsafe.Add(ref rFL, i);
                        var ymm1 = Unsafe.Add(ref rFR, i);
                        var ymm2 = Unsafe.Add(ref rRL, i);
                        var ymm3 = Unsafe.Add(ref rRR, i);
                        var x = Avx2.UnpackLow(ymm0, ymm1);
                        var y = Avx2.UnpackHigh(ymm0, ymm1);
                        var z = Avx2.UnpackLow(ymm2, ymm3);
                        var w = Avx2.UnpackHigh(ymm2, ymm3);
                        var d = Avx2.Permute2x128(x, y, 0x20).AsInt64();
                        var p = Avx2.Permute2x128(z, w, 0x20).AsInt64();
                        var q = Avx2.Permute2x128(x, y, 0x31).AsInt64();
                        var b = Avx2.Permute2x128(z, w, 0x31).AsInt64();
                        var h0 = Avx2.UnpackLow(d, p);
                        var h1 = Avx2.UnpackHigh(d, p);
                        var h2 = Avx2.UnpackLow(q, b);
                        var h3 = Avx2.UnpackHigh(q, b);
                        var s0 = Avx2.Permute2x128(h0, h1, 0x20).AsInt32();
                        var s1 = Avx2.Permute2x128(h0, h1, 0x31).AsInt32();
                        var s2 = Avx2.Permute2x128(h2, h3, 0x20).AsInt32();
                        var s3 = Avx2.Permute2x128(h2, h3, 0x31).AsInt32();
                        Unsafe.AddByteOffset(ref rB, j + sizeof(Vector256<int>) * 0) = s0;
                        Unsafe.AddByteOffset(ref rB, j + sizeof(Vector256<int>) * 1) = s1;
                        Unsafe.AddByteOffset(ref rB, j + sizeof(Vector256<int>) * 2) = s2;
                        Unsafe.AddByteOffset(ref rB, j + sizeof(Vector256<int>) * 3) = s3;
                        j += 4 * sizeof(Vector256<int>);
                    }
                    if (rfleft.Length > 0) Fallback.InterleaveQuadInt32(rbuffer, rfleft, rfright, rrleft, rrright);
                }
            }

            #endregion Quad
        }
    }
}

#endif
