using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using OpenTK.Audio.OpenAL;
using OpenTK.Audio.OpenAL.ALC;

using ALCContextAttribute = OpenTK.Audio.OpenAL.ALC.ContextAttribute;
using ALAll = OpenTK.Audio.OpenAL.All;
using ALErrorCode = OpenTK.Audio.OpenAL.ErrorCode;
using ALStringName = OpenTK.Audio.OpenAL.StringName;

using Shamisen.Filters;
using Shamisen.IO.Spatial;

namespace Shamisen.IO.OpenTK.OpenAL
{
    /// <summary>
    /// Provides a base implementation of <see cref="ISpatialSoundOut{TSource, TSample, TFormat, TListener}"/> for OpenAL bindings.
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    public abstract class OpenALSpatialOutput<TSource> : ISpatialSoundOut<TSource, byte, IWaveFormat, SimpleMutableSpatialSoundListener>, ISoundOut
        where TSource : IWaveSource, ISpatializedAudioSource, IEquatable<TSource>, IComparable<TSource>
    {
        private uint disposedValue;
        private ALCContext context;
        private ALCDevice device;

        private protected ALCContext Context => context;

        /// <inheritdoc/>
        public SimpleMutableSpatialSoundListener Listener { get; init; }
        /// <inheritdoc/>
        public PlaybackState PlaybackState { get; }

        /// <summary>
        /// Gets the current rendering sample rate.
        /// </summary>
        public int SampleRate { get; }

        private protected OpenALSpatialOutput(ALCDevice device)
        {
            this.device = device;
            unsafe
            {
                context = ALC.CreateContext(device, (int*)null);
            }
            int sampleRate = -1;
            var sAttr = OpenALContextManager.GetContextAttributes(device);
            foreach (var entry in sAttr)
            {
                switch (entry.Key)
                {
                    case ALCContextAttribute.Frequency:
                        sampleRate = MathI.Max(sampleRate, entry.Value);
                        break;
                    default:
                        continue;
                }
                break;
            }
            sAttr.Clear();
            if (sampleRate < 0) throw new ArgumentException($"The device {device} is not supported!", nameof(device));
            SampleRate = sampleRate;
        }

        /// <inheritdoc/>
        public abstract bool Contains(TSource? source);
        /// <inheritdoc/>
        public abstract bool TryAddSource(TSource source);
        /// <inheritdoc/>
        public abstract int TryAddSourceRange(params scoped ReadOnlySpan<TSource> sources);
        /// <inheritdoc/>
        public abstract IEnumerable<bool> TryAddSourceRange<TEnumerable>(TEnumerable sources) where TEnumerable : IEnumerable<TSource>;
        /// <inheritdoc/>
        public abstract void TrimFinishedSources();
        /// <inheritdoc/>
        public abstract void EjectFinishedSources<TBufferWriter>(TBufferWriter bufferWriter) where TBufferWriter : class, IBufferWriter<TSource>;
        /// <inheritdoc/>
        public abstract void EjectFinishedSources<TBufferWriter>(scoped ref TBufferWriter bufferWriter) where TBufferWriter : struct, IBufferWriter<TSource>;
        /// <inheritdoc/>
        public abstract int EjectFinishedSources(Span<TSource> destination);
        /// <inheritdoc/>
        public abstract void EjectFinishedSources(List<TSource> destination);
        /// <inheritdoc/>
        public abstract IEnumerable<TSource> EjectFinishedSources();

        /// <inheritdoc/>
        public abstract void Play();
        /// <inheritdoc/>
        public abstract void Pause();
        /// <inheritdoc/>
        public abstract void Resume();
        /// <inheritdoc/>
        public abstract void Stop();

        /// <inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            if (Interlocked.Exchange(ref disposedValue, 1) == 0)
            {
                _ = ALC.CloseDevice(device);
                device = ALCDevice.Null;
                ALC.DestroyContext(context);
                context = ALCContext.Null;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~OpenALSpatialOutput()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        /// <inheritdoc/>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
