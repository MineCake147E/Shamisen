using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Filters
{
    /// <summary>
    /// Defines a base infrastructure of an audio source receiver that supports multiple audio sources.
    /// Implementations might be something like mixers, spatial renderers, etc.
    /// </summary>
    public interface IMultipleSourceAudioTarget<TSource, TSample, TFormat>
        where TSource : IAudioSource<TSample, TFormat>, IEquatable<TSource>
        where TSample : unmanaged
        where TFormat : IAudioFormat<TSample>
    {
        /// <summary>
        /// Tries to add a source.
        /// It fails to add the source when the source's format is incompatible with the <see cref="IMultipleSourceAudioTarget{TSource, TSample, TFormat}"/>, or the source is already added.
        /// </summary>
        /// <param name="source">The source to add.</param>
        /// <returns><see langword="true"/> if the source is successfully added to the <see cref="IMultipleSourceAudioTarget{TSource, TSample, TFormat}"/>; otherwise, <see langword="false"/>.</returns>
        bool TryAddSource(TSource source);

        /// <summary>
        /// Tries to add multiple sources at once.
        /// It aborts adding sources when it encounters a source that cannot be added.
        /// </summary>
        /// <param name="sources">The sources to add.</param>
        /// <returns>The number of sources that are successfully added to the <see cref="IMultipleSourceAudioTarget{TSource, TSample, TFormat}"/>.</returns>
        int TryAddSourceRange(params ReadOnlySpan<TSource> sources);

        /// <summary>
        /// Tries to add multiple sources at once.
        /// </summary>
        /// <param name="sources">The sources to add.</param>
        /// <returns>The number of sources that are successfully added to the <see cref="IMultipleSourceAudioTarget{TSource, TSample, TFormat}"/>.</returns>
        IEnumerable<bool> TryAddSourceRange<TEnumerable>(TEnumerable sources) where TEnumerable : IEnumerable<TSource>;

        /// <summary>
        /// Determines whether an element is in the <see cref="IMultipleSourceAudioTarget{TSource, TSample, TFormat}"/>.
        /// </summary>
        /// <param name="source">The <typeparamref name="TSource"/> to locate in the <see cref="IMultipleSourceAudioTarget{TSource, TSample, TFormat}"/>.
        /// The value can be <see langword="null"/> for reference types.</param>
        /// <returns><see langword="true"/> if <paramref name="source"/> is found in the <see cref="IMultipleSourceAudioTarget{TSource, TSample, TFormat}"/>; otherwise, <see langword="false"/>.</returns>
        bool Contains(TSource? source);

        /// <summary>
        /// Trims finished sources. All finished sources will be disposed.
        /// </summary>
        void TrimFinishedSources();

        /// <summary>
        /// Transfers finished sources to the given <see cref="IBufferWriter{T}"/>. Finished sources will not be disposed.
        /// </summary>
        /// <typeparam name="TBufferWriter">The type of <see cref="IBufferWriter{T}"/> that accepts the ejected <typeparamref name="TSource"/>.</typeparam>
        /// <param name="bufferWriter">The <see cref="IBufferWriter{T}"/> that accepts the ejected <typeparamref name="TSource"/>.</param>
        void EjectFinishedSources<TBufferWriter>(TBufferWriter bufferWriter) where TBufferWriter : class, IBufferWriter<TSource>;

        /// <summary>
        /// Transfers finished sources to the given <see cref="IBufferWriter{T}"/>. Finished sources will not be disposed.
        /// </summary>
        /// <typeparam name="TBufferWriter">The type of <see cref="IBufferWriter{T}"/> that accepts the ejected <typeparamref name="TSource"/>.</typeparam>
        /// <param name="bufferWriter">The reference to the <see cref="IBufferWriter{T}"/> that accepts the ejected <typeparamref name="TSource"/>.</param>
        void EjectFinishedSources<TBufferWriter>(scoped ref TBufferWriter bufferWriter) where TBufferWriter : struct, IBufferWriter<TSource>;

        /// <summary>
        /// Transfers finished sources to the given destination span. Finished sources will not be disposed.
        /// </summary>
        /// <param name="destination">The destination <see cref="Span{T}"/>.</param>
        /// <returns>The number of <typeparamref name="TSource"/> that are transferred.</returns>
        int EjectFinishedSources(Span<TSource> destination);

        /// <summary>
        /// Transfers finished sources to the given destination list. Finished sources will not be disposed.
        /// </summary>
        /// <param name="destination">The destination <see cref="List{T}"/>.</param>
        void EjectFinishedSources(List<TSource> destination);

        /// <summary>
        /// Ejects finished sources. Finished sources will not be disposed.
        /// </summary>
        /// <returns>The ejected finished sources.</returns>
        IEnumerable<TSource> EjectFinishedSources();
    }
}
