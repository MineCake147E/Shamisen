using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using Shamisen.Conversion.WaveToSampleConverters;
using Shamisen.Synthesis;

namespace Shamisen.Core.Tests.CoreFx.Conversion
{
    [TestFixture]
    public class ALawToSampleConverterTest
    {
        private const int Mask = 0b01010101;

        [TestCase((byte)(0b1_001_1010 ^ Mask), unchecked((short)0b0_000000_1_1010_1_000))]
        [TestCase((byte)(0b0_111_1111 ^ Mask), unchecked((short)-0b1_1111_1_000000_000))]
        public void ConvertsCorrectly(byte value, short expected)
            => Assert.AreEqual(expected, ALawToSampleConverter.ConvertALawToInt16(value));

        [TestCase((byte)(0b1_001_1010 ^ Mask), unchecked((short)0b0_000000_1_1010_1_000))]
        [TestCase((byte)(0b0_111_1111 ^ Mask), unchecked((short)-0b1_1111_1_000000_000))]
        public void SingleVariantConvertsCorrectly(byte value, short expected)
            => Assert.AreEqual(expected, (short)(ALawToSampleConverter.ConvertALawToSingle(value) * 32768.0f));
        [Test]
        public void SingleVariantConsistency()
        {
            Assert.Multiple(() =>
            {
                for(int i = 0; i < byte.MaxValue + 1; i++)
                {
                    Assert.AreEqual((float)ALawToSampleConverter.ConvertALawToInt16((byte)i), ALawToSampleConverter.ConvertALawToSingle((byte)i) * 32768.0f);
                }
            });
        }
        [Test]
        public void BlockConvertsCorrectly()
        {
            using var source = new DummySource<byte, IWaveFormat>(new WaveFormat(192000, 8, 1, AudioEncoding.Alaw));
            var converter = new ALawToSampleConverter(source);
            var buffer = new float[1024 * 1];
            var span = buffer.AsSpan();
            var rr = converter.Read(span);

        }
    }
}
