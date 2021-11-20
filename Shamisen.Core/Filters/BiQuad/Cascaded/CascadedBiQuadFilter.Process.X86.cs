#if NETCOREAPP3_1_OR_GREATER
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Filters.BiQuad.Cascaded
{
    public sealed partial class CascadedBiQuadFilter
    {
        internal static partial class Process
        {
            internal static partial class X86
            {
                /// <summary>
                /// Processes the signal with 5-8 order of cascaded BiQuad filters with the technique called "software pipelining".<br/>
                /// Software pipelining enables us to implement cascaded BiQuad parallelization as we did in scalar version of single BiQuad filter.<br/>
                /// The bypass can be represented by 1.0f for b0 and 0.0f for b1, b2, a1, and a2.<br/>
                /// The orders more than 8 should be processed with serially cascaded function calls.
                /// </summary>
                /// <param name="target"></param>
                /// <param name="parameters"></param>
                /// <param name="internalStates"></param>
                internal static void ProcessMonauralOrder8Avx2(Span<float> target, Span<float> parameters, Span<float> internalStates)
                {
                    const int MaxOrder = 8;
                    ref var p = ref MemoryMarshal.GetReference(parameters);
                    ref var s = ref MemoryMarshal.GetReference(internalStates);
                    var ymm15 = Vector256.Create(1, 2, 3, 4, 5, 6, 7, 0);
                    var b0 = Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref p, MaxOrder * 0));  //ymm14
                    var b1 = Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref p, MaxOrder * 1));  //ymm13
                    var b2 = Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref p, MaxOrder * 2));  //ymm12
                    var a1 = Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref p, MaxOrder * 3));  //ymm11
                    var a2 = Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref p, MaxOrder * 4));  //ymm10
                    //The name "d0"(delay-0) is preferred over "z0" since SVE and AVX-512 uses "z0_ns" and "zmm0" in their register name, respectively.
                    var d0 = Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref s, MaxOrder * 0));  //ymm0
                    var d1 = Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref s, MaxOrder * 1));  //ymm1
                    var ymm2 = Vector256.Create(0f);    //stores a temporal output of each BiQuad filters.
                    ref var rdi = ref MemoryMarshal.GetReference(target);

                    nint i = 0, length = target.Length;
                    //TODO: corner cases that length is less than 16.
                    var olen = MathI.Min(7, length);
                    //startup
                    for (; i < olen; i++)
                    {
                        var ymm3 = Vector256.CreateScalarUnsafe(Unsafe.Add(ref rdi, i));
                        ymm3 = Avx2.PermuteVar8x32(ymm3, ymm15);
                        var ymm4 = Avx.Blend(ymm2, ymm3, 0b1000_0000);
                        ymm2 = Avx.Multiply(ymm4, b0);
                        var ymm5 = Avx.Multiply(ymm4, b1);
                        ymm2 = Avx.Add(ymm2, d0);
                        var ymm6 = Avx.Multiply(ymm2, a1);
                        d0 = Avx.Add(ymm5, d1);
                        ymm5 = Avx.Multiply(ymm4, b2);
                        ymm4 = Avx.Multiply(ymm2, a2);
                        ymm2 = Avx2.PermuteVar8x32(ymm2, ymm15);
                        d0 = Avx.Add(d0, ymm6);
                        d1 = Avx.Add(ymm5, ymm4);
                    }
                    //main loop
                    olen = length;
                    for (; i < olen; i++)
                    {
                        var ymm3 = Vector256.CreateScalarUnsafe(Unsafe.Add(ref rdi, i));
                        ymm3 = Avx2.PermuteVar8x32(ymm3, ymm15);
                        var ymm4 = Avx.Blend(ymm2, ymm3, 0b1000_0000);
                        ymm2 = Avx.Multiply(ymm4, b0);
                        ymm2 = Avx.Add(ymm2, d0);
                        Unsafe.Add(ref rdi, i - 7) = ymm2.GetElement(0);
                        var ymm5 = Avx.Multiply(ymm4, b1);
                        var ymm6 = Avx.Multiply(ymm2, a1);
                        d0 = Avx.Add(ymm5, d1);
                        ymm5 = Avx.Multiply(ymm4, b2);
                        d0 = Avx.Add(d0, ymm6);
                        ymm6 = Avx.Multiply(ymm2, a2);
                        ymm2 = Avx2.PermuteVar8x32(ymm2, ymm15);
                        d1 = Avx.Add(ymm5, ymm6);
                    }
                    i -= 7;
                    for (; i < 0; i++)  //for the cases where length is less than 8.
                    {
                        var ymm4 = ymm2;
                        ymm2 = Avx.Multiply(ymm4, b0);
                        ymm2 = Avx.Add(ymm2, d0);
                        var ymm5 = Avx.Multiply(ymm4, b1);
                        var ymm6 = Avx.Multiply(ymm2, a1);
                        d0 = Avx.Add(ymm5, d1);
                        ymm5 = Avx.Multiply(ymm4, b2);
                        d0 = Avx.Add(d0, ymm6);
                        ymm6 = Avx.Multiply(ymm2, a2);
                        ymm2 = Avx2.PermuteVar8x32(ymm2, ymm15);
                        d1 = Avx.Add(ymm5, ymm6);
                    }
                    for (; i < length; i++)
                    {
                        var ymm4 = ymm2;
                        ymm2 = Avx.Multiply(ymm4, b0);
                        ymm2 = Avx.Add(ymm2, d0);
                        Unsafe.Add(ref rdi, i) = ymm2.GetElement(0);
                        var ymm5 = Avx.Multiply(ymm4, b1);
                        var ymm6 = Avx.Multiply(ymm2, a1);
                        d0 = Avx.Add(ymm5, d1);
                        ymm5 = Avx.Multiply(ymm4, b2);
                        d0 = Avx.Add(d0, ymm6);
                        ymm6 = Avx.Multiply(ymm2, a2);
                        ymm2 = Avx2.PermuteVar8x32(ymm2, ymm15);
                        d1 = Avx.Add(ymm5, ymm6);
                    }
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref s, MaxOrder * 0)) = d0;
                    Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref s, MaxOrder * 1)) = d1;
                }
                /// <summary>
                /// Processes the signal with 2-4 order of cascaded BiQuad filters with the technique called "software pipelining".<br/>
                /// Single BiQuad filters must be processed with <see cref="BiQuadFilter"/>.<br/>
                /// Software pipelining enables us to implement cascaded BiQuad parallelization as we did in scalar version of single BiQuad filter.<br/>
                /// The bypass can be represented by 1.0f for b0 and 0.0f for b1, b2, a1, and a2.
                /// </summary>
                /// <param name="target"></param>
                /// <param name="parameters"></param>
                /// <param name="internalStates"></param>
                internal static void ProcessMonauralOrder4Sse41(Span<float> target, Span<float> parameters, Span<float> internalStates)
                {
                    const int MaxOrder = 4;
                    ref var p = ref MemoryMarshal.GetReference(parameters);
                    ref var s = ref MemoryMarshal.GetReference(internalStates);
                    var b0 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref p, MaxOrder * 0));  //xmm15
                    var b1 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref p, MaxOrder * 1));  //xmm14
                    var b2 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref p, MaxOrder * 2));  //xmm13
                    var a1 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref p, MaxOrder * 3));  //xmm12
                    var a2 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref p, MaxOrder * 4));  //xmm11
                    //The name "d0"(delay-0) is preferred over "z0" since SVE and AVX-512 uses "z0_ns" and "zmm0" in their register name, respectively.
                    var d0 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref s, MaxOrder * 0));  //xmm0
                    var d1 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref s, MaxOrder * 1));  //xmm1
                    var xmm2 = Vector128.Create(0f);    //stores a temporal output of each BiQuad filters.
                    ref var rdi = ref MemoryMarshal.GetReference(target);

                    nint i = 0, length = target.Length;
                    var olen = MathI.Min(3, length);
                    //startup
                    for (; i < olen; i++)
                    {
                        var xmm3 = Vector128.CreateScalarUnsafe(Unsafe.Add(ref rdi, i));
                        xmm3 = Sse.Shuffle(xmm3, xmm3, 0b00_11_10_01);
                        var xmm4 = Sse41.Blend(xmm2, xmm3, 0b1000_0000);
                        xmm2 = Sse.Multiply(xmm4, b0);
                        var xmm5 = Sse.Multiply(xmm4, b1);
                        xmm2 = Sse.Add(xmm2, d0);
                        var xmm6 = Sse.Multiply(xmm2, a1);
                        d0 = Sse.Add(xmm5, d1);
                        xmm5 = Sse.Multiply(xmm4, b2);
                        xmm4 = Sse.Multiply(xmm2, a2);
                        xmm2 = Sse.Shuffle(xmm2, xmm2, 0b00_11_10_01);
                        d0 = Sse.Add(d0, xmm6);
                        d1 = Sse.Add(xmm5, xmm4);
                    }
                    //main loop
                    olen = length;
                    for (; i < olen; i++)
                    {
                        var xmm3 = Vector128.CreateScalarUnsafe(Unsafe.Add(ref rdi, i));
                        xmm3 = Sse.Shuffle(xmm3, xmm3, 0b00_11_10_01);
                        var xmm4 = Sse41.Blend(xmm2, xmm3, 0b1000_0000);
                        xmm2 = Sse.Multiply(xmm4, b0);
                        xmm2 = Sse.Add(xmm2, d0);
                        Unsafe.Add(ref rdi, i - 3) = xmm2.GetElement(0);
                        var xmm5 = Sse.Multiply(xmm4, b1);
                        var xmm6 = Sse.Multiply(xmm2, a1);
                        d0 = Sse.Add(xmm5, d1);
                        xmm5 = Sse.Multiply(xmm4, b2);
                        d0 = Sse.Add(d0, xmm6);
                        xmm6 = Sse.Multiply(xmm2, a2);
                        xmm2 = Sse.Shuffle(xmm2, xmm2, 0b00_11_10_01);
                        d1 = Sse.Add(xmm5, xmm6);
                    }
                    //i now points to the place to write.
                    i -= 3;
                    for (; i < 0; i++)  //for the cases where length is less than 4.
                    {
                        var xmm4 = xmm2;
                        xmm2 = Sse.Multiply(xmm4, b0);
                        xmm2 = Sse.Add(xmm2, d0);
                        var xmm5 = Sse.Multiply(xmm4, b1);
                        var xmm6 = Sse.Multiply(xmm2, a1);
                        d0 = Sse.Add(xmm5, d1);
                        xmm5 = Sse.Multiply(xmm4, b2);
                        d0 = Sse.Add(d0, xmm6);
                        xmm6 = Sse.Multiply(xmm2, a2);
                        xmm2 = Sse.Shuffle(xmm2, xmm2, 0b00_11_10_01);
                        d1 = Sse.Add(xmm5, xmm6);
                    }
                    for (; i < length; i++)
                    {
                        var xmm4 = xmm2;
                        xmm2 = Sse.Multiply(xmm4, b0);
                        xmm2 = Sse.Add(xmm2, d0);
                        Unsafe.Add(ref rdi, i) = xmm2.GetElement(0);
                        var xmm5 = Sse.Multiply(xmm4, b1);
                        var xmm6 = Sse.Multiply(xmm2, a1);
                        d0 = Sse.Add(xmm5, d1);
                        xmm5 = Sse.Multiply(xmm4, b2);
                        d0 = Sse.Add(d0, xmm6);
                        xmm6 = Sse.Multiply(xmm2, a2);
                        xmm2 = Sse.Shuffle(xmm2, xmm2, 0b00_11_10_01);
                        d1 = Sse.Add(xmm5, xmm6);
                    }
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref s, 8 * 0)) = d0;
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref s, 8 * 1)) = d1;
                }
            }
        }
    }
}
#endif