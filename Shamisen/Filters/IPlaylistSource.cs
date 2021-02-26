using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Filters
{
    /// <summary>
    /// Defines a base infrastructure of a playlist source.
    /// </summary>
    public interface IPlaylistSource<TSample, TFormat> : IMultipleInputAudioFilter<TSample, TFormat>
        where TSample : unmanaged where TFormat : IAudioFormat<TSample>
    {
        /// <summary>
        /// Gets the current source.
        /// </summary>
        /// <value>
        /// The current source.
        /// </value>
        IReadableAudioSource<TSample, TFormat>? CurrentSource { get; }

        /// <summary>
        /// Gets the shuffle support.
        /// </summary>
        /// <value>
        /// The shuffle support.
        /// </value>
        IShuffleSupport? ShuffleSupport { get; }

        /// <summary>
        /// Gets the loop support.
        /// </summary>
        /// <value>
        /// The loop support.
        /// </value>
        ILoopSupport? LoopSupport { get; }

        /// <summary>
        /// Gets the simple addition support.
        /// </summary>
        /// <value>
        /// The simple addition support.
        /// </value>
        ISimpleAdditionSupport<TSample, TFormat>? SimpleAdditionSupport { get; }

        /// <summary>
        /// Gets the list support.
        /// </summary>
        /// <value>
        /// The list support.
        /// </value>
        IList<IReadableAudioSource<TSample, TFormat>>? ListSupport { get; }
    }

    /// <summary>
    /// Defines a base infrastructure that contains loop support of <see cref="IPlaylistSource{TSample, TFormat}"/>.
    /// </summary>
    public interface ILoopSupport
    {
        /// <summary>
        /// Gets a value indicating whether this instance is looping enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is looping enabled; otherwise, <c>false</c>.
        /// </value>
        bool IsLoopingEnabled { get; set; }
    }

    /// <summary>
    /// Defines a base infrastructure that contains shuffle support of <see cref="IPlaylistSource{TSample, TFormat}"/>.
    /// </summary>
    public interface IShuffleSupport
    {
        /// <summary>
        /// Shuffles the <see cref="IPlaylistSource{TSample, TFormat}"/>.
        /// </summary>
        void Shuffle();

        /// <summary>
        /// Undoes the shuffle of <see cref="IPlaylistSource{TSample, TFormat}"/>.
        /// </summary>
        void UndoShuffle();
    }

    /// <summary>
    /// Defines a base infrastructure that contains source addition support of <see cref="IPlaylistSource{TSample, TFormat}"/>.
    /// </summary>
    /// <typeparam name="TSample">The type of the sample.</typeparam>
    /// <typeparam name="TFormat">The type of the format.</typeparam>
    public interface ISimpleAdditionSupport<TSample, TFormat>
        where TSample : unmanaged where TFormat : IAudioFormat<TSample>
    {
        /// <summary>
        /// Adds the specified source to the tail of the playlist.
        /// </summary>
        /// <param name="source">The source to add.</param>
        void AddToTail(IReadableAudioSource<TSample, TFormat> source);
    }
}
