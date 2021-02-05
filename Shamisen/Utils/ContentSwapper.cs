using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Shamisen.Utils
{
    /// <summary>
    /// Swaps two <typeparamref name="T"/> instances.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class ContentSwapper<T>
    {
        private T valueA, valueB;
        private bool isBActive = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentSwapper{T}"/> class.
        /// </summary>
        /// <param name="initialVisibleValue">The value initial visible.</param>
        /// <param name="initiallyHiddenValue">The value initially hidden.</param>
        public ContentSwapper(T initialVisibleValue, T initiallyHiddenValue)
        {
            valueA = initialVisibleValue;
            valueB = initiallyHiddenValue;
        }

        /// <summary>
        /// Swaps two values of this instance.
        /// </summary>
        public void Swap() => isBActive = !isBActive;

        /// <summary>
        /// Gets the current value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public ref T Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref isBActive ? ref valueB : ref valueA;
        }
    }
}
