using System;
using System.Collections.Generic;
using System.Text;
using Shamisen.Conversion.Resampling.Sample;
using Shamisen.Synthesis;
using NUnit.Framework;

//using CSCodec.Filters.Transformation;
using System.Numerics;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Shamisen.Core.Tests.CoreFx
{
    [TestFixture]
    public class SinusoidSourceTest
    {
        private const double Freq = 523.2511306011972693556999870466094027289077206840796617283;

        [TestCase(1, 24000)]
        [TestCase(2, 24000)]
        public void SinusoidManyFrameDump(int channels, int sourceSampleRate)
        {
            var src = new SinusoidSource(new SampleFormat(1, sourceSampleRate)) { Frequency = Freq };
            var h = new System.IO.FileInfo($"SinusoidDump_{channels}ch_{sourceSampleRate}_{DateTime.Now:yyyy_MM_dd_HH_mm_ss_fffffff}.raw");
            Console.WriteLine(h.FullName);
            using (var file = h.OpenWrite())
            {
                var buffer = new float[1024];
                var brspan = MemoryMarshal.Cast<float, byte>(buffer);
                for (int i = 0; i < 1024; i++)
                {
                    var rr = src.Read(buffer);
                    if (rr.HasData)
                    {
                        var bwspan = brspan.SliceWhile(rr.Length * sizeof(float));
                        file.Write(bwspan);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            Assert.Pass();
            src.Dispose();
        }
    }
}
