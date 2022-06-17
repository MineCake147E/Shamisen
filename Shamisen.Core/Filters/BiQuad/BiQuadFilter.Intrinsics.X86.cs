#if NETCOREAPP3_1_OR_GREATER

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

using Shamisen.Utils;
using Shamisen.Utils.Intrinsics;

namespace Shamisen.Filters
{
    public sealed partial class BiQuadFilter
    {
        internal static class X86
        {
            internal static void ProcessMonauralAvx2Fma(Span<float> buffer, BiQuadParameter parameter, Span<Vector2> states)
            {
                unsafe
                {
                    Debug.Assert(!states.IsEmpty);
                    Debug.Assert(!buffer.IsEmpty);
                    var factorB = parameter.B;
                    var factorA = parameter.A;
                    ref var rdi = ref MemoryMarshal.GetReference(buffer);
                    var xmm1 = Vector128.CreateScalarUnsafe(Unsafe.As<Vector3, double>(ref factorB)).AsSingle();
                    var xmm3 = Vector128.CreateScalarUnsafe(factorB.Z);
                    var xmm0 = Vector128.CreateScalarUnsafe(Unsafe.As<Vector2, double>(ref factorA)).AsSingle();
                    var xmm2 = Vector128.CreateScalarUnsafe(Unsafe.As<Vector2, double>(ref states[0])).AsSingle();
                    //LLVM has found a way to vectorize monaural BiQuad filtering in transposed form 2.
                    nint i, length = buffer.Length;
                    var olen = length - 1;
                    for (i = 0; i < olen; i += 2)
                    {
                        var xmm4 = Vector128.Create(Unsafe.Add(ref rdi, i));
                        xmm2 = Fma.MultiplyAdd(xmm1, xmm4, xmm2);
                        xmm4 = Sse.MultiplyScalar(xmm4, xmm3);
                        Unsafe.Add(ref rdi, i) = xmm2.GetElement(0);
                        var xmm5 = Avx2.BroadcastScalarToVector128(xmm2);
                        xmm2 = Sse3.MoveHighAndDuplicate(xmm2);
                        xmm2 = Sse41.Insert(xmm2, xmm4, 0x10);
                        xmm2 = Fma.MultiplyAdd(xmm0, xmm5, xmm2);
                        xmm4 = Vector128.Create(Unsafe.Add(ref rdi, i + 1));
                        xmm2 = Fma.MultiplyAdd(xmm1, xmm4, xmm2);
                        xmm4 = Sse.MultiplyScalar(xmm4, xmm3);
                        Unsafe.Add(ref rdi, i + 1) = xmm2.GetElement(0);
                        xmm5 = Avx2.BroadcastScalarToVector128(xmm2);
                        xmm2 = Sse3.MoveHighAndDuplicate(xmm2);
                        xmm2 = Sse41.Insert(xmm2, xmm4, 0x10);
                        xmm2 = Fma.MultiplyAdd(xmm0, xmm5, xmm2);
                    }
                    if (i < length)
                    {
                        var xmm4 = Vector128.Create(Unsafe.Add(ref rdi, i));
                        xmm2 = Fma.MultiplyAdd(xmm1, xmm4, xmm2);
                        xmm4 = Sse.MultiplyScalar(xmm4, xmm3);
                        Unsafe.Add(ref rdi, i) = xmm2.GetElement(0);
                        var xmm5 = Avx2.BroadcastScalarToVector128(xmm2);
                        xmm2 = Sse3.MoveHighAndDuplicate(xmm2);
                        xmm2 = Sse41.Insert(xmm2, xmm4, 0x10);
                        xmm2 = Fma.MultiplyAdd(xmm0, xmm5, xmm2);
                    }
                    states[0] = xmm2.AsVector2();
                }
            }

