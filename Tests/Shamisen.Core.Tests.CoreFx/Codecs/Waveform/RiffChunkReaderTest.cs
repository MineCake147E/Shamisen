using System;
using System.Collections.Generic;
using System.Text;
using Shamisen.Conversion.Resampling.Sample;
using Shamisen.Synthesis;
using NUnit.Framework;

//using CSCodec.Filters.Transformation;
using System.Numerics;
using System.Diagnostics;
using Shamisen.Filters;
using System.Buffers.Binary;
using Shamisen.Codecs.Waveform;
using Shamisen.Codecs.Waveform.Riff;
using System.IO;
using Shamisen.Data.Binary;
using Shamisen.Data;

namespace Shamisen.Core.Tests.CoreFx.Codecs.Waveform
{
    [TestFixture]
    public class RiffChunkReaderTest
    {
        [Test]
        public void ReadsRiffCorrectly()
        {
            Span<byte> testData = stackalloc byte[128];
            testData.FastFill();
            BinaryPrimitives.WriteUInt32LittleEndian(testData, (uint)ChunkId.Riff);
            BinaryPrimitives.WriteUInt32LittleEndian(testData.Slice(4), 36u);
            BinaryPrimitives.WriteUInt32LittleEndian(testData.Slice(8), (uint)RiffSubChunkId.Wave);
            BinaryPrimitives.WriteUInt32LittleEndian(testData.Slice(12), (uint)ChunkId.Format);
            BinaryPrimitives.WriteUInt32LittleEndian(testData.Slice(16), 16u);
            BinaryPrimitives.WriteUInt16LittleEndian(testData.Slice(20), (ushort)AudioEncoding.LinearPcm);
            BinaryPrimitives.WriteUInt16LittleEndian(testData.Slice(22), 1);
            BinaryPrimitives.WriteUInt32LittleEndian(testData.Slice(24), 192000u);
            BinaryPrimitives.WriteUInt32LittleEndian(testData.Slice(28), 768000u);
            BinaryPrimitives.WriteUInt16LittleEndian(testData.Slice(32), 4);
            BinaryPrimitives.WriteUInt16LittleEndian(testData.Slice(34), 16);
            BinaryPrimitives.WriteUInt32LittleEndian(testData.Slice(36), (uint)ChunkId.Data);
            BinaryPrimitives.WriteUInt32LittleEndian(testData.Slice(40), 128 - 44);
            using (var ms = new DataCache<byte>())
            {
                ms.Write(testData);
                ms.Seek(0, SeekOrigin.Begin);
                RiffChunkReader riffReader = null;
                Assert.Multiple(() =>
                {
                    Assert.DoesNotThrow(() => riffReader = new RiffChunkReader(ms));
                    Assert.NotNull(riffReader);
                    Assert.AreEqual((uint)RiffSubChunkId.Wave, riffReader.ReadUInt32LittleEndian());
                    IChunkReader fmt = null;
                    Assert.DoesNotThrow(() => fmt = riffReader.ReadSubChunk());
                    Assert.NotNull(fmt);
                    Assert.AreEqual((ushort)AudioEncoding.LinearPcm, fmt.ReadUInt16LittleEndian());
                    Assert.AreEqual(1, fmt.ReadUInt16LittleEndian());
                    Assert.AreEqual(192000, fmt.ReadUInt32LittleEndian());
                    Assert.AreEqual(768000, fmt.ReadUInt32LittleEndian());
                    Assert.AreEqual(4, fmt.ReadUInt16LittleEndian());
                    Assert.AreEqual(16, fmt.ReadUInt16LittleEndian());
                    Assert.AreEqual(ReadResult.EndOfStream, fmt.TryReadByte(out _));
                    Assert.DoesNotThrow(() => fmt.Dispose());
                    IChunkReader data = null;
                    Assert.DoesNotThrow(() => data = riffReader.ReadSubChunk());
                    Assert.NotNull(data);
                    Assert.AreEqual(128 - 44, data.RemainingBytes);
                });
            }
        }
    }
}
