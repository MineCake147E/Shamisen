using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Shamisen.Memory;

namespace Shamisen.Utils.Tests.Memory
{
    [TestFixture]
    public class NativeArrayTests
    {
        private static IEnumerable<nint> LengthValues
        {
            get
            {
                yield return 65536;
                var totalAvailableMemoryBytes = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes;
                if (Environment.Is64BitProcess && totalAvailableMemoryBytes > uint.MaxValue)
                {
                    yield return (nint)MathI.ExtractHighestSetBit((ulong)totalAvailableMemoryBytes) >> 3;
                }
            }
        }

        private static IEnumerable<bool> BoolValues
        {
            get
            {
                yield return true;
                yield return false;
            }
        }

        private static IEnumerable<byte> AlignmentExponentValues
        {
            get
            {
                for (var i = 0; i < 6; i++)
                {
                    yield return (byte)i;
                }
            }
        }

        public void NativeArrayConstructsCorrectly<T>(nint lengthInBytes, byte alignmentExponent, bool memoryPressure)
            => Assert.Multiple(() =>
            {
                var realLength = lengthInBytes / Unsafe.SizeOf<T>();
                using var narr = new NativeArray<T>(lengthInBytes, alignmentExponent, memoryPressure);
                Assert.That(narr.Length, Is.EqualTo(lengthInBytes));
                var expectedAlignment = (nint)1 << alignmentExponent;
                Assert.That(narr.RequestedAlignment, Is.EqualTo(expectedAlignment));
                Assert.That(narr.CurrentAlignment, Is.GreaterThanOrEqualTo(expectedAlignment));
                Assert.That(narr.IsDisposed, Is.False);
                Assert.That(narr.MemoryPressure, Is.EqualTo(memoryPressure));

            });

        private static IEnumerable<TestCaseData> NativeArrayConstructsCorrectlyTestCaseSource
            => LengthValues.SelectMany(len => AlignmentExponentValues.SelectMany(ae => BoolValues.Select(memp => new TestCaseData(len, ae, memp))));

        [NonParallelizable]
        [TestCaseSource(nameof(NativeArrayConstructsCorrectlyTestCaseSource))]
        public void NativeArrayConstructsCorrectlyForInt(nint lengthInBytes, byte alignmentExponent, bool memoryPressure) => NativeArrayConstructsCorrectly<int>(lengthInBytes, alignmentExponent, memoryPressure);

        [NonParallelizable]
        [TestCaseSource(nameof(NativeArrayConstructsCorrectlyTestCaseSource))]
        public void NativeArrayConstructsCorrectlyForString(nint lengthInBytes, byte alignmentExponent, bool memoryPressure) => NativeArrayConstructsCorrectly<string>(lengthInBytes, alignmentExponent, memoryPressure);
    }
}
