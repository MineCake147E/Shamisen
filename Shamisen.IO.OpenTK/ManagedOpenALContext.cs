using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Audio.OpenAL;
using OpenTK.Audio.OpenAL.ALC;

namespace Shamisen.IO.OpenTK.OpenAL
{
    /// <summary>
    /// Represents and manages an OpenAL context.
    /// </summary>
    public sealed class ManagedOpenALContext : IDisposable
    {
        private ALCContext context;
        private readonly bool isThreadLocalContextSupported;
        private readonly bool isDirectContextExtensionSupported;

        private FrozenDictionary<string, bool> commonContextExtensions;
        private Dictionary<string, bool> extensionQueryCache;

        /// <summary>
        /// Gets the underlying <see cref="ALCContext"/> for uses in Extension APIs for `AL_EXT_direct_context` and `ALC_EXT_direct_context`.
        /// </summary>
        public unsafe ALCContext Context => context;

        internal ManagedOpenALContext(ALCContext context)
        {
            this.context = context;
            isThreadLocalContextSupported = ALC.IsExtensionPresent(ALCDevice.Null, "ALC_EXT_thread_local_context");
            var alcDirectSupported = ALC.IsExtensionPresent(ALCDevice.Null, "ALC_EXT_direct_context");
            if (alcDirectSupported)
            {
                OpenALContextManager.wa
            }
        }

        public ThreadLocalCurrentContextHandle TrySetThreadLocalContext()
        {

        }

        /// <summary>
        /// Waits until the context is set to current and returns a handle that exits the context when disposed.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A new handle that exits the context when disposed</returns>
        public async ValueTask<CurrentContextHandle> WaitForContextAsync(CancellationToken cancellationToken = default)
            => await OpenALContextManager.WaitForContextAsync(context, cancellationToken).ConfigureAwait(false);

        private void Dispose(bool disposing)
        {
            var currentContext = (ALCContext)Interlocked.Exchange(ref Unsafe.As<ALCContext, nint>(ref context), ALCContext.Null);
            if (currentContext != ALCContext.Null)
            {
                ALC.DestroyContext(currentContext);
                Debug.WriteLine($"Disposed {currentContext} with {nameof(disposing)}: {disposing}");
            }
        }

        /// <inheritdoc/>
        ~ManagedOpenALContext()
        {
            Dispose(disposing: false);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
