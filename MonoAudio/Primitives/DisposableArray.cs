using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace MonoAudio.Primitives
{
    /// <summary>
    /// Provides a disposable array.
    /// </summary>
    public sealed partial class DisposableArray<T> : IList<T>, IStructuralComparable, IStructuralEquatable
    {
        private T[] actualArray;

        /// <summary>
        /// Initializes a new instance of the <see cref="DisposableArray{T}"/> class.
        /// </summary>
        /// <param name="length">The length.</param>
        public DisposableArray(int length)
        {
            actualArray = new T[length];
        }

        /// <summary>
        /// Gets or sets the <typeparamref name="T"/> value at the specified index.
        /// </summary>
        /// <value>
        /// The <typeparamref name="T"/> value.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public T this[int index] { get => ((IList<T>)actualArray)[index]; set => ((IList<T>)actualArray)[index] = value; }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="ICollection{T}"></see>.
        /// </summary>
        public int Count => ((IList<T>)actualArray).Count;

        /// <summary>
        /// Gets a value indicating whether the <see cref="ICollection{T}"></see> is read-only.
        /// </summary>
        public bool IsReadOnly => ((IList<T>)actualArray).IsReadOnly;

        /// <summary>
        /// Adds an item to the <see cref="ICollection{T}"></see>.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="ICollection{T}"></see>.</param>
        public void Add(T item) => ((IList<T>)actualArray).Add(item);

        /// <summary>
        /// Removes all items from the <see cref="ICollection{T}"></see>.
        /// </summary>
        public void Clear() => ((IList<T>)actualArray).Clear();

        /// <summary>
        /// Compares to.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <param name="comparer">The comparer.</param>
        /// <returns></returns>
        public int CompareTo(object? other, IComparer comparer) => ((IStructuralComparable)actualArray).CompareTo(other, comparer);

        /// <summary>
        /// Determines whether this instance contains the object.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="ICollection{T}"></see>.</param>
        /// <returns>
        /// true if <paramref name="item">item</paramref> is found in the <see cref="ICollection{T}"></see>; otherwise, false.
        /// </returns>
        public bool Contains(T item) => ((IList<T>)actualArray).Contains(item);

        /// <summary>
        /// Copies the elements of the <see cref="ICollection{T}"></see> to an <see cref="Array"></see>, starting at a particular <see cref="Array"></see> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="Array"></see> that is the destination of the elements copied from <see cref="ICollection{T}"></see>. The <see cref="Array"></see> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        public void CopyTo(T[] array, int arrayIndex) => ((IList<T>)actualArray).CopyTo(array, arrayIndex);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <param name="comparer">The comparer.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(object? other, IEqualityComparer comparer) => ((IStructuralEquatable)actualArray).Equals(other, comparer);

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<T> GetEnumerator() => ((IList<T>)actualArray).GetEnumerator();

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <param name="comparer">The comparer.</param>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public int GetHashCode(IEqualityComparer comparer) => ((IStructuralEquatable)actualArray).GetHashCode(comparer);

        /// <summary>
        /// Determines the index of a specific item in the <see cref="IList{T}"></see>.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="IList{T}"></see>.</param>
        /// <returns>
        /// The index of <paramref name="item">item</paramref> if found in the list; otherwise, -1.
        /// </returns>
        public int IndexOf(T item) => ((IList<T>)actualArray).IndexOf(item);

        /// <summary>
        /// Inserts an item to the <see cref="IList{T}"></see> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <param name="item">The object to insert into the <see cref="IList{T}"></see>.</param>
        public void Insert(int index, T item) => ((IList<T>)actualArray).Insert(index, item);

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="ICollection{T}"></see>.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="ICollection{T}"></see>.</param>
        /// <returns>
        /// true if <paramref name="item">item</paramref> was successfully removed from the <see cref="ICollection{T}"></see>; otherwise, false. This method also returns false if <paramref name="item">item</paramref> is not found in the original <see cref="ICollection{T}"></see>.
        /// </returns>
        public bool Remove(T item) => ((IList<T>)actualArray).Remove(item);

        /// <summary>
        /// Removes the <see cref="IList{T}"></see> item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        public void RemoveAt(int index) => ((IList<T>)actualArray).RemoveAt(index);

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator"></see> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator() => ((IList<T>)actualArray).GetEnumerator();
    }

    //Dispose part
    public sealed partial class DisposableArray<T> : IDisposable
    {
        private bool disposedValue = false;

        #region IDisposable Support

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //
                }
                //actualArray = null;
                disposedValue = true;
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="DisposableArray{T}"/> class.
        /// </summary>
        ~DisposableArray()
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
