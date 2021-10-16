
using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

#if NETCOREAPP3_1_OR_GREATER

using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Shamisen.Conversion.SampleToWaveConverters
{
    public sealed partial class SampleToPcm16Converter
    {
        #region Normal
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
                short v = Convert(rWrote[i]);
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
                short v = Convert(rWrote[i]);
                rDest[i] = BinaryPrimitives.ReverseEndianness(v);
            }
        }

        #endregion

        #region Accurate
        private static void ProcessAccurateStereoSse41(Span<float> wrote, Span<short> dest, Span<float> dsmAccumulator, Span<short> dsmLastOutput)
        {
            var max = Vector128.Create(32767.0f);
            var min = Vector128.Create(-32768.0f);
            var mul = Vector128.Create(32768.0f);
            var acc = Vector128.CreateScalar(Unsafe.As<float, double>(ref MemoryMarshal.GetReference(dsmAccumulator))).AsSingle();
            var lov = Vector128.CreateScalar(Unsafe.As<short, float>(ref MemoryMarshal.GetReference(dsmLastOutput))).AsInt16();
            ref double wh = ref Unsafe.As<float, double>(ref MemoryMarshal.GetReference(wrote));
            ref short dh = ref MemoryMarshal.GetReference(dest);
            nint length = wrote.Length / 2;
            nint i;
            for (i = 0; i < length - 1; i += 2)
            {
                var lo = Sse2.ConvertToVector128Single(Sse41.ConvertToVector128Int32(lov));
                var xmm5 = Vector128.CreateScalar(Unsafe.Add(ref wh, i)).AsSingle();
                xmm5 = Sse.Multiply(xmm5, mul);
                lo = Sse.Subtract(xmm5, lo);
                acc = Sse.Add(acc, lo);
                acc = Sse.Max(acc, min);
                acc = Sse.Min(acc, max);
                var xmm1 = Sse2.ConvertToVector128Int32WithTruncation(acc);
                Unsafe.Add(ref dh, 2 * i) = xmm1.AsInt16().GetElement(0);
                var xmm5epi16 = Sse2.PackSignedSaturate(xmm1, xmm1);
                Unsafe.Add(ref dh, 2 * i + 1) = xmm1.AsInt16().GetElement(2);
                lo = Sse2.ConvertToVector128Single(Sse41.ConvertToVector128Int32(xmm5epi16));
                xmm5 = Vector128.CreateScalar(Unsafe.Add(ref wh, i + 1)).AsSingle();
                xmm5 = Sse.Multiply(xmm5, mul);
                lo = Sse.Subtract(xmm5, lo);
                acc = Sse.Add(acc, lo);
                acc = Sse.Max(acc, min);
                acc = Sse.Min(acc, max);
                xmm1 = Sse2.ConvertToVector128Int32WithTruncation(acc);
                Unsafe.Add(ref dh, 2 * i + 2) = xmm1.AsInt16().GetElement(0);
                Unsafe.Add(ref dh, 2 * i + 3) = xmm1.AsInt16().GetElement(2);
                lov = Sse2.PackSignedSaturate(xmm1, xmm1);
            }
            if (i < length)
            {
                var lo = Sse2.ConvertToVector128Single(Sse41.ConvertToVector128Int32(lov));
                var xmm5 = Vector128.CreateScalar(Unsafe.Add(ref wh, i)).AsSingle();
                xmm5 = Sse.Multiply(xmm5, mul);
                lo = Sse.Subtract(xmm5, lo);
                acc = Sse.Add(acc, lo);
                acc = Sse.Max(acc, min);
                acc = Sse.Min(acc, max);
                var xmm1 = Sse2.ConvertToVector128Int32WithTruncation(acc);
                Unsafe.Add(ref dh, 2 * i) = xmm1.AsInt16().GetElement(0);
                Unsafe.Add(ref dh, 2 * i + 1) = xmm1.AsInt16().GetElement(2);
                lov = Sse2.PackSignedSaturate(xmm1, xmm1);
            }
            Unsafe.As<float, double>(ref MemoryMarshal.GetReference(dsmAccumulator)) = acc.AsDouble().GetElement(0);
            Unsafe.As<short, float>(ref MemoryMarshal.GetReference(dsmLastOutput)) = lov.AsSingle().GetElement(0);
        }

        private static void ProcessAccurateMonauralSse41(Span<float> wrote, Span<short> dest, Span<float> dsmAccumulator, Span<short> dsmLastOutput)
        {
            var max = Vector128.CreateScalar(32767.0f);
            var min = Vector128.CreateScalar(-32768.0f);
            var mul = Vector128.CreateScalar(32768.0f);
            var acc = Vector128.CreateScalar(MemoryMarshal.GetReference(dsmAccumulator));
            var lov = Vector128.CreateScalar(MemoryMarshal.GetReference(dsmLastOutput));
            var lo = Sse2.ConvertToVector128Single(Sse41.ConvertToVector128Int32(lov));
            ref float wh = ref MemoryMarshal.GetReference(wrote);
            ref short dh = ref MemoryMarshal.GetReference(dest);
            nint length = wrote.Length;
            nint i;
            for (i = 0; i < length - 1; i += 2)
            {
                var xmm5 = Vector128.CreateScalar(Unsafe.Add(ref wh, i));
                xmm5 = Sse.MultiplyScalar(xmm5, mul);
                lo = Sse.SubtractScalar(xmm5, lo);
                acc = Sse.AddScalar(acc, lo);
                acc = Sse.MaxScalar(acc, min);
                acc = Sse.MinScalar(acc, max);
                int eax = Sse.ConvertToInt32WithTruncation(acc);
                Unsafe.Add(ref dh, 2 * i) = (short)eax;
                lo = Sse41.RoundToZeroScalar(acc);
                xmm5 = Vector128.CreateScalar(Unsafe.Add(ref wh, i + 1));
                xmm5 = Sse.MultiplyScalar(xmm5, mul);
                lo = Sse.SubtractScalar(xmm5, lo);
                acc = Sse.AddScalar(acc, lo);
                acc = Sse.MaxScalar(acc, min);
                acc = Sse.MinScalar(acc, max);
                eax = Sse.ConvertToInt32WithTruncation(acc);
                Unsafe.Add(ref dh, 2 * i + 1) = (short)eax;
                lo = Sse41.RoundToZeroScalar(acc);
            }
            if (i < length)
            {
                var xmm5 = Vector128.CreateScalar(Unsafe.Add(ref wh, i));
                xmm5 = Sse.MultiplyScalar(xmm5, mul);
                lo = Sse.SubtractScalar(xmm5, lo);
                acc = Sse.AddScalar(acc, lo);
                acc = Sse.MaxScalar(acc, min);
                acc = Sse.MinScalar(acc, max);
                int eax = Sse.ConvertToInt32WithTruncation(acc);
                Unsafe.Add(ref dh, 2 * i) = (short)eax;
                lo = Sse41.RoundToZeroScalar(acc);
            }
            MemoryMarshal.GetReference(dsmAccumulator) = acc.GetElement(0);
            MemoryMarshal.GetReference(dsmLastOutput) = (short)lo.GetElement(0);
        }
        #endregion
        #region AccurateFused
        private static void ProcessAccurateFusedStereoFma(Span<float> wrote, Span<short> dest, Span<float> dsmAccumulator, Span<short> dsmLastOutput)
        {
            var max = Vector128.Create(32767.0f);
            var min = Vector128.Create(-32768.0f);
            var mul = Vector128.Create(32768.0f);
            var acc = Vector128.CreateScalar(Unsafe.As<float, double>(ref MemoryMarshal.GetReference(dsmAccumulator))).AsSingle();
            var lov = Vector128.CreateScalar(Unsafe.As<short, float>(ref MemoryMarshal.GetReference(dsmLastOutput))).AsInt16();
            ref double wh = ref Unsafe.As<float, double>(ref MemoryMarshal.GetReference(wrote));
            ref short dh = ref MemoryMarshal.GetReference(dest);
            nint length = wrote.Length / 2;
            nint i;
            for (i = 0; i < length - 1; i += 2)
            {
                var lo = Sse2.ConvertToVector128Single(Sse41.ConvertToVector128Int32(lov));
                var xmm5 = Vector128.CreateScalarUnsafe(Unsafe.Add(ref wh, i)).AsSingle();
                xmm5 = Fma.MultiplySubtract(xmm5, mul, lo);   //C# preferes to use vfmsub213ps+vmovaps rather than vfmsub231ps
                acc = Sse.Add(acc, xmm5);
                acc = Sse.Max(acc, min);
                acc = Sse.Min(acc, max);
                var xmm1 = Sse2.ConvertToVector128Int32WithTruncation(acc);
                Unsafe.Add(ref dh, 2 * i) = xmm1.AsInt16().GetElement(0);
                var xmm5epi16 = Sse2.PackSignedSaturate(xmm1, xmm1);
                Unsafe.Add(ref dh, 2 * i + 1) = xmm1.AsInt16().GetElement(2);
                lo = Sse2.ConvertToVector128Single(Sse41.ConvertToVector128Int32(xmm5epi16));
                xmm5 = Vector128.CreateScalarUnsafe(Unsafe.Add(ref wh, i + 1)).AsSingle();
                xmm5 = Fma.MultiplySubtract(xmm5, mul, lo);
                acc = Sse.Add(acc, xmm5);
                acc = Sse.Max(acc, min);
                acc = Sse.Min(acc, max);
                xmm1 = Sse2.ConvertToVector128Int32WithTruncation(acc);
                Unsafe.Add(ref dh, 2 * i + 2) = xmm1.AsInt16().GetElement(0);
                Unsafe.Add(ref dh, 2 * i + 3) = xmm1.AsInt16().GetElement(2);
                lov = Sse2.PackSignedSaturate(xmm1, xmm1);
            }
            if (i < length)
            {
                var lo = Sse2.ConvertToVector128Single(Sse41.ConvertToVector128Int32(lov));
                var xmm5 = Vector128.CreateScalarUnsafe(Unsafe.Add(ref wh, i)).AsSingle();
                xmm5 = Sse.Multiply(xmm5, mul);
                lo = Fma.MultiplySubtract(xmm5, mul, lo);
                acc = Sse.Add(acc, lo);
                acc = Sse.Max(acc, min);
                acc = Sse.Min(acc, max);
                var xmm1 = Sse2.ConvertToVector128Int32WithTruncation(acc);
                Unsafe.Add(ref dh, 2 * i) = xmm1.AsInt16().GetElement(0);
                Unsafe.Add(ref dh, 2 * i + 1) = xmm1.AsInt16().GetElement(2);
                lov = Sse2.PackSignedSaturate(xmm1, xmm1);
            }
            Unsafe.As<float, double>(ref MemoryMarshal.GetReference(dsmAccumulator)) = acc.AsDouble().GetElement(0);
            Unsafe.As<short, float>(ref MemoryMarshal.GetReference(dsmLastOutput)) = lov.AsSingle().GetElement(0);
        }
        private static void ProcessAccurateFusedMonauralFma(Span<float> wrote, Span<short> dest, Span<float> dsmAccumulator, Span<short> dsmLastOutput)
        {
            var max = Vector128.CreateScalar(32767.0f);
            var min = Vector128.CreateScalar(-32768.0f);
            var mul = Vector128.CreateScalar(32768.0f);
            var acc = Vector128.CreateScalar(MemoryMarshal.GetReference(dsmAccumulator));
            var lov = Vector128.CreateScalar(MemoryMarshal.GetReference(dsmLastOutput));
            var lo = Sse2.ConvertToVector128Single(Sse41.ConvertToVector128Int32(lov));
            ref float wh = ref MemoryMarshal.GetReference(wrote);
            ref short dh = ref MemoryMarshal.GetReference(dest);
            nint length = wrote.Length;
            nint i;
            for (i = 0; i < length - 1; i += 2)
            {
                var xmm5 = Vector128.CreateScalar(Unsafe.Add(ref wh, i));
                xmm5 = Fma.MultiplySubtractScalar(mul, xmm5, lo);   //C# preferes to use vfmsub213ss rather than vfmsub231ss
                acc = Sse.AddScalar(acc, xmm5);
                acc = Sse.MaxScalar(acc, min);
                acc = Sse.MinScalar(acc, max);
                int eax = Sse.ConvertToInt32WithTruncation(acc);
                Unsafe.Add(ref dh, 2 * i) = (short)eax;
                lo = Sse41.RoundToZeroScalar(acc);
                xmm5 = Vector128.CreateScalar(Unsafe.Add(ref wh, i + 1));
                xmm5 = Fma.MultiplySubtractScalar(mul, xmm5, lo);
                acc = Sse.AddScalar(acc, xmm5);
                acc = Sse.MaxScalar(acc, min);
                acc = Sse.MinScalar(acc, max);
                eax = Sse.ConvertToInt32WithTruncation(acc);
                Unsafe.Add(ref dh, 2 * i + 1) = (short)eax;
                lo = Sse41.RoundToZeroScalar(acc);
            }
            if (i < length)
            {
                var xmm5 = Vector128.CreateScalar(Unsafe.Add(ref wh, i));
                xmm5 = Fma.MultiplySubtractScalar(mul, xmm5, lo);
                acc = Sse.AddScalar(acc, xmm5);
                acc = Sse.MaxScalar(acc, min);
                acc = Sse.MinScalar(acc, max);
                int eax = Sse.ConvertToInt32WithTruncation(acc);
                Unsafe.Add(ref dh, 2 * i) = (short)eax;
                lo = Sse41.RoundToZeroScalar(acc);
            }
            MemoryMarshal.GetReference(dsmAccumulator) = acc.GetElement(0);
            MemoryMarshal.GetReference(dsmLastOutput) = (short)lo.GetElement(0);
        }

        #endregion
    }
}

#endif
