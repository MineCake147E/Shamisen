using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Shamisen
{
    /// <summary>
    /// Stores <see cref="Action{T}"/> that cannot leave the stack.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public readonly ref struct StackOnlyActionContainer<T>
    {
        private readonly Action<T> value;

        /// <summary>
        /// Initializes a new instance of the <see cref="StackOnlyActionContainer{T}"/> struct.
        /// </summary>
        /// <param name="action">The action.</param>
        public StackOnlyActionContainer(Action<T> action)
        {
            value = action;
        }

        /// <summary>
        /// Invokes this instance with specified parameter.
        /// </summary>
        /// <param name="parameter">The first parameter.</param>
        public void Invoke(T parameter) => value(parameter);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the obj parameter; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object? obj) => false;

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="value">An object to compare with this object.</param>
        /// <returns>
        ///   <c>true</c> if the current object is equal to the obj parameter; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(StackOnlyActionContainer<T> value) => value.value == this.value;

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode() => -1584136870 + EqualityComparer<Action<T>>.Default.GetHashCode(value);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="StackOnlyActionContainer{T}"/> objects are equal.
        /// </summary>
        /// <param name="left">The first <see cref="StackOnlyActionContainer{T}"/> to compare.</param>
        /// <param name="right">The second <see cref="StackOnlyActionContainer{T}"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if the left is the same as the right; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator ==(StackOnlyActionContainer<T> left, StackOnlyActionContainer<T> right) => left.Equals(right);

        /// <summary>
        /// Indicates whether the values of two specified <see cref="StackOnlyActionContainer{T}"/> objects are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="StackOnlyActionContainer{T}"/> to compare.</param>
        /// <param name="right">The second  <see cref="StackOnlyActionContainer{T}"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if left and right are not equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator !=(StackOnlyActionContainer<T> left, StackOnlyActionContainer<T> right) => !(left == right);

        private string GetDebuggerDisplay() => value.ToString() ?? "null";
    }
}
