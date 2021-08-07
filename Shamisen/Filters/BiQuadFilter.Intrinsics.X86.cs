#if NETCOREAPP3_1_OR_GREATER

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Runtime.InteropServices;

namespace Shamisen.Filters
{
    public sealed partial class BiQuadFilter
    {
        private void ProcessMonauralSse(Span<float> buffer)
        {
            unsafe
            {
                var factorB = Parameter.B;
                var factorA = Parameter.A;
                var iState = internalStates[0];
                var fB = Vector128.Create(factorB.X, factorB.Y, factorB.Z, 0);
                var fA = Vector128.Create(factorA.X, factorA.Y, 0, 0);
                var iS = Vector128.Create(iState.X, iState.Y, 0, 0);
                //Factor localization greatly improved performance
                for (int i = 0; i < buffer.Length; i++)
                {
                    //Reference: https://en.wikipedia.org/wiki/Digital_biquad_filter#Transposed_Direct_form_2
                    //Transformed for SIMD awareness.
                    ref float v = ref buffer[i];
                    var vv = Vector128.Create(v);
                    var feedForward = Sse.Multiply(vv, fB); //Multiply in one go
                    var sum = Sse.Add(feedForward, iS);
                    var sum1 = sum.GetElement(0);
                    v = sum1;
                    var sum1v = Sse.Shuffle(sum, sum, 0b1111_0000);
                    var feedBack = Sse.Multiply(sum1v, fA);  //Multiply in one go
                    var ffv = Sse.Shuffle(sum, sum, 0b1111_1001);
                    iS = Sse.Add(ffv, feedBack);
                }
                iState = new Vector2(iS.GetElement(0), iS.GetElement(1));
                internalStates[0] = iState;
            }
        }

        private void ProcessMonauralFma(Span<float> buffer)
        {
            unsafe
            {
                var factorB = Parameter.B;
                var factorA = Parameter.A;
                var iState = internalStates[0];
                var fB = Vector128.Create(factorB.X, factorB.Y, factorB.Z, 0);
                var fA = Vector128.Create(factorA.X, factorA.Y, 0, 0);
                var iS = Vector128.Create(iState.X, iState.Y, 0, 0);
                //Factor localization greatly improved performance
                for (int i = 0; i < buffer.Length; i++)
                {
                    //Reference: https://en.wikipedia.org/wiki/Digital_biquad_filter#Transposed_Direct_form_2
                    //Transformed for SIMD awareness.
                    ref float v = ref buffer[i];
                    var vv = Vector128.Create(v);
                    var sum = Fma.MultiplyAdd(vv, fB, iS);  //Multiply and add in one go
                    var sum1 = sum.GetElement(0);
                    v = sum1;
                    var sum1v = Sse.Shuffle(sum, sum, 0b1111_0000);
                    var feedBack = Sse.Multiply(sum1v, fA);  //Multiply in one go
                    var ffv = Sse.Shuffle(sum, sum, 0b1111_1001);
                    iS = Sse.Add(ffv, feedBack);
                }
                iState = new Vector2(iS.GetElement(0), iS.GetElement(1));
                internalStates[0] = iState;
            }
        }

