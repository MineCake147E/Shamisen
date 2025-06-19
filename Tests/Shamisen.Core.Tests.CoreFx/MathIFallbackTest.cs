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
    public class MathFallbackITest
    {
        [TestCase(0u, 32)]
        [TestCase(1u, 0)]
        [TestCase(2u, 1)]
        [TestCase(3u, 0)]
        public void TzcntReturnsCorrectly(uint value, int expected) => Assert.That(MathIFallbacks.TrailingZeroCount(value), Is.EqualTo(expected));

        [TestCase(0u, 0)]
        [TestCase(1u, 0)]
        [TestCase(2u, 1)]
        [TestCase(3u, 1)]
        public void Log2ReturnsCorrectly(uint value, int expected) => Assert.That(MathIFallbacks.LogBase2(value), Is.EqualTo(expected));

        [TestCase(0u, 32)]
        [TestCase(1u, 31)]
        [TestCase(2u, 30)]
        [TestCase(3u, 30)]
        [TestCase(0x8000_0000u, 0)]
        [TestCase(uint.MaxValue, 0)]
        public void LzcntReturnsCorrectly(uint value, int expected) => Assert.That(MathIFallbacks.LeadingZeroCount(value), Is.EqualTo(expected));

        /*
        [TestCase(0, 0)]
        [TestCase(1, 1)]
        [TestCase(-2, 0)]
        [TestCase(-1, 0)]
        public void RectifyReturnsCorrectly(int value, int expected) => Assert.AreEqual(expected, MathIFallbacks.Rectify(value));*/
    }
}
