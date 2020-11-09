using System;
using System.Collections.Generic;
using System.Text;

using MonoAudio.Synthesis;
using NUnit.Framework;

//using CSCodec.Filters.Transformation;
using System.Numerics;
using System.Diagnostics;
using MonoAudio.Filters;
using MonoAudio.Data;
using System.Buffers.Binary;

namespace MonoAudio.Core.Tests.CoreFx
{
    [TestFixture]
    public class DataCacheTest
    {
        [Test]
        public void CorrectlyStores()
        {
            Assert.Multiple(() =>
            {
                Span<byte> testData = stackalloc byte[sizeof(ulong)];
                BinaryPrimitives.WriteUInt64LittleEndian(testData, 8238509049889332737ul);
                var dc = new DataCache<byte>();
                dc.Write(testData);
                Assert.AreEqual(testData.Length, dc.BytesWritten, nameof(dc.BytesWritten));
                Span<byte> testRead = stackalloc byte[sizeof(ulong)];
                var h = dc.Read(testRead);
                Assert.AreEqual(testRead.Length, h.Length, nameof(h));
                Assert.AreEqual(8238509049889332737ul, BinaryPrimitives.ReadUInt64LittleEndian(testRead));
            });
        }
    }
}