        private void ProcessStereoAvx(Span<float> buffer)
        {
            unsafe
            {
                //Factor localization greatly improved performance
                var factorB = Parameter.B;
                var factorA = Parameter.A;
                var ist = internalStates;
                var iState = Unsafe.As<Vector2, Vector128<float>>(ref ist[0]);
                var vBuffer128 = MemoryUtils.CastSplit<float, Vector128<float>>(buffer, out var res);
                var vBuffer64 = MemoryUtils.CastSplit<float, Vector2>(res, out _);
                var vzero128 = Vector128<float>.Zero;
                var fB = Vector128.Create(factorB.X, factorB.Y, factorB.Z, 0);
                var fBx = Vector256.Create(fB, fB);
                var fA = Sse.LoadLow(vzero128, (float*)Unsafe.AsPointer(ref factorA));
                var fAx = Vector256.Create(fA, fA);
                var iSL = Sse.Shuffle(iState, vzero128, 0b1111_0100);
                var iSR = Sse.Shuffle(iState, vzero128, 0b1111_1110);
                var iSx = Vector256.Create(iSL, iSR);
                for (int i = 0; i < vBuffer128.Length; i++)
                {
                    //Reference: https://en.wikipedia.org/wiki/Digital_biquad_filter#Transposed_Direct_form_2
                    //Transformed for SIMD awareness.
                    ref var v = ref vBuffer128[i];
                    Vector128<float> sum;
                    {
                        var vvL = Vector128.CreateScalar(v.GetElement(0));
                        var vvR = Vector128.CreateScalar(v.GetElement(1));
                        var vvx = Vector256.Create(vvL, vvR);
                        vvx = Avx.Permute(vvx, 0b1100_0000);
                        vvx = Avx.Multiply(vvx, fBx);   //Decreased performance on Haswell Refresh in benchmarks but usable in post-Rocket-Lake
                        vvx = Avx.Add(vvx, iSx);
                        sum = Vector128.CreateScalarUnsafe(vvx.GetElement(0));
                        sum = sum.WithElement(1, vvx.GetElement(4));
                        var sum1xv = Avx.Permute(vvx, 0b1111_0000);
                        sum1xv = Avx.Multiply(sum1xv, fAx);
                        var ffvx = Avx.Permute(vvx, 0b1111_1001);
                        iSx = Avx.Add(ffvx, sum1xv);
                    }
                    {
                        var vvL = Vector128.CreateScalar(v.GetElement(2));
                        var vvR = Vector128.CreateScalar(v.GetElement(3));
                        var vvx = Vector256.Create(vvL, vvR);
                        vvx = Avx.Permute(vvx, 0b1100_0000);
                        vvx = Avx.Multiply(vvx, fBx);
                        vvx = Avx.Add(vvx, iSx);
                        sum = sum.WithElement(2, vvx.GetElement(0));
                        sum = sum.WithElement(3, vvx.GetElement(4));
                        Sse.Store((float*)Unsafe.AsPointer(ref v), sum);
                        var sum1xv = Avx.Permute(vvx, 0b1111_0000);
                        sum1xv = Avx.Multiply(sum1xv, fAx);
                        var ffvx = Avx.Permute(vvx, 0b1111_1001);
                        iSx = Avx.Add(ffvx, sum1xv);
                    }
                }
                for (int i = 0; i < vBuffer64.Length; i++)
                {
                    //Reference: https://en.wikipedia.org/wiki/Digital_biquad_filter#Transposed_Direct_form_2
                    //Transformed for SIMD awareness.
                    ref var v = ref vBuffer64[i];
                    Vector128<float> sum = Vector128<float>.Zero;
                    sum = Sse.LoadLow(sum, (float*)Unsafe.AsPointer(ref v));
                    var vvL = Vector128.CreateScalar(sum.GetElement(0));
                    var vvR = Vector128.CreateScalar(sum.GetElement(1));
                    var vvx = Vector256.Create(vvL, vvR);
                    vvx = Avx.Permute(vvx, 0b1100_0000);
                    vvx = Avx.Multiply(vvx, fBx);
                    vvx = Avx.Add(vvx, iSx);
                    sum = Vector128.CreateScalarUnsafe(vvx.GetElement(0));
                    sum = sum.WithElement(1, vvx.GetElement(4));
                    Sse.StoreLow((float*)Unsafe.AsPointer(ref v), sum);
                    var sum1xv = Avx.Permute(vvx, 0b1111_0000);
                    sum1xv = Avx.Multiply(sum1xv, fAx);
                    var ffvx = Avx.Permute(vvx, 0b1111_1001);
                    iSx = Avx.Add(ffvx, sum1xv);
                }
                var l = iSx.GetLower();
                var u = iSx.GetUpper();
                iState = Sse.Shuffle(l, u, 0b0100_0100);
                Unsafe.As<Vector2, Vector128<float>>(ref internalStates.AsSpan()[0]) = iState;
            }
        }

