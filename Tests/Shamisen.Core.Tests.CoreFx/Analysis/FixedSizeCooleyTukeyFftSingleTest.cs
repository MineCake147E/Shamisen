using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using Shamisen.Analysis;
using Shamisen.Core.Tests.CoreFx.TestUtils;

namespace Shamisen.Core.Tests.CoreFx.Analysis
{
    [TestFixture]
    public class FixedSizeCooleyTukeyFftSingleTest
    {

        [TestCase(2048)]
        [TestCase(4096)]
        public void FftSingleConsistency(int size)
        {
            size = (int)MathI.ExtractHighestSetBit((uint)size);
            var array = new ComplexF[size];
            TestHelper.GenerateRandomComplexNumbers(array);
            var dst = new ComplexF[array.Length];
            var exp = new ComplexF[array.Length];
            array.AsSpan().CopyTo(dst);
            array.AsSpan().CopyTo(exp);
            CooleyTukeyFft.FFT(exp, FftMode.Forward);
            var fftp = new FixedSizeCooleyTukeyFftSingle(size, FftMode.Forward);
            fftp.PerformFft(dst);
            TestHelper.AssertArrays(exp, dst);
        }
    }
}