            internal static void ProcessMonauralAvx2(Span<float> buffer, BiQuadParameter parameter, Span<Vector2> states)
            {
                unsafe
                {
                    Debug.Assert(!states.IsEmpty);
                    Debug.Assert(!buffer.IsEmpty);
                    var factorB = parameter.B;
                    var factorA = parameter.A;
                    ref var rdi = ref MemoryMarshal.GetReference(buffer);
                    var xmm1 = Vector128.CreateScalarUnsafe(Unsafe.As<Vector3, double>(ref factorB)).AsSingle();
                    var xmm3 = Vector128.CreateScalarUnsafe(factorB.Z);
                    var xmm0 = Vector128.CreateScalarUnsafe(Unsafe.As<Vector2, double>(ref factorA)).AsSingle();
                    var xmm2 = Vector128.CreateScalarUnsafe(Unsafe.As<Vector2, double>(ref states[0])).AsSingle();
                    //LLVM has found a way to vectorize monaural BiQuad filtering in transposed form 2.
                    nint i, length = buffer.Length;
                    var olen = length - 1;
                    for (i = 0; i < olen; i += 2)
                    {
                        var xmm4 = Vector128.Create(Unsafe.Add(ref rdi, i));
                        var xmm6 = Sse.Multiply(xmm1, xmm4);
                        xmm2 = Sse.Add(xmm2, xmm6);
                        xmm4 = Sse.MultiplyScalar(xmm4, xmm3);
                        Unsafe.Add(ref rdi, i) = xmm2.GetElement(0);
                        var xmm5 = Avx2.BroadcastScalarToVector128(xmm2);
                        xmm2 = Sse3.MoveHighAndDuplicate(xmm2);
                        xmm2 = Sse41.Insert(xmm2, xmm4, 0x10);
                        xmm4 = Vector128.Create(Unsafe.Add(ref rdi, i + 1));
                        xmm6 = Sse.Multiply(xmm0, xmm5);
                        xmm2 = Sse.Add(xmm2, xmm6);
                        xmm6 = Sse.Multiply(xmm1, xmm4);
                        xmm2 = Sse.Add(xmm2, xmm6);
                        xmm4 = Sse.MultiplyScalar(xmm4, xmm3);
                        Unsafe.Add(ref rdi, i + 1) = xmm2.GetElement(0);
                        xmm5 = Avx2.BroadcastScalarToVector128(xmm2);
                        xmm2 = Sse3.MoveHighAndDuplicate(xmm2);
                        xmm2 = Sse41.Insert(xmm2, xmm4, 0x10);
                        xmm6 = Sse.Multiply(xmm0, xmm5);
                        xmm2 = Sse.Add(xmm2, xmm6);
                    }
                    if (i < length)
                    {
                        var xmm4 = Vector128.Create(Unsafe.Add(ref rdi, i));
                        var xmm6 = Sse.Multiply(xmm1, xmm4);
                        xmm2 = Sse.Add(xmm2, xmm6);
                        xmm4 = Sse.MultiplyScalar(xmm4, xmm3);
                        Unsafe.Add(ref rdi, i) = xmm2.GetElement(0);
                        var xmm5 = Avx2.BroadcastScalarToVector128(xmm2);
                        xmm2 = Sse3.MoveHighAndDuplicate(xmm2);
                        xmm2 = Sse41.Insert(xmm2, xmm4, 0x10);
                        xmm6 = Sse.Multiply(xmm0, xmm5);
                        xmm2 = Sse.Add(xmm2, xmm6);
                    }
                    states[0] = xmm2.AsVector2();
                }
            }
        }

        private void ProcessMultipleSse(Span<float> buffer)
        {
            if ((internalStates.Length & 1) > 0)
            {
                ProcessMultipleSseOddChannels(buffer);
            }
            else
            {
                ProcessMultipleSseEvenChannels(buffer);
            }
        }

