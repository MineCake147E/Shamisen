using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
//using CSCodec.Filters.Transformation;
using System.Numerics;
using System.Text;

using NUnit.Framework;

using Shamisen.Data;
using Shamisen.Filters;
using Shamisen.Synthesis;

namespace Shamisen.Core.Tests.CoreFx
{
    [TestFixture]
    public class DataCacheTest
    {
        [Test]
        public void CorrectlyStores() => Assert.Multiple(() =>
                                       {
                                           Span<byte> testData = stackalloc byte[sizeof(ulong)];
                                           BinaryPrimitives.WriteUInt64LittleEndian(testData, 8238509049889332737ul);
                                           var dc = new DataCache<byte>();
                                           dc.Write(testData);
                                           Assert.That(dc.BytesWritten, Is.EqualTo(testData.Length), nameof(dc.BytesWritten));
                                           Span<byte> testRead = stackalloc byte[sizeof(ulong)];
                                           var h = dc.Read(testRead);
                                           Assert.That(h.Length, Is.EqualTo(testRead.Length), nameof(h));
                                           Assert.That(BinaryPrimitives.ReadUInt64LittleEndian(testRead), Is.EqualTo(8238509049889332737ul));
                                       });
    }
}
