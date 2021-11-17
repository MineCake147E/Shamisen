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
        [TestCase((ushort)0)]
        [TestCase((ushort)1)]
        [TestCase((ushort)0x8000u)]
        [TestCase((ushort)0x8001u)]
        [TestCase((ushort)0xffffu)]
        [TestCase((ushort)0, 4097)]
        [TestCase((ushort)1, 4097)]
        [TestCase((ushort)0x8000u, 4097)]
        [TestCase((ushort)0x8001u, 4097)]
        [TestCase((ushort)0xffffu, 4097)]
        public void CalculateCrc16PclmulqdqCalculatesCorrectly(ushort initial, int length = 4095)
        {
            var u = new ulong[length];
            var v = MemoryMarshal.AsBytes(u.AsSpan());
            v.FastFill(0x00);
            new Random(3).NextBytes(v);
            //u[4033] = 0x80000000_00000001ul;
            var stdCrc = new FlacCrc16(initial);
            var optCrc = new FlacCrc16(initial);
            stdCrc *= u.AsSpan();
            optCrc = FlacCrc16.CalculateCrc16Pclmulqdq(optCrc, u.AsSpan());
            Console.WriteLine($"Expected: {stdCrc.State:X4}, Actual: {optCrc.State:X4}");
            Assert.AreEqual(stdCrc.State, optCrc.State);
        }
    }
}
