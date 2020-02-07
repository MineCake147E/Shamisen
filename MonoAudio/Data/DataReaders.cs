using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MonoAudio.Data;

namespace MonoAudio.Data
{
    /// <summary>
    /// Abstractions for the delegates below:<br/>
    /// <see cref="ReadFunc{TSample}"/> and
    /// <see cref="ReadWithParameterFunc{TSample, TParam}"/>
    /// </summary>
    /// <typeparam name="TSample">The type of the sample.</typeparam>
    public interface ISynchronizedDataReader<TSample>
    {
        /// <summary>
        /// Reads the data to the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>The length of the data written.</returns>
        ReadResult Read(Span<TSample> buffer);
    }

    /// <summary>
    /// The data reader to the <see cref="ReadFunc{TSample}"/>
    /// </summary>
    /// <typeparam name="TSample">The type of Sample.</typeparam>
    /// <seealso cref="DataReader{TSample}" />
    public sealed class DataReaderOrdinal<TSample> : DataReader<TSample>, ISynchronizedDataReader<TSample>
    {
        private ReadFunc<TSample> func;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataReaderOrdinal{TSample}"/> class.
        /// </summary>
        /// <param name="func">The function to call.</param>
        /// <exception cref="ArgumentNullException">func</exception>
        public DataReaderOrdinal(ReadFunc<TSample> func) => this.func = func ??
        throw new ArgumentNullException(nameof(func));

        /// <summary>
        /// Reads the data to the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>
        /// The length of the data written.
        /// </returns>
        public ReadResult Read(Span<TSample> buffer) => func(buffer);

#pragma warning disable CS1998 // 非同期メソッドは、'await' 演算子がないため、同期的に実行されます

        /// <summary>
        /// Reads the data asynchronously.
        /// </summary>
        /// <param name="buffer">The buffer to fill with the read data.</param>
        public override async Task<int> ReadAsync(Memory<TSample> buffer) => func(buffer.Span);

#pragma warning restore CS1998 // 非同期メソッドは、'await' 演算子がないため、同期的に実行されます

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void DisposeInternal(bool disposing) => func = null;
    }

    /// <summary>
    /// The data reader to the <see cref="ReadAsyncFunc{TSample}"/>
    /// </summary>
    /// <typeparam name="TSample">The type of Sample.</typeparam>
    /// <seealso cref="DataReader{TSample}" />
    public sealed class AsyncDataReader<TSample> : DataReader<TSample>
    {
        private ReadAsyncFunc<TSample> function;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncDataReader{TSample}"/> class.
        /// </summary>
        /// <param name="function">The function.</param>
        /// <exception cref="ArgumentNullException">function</exception>
        public AsyncDataReader(ReadAsyncFunc<TSample> function) => this.function = function ??
        throw new ArgumentNullException(nameof(function));

        /// <summary>
        /// Reads the data asynchronously.
        /// </summary>
        /// <param name="buffer">The buffer to fill with the read data.</param>
        public override async Task<int> ReadAsync(Memory<TSample> buffer) => await function(buffer);

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        /// <exception cref="NotImplementedException"></exception>
        protected override void DisposeInternal(bool disposing) => function = null;
    }

    /// <summary>
    /// The data reader to the <see cref="ReadWithParameterFunc{TSample, TParam}"/>
    /// </summary>
    /// <typeparam name="TSample">The type of Sample.</typeparam>
    /// <typeparam name="TParam">The type of parameter to call the method with.</typeparam>
    /// <seealso cref="DataReader{TSample}" />
    public sealed class ParametrizedDataReader<TSample, TParam> : DataReader<TSample>, ISynchronizedDataReader<TSample>
    {
        private ReadWithParameterFunc<TSample, TParam> action;
        private TParam parameter;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParametrizedDataReader{TSample, TParam}"/> class.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="parameter">The parameter.</param>
        /// <exception cref="ArgumentNullException">action</exception>
        public ParametrizedDataReader(ReadWithParameterFunc<TSample, TParam> action, TParam parameter)
        {
            this.action = action ??
                throw new ArgumentNullException(nameof(action));
            this.parameter = parameter;
        }

        /// <summary>
        /// Reads the data to the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>
        /// The length of the data written.
        /// </returns>
        public ReadResult Read(Span<TSample> buffer) => action(buffer, parameter);

#pragma warning disable CS1998 // 非同期メソッドは、'await' 演算子がないため、同期的に実行されます

        /// <summary>
        /// Reads the data asynchronously.
        /// </summary>
        /// <param name="buffer">The buffer to fill with the read data.</param>
        public override async Task<int> ReadAsync(Memory<TSample> buffer) => action(buffer.Span, parameter);

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
    /// The data reader to the <see cref="ReadWithParameterAsyncFunc{TSample, TParam}"/>
    /// </summary>
    /// <typeparam name="TSample">The type of Sample.</typeparam>
    /// <typeparam name="TParam">The type of parameter to call the method with.</typeparam>
    /// <seealso cref="DataReader{TSample}" />
    public sealed class ParametrizedAsyncDataReader<TSample, TParam> : DataReader<TSample>
    {
        private ReadWithParameterAsyncFunc<TSample, TParam> function;
        private TParam parameter;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParametrizedAsyncDataReader{TSample, TParam}"/> class.
        /// </summary>
        /// <param name="function">The function.</param>
        /// <param name="parameter">The parameter.</param>
        /// <exception cref="ArgumentNullException">function</exception>
        public ParametrizedAsyncDataReader(ReadWithParameterAsyncFunc<TSample, TParam> function, TParam parameter)
        {
            this.function = function ??
                throw new ArgumentNullException(nameof(function));
            this.parameter = parameter;
        }

        /// <summary>
        /// Reads the data asynchronously.
        /// </summary>
        /// <param name="buffer">The buffer to fill with the read data.</param>
        public override async Task<int> ReadAsync(Memory<TSample> buffer) => await function(buffer, parameter);

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
