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
                Assert.Multiple(() =>
                {
                    Assert.AreEqual(AudioEncoding.LinearPcm, parser.Format.Encoding);
                    Assert.AreEqual(24, parser.Format.BitDepth);
                    Assert.AreEqual(192000, parser.Format.SampleRate);
                    Assert.AreEqual(1, parser.Format.Channels);
                    Assert.AreEqual(3, parser.Format.SampleSize);
                    Assert.AreEqual(576000, parser.DataSize);
                });
            }
        }

        [Test]
        public void ReadsRf64WaveCorrectly()
        {
            using (var ms = GetDataFromResource("Test_rf64.wav"))
            {
                using (var parser = new SimpleWaveParser(new SimpleChunkParserFactory(), ms))
                {
                    Assert.Multiple(() =>
                    {
                        Assert.AreEqual(AudioEncoding.Extensible, parser.Format.Encoding);
                        Assert.AreEqual(16, parser.Format.BitDepth);
                        Assert.AreEqual(192000, parser.Format.SampleRate);
                        Assert.AreEqual(1, parser.Format.Channels);
                        Assert.AreEqual(2, parser.Format.SampleSize);
                        Assert.AreEqual(384000, parser.DataSize);
                    });
                }
            }
        }

        private static DataCache<byte> GetDataFromResource(string name) => TestHelper.GetDataCacheFromResource(name);
    }
}
