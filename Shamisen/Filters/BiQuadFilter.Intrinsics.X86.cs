#if NETCOREAPP3_1_OR_GREATER

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

using Shamisen.Utils.Intrinsics;

namespace Shamisen.Filters
{
    public sealed partial class BiQuadFilter
    {
        internal static void ProcessMonauralSse(Span<float> buffer, BiQuadParameter parameter, Span<Vector2> states)
        {
            unsafe
            {
                var factorB = parameter.B;
                var factorA = parameter.A;
                var iState = states[0];
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
                states[0] = iState;
            }
        }

        //Modernization(use of nint instead of int)
        internal static void ProcessMonauralSseM1(Span<float> buffer, BiQuadParameter parameter, Span<Vector2> states)
        {
            unsafe
            {
                var factorB = parameter.B;
                var factorA = parameter.A;
                var iState = states[0];
                ref var rdi = ref MemoryMarshal.GetReference(buffer);
                var fB = Vector128.Create(factorB.X, factorB.Y, factorB.Z, 0);
                var fA = Vector128.Create(factorA.X, factorA.Y, 0, 0);
                var iS = Vector128.Create(iState.X, iState.Y, 0, 0);
                nint i, length = buffer.Length;

                //Factor localization greatly improved performance
                for (i = 0; i < length; i++)
                {
                    //Reference: https://en.wikipedia.org/wiki/Digital_biquad_filter#Transposed_Direct_form_2
                    //Transformed for SIMD awareness.
                    float v = Unsafe.Add(ref rdi, i);
                    var vv = Vector128.Create(v);
                    var feedForward = Sse.Multiply(vv, fB); //Multiply in one go
                    var sum = Sse.Add(feedForward, iS);
                    var sum1 = sum.GetElement(0);
                    var sum1v = Sse.Shuffle(sum, sum, 0b1111_0000);
                    var ffv = Sse.Shuffle(sum, sum, 0b1111_1001);
                    Unsafe.Add(ref rdi, i) = sum1;
                    var feedBack = Sse.Multiply(sum1v, fA);  //Multiply in one go
                    iS = Sse.Add(ffv, feedBack);
                }
                iState = new Vector2(iS.GetElement(0), iS.GetElement(1));
                states[0] = iState;
            }
        }
        internal static void ProcessMonauralSseM2(Span<float> buffer, BiQuadParameter parameter, Span<Vector2> states)
        {
            unsafe
            {
                var factorB = parameter.B;
                var factorA = parameter.A;
                var iState = states[0];
                ref var rdi = ref MemoryMarshal.GetReference(buffer);
                var xmm3 = Vector128.CreateScalarUnsafe(factorB.X);
                var xmm8 = Vector128.CreateScalarUnsafe(factorB.Y);
                var xmm2 = Vector128.CreateScalarUnsafe(factorB.Z);
                var xmm1 = Vector128.CreateScalarUnsafe(factorA.X);
                var xmm4 = Vector128.CreateScalarUnsafe(factorA.Y);
                var xmm6 = Vector128.CreateScalarUnsafe(iState.X);
                var xmm5 = Vector128.CreateScalarUnsafe(iState.Y);
                //LLVM had me to do everything in scalar
                nint i, length = buffer.Length;
                for (i = 0; i < length - 1; i += 2)
                {
                    var xmm7 = Vector128.CreateScalarUnsafe(Unsafe.Add(ref rdi, i));
                    var xmm0 = Sse.MultiplyScalar(xmm3, xmm7);
                    xmm0 = Sse.AddScalar(xmm0, xmm6);
                    xmm6 = Sse.MultiplyScalar(xmm8, xmm7);
                    xmm5 = Sse.AddScalar(xmm6, xmm5);
                    xmm6 = Sse.MultiplyScalar(xmm2, xmm7);
                    Unsafe.Add(ref rdi, i) = xmm0.GetElement(0);
                    xmm7 = Sse.MultiplyScalar(xmm1, xmm0);
                    xmm5 = Sse.AddScalar(xmm5, xmm7);
                    xmm0 = Sse.MultiplyScalar(xmm4, xmm0);
                    xmm0 = Sse.AddScalar(xmm6, xmm0);
                    xmm6 = Vector128.CreateScalarUnsafe(Unsafe.Add(ref rdi, i + 1));
                    xmm7 = Sse.MultiplyScalar(xmm3, xmm6);
                    xmm5 = Sse.AddScalar(xmm7, xmm5);
                    xmm7 = Sse.MultiplyScalar(xmm8, xmm6);
                    xmm0 = Sse.AddScalar(xmm7, xmm0);
                    xmm7 = Sse.MultiplyScalar(xmm2, xmm6);
                    Unsafe.Add(ref rdi, i + 1) = xmm5.GetElement(0);
                    xmm6 = Sse.MultiplyScalar(xmm1, xmm5);
                    xmm6 = Sse.AddScalar(xmm0, xmm6);
                    xmm0 = Sse.MultiplyScalar(xmm4, xmm5);
                    xmm5 = Sse.AddScalar(xmm7, xmm0);
                }
                if (i < length)
                {
                    var xmm0 = Vector128.CreateScalarUnsafe(Unsafe.Add(ref rdi, i));
                    xmm3 = Sse.MultiplyScalar(xmm3, xmm0);
                    xmm3 = Sse.AddScalar(xmm3, xmm6);
                    Unsafe.Add(ref rdi, i) = xmm3.GetElement(0);
                    xmm2 = Sse.MultiplyScalar(xmm2, xmm0);
                    xmm4 = Sse.MultiplyScalar(xmm4, xmm3);
                    xmm0 = Sse.MultiplyScalar(xmm8, xmm0);
                    xmm0 = Sse.AddScalar(xmm0, xmm5);
                    xmm5 = Sse.AddScalar(xmm2, xmm4);
                    xmm1 = Sse.MultiplyScalar(xmm1, xmm3);
                    xmm6 = Sse.AddScalar(xmm0, xmm1);
                }
                iState = new Vector2(xmm6.GetElement(0), xmm5.GetElement(0));
                states[0] = iState;
            }
        }

