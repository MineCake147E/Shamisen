using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Utils
{
    /// <summary>
    /// Prevents the stored <see cref="Value"/> from being read after the <see cref="Value"/> has been read for the first time.
    /// </summary>
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public ref struct ReadOnceObjectContainer<T>
    {
        private T? value;

        /// <summary>
        /// Initializes a new instance of <see cref="ReadOnceObjectContainer{T}"/>.
        /// </summary>
        /// <param name="value">The value to store.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public ReadOnceObjectContainer(T? value)
        {
            this.value = value;
        }

        /// <summary>
        /// Gets the contained value.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public T? Value 
        { 
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get
            {
                var v = value;
                value = default;    //Forget the stored value.
                return v;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        private string GetDebuggerDisplay() => value?.ToString() ?? "null";
    }
}