        private static void ProcessMultipleAvx(Span<float> buffer, BiQuadParameter parameter, Span<Vector2> internalStates)
        {
            unsafe
            {
                //Factor localization greatly improved performance
                var factorB = parameter.B;
                var factorA = parameter.A;
                var channels = internalStates.Length;
                var iStateX = stackalloc float[channels * 2];
                var iStateY = iStateX + channels;
                for (var c = 0; c < channels; c++)
                {
                    var g = internalStates[c];
                    iStateX[c] = g.X;
                    iStateY[c] = g.Y;
                }
                nint nchannels = channels;
                var rch = channels & 0x3;
                var mask = Sse2.CompareGreaterThan(Vector128.Create(rch), Vector128.Create(0, 1, 2, 3)).AsSingle();
                var fBX = Vector256.Create(factorB.X);
                var fBY = Vector256.Create(factorB.Y);
                var fBZ = Vector256.Create(factorB.Z);
                var fAX = Vector256.Create(factorA.X);
                var fAY = Vector256.Create(factorA.Y);
                nint i = 0, length = buffer.Length - nchannels + 1;
                var olen = length - nchannels;
                fixed (float* rsi = buffer)
                {
                    for (; i < olen; i += nchannels * 2)
                    {
                        var ihead = rsi + i;
                        nint nch = 0;
                        var cholen = nchannels - 7;
                        for (; nch < cholen; nch += 8)
                        {
                            var ymm0 = *(Vector256<float>*)(ihead + nch);
                            var iSX = *(Vector256<float>*)(iStateX + nch);
                            var iSY = *(Vector256<float>*)(iStateY + nch);
                            var ymm2 = Avx.Multiply(fBX, ymm0);
                            ymm2 = Avx.Add(iSX, ymm2);
                            iSX = Avx.Multiply(fBY, ymm0);
                            iSY = Avx.Add(iSX, iSY);
                            iSX = Avx.Multiply(fBZ, ymm0);
                            *(Vector256<float>*)(ihead + nch) = ymm2;
                            ymm0 = Avx.Multiply(fAX, ymm2);
                            iSY = Avx.Add(ymm0, iSY);
                            ymm2 = Avx.Multiply(fAY, ymm2);
                            ymm2 = Avx.Add(iSX, ymm2);
                            iSX = *(Vector256<float>*)(ihead + nch + nchannels);
                            ymm0 = Avx.Multiply(fBX, iSX);
                            iSY = Avx.Add(ymm0, iSY);
                            ymm0 = Avx.Multiply(fBY, iSX);
                            ymm2 = Avx.Add(ymm0, ymm2);
                            ymm0 = Avx.Multiply(fBZ, iSX);
                            *(Vector256<float>*)(ihead + nch + nchannels) = iSY;
                            iSX = Avx.Multiply(fAX, iSY);
                            *(Vector256<float>*)(iStateX + nch) = Avx.Add(ymm2, iSX);
                            ymm2 = Avx.Multiply(fAY, iSY);
                            *(Vector256<float>*)(iStateY + nch) = Avx.Add(ymm0, ymm2);
                        }
                        cholen = nchannels - 3;
                        for (; nch < cholen; nch += 4)
                        {
                            var xmm0 = *(Vector128<float>*)(ihead + nch);
                            var iSX = *(Vector128<float>*)(iStateX + nch);
                            var iSY = *(Vector128<float>*)(iStateY + nch);
                            var xmm2 = Sse.Multiply(fBX.GetLower(), xmm0);
                            xmm2 = Sse.Add(iSX, xmm2);
                            iSX = Sse.Multiply(fBY.GetLower(), xmm0);
                            iSY = Sse.Add(iSX, iSY);
                            iSX = Sse.Multiply(fBZ.GetLower(), xmm0);
                            *(Vector128<float>*)(ihead + nch) = xmm2;
                            xmm0 = Sse.Multiply(fAX.GetLower(), xmm2);
                            iSY = Sse.Add(xmm0, iSY);
                            xmm2 = Sse.Multiply(fAY.GetLower(), xmm2);
                            xmm2 = Sse.Add(iSX, xmm2);
                            iSX = *(Vector128<float>*)(ihead + nch + nchannels);
                            xmm0 = Sse.Multiply(fBX.GetLower(), iSX);
                            iSY = Sse.Add(xmm0, iSY);
                            xmm0 = Sse.Multiply(fBY.GetLower(), iSX);
                            xmm2 = Sse.Add(xmm0, xmm2);
                            xmm0 = Sse.Multiply(fBZ.GetLower(), iSX);
                            *(Vector128<float>*)(ihead + nch + nchannels) = iSY;
                            iSX = Sse.Multiply(fAX.GetLower(), iSY);
                            *(Vector128<float>*)(iStateX + nch) = Sse.Add(xmm2, iSX);
                            xmm2 = Sse.Multiply(fAY.GetLower(), iSY);
                            *(Vector128<float>*)(iStateY + nch) = Sse.Add(xmm0, xmm2);
                        }
                        if (rch != 0)
                        {
                            var xmm0 = Avx.MaskLoad(ihead + nch, mask);
                            var xmm1 = Avx.MaskLoad(ihead + nch + nchannels, mask);
                            var iSX = Avx.MaskLoad(iStateX + nch, mask);
                            var iSY = Avx.MaskLoad(iStateY + nch, mask);
                            var xmm2 = Sse.Multiply(fBX.GetLower(), xmm0);
                            xmm2 = Sse.Add(iSX, xmm2);
                            iSX = Sse.Multiply(fBY.GetLower(), xmm0);
                            iSY = Sse.Add(iSX, iSY);
                            iSX = Sse.Multiply(fBZ.GetLower(), xmm0);
                            Avx.MaskStore(ihead + nch, mask, xmm2);
                            xmm0 = Sse.Multiply(fAX.GetLower(), xmm2);
                            iSY = Sse.Add(xmm0, iSY);
                            xmm2 = Sse.Multiply(fAY.GetLower(), xmm2);
                            xmm2 = Sse.Add(iSX, xmm2);
                            iSX = xmm1;
                            xmm0 = Sse.Multiply(fBX.GetLower(), iSX);
                            iSY = Sse.Add(xmm0, iSY);
                            xmm0 = Sse.Multiply(fBY.GetLower(), iSX);
                            xmm2 = Sse.Add(xmm0, xmm2);
                            xmm0 = Sse.Multiply(fBZ.GetLower(), iSX);
                            Avx.MaskStore(ihead + nch + nchannels, mask, iSY);
                            iSX = Sse.Multiply(fAX.GetLower(), iSY);
                            Avx.MaskStore(iStateX + nch, mask, Sse.Add(xmm2, iSX));
                            xmm2 = Sse.Multiply(fAY.GetLower(), iSY);
                            Avx.MaskStore(iStateY + nch, mask, Sse.Add(xmm0, xmm2));
                        }
                    }
                    for (; i < length; i += nchannels)
                    {
                        var ihead = rsi + i;
                        nint nch = 0;
                        var cholen = nchannels - 7;
                        for (; nch < cholen; nch += 8)
                        {
                            var ymm0 = *(Vector256<float>*)(ihead + nch);
                            var iSX = *(Vector256<float>*)(iStateX + nch);
                            var iSY = *(Vector256<float>*)(iStateY + nch);
                            var ymm2 = Avx.Multiply(fBX, ymm0);
                            ymm2 = Avx.Add(iSX, ymm2);
                            iSX = Avx.Multiply(fBY, ymm0);
                            iSY = Avx.Add(iSX, iSY);
                            iSX = Avx.Multiply(fBZ, ymm0);
                            *(Vector256<float>*)(ihead + nch) = ymm2;
                            ymm0 = Avx.Multiply(fAX, ymm2);
                            *(Vector256<float>*)(iStateX + nch) = Avx.Add(ymm0, iSY);
                            ymm2 = Avx.Multiply(fAY, ymm2);
                            *(Vector256<float>*)(iStateY + nch) = Avx.Add(iSX, ymm2);
                        }
                        cholen = nchannels - 3;
                        for (; nch < cholen; nch += 4)
                        {
                            var xmm0 = *(Vector128<float>*)(ihead + nch);
                            var iSX = *(Vector128<float>*)(iStateX + nch);
                            var iSY = *(Vector128<float>*)(iStateY + nch);
                            var xmm2 = Sse.Multiply(fBX.GetLower(), xmm0);
                            xmm2 = Sse.Add(iSX, xmm2);
                            iSX = Sse.Multiply(fBY.GetLower(), xmm0);
                            iSY = Sse.Add(iSX, iSY);
                            iSX = Sse.Multiply(fBZ.GetLower(), xmm0);
                            *(Vector128<float>*)(ihead + nch) = xmm2;
                            xmm0 = Sse.Multiply(fAX.GetLower(), xmm2);
                            *(Vector128<float>*)(iStateX + nch) = Sse.Add(xmm0, iSY);
                            xmm2 = Sse.Multiply(fAY.GetLower(), xmm2);
                            *(Vector128<float>*)(iStateY + nch) = Sse.Add(iSX, xmm2);
                        }
                        if (rch != 0)
                        {
                            var xmm0 = Avx.MaskLoad(ihead + nch, mask);
                            var iSX = Avx.MaskLoad(iStateX + nch, mask);
                            var iSY = Avx.MaskLoad(iStateY + nch, mask);
                            var xmm2 = Sse.Multiply(fBX.GetLower(), xmm0);
                            xmm2 = Sse.Add(iSX, xmm2);
                            iSX = Sse.Multiply(fBY.GetLower(), xmm0);
                            iSY = Sse.Add(iSX, iSY);
                            iSX = Sse.Multiply(fBZ.GetLower(), xmm0);
                            Avx.MaskStore(ihead + nch, mask, xmm2);
                            xmm0 = Sse.Multiply(fAX.GetLower(), xmm2);
                            Avx.MaskStore(iStateX + nch, mask, Sse.Add(xmm0, iSY));
                            xmm2 = Sse.Multiply(fAY.GetLower(), xmm2);
                            Avx.MaskStore(iStateY + nch, mask, Sse.Add(iSX, xmm2));
                        }
                    }
                }
                var sX = new Span<float>(iStateX, channels);
                var sY = new Span<float>(iStateY, channels);
                AudioUtils.InterleaveStereo(MemoryMarshal.Cast<Vector2, float>(internalStates), sX, sY);
            }
        }