        //WIP
#if true

        [Obsolete("Work in progress!")]
        private static void ProcessMonauralAvx2(Span<float> buffer, BiQuadParameter parameter, Span<Vector2> states)
        {
            unsafe
            {
                var factorB = parameter.B;
                var factorA = parameter.A;
                var iState = states[0];
                var fB02 = Vector256.Create(Vector128.Create(factorB.X), Vector128.Create(factorB.Z));
                var fB1 = Vector128.Create(factorB.Y);
                var fa1 = factorA.X;
                var fa2 = factorA.Y;
                var fa1p2 = fa1 * fa1;
                var fAp = Vector128.Create(0f, fa1, fa2 + fa1p2, 2.0f * (fa1 * fa2) + fa1p2 * fa1);
                var fA = Vector128.Create(fa1, fa2, 0f, 0f);
                ref var rsi = ref MemoryMarshal.GetReference(buffer);

                nint i = 0, length = buffer.Length;
                var xmm11 = Vector128.Create(0f, 0f, 0f, iState.Y);
                var xmm10 = Vector128.Create(0f, 0f, 0f, iState.X);
                for (; i < length - 7; i += 8)
                {
                    var xmm0 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rsi, i));
                    var xmm1 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rsi, i + 4));
                    var xmm2 = Sse.Multiply(xmm0, fB02.GetLower());
                    var xmm3 = Sse.Multiply(xmm1, fB02.GetLower());
                    var xmm9 = Avx.ExtractVector128(fB02, 1);
                    var xmm4 = Sse.Multiply(xmm0, fB1);
                    var xmm5 = Sse.Multiply(xmm1, fB1);
                    var xmm6 = Sse.Multiply(xmm0, xmm9);
                    var xmm7 = Sse.Multiply(xmm1, xmm9);
                    xmm0 = Ssse3Utils.AlignRight(xmm4, xmm10, 12);
                    xmm1 = Ssse3Utils.AlignRight(xmm6, xmm11, 8);
                    xmm0 = Sse.Add(xmm2, xmm0);
                    xmm4 = Ssse3Utils.AlignRight(xmm5, xmm4, 12);
                    xmm1 = Sse.Add(xmm3, xmm1);
                    xmm6 = Ssse3Utils.AlignRight(xmm7, xmm6, 8);
                    xmm0 = Sse.Add(xmm0, xmm4);
                    xmm5 = Ssse3Utils.AlignRight(xmm10, xmm5, 12);
                    xmm1 = Sse.Add(xmm1, xmm6);
                    xmm7 = Ssse3Utils.AlignRight(xmm11, xmm7, 8);
                    xmm2 = Sse2Utils.ShiftLeftLogical128BitLane(fAp, 8);
                    xmm5 = Sse.Add(xmm5, xmm7);

