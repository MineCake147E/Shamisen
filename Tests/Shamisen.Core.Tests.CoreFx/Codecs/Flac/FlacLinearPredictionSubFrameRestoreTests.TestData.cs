using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;

using NUnit.Framework;

using Shamisen.Codecs.Flac;
using Shamisen.Codecs.Flac.SubFrames;

namespace Shamisen.Core.Tests.CoreFx.Codecs.Flac
{
    public partial class FlacLinearPredictionSubFrameRestoreTests
    {
        private sealed class TestData
        {
            public int Order { get; init; }
            public int ShiftsNeeded { get; init; }
            public int BitsPerSample { get; init; }
            public int QuantizedPrecision { get; init; }
            public int MultiplyBitsRequired => BitsPerSample + QuantizedPrecision;
            public int AccumulatorBitsRequired => MultiplyBitsRequired + BitOperations.Log2((uint)Order);
            public int[] Coefficients { get; init; }
            public int[] Residuals { get; init; }
            public int[] WarmupSignal { get; init; }
            public int[] ExpectedOutput { get; init; }

            public TestData(int order, int shiftsNeeded, int bitsPerSample, int quantizedPrecision, int outputLength)
            {
                Order = order;
                ShiftsNeeded = shiftsNeeded;
                BitsPerSample = bitsPerSample;
                QuantizedPrecision = quantizedPrecision;
                Coefficients = new int[32];
                Residuals = new int[outputLength];
                WarmupSignal = new int[order];
                ExpectedOutput = new int[outputLength + order];
                var coefficients = Coefficients.AsSpan().Slice(0, order);
                GenerateCoefficients(quantizedPrecision, TestContext.CurrentContext.Random, coefficients);
                GenerateCoefficients(bitsPerSample, TestContext.CurrentContext.Random, WarmupSignal);
                GenerateResidualsAndExpectedOutput(shiftsNeeded, bitsPerSample, quantizedPrecision, TestContext.CurrentContext.Random, Residuals, ExpectedOutput, coefficients, WarmupSignal);
            }

            public FlacLinearPredictionSubFrame.RestoreParameters GenerateParameters(Span<int> outputBuffer)
                => new(new((byte)Order, (byte)MultiplyBitsRequired, (byte)AccumulatorBitsRequired), ShiftsNeeded, Coefficients, outputBuffer);

            private static void GenerateCoefficients(int quantizedPrecision, Random random, Span<int> coefficients)
            {
                random.NextBytes(MemoryMarshal.AsBytes(coefficients));
                if (quantizedPrecision == 32) return;
                FlacUtils.ShiftRightArithmetic(coefficients, 32 - quantizedPrecision);
            }

            private static void GenerateResidualsAndExpectedOutput(int shiftsNeeded, int bitsPerSample, int quantizedPrecision, Random random, Span<int> residuals, Span<int> expectedOutput, ReadOnlySpan<int> coefficients, ReadOnlySpan<int> warmupSignal)
            {
                random.NextBytes(MemoryMarshal.AsBytes(residuals));
                ReshapeResidualDistribution(bitsPerSample, residuals, random);
                warmupSignal.CopyTo(expectedOutput);
                residuals.CopyTo(expectedOutput.Slice(warmupSignal.Length));
                GenerateExpectedOutput(shiftsNeeded, bitsPerSample, coefficients, residuals, expectedOutput);
            }

            private static void ReshapeResidualDistribution(int bitsPerSample, Span<int> residuals, Random random)
            {
                int minShift = 32 - bitsPerSample;
                for (int i = 0; i < residuals.Length; i++)
                {
                    long f = residuals[i];
                    f >>= minShift + random.Next(bitsPerSample + 1);
                    Debug.Assert((int)f == f);
                    residuals[i] = (int)f;
                }
            }

            private static void GenerateExpectedOutput(int shiftsNeeded, int bitsPerSample, ReadOnlySpan<int> coefficients, Span<int> residuals, Span<int> expectedOutput)
            {
                int order = coefficients.Length;
                var output = expectedOutput.Slice(order);
                long minValue = ~0L << (bitsPerSample - 1);
                long maxValue = ~minValue;
                for (int i = 0; i < output.Length; i++)
                {
                    long sum = 0;
                    for (int j = 0; j < order; j++)
                    {
                        sum += (long)expectedOutput[i + order - j - 1] * coefficients[j];
                    }
                    sum >>= shiftsNeeded;
                    var residual = residuals[i];
                    var lsum = sum + residual;
                    var csum = long.Clamp(lsum, minValue, maxValue);
                    if (lsum != csum)
                    {
                        // Overflow should be corrected before being visible from output, by adjusting residual.
                        var newResidual = csum - sum;
                        Debug.Assert(newResidual == (int)newResidual);
                        residual = (int)newResidual;
                        residuals[i] = residual;
                    }
                    sum = csum;
                    output[i] = (int)sum;
                }
            }
        }
    }
}
