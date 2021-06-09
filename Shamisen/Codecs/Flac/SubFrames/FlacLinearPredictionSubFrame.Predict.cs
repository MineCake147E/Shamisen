using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Shamisen.Codecs.Flac.Parsing;

namespace Shamisen.Codecs.Flac.SubFrames
{
    public sealed partial class FlacLinearPredictionSubFrame
    {
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalStandard(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            var order = coeffs.Length;
            switch (order)
            {
                case 1:
                    RestoreSignalOrder1(shiftsNeeded, residual, coeffs, output);
                    return;
                case 2:
                    RestoreSignalOrder2Standard(shiftsNeeded, residual, coeffs, output);
                    return;
                case 3:
                    RestoreSignalOrder3(shiftsNeeded, residual, coeffs, output);
                    return;
                case 4:
                    RestoreSignalOrder4(shiftsNeeded, residual, coeffs, output);
                    return;
                case 5:
                    RestoreSignalOrder5(shiftsNeeded, residual, coeffs, output);
                    return;
                case 6:
                    RestoreSignalOrder6(shiftsNeeded, residual, coeffs, output);
                    return;
                case 7:
                    RestoreSignalOrder7(shiftsNeeded, residual, coeffs, output);
                    return;
                case 8:
                    RestoreSignalOrder8(shiftsNeeded, residual, coeffs, output);
                    return;
                case 9:
                    RestoreSignalOrder9(shiftsNeeded, residual, coeffs, output);
                    return;
                case 10:
                    RestoreSignalOrder10(shiftsNeeded, residual, coeffs, output);
                    return;
                case 11:
                    RestoreSignalOrder11(shiftsNeeded, residual, coeffs, output);
                    return;
                case 12:
                    RestoreSignalOrder12(shiftsNeeded, residual, coeffs, output);
                    return;
                case 13:
                    RestoreSignalOrder13(shiftsNeeded, residual, coeffs, output);
                    return;
                case 14:
                    RestoreSignalOrder14(shiftsNeeded, residual, coeffs, output);
                    return;
                case 15:
                    RestoreSignalOrder15(shiftsNeeded, residual, coeffs, output);
                    return;
                case 16:
                    RestoreSignalOrder16(shiftsNeeded, residual, coeffs, output);
                    return;
                case 17:
                    RestoreSignalOrder17(shiftsNeeded, residual, coeffs, output);
                    return;
                case 18:
                    RestoreSignalOrder18(shiftsNeeded, residual, coeffs, output);
                    return;
                case 19:
                    RestoreSignalOrder19(shiftsNeeded, residual, coeffs, output);
                    return;
                case 20:
                    RestoreSignalOrder20(shiftsNeeded, residual, coeffs, output);
                    return;
                case 21:
                    RestoreSignalOrder21(shiftsNeeded, residual, coeffs, output);
                    return;
                case 22:
                    RestoreSignalOrder22(shiftsNeeded, residual, coeffs, output);
                    return;
                case 23:
                    RestoreSignalOrder23(shiftsNeeded, residual, coeffs, output);
                    return;
                case 24:
                    RestoreSignalOrder24(shiftsNeeded, residual, coeffs, output);
                    return;
                case 25:
                    RestoreSignalOrder25(shiftsNeeded, residual, coeffs, output);
                    return;
                case 26:
                    RestoreSignalOrder26(shiftsNeeded, residual, coeffs, output);
                    return;
                case 27:
                    RestoreSignalOrder27(shiftsNeeded, residual, coeffs, output);
                    return;
                case 28:
                    RestoreSignalOrder28(shiftsNeeded, residual, coeffs, output);
                    return;
                case 29:
                    RestoreSignalOrder29(shiftsNeeded, residual, coeffs, output);
                    return;
                case 30:
                    RestoreSignalOrder30(shiftsNeeded, residual, coeffs, output);
                    return;
                case 31:
                    RestoreSignalOrder31(shiftsNeeded, residual, coeffs, output);
                    return;
                case 32:
                    RestoreSignalOrder32(shiftsNeeded, residual, coeffs, output);
                    return;
                default:
                    throw new FlacException("Invalid FLAC stream!");
            }
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder1(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 1;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
			var prev0 = output[0];
			var coeff0 = coeffs[0];
            int sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeff0 * prev0;
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = sum;
                prev0 = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder2Standard(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 2;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
			var prev0 = output[0];
			var prev1 = output[1];
			var coeff0 = coeffs[0];
			var coeff1 = coeffs[1];
            int sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeff1 * prev0;
				prev0 = prev1;
				sum += coeff0 * prev1;
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = sum;
                prev1 = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder3(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 3;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
			var prev0 = output[0];
			var prev1 = output[1];
			var prev2 = output[2];
			var coeff0 = coeffs[0];
			var coeff1 = coeffs[1];
			var coeff2 = coeffs[2];
            int sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeff2 * prev0;
				prev0 = prev1;
				sum += coeff1 * prev1;
				prev1 = prev2;
				sum += coeff0 * prev2;
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = sum;
                prev2 = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder4(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 4;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
			var prev0 = output[0];
			var prev1 = output[1];
			var prev2 = output[2];
			var prev3 = output[3];
			var coeff0 = coeffs[0];
			var coeff1 = coeffs[1];
			var coeff2 = coeffs[2];
			var coeff3 = coeffs[3];
            int sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeff3 * prev0;
				prev0 = prev1;
				sum += coeff2 * prev1;
				prev1 = prev2;
				sum += coeff1 * prev2;
				prev2 = prev3;
				sum += coeff0 * prev3;
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = sum;
                prev3 = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder5(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 5;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
			var coeff0 = coeffs[0];
			var coeff1 = coeffs[1];
			var coeff2 = coeffs[2];
			var coeff3 = coeffs[3];
			var coeff4 = coeffs[4];
            int sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeff4 * output[i + 0];
				sum += coeff3 * output[i + 1];
				sum += coeff2 * output[i + 2];
				sum += coeff1 * output[i + 3];
				sum += coeff0 * output[i + 4];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder6(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 6;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
			var coeff0 = coeffs[0];
			var coeff1 = coeffs[1];
			var coeff2 = coeffs[2];
			var coeff3 = coeffs[3];
			var coeff4 = coeffs[4];
			var coeff5 = coeffs[5];
            int sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeff5 * output[i + 0];
				sum += coeff4 * output[i + 1];
				sum += coeff3 * output[i + 2];
				sum += coeff2 * output[i + 3];
				sum += coeff1 * output[i + 4];
				sum += coeff0 * output[i + 5];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder7(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 7;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
			var coeff0 = coeffs[0];
			var coeff1 = coeffs[1];
			var coeff2 = coeffs[2];
			var coeff3 = coeffs[3];
			var coeff4 = coeffs[4];
			var coeff5 = coeffs[5];
			var coeff6 = coeffs[6];
            int sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeff6 * output[i + 0];
				sum += coeff5 * output[i + 1];
				sum += coeff4 * output[i + 2];
				sum += coeff3 * output[i + 3];
				sum += coeff2 * output[i + 4];
				sum += coeff1 * output[i + 5];
				sum += coeff0 * output[i + 6];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder8(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 8;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
			var coeff0 = coeffs[0];
			var coeff1 = coeffs[1];
			var coeff2 = coeffs[2];
			var coeff3 = coeffs[3];
			var coeff4 = coeffs[4];
			var coeff5 = coeffs[5];
			var coeff6 = coeffs[6];
			var coeff7 = coeffs[7];
            int sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeff7 * output[i + 0];
				sum += coeff6 * output[i + 1];
				sum += coeff5 * output[i + 2];
				sum += coeff4 * output[i + 3];
				sum += coeff3 * output[i + 4];
				sum += coeff2 * output[i + 5];
				sum += coeff1 * output[i + 6];
				sum += coeff0 * output[i + 7];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder9(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 9;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            int sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[8] * output[i + 0];
				sum += coeffs[7] * output[i + 1];
				sum += coeffs[6] * output[i + 2];
				sum += coeffs[5] * output[i + 3];
				sum += coeffs[4] * output[i + 4];
				sum += coeffs[3] * output[i + 5];
				sum += coeffs[2] * output[i + 6];
				sum += coeffs[1] * output[i + 7];
				sum += coeffs[0] * output[i + 8];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder10(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 10;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            int sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[9] * output[i + 0];
				sum += coeffs[8] * output[i + 1];
				sum += coeffs[7] * output[i + 2];
				sum += coeffs[6] * output[i + 3];
				sum += coeffs[5] * output[i + 4];
				sum += coeffs[4] * output[i + 5];
				sum += coeffs[3] * output[i + 6];
				sum += coeffs[2] * output[i + 7];
				sum += coeffs[1] * output[i + 8];
				sum += coeffs[0] * output[i + 9];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder11(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 11;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            int sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[10] * output[i + 0];
				sum += coeffs[9] * output[i + 1];
				sum += coeffs[8] * output[i + 2];
				sum += coeffs[7] * output[i + 3];
				sum += coeffs[6] * output[i + 4];
				sum += coeffs[5] * output[i + 5];
				sum += coeffs[4] * output[i + 6];
				sum += coeffs[3] * output[i + 7];
				sum += coeffs[2] * output[i + 8];
				sum += coeffs[1] * output[i + 9];
				sum += coeffs[0] * output[i + 10];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder12(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 12;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            int sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[11] * output[i + 0];
				sum += coeffs[10] * output[i + 1];
				sum += coeffs[9] * output[i + 2];
				sum += coeffs[8] * output[i + 3];
				sum += coeffs[7] * output[i + 4];
				sum += coeffs[6] * output[i + 5];
				sum += coeffs[5] * output[i + 6];
				sum += coeffs[4] * output[i + 7];
				sum += coeffs[3] * output[i + 8];
				sum += coeffs[2] * output[i + 9];
				sum += coeffs[1] * output[i + 10];
				sum += coeffs[0] * output[i + 11];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder13(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 13;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            int sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[12] * output[i + 0];
				sum += coeffs[11] * output[i + 1];
				sum += coeffs[10] * output[i + 2];
				sum += coeffs[9] * output[i + 3];
				sum += coeffs[8] * output[i + 4];
				sum += coeffs[7] * output[i + 5];
				sum += coeffs[6] * output[i + 6];
				sum += coeffs[5] * output[i + 7];
				sum += coeffs[4] * output[i + 8];
				sum += coeffs[3] * output[i + 9];
				sum += coeffs[2] * output[i + 10];
				sum += coeffs[1] * output[i + 11];
				sum += coeffs[0] * output[i + 12];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder14(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 14;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            int sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[13] * output[i + 0];
				sum += coeffs[12] * output[i + 1];
				sum += coeffs[11] * output[i + 2];
				sum += coeffs[10] * output[i + 3];
				sum += coeffs[9] * output[i + 4];
				sum += coeffs[8] * output[i + 5];
				sum += coeffs[7] * output[i + 6];
				sum += coeffs[6] * output[i + 7];
				sum += coeffs[5] * output[i + 8];
				sum += coeffs[4] * output[i + 9];
				sum += coeffs[3] * output[i + 10];
				sum += coeffs[2] * output[i + 11];
				sum += coeffs[1] * output[i + 12];
				sum += coeffs[0] * output[i + 13];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder15(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 15;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            int sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[14] * output[i + 0];
				sum += coeffs[13] * output[i + 1];
				sum += coeffs[12] * output[i + 2];
				sum += coeffs[11] * output[i + 3];
				sum += coeffs[10] * output[i + 4];
				sum += coeffs[9] * output[i + 5];
				sum += coeffs[8] * output[i + 6];
				sum += coeffs[7] * output[i + 7];
				sum += coeffs[6] * output[i + 8];
				sum += coeffs[5] * output[i + 9];
				sum += coeffs[4] * output[i + 10];
				sum += coeffs[3] * output[i + 11];
				sum += coeffs[2] * output[i + 12];
				sum += coeffs[1] * output[i + 13];
				sum += coeffs[0] * output[i + 14];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder16(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 16;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            int sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[15] * output[i + 0];
				sum += coeffs[14] * output[i + 1];
				sum += coeffs[13] * output[i + 2];
				sum += coeffs[12] * output[i + 3];
				sum += coeffs[11] * output[i + 4];
				sum += coeffs[10] * output[i + 5];
				sum += coeffs[9] * output[i + 6];
				sum += coeffs[8] * output[i + 7];
				sum += coeffs[7] * output[i + 8];
				sum += coeffs[6] * output[i + 9];
				sum += coeffs[5] * output[i + 10];
				sum += coeffs[4] * output[i + 11];
				sum += coeffs[3] * output[i + 12];
				sum += coeffs[2] * output[i + 13];
				sum += coeffs[1] * output[i + 14];
				sum += coeffs[0] * output[i + 15];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder17(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 17;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            int sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[16] * output[i + 0];
				sum += coeffs[15] * output[i + 1];
				sum += coeffs[14] * output[i + 2];
				sum += coeffs[13] * output[i + 3];
				sum += coeffs[12] * output[i + 4];
				sum += coeffs[11] * output[i + 5];
				sum += coeffs[10] * output[i + 6];
				sum += coeffs[9] * output[i + 7];
				sum += coeffs[8] * output[i + 8];
				sum += coeffs[7] * output[i + 9];
				sum += coeffs[6] * output[i + 10];
				sum += coeffs[5] * output[i + 11];
				sum += coeffs[4] * output[i + 12];
				sum += coeffs[3] * output[i + 13];
				sum += coeffs[2] * output[i + 14];
				sum += coeffs[1] * output[i + 15];
				sum += coeffs[0] * output[i + 16];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder18(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 18;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            int sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[17] * output[i + 0];
				sum += coeffs[16] * output[i + 1];
				sum += coeffs[15] * output[i + 2];
				sum += coeffs[14] * output[i + 3];
				sum += coeffs[13] * output[i + 4];
				sum += coeffs[12] * output[i + 5];
				sum += coeffs[11] * output[i + 6];
				sum += coeffs[10] * output[i + 7];
				sum += coeffs[9] * output[i + 8];
				sum += coeffs[8] * output[i + 9];
				sum += coeffs[7] * output[i + 10];
				sum += coeffs[6] * output[i + 11];
				sum += coeffs[5] * output[i + 12];
				sum += coeffs[4] * output[i + 13];
				sum += coeffs[3] * output[i + 14];
				sum += coeffs[2] * output[i + 15];
				sum += coeffs[1] * output[i + 16];
				sum += coeffs[0] * output[i + 17];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder19(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 19;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            int sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[18] * output[i + 0];
				sum += coeffs[17] * output[i + 1];
				sum += coeffs[16] * output[i + 2];
				sum += coeffs[15] * output[i + 3];
				sum += coeffs[14] * output[i + 4];
				sum += coeffs[13] * output[i + 5];
				sum += coeffs[12] * output[i + 6];
				sum += coeffs[11] * output[i + 7];
				sum += coeffs[10] * output[i + 8];
				sum += coeffs[9] * output[i + 9];
				sum += coeffs[8] * output[i + 10];
				sum += coeffs[7] * output[i + 11];
				sum += coeffs[6] * output[i + 12];
				sum += coeffs[5] * output[i + 13];
				sum += coeffs[4] * output[i + 14];
				sum += coeffs[3] * output[i + 15];
				sum += coeffs[2] * output[i + 16];
				sum += coeffs[1] * output[i + 17];
				sum += coeffs[0] * output[i + 18];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder20(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 20;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            int sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[19] * output[i + 0];
				sum += coeffs[18] * output[i + 1];
				sum += coeffs[17] * output[i + 2];
				sum += coeffs[16] * output[i + 3];
				sum += coeffs[15] * output[i + 4];
				sum += coeffs[14] * output[i + 5];
				sum += coeffs[13] * output[i + 6];
				sum += coeffs[12] * output[i + 7];
				sum += coeffs[11] * output[i + 8];
				sum += coeffs[10] * output[i + 9];
				sum += coeffs[9] * output[i + 10];
				sum += coeffs[8] * output[i + 11];
				sum += coeffs[7] * output[i + 12];
				sum += coeffs[6] * output[i + 13];
				sum += coeffs[5] * output[i + 14];
				sum += coeffs[4] * output[i + 15];
				sum += coeffs[3] * output[i + 16];
				sum += coeffs[2] * output[i + 17];
				sum += coeffs[1] * output[i + 18];
				sum += coeffs[0] * output[i + 19];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder21(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 21;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            int sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[20] * output[i + 0];
				sum += coeffs[19] * output[i + 1];
				sum += coeffs[18] * output[i + 2];
				sum += coeffs[17] * output[i + 3];
				sum += coeffs[16] * output[i + 4];
				sum += coeffs[15] * output[i + 5];
				sum += coeffs[14] * output[i + 6];
				sum += coeffs[13] * output[i + 7];
				sum += coeffs[12] * output[i + 8];
				sum += coeffs[11] * output[i + 9];
				sum += coeffs[10] * output[i + 10];
				sum += coeffs[9] * output[i + 11];
				sum += coeffs[8] * output[i + 12];
				sum += coeffs[7] * output[i + 13];
				sum += coeffs[6] * output[i + 14];
				sum += coeffs[5] * output[i + 15];
				sum += coeffs[4] * output[i + 16];
				sum += coeffs[3] * output[i + 17];
				sum += coeffs[2] * output[i + 18];
				sum += coeffs[1] * output[i + 19];
				sum += coeffs[0] * output[i + 20];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder22(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 22;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            int sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[21] * output[i + 0];
				sum += coeffs[20] * output[i + 1];
				sum += coeffs[19] * output[i + 2];
				sum += coeffs[18] * output[i + 3];
				sum += coeffs[17] * output[i + 4];
				sum += coeffs[16] * output[i + 5];
				sum += coeffs[15] * output[i + 6];
				sum += coeffs[14] * output[i + 7];
				sum += coeffs[13] * output[i + 8];
				sum += coeffs[12] * output[i + 9];
				sum += coeffs[11] * output[i + 10];
				sum += coeffs[10] * output[i + 11];
				sum += coeffs[9] * output[i + 12];
				sum += coeffs[8] * output[i + 13];
				sum += coeffs[7] * output[i + 14];
				sum += coeffs[6] * output[i + 15];
				sum += coeffs[5] * output[i + 16];
				sum += coeffs[4] * output[i + 17];
				sum += coeffs[3] * output[i + 18];
				sum += coeffs[2] * output[i + 19];
				sum += coeffs[1] * output[i + 20];
				sum += coeffs[0] * output[i + 21];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder23(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 23;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            int sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[22] * output[i + 0];
				sum += coeffs[21] * output[i + 1];
				sum += coeffs[20] * output[i + 2];
				sum += coeffs[19] * output[i + 3];
				sum += coeffs[18] * output[i + 4];
				sum += coeffs[17] * output[i + 5];
				sum += coeffs[16] * output[i + 6];
				sum += coeffs[15] * output[i + 7];
				sum += coeffs[14] * output[i + 8];
				sum += coeffs[13] * output[i + 9];
				sum += coeffs[12] * output[i + 10];
				sum += coeffs[11] * output[i + 11];
				sum += coeffs[10] * output[i + 12];
				sum += coeffs[9] * output[i + 13];
				sum += coeffs[8] * output[i + 14];
				sum += coeffs[7] * output[i + 15];
				sum += coeffs[6] * output[i + 16];
				sum += coeffs[5] * output[i + 17];
				sum += coeffs[4] * output[i + 18];
				sum += coeffs[3] * output[i + 19];
				sum += coeffs[2] * output[i + 20];
				sum += coeffs[1] * output[i + 21];
				sum += coeffs[0] * output[i + 22];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder24(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 24;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            int sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[23] * output[i + 0];
				sum += coeffs[22] * output[i + 1];
				sum += coeffs[21] * output[i + 2];
				sum += coeffs[20] * output[i + 3];
				sum += coeffs[19] * output[i + 4];
				sum += coeffs[18] * output[i + 5];
				sum += coeffs[17] * output[i + 6];
				sum += coeffs[16] * output[i + 7];
				sum += coeffs[15] * output[i + 8];
				sum += coeffs[14] * output[i + 9];
				sum += coeffs[13] * output[i + 10];
				sum += coeffs[12] * output[i + 11];
				sum += coeffs[11] * output[i + 12];
				sum += coeffs[10] * output[i + 13];
				sum += coeffs[9] * output[i + 14];
				sum += coeffs[8] * output[i + 15];
				sum += coeffs[7] * output[i + 16];
				sum += coeffs[6] * output[i + 17];
				sum += coeffs[5] * output[i + 18];
				sum += coeffs[4] * output[i + 19];
				sum += coeffs[3] * output[i + 20];
				sum += coeffs[2] * output[i + 21];
				sum += coeffs[1] * output[i + 22];
				sum += coeffs[0] * output[i + 23];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder25(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 25;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            int sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[24] * output[i + 0];
				sum += coeffs[23] * output[i + 1];
				sum += coeffs[22] * output[i + 2];
				sum += coeffs[21] * output[i + 3];
				sum += coeffs[20] * output[i + 4];
				sum += coeffs[19] * output[i + 5];
				sum += coeffs[18] * output[i + 6];
				sum += coeffs[17] * output[i + 7];
				sum += coeffs[16] * output[i + 8];
				sum += coeffs[15] * output[i + 9];
				sum += coeffs[14] * output[i + 10];
				sum += coeffs[13] * output[i + 11];
				sum += coeffs[12] * output[i + 12];
				sum += coeffs[11] * output[i + 13];
				sum += coeffs[10] * output[i + 14];
				sum += coeffs[9] * output[i + 15];
				sum += coeffs[8] * output[i + 16];
				sum += coeffs[7] * output[i + 17];
				sum += coeffs[6] * output[i + 18];
				sum += coeffs[5] * output[i + 19];
				sum += coeffs[4] * output[i + 20];
				sum += coeffs[3] * output[i + 21];
				sum += coeffs[2] * output[i + 22];
				sum += coeffs[1] * output[i + 23];
				sum += coeffs[0] * output[i + 24];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder26(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 26;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            int sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[25] * output[i + 0];
				sum += coeffs[24] * output[i + 1];
				sum += coeffs[23] * output[i + 2];
				sum += coeffs[22] * output[i + 3];
				sum += coeffs[21] * output[i + 4];
				sum += coeffs[20] * output[i + 5];
				sum += coeffs[19] * output[i + 6];
				sum += coeffs[18] * output[i + 7];
				sum += coeffs[17] * output[i + 8];
				sum += coeffs[16] * output[i + 9];
				sum += coeffs[15] * output[i + 10];
				sum += coeffs[14] * output[i + 11];
				sum += coeffs[13] * output[i + 12];
				sum += coeffs[12] * output[i + 13];
				sum += coeffs[11] * output[i + 14];
				sum += coeffs[10] * output[i + 15];
				sum += coeffs[9] * output[i + 16];
				sum += coeffs[8] * output[i + 17];
				sum += coeffs[7] * output[i + 18];
				sum += coeffs[6] * output[i + 19];
				sum += coeffs[5] * output[i + 20];
				sum += coeffs[4] * output[i + 21];
				sum += coeffs[3] * output[i + 22];
				sum += coeffs[2] * output[i + 23];
				sum += coeffs[1] * output[i + 24];
				sum += coeffs[0] * output[i + 25];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder27(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 27;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            int sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[26] * output[i + 0];
				sum += coeffs[25] * output[i + 1];
				sum += coeffs[24] * output[i + 2];
				sum += coeffs[23] * output[i + 3];
				sum += coeffs[22] * output[i + 4];
				sum += coeffs[21] * output[i + 5];
				sum += coeffs[20] * output[i + 6];
				sum += coeffs[19] * output[i + 7];
				sum += coeffs[18] * output[i + 8];
				sum += coeffs[17] * output[i + 9];
				sum += coeffs[16] * output[i + 10];
				sum += coeffs[15] * output[i + 11];
				sum += coeffs[14] * output[i + 12];
				sum += coeffs[13] * output[i + 13];
				sum += coeffs[12] * output[i + 14];
				sum += coeffs[11] * output[i + 15];
				sum += coeffs[10] * output[i + 16];
				sum += coeffs[9] * output[i + 17];
				sum += coeffs[8] * output[i + 18];
				sum += coeffs[7] * output[i + 19];
				sum += coeffs[6] * output[i + 20];
				sum += coeffs[5] * output[i + 21];
				sum += coeffs[4] * output[i + 22];
				sum += coeffs[3] * output[i + 23];
				sum += coeffs[2] * output[i + 24];
				sum += coeffs[1] * output[i + 25];
				sum += coeffs[0] * output[i + 26];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder28(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 28;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            int sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[27] * output[i + 0];
				sum += coeffs[26] * output[i + 1];
				sum += coeffs[25] * output[i + 2];
				sum += coeffs[24] * output[i + 3];
				sum += coeffs[23] * output[i + 4];
				sum += coeffs[22] * output[i + 5];
				sum += coeffs[21] * output[i + 6];
				sum += coeffs[20] * output[i + 7];
				sum += coeffs[19] * output[i + 8];
				sum += coeffs[18] * output[i + 9];
				sum += coeffs[17] * output[i + 10];
				sum += coeffs[16] * output[i + 11];
				sum += coeffs[15] * output[i + 12];
				sum += coeffs[14] * output[i + 13];
				sum += coeffs[13] * output[i + 14];
				sum += coeffs[12] * output[i + 15];
				sum += coeffs[11] * output[i + 16];
				sum += coeffs[10] * output[i + 17];
				sum += coeffs[9] * output[i + 18];
				sum += coeffs[8] * output[i + 19];
				sum += coeffs[7] * output[i + 20];
				sum += coeffs[6] * output[i + 21];
				sum += coeffs[5] * output[i + 22];
				sum += coeffs[4] * output[i + 23];
				sum += coeffs[3] * output[i + 24];
				sum += coeffs[2] * output[i + 25];
				sum += coeffs[1] * output[i + 26];
				sum += coeffs[0] * output[i + 27];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder29(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 29;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            int sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[28] * output[i + 0];
				sum += coeffs[27] * output[i + 1];
				sum += coeffs[26] * output[i + 2];
				sum += coeffs[25] * output[i + 3];
				sum += coeffs[24] * output[i + 4];
				sum += coeffs[23] * output[i + 5];
				sum += coeffs[22] * output[i + 6];
				sum += coeffs[21] * output[i + 7];
				sum += coeffs[20] * output[i + 8];
				sum += coeffs[19] * output[i + 9];
				sum += coeffs[18] * output[i + 10];
				sum += coeffs[17] * output[i + 11];
				sum += coeffs[16] * output[i + 12];
				sum += coeffs[15] * output[i + 13];
				sum += coeffs[14] * output[i + 14];
				sum += coeffs[13] * output[i + 15];
				sum += coeffs[12] * output[i + 16];
				sum += coeffs[11] * output[i + 17];
				sum += coeffs[10] * output[i + 18];
				sum += coeffs[9] * output[i + 19];
				sum += coeffs[8] * output[i + 20];
				sum += coeffs[7] * output[i + 21];
				sum += coeffs[6] * output[i + 22];
				sum += coeffs[5] * output[i + 23];
				sum += coeffs[4] * output[i + 24];
				sum += coeffs[3] * output[i + 25];
				sum += coeffs[2] * output[i + 26];
				sum += coeffs[1] * output[i + 27];
				sum += coeffs[0] * output[i + 28];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder30(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 30;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            int sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[29] * output[i + 0];
				sum += coeffs[28] * output[i + 1];
				sum += coeffs[27] * output[i + 2];
				sum += coeffs[26] * output[i + 3];
				sum += coeffs[25] * output[i + 4];
				sum += coeffs[24] * output[i + 5];
				sum += coeffs[23] * output[i + 6];
				sum += coeffs[22] * output[i + 7];
				sum += coeffs[21] * output[i + 8];
				sum += coeffs[20] * output[i + 9];
				sum += coeffs[19] * output[i + 10];
				sum += coeffs[18] * output[i + 11];
				sum += coeffs[17] * output[i + 12];
				sum += coeffs[16] * output[i + 13];
				sum += coeffs[15] * output[i + 14];
				sum += coeffs[14] * output[i + 15];
				sum += coeffs[13] * output[i + 16];
				sum += coeffs[12] * output[i + 17];
				sum += coeffs[11] * output[i + 18];
				sum += coeffs[10] * output[i + 19];
				sum += coeffs[9] * output[i + 20];
				sum += coeffs[8] * output[i + 21];
				sum += coeffs[7] * output[i + 22];
				sum += coeffs[6] * output[i + 23];
				sum += coeffs[5] * output[i + 24];
				sum += coeffs[4] * output[i + 25];
				sum += coeffs[3] * output[i + 26];
				sum += coeffs[2] * output[i + 27];
				sum += coeffs[1] * output[i + 28];
				sum += coeffs[0] * output[i + 29];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder31(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 31;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            int sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[30] * output[i + 0];
				sum += coeffs[29] * output[i + 1];
				sum += coeffs[28] * output[i + 2];
				sum += coeffs[27] * output[i + 3];
				sum += coeffs[26] * output[i + 4];
				sum += coeffs[25] * output[i + 5];
				sum += coeffs[24] * output[i + 6];
				sum += coeffs[23] * output[i + 7];
				sum += coeffs[22] * output[i + 8];
				sum += coeffs[21] * output[i + 9];
				sum += coeffs[20] * output[i + 10];
				sum += coeffs[19] * output[i + 11];
				sum += coeffs[18] * output[i + 12];
				sum += coeffs[17] * output[i + 13];
				sum += coeffs[16] * output[i + 14];
				sum += coeffs[15] * output[i + 15];
				sum += coeffs[14] * output[i + 16];
				sum += coeffs[13] * output[i + 17];
				sum += coeffs[12] * output[i + 18];
				sum += coeffs[11] * output[i + 19];
				sum += coeffs[10] * output[i + 20];
				sum += coeffs[9] * output[i + 21];
				sum += coeffs[8] * output[i + 22];
				sum += coeffs[7] * output[i + 23];
				sum += coeffs[6] * output[i + 24];
				sum += coeffs[5] * output[i + 25];
				sum += coeffs[4] * output[i + 26];
				sum += coeffs[3] * output[i + 27];
				sum += coeffs[2] * output[i + 28];
				sum += coeffs[1] * output[i + 29];
				sum += coeffs[0] * output[i + 30];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder32(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 32;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            int sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[31] * output[i + 0];
				sum += coeffs[30] * output[i + 1];
				sum += coeffs[29] * output[i + 2];
				sum += coeffs[28] * output[i + 3];
				sum += coeffs[27] * output[i + 4];
				sum += coeffs[26] * output[i + 5];
				sum += coeffs[25] * output[i + 6];
				sum += coeffs[24] * output[i + 7];
				sum += coeffs[23] * output[i + 8];
				sum += coeffs[22] * output[i + 9];
				sum += coeffs[21] * output[i + 10];
				sum += coeffs[20] * output[i + 11];
				sum += coeffs[19] * output[i + 12];
				sum += coeffs[18] * output[i + 13];
				sum += coeffs[17] * output[i + 14];
				sum += coeffs[16] * output[i + 15];
				sum += coeffs[15] * output[i + 16];
				sum += coeffs[14] * output[i + 17];
				sum += coeffs[13] * output[i + 18];
				sum += coeffs[12] * output[i + 19];
				sum += coeffs[11] * output[i + 20];
				sum += coeffs[10] * output[i + 21];
				sum += coeffs[9] * output[i + 22];
				sum += coeffs[8] * output[i + 23];
				sum += coeffs[7] * output[i + 24];
				sum += coeffs[6] * output[i + 25];
				sum += coeffs[5] * output[i + 26];
				sum += coeffs[4] * output[i + 27];
				sum += coeffs[3] * output[i + 28];
				sum += coeffs[2] * output[i + 29];
				sum += coeffs[1] * output[i + 30];
				sum += coeffs[0] * output[i + 31];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalStandardWide(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            var order = coeffs.Length;
            switch (order)
            {
                case 1:
                    RestoreSignalOrder1Wide(shiftsNeeded, residual, coeffs, output);
                    return;
                case 2:
                    RestoreSignalOrder2WideStandard(shiftsNeeded, residual, coeffs, output);
                    return;
                case 3:
                    RestoreSignalOrder3Wide(shiftsNeeded, residual, coeffs, output);
                    return;
                case 4:
                    RestoreSignalOrder4Wide(shiftsNeeded, residual, coeffs, output);
                    return;
                case 5:
                    RestoreSignalOrder5Wide(shiftsNeeded, residual, coeffs, output);
                    return;
                case 6:
                    RestoreSignalOrder6Wide(shiftsNeeded, residual, coeffs, output);
                    return;
                case 7:
                    RestoreSignalOrder7Wide(shiftsNeeded, residual, coeffs, output);
                    return;
                case 8:
                    RestoreSignalOrder8Wide(shiftsNeeded, residual, coeffs, output);
                    return;
                case 9:
                    RestoreSignalOrder9Wide(shiftsNeeded, residual, coeffs, output);
                    return;
                case 10:
                    RestoreSignalOrder10Wide(shiftsNeeded, residual, coeffs, output);
                    return;
                case 11:
                    RestoreSignalOrder11Wide(shiftsNeeded, residual, coeffs, output);
                    return;
                case 12:
                    RestoreSignalOrder12Wide(shiftsNeeded, residual, coeffs, output);
                    return;
                case 13:
                    RestoreSignalOrder13Wide(shiftsNeeded, residual, coeffs, output);
                    return;
                case 14:
                    RestoreSignalOrder14Wide(shiftsNeeded, residual, coeffs, output);
                    return;
                case 15:
                    RestoreSignalOrder15Wide(shiftsNeeded, residual, coeffs, output);
                    return;
                case 16:
                    RestoreSignalOrder16Wide(shiftsNeeded, residual, coeffs, output);
                    return;
                case 17:
                    RestoreSignalOrder17Wide(shiftsNeeded, residual, coeffs, output);
                    return;
                case 18:
                    RestoreSignalOrder18Wide(shiftsNeeded, residual, coeffs, output);
                    return;
                case 19:
                    RestoreSignalOrder19Wide(shiftsNeeded, residual, coeffs, output);
                    return;
                case 20:
                    RestoreSignalOrder20Wide(shiftsNeeded, residual, coeffs, output);
                    return;
                case 21:
                    RestoreSignalOrder21Wide(shiftsNeeded, residual, coeffs, output);
                    return;
                case 22:
                    RestoreSignalOrder22Wide(shiftsNeeded, residual, coeffs, output);
                    return;
                case 23:
                    RestoreSignalOrder23Wide(shiftsNeeded, residual, coeffs, output);
                    return;
                case 24:
                    RestoreSignalOrder24Wide(shiftsNeeded, residual, coeffs, output);
                    return;
                case 25:
                    RestoreSignalOrder25Wide(shiftsNeeded, residual, coeffs, output);
                    return;
                case 26:
                    RestoreSignalOrder26Wide(shiftsNeeded, residual, coeffs, output);
                    return;
                case 27:
                    RestoreSignalOrder27Wide(shiftsNeeded, residual, coeffs, output);
                    return;
                case 28:
                    RestoreSignalOrder28Wide(shiftsNeeded, residual, coeffs, output);
                    return;
                case 29:
                    RestoreSignalOrder29Wide(shiftsNeeded, residual, coeffs, output);
                    return;
                case 30:
                    RestoreSignalOrder30Wide(shiftsNeeded, residual, coeffs, output);
                    return;
                case 31:
                    RestoreSignalOrder31Wide(shiftsNeeded, residual, coeffs, output);
                    return;
                case 32:
                    RestoreSignalOrder32Wide(shiftsNeeded, residual, coeffs, output);
                    return;
                default:
                    throw new FlacException("Invalid FLAC stream!");
            }
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder1Wide(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 1;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
			var prev0 = output[0];
			long coeff0 = coeffs[0];
            long sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeff0 * prev0;
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = (int)sum;
                prev0 = (int)sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder2WideStandard(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 2;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
			var prev0 = output[0];
			var prev1 = output[1];
			long coeff0 = coeffs[0];
			long coeff1 = coeffs[1];
            long sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeff1 * prev0;
				prev0 = prev1;
				sum += coeff0 * prev1;
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = (int)sum;
                prev1 = (int)sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder3Wide(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 3;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
			var prev0 = output[0];
			var prev1 = output[1];
			var prev2 = output[2];
			long coeff0 = coeffs[0];
			long coeff1 = coeffs[1];
			long coeff2 = coeffs[2];
            long sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeff2 * prev0;
				prev0 = prev1;
				sum += coeff1 * prev1;
				prev1 = prev2;
				sum += coeff0 * prev2;
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = (int)sum;
                prev2 = (int)sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder4Wide(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 4;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
			var prev0 = output[0];
			var prev1 = output[1];
			var prev2 = output[2];
			var prev3 = output[3];
			long coeff0 = coeffs[0];
			long coeff1 = coeffs[1];
			long coeff2 = coeffs[2];
			long coeff3 = coeffs[3];
            long sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeff3 * prev0;
				prev0 = prev1;
				sum += coeff2 * prev1;
				prev1 = prev2;
				sum += coeff1 * prev2;
				prev2 = prev3;
				sum += coeff0 * prev3;
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = (int)sum;
                prev3 = (int)sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder5Wide(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 5;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
			long coeff0 = coeffs[0];
			long coeff1 = coeffs[1];
			long coeff2 = coeffs[2];
			long coeff3 = coeffs[3];
			long coeff4 = coeffs[4];
            long sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeff4 * output[i + 0];
				sum += coeff3 * output[i + 1];
				sum += coeff2 * output[i + 2];
				sum += coeff1 * output[i + 3];
				sum += coeff0 * output[i + 4];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = (int)sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder6Wide(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 6;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
			long coeff0 = coeffs[0];
			long coeff1 = coeffs[1];
			long coeff2 = coeffs[2];
			long coeff3 = coeffs[3];
			long coeff4 = coeffs[4];
			long coeff5 = coeffs[5];
            long sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeff5 * output[i + 0];
				sum += coeff4 * output[i + 1];
				sum += coeff3 * output[i + 2];
				sum += coeff2 * output[i + 3];
				sum += coeff1 * output[i + 4];
				sum += coeff0 * output[i + 5];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = (int)sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder7Wide(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 7;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
			long coeff0 = coeffs[0];
			long coeff1 = coeffs[1];
			long coeff2 = coeffs[2];
			long coeff3 = coeffs[3];
			long coeff4 = coeffs[4];
			long coeff5 = coeffs[5];
			long coeff6 = coeffs[6];
            long sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeff6 * output[i + 0];
				sum += coeff5 * output[i + 1];
				sum += coeff4 * output[i + 2];
				sum += coeff3 * output[i + 3];
				sum += coeff2 * output[i + 4];
				sum += coeff1 * output[i + 5];
				sum += coeff0 * output[i + 6];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = (int)sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder8Wide(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 8;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
			long coeff0 = coeffs[0];
			long coeff1 = coeffs[1];
			long coeff2 = coeffs[2];
			long coeff3 = coeffs[3];
			long coeff4 = coeffs[4];
			long coeff5 = coeffs[5];
			long coeff6 = coeffs[6];
			long coeff7 = coeffs[7];
            long sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeff7 * output[i + 0];
				sum += coeff6 * output[i + 1];
				sum += coeff5 * output[i + 2];
				sum += coeff4 * output[i + 3];
				sum += coeff3 * output[i + 4];
				sum += coeff2 * output[i + 5];
				sum += coeff1 * output[i + 6];
				sum += coeff0 * output[i + 7];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = (int)sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder9Wide(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 9;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            long sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[8] * (long)output[i + 0];
				sum += coeffs[7] * (long)output[i + 1];
				sum += coeffs[6] * (long)output[i + 2];
				sum += coeffs[5] * (long)output[i + 3];
				sum += coeffs[4] * (long)output[i + 4];
				sum += coeffs[3] * (long)output[i + 5];
				sum += coeffs[2] * (long)output[i + 6];
				sum += coeffs[1] * (long)output[i + 7];
				sum += coeffs[0] * (long)output[i + 8];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = (int)sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder10Wide(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 10;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            long sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[9] * (long)output[i + 0];
				sum += coeffs[8] * (long)output[i + 1];
				sum += coeffs[7] * (long)output[i + 2];
				sum += coeffs[6] * (long)output[i + 3];
				sum += coeffs[5] * (long)output[i + 4];
				sum += coeffs[4] * (long)output[i + 5];
				sum += coeffs[3] * (long)output[i + 6];
				sum += coeffs[2] * (long)output[i + 7];
				sum += coeffs[1] * (long)output[i + 8];
				sum += coeffs[0] * (long)output[i + 9];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = (int)sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder11Wide(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 11;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            long sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[10] * (long)output[i + 0];
				sum += coeffs[9] * (long)output[i + 1];
				sum += coeffs[8] * (long)output[i + 2];
				sum += coeffs[7] * (long)output[i + 3];
				sum += coeffs[6] * (long)output[i + 4];
				sum += coeffs[5] * (long)output[i + 5];
				sum += coeffs[4] * (long)output[i + 6];
				sum += coeffs[3] * (long)output[i + 7];
				sum += coeffs[2] * (long)output[i + 8];
				sum += coeffs[1] * (long)output[i + 9];
				sum += coeffs[0] * (long)output[i + 10];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = (int)sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder12Wide(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 12;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            long sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[11] * (long)output[i + 0];
				sum += coeffs[10] * (long)output[i + 1];
				sum += coeffs[9] * (long)output[i + 2];
				sum += coeffs[8] * (long)output[i + 3];
				sum += coeffs[7] * (long)output[i + 4];
				sum += coeffs[6] * (long)output[i + 5];
				sum += coeffs[5] * (long)output[i + 6];
				sum += coeffs[4] * (long)output[i + 7];
				sum += coeffs[3] * (long)output[i + 8];
				sum += coeffs[2] * (long)output[i + 9];
				sum += coeffs[1] * (long)output[i + 10];
				sum += coeffs[0] * (long)output[i + 11];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = (int)sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder13Wide(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 13;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            long sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[12] * (long)output[i + 0];
				sum += coeffs[11] * (long)output[i + 1];
				sum += coeffs[10] * (long)output[i + 2];
				sum += coeffs[9] * (long)output[i + 3];
				sum += coeffs[8] * (long)output[i + 4];
				sum += coeffs[7] * (long)output[i + 5];
				sum += coeffs[6] * (long)output[i + 6];
				sum += coeffs[5] * (long)output[i + 7];
				sum += coeffs[4] * (long)output[i + 8];
				sum += coeffs[3] * (long)output[i + 9];
				sum += coeffs[2] * (long)output[i + 10];
				sum += coeffs[1] * (long)output[i + 11];
				sum += coeffs[0] * (long)output[i + 12];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = (int)sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder14Wide(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 14;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            long sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[13] * (long)output[i + 0];
				sum += coeffs[12] * (long)output[i + 1];
				sum += coeffs[11] * (long)output[i + 2];
				sum += coeffs[10] * (long)output[i + 3];
				sum += coeffs[9] * (long)output[i + 4];
				sum += coeffs[8] * (long)output[i + 5];
				sum += coeffs[7] * (long)output[i + 6];
				sum += coeffs[6] * (long)output[i + 7];
				sum += coeffs[5] * (long)output[i + 8];
				sum += coeffs[4] * (long)output[i + 9];
				sum += coeffs[3] * (long)output[i + 10];
				sum += coeffs[2] * (long)output[i + 11];
				sum += coeffs[1] * (long)output[i + 12];
				sum += coeffs[0] * (long)output[i + 13];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = (int)sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder15Wide(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 15;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            long sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[14] * (long)output[i + 0];
				sum += coeffs[13] * (long)output[i + 1];
				sum += coeffs[12] * (long)output[i + 2];
				sum += coeffs[11] * (long)output[i + 3];
				sum += coeffs[10] * (long)output[i + 4];
				sum += coeffs[9] * (long)output[i + 5];
				sum += coeffs[8] * (long)output[i + 6];
				sum += coeffs[7] * (long)output[i + 7];
				sum += coeffs[6] * (long)output[i + 8];
				sum += coeffs[5] * (long)output[i + 9];
				sum += coeffs[4] * (long)output[i + 10];
				sum += coeffs[3] * (long)output[i + 11];
				sum += coeffs[2] * (long)output[i + 12];
				sum += coeffs[1] * (long)output[i + 13];
				sum += coeffs[0] * (long)output[i + 14];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = (int)sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder16Wide(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 16;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            long sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[15] * (long)output[i + 0];
				sum += coeffs[14] * (long)output[i + 1];
				sum += coeffs[13] * (long)output[i + 2];
				sum += coeffs[12] * (long)output[i + 3];
				sum += coeffs[11] * (long)output[i + 4];
				sum += coeffs[10] * (long)output[i + 5];
				sum += coeffs[9] * (long)output[i + 6];
				sum += coeffs[8] * (long)output[i + 7];
				sum += coeffs[7] * (long)output[i + 8];
				sum += coeffs[6] * (long)output[i + 9];
				sum += coeffs[5] * (long)output[i + 10];
				sum += coeffs[4] * (long)output[i + 11];
				sum += coeffs[3] * (long)output[i + 12];
				sum += coeffs[2] * (long)output[i + 13];
				sum += coeffs[1] * (long)output[i + 14];
				sum += coeffs[0] * (long)output[i + 15];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = (int)sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder17Wide(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 17;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            long sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[16] * (long)output[i + 0];
				sum += coeffs[15] * (long)output[i + 1];
				sum += coeffs[14] * (long)output[i + 2];
				sum += coeffs[13] * (long)output[i + 3];
				sum += coeffs[12] * (long)output[i + 4];
				sum += coeffs[11] * (long)output[i + 5];
				sum += coeffs[10] * (long)output[i + 6];
				sum += coeffs[9] * (long)output[i + 7];
				sum += coeffs[8] * (long)output[i + 8];
				sum += coeffs[7] * (long)output[i + 9];
				sum += coeffs[6] * (long)output[i + 10];
				sum += coeffs[5] * (long)output[i + 11];
				sum += coeffs[4] * (long)output[i + 12];
				sum += coeffs[3] * (long)output[i + 13];
				sum += coeffs[2] * (long)output[i + 14];
				sum += coeffs[1] * (long)output[i + 15];
				sum += coeffs[0] * (long)output[i + 16];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = (int)sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder18Wide(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 18;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            long sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[17] * (long)output[i + 0];
				sum += coeffs[16] * (long)output[i + 1];
				sum += coeffs[15] * (long)output[i + 2];
				sum += coeffs[14] * (long)output[i + 3];
				sum += coeffs[13] * (long)output[i + 4];
				sum += coeffs[12] * (long)output[i + 5];
				sum += coeffs[11] * (long)output[i + 6];
				sum += coeffs[10] * (long)output[i + 7];
				sum += coeffs[9] * (long)output[i + 8];
				sum += coeffs[8] * (long)output[i + 9];
				sum += coeffs[7] * (long)output[i + 10];
				sum += coeffs[6] * (long)output[i + 11];
				sum += coeffs[5] * (long)output[i + 12];
				sum += coeffs[4] * (long)output[i + 13];
				sum += coeffs[3] * (long)output[i + 14];
				sum += coeffs[2] * (long)output[i + 15];
				sum += coeffs[1] * (long)output[i + 16];
				sum += coeffs[0] * (long)output[i + 17];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = (int)sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder19Wide(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 19;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            long sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[18] * (long)output[i + 0];
				sum += coeffs[17] * (long)output[i + 1];
				sum += coeffs[16] * (long)output[i + 2];
				sum += coeffs[15] * (long)output[i + 3];
				sum += coeffs[14] * (long)output[i + 4];
				sum += coeffs[13] * (long)output[i + 5];
				sum += coeffs[12] * (long)output[i + 6];
				sum += coeffs[11] * (long)output[i + 7];
				sum += coeffs[10] * (long)output[i + 8];
				sum += coeffs[9] * (long)output[i + 9];
				sum += coeffs[8] * (long)output[i + 10];
				sum += coeffs[7] * (long)output[i + 11];
				sum += coeffs[6] * (long)output[i + 12];
				sum += coeffs[5] * (long)output[i + 13];
				sum += coeffs[4] * (long)output[i + 14];
				sum += coeffs[3] * (long)output[i + 15];
				sum += coeffs[2] * (long)output[i + 16];
				sum += coeffs[1] * (long)output[i + 17];
				sum += coeffs[0] * (long)output[i + 18];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = (int)sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder20Wide(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 20;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            long sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[19] * (long)output[i + 0];
				sum += coeffs[18] * (long)output[i + 1];
				sum += coeffs[17] * (long)output[i + 2];
				sum += coeffs[16] * (long)output[i + 3];
				sum += coeffs[15] * (long)output[i + 4];
				sum += coeffs[14] * (long)output[i + 5];
				sum += coeffs[13] * (long)output[i + 6];
				sum += coeffs[12] * (long)output[i + 7];
				sum += coeffs[11] * (long)output[i + 8];
				sum += coeffs[10] * (long)output[i + 9];
				sum += coeffs[9] * (long)output[i + 10];
				sum += coeffs[8] * (long)output[i + 11];
				sum += coeffs[7] * (long)output[i + 12];
				sum += coeffs[6] * (long)output[i + 13];
				sum += coeffs[5] * (long)output[i + 14];
				sum += coeffs[4] * (long)output[i + 15];
				sum += coeffs[3] * (long)output[i + 16];
				sum += coeffs[2] * (long)output[i + 17];
				sum += coeffs[1] * (long)output[i + 18];
				sum += coeffs[0] * (long)output[i + 19];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = (int)sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder21Wide(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 21;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            long sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[20] * (long)output[i + 0];
				sum += coeffs[19] * (long)output[i + 1];
				sum += coeffs[18] * (long)output[i + 2];
				sum += coeffs[17] * (long)output[i + 3];
				sum += coeffs[16] * (long)output[i + 4];
				sum += coeffs[15] * (long)output[i + 5];
				sum += coeffs[14] * (long)output[i + 6];
				sum += coeffs[13] * (long)output[i + 7];
				sum += coeffs[12] * (long)output[i + 8];
				sum += coeffs[11] * (long)output[i + 9];
				sum += coeffs[10] * (long)output[i + 10];
				sum += coeffs[9] * (long)output[i + 11];
				sum += coeffs[8] * (long)output[i + 12];
				sum += coeffs[7] * (long)output[i + 13];
				sum += coeffs[6] * (long)output[i + 14];
				sum += coeffs[5] * (long)output[i + 15];
				sum += coeffs[4] * (long)output[i + 16];
				sum += coeffs[3] * (long)output[i + 17];
				sum += coeffs[2] * (long)output[i + 18];
				sum += coeffs[1] * (long)output[i + 19];
				sum += coeffs[0] * (long)output[i + 20];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = (int)sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder22Wide(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 22;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            long sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[21] * (long)output[i + 0];
				sum += coeffs[20] * (long)output[i + 1];
				sum += coeffs[19] * (long)output[i + 2];
				sum += coeffs[18] * (long)output[i + 3];
				sum += coeffs[17] * (long)output[i + 4];
				sum += coeffs[16] * (long)output[i + 5];
				sum += coeffs[15] * (long)output[i + 6];
				sum += coeffs[14] * (long)output[i + 7];
				sum += coeffs[13] * (long)output[i + 8];
				sum += coeffs[12] * (long)output[i + 9];
				sum += coeffs[11] * (long)output[i + 10];
				sum += coeffs[10] * (long)output[i + 11];
				sum += coeffs[9] * (long)output[i + 12];
				sum += coeffs[8] * (long)output[i + 13];
				sum += coeffs[7] * (long)output[i + 14];
				sum += coeffs[6] * (long)output[i + 15];
				sum += coeffs[5] * (long)output[i + 16];
				sum += coeffs[4] * (long)output[i + 17];
				sum += coeffs[3] * (long)output[i + 18];
				sum += coeffs[2] * (long)output[i + 19];
				sum += coeffs[1] * (long)output[i + 20];
				sum += coeffs[0] * (long)output[i + 21];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = (int)sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder23Wide(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 23;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            long sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[22] * (long)output[i + 0];
				sum += coeffs[21] * (long)output[i + 1];
				sum += coeffs[20] * (long)output[i + 2];
				sum += coeffs[19] * (long)output[i + 3];
				sum += coeffs[18] * (long)output[i + 4];
				sum += coeffs[17] * (long)output[i + 5];
				sum += coeffs[16] * (long)output[i + 6];
				sum += coeffs[15] * (long)output[i + 7];
				sum += coeffs[14] * (long)output[i + 8];
				sum += coeffs[13] * (long)output[i + 9];
				sum += coeffs[12] * (long)output[i + 10];
				sum += coeffs[11] * (long)output[i + 11];
				sum += coeffs[10] * (long)output[i + 12];
				sum += coeffs[9] * (long)output[i + 13];
				sum += coeffs[8] * (long)output[i + 14];
				sum += coeffs[7] * (long)output[i + 15];
				sum += coeffs[6] * (long)output[i + 16];
				sum += coeffs[5] * (long)output[i + 17];
				sum += coeffs[4] * (long)output[i + 18];
				sum += coeffs[3] * (long)output[i + 19];
				sum += coeffs[2] * (long)output[i + 20];
				sum += coeffs[1] * (long)output[i + 21];
				sum += coeffs[0] * (long)output[i + 22];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = (int)sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder24Wide(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 24;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            long sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[23] * (long)output[i + 0];
				sum += coeffs[22] * (long)output[i + 1];
				sum += coeffs[21] * (long)output[i + 2];
				sum += coeffs[20] * (long)output[i + 3];
				sum += coeffs[19] * (long)output[i + 4];
				sum += coeffs[18] * (long)output[i + 5];
				sum += coeffs[17] * (long)output[i + 6];
				sum += coeffs[16] * (long)output[i + 7];
				sum += coeffs[15] * (long)output[i + 8];
				sum += coeffs[14] * (long)output[i + 9];
				sum += coeffs[13] * (long)output[i + 10];
				sum += coeffs[12] * (long)output[i + 11];
				sum += coeffs[11] * (long)output[i + 12];
				sum += coeffs[10] * (long)output[i + 13];
				sum += coeffs[9] * (long)output[i + 14];
				sum += coeffs[8] * (long)output[i + 15];
				sum += coeffs[7] * (long)output[i + 16];
				sum += coeffs[6] * (long)output[i + 17];
				sum += coeffs[5] * (long)output[i + 18];
				sum += coeffs[4] * (long)output[i + 19];
				sum += coeffs[3] * (long)output[i + 20];
				sum += coeffs[2] * (long)output[i + 21];
				sum += coeffs[1] * (long)output[i + 22];
				sum += coeffs[0] * (long)output[i + 23];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = (int)sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder25Wide(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 25;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            long sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[24] * (long)output[i + 0];
				sum += coeffs[23] * (long)output[i + 1];
				sum += coeffs[22] * (long)output[i + 2];
				sum += coeffs[21] * (long)output[i + 3];
				sum += coeffs[20] * (long)output[i + 4];
				sum += coeffs[19] * (long)output[i + 5];
				sum += coeffs[18] * (long)output[i + 6];
				sum += coeffs[17] * (long)output[i + 7];
				sum += coeffs[16] * (long)output[i + 8];
				sum += coeffs[15] * (long)output[i + 9];
				sum += coeffs[14] * (long)output[i + 10];
				sum += coeffs[13] * (long)output[i + 11];
				sum += coeffs[12] * (long)output[i + 12];
				sum += coeffs[11] * (long)output[i + 13];
				sum += coeffs[10] * (long)output[i + 14];
				sum += coeffs[9] * (long)output[i + 15];
				sum += coeffs[8] * (long)output[i + 16];
				sum += coeffs[7] * (long)output[i + 17];
				sum += coeffs[6] * (long)output[i + 18];
				sum += coeffs[5] * (long)output[i + 19];
				sum += coeffs[4] * (long)output[i + 20];
				sum += coeffs[3] * (long)output[i + 21];
				sum += coeffs[2] * (long)output[i + 22];
				sum += coeffs[1] * (long)output[i + 23];
				sum += coeffs[0] * (long)output[i + 24];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = (int)sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder26Wide(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 26;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            long sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[25] * (long)output[i + 0];
				sum += coeffs[24] * (long)output[i + 1];
				sum += coeffs[23] * (long)output[i + 2];
				sum += coeffs[22] * (long)output[i + 3];
				sum += coeffs[21] * (long)output[i + 4];
				sum += coeffs[20] * (long)output[i + 5];
				sum += coeffs[19] * (long)output[i + 6];
				sum += coeffs[18] * (long)output[i + 7];
				sum += coeffs[17] * (long)output[i + 8];
				sum += coeffs[16] * (long)output[i + 9];
				sum += coeffs[15] * (long)output[i + 10];
				sum += coeffs[14] * (long)output[i + 11];
				sum += coeffs[13] * (long)output[i + 12];
				sum += coeffs[12] * (long)output[i + 13];
				sum += coeffs[11] * (long)output[i + 14];
				sum += coeffs[10] * (long)output[i + 15];
				sum += coeffs[9] * (long)output[i + 16];
				sum += coeffs[8] * (long)output[i + 17];
				sum += coeffs[7] * (long)output[i + 18];
				sum += coeffs[6] * (long)output[i + 19];
				sum += coeffs[5] * (long)output[i + 20];
				sum += coeffs[4] * (long)output[i + 21];
				sum += coeffs[3] * (long)output[i + 22];
				sum += coeffs[2] * (long)output[i + 23];
				sum += coeffs[1] * (long)output[i + 24];
				sum += coeffs[0] * (long)output[i + 25];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = (int)sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder27Wide(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 27;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            long sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[26] * (long)output[i + 0];
				sum += coeffs[25] * (long)output[i + 1];
				sum += coeffs[24] * (long)output[i + 2];
				sum += coeffs[23] * (long)output[i + 3];
				sum += coeffs[22] * (long)output[i + 4];
				sum += coeffs[21] * (long)output[i + 5];
				sum += coeffs[20] * (long)output[i + 6];
				sum += coeffs[19] * (long)output[i + 7];
				sum += coeffs[18] * (long)output[i + 8];
				sum += coeffs[17] * (long)output[i + 9];
				sum += coeffs[16] * (long)output[i + 10];
				sum += coeffs[15] * (long)output[i + 11];
				sum += coeffs[14] * (long)output[i + 12];
				sum += coeffs[13] * (long)output[i + 13];
				sum += coeffs[12] * (long)output[i + 14];
				sum += coeffs[11] * (long)output[i + 15];
				sum += coeffs[10] * (long)output[i + 16];
				sum += coeffs[9] * (long)output[i + 17];
				sum += coeffs[8] * (long)output[i + 18];
				sum += coeffs[7] * (long)output[i + 19];
				sum += coeffs[6] * (long)output[i + 20];
				sum += coeffs[5] * (long)output[i + 21];
				sum += coeffs[4] * (long)output[i + 22];
				sum += coeffs[3] * (long)output[i + 23];
				sum += coeffs[2] * (long)output[i + 24];
				sum += coeffs[1] * (long)output[i + 25];
				sum += coeffs[0] * (long)output[i + 26];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = (int)sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder28Wide(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 28;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            long sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[27] * (long)output[i + 0];
				sum += coeffs[26] * (long)output[i + 1];
				sum += coeffs[25] * (long)output[i + 2];
				sum += coeffs[24] * (long)output[i + 3];
				sum += coeffs[23] * (long)output[i + 4];
				sum += coeffs[22] * (long)output[i + 5];
				sum += coeffs[21] * (long)output[i + 6];
				sum += coeffs[20] * (long)output[i + 7];
				sum += coeffs[19] * (long)output[i + 8];
				sum += coeffs[18] * (long)output[i + 9];
				sum += coeffs[17] * (long)output[i + 10];
				sum += coeffs[16] * (long)output[i + 11];
				sum += coeffs[15] * (long)output[i + 12];
				sum += coeffs[14] * (long)output[i + 13];
				sum += coeffs[13] * (long)output[i + 14];
				sum += coeffs[12] * (long)output[i + 15];
				sum += coeffs[11] * (long)output[i + 16];
				sum += coeffs[10] * (long)output[i + 17];
				sum += coeffs[9] * (long)output[i + 18];
				sum += coeffs[8] * (long)output[i + 19];
				sum += coeffs[7] * (long)output[i + 20];
				sum += coeffs[6] * (long)output[i + 21];
				sum += coeffs[5] * (long)output[i + 22];
				sum += coeffs[4] * (long)output[i + 23];
				sum += coeffs[3] * (long)output[i + 24];
				sum += coeffs[2] * (long)output[i + 25];
				sum += coeffs[1] * (long)output[i + 26];
				sum += coeffs[0] * (long)output[i + 27];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = (int)sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder29Wide(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 29;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            long sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[28] * (long)output[i + 0];
				sum += coeffs[27] * (long)output[i + 1];
				sum += coeffs[26] * (long)output[i + 2];
				sum += coeffs[25] * (long)output[i + 3];
				sum += coeffs[24] * (long)output[i + 4];
				sum += coeffs[23] * (long)output[i + 5];
				sum += coeffs[22] * (long)output[i + 6];
				sum += coeffs[21] * (long)output[i + 7];
				sum += coeffs[20] * (long)output[i + 8];
				sum += coeffs[19] * (long)output[i + 9];
				sum += coeffs[18] * (long)output[i + 10];
				sum += coeffs[17] * (long)output[i + 11];
				sum += coeffs[16] * (long)output[i + 12];
				sum += coeffs[15] * (long)output[i + 13];
				sum += coeffs[14] * (long)output[i + 14];
				sum += coeffs[13] * (long)output[i + 15];
				sum += coeffs[12] * (long)output[i + 16];
				sum += coeffs[11] * (long)output[i + 17];
				sum += coeffs[10] * (long)output[i + 18];
				sum += coeffs[9] * (long)output[i + 19];
				sum += coeffs[8] * (long)output[i + 20];
				sum += coeffs[7] * (long)output[i + 21];
				sum += coeffs[6] * (long)output[i + 22];
				sum += coeffs[5] * (long)output[i + 23];
				sum += coeffs[4] * (long)output[i + 24];
				sum += coeffs[3] * (long)output[i + 25];
				sum += coeffs[2] * (long)output[i + 26];
				sum += coeffs[1] * (long)output[i + 27];
				sum += coeffs[0] * (long)output[i + 28];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = (int)sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder30Wide(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 30;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            long sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[29] * (long)output[i + 0];
				sum += coeffs[28] * (long)output[i + 1];
				sum += coeffs[27] * (long)output[i + 2];
				sum += coeffs[26] * (long)output[i + 3];
				sum += coeffs[25] * (long)output[i + 4];
				sum += coeffs[24] * (long)output[i + 5];
				sum += coeffs[23] * (long)output[i + 6];
				sum += coeffs[22] * (long)output[i + 7];
				sum += coeffs[21] * (long)output[i + 8];
				sum += coeffs[20] * (long)output[i + 9];
				sum += coeffs[19] * (long)output[i + 10];
				sum += coeffs[18] * (long)output[i + 11];
				sum += coeffs[17] * (long)output[i + 12];
				sum += coeffs[16] * (long)output[i + 13];
				sum += coeffs[15] * (long)output[i + 14];
				sum += coeffs[14] * (long)output[i + 15];
				sum += coeffs[13] * (long)output[i + 16];
				sum += coeffs[12] * (long)output[i + 17];
				sum += coeffs[11] * (long)output[i + 18];
				sum += coeffs[10] * (long)output[i + 19];
				sum += coeffs[9] * (long)output[i + 20];
				sum += coeffs[8] * (long)output[i + 21];
				sum += coeffs[7] * (long)output[i + 22];
				sum += coeffs[6] * (long)output[i + 23];
				sum += coeffs[5] * (long)output[i + 24];
				sum += coeffs[4] * (long)output[i + 25];
				sum += coeffs[3] * (long)output[i + 26];
				sum += coeffs[2] * (long)output[i + 27];
				sum += coeffs[1] * (long)output[i + 28];
				sum += coeffs[0] * (long)output[i + 29];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = (int)sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder31Wide(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 31;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            long sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[30] * (long)output[i + 0];
				sum += coeffs[29] * (long)output[i + 1];
				sum += coeffs[28] * (long)output[i + 2];
				sum += coeffs[27] * (long)output[i + 3];
				sum += coeffs[26] * (long)output[i + 4];
				sum += coeffs[25] * (long)output[i + 5];
				sum += coeffs[24] * (long)output[i + 6];
				sum += coeffs[23] * (long)output[i + 7];
				sum += coeffs[22] * (long)output[i + 8];
				sum += coeffs[21] * (long)output[i + 9];
				sum += coeffs[20] * (long)output[i + 10];
				sum += coeffs[19] * (long)output[i + 11];
				sum += coeffs[18] * (long)output[i + 12];
				sum += coeffs[17] * (long)output[i + 13];
				sum += coeffs[16] * (long)output[i + 14];
				sum += coeffs[15] * (long)output[i + 15];
				sum += coeffs[14] * (long)output[i + 16];
				sum += coeffs[13] * (long)output[i + 17];
				sum += coeffs[12] * (long)output[i + 18];
				sum += coeffs[11] * (long)output[i + 19];
				sum += coeffs[10] * (long)output[i + 20];
				sum += coeffs[9] * (long)output[i + 21];
				sum += coeffs[8] * (long)output[i + 22];
				sum += coeffs[7] * (long)output[i + 23];
				sum += coeffs[6] * (long)output[i + 24];
				sum += coeffs[5] * (long)output[i + 25];
				sum += coeffs[4] * (long)output[i + 26];
				sum += coeffs[3] * (long)output[i + 27];
				sum += coeffs[2] * (long)output[i + 28];
				sum += coeffs[1] * (long)output[i + 29];
				sum += coeffs[0] * (long)output[i + 30];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = (int)sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder32Wide(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 32;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            long sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[31] * (long)output[i + 0];
				sum += coeffs[30] * (long)output[i + 1];
				sum += coeffs[29] * (long)output[i + 2];
				sum += coeffs[28] * (long)output[i + 3];
				sum += coeffs[27] * (long)output[i + 4];
				sum += coeffs[26] * (long)output[i + 5];
				sum += coeffs[25] * (long)output[i + 6];
				sum += coeffs[24] * (long)output[i + 7];
				sum += coeffs[23] * (long)output[i + 8];
				sum += coeffs[22] * (long)output[i + 9];
				sum += coeffs[21] * (long)output[i + 10];
				sum += coeffs[20] * (long)output[i + 11];
				sum += coeffs[19] * (long)output[i + 12];
				sum += coeffs[18] * (long)output[i + 13];
				sum += coeffs[17] * (long)output[i + 14];
				sum += coeffs[16] * (long)output[i + 15];
				sum += coeffs[15] * (long)output[i + 16];
				sum += coeffs[14] * (long)output[i + 17];
				sum += coeffs[13] * (long)output[i + 18];
				sum += coeffs[12] * (long)output[i + 19];
				sum += coeffs[11] * (long)output[i + 20];
				sum += coeffs[10] * (long)output[i + 21];
				sum += coeffs[9] * (long)output[i + 22];
				sum += coeffs[8] * (long)output[i + 23];
				sum += coeffs[7] * (long)output[i + 24];
				sum += coeffs[6] * (long)output[i + 25];
				sum += coeffs[5] * (long)output[i + 26];
				sum += coeffs[4] * (long)output[i + 27];
				sum += coeffs[3] * (long)output[i + 28];
				sum += coeffs[2] * (long)output[i + 29];
				sum += coeffs[1] * (long)output[i + 30];
				sum += coeffs[0] * (long)output[i + 31];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = (int)sum;
            }
        }
    }
}
