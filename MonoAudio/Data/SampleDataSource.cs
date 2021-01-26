using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MonoAudio.Data
{
    /// <summary>
    /// Translates calls between <see cref="IDataSource{TSample}"/> and <see cref="IReadableAudioSource{TSample, TFormat}"/>.
    /// </summary>
    /// <typeparam name="TSample">The type of the sample.</typeparam>
    /// <typeparam name="TFormat">The type of the format.</typeparam>
    public sealed class SampleDataSource<TSample, TFormat> : IDataSource<TSample>
        where TSample : unmanaged
        where TFormat : IAudioFormat<TSample>
    {
        private bool disposedValue = false;

        private IReadableAudioSource<TSample, TFormat> source;

        /// <summary>
        /// Initializes a new instance of the <see cref="SampleDataSource{TSample, TFormat}"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <exception cref="ArgumentNullException">source</exception>
        public SampleDataSource(IReadableAudioSource<TSample, TFormat> source) => this.source = source ?? throw new ArgumentNullException(nameof(source));

        /// <summary>
        /// Gets the current position of this <see cref="IDataSource{TSample}" />.
        /// </summary>
        /// <value>
        /// The position.
        /// </value>
        public ulong Position { get => (ulong)source.Position; }

        /// <summary>
        /// Reads the data to the specified destination.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <returns>
        /// The number of <typeparamref name="TSample" />s read from this <see cref="IDataSource{TSample}" />.
        /// </returns>
        public ReadResult Read(Span<TSample> destination) => source.Read(destination);

        /// <summary>
        /// Reads the data asynchronously to the specified destination.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <returns>
        /// The number of <typeparamref name="TSample" />s read from this <see cref="IDataSource{TSample}" />.
        /// </returns>
        public async ValueTask<ReadResult> ReadAsync(Memory<TSample> destination) => await source.ReadAsAsync(destination);

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    source.Dispose();
                }
                //source = null;
                disposedValue = true;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