        private void ProcessMultipleSseOddChannels(Span<float> buffer)
        {
            unsafe
            {
                //Factor localization greatly improved performance
                var factorB = Parameter.B;
                var factorA = Parameter.A;
                Span<Vector128<float>> iState = stackalloc Vector128<float>[internalStates.Length];
                var fB = Vector128.Create(factorB.X, factorB.Y, factorB.Z, 0);
                var fA = Vector128.Create(factorA.X, factorA.Y, 0, 0);
                //Vector128<float> iS = Vector128.Create(iState.X, iState.Y, 0, 0);
                var channels = iState.Length;
                for (var ch = 0; ch < channels; ch++)
                {
                    var g = internalStates[ch];
                    iState[ch] = Vector128.Create(g.X, g.Y, 0, 0);
                }
                ref var iS = ref MemoryMarshal.GetReference(iState);
                for (var i = 0; i < buffer.Length - channels + 1; i += channels)
                {
                    ref var pos = ref buffer[i];
                    //var span = buffer.Slice(i, internalStates.Length);
                    for (var ch = 0; ch < channels; ch++)
                    {
                        //Reference: https://en.wikipedia.org/wiki/Digital_biquad_filter#Transposed_Direct_form_2
                        //Transformed for SIMD awareness.
                        ref var a = ref Unsafe.Add(ref iS, ch);
                        ref var v = ref Unsafe.Add(ref pos, ch);
                        var aa = a;
                        var vv = Vector128.Create(v);
                        vv = Sse.Multiply(vv, fB); //Multiply in one go
                        vv = Sse.Add(vv, aa);
                        var sum1 = vv.GetElement(0);
                        v = sum1;
                        var sum1v = Sse.Shuffle(vv, vv, 0b1111_0000);
                        sum1v = Sse.Multiply(sum1v, fA);  //Multiply in one go
                        var ffv = Sse.Shuffle(vv, vv, 0b1111_1001);
                        a = Sse.Add(ffv, sum1v);
                    }
                }
                for (var ch = 0; ch < channels; ch++)
                {
                    var f = iState[ch];
                    internalStates[ch] = new Vector2(f.GetElement(0), f.GetElement(1));
                }
            }
        }

