﻿using System;
//using CSCodec.Filters.Transformation;
using NUnit.Framework;

using Shamisen.Codecs.Waveform.Composing;
using Shamisen.Conversion.SampleToWaveConverters;
using Shamisen.Data;
using Shamisen.Filters;
using Shamisen.Filters.Buffering;
using Shamisen.Synthesis;

namespace Shamisen.Core.Tests.CoreFx.Synthesis
{
    [TestFixture]
    public class SquareWaveSourceTest
    {
        private const double A4 = 440.0;
        private const double C5 = 523.2511306011972693556999870466094027289077206840796617283;

        [TestCase(1, 24000, 1024 + 31, 64, C5)]
        [TestCase(2, 24000, 1024 + 31, 64, C5)]
        [TestCase(1, 192000, 1024 + 31, 187, C5)]
        [TestCase(1, 192000, 1024 + 31, 187, 96000.0)]
        public void SquareWaveManyFrameDump(int channels, int sourceSampleRate, int frameLen = 1024, int framesToWrite = 64, double frequency = C5)
        {
            var src = new SquareWaveSource(new SampleFormat(channels, sourceSampleRate)) { Frequency = frequency };
            var path = new System.IO.FileInfo($"./dumps/SquareWaveDump_{channels}ch_{sourceSampleRate}_{frameLen}fpb_{DateTime.Now:yyyy_MM_dd_HH_mm_ss_fffffff}.wav");
            using var dc = new AudioCache<float, SampleFormat>(src.Format);
            Console.WriteLine(path.FullName);
            float[] buffer = new float[(ulong)frameLen * (ulong)channels];
            for (int i = 0; i < framesToWrite; i++)
            {
                var rr = src.Read(buffer);
                if (rr.HasData)
                {
                    dc.Write(buffer.AsSpan(0, rr.Length));
                }
                else
                {
                    break;
                }
            }
            var trunc = new LengthTruncationSource<float, SampleFormat>(dc, (ulong)framesToWrite * (ulong)frameLen);
            using (var ssink = new StreamDataSink(path.OpenWrite(), true, true))
            {
                Assert.DoesNotThrow(() =>
                SimpleWaveEncoder.Instance.Encode(new SampleToFloat32Converter(trunc), ssink));
            }

            Assert.Pass();
            src.Dispose();
        }
    }
}
