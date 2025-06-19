using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
//using CSCodec.Filters.Transformation;
using System.Numerics;
using System.Text;

using NUnit.Framework;

using Shamisen.Codecs.Waveform;
using Shamisen.Codecs.Waveform.Riff;
using Shamisen.Conversion.Resampling.Sample;
using Shamisen.Data;
using Shamisen.Data.Binary;
using Shamisen.Filters;
using Shamisen.Synthesis;

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
                using (Assert.EnterMultipleScope())
                {
                    Assert.DoesNotThrow(() => riffReader = new RiffChunkReader(ms));
                    Assert.That(riffReader, Is.Not.Null);
                    Assert.That(riffReader.ReadUInt32LittleEndian(), Is.EqualTo((uint)RiffSubChunkId.Wave));
                    IChunkReader fmt = null;
                    Assert.DoesNotThrow(() => fmt = riffReader.ReadSubChunk());
                    Assert.That(fmt, Is.Not.Null);
                    Assert.That(fmt.ReadUInt16LittleEndian(), Is.EqualTo((ushort)AudioEncoding.LinearPcm));
                    Assert.That(fmt.ReadUInt16LittleEndian(), Is.EqualTo(1));
                    Assert.That(fmt.ReadUInt32LittleEndian(), Is.EqualTo(192000));
                    Assert.That(fmt.ReadUInt32LittleEndian(), Is.EqualTo(768000));
                    Assert.That(fmt.ReadUInt16LittleEndian(), Is.EqualTo(4));
                    Assert.That(fmt.ReadUInt16LittleEndian(), Is.EqualTo(16));
                    Assert.That(fmt.TryReadByte(out _), Is.EqualTo(ReadResult.EndOfStream));
                    Assert.DoesNotThrow(() => fmt.Dispose());
                    IChunkReader data = null;
                    Assert.DoesNotThrow(() => data = riffReader.ReadSubChunk());
                    Assert.That(data, Is.Not.Null);
                    Assert.That(data.RemainingBytes, Is.EqualTo(128 - 44));
                }
            }
        }
    }
}
