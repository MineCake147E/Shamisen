using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MonoAudio.Data
{
#pragma warning disable S3881 // "IDisposable" should be implemented correctly

    /// <summary>
    /// Abstractions for the delegates below:<br/>
    /// <see cref="WriteAction{TSample}"/>,
    /// <see cref="WriteAsyncFunc{TSample}"/>,
    /// <see cref="WriteWithParameterAction{TSample, TParam}"/>, and
    /// <see cref="WriteWithParameterAsyncFunc{TSample, TParam}"/>
    /// </summary>
    /// <typeparam name="TSample">The type of Sample.</typeparam>
    public abstract class DataWriter<TSample> : IDisposable
#pragma warning restore S3881 // "IDisposable" should be implemented correctly
    {
        private bool disposedValue = false;

        /// <summary>
        /// Writes the data asynchronously.
        /// </summary>
        /// <param name="data">The data to write.</param>
        /// <returns></returns>
        public abstract Task WriteAsync(ReadOnlyMemory<TSample> data);

        #region IDisposable Support

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected abstract void DisposeInternal(bool disposing);

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                DisposeInternal(disposing);
                disposedValue = true;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }

    /// <summary>
    /// The data writer to the <see cref="WriteAction{TSample}"/>
    /// </summary>
    /// <typeparam name="TSample">The type of Sample.</typeparam>
    /// <seealso cref="DataWriter{TSample}" />
    public sealed class DataWriterOrdinal<TSample> : DataWriter<TSample>
    {
        private WriteAction<TSample> func;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataWriterOrdinal{TSample}"/> class.
        /// </summary>
        /// <param name="func">The function to call.</param>
        /// <exception cref="ArgumentNullException">func</exception>
        public DataWriterOrdinal(WriteAction<TSample> func) => this.func = func ??
            throw new ArgumentNullException(nameof(func));

#pragma warning disable CS1998 // 非同期メソッドは、'await' 演算子がないため、同期的に実行されます

        /// <summary>
        /// Writes the data asynchronously.
        /// </summary>
        /// <param name="data">The data to write.</param>
        public override async Task WriteAsync(ReadOnlyMemory<TSample> data) => func(data.Span);

#pragma warning restore CS1998 // 非同期メソッドは、'await' 演算子がないため、同期的に実行されます

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void DisposeInternal(bool disposing) => func = null;
    }

    /// <summary>
    /// The data writer to the <see cref="WriteAsyncFunc{TSample}"/>
    /// </summary>
    /// <typeparam name="TSample">The type of Sample.</typeparam>
    /// <seealso cref="DataWriter{TSample}" />
    public sealed class AsyncDataWriter<TSample> : DataWriter<TSample>
    {
        private WriteAsyncFunc<TSample> function;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncDataWriter{TSample}"/> class.
        /// </summary>
        /// <param name="function">The function.</param>
        /// <exception cref="ArgumentNullException">function</exception>
        public AsyncDataWriter(WriteAsyncFunc<TSample> function) => this.function = function ??
            throw new ArgumentNullException(nameof(function));

        /// <summary>
        /// Writes the data asynchronously.
        /// </summary>
        /// <param name="data">The data to write.</param>
        public override async Task WriteAsync(ReadOnlyMemory<TSample> data) => await function(data);

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        /// <exception cref="NotImplementedException"></exception>
        protected override void DisposeInternal(bool disposing) => function = null;
    }

    /// <summary>
    /// The data writer to the <see cref="WriteWithParameterAction{TSample, TParam}"/>
    /// </summary>
    /// <typeparam name="TSample">The type of Sample.</typeparam>
    /// <typeparam name="TParam">The type of parameter to call the method with.</typeparam>
    /// <seealso cref="DataWriter{TSample}" />
    public sealed class ParametrizedDataWriter<TSample, TParam> : DataWriter<TSample>
    {
        private WriteWithParameterAction<TSample, TParam> action;
        private TParam parameter;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParametrizedDataWriter{TSample, TParam}"/> class.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="parameter">The parameter.</param>
        /// <exception cref="ArgumentNullException">action</exception>
        public ParametrizedDataWriter(WriteWithParameterAction<TSample, TParam> action, TParam parameter)
        {
            this.action = action ?? throw new ArgumentNullException(nameof(action));
            this.parameter = parameter;
        }

#pragma warning disable CS1998 // 非同期メソッドは、'await' 演算子がないため、同期的に実行されます

        /// <summary>
        /// Writes the data asynchronously.
        /// </summary>
        /// <param name="data">The data to write.</param>
        public override async Task WriteAsync(ReadOnlyMemory<TSample> data) => action(data.Span, parameter);

#pragma warning restore CS1998 // 非同期メソッドは、'await' 演算子がないため、同期的に実行されます

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void DisposeInternal(bool disposing)
        {
            parameter = default;
            action = null;
        }
    }

    /// <summary>
    /// The data writer to the <see cref="WriteWithParameterAction{TSample, TParam}"/>
    /// </summary>
    /// <typeparam name="TSample">The type of Sample.</typeparam>
    /// <typeparam name="TParam">The type of parameter to call the method with.</typeparam>
    /// <seealso cref="DataWriter{TSample}" />
    public sealed class ParametrizedAsyncDataWriter<TSample, TParam> : DataWriter<TSample>
    {
        private WriteWithParameterAsyncFunc<TSample, TParam> function;
        private TParam parameter;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParametrizedAsyncDataWriter{TSample, TParam}"/> class.
        /// </summary>
        /// <param name="function">The function.</param>
        /// <param name="parameter">The parameter.</param>
        /// <exception cref="ArgumentNullException">function</exception>
        public ParametrizedAsyncDataWriter(WriteWithParameterAsyncFunc<TSample, TParam> function, TParam parameter)
        {
            this.function = function ?? throw new ArgumentNullException(nameof(function));
            this.parameter = parameter;
        }

        /// <summary>
        /// Writes the data asynchronously.
        /// </summary>
        /// <param name="data">The data to write.</param>
        public override async Task WriteAsync(ReadOnlyMemory<TSample> data) => await function(data, parameter);

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void DisposeInternal(bool disposing)
        {
            parameter = default;
            function = null;
        }
    }
}
