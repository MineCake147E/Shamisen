using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;

using NUnit.Framework;

using Shamisen.Codecs.Flac.SubFrames;

namespace Shamisen.Core.Tests.CoreFx.Codecs.Flac
{
    [TestFixture]
    public partial class FlacLinearPredictionSubFrameRestoreTests
    {
        private static IReadOnlyList<int> OutputLengthSource => [16383];
        private static IReadOnlyList<int> ShiftsNeededSource => [];
        private static IReadOnlyList<int> BitsPerSampleSource => [8, 16, 24, 32];
        private static IReadOnlyList<int> QuantizedPrecisionSource => [1, 4, 16];
        private static IEnumerable<TestCaseData> RestoreSignalTestCaseSource()
        {
            for (int order = 1; order < 33; order++)
            {
                foreach (var bitsPerSample in BitsPerSampleSource)
                {
                    var maxPrecisionBeforeOverflow32 = 32 - (bitsPerSample + BitOperations.Log2((uint)order));
                    foreach (var quantizedPrecision in QuantizedPrecisionSource.Concat([maxPrecisionBeforeOverflow32]).Where(a => a > 0 && a < 32).Distinct())
                    {
                        var accumulatorBitsNeeded = bitsPerSample + quantizedPrecision + BitOperations.Log2((uint)order);
                        var minShift = accumulatorBitsNeeded - 32;
                        foreach (var shiftsNeeded in ShiftsNeededSource.Concat([accumulatorBitsNeeded - 1, quantizedPrecision, minShift]).Where(a => a >= 0 && a < accumulatorBitsNeeded && a >= minShift).Distinct())
                        {
                            foreach (var outputLength in OutputLengthSource)
                            {
                                yield return new(order, shiftsNeeded, bitsPerSample, quantizedPrecision, outputLength);
                            }
                        }
                    }
                }
            }
        }

        private static IEnumerable<TestCaseData> RestoreSignalNativeTestCaseSource()
        {
            for (int order = 1; order < 33; order++)
            {
                foreach (var bitsPerSample in BitsPerSampleSource)
                {
                    var maxPrecisionBeforeOverflow32 = 32 - (bitsPerSample + BitOperations.Log2((uint)order));
                    foreach (var quantizedPrecision in QuantizedPrecisionSource.Concat([maxPrecisionBeforeOverflow32]).Where(a => a > 0 && a < 32).Distinct())
                    {
                        var accumulatorBitsNeeded = bitsPerSample + quantizedPrecision + BitOperations.Log2((uint)order);
                        if (accumulatorBitsNeeded > Unsafe.SizeOf<nint>() * 8) continue;
                        var minShift = accumulatorBitsNeeded - 32;
                        foreach (var shiftsNeeded in ShiftsNeededSource.Concat([accumulatorBitsNeeded - 1, quantizedPrecision, minShift]).Where(a => a >= 0 && a < accumulatorBitsNeeded && a >= minShift).Distinct())
                        {
                            foreach (var outputLength in OutputLengthSource)
                            {
                                yield return new(order, shiftsNeeded, bitsPerSample, quantizedPrecision, outputLength);
                            }
                        }
                    }
                }
            }
        }

        [TestCaseSource(nameof(RestoreSignalNativeTestCaseSource))]
        public void RestoreSignalNativeStandardRestoresCorrectly(int order, int shiftsNeeded, int bitsPerSample, int quantizedPrecision, int outputLength)
        {
            if (BitOperations.Log2((uint)order) + bitsPerSample + quantizedPrecision > Unsafe.SizeOf<nuint>() * 8)
            {
                Assert.Ignore("The accumulator will overflow!");
            }
            var data = new TestData(order, shiftsNeeded, bitsPerSample, quantizedPrecision, outputLength);
            var outputBuffer = new int[outputLength + order];
            data.WarmupSignal.CopyTo(outputBuffer);
            data.Residuals.CopyTo(outputBuffer.AsSpan(data.Order));
            FlacLinearPredictionSubFrame.RestoreSignalNativeStandard(data.GenerateParameters(outputBuffer));
            Assert.That(outputBuffer, Is.EqualTo(data.ExpectedOutput));
        }

        [TestCaseSource(nameof(RestoreSignalTestCaseSource))]
        public void RestoreSignal64StandardRestoresCorrectly(int order, int shiftsNeeded, int bitsPerSample, int quantizedPrecision, int outputLength)
        {
            var data = new TestData(order, shiftsNeeded, bitsPerSample, quantizedPrecision, outputLength);
            var outputBuffer = new int[outputLength + order];
            data.WarmupSignal.CopyTo(outputBuffer);
            data.Residuals.CopyTo(outputBuffer.AsSpan(data.Order));
            FlacLinearPredictionSubFrame.RestoreSignal64Standard(data.GenerateParameters(outputBuffer));
            Assert.That(outputBuffer, Is.EqualTo(data.ExpectedOutput));
        }
    }
}