        private void ProcessStereoSse(Span<float> buffer)
        {
            unsafe
            {
                //Factor localization greatly improved performance
                var factorB = Parameter.B;
                var factorA = Parameter.A;
                var ist = internalStates;
                var iStateL = ist[0];
                var iStateR = ist[1];
                var fB = Vector128.Create(factorB.X, factorB.Y, factorB.Z, 0);
                var fA = Vector128.Create(factorA.X, factorA.Y, 0, 0);
                var iSL = Vector128.Create(iStateL.X, iStateL.Y, 0, 0);
                var iSR = Vector128.Create(iStateR.X, iStateR.Y, 0, 0);
                var zero = Vector128<float>.Zero;
                for (int i = 0; i < buffer.Length - 1; i += 2)
                {
                    //Reference: https://en.wikipedia.org/wiki/Digital_biquad_filter#Transposed_Direct_form_2
                    //Transformed for SIMD awareness.
                    ref float vL = ref buffer[i];
                    ref float vR = ref buffer[i + 1];
                    var vvL = Vector128.Create(vL);
                    var vvR = Vector128.Create(vR);
                    vvL = Sse.Multiply(vvL, fB);
                    vvR = Sse.Multiply(vvR, fB);
                    vvL = Sse.Add(vvL, iSL);
                    vvR = Sse.Add(vvR, iSR);
                    var sum1L = vvL.GetElement(0);
                    var sum1R = vvR.GetElement(0);
                    vL = sum1L;
                    vR = sum1R;
                    var sum1Lv = Sse.Shuffle(vvL, vvL, 0b1111_0000);
                    var sum1Rv = Sse.Shuffle(vvR, vvR, 0b1111_0000);
                    sum1Lv = Sse.Multiply(sum1Lv, fA);
                    sum1Rv = Sse.Multiply(sum1Rv, fA);
                    var ffvL = Sse.Shuffle(vvL, vvL, 0b1111_1001);
                    var ffvR = Sse.Shuffle(vvR, vvR, 0b1111_1001);
                    iSL = Sse.Add(ffvL, sum1Lv);
                    iSR = Sse.Add(ffvR, sum1Rv);
                }
                iStateL = new Vector2(iSL.GetElement(0), iSL.GetElement(1));
                iStateR = new Vector2(iSR.GetElement(0), iSR.GetElement(1));
                ist[0] = iStateL;
                ist[1] = iStateR;
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
                int channels = iState.Length;
                for (int ch = 0; ch < channels; ch++)
                {
                    var g = internalStates[ch];
                    iState[ch] = Vector128.Create(g.X, g.Y, 0, 0);
                }
                ref var iS = ref MemoryMarshal.GetReference(iState);
                for (int i = 0; i < buffer.Length - channels + 1; i += channels)
                {
                    ref var pos = ref buffer[i];
                    //var span = buffer.Slice(i, internalStates.Length);
                    for (int ch = 0; ch < channels; ch++)
                    {
                        //Reference: https://en.wikipedia.org/wiki/Digital_biquad_filter#Transposed_Direct_form_2
                        //Transformed for SIMD awareness.
                        ref var a = ref Unsafe.Add(ref iS, ch);
                        ref float v = ref Unsafe.Add(ref pos, ch);
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
                for (int ch = 0; ch < channels; ch++)
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
                int channels = iState.Length;
                for (int ch = 0; ch < channels; ch++)
                {
                    var g = internalStates[ch];
                    iState[ch] = Vector128.Create(g.X, g.Y, 0, 0);
                }
                ref var iS = ref MemoryMarshal.GetReference(iState);
                for (int i = 0; i < buffer.Length - channels + 1; i += channels)
                {
                    ref var pos = ref buffer[i];
                    //var span = buffer.Slice(i, internalStates.Length);
                    for (int ch = 0; ch < channels; ch++)
                    {
                        //Reference: https://en.wikipedia.org/wiki/Digital_biquad_filter#Transposed_Direct_form_2
                        //Transformed for SIMD awareness.
                        ref var a = ref Unsafe.Add(ref iS, ch);
                        ref float v = ref Unsafe.Add(ref pos, ch);
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
                for (int ch = 0; ch < channels; ch++)
                {
                    var f = iState[ch];
                    internalStates[ch] = new Vector2(f.GetElement(0), f.GetElement(1));
                }
            }
        }
    }
}

#endif