                    //feed-forward is done
                    //feedback for 1st block
                    xmm3 = Sse2Utils.ShiftLeftLogical128BitLane(xmm0, 4);
                    xmm4 = Sse2Utils.ShiftLeftLogical128BitLane(fAp, 4);
                    xmm2 = Sse.Multiply(xmm3, xmm2);
                    xmm0 = Sse.Add(xmm2, xmm0);
                    xmm6 = Sse.Shuffle(xmm0, xmm0, 0b01_01_01_01);
                    xmm3 = Sse.Multiply(xmm6, xmm4);
                    xmm2 = Sse2Utils.ShiftLeftLogical128BitLane(fAp, 8);
                    xmm0 = Sse.Add(xmm0, xmm3);
                    xmm7 = Sse.Shuffle(xmm0, xmm0, 0);
                    xmm7 = Sse.Multiply(xmm7, fAp);
                    xmm4 = Sse2Utils.ShiftLeftLogical128BitLane(fAp, 4);
                    xmm0 = Sse.Add(xmm0, xmm7);
                    //feedback for 2nd block
                    xmm3 = Ssse3Utils.AlignRight(xmm1, xmm0, 8);

                }
                //iState = new Vector2(iS.GetElement(0), iS.GetElement(1));
                states[0] = iState;
            }
        }

