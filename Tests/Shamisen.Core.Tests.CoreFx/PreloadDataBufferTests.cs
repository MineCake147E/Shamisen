using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

using Shamisen.Core.Tests.CoreFx.TestUtils;
using Shamisen.Data;
using Shamisen.Data.Binary;

namespace Shamisen.Core.Tests.CoreFx
{
    using TVector = Vector256<ulong>;
    using TVectorInside = UInt64;

    [TestFixture]
    [NonParallelizable]
    public class PreloadDataBufferTests
    {
        private static IEnumerable<int> Sizes => new int[] { 2048, 4096 };

        private static IEnumerable<int> NumbersOfBuffers => new int[] { 64 };

        private static IEnumerable<TimeSpan> Timeouts => new double[] { 10000 }.Select(a => TimeSpan.FromMilliseconds(a));

        public static IEnumerable<TestCaseData> TestCases
            => Sizes.SelectMany(a => NumbersOfBuffers.SelectMany(b => Timeouts.Select(c => new TestCaseData(a, b, c))));

        [TestCaseSource(nameof(TestCases))]
        [NonParallelizable]
        public void ReadsCorrectly(int initialBlockSize, int bufferCount, TimeSpan timeout)
        {
            Vector128<ulong> a;
            Span<byte> q = stackalloc byte[Unsafe.SizeOf<Vector128<ulong>>()];
            RandomNumberGenerator.Fill(q);
            a = MemoryMarshal.Cast<byte, Vector128<ulong>>(q)[0];
            ulong seed = a.GetElement(0), id = a.GetElement(1);
            using var ns = new RandomDataSource(seed, id, 0);
            using var bs = new RandomDataSource(seed, id, 1, false);
            using var ps = new PreloadDataBuffer<byte>(bs, initialBlockSize, bufferCount, true);
            RandomNumberGenerator.Fill(q);
            a = MemoryMarshal.Cast<byte, Vector128<ulong>>(q)[0];
            ulong hseed = a.GetElement(0), hid = a.GetElement(1);
            using var ls = new RandomDataSource(hseed, hid, 4);
            Memory<byte> gn = new byte[sizeof(ulong) * 128];
            Memory<byte> gb = new byte[gn.Length];
            Memory<byte> pn = new byte[gn.Length];
            Memory<byte> pb = new byte[gn.Length];
            var vgn = MemoryMarshal.Cast<byte, TVector>(gn.Span);
            var vgb = MemoryMarshal.Cast<byte, TVector>(gb.Span);
            var vpn = MemoryMarshal.Cast<byte, TVector>(pn.Span);
            var vpb = MemoryMarshal.Cast<byte, TVector>(pb.Span);
            try
            {
                for (var i = 0; i < 1 << 15; i++)
                {
                    var u = ls.ReadByte();
                    var len = gb.Length - u;
                    TestHelper.DoesNotTakeSoLong(() => ps.ReadAll(gb.Span.SliceWhile(len)), timeout);
                    ns.ReadAll(gn.Span.SliceWhile(len));
                    for (var j = 0; j < vgn.Length; j++)
                    {
                        Assert.AreEqual(vgn[j], vgb[j], $"On the {i}th try, {j}th element: The elements aren't the same");
                    }
                    var hn = gn;
                    gn = pn;
                    pn = hn;
                    var hb = gb;
                    gb = pb;
                    pb = hb;
                    var vhn = vgn;
                    vgn = vpn;
                    vpn = vhn;
                    var vhb = vgb;
                    vgb = vpb;
                    vpb = vhb;
                }
            }
            catch (Exception)
            {
                Console.WriteLine($"The stream: {seed}, {id}");
                for (var i = 0; i < vgn.Length; i++)
                {
                    Console.WriteLine($"{i}: {FormatAllElementsHexadecimal(vgn[i])}, {FormatAllElementsHexadecimal(vgb[i])}");
                }
                Console.WriteLine("Previous:");
                for (var i = 0; i < vpn.Length; i++)
                {
                    Console.WriteLine($"{i}: {FormatAllElementsHexadecimal(vpn[i])}, {FormatAllElementsHexadecimal(vpb[i])}");
                }
                throw;
            }
            Assert.Pass();
        }

        private static string FormatAllElementsHexadecimal(TVector vector)
        {
            var sb = new StringBuilder();
            sb.Append('<');
            var elements = new TVectorInside[TVector.Count];
            for (var i = 0; i < TVector.Count; i++)
            {
                elements[i] = vector.GetElement(i);
            }
            var format = $"X{sizeof(TVectorInside) * 2}";
            sb.AppendJoin(',', elements.Select(a => a.ToString(format)));
            sb.Append('>');
            return sb.ToString();
        }
    }
}
