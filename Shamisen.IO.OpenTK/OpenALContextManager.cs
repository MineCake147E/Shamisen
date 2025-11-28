using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

using OpenTK.Audio.OpenAL;
using OpenTK.Audio.OpenAL.ALC;

using ALCErrorCode = OpenTK.Audio.OpenAL.ALC.ErrorCode;
using ALCGetPNameIV = OpenTK.Audio.OpenAL.ALC.GetPNameIV;

namespace Shamisen.IO.OpenTK.OpenAL
{
    /// <summary>
    /// Manages <see cref="ALCContext"/> and <see cref="ALC.MakeContextCurrent(ALCContext)"/>.
    /// </summary>
    public static partial class OpenALContextManager
    {
        private static SemaphoreSlim Semaphore { get; } = new(1);

        /// <summary>
        /// Waits for setting context asynchronously.
        /// </summary>
        /// <param name="context">The context to activate.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static async ValueTask<CurrentContextHandle> WaitForContextAsync(ALCContext context, CancellationToken cancellationToken = default)
        {
            await Semaphore.WaitAsync(cancellationToken);
            if (ALC.GetCurrentContext() != context)
            {
                if (!ALC.MakeContextCurrent(context))
                {
                    _ = Semaphore.Release();
                    throw new ArgumentException("Failed to make the specified context current.", nameof(context));
                }
                else
                {
                    ALC.SuspendContext(context);
                }
            }
            return new CurrentContextHandle(context);
        }



        /// <summary>
        /// Runs <paramref name="action"/> with setting context asynchronously.
        /// </summary>
        /// <param name="context">The context to activate.</param>
        /// <param name="action">The action to run.</param>
        /// <returns></returns>
        public static async ValueTask RunWithContextAsync(ALCContext context, Action action)
        {
            using (_ = await WaitForContextAsync(context))
            {
                action?.Invoke();
            }
        }

        internal static void ExitContext(ALCContext context)
        {
            ALC.ProcessContext(context);
            _ = ALC.MakeContextCurrent(ALCContext.Null);
            _ = Semaphore.Release();
        }

        /// <summary>
        /// Queries the number of context attributes of the specified <paramref name="device"/>.
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public static int GetDeviceAttributeCount(ALCDevice device) => (ALC.GetInteger(device, ALCGetPNameIV.AttributesSize) + 1) / 2;

        /// <summary>
        /// Gets all context attributes of the specified <paramref name="device"/>.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="outputValues"></param>
        public static unsafe void GetContextAttributes<TBufferWriter>(ALCDevice device, scoped ref TBufferWriter outputValues) where TBufferWriter : struct, IBufferWriter<AlcContextAttributeKeyValuePair>
        {
            var minimumLength = GetDeviceAttributeCount(device);
            var span = outputValues.GetSpan(minimumLength);
            var intSpan = MemoryMarshal.Cast<AlcContextAttributeKeyValuePair, int>(span);
            ALC.GetInteger(device, ALCGetPNameIV.AllAttributes, intSpan.Length, intSpan);
            outputValues.Advance(minimumLength);
        }

        /// <summary>
        /// Gets all context attributes of the specified <paramref name="device"/>.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="outputValues"></param>
        public static unsafe void GetContextAttributes<TBufferWriter>(ALCDevice device, TBufferWriter outputValues) where TBufferWriter : class, IBufferWriter<AlcContextAttributeKeyValuePair>
        {
            var minimumLength = GetDeviceAttributeCount(device);
            var span = outputValues.GetSpan(minimumLength);
            var intSpan = MemoryMarshal.Cast<AlcContextAttributeKeyValuePair, int>(span);
            ALC.GetInteger(device, ALCGetPNameIV.AllAttributes, intSpan.Length, intSpan);
            outputValues.Advance(minimumLength);
        }

        /// <summary>
        /// Gets all context attributes of the specified <paramref name="device"/>.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="outputValues"></param>
        public static void GetContextAttributes(ALCDevice device, List<AlcContextAttributeKeyValuePair> outputValues)
        {
            var minimumLength = GetDeviceAttributeCount(device);
            var attr = ArrayPool<int>.Shared.Rent(minimumLength * 2);
            _ = outputValues.EnsureCapacity(outputValues.Count + minimumLength);
            ALC.GetInteger(device, ALCGetPNameIV.AllAttributes, attr.Length, attr);
            var sAttr = MemoryMarshal.Cast<int, AlcContextAttributeKeyValuePair>(attr.AsSpan());
            outputValues.AddRange(sAttr);
            ArrayPool<int>.Shared.Return(attr, true);
        }

        /// <summary>
        /// Gets all context attributes of the specified <paramref name="device"/>.
        /// </summary>
        /// <param name="device"></param>
        public static List<AlcContextAttributeKeyValuePair> GetContextAttributes(ALCDevice device)
        {
            var list = new List<AlcContextAttributeKeyValuePair>();
            GetContextAttributes(device, list);
            return list;
        }
    }
}
