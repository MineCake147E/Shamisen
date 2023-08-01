using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Shamisen.Buffers;

namespace Shamisen.Utils.Tests.Memory
{
    [TestFixture]
    public class PooledArrayBufferWriterTests
    {
        [TestCase(0, 16384)]
        [TestCase(256, 16384)]
        [TestCase(511, 32767)]
        public void PooledArrayBufferWriterExpandsCorrectly(int initialSize, int newSize) => Assert.Multiple(() =>
        {
            PooledArrayBufferWriter<int>? a = null;
            try
            {
                Assert.DoesNotThrow(() => a = new(initialSize));
                Assert.That(a, Is.Not.Null);
                if (a is null) return;
                Assert.That(a.Capacity, Is.GreaterThanOrEqualTo(initialSize));
                if (initialSize > 0)
                {
                    var span = a.GetSpan(a.FreeCapacity);
                    Assert.That(a.FreeCapacity, Is.EqualTo(span.Length));
                    for (int i = 0; i < span.Length; i++)
                    {
                        span[i] = i;
                    }
                    Assert.DoesNotThrow(() => a.Advance(initialSize));
                }
                Assert.DoesNotThrow(() => a.GetMemory(newSize));
                if (initialSize > 0)
                {
                    var span = a.WrittenSpan;
                    Assert.That(span.Length, Is.EqualTo(initialSize));
                    for (int i = 0; i < span.Length; i++)
                    {
                        Assert.That(i, Is.EqualTo(span[i]));
                    }
                }
            }
            finally
            {
                a?.Dispose();
            }
        });

        [TestCase(0, -1, typeof(ArgumentException))]
        [TestCase(0, 1, typeof(InvalidOperationException))]
        [TestCase(256, 257, typeof(InvalidOperationException))]
        public void AdvanceThrowsCorrectly(int capacity, int count, Type type)
        {
            using var a = new PooledArrayBufferWriter<byte>(capacity);
            Assert.That(() => a.Advance(count), Throws.TypeOf(type));
        }
    }
}
