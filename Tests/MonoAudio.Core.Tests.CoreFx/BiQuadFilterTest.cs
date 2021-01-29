using System;
using System.Collections.Generic;
using System.Text;
using MonoAudio.Conversion.Resampling.Sample;
using MonoAudio.Synthesis;
using NUnit.Framework;

//using CSCodec.Filters.Transformation;
using System.Numerics;
using System.Diagnostics;
using MonoAudio.Filters;

namespace MonoAudio.Core.Tests.CoreFx
{
    [TestFixture]
    public class BiQuadFilterTest
    {
        private void BiQuadFilterDump(int sampleRate, BiQuadParameter parameter)
        {
            var src = new SquareWaveSource(new SampleFormat(1, sampleRate)) { Frequency = 2000 };
            var filter = new BiQuadFilter(src, parameter);
            var buffer = new float[128];
            filter.Read(buffer);
            foreach (var item in buffer)
            {
                Console.WriteLine(item);    //dump the transient part
            }
            filter.Read(buffer);
            foreach (var item in buffer)
            {
                Console.WriteLine(item);
            }
            filter.Dispose();
        }

        [Test]
        public void BiQuadLPFTwoFrameDump()
        {
            const int SampleRate = 48000;

            BiQuadFilterDump(SampleRate, BiQuadParameter.CreateLPFParameter(SampleRate, 4000, Math.Sqrt(0.5)));
            Assert.Pass();
        }

        [Test]
        public void BiQuadHPFTwoFrameDump()
        {
            const int SampleRate = 48000;
            BiQuadFilterDump(SampleRate, BiQuadParameter.CreateHPFParameter(SampleRate, 4000, Math.Sqrt(0.5)));
            Assert.Pass();
        }

        [Test]
        public void BiQuadBPFTwoFrameDumpFromBandWidth()
        {
            const int SampleRate = 48000;
            BiQuadFilterDump(SampleRate,    //Equivalent to Q=5.1875164769281621706495731692014
                BiQuadParameter.CreateBPFParameterFromBandWidth(SampleRate, 6000, 0.25, BpfGainKind.ZeroDBPeakGain));
            Assert.Pass();
        }

        [Test]
        public void BiQuadBPFTwoFrameDumpFromQuality()
        {
            const int SampleRate = 48000;
            BiQuadFilterDump(SampleRate, BiQuadParameter.CreateBPFParameterFromQuality(SampleRate, 6000, 1, BpfGainKind.ZeroDBPeakGain));
            Assert.Pass();
        }

        [Test]
        public void BiQuadAPFTwoFrameDump()
        {
            const int SampleRate = 48000;
            BiQuadFilterDump(SampleRate, BiQuadParameter.CreateAPFParameter(SampleRate, 6000, 1));
            Assert.Pass();
        }

        [Test]
        public void BiQuadNotchTwoFrameDumpFromBandWidth()
        {
            const int SampleRate = 48000;
            BiQuadFilterDump(SampleRate, BiQuadParameter.CreateNotchFilterParameterFromBandWidth(SampleRate, 6000, 0.25));
            Assert.Pass();
        }

        [Test]
        public void BiQuadNotchTwoFrameDumpFromQuality()
        {
            const int SampleRate = 48000;
            BiQuadFilterDump(SampleRate, BiQuadParameter.CreateNotchFilterParameterFromQuality(SampleRate, 6000, 1));
            Assert.Pass();
        }

        [Test]
        public void BiQuadPeakingEQTwoFrameDumpFromBandWidth()
        {
            const int SampleRate = 48000;
            BiQuadFilterDump(SampleRate, BiQuadParameter.CreatePeakingEqualizerParameterFromBandWidth(SampleRate, 6000, 0.25, -6));
            Assert.Pass();
        }

        [Test]
        public void BiQuadPeakingEQTwoFrameDumpFromQuality()
        {
            const int SampleRate = 48000;
            BiQuadFilterDump(SampleRate, BiQuadParameter.CreatePeakingEqualizerParameterFromQuality(SampleRate, 6000, 1, -6));
            Assert.Pass();
        }

        [Test]
        public void BiQuadHSFTwoFrameDumpFromSlope()
        {
            const int SampleRate = 48000;
            BiQuadFilterDump(SampleRate, BiQuadParameter.CreateHighShelfFilterParameterFromSlope(SampleRate, 6000, 6, -6));
            Assert.Pass();
        }

        [Test]
        public void BiQuadHSFTwoFrameDumpFromQuality()
        {
            const int SampleRate = 48000;
            BiQuadFilterDump(SampleRate, BiQuadParameter.CreateHighShelfFilterParameterFromQuality(SampleRate, 6000, 1, -6));
            Assert.Pass();
        }

        [Test]
        public void BiQuadLSFTwoFrameDumpFromSlope()
        {
            const int SampleRate = 48000;
            BiQuadFilterDump(SampleRate, BiQuadParameter.CreateLowShelfFilterParameterFromSlope(SampleRate, 6000, 6, -6));
            Assert.Pass();
        }

        [Test]
        public void BiQuadLSFTwoFrameDumpFromQuality()
        {
            const int SampleRate = 48000;
            BiQuadFilterDump(SampleRate, BiQuadParameter.CreateLowShelfFilterParameterFromQuality(SampleRate, 6000, 2, -6));
            Assert.Pass();
        }
    }
}
