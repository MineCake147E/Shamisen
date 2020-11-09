using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace MonoAudio
{
    /// <summary>
    /// Provides some functions that supports implementing some functions.
    /// </summary>
    internal static class InternalExtensions
    {
#pragma warning disable IDE0060 // 未使用のパラメーターを削除します

        /// <summary>
        /// Throws if the specified <paramref name="instance"/> is disposed.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance">The instance of <typeparamref name="T"/>.</param>
        /// <param name="isDisposed">if set to <c>true</c> it throws.</param>
        /// <exception cref="ObjectDisposedException">This instance of <typeparamref name="T"/> is disposed!</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerNonUserCode]
        internal static void ThrowIfDisposed<T>(this T instance, bool isDisposed) where T : IDisposable
        {
            if (isDisposed) throw new ObjectDisposedException(typeof(T).Name);
        }

#pragma warning restore IDE0060 // 未使用のパラメーターを削除します
    }
}
