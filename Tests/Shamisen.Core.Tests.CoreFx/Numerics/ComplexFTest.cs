using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

namespace Shamisen.Core.Tests.CoreFx.Numerics
{
    [TestFixture]
    public class ComplexFTest
    {
        private const float SqrtHalf = 0.70710678118654752440084436210485f;
        private static ComplexF[] Values => new[] {
            new ComplexF(1.0f, 0.0f), new ComplexF(0.0f, 1.0f), new ComplexF(-1.0f, 0.0f), new ComplexF(0.0f, -1.0f),
            new ComplexF(SqrtHalf, SqrtHalf), new ComplexF(SqrtHalf, -SqrtHalf), new ComplexF(-SqrtHalf, SqrtHalf), new ComplexF(-SqrtHalf, -SqrtHalf),
        };
        private static IEnumerable<TestCaseData> UaryTestCaseSource()
        {
            var v = Values;
            return v.Select(b => new TestCaseData(b));
        }
        private static IEnumerable<TestCaseData> BinaryTestCaseSource()
        {
            var v = Values;
            return v.SelectMany(a => v.Select(b => new TestCaseData(a, b)));
        }
        #region Binary Operations
        [TestCaseSource(nameof(BinaryTestCaseSource))]
        public void CorrectlyAdds(ComplexF a, ComplexF b)
        {
            var c = a + b;
            Assert.Multiple(() =>
            {
                Assert.AreEqual(a.Real + b.Real, c.Real);
                Assert.AreEqual(a.Imaginary + b.Imaginary, c.Imaginary);
            });
        }
        [TestCaseSource(nameof(BinaryTestCaseSource))]
        public void CorrectlySubtracts(ComplexF a, ComplexF b)
        {
            var c = a - b;
            Assert.Multiple(() =>
            {
                Assert.AreEqual(a.Real - b.Real, c.Real);
                Assert.AreEqual(a.Imaginary - b.Imaginary, c.Imaginary);
            });
        }
        [TestCaseSource(nameof(BinaryTestCaseSource))]
        public void CorrectlyMultiplies(ComplexF a, ComplexF b)
        {
            var c = a * b;
            Assert.Multiple(() =>
            {
                Assert.AreEqual(a.Real * b.Real - a.Imaginary * b.Imaginary, c.Real);
                Assert.AreEqual(a.Real * b.Imaginary + a.Imaginary * b.Real, c.Imaginary);
            });
        }
        #endregion
    }
}
