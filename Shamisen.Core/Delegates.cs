using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen
{
    #region Read

    /// <summary>
    /// Encapsulates a method that asynchronously fills given <see cref="Memory{T}"/> and returns how many <typeparamref name="TSample"/>s are filled.
    /// </summary>
    /// <typeparam name="TSample">The type of Sample.</typeparam>
    /// <param name="buffer">The destination buffer.</param>
    /// <returns>The reading task that returns the length of buffer filled.</returns>
    public delegate Task<int> ReadAsyncFunc<TSample>(Memory<TSample> buffer);

    /// <summary>
    /// Encapsulates a method that fills given <see cref="Span{T}"/> and returns how many <typeparamref name="TSample"/>s are filled.
    /// </summary>
    /// <typeparam name="TSample">The type of Sample.</typeparam>
    /// <param name="buffer">The destination buffer.</param>
    /// <returns>The length of buffer filled.</returns>
    public delegate int ReadFunc<TSample>(Span<TSample> buffer);

    /// <summary>
    /// Encapsulates a method that asynchronously fills given <see cref="Memory{T}"/> and returns how many <typeparamref name="TSample"/>s are filled.
    /// </summary>
    /// <typeparam name="TSample">The type of Sample.</typeparam>
    /// <typeparam name="TParam">The type of parameter to call the method with.</typeparam>
    /// <param name="buffer">The destination buffer.</param>
    /// <param name="parameter">The parameter to call the method with.</param>
    /// <returns>The reading task that returns the length of buffer filled.</returns>
    public delegate Task<int> ReadWithParameterAsyncFunc<TSample, TParam>(Memory<TSample> buffer, TParam parameter);

    /// <summary>
    /// Encapsulates a method that fills given <see cref="Span{T}"/> and returns how many <typeparamref name="TSample"/>s are filled.
    /// </summary>
    /// <typeparam name="TSample">The type of Sample.</typeparam>
    /// <typeparam name="TParam">The type of parameter to call the method with.</typeparam>
    /// <param name="buffer">The destination buffer.</param>
    /// <param name="parameter">The parameter to call the method with.</param>
    /// <returns>The length of buffer filled.</returns>
    public delegate int ReadWithParameterFunc<TSample, TParam>(Span<TSample> buffer, TParam parameter);

    #endregion Read

    #region Write

    /// <summary>
    /// Encapsulates a method that writes the given <see cref="ReadOnlySpan{T}"/> into the destination.
    /// </summary>
    /// <typeparam name="TSample">The type of Sample.</typeparam>
    /// <param name="data">The data to write.</param>
    public delegate void WriteAction<TSample>(ReadOnlySpan<TSample> data);

    /// <summary>
    /// Encapsulates a method that asynchronously writes the given <see cref="ReadOnlyMemory{T}"/> into the destination.
    /// </summary>
    /// <typeparam name="TSample">The type of Sample.</typeparam>
    /// <param name="data">The data to write.</param>
    /// <returns>The writing task.</returns>
    public delegate Task WriteAsyncFunc<TSample>(ReadOnlyMemory<TSample> data);

    /// <summary>
    /// Encapsulates a method that writes the given <see cref="ReadOnlySpan{T}"/> into the destination.
    /// </summary>
    /// <typeparam name="TSample">The type of Sample.</typeparam>
    /// <typeparam name="TParam">The type of parameter to call the method with.</typeparam>
    /// <param name="data">The data to write.</param>
    /// <param name="parameter">The parameter to call the method with.</param>
    public delegate void WriteWithParameterAction<TSample, TParam>(ReadOnlySpan<TSample> data, TParam parameter);

    /// <summary>
    /// Encapsulates a method that asynchronously writes the given <see cref="ReadOnlyMemory{T}"/> into the destination.
    /// </summary>
    /// <typeparam name="TSample">The type of Sample.</typeparam>
    /// <typeparam name="TParam">The type of parameter to call the method with.</typeparam>
    /// <param name="data">The data to write.</param>
    /// <param name="parameter">The parameter to call the method with.</param>
    /// <returns>The reading task that returns the length of buffer filled.</returns>
    public delegate Task WriteWithParameterAsyncFunc<TSample, TParam>(ReadOnlyMemory<TSample> data, TParam parameter);

    #endregion Write
}
