using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Shamisen.Data;

namespace Shamisen.Codecs.Waveform.Composing
{
    /// <summary>
    /// Represents a mutable RF64 chunk.
    /// </summary>
    public sealed class MutableRf64Chunk : IRf64Chunk, IList<IRf64Content>, IDisposable
    {
        private bool disposedValue;
        private readonly uint? size;

        /// <summary>
        /// Initializes a new instance of the <see cref="MutableRf64Chunk"/> class.
        /// </summary>
        /// <param name="chunkId">The chunk identifier.</param>
        /// <param name="size">The size value to overwrite.</param>
        /// <exception cref="ArgumentNullException">dataCache</exception>
        public MutableRf64Chunk(ChunkId chunkId, uint? size = null)
        {
            ChunkId = chunkId;
            this.size = size;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MutableRf64Chunk"/> class.
        /// </summary>
        /// <param name="chunkId">The chunk identifier.</param>
        /// <param name="contents">The contents.</param>
        /// <param name="size">The size value to overwrite.</param>
        /// <exception cref="ArgumentNullException">contents</exception>
        public MutableRf64Chunk(ChunkId chunkId, List<IRf64Content> contents, uint? size = null) : this(chunkId, size)
        {
            this.contents = contents ?? throw new ArgumentNullException(nameof(contents));
        }

        /// <summary>
        /// Gets the chunk identifier.
        /// </summary>
        /// <value>
        /// The chunk identifier.
        /// </value>
        public ChunkId ChunkId { get; }

        /// <summary>
        /// Gets the actual size including RIFF chunk header.
        /// </summary>
        /// <value>
        /// The actual size.
        /// </value>
        public ulong ActualSize => ContentSize + sizeof(uint) * 2;

        /// <summary>
        /// Gets the size of the content excluding header.
        /// </summary>
        /// <value>
        /// The size of the content.
        /// </value>
        public ulong ContentSize => size ?? contents.Aggregate(0ul, (a, b) => a + b.Size);

        /// <summary>
        /// Gets the size of the riff.
        /// </summary>
        /// <value>
        /// The size of the riff.
        /// </value>
        public uint RiffSize => ContentSize > uint.MaxValue ? uint.MaxValue : (uint)ContentSize;

        /// <summary>
        /// Gets the contents.
        /// </summary>
        /// <value>
        /// The contents.
        /// </value>
        public IEnumerable<IRf64Content>? Contents => contents;

        ulong IRf64Content.Size => ActualSize;

        /// <summary>
        /// Gets the number of elements contained in the <see cref="ICollection{T}" />.
        /// </summary>
        public int Count => ((ICollection<IRf64Content>)contents).Count;

        /// <summary>
        /// Gets a value indicating whether the <see cref="ICollection{T}" /> is read-only.
        /// </summary>
        public bool IsReadOnly => ((ICollection<IRf64Content>)contents).IsReadOnly;

        /// <summary>
        /// Gets or sets the <see cref="IRf64Content"/> at the specified index.
        /// </summary>
        /// <value>
        /// The <see cref="IRf64Content"/>.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public IRf64Content this[int index] { get => ((IList<IRf64Content>)contents)[index]; set => ((IList<IRf64Content>)contents)[index] = value; }

        private List<IRf64Content> contents = new();

        /// <summary>
        /// Writes this <see cref="IComparable"/> instance to <see cref="IDataSink{TSample}"/>.
        /// </summary>
        /// <param name="sink">The sink.</param>
        public void WriteTo(IDataSink<byte> sink)
        {
            sink.WriteUInt32LittleEndian((uint)ChunkId);
            sink.WriteUInt32LittleEndian(RiffSize);
            foreach (var item in contents)
            {
                item.WriteTo(sink);
            }
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
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Determines the index of a specific item in the <see cref="IList{T}"/>.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public int IndexOf(IRf64Content item) => ((IList<IRf64Content>)contents).IndexOf(item);

        /// <summary>
        /// Inserts an item to the <see cref="IList{T}"/> at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public void Insert(int index, IRf64Content item) => ((IList<IRf64Content>)contents).Insert(index, item);

        /// <summary>
        /// Removes the <see cref="IList{T}"/> item at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public void RemoveAt(int index) => ((IList<IRf64Content>)contents).RemoveAt(index);

        /// <summary>
        /// Adds the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public void Add(IRf64Content item) => ((ICollection<IRf64Content>)contents).Add(item);

        /// <summary>
        /// Clears this instance.
        /// </summary>
        /// <returns></returns>
        public void Clear() => ((ICollection<IRf64Content>)contents).Clear();

        /// <summary>
        /// Determines whether this instance contains the object.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>
        ///   <c>true</c> if the specified item exists; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(IRf64Content item) => ((ICollection<IRf64Content>)contents).Contains(item);

        /// <summary>
        /// Copies the elements of the <see cref="ICollection{T}"/> to an <see cref="Array"/>, starting at a particular <see cref="Array"/> index.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="arrayIndex">Index of the array.</param>
        /// <returns></returns>
        public void CopyTo(IRf64Content[] array, int arrayIndex) => ((ICollection<IRf64Content>)contents).CopyTo(array, arrayIndex);

        /// <summary>
        /// Removes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public bool Remove(IRf64Content item) => ((ICollection<IRf64Content>)contents).Remove(item);

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<IRf64Content> GetEnumerator() => ((IEnumerable<IRf64Content>)contents).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)contents).GetEnumerator();
    }
}
