using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using Shamisen.Conversion.WaveToSampleConverters;

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
    }
}
