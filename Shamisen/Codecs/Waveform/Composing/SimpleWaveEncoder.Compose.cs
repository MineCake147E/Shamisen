using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Shamisen.Codecs.Waveform.Chunks;
using Shamisen.Codecs.Waveform.Riff;
using Shamisen.Data;
using Shamisen.Formats;

namespace Shamisen.Codecs.Waveform.Composing
{
#pragma warning disable S3453 // Classes should not have only "private" constructors

    public sealed partial class SimpleWaveEncoder
#pragma warning restore S3453 // Classes should not have only "private" constructors
    {
        internal static void Compose(IReadableAudioSource<byte, IWaveFormat> source, IDataSink<byte> sink, SimpleWaveEncoderOptions options)
        {
            if (sink.SeekSupport is { })
            {
                ComposeSeekable(source, sink, options);
            }
            else
            {
                ComposeUnseekable(source, sink, options);
            }
        }

        internal static void ComposeUnseekable(IReadableAudioSource<byte, IWaveFormat> source, IDataSink<byte> sink, SimpleWaveEncoderOptions options)
        {
            if (source.TotalLength is { } tlen)
            {
                if (tlen * (uint)source.Format.BlockSize <= MaxBufferSize)
                {
                    ComposeSimple(source, sink, options);
                }
                else
                {
                    ComposeComplexFixed(source, sink, options);
                }
            }
            else
            {
                throw new ArgumentException("Cannot compose wave file from infinitely long source to non-seekable sink!");
            }
        }

        internal static IRf64Chunk GenerateFactChunk(uint sampleCount)
            => new FactChunk(sampleCount);

        internal static MutableRf64Chunk GenerateFormatChunk(IWaveFormat format, bool preferFallbackForTest = false)
        {
            var fmt = new MutableRf64Chunk(ChunkId.Format);
            switch (format)
            {
                case IRf64Content content:
                    {
                        fmt.Add(content);
                        return fmt;
                    }
                case IExtensibleWaveFormat ef:
                    {
                        var std = new StandardWaveFormat(AudioEncoding.Extensible, (ushort)ef.Channels, (uint)ef.SampleRate,
                            (uint)ef.BlockSize * (uint)ef.SampleRate, (ushort)ef.BlockSize, (ushort)ef.BitDepth);
                        var ext = new ExtensibleWaveFormat(std, ef.ExtensionSize, ef.ValidBitsPerSample, ef.ChannelCombination, ef.SubFormat, ef.ExtraData);
                        fmt.Add(ext);
                        return fmt;
                    }

                case IChannelMaskedFormat masked:
                    {
                        var std = new StandardWaveFormat(AudioEncoding.Extensible, (ushort)format.Channels, (uint)format.SampleRate,
                            (uint)format.BlockSize * (uint)format.SampleRate, (ushort)format.BlockSize, (ushort)format.BitDepth);
                        var ext = new ExtensibleWaveFormat(std, 22, (ushort)format.BitDepth, masked.ChannelCombination, format.Encoding.ToGuid(), format.ExtraData);
                        fmt.Add(ext);
                        return fmt;
                    }
                case { } when !format.ExtraData.IsEmpty:
                    {
                        var std = new StandardWaveFormat(format.Encoding, (ushort)format.Channels, (uint)format.SampleRate,
                            (uint)format.BlockSize * (uint)format.SampleRate, (ushort)format.BlockSize, (ushort)format.BitDepth);
                        var ed = new StandardWaveFormatWithExtraData(std, format.ExtraData);
                        fmt.Add(ed);
                        return fmt;
                    }
                case { }:
                    {
                        var std = new StandardWaveFormat(format.Encoding, (ushort)format.Channels, (uint)format.SampleRate,
                            (uint)format.BlockSize * (uint)format.SampleRate, (ushort)format.BlockSize, (ushort)format.BitDepth);
                        fmt.Add(std);
                        return fmt;
                    }
                default:
                    throw new ArgumentException("", nameof(format));
            }
        }

        private static void ComposeComplexFixed(IReadableAudioSource<byte, IWaveFormat> source, IDataSink<byte> sink, SimpleWaveEncoderOptions options)
        {
            if (source.TotalLength is { } tlen)
            {
                //TODO: metadata support
                ulong dsize = tlen * (ulong)source.Format.BlockSize;
                var fmt = GenerateFormatChunk(source.Format);
                ulong riffSize = ChunkHeaderSize * 2 + fmt.ActualSize + sizeof(RiffSubChunkId) + dsize;
                uint dataSize = dsize > uint.MaxValue ? uint.MaxValue : (uint)dsize;
                MutableRf64Chunk riff;
                if (riffSize <= uint.MaxValue && options.PreferRiff)
                {
                    riff = new MutableRf64Chunk(ChunkId.Riff, (uint)riffSize)
                    {
                        BinaryContent.CreateLittleEndian((uint)RiffSubChunkId.Wave),
                        fmt,
                        GenerateFactChunk((uint)tlen)
                    };
                    riff.WriteTo(sink);
                }
                else
                {
                    var fact = GenerateFactChunk(uint.MaxValue);
                    riff = new MutableRf64Chunk(ChunkId.Rf64, uint.MaxValue)
                    {
                        BinaryContent.CreateLittleEndian((uint)RiffSubChunkId.Wave),
                        new Rf64DataSizeChunkHeader(
                            riffSize + (ulong)Unsafe.SizeOf<Rf64DataSizeChunkHeader>() + fmt.ActualSize
                            + fact.ActualSize + ChunkHeaderSize + dsize,
                            dsize, tlen, 0),
                        fmt,
                        fact
                    };
                    riff.WriteTo(sink);
                }
                //write data chunk directly
                sink.WriteInt32LittleEndian((int)ChunkId.Data);
                sink.WriteUInt32LittleEndian(dataSize);
                _ = WriteData(source, sink, tlen, riff.ActualSize + sizeof(uint) * 2);
            }
        }

        private static void ComposeSeekable(IReadableAudioSource<byte, IWaveFormat> source, IDataSink<byte> sink, SimpleWaveEncoderOptions options)
        {
            if (sink.SeekSupport is { } seek)
            {
                var fmt = GenerateFormatChunk(source.Format);
                byte[]? headerBuffer = new byte[sizeof(uint) * 3];
                var hSpan = headerBuffer.AsSpan();
                hSpan.FastFill(0);
                ref var rf64head = ref Unsafe.As<byte, RiffChunkHeader>(ref MemoryMarshal.GetReference(hSpan));
                ref var rf64subChunkId = ref Unsafe.As<byte, RiffSubChunkId>(ref MemoryMarshal.GetReference(hSpan.Slice(sizeof(uint) * 2)));
                rf64head = new((ChunkId)BinaryExtensions.ConvertToLittleEndian((uint)ChunkId.Rf64), uint.MaxValue);
                rf64subChunkId = (RiffSubChunkId)BinaryExtensions.ConvertToLittleEndian((uint)RiffSubChunkId.Wave);
                sink.Write(hSpan);

                new Rf64DataSizeChunkHeader().WriteTo(sink);
                fmt.WriteTo(sink);
                GenerateFactChunk(uint.MaxValue).WriteTo(sink);
                sink.WriteInt32LittleEndian((int)ChunkId.Data);
                sink.WriteInt32LittleEndian(-1);
                ulong dsize = WriteData(source, sink, 0, ChunkHeaderSize + fmt.ActualSize + (ulong)hSpan.Length);
                ulong riffSize = ChunkHeaderSize + fmt.ActualSize + dsize + (ulong)hSpan.Length;
                ulong sampleCount = dsize / (uint)source.Format.BlockSize;
                var ds64 = new Rf64DataSizeChunkHeader(riffSize, dsize, sampleCount, 0);
                seek.SeekTo(sizeof(uint) * 3);
                ds64.WriteTo(sink);
            }
        }

        private static void ComposeSimple(IReadableAudioSource<byte, IWaveFormat> source, IDataSink<byte> sink, SimpleWaveEncoderOptions options)
        {
            if (source.TotalLength is { } tlen)
            {
                byte[]? buffer = new byte[(int)tlen * source.Format.BlockSize];
                var rem = buffer.AsSpan();
                while (!rem.IsEmpty)
                {
                    var rr = source.Read(rem);
                    if (rr.IsEndOfStream)
                    {
                        tlen = (ulong)(buffer.Length - rem.Length) / (ulong)source.Format.BlockSize;
                        break;
                    }
                    rem = rem.Slice(rr.Length);
                }
                var data = new MutableRf64Chunk(ChunkId.Data)
                {
                    new BinaryContent(buffer.AsMemory(0,(int)tlen * source.Format.BlockSize))
                };
                var fact = GenerateFactChunk((uint)tlen);
                var riff = new MutableRf64Chunk(ChunkId.Riff)
                {
                    BinaryContent.CreateLittleEndian((uint)RiffSubChunkId.Wave),
                    GenerateFormatChunk(source.Format),
                    fact,
                    data
                };
                riff.WriteTo(sink);
            }
        }

        private static ulong WriteData(IReadableAudioSource<byte, IWaveFormat> source, IDataSink<byte> sink, ulong tlen, ulong headerSize)
        {
            byte[] buffer = new byte[MaxBufferSize - MaxBufferSize % source.Format.BlockSize];
            var span = buffer.AsSpan();
            ulong written = 0;
            ulong availableSize = sink.RemainingSpace ?? ulong.MaxValue - headerSize;
            while (true)
            {
                var rr = source.Read(span);
                if (rr.IsEndOfStream) break;
                if (availableSize - written < (ulong)rr.Length)
                {
                    break;
                }
                sink.Write(span.SliceWhileIfLongerThan(rr.Length));
                written += (ulong)rr.Length;
            }
            ulong length = tlen * (ulong)source.Format.BlockSize;
            if (written < length)
            {
                span.FastFill();
                while (written < length)
                {
                    var ns = span.SliceWhileIfLongerThan(length - written);
                    sink.Write(ns);
                    written += (ulong)ns.Length;
                }
            }
            return written;
        }
    }
}