        private void ProcessMultipleSseEvenChannels(Span<float> buffer)
        {
            unsafe
            {
                //Factor localization greatly improved performance
                var factorB = Parameter.B;
                var factorA = Parameter.A;
                Span<Vector128<float>> iState = stackalloc Vector128<float>[internalStates.Length];
                var fB = Vector128.Create(factorB.X, factorB.Y, factorB.Z, 0);
                var fA = Vector128.Create(factorA.X, factorA.Y, 0, 0);
                //Vector128<float> iS = Vector128.Create(iState.X, iState.Y, 0, 0);
                var channels = iState.Length;
                for (var ch = 0; ch < channels; ch++)
                {
                    var g = internalStates[ch];
                    iState[ch] = Vector128.Create(g.X, g.Y, 0, 0);
                }
                ref var iS = ref MemoryMarshal.GetReference(iState);
                for (var i = 0; i < buffer.Length - channels + 1; i += channels)
                {
                    ref var pos = ref buffer[i];
                    //var span = buffer.Slice(i, internalStates.Length);
                    for (var ch = 0; ch < channels; ch++)
                    {
                        //Reference: https://en.wikipedia.org/wiki/Digital_biquad_filter#Transposed_Direct_form_2
                        //Transformed for SIMD awareness.
                        ref var a = ref Unsafe.Add(ref iS, ch);
                        ref var v = ref Unsafe.Add(ref pos, ch);
                        var aa = a;
                        var vv = Vector128.CreateScalar(v);
                        vv = Sse.Shuffle(vv, vv, 0b1100_0000);
                        vv = Sse.Multiply(vv, fB); //Multiply in one go
                        vv = Sse.Add(vv, aa);
                        var sum1 = vv.GetElement(0);
                        v = sum1;
                        var sum1v = Sse.Shuffle(vv, vv, 0b1111_0000);
                        sum1v = Sse.Multiply(sum1v, fA);  //Multiply in one go
                        var ffv = Sse.Shuffle(vv, vv, 0b1111_1001);
                        a = Sse.Add(ffv, sum1v);
                    }
                }
                for (var ch = 0; ch < channels; ch++)
                {
                    var f = iState[ch];
                    internalStates[ch] = new Vector2(f.GetElement(0), f.GetElement(1));
                }
            }
        }
    }
}

#endif