#endif

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

        private void ProcessStereoSse2(Span<float> buffer)
        {
            unsafe
            {
                //Factor localization greatly improved performance
                var parameter = Parameter;
                var factorB = parameter.B;
                var factorA = parameter.A;
                var ist = internalStates;
                var iState = Unsafe.As<Vector2, Vector128<float>>(ref MemoryMarshal.GetReference(internalStates.AsSpan()));
                var fB = Vector128.Create(factorB.X, factorB.Y, factorB.Z, 0);
                var fA = Vector128.Create(Unsafe.As<Vector2, double>(ref factorA)).AsSingle();
                var iSx = iState;
                ref var rB = ref MemoryMarshal.GetReference(buffer);
                nint i, length = buffer.Length;
                Vector128<float> sum;
                for (i = 0; i < length - 3; i += 4)
                {
                    //Reference: https://en.wikipedia.org/wiki/Digital_biquad_filter#Transposed_Direct_form_2
                    //Transformed for SIMD awareness.
                    var xmm0 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rB, i));
                    var vvL = Sse.Shuffle(xmm0, xmm0, 0b00_00_00_00);
                    var vvR = Sse.Shuffle(xmm0, xmm0, 0b01_01_01_01);
                    var iSL = Sse.Shuffle(iSx, fB, 0b11_11_01_00);
                    var iSR = Sse.Shuffle(iSx, fB, 0b11_11_11_10);
                    vvL = Sse.Multiply(vvL, fB);
                    vvR = Sse.Multiply(vvR, fB);
                    vvL = Sse.Add(vvL, iSL);
                    vvR = Sse.Add(vvR, iSR);
                    var sum1xv = Sse.Shuffle(vvL, vvR, 0b00_00_00_00);
                    var ffvx = Sse.Shuffle(vvL, vvR, 0b10_01_10_01);
                    sum = Sse.UnpackLow(vvL, vvR);
                    sum1xv = Sse.Multiply(sum1xv, fA);
                    iSx = Sse.Add(ffvx, sum1xv);

                    vvL = Sse.Shuffle(xmm0, xmm0, 0b00_00_00_00);
                    vvR = Sse.Shuffle(xmm0, xmm0, 0b01_01_01_01);
                    iSL = Sse.Shuffle(iSx, fB, 0b11_11_01_00);
                    iSR = Sse.Shuffle(iSx, fB, 0b11_11_11_10);
                    vvL = Sse.Multiply(vvL, fB);
                    vvR = Sse.Multiply(vvR, fB);
                    vvL = Sse.Add(vvL, iSL);
                    vvR = Sse.Add(vvR, iSR);
                    var xmm9 = Sse.UnpackLow(vvL, vvR);
                    sum = Sse2.UnpackLow(sum.AsDouble(), xmm9.AsDouble()).AsSingle();
                    Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rB, i)) = sum;
                    sum1xv = Sse.Shuffle(vvL, vvR, 0b00_00_00_00);
                    ffvx = Sse.Shuffle(vvL, vvR, 0b10_01_10_01);
                    sum1xv = Sse.Multiply(sum1xv, fA);
                    iSx = Sse.Add(ffvx, sum1xv);
                }
                for (; i < length - 2; i += 2)
                {
                    var xmm0 = Unsafe.As<float, Vector128<float>>(ref Unsafe.Add(ref rB, i));
                    var vvL = Sse.Shuffle(xmm0, xmm0, 0b00_00_00_00);
                    var vvR = Sse.Shuffle(xmm0, xmm0, 0b01_01_01_01);
                    var iSL = Sse.Shuffle(iSx, fB, 0b11_11_01_00);
                    var iSR = Sse.Shuffle(iSx, fB, 0b11_11_11_10);
                    vvL = Sse.Multiply(vvL, fB);
                    vvR = Sse.Multiply(vvR, fB);
                    vvL = Sse.Add(vvL, iSL);
                    vvR = Sse.Add(vvR, iSR);
                    sum = Sse.UnpackLow(vvL, vvR);
                    Unsafe.As<float, double>(ref Unsafe.Add(ref rB, i)) = sum.AsDouble().GetElement(0);
                    var sum1xv = Sse.Shuffle(vvL, vvR, 0b00_00_00_00);
                    sum1xv = Sse.Multiply(sum1xv, fA);
                    var ffvx = Sse.Shuffle(vvL, vvR, 0b10_01_10_01);
                    iSx = Sse.Add(ffvx, sum1xv);
                }
                iState = iSx;
                Unsafe.As<Vector2, Vector128<float>>(ref MemoryMarshal.GetReference(internalStates.AsSpan())) = iState;
            }
        }

        private void ProcessStereoSse(Span<float> buffer)
        {
            unsafe
            {
                //Factor localization greatly improved performance
                var parameter = Parameter;
                var factorB = parameter.B;
                var factorA = parameter.A;
                var ist = internalStates;
                var iStateL = ist[0];
                var iStateR = ist[1];
                var fB = Vector128.Create(factorB.X, factorB.Y, factorB.Z, 0);
                var fA = Vector128.Create(factorA.X, factorA.Y, 0, 0);
                var iSL = Vector128.Create(iStateL.X, iStateL.Y, 0, 0);
                var iSR = Vector128.Create(iStateR.X, iStateR.Y, 0, 0);
                ref var rB = ref MemoryMarshal.GetReference(buffer);
                nint i, length = buffer.Length;
                for (i = 0; i < length - 1; i += 2)
                {
                    //Reference: https://en.wikipedia.org/wiki/Digital_biquad_filter#Transposed_Direct_form_2
                    //Transformed for SIMD awareness.
                    var xmm0 = Vector128.CreateScalarUnsafe(Unsafe.As<float, double>(ref Unsafe.Add(ref rB, i))).AsSingle();
                    var vvL = Sse.Shuffle(xmm0, xmm0, 0b00_00_00_00);
                    var vvR = Sse.Shuffle(xmm0, xmm0, 0b01_01_01_01);
                    vvL = Sse.Multiply(vvL, fB);
                    vvR = Sse.Multiply(vvR, fB);
                    vvL = Sse.Add(vvL, iSL);
                    vvR = Sse.Add(vvR, iSR);
                    Unsafe.Add(ref rB, i) = vvL.GetElement(0);
                    Unsafe.Add(ref rB, i + 1) = vvR.GetElement(0);
                    var sum1Lv = Sse.Shuffle(vvL, vvL, 0b1111_0000);
                    var sum1Rv = Sse.Shuffle(vvR, vvR, 0b1111_0000);
                    var ffvL = Sse.Shuffle(vvL, vvL, 0b1111_1001);
                    var ffvR = Sse.Shuffle(vvR, vvR, 0b1111_1001);
                    sum1Lv = Sse.Multiply(sum1Lv, fA);
                    sum1Rv = Sse.Multiply(sum1Rv, fA);
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
