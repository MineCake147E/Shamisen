using Shamisen.Extensions;

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using System.Text;
using Shamisen.Optimization;
using System.Runtime.CompilerServices;

#if NET5_0 || NETCOREAPP3_1

using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Shamisen.Conversion.SampleToWaveConverters
{
    public sealed partial class SampleToPcm16Converter
    {
        private static void ProcessNormalSse2(Span<float> wrote, Span<short> dest)
        {
            var wy = MemoryUtils.CastSplit<float, (Vector128<float> xmm0, Vector128<float> xmm1)>(wrote, out var ryWrote);
            var dy = MemoryUtils.CastSplit<short, Vector128<short>>(dest, out var ryDest);
            var wv = MemoryUtils.CastSplit<float, Vector128<float>>(ryWrote, out var rWrote);
            var dv = MemoryUtils.CastSplit<short, Vector64<short>>(ryDest, out var rDest);
            var vmult = Vector128.Create(Multiplier);
            var vmin = Vector128.Create((float)short.MinValue);
            var vmax = Vector128.Create((float)short.MaxValue);
            var vcvtmask = Vector128.Create(0x0000_ffff);
            if (wy.Length > dy.Length) return;
            for (int i = 0; i < wy.Length; i++)
            {
                var g0 = wy[i].xmm0;
                var g1 = wy[i].xmm1;
                g0 = Sse.Multiply(g0, vmult);
                g1 = Sse.Multiply(g1, vmult);
                g0 = Sse.Max(g0, vmin);
                g1 = Sse.Max(g1, vmin);
                g0 = Sse.Min(g0, vmax);
                g1 = Sse.Min(g1, vmax);
                var q0 = Sse2.ConvertToVector128Int32WithTruncation(g0);
                var q1 = Sse2.ConvertToVector128Int32WithTruncation(g1);
                var h = Sse2.PackSignedSaturate(q0, q1);
                dy[i] = h;
            }
            if (wv.Length > dv.Length) return;
            for (int i = 0; i < wv.Length; i++)
            {
                var g0 = wv[i];
                g0 = Sse.Multiply(g0, vmult);
                g0 = Sse.Max(g0, vmin);
                g0 = Sse.Min(g0, vmax);
                var q = Sse2.ConvertToVector128Int32WithTruncation(g0);
                q = Sse2.And(q, vcvtmask);
                var h = Sse2.PackSignedSaturate(q, q);
                dv[i] = h.GetLower();
            }
            if (rWrote.Length > rDest.Length) return;
            for (int i = 0; i < rWrote.Length; i++)
            {
                var v = Convert(rWrote[i]);
                rDest[i] = v;
            }
        }

        private static void ProcessReversedSsse3(Span<float> wrote, Span<short> dest)
        {
            var wy = MemoryUtils.CastSplit<float, (Vector128<float> xmm0, Vector128<float> xmm1)>(wrote, out var ryWrote);
            var dy = MemoryUtils.CastSplit<short, Vector128<short>>(dest, out var ryDest);
            var wv = MemoryUtils.CastSplit<float, Vector128<float>>(ryWrote, out var rWrote);
            var dv = MemoryUtils.CastSplit<short, Vector64<short>>(ryDest, out var rDest);
            var vmult = Vector128.Create(Multiplier);
            var vmin = Vector128.Create((float)short.MinValue);
            var vmax = Vector128.Create((float)short.MaxValue);
            var vcvtmask = Vector128.Create(0x0000_ffff);
            var shuffle = Vector128.Create(1, 0, 3, 2, 5, 4, 7, 6, 9, 8, 11, 10, 13, 12, 15, 14).AsByte();
            if (wy.Length > dy.Length) return;
            for (int i = 0; i < wy.Length; i++)
            {
                var g0 = wy[i].xmm0;
                var g1 = wy[i].xmm1;
                g0 = Sse.Multiply(g0, vmult);
                g1 = Sse.Multiply(g1, vmult);
                g0 = Sse.Max(g0, vmin);
                g1 = Sse.Max(g1, vmin);
                g0 = Sse.Min(g0, vmax);
                g1 = Sse.Min(g1, vmax);
                var q0 = Sse2.ConvertToVector128Int32WithTruncation(g0);
                var q1 = Sse2.ConvertToVector128Int32WithTruncation(g1);
                var h = Sse2.PackSignedSaturate(q0, q1);
                h = Ssse3.Shuffle(h.AsByte(), shuffle).AsInt16();
                dy[i] = h;
            }
            if (wv.Length > dv.Length) return;
            for (int i = 0; i < wv.Length; i++)
            {
                var g0 = wv[i];
                g0 = Sse.Multiply(g0, vmult);
                g0 = Sse.Max(g0, vmin);
                g0 = Sse.Min(g0, vmax);
                var q = Sse2.ConvertToVector128Int32WithTruncation(g0);
                q = Sse2.And(q, vcvtmask);
                var h = Sse2.PackSignedSaturate(q, q);
                h = Ssse3.Shuffle(h.AsByte(), shuffle).AsInt16();
                dv[i] = h.GetLower();
            }
            if (rWrote.Length > rDest.Length) return;
            for (int i = 0; i < rWrote.Length; i++)
            {
                var v = Convert(rWrote[i]);
                rDest[i] = BinaryPrimitives.ReverseEndianness(v);
            }
        }
    }
}

#endif
