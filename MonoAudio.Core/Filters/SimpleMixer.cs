using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using MonoAudio.Utils;
using System.Runtime.CompilerServices;

namespace MonoAudio.Filters
{
    /// <summary>
    /// Mixes the sound simply from some input nodes.
    /// </summary>
    public sealed class SimpleMixer : ISampleSource
    {
        private bool disposedValue = false;

        private Dictionary<IReadableAudioSource<float, SampleFormat>, MixerEntry> Entries { get; set; }

        /// <summary>
        /// The buffer wrapper
        /// </summary>
        private ResizableBufferWrapper<float> bufferWrapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleMixer"/> class.
        /// </summary>
        /// <param name="format">The output format.</param>
        public SimpleMixer(SampleFormat format)
        {
            Format = format;
            Entries = new Dictionary<IReadableAudioSource<float, SampleFormat>, MixerEntry>();
            bufferWrapper = new ResizablePooledBufferWrapper<float>(1);
        }

        /// <summary>
        /// Gets a value indicating whether this instance supports seeking.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can seek; otherwise, <c>false</c>.
        /// </value>
        [Obsolete("Always returns false!", false)]
        public bool CanSeek => false;

        /// <summary>
        /// Gets the output format.
        /// </summary>
        /// <value>
        /// The format.
        /// </value>
        public SampleFormat Format { get; }

        /// <summary>
        /// Gets or sets the position.
        /// </summary>
        /// <value>
        /// The position.
        /// </value>
        /// <exception cref="NotSupportedException">
        /// </exception>
        [Obsolete("Not Supported!", true)]
        public long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        /// <summary>
        /// Gets the length.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        public long Length => -1;

        /// <summary>
        /// Adds the source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="volume">The volume.</param>
        /// <param name="disposeSourceOnRemove">if set to <c>true</c> it disposes source on removal.</param>
        public void AddSource(IReadableAudioSource<float, SampleFormat> source, float volume = 1, bool disposeSourceOnRemove = false)
        {
            var entry = new MixerEntry(source, volume, disposeSourceOnRemove);
            AddingQueue.Enqueue(entry);
        }

        /// <summary>
        /// Removes the source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <exception cref="ArgumentException">The Mixer doesn't contain the specified <paramref name="source"/>! - source</exception>
        public void RemoveSource(IReadableAudioSource<float, SampleFormat> source)
        {
            if (Entries.TryGetValue(source, out var entry))
            {
                DeletingQueue.Enqueue(entry);
            }
            else
            {
                throw new ArgumentException($"The Mixer doesn't contain the specified {nameof(source)}!", nameof(source));
            }
        }

        private Queue<MixerEntry> DeletingQueue { get; set; }

        private Queue<MixerEntry> AddingQueue { get; set; }

        /// <summary>
        /// Reads the audio to the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>The length of the data written.</returns>
        public int Read(Span<float> buffer)
        {
            buffer = CheckBuffer(buffer);
            FetchAddInternal();

            //Mixing
            var readBuffer = bufferWrapper.Buffer.Slice(0, buffer.Length);
            foreach (var item in Entries.Values)
            {
                readBuffer.FastFill(0);
                var rc = item.Source.Read(readBuffer);
                if (rc < readBuffer.Length)
                {
                    var rc2 = item.Source.Read(readBuffer.Slice(rc));
                }
                SpanExtensions.FastMix(readBuffer.Slice(0, rc), buffer, item.Volume);
            }
            RemoveRetiredInternal();

            return buffer.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void FetchAddInternal()
        {
            //Fetch additional entries
            while (AddingQueue.Count > 0)
            {
                var entry = AddingQueue.Dequeue();
                Entries.Add(entry.Source, entry);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RemoveRetiredInternal()
        {
            //Remove retired entries
            while (DeletingQueue.Count > 0)
            {
                var entry = DeletingQueue.Dequeue();
                Entries.Remove(entry.Source);
                entry.Dispose();
            }
        }

        private Span<float> CheckBuffer(Span<float> buffer)
        {
            //Check buffer
            buffer = buffer.SliceAlign(Format.Channels);
            if (bufferWrapper.Buffer.Length < buffer.Length)
            {
                bufferWrapper.Resize(buffer.Length);
            }

            return buffer;
        }

        #region IDisposable Support

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                }
                foreach (var item in Entries.Values)
                {
                    item.Dispose();
                }
                Entries = null;
                while (DeletingQueue.Count > 0)
                {
                    DeletingQueue.Dequeue()?.Dispose();
                }
                DeletingQueue = null;

                while (AddingQueue.Count > 0)
                {
                    AddingQueue.Dequeue()?.Dispose();
                }
                AddingQueue = null;
                disposedValue = true;
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="SimpleMixer"/> class.
        /// </summary>
        ~SimpleMixer()
        {
            Dispose(false);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}
