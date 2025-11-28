using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Audio.OpenAL;

namespace Shamisen.IO.OpenTK.OpenAL
{
    /// <summary>
    /// Represents a handle that exits the current OpenAL context when disposed.
    /// 
    /// </summary>
    public ref struct ThreadLocalCurrentContextHandle
    {
        private readonly ALCContext context;
        private bool disposedValue;
        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadLocalCurrentContextHandle"/> struct.
        /// </summary>
        /// <param name="context">The context to manage.</param>
        internal ThreadLocalCurrentContextHandle(ALCContext context)
        {
            this.context = context;
            disposedValue = context == ALCContext.Null;
        }

        /// <summary>
        /// Gets a value indicating whether the handle is valid.
        /// </summary>
        public readonly bool IsValid => context != ALCContext.Null;

        /// <summary>
        /// Gets an invalid handle.
        /// </summary>
        public static ThreadLocalCurrentContextHandle Invalid => new(ALCContext.Null);
        /// <summary>
        /// Exits the current context.
        /// </summary>
        public void Dispose()
        {
            if (disposedValue) return;
            OpenALContextManager.ExitContext(context);
            disposedValue = true;
        }
    }
}
