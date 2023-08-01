using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

namespace Shamisen.TestUtils
{
    public class AssertionHelper
    {
        public static void AreEqual<T>(Span<T> expected, Span<T> actual) => AreEqual((ReadOnlySpan<T>)expected, (ReadOnlySpan<T>)actual);
        public static void AreEqual<T>(Span<T> expected, ReadOnlySpan<T> actual) => AreEqual((ReadOnlySpan<T>)expected, actual);
        public static void AreEqual<T>(ReadOnlySpan<T> expected, Span<T> actual) => AreEqual(expected, (ReadOnlySpan<T>)actual);

        public static void AreEqual<T>(ReadOnlySpan<T> expected, ReadOnlySpan<T> actual)
        {
            Assert.AreEqual(expected.Length, actual.Length);
            if (AreEqual(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(expected)), ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(actual)),
                checked((nuint)expected.Length * (nuint)Unsafe.SizeOf<T>())))
            {
                Assert.Pass();
                return;
            }
            Assert.AreEqual(expected.ToArray(), actual.ToArray());
        }

        private static bool AreEqual(ref byte expected, ref byte actual, nuint length)
        {
            if (Unsafe.AreSame(ref expected, ref actual)) return true;
            var flag = true;
            nuint i = 0;
            Vector<byte> v0_ns, v1_ns, v2_ns, v3_ns;
            v0_ns = v1_ns = v2_ns = v3_ns = new(byte.MaxValue);
            var olen = length - 8 * (nuint)Vector<byte>.Count + 1;
            if (olen < length)
            {
                for (; i < olen; i += 8 * (nuint)Vector<byte>.Count)
                {
                    var v4_ns = Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref expected, i + 0 * (nuint)Vector<byte>.Count));
                    var v5_ns = Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref expected, i + 1 * (nuint)Vector<byte>.Count));
                    var v6_ns = Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref expected, i + 2 * (nuint)Vector<byte>.Count));
                    var v7_ns = Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref expected, i + 3 * (nuint)Vector<byte>.Count));
                    v4_ns = Vector.Equals(v4_ns, Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref actual, i + 0 * (nuint)Vector<byte>.Count)));
                    v5_ns = Vector.Equals(v5_ns, Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref actual, i + 1 * (nuint)Vector<byte>.Count)));
                    v6_ns = Vector.Equals(v6_ns, Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref actual, i + 2 * (nuint)Vector<byte>.Count)));
                    v7_ns = Vector.Equals(v7_ns, Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref actual, i + 3 * (nuint)Vector<byte>.Count)));
                    v0_ns &= v4_ns;
                    v4_ns = Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref expected, i + 4 * (nuint)Vector<byte>.Count));
                    v1_ns &= v5_ns;
                    v5_ns = Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref expected, i + 5 * (nuint)Vector<byte>.Count));
                    v2_ns &= v6_ns;
                    v6_ns = Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref expected, i + 6 * (nuint)Vector<byte>.Count));
                    v3_ns &= v7_ns;
                    v7_ns = Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref expected, i + 7 * (nuint)Vector<byte>.Count));
                    v4_ns = Vector.Equals(v4_ns, Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref actual, i + 4 * (nuint)Vector<byte>.Count)));
                    v5_ns = Vector.Equals(v5_ns, Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref actual, i + 5 * (nuint)Vector<byte>.Count)));
                    v6_ns = Vector.Equals(v6_ns, Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref actual, i + 6 * (nuint)Vector<byte>.Count)));
                    v7_ns = Vector.Equals(v7_ns, Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref actual, i + 7 * (nuint)Vector<byte>.Count)));
                    v0_ns &= v4_ns;
                    v1_ns &= v5_ns;
                    v2_ns &= v6_ns;
                    v3_ns &= v7_ns;
                }
            }
            v0_ns &= v2_ns;
            v1_ns &= v3_ns;
            olen = length - 2 * (nuint)Vector<byte>.Count + 1;
            if (olen < length)
            {
                for (; i < olen; i += 2 * (nuint)Vector<byte>.Count)
                {
                    var v4_ns = Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref expected, i + 0 * (nuint)Vector<byte>.Count));
                    var v5_ns = Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref expected, i + 1 * (nuint)Vector<byte>.Count));
                    v4_ns = Vector.Equals(v4_ns, Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref actual, i + 0 * (nuint)Vector<byte>.Count)));
                    v5_ns = Vector.Equals(v5_ns, Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref actual, i + 1 * (nuint)Vector<byte>.Count)));
                    v0_ns &= v4_ns;
                    v1_ns &= v5_ns;
                }
            }
            v0_ns &= v1_ns;
            olen = length - (nuint)Vector<byte>.Count + 1;
            if (olen < length)
            {
                for (; i < olen; i += (nuint)Vector<byte>.Count)
                {
                    var v4_ns = Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref expected, i));
                    v4_ns = Vector.Equals(v4_ns, Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref actual, i)));
                    v0_ns &= v4_ns;
                }
            }
            v3_ns = new(byte.MaxValue);
            flag &= v0_ns == v3_ns;
            if (!flag) return flag;
            for (; i < length; i++)
            {
                flag &= Unsafe.Add(ref expected, i) == Unsafe.Add(ref actual, i);
            }
            return flag;
        }
    }
}
