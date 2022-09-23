using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using Shamisen.Codecs.Waveform;
using Shamisen.Codecs.Waveform.Composing;
using Shamisen.Codecs.Waveform.Parsing;
using Shamisen.Conversion.SampleToWaveConverters;
using Shamisen.Data;
using Shamisen.Filters;
using Shamisen.Synthesis;

namespace Shamisen.Core.Tests.CoreFx.Codecs.Waveform
{
    [TestFixture]
    public class SimpleWaveEncoderTest
    {
        [TestCase(1, 192000, 192000ul)]
        [TestCase(2, 192000, 192000ul)]
        public void SeekableEncodesCorrectly(int channels, int sampleRate, ulong length)
        {
            var src = new SinusoidSource(new(channels, sampleRate)) { Frequency = 60 };
            var truncate = new LengthTruncationSource<float, SampleFormat>(src, length);
            var cnv = new SampleToPcm16Converter(truncate, false);
            using var ms = new MemoryStream();
            AssertEncode(channels, sampleRate, length, cnv, ms, nameof(SeekableEncodesCorrectly));
        }

        [TestCase(1, 192000, 192000ul)]
        [TestCase(2, 192000, 192000ul)]
        public void UnseekableComplexEncodesCorrectly(int channels, int sampleRate, ulong length)
        {
            var src = new SinusoidSource(new(channels, sampleRate)) { Frequency = 60 };
            var truncate = new LengthTruncationSource<float, SampleFormat>(src, length);
            var cnv = new SampleToPcm16Converter(truncate, false);
            using var ms = new MemoryStream();
            AssertEncode(channels, sampleRate, length, cnv, ms, nameof(UnseekableComplexEncodesCorrectly), true);
        }

        [TestCase(1, 192000, 32768ul)]
        [TestCase(2, 192000, 32768ul)]
        public void UnseekableSimpleEncodesCorrectly(int channels, int sampleRate, ulong length)
        {
            var src = new SinusoidSource(new(channels, sampleRate)) { Frequency = 60 };
            var truncate = new LengthTruncationSource<float, SampleFormat>(src, length);
            var cnv = new SampleToPcm16Converter(truncate, false);
            using var ms = new MemoryStream();
            AssertEncode(channels, sampleRate, length, cnv, ms, nameof(UnseekableSimpleEncodesCorrectly), true);
        }

        [TestCase((ushort)2u, 192000u, 192000ul)]
        public void ExtensibleFormatEncodesCorrectly(ushort channels, uint sampleRate, ulong length)
        {
            var src = new DummySource<byte, IWaveFormat>(
                new ExtensibleWaveFormat(
                    new StandardWaveFormat(AudioEncoding.Extensible, channels, sampleRate, 2u * channels * sampleRate, (ushort)(2u * channels), (ushort)16u)
                    , 24, (ushort)16u, StandardSpeakerChannels.SideStereo, AudioEncoding.LinearPcm.ToGuid(), default));

            var truncate = new LengthTruncationSource<byte, IWaveFormat>(src, length);
            using var ms = new MemoryStream();
            AssertEncode(channels, (int)sampleRate, length, truncate.ToWaveSource(), ms, nameof(ExtensibleFormatEncodesCorrectly), true, AudioEncoding.Extensible);
        }

        private static void AssertEncode(int channels, int sampleRate, ulong length, IWaveSource cnv, MemoryStream ms, string name, bool disableSeeking = false, AudioEncoding encoding = AudioEncoding.LinearPcm)
        {
            string path = $"./dumps/{name}_{channels}ch_{sampleRate}Hz_{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.wav";
            using (var ssink = new StreamDataSink(ms, false, disableSeeking))
            {
                Assert.DoesNotThrow(() => SimpleWaveEncoder.Instance.Encode(cnv, ssink));
                ms.Seek(0, SeekOrigin.Begin);
                if (!Directory.Exists("./dumps")) _ = Directory.CreateDirectory("./dumps");

                ms.CopyTo(File.OpenWrite(path));
            }
            ms.Seek(0, SeekOrigin.Begin);
            var ssource = new StreamDataSource(ms);
            var h = new SimpleWaveParser(new SimpleChunkParserFactory(), ssource);
            Assert.Multiple(() =>
            {
                Assert.AreEqual(channels, h.Format.Channels, "Channels differs!");
                Assert.AreEqual(sampleRate, h.Format.SampleRate, "SampleRate differs!");
                Assert.AreEqual(encoding, h.Format.Encoding, "Encoding differs!");
                Assert.AreEqual(16, h.Format.BitDepth, "BitDepth differs!");
                Assert.AreEqual(length, h.TotalLength, "Length differs!");
            });
        }
    }
}
