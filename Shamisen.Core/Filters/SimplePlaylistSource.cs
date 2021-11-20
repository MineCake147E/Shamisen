using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Filters
{
    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="TSample">The type of the sample.</typeparam>
    /// <typeparam name="TFormat">The type of the format.</typeparam>
    /// <seealso cref="IPlaylistSource{TSample, TFormat}" />
    public sealed class SimplePlaylistSource<TSample, TFormat> : IPlaylistSource<TSample, TFormat>, ILoopSupport
        where TSample : unmanaged where TFormat : IAudioFormat<TSample>
    {
        private bool disposedValue;

        /// <summary>
        /// Gets the current source.
        /// </summary>
        /// <value>
        /// The current source.
        /// </value>
        public IReadableAudioSource<TSample, TFormat>? CurrentSource { get; private set; }

        /// <summary>
        /// Gets the list support.
        /// </summary>
        /// <value>
        /// The list support.
        /// </value>
        public IList<IReadableAudioSource<TSample, TFormat>>? ListSupport { get; }

        /// <summary>
        /// Gets the loop support.
        /// </summary>
        /// <value>
        /// The loop support.
        /// </value>
        public ILoopSupport? LoopSupport => this;

        /// <summary>
        /// Gets the shuffle support.
        /// </summary>
        /// <value>
        /// The shuffle support.
        /// </value>
        public IShuffleSupport? ShuffleSupport { get; }

        /// <summary>
        /// Gets the simple addition support.
        /// </summary>
        /// <value>
        /// The simple addition support.
        /// </value>
        public ISimpleAdditionSupport<TSample, TFormat>? SimpleAdditionSupport { get; }

        /// <summary>
        /// Gets the sources.
        /// </summary>
        /// <value>
        /// The sources.
        /// </value>
        public IEnumerable<IReadableAudioSource<TSample, TFormat>> Sources => ShuffledContents.ToArray();

        private List<IReadableAudioSource<TSample, TFormat>> Contents { get; }

        private List<IReadableAudioSource<TSample, TFormat>> ShuffledContents { get; }

        /// <summary>
        /// Gets the format of the audio data.
        /// </summary>
        /// <value>
        /// The format of the audio data.
        /// </value>
        public TFormat Format { get; }

        /// <summary>
        /// Gets the remaining length of the <see cref="IAudioSource{TSample, TFormat}"/> in frames.<br/>
        /// The <c>null</c> means that the <see cref="IAudioSource{TSample, TFormat}"/> continues infinitely.
        /// </summary>
        /// <value>
        /// The remaining length of the <see cref="IAudioSource{TSample, TFormat}"/> in frames.
        /// </value>
        public ulong? Length { get; }

        /// <summary>
        /// Gets the total length of the <see cref="IAudioSource{TSample, TFormat}" /> in frames.<br/>
        /// The <c>null</c> means that the <see cref="IAudioSource{TSample, TFormat}"/> continues infinitely.
        /// </summary>
        /// <value>
        /// The total length of the <see cref="IAudioSource{TSample, TFormat}" /> in frames.
        /// </value>
        public ulong? TotalLength { get; }

        /// <summary>
        /// Gets the position of the <see cref="IAudioSource{TSample, TFormat}" /> in frames.<br/>
        /// The <c>null</c> means that the <see cref="IAudioSource{TSample, TFormat}"/> doesn't support this property.
        /// </summary>
        /// <value>
        /// The position of the <see cref="IAudioSource{TSample, TFormat}" /> in frames.
        /// </value>
        public ulong? Position { get; }

        /// <summary>
        /// Gets the skip support of the <see cref="IAudioSource{TSample,TFormat}"/>.
        /// </summary>
        /// <value>
        /// The skip support.
        /// </value>
        public ISkipSupport? SkipSupport { get; }

        /// <summary>
        /// Gets the seek support of the <see cref="IAudioSource{TSample,TFormat}"/>.
        /// </summary>
        /// <value>
        /// The seek support.
        /// </value>
        public ISeekSupport? SeekSupport { get; }

        bool ILoopSupport.IsLoopingEnabled { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimplePlaylistSource{TSample, TFormat}"/> class.
        /// </summary>
        /// <param name="contents">The contents.</param>
        /// <exception cref="ArgumentNullException">contents</exception>
        private SimplePlaylistSource(List<IReadableAudioSource<TSample, TFormat>> contents)
        {
            Contents = contents ?? throw new ArgumentNullException(nameof(contents));
            _ = contents.All(a => a.Format.Equals(contents.First().Format)) ? 0 : throw new ArgumentException("All sources", nameof(contents));
            ShuffledContents = new(contents);
            Format = contents.First().Format;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimplePlaylistSource{TSample, TFormat}"/> class.
        /// </summary>
        /// <param name="contents">The contents.</param>
        public SimplePlaylistSource(IEnumerable<IReadableAudioSource<TSample, TFormat>> contents)
            : this(new List<IReadableAudioSource<TSample, TFormat>>(contents))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimplePlaylistSource{TSample, TFormat}"/> class.
        /// </summary>
        /// <param name="format">The format.</param>
        public SimplePlaylistSource(TFormat format)
        {
            Contents = new();
            ShuffledContents = new();
            Format = format;
        }

        /// <summary>
        /// Reads the audio to the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>The length of the data written.</returns>
        public ReadResult Read(Span<TSample> buffer)
        {
            while (CurrentSource is null)
            {
                if (!MoveNext()) return ReadResult.EndOfStream;
            }
            var rr = CurrentSource.Read(buffer);
            while (rr.IsEndOfStream)
            {
                if (!MoveNext()) return ReadResult.EndOfStream;
                rr = CurrentSource.Read(buffer);
            }
            return rr;
        }

        private bool MoveNext()
        {
            if (ShuffledContents.Count > 0)
            {
                DequeueNext();
                return true;
            }
            else if (LoopSupport is { } ls && ls.IsLoopingEnabled)
            {
                //TODO: shuffle
                ShuffledContents.AddRange(Contents);
                if (ShuffledContents.Count > 0)
                {
                    DequeueNext();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private void DequeueNext()
        {
            var s = ShuffledContents[0];
            ShuffledContents.RemoveAt(0);
            CurrentSource = s;
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //
                }

                disposedValue = true;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <returns></returns>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
