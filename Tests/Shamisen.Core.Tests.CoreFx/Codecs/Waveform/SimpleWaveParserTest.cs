using System;
using System.Collections.Generic;
using System.Text;
using Shamisen.Conversion.Resampling.Sample;
using Shamisen.Synthesis;
using NUnit.Framework;

//using CSCodec.Filters.Transformation;
using System.Numerics;
using System.Diagnostics;
using Shamisen.Filters;
using System.Buffers.Binary;
using Shamisen.Codecs.Waveform;
using Shamisen.Codecs.Waveform.Riff;
using System.IO;
using Shamisen.Data.Binary;
using Shamisen.Data;
using System.Reflection;
using Shamisen.Codecs.Waveform.Parsing;

namespace Shamisen.Core.Tests.CoreFx.Codecs.Waveform
{
    [TestFixture]
    public class SimpleWaveParserTest
    {
        public const string ResourcesPath = "Shamisen.Core.Tests.CoreFx.Resources";

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
            using (DataCache<byte> ms = GetDataFromResource("Test_rf64.wav"))
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

        private static DataCache<byte> GetDataFromResource(string name)
        {
            var lib = Assembly.GetExecutingAssembly();
            var ms = new DataCache<byte>();
            using (var stream = lib.GetManifestResourceStream($"{ResourcesPath}.{name}"))
            {
                using (var mem = new MemoryStream())
                {
                    stream.CopyTo(mem);
                    _ = mem.Seek(0, SeekOrigin.Begin);
                    ms.Write(mem.GetBuffer().AsSpan().Slice(0, (int)mem.Length));
                }
            }
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }
    }
}
