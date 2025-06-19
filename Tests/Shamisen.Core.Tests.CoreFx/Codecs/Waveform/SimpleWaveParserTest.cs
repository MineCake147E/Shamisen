using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
//using CSCodec.Filters.Transformation;
using System.Numerics;
using System.Reflection;
using System.Text;

using NUnit.Framework;

using Shamisen.Codecs.Waveform;
using Shamisen.Codecs.Waveform.Parsing;
using Shamisen.Codecs.Waveform.Riff;
using Shamisen.Conversion.Resampling.Sample;
using Shamisen.Core.Tests.CoreFx.TestUtils;
using Shamisen.Data;
using Shamisen.Data.Binary;
using Shamisen.Filters;
using Shamisen.Synthesis;

namespace Shamisen.Core.Tests.CoreFx.Codecs.Waveform
{
    [TestFixture]
    public class SimpleWaveParserTest
    {
        public const string ResourcesPath = TestHelper.ResourcesPath;

        [Test]
        public void ReadsSimpleWaveCorrectly()
        {
            var ms = GetDataFromResource("Test.wav");
            using (var parser = new SimpleWaveParser(new SimpleChunkParserFactory(), ms))
            {
                using (Assert.EnterMultipleScope())
                {
                    Assert.That(parser.Format.Encoding, Is.EqualTo(AudioEncoding.LinearPcm));
                    Assert.That(parser.Format.BitDepth, Is.EqualTo(24));
                    Assert.That(parser.Format.SampleRate, Is.EqualTo(192000));
                    Assert.That(parser.Format.Channels, Is.EqualTo(1));
                    Assert.That(parser.Format.SampleSize, Is.EqualTo(3));
                    Assert.That(parser.DataSize, Is.EqualTo(576000));
                }
            }
        }

        [Test]
        public void ReadsRf64WaveCorrectly()
        {
            using (var ms = GetDataFromResource("Test_rf64.wav"))
            {
                using (var parser = new SimpleWaveParser(new SimpleChunkParserFactory(), ms))
                {
                    using (Assert.EnterMultipleScope())
                    {
                        Assert.That(parser.Format.Encoding, Is.EqualTo(AudioEncoding.Extensible));
                        Assert.That(parser.Format.BitDepth, Is.EqualTo(16));
                        Assert.That(parser.Format.SampleRate, Is.EqualTo(192000));
                        Assert.That(parser.Format.Channels, Is.EqualTo(1));
                        Assert.That(parser.Format.SampleSize, Is.EqualTo(2));
                        Assert.That(parser.DataSize, Is.EqualTo(384000));
                    }
                }
            }
        }

        private static DataCache<byte> GetDataFromResource(string name) => TestHelper.GetDataCacheFromResource(name);
    }
}
