#if NET5_0 || NETCOREAPP3_1

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

namespace MonoAudio.Filters
{
    public sealed partial class BiQuadFilter
    {
        private void ProcessMonauralSse(Span<float> buffer)
        {
            unsafe
            {
                Vector3 factorB = Parameter.B;
                Vector2 factorA = Parameter.A;
                var iState = internalStates[0];
                Vector128<float> fB = Vector128.Create(factorB.X, factorB.Y, factorB.Z, 0);
                Vector128<float> fA = Vector128.Create(factorA.X, factorA.Y, 0, 0);
                Vector128<float> iS = Vector128.Create(iState.X, iState.Y, 0, 0);
                //Factor localization greatly improved performance
                for (int i = 0; i < buffer.Length; i++)
                {
                    //Reference: https://en.wikipedia.org/wiki/Digital_biquad_filter#Transposed_Direct_form_2
                    //Transformed for SIMD awareness.
                    ref float v = ref buffer[i];
                    var vv = Vector128.CreateScalar(v);
                    vv = Sse.Shuffle(vv, vv, 0b1100_0000);
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

        private void ProcessMonauralAvx(Span<float> buffer)
        {
            unsafe
            {
                Vector3 factorB = Parameter.B;
                Vector2 factorA = Parameter.A;
                var iState = internalStates[0];
                Vector128<float> fB = Vector128.Create(factorB.X, factorB.Y, factorB.Z, 0);
                Vector128<float> fA = Vector128.Create(factorA.X, factorA.Y, 0, 0);
                Vector128<float> iS = Vector128.Create(iState.X, iState.Y, 0, 0);
                //Factor localization greatly improved performance
                for (int i = 0; i < buffer.Length; i++)
                {
                    //Reference: https://en.wikipedia.org/wiki/Digital_biquad_filter#Transposed_Direct_form_2
                    //Transformed for SIMD awareness.
                    //TODO: use AVX
                    ref float v = ref buffer[i];
                    var vv = Vector128.CreateScalar(v);
                    vv = Sse.Shuffle(vv, vv, 0b1100_0000);
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
                Vector3 factorB = Parameter.B;
                Vector2 factorA = Parameter.A;
                Span<Vector128<float>> iState = stackalloc Vector128<float>[internalStates.Length];
                Vector128<float> fB = Vector128.Create(factorB.X, factorB.Y, factorB.Z, 0);
                Vector128<float> fA = Vector128.Create(factorA.X, factorA.Y, 0, 0);
                //Vector128<float> iS = Vector128.Create(iState.X, iState.Y, 0, 0);
                for (int ch = 0; ch < iState.Length; ch++)
                {
                    var g = internalStates[ch];
                    iState[ch] = Vector128.Create(g.X, g.Y, 0, 0);
                }
                ref var iS = ref MemoryMarshal.GetReference(iState);
                for (int i = 0; i < buffer.Length; i += iState.Length)
                {
                    ref var pos = ref buffer[i];
                    //var span = buffer.Slice(i, internalStates.Length);
                    for (int ch = 0; ch < iState.Length; ch++)
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
                for (int ch = 0; ch < iState.Length; ch++)
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
                Vector3 factorB = Parameter.B;
                Vector2 factorA = Parameter.A;
                Span<Vector128<float>> iState = stackalloc Vector128<float>[internalStates.Length];
                Vector128<float> fB = Vector128.Create(factorB.X, factorB.Y, factorB.Z, 0);
                Vector128<float> fA = Vector128.Create(factorA.X, factorA.Y, 0, 0);
                //Vector128<float> iS = Vector128.Create(iState.X, iState.Y, 0, 0);
                for (int ch = 0; ch < iState.Length; ch++)
                {
                    var g = internalStates[ch];
                    iState[ch] = Vector128.Create(g.X, g.Y, 0, 0);
                }
                ref var iS = ref MemoryMarshal.GetReference(iState);
                for (int i = 0; i < buffer.Length; i += iState.Length)
                {
                    ref var pos = ref buffer[i];
                    //var span = buffer.Slice(i, internalStates.Length);
                    for (int ch = 0; ch < iState.Length; ch++)
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
                for (int ch = 0; ch < iState.Length; ch++)
                {
                    var f = iState[ch];
                    internalStates[ch] = new Vector2(f.GetElement(0), f.GetElement(1));
                }
            }
        }
    }
}

#endif
