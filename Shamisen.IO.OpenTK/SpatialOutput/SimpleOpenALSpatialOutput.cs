using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Audio.OpenAL;
using OpenTK.Audio.OpenAL.ALC;

using ALCContextAttribute = OpenTK.Audio.OpenAL.ALC.ContextAttribute;
using ALAll = OpenTK.Audio.OpenAL.All;
using ALErrorCode = OpenTK.Audio.OpenAL.ErrorCode;
using ALStringName = OpenTK.Audio.OpenAL.StringName;

using Shamisen.IO.Spatial;
using System.Collections.Concurrent;
using System.Buffers;

namespace Shamisen.IO.OpenTK.OpenAL.SpatialOutput
{
    internal sealed class SimpleOpenALSpatialOutput<TSource> : OpenALSpatialOutput<TSource>
        where TSource : IWaveSource, ISpatializedAudioSource, IEquatable<TSource>, IComparable<TSource>
    {
#if DEBUG
        private const double MinLengthInMilliseconds = 16;
        private const int BUFNUM = 4;
#else
        private const double MinLengthInMilliseconds = 4;
        private const int BUFNUM = 16;
#endif
        private const int NumberOfBuffers = BUFNUM;
        private const double ReciprocalNumberOfBuffers = 1.0 / NumberOfBuffers;

        private readonly TimeSpan bufferSize;

        ConcurrentBag<TSource> addingSources = [];
        ConcurrentBag<TSource> finishedSources = [];

        List<SourceState> activeSources = [];

        Thread? loopThread;

        public SimpleOpenALSpatialOutput(ALCDevice device, TimeSpan latency)
            : base(device)
        {
            var l = latency * ReciprocalNumberOfBuffers;
            l = l.TotalMilliseconds < MinLengthInMilliseconds ? TimeSpan.FromMilliseconds(MinLengthInMilliseconds) : l;
            bufferSize = l;
            
        }

        public override bool Contains(TSource? source) => throw new NotImplementedException();
        public override void EjectFinishedSources<TBufferWriter>(TBufferWriter bufferWriter) => throw new NotImplementedException();
        public override void EjectFinishedSources<TBufferWriter>(scoped ref TBufferWriter bufferWriter) => throw new NotImplementedException();
        public override int EjectFinishedSources(Span<TSource> destination) => throw new NotImplementedException();
        public override void EjectFinishedSources(List<TSource> destination) => throw new NotImplementedException();
        public override IEnumerable<TSource> EjectFinishedSources() => throw new NotImplementedException();
        public override void Pause() => throw new NotImplementedException();
        public override void Play() => throw new NotImplementedException();
        public override void Resume() => throw new NotImplementedException();
        public override void Stop() => throw new NotImplementedException();
        public override void TrimFinishedSources() => throw new NotImplementedException();
        public override bool TryAddSource(TSource source) => throw new NotImplementedException();
        public override int TryAddSourceRange(params scoped ReadOnlySpan<TSource> sources) => throw new NotImplementedException();
        public override IEnumerable<bool> TryAddSourceRange<TEnumerable>(TEnumerable sources) => throw new NotImplementedException();

        private struct SourceState
        {
            public TSource Source { get; init; }
            public Format Format { get; init; }
            public int BufferSizeInBytes { get; init; }
            public PlaybackState PlaybackState { get; set; } = PlaybackState.NotInitialized;
            public bool IsFinished { get; set; } = false;

            public SourceState(TSource source, TimeSpan bufferSize)
            {
                Source = source;
                var format = source.Format;
                Format = format.ConvertToFormat();
                BufferSizeInBytes = format.GetBufferSizeRequired(bufferSize);
            }
        }
    }
}
