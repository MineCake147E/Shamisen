using System;
using System.Collections.Generic;
using System.Diagnostics;
//using CSCodec.Filters.Transformation;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

using NUnit.Framework;

using Shamisen.Conversion.Resampling.Sample;
using Shamisen.Conversion.SampleToWaveConverters;
using Shamisen.Filters;
using Shamisen.Optimization;
using Shamisen.Synthesis;

namespace Shamisen.Core.Tests.CoreFx.Conversion
{
    [TestFixture]
    public class SampleToPcm16ConverterTest
    {
        private static void CheckIntrinsicsConsistency(int sampleRate, int channels, X86Intrinsics x86Intrinsics, ArmIntrinsics armIntrinsics, bool accuracyNeeded = true, Endianness endianness = Endianness.Little)
        {
            Console.WriteLine($"Enabled x86-64 intrinsics: {x86Intrinsics}");
            Console.WriteLine($"Enabled ARM intrinsics: {armIntrinsics}");
            const int Frequency = 2000;
            var format = new SampleFormat(channels, sampleRate);
            using var srcNoIntrinsics = new FilterTestSignalSource(format) { Frequency = Frequency };
            using var srcIntrinsics = new FilterTestSignalSource(format) { Frequency = Frequency };
            using var filterNoIntrinsics = new SampleToPcm16Converter(srcNoIntrinsics, false, x86Intrinsics, armIntrinsics, accuracyNeeded, endianness);
            using var filterIntrinsics = new SampleToPcm16Converter(srcIntrinsics, true, x86Intrinsics, armIntrinsics, accuracyNeeded, endianness);
            short[] bufferNoIntrinsics = new short[128 * channels];
            short[] bufferIntrinsics = new short[bufferNoIntrinsics.Length];

            filterNoIntrinsics.Read(MemoryMarshal.Cast<short, byte>(bufferNoIntrinsics));
            filterIntrinsics.Read(MemoryMarshal.Cast<short, byte>(bufferIntrinsics));
            double sumdiff = 0;
            for (int i = 0; i < bufferNoIntrinsics.Length; i++)
            {
                short simple = bufferNoIntrinsics[i];
                short optimized = bufferIntrinsics[i];
                float diff = simple - optimized;
                sumdiff += MathF.Abs(diff);
                Console.WriteLine($"{simple}, {optimized}, {diff}");
            }
            Console.WriteLine($"Total difference: {sumdiff}");
            double avgDiff = sumdiff / bufferNoIntrinsics.Length;
            Console.WriteLine($"Average difference: {avgDiff}");
            Assert.Less(avgDiff, 1f - MathF.BitDecrement(1f));
        }

        [TestCase(1, false, Endianness.Little, (X86Intrinsics)X86IntrinsicsMask.Avx2)]
        [TestCase(1, false, Endianness.Big, (X86Intrinsics)X86IntrinsicsMask.Avx2)]
        public void CF2Pcm16IntrinsicsConsistency(int channels, bool isAccuracyNeeded, Endianness endianness, X86Intrinsics x86Intrinsics = (X86Intrinsics)~0ul, ArmIntrinsics armIntrinsics = (ArmIntrinsics)~0ul)
        {
            const int SampleRate = 48000;
            x86Intrinsics &= IntrinsicsUtils.X86Intrinsics;
            armIntrinsics &= IntrinsicsUtils.ArmIntrinsics;
            CheckIntrinsicsConsistency(SampleRate, channels, x86Intrinsics, armIntrinsics, isAccuracyNeeded, endianness);
        }
    }
}
