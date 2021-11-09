using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Silk.NET.OpenAL;

using ALCApi = Silk.NET.OpenAL.ALContext;
using ALContext = Silk.NET.OpenAL.Context;
using OpenALApi = Silk.NET.OpenAL.AL;

namespace Shamisen.IO
{
    /// <summary>
    /// Manages <see cref="ALContext"/> and <see cref="ALCApi.MakeContextCurrent(ALContext*)"/>.
    /// </summary>
    public sealed partial class OpenALContextManager : SilkOpenALObjectBase
    {
        private bool disposedValue;
        /// <summary>
        /// Initializes a new instance of <see cref="OpenALContextManager"/>.
        /// </summary>
        /// <inheritdoc/>
        public OpenALContextManager(bool useOpenALSoft) : base(useOpenALSoft)
        {
        }

        private HashSet<nint> ALContexts { get; } = new();

        private SemaphoreSlim Semaphore { get; } = new(1);

        /// <summary>
        /// Waits for setting context asynchronously.
        /// </summary>
        /// <param name="context">The context to activate.</param>
        /// <returns></returns>
        public async ValueTask<IDisposable> WaitForContextAsync(nint context)
        {
            var newContext = !ALContexts.Contains(context);
            await Semaphore.WaitAsync();
            if (newContext)
            {
                _ = ALC.MakeContextCurrent(context);
                _ = ALContexts.Add(context);
            }
            else
            {
                _ = ALC.MakeContextCurrent(context);
                unsafe
                {
                    ALC.ProcessContext((ALContext*)context);
                }
            }
            return new CurrentContextHandle(context, this);
        }

        /// <summary>
        /// Runs <paramref name="action"/> with setting context asynchronously.
        /// </summary>
        /// <param name="context">The context to activate.</param>
        /// <param name="action">The action to run.</param>
        /// <returns></returns>
        public async ValueTask RunWithContextAsync(nint context, Action action)
        {
            using (_ = await WaitForContextAsync(context))
            {
                action?.Invoke();
            }
        }

        internal void SuspendContext(nint context)
        {
            unsafe
            {
                ALC.SuspendContext((ALContext*)context);
                _ = ALC.MakeContextCurrent(null);
            }
            _ = Semaphore.Release();
        }

        /// <inheritdoc/>
        protected override void ActualDispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //
                }
                Semaphore.Dispose();
                foreach (var item in ALContexts)
                {
                    unsafe
                    {
                        ALC.DestroyContext((ALContext*)item);
                    }
                }
                ALContexts.Clear();
                disposedValue = true;
            }
        }

        private struct CurrentContextHandle : IDisposable
        {
            private bool disposedValue;

            public CurrentContextHandle(nint context, OpenALContextManager contextManager) : this()
            {
                Context = context;
                ContextManager = contextManager;
            }

            private nint Context { get; }

            private OpenALContextManager ContextManager { get; }

            private void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        //
                    }
                    ContextManager.SuspendContext(Context);
                    disposedValue = true;
                }
            }

            public void Dispose()
            {
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
        }
    }
}
