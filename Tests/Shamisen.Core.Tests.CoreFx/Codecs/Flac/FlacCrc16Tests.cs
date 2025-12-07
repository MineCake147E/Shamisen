using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using Shamisen.Codecs.Flac.Parsing;

namespace Shamisen.Core.Tests.CoreFx.Codecs.Flac
{
    [TestFixture]
    public class FlacCrc16Tests
    {
        private static IReadOnlyList<int> LengthSource() => [.. Enumerable.Range(0, 16), 16, 17, 4095, 4096, 4097];
        private static IReadOnlyList<ushort> InitialSource() => [0, 1, (ushort)0x8000u, (ushort)0x8001u, (ushort)0xffffu];

        [Test]
        public void CalculateCrc16PclmulqdqCalculatesCorrectly([ValueSource(nameof(InitialSource))] ushort initial, [ValueSource(nameof(LengthSource))] int length = 4095)
        {
            var u = new ulong[length];
            var v = MemoryMarshal.AsBytes(u.AsSpan());
            TestContext.CurrentContext.Random.NextBytes(v);
            var stdCrc = new FlacCrc16(initial);
            var optCrc = new FlacCrc16(initial);
            stdCrc *= u.AsSpan();
            optCrc = FlacCrc16.X86.CalculateCrc16UInt64BigEndianPclmulqdq(optCrc, u.AsSpan());
            Console.WriteLine($"Expected: {stdCrc.State:X4}, Actual: {optCrc.State:X4}");
            Assert.That(optCrc.State, Is.EqualTo(stdCrc.State));
        }

        [Test]
        public void ByteMultiplyOperatorCalculatesCorrectly([ValueSource(nameof(InitialSource))] ushort initial, [ValueSource(nameof(LengthSource))] int length = 4095)
        {
            var u = new byte[length];
            var v = u.AsSpan();
            TestContext.CurrentContext.Random.NextBytes(v);
            var stdCrc = new FlacCrc16(initial);
            var optCrc = new FlacCrc16(initial);
            for (int i = 0; i < v.Length; i++)
            {
                stdCrc *= v[i];
            }
            optCrc *= v;
            Console.WriteLine($"Expected: {stdCrc.State:X4}, Actual: {optCrc.State:X4}");
            Assert.That(optCrc.State, Is.EqualTo(stdCrc.State));
        }
    }
}
