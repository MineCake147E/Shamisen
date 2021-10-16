using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

using OpenTK.Audio.OpenAL;

namespace Shamisen.IO
{
    /// <summary>
    /// Manages <see cref="ALContext"/> and <see cref="ALC.MakeContextCurrent(ALContext)"/>.
    /// </summary>
    public static partial class OpenALContextManager
    {
        private static HashSet<ALContext> ALContexts { get; } = new();

        private static SemaphoreSlim Semaphore { get; } = new(1);

        /// <summary>
        /// Waits for setting context asynchronously.
        /// </summary>
        /// <param name="context">The context to activate.</param>
        /// <returns></returns>
        public static async ValueTask<IDisposable> WaitForContextAsync(ALContext context)
        {
            bool newContext = !ALContexts.Contains(context);
            await Semaphore.WaitAsync();
            if (newContext)
            {
                _ = ALC.MakeContextCurrent(context);
                _ = ALContexts.Add(context);
            }
            else
            {
                _ = ALC.MakeContextCurrent(context);
                ALC.ProcessContext(context);
            }
            return new CurrentContextHandle(context);
        }

        /// <summary>
        /// Runs <paramref name="action"/> with setting context asynchronously.
        /// </summary>
        /// <param name="context">The context to activate.</param>
        /// <param name="action">The action to run.</param>
        /// <returns></returns>
        public static async ValueTask RunWithContextAsync(ALContext context, Action action)
        {
            using (_ = await WaitForContextAsync(context))
            {
                action?.Invoke();
            }
        }

        internal static void SuspendContext(ALContext context)
        {
            ALC.SuspendContext(context);
            _ = ALC.MakeContextCurrent(ALContext.Null);
            _ = Semaphore.Release();
        }

        private struct CurrentContextHandle : IDisposable
        {
            private bool disposedValue;

            public CurrentContextHandle(ALContext context) : this()
            {
                Context = context;
            }

            private ALContext Context { get; }

            private void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        //
                    }
                    SuspendContext(Context);
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
