#if NETCOREAPP3_1_OR_GREATER

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Intrinsics;

namespace Shamisen.Codecs.Flac
{
    public static partial class FlacSideStereoUtils
    {
        internal static class X86
        {
            internal static bool IsSupported =>
#if NET5_0_OR_GREATER
                X86Base.IsSupported;

#else
                Sse.IsSupported;
#endif

            #region LeftSide

            internal static void DecodeAndInterleaveLeftSideStereoInt32(Span<int> buffer, ReadOnlySpan<int> left, ReadOnlySpan<int> right)
            {
                if (Avx2.IsSupported)
                {
                    DecodeAndInterleaveLeftSideStereoInt32Avx2(buffer, left, right);
                    return;
                }
                Fallback.DecodeAndInterleaveLeftSideStereoInt32(buffer, left, right);
            }

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void DecodeAndInterleaveLeftSideStereoInt32Avx2(Span<int> buffer, ReadOnlySpan<int> left, ReadOnlySpan<int> right)
            {
                unsafe
                {
                    if (left.Length > right.Length) throw new ArgumentException("right must be as long as left!", nameof(right));
                    if (buffer.Length < left.Length * 2) throw new ArgumentException("buffer must be twice as long as left!");
                    if (left.Length < Vector256<int>.Count)
                    {
                        Fallback.DecodeAndInterleaveLeftSideStereoInt32(buffer, left, right);
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
                        ymm1 = Avx2.Subtract(ymm0, ymm1);
                        var x = Avx2.UnpackLow(ymm0, ymm1);
                        var y = Avx2.UnpackHigh(ymm0, ymm1);
                        var d = Avx2.Permute2x128(x, y, 0x20);
                        var q = Avx2.Permute2x128(x, y, 0x31);
                        Unsafe.AddByteOffset(ref rB, j) = (d, q);
                        j += 2 * sizeof(Vector256<int>);
                    }
                    Fallback.DecodeAndInterleaveLeftSideStereoInt32(rbuffer, rleft, rright);
                }
            }

            #endregion LeftSide

            #region RightSide

            internal static void DecodeAndInterleaveRightSideStereoInt32(Span<int> buffer, ReadOnlySpan<int> left, ReadOnlySpan<int> right)
            {
                if (Avx2.IsSupported)
                {
                    DecodeAndInterleaveRightSideStereoInt32Avx2(buffer, left, right);
                    return;
                }
                Fallback.DecodeAndInterleaveRightSideStereoInt32(buffer, left, right);
            }

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void DecodeAndInterleaveRightSideStereoInt32Avx2(Span<int> buffer, ReadOnlySpan<int> left, ReadOnlySpan<int> right)
            {
                unsafe
                {
                    unsafe
                    {
                        if (left.Length > right.Length) throw new ArgumentException("right must be as long as left!", nameof(right));
                        if (buffer.Length < left.Length * 2) throw new ArgumentException("buffer must be twice as long as left!");
                        if (left.Length < Vector256<int>.Count)
                        {
                            Fallback.DecodeAndInterleaveRightSideStereoInt32(buffer, left, right);
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
                            ymm0 = Avx2.Add(ymm0, ymm1);
                            var x = Avx2.UnpackLow(ymm0, ymm1);
                            var y = Avx2.UnpackHigh(ymm0, ymm1);
                            var d = Avx2.Permute2x128(x, y, 0x20);
                            var q = Avx2.Permute2x128(x, y, 0x31);
                            Unsafe.AddByteOffset(ref rB, j) = (d, q);
                            j += 2 * sizeof(Vector256<int>);
                        }
                        Fallback.DecodeAndInterleaveRightSideStereoInt32(rbuffer, rleft, rright);
                    }
                }
            }

            #endregion RightSide

            #region MidSide

            internal static void DecodeAndInterleaveMidSideStereoInt32(Span<int> buffer, ReadOnlySpan<int> left, ReadOnlySpan<int> right)
            {
                if (Avx2.IsSupported)
                {
                    DecodeAndInterleaveMidSideStereoInt32Avx2(buffer, left, right);
                    return;
                }
                Fallback.DecodeAndInterleaveMidSideStereoInt32(buffer, left, right);
            }

            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            internal static void DecodeAndInterleaveMidSideStereoInt32Avx2(Span<int> buffer, ReadOnlySpan<int> left, ReadOnlySpan<int> right)
            {
                unsafe
                {
                    unsafe
                    {
                        if (left.Length > right.Length) throw new ArgumentException("right must be as long as left!", nameof(right));
                        if (buffer.Length < left.Length * 2) throw new ArgumentException("buffer must be twice as long as left!");
                        if (left.Length < Vector256<int>.Count)
                        {
                            Fallback.DecodeAndInterleaveMidSideStereoInt32(buffer, left, right);
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
                        var ymm15 = Vector256.Create(1);
                        for (var i = IntPtr.Zero; i.ToPointer() < length; i += sizeof(Vector256<int>))
                        {
                            var ymm0 = Unsafe.AddByteOffset(ref rL, i);
                            var ymm1 = Unsafe.AddByteOffset(ref rR, i);
                            ymm0 = Avx2.Add(ymm0, ymm0);
                            var ymm2 = Avx2.And(ymm1, ymm15);
                            ymm0 = Avx2.Or(ymm0, ymm2);
                            var ymm3 = Avx2.Add(ymm0, ymm1);
                            var ymm4 = Avx2.Subtract(ymm0, ymm1);
                            ymm3 = Avx2.ShiftRightArithmetic(ymm3, 1);
                            ymm4 = Avx2.ShiftRightArithmetic(ymm4, 1);
                            var x = Avx2.UnpackLow(ymm3, ymm4);
                            var y = Avx2.UnpackHigh(ymm3, ymm4);
                            var d = Avx2.Permute2x128(x, y, 0x20);
                            var q = Avx2.Permute2x128(x, y, 0x31);
                            Unsafe.AddByteOffset(ref rB, j) = (d, q);
                            j += 2 * sizeof(Vector256<int>);
                        }
                        Fallback.DecodeAndInterleaveMidSideStereoInt32(rbuffer, rleft, rright);
                    }
                }
            }

            #endregion MidSide
        }
    }
}

#endif
