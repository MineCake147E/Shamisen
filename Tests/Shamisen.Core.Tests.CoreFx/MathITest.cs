using System;

using NUnit.Framework;

//using CSCodec.Filters.Transformation;

namespace Shamisen.Core.Tests.CoreFx
{
    [TestFixture]
    public class MathITest
    {
        [TestCase(0u, 32)]
        [TestCase(1u, 0)]
        [TestCase(2u, 1)]
        [TestCase(3u, 0)]
        public void TzcntReturnsCorrectly(uint value, int expected) => Assert.AreEqual(expected, MathI.TrailingZeroCount(value));

        [TestCase(0u, 0)]
        [TestCase(1u, 0)]
        [TestCase(2u, 1)]
        [TestCase(3u, 1)]
        public void Log2ReturnsCorrectly(uint value, int expected) => Assert.AreEqual(expected, MathI.LogBase2(value));

        [TestCase(0u, 32)]
        [TestCase(1u, 31)]
        [TestCase(2u, 30)]
        [TestCase(3u, 30)]
        [TestCase(0x8000_0000u, 0)]
        [TestCase(uint.MaxValue, 0)]
        public void LzcntReturnsCorrectly(uint value, int expected) => Assert.AreEqual(expected, MathI.LeadingZeroCount(value));

        [TestCase(0, 0)]
        [TestCase(1, 1)]
        [TestCase(-2, 0)]
        [TestCase(-1, 0)]
        public void RectifyReturnsCorrectly(int value, int expected) => Assert.AreEqual(expected, MathI.Rectify(value));

        [TestCase(0, 0)]
        [TestCase(1, 0)]
        [TestCase(2, 1)]
        [TestCase(7, 4)]
        [TestCase(-2, 5)]
        [TestCase(-1, -4)]
        public void MinNintReturnsCorrectly(int val1, int val2) => Assert.AreEqual(val1 > val2 ? val2 : (nint)val1, MathI.Min(val1, (nint)val2));
        [TestCase(0, 0)]
        [TestCase(1, 0)]
        [TestCase(2, 1)]
        [TestCase(7, 4)]
        [TestCase(-2, 5)]
        [TestCase(-1, -4)]
        public void MinReturnsCorrectly(int val1, int val2) => Assert.AreEqual(val1 > val2 ? val2 : val1, MathI.Min(val1, val2));
    }
}
