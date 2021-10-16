using System;
using System.Collections.Generic;
using System.Diagnostics;
//using CSCodec.Filters.Transformation;
using System.Numerics;
using System.Text;

using NUnit.Framework;

using Shamisen.Conversion.Resampling.Sample;
using Shamisen.Filters;
using Shamisen.Synthesis;

namespace Shamisen.Core.Tests.CoreFx
{
    [TestFixture]
    public class AttenuatorTest
    {
        [Test]
        public void AttenuatorAttenuatesCorrectly()
        {
            const int Channels = 3;
            const int SampleRate = 384000;
            var src = new SquareWaveSource(new SampleFormat(Channels, SampleRate)) { Frequency = 1 };
            var att = new Attenuator(src)
            {
                Scale = 0.5f
            };
            float[] buffer = new float[Channels * 1024];
            att.Read(buffer);
            Assert.AreEqual(0.5f, buffer[Channels]);
            att.Dispose();
        }
    }
}