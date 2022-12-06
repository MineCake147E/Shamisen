using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.TestUtils
{
    public readonly struct ErrorEvaluationResult<T> where T : unmanaged
    {
        public ErrorEvaluationResult(ErrorDetails<T> maxUlpError, ulong zeroUlpErrorValues, ReadOnlyMemory<ulong> nonzeroUlpErrorValues, double averageUlpError, ulong totalUlpError, ulong valuesTested)
        {
            MaxUlpError = maxUlpError;
            ZeroUlpErrorValues = zeroUlpErrorValues;
            NonzeroUlpErrorValues = nonzeroUlpErrorValues;
            AverageUlpError = averageUlpError;
            TotalUlpError = totalUlpError;
            ValuesTested = valuesTested;
        }

        public ErrorDetails<T> MaxUlpError { get; }

        public ulong ZeroUlpErrorValues { get; }

        public ReadOnlyMemory<ulong> NonzeroUlpErrorValues { get; }

        public double AverageUlpError { get; }

        public ulong TotalUlpError { get; }
        public ulong ValuesTested { get; }
    }

    public record struct ErrorDetails<T>(T ParameterAt, long UlpDifference, T AbsoluteDifference) where T : unmanaged
    {
        public static implicit operator (T parameterAt, long ulpDifference, T absoluteDifference)(ErrorDetails<T> value) => (value.ParameterAt, value.UlpDifference, value.AbsoluteDifference);
        public static implicit operator ErrorDetails<T>((T parameterAt, long ulpDifference, T absoluteDifference) value) => new(value.parameterAt, value.ulpDifference, value.absoluteDifference);
    }
}
