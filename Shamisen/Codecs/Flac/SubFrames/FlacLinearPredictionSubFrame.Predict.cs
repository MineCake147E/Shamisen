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
				sum += coeff1 * prev1;
				prev1 = prev0;
				sum += coeff0 * prev0;
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = sum;
                prev0 = sum;
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
				sum += coeff2 * prev2;
				prev2 = prev1;
				sum += coeff1 * prev1;
				prev1 = prev0;
				sum += coeff0 * prev0;
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = sum;
                prev0 = sum;
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
				sum += coeff3 * prev3;
				prev3 = prev2;
				sum += coeff2 * prev2;
				prev2 = prev1;
				sum += coeff1 * prev1;
				prev1 = prev0;
				sum += coeff0 * prev0;
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = sum;
                prev0 = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder5(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 5;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            Span<int> prev = stackalloc int[Order];
            output.SliceWhile(Order).CopyTo(prev);
            _ = prev[Order - 1];
            int sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[4] * prev[4];
				sum += coeffs[3] * prev[3];
				sum += coeffs[2] * prev[2];
				sum += coeffs[1] * prev[1];
				sum += coeffs[0] * prev[0];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = sum;
				prev[4] = prev[3];
				prev[3] = prev[2];
				prev[2] = prev[1];
				prev[1] = prev[0];
                prev[0] = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder6(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 6;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            Span<int> prev = stackalloc int[Order];
            output.SliceWhile(Order).CopyTo(prev);
            _ = prev[Order - 1];
            int sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[5] * prev[5];
				sum += coeffs[4] * prev[4];
				sum += coeffs[3] * prev[3];
				sum += coeffs[2] * prev[2];
				sum += coeffs[1] * prev[1];
				sum += coeffs[0] * prev[0];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = sum;
				prev[5] = prev[4];
				prev[4] = prev[3];
				prev[3] = prev[2];
				prev[2] = prev[1];
				prev[1] = prev[0];
                prev[0] = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder7(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 7;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            Span<int> prev = stackalloc int[Order];
            output.SliceWhile(Order).CopyTo(prev);
            _ = prev[Order - 1];
            int sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[6] * prev[6];
				sum += coeffs[5] * prev[5];
				sum += coeffs[4] * prev[4];
				sum += coeffs[3] * prev[3];
				sum += coeffs[2] * prev[2];
				sum += coeffs[1] * prev[1];
				sum += coeffs[0] * prev[0];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = sum;
				prev[6] = prev[5];
				prev[5] = prev[4];
				prev[4] = prev[3];
				prev[3] = prev[2];
				prev[2] = prev[1];
				prev[1] = prev[0];
                prev[0] = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder8(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 8;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            Span<int> prev = stackalloc int[Order];
            output.SliceWhile(Order).CopyTo(prev);
            _ = prev[Order - 1];
            int sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[7] * prev[7];
				sum += coeffs[6] * prev[6];
				sum += coeffs[5] * prev[5];
				sum += coeffs[4] * prev[4];
				sum += coeffs[3] * prev[3];
				sum += coeffs[2] * prev[2];
				sum += coeffs[1] * prev[1];
				sum += coeffs[0] * prev[0];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = sum;
				prev[7] = prev[6];
				prev[6] = prev[5];
				prev[5] = prev[4];
				prev[4] = prev[3];
				prev[3] = prev[2];
				prev[2] = prev[1];
				prev[1] = prev[0];
                prev[0] = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder9(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 9;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            Span<int> prev = stackalloc int[Order];
            output.SliceWhile(Order).CopyTo(prev);
            _ = prev[Order - 1];
            int sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[8] * prev[8];
				sum += coeffs[7] * prev[7];
				sum += coeffs[6] * prev[6];
				sum += coeffs[5] * prev[5];
				sum += coeffs[4] * prev[4];
				sum += coeffs[3] * prev[3];
				sum += coeffs[2] * prev[2];
				sum += coeffs[1] * prev[1];
				sum += coeffs[0] * prev[0];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = sum;
				prev[8] = prev[7];
				prev[7] = prev[6];
				prev[6] = prev[5];
				prev[5] = prev[4];
				prev[4] = prev[3];
				prev[3] = prev[2];
				prev[2] = prev[1];
				prev[1] = prev[0];
                prev[0] = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder10(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 10;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            Span<int> prev = stackalloc int[Order];
            output.SliceWhile(Order).CopyTo(prev);
            _ = prev[Order - 1];
            int sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[9] * prev[9];
				sum += coeffs[8] * prev[8];
				sum += coeffs[7] * prev[7];
				sum += coeffs[6] * prev[6];
				sum += coeffs[5] * prev[5];
				sum += coeffs[4] * prev[4];
				sum += coeffs[3] * prev[3];
				sum += coeffs[2] * prev[2];
				sum += coeffs[1] * prev[1];
				sum += coeffs[0] * prev[0];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = sum;
				prev[9] = prev[8];
				prev[8] = prev[7];
				prev[7] = prev[6];
				prev[6] = prev[5];
				prev[5] = prev[4];
				prev[4] = prev[3];
				prev[3] = prev[2];
				prev[2] = prev[1];
				prev[1] = prev[0];
                prev[0] = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder11(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 11;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            Span<int> prev = stackalloc int[Order];
            output.SliceWhile(Order).CopyTo(prev);
            _ = prev[Order - 1];
            int sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[10] * prev[10];
				sum += coeffs[9] * prev[9];
				sum += coeffs[8] * prev[8];
				sum += coeffs[7] * prev[7];
				sum += coeffs[6] * prev[6];
				sum += coeffs[5] * prev[5];
				sum += coeffs[4] * prev[4];
				sum += coeffs[3] * prev[3];
				sum += coeffs[2] * prev[2];
				sum += coeffs[1] * prev[1];
				sum += coeffs[0] * prev[0];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = sum;
				prev[10] = prev[9];
				prev[9] = prev[8];
				prev[8] = prev[7];
				prev[7] = prev[6];
				prev[6] = prev[5];
				prev[5] = prev[4];
				prev[4] = prev[3];
				prev[3] = prev[2];
				prev[2] = prev[1];
				prev[1] = prev[0];
                prev[0] = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder12(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 12;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            Span<int> prev = stackalloc int[Order];
            output.SliceWhile(Order).CopyTo(prev);
            _ = prev[Order - 1];
            int sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[11] * prev[11];
				sum += coeffs[10] * prev[10];
				sum += coeffs[9] * prev[9];
				sum += coeffs[8] * prev[8];
				sum += coeffs[7] * prev[7];
				sum += coeffs[6] * prev[6];
				sum += coeffs[5] * prev[5];
				sum += coeffs[4] * prev[4];
				sum += coeffs[3] * prev[3];
				sum += coeffs[2] * prev[2];
				sum += coeffs[1] * prev[1];
				sum += coeffs[0] * prev[0];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = sum;
				prev[11] = prev[10];
				prev[10] = prev[9];
				prev[9] = prev[8];
				prev[8] = prev[7];
				prev[7] = prev[6];
				prev[6] = prev[5];
				prev[5] = prev[4];
				prev[4] = prev[3];
				prev[3] = prev[2];
				prev[2] = prev[1];
				prev[1] = prev[0];
                prev[0] = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder13(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 13;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            Span<int> prev = stackalloc int[Order];
            output.SliceWhile(Order).CopyTo(prev);
            _ = prev[Order - 1];
            int sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[12] * prev[12];
				sum += coeffs[11] * prev[11];
				sum += coeffs[10] * prev[10];
				sum += coeffs[9] * prev[9];
				sum += coeffs[8] * prev[8];
				sum += coeffs[7] * prev[7];
				sum += coeffs[6] * prev[6];
				sum += coeffs[5] * prev[5];
				sum += coeffs[4] * prev[4];
				sum += coeffs[3] * prev[3];
				sum += coeffs[2] * prev[2];
				sum += coeffs[1] * prev[1];
				sum += coeffs[0] * prev[0];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = sum;
				prev[12] = prev[11];
				prev[11] = prev[10];
				prev[10] = prev[9];
				prev[9] = prev[8];
				prev[8] = prev[7];
				prev[7] = prev[6];
				prev[6] = prev[5];
				prev[5] = prev[4];
				prev[4] = prev[3];
				prev[3] = prev[2];
				prev[2] = prev[1];
				prev[1] = prev[0];
                prev[0] = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder14(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 14;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            Span<int> prev = stackalloc int[Order];
            output.SliceWhile(Order).CopyTo(prev);
            _ = prev[Order - 1];
            int sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[13] * prev[13];
				sum += coeffs[12] * prev[12];
				sum += coeffs[11] * prev[11];
				sum += coeffs[10] * prev[10];
				sum += coeffs[9] * prev[9];
				sum += coeffs[8] * prev[8];
				sum += coeffs[7] * prev[7];
				sum += coeffs[6] * prev[6];
				sum += coeffs[5] * prev[5];
				sum += coeffs[4] * prev[4];
				sum += coeffs[3] * prev[3];
				sum += coeffs[2] * prev[2];
				sum += coeffs[1] * prev[1];
				sum += coeffs[0] * prev[0];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = sum;
				prev[13] = prev[12];
				prev[12] = prev[11];
				prev[11] = prev[10];
				prev[10] = prev[9];
				prev[9] = prev[8];
				prev[8] = prev[7];
				prev[7] = prev[6];
				prev[6] = prev[5];
				prev[5] = prev[4];
				prev[4] = prev[3];
				prev[3] = prev[2];
				prev[2] = prev[1];
				prev[1] = prev[0];
                prev[0] = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder15(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 15;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            Span<int> prev = stackalloc int[Order];
            output.SliceWhile(Order).CopyTo(prev);
            _ = prev[Order - 1];
            int sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[14] * prev[14];
				sum += coeffs[13] * prev[13];
				sum += coeffs[12] * prev[12];
				sum += coeffs[11] * prev[11];
				sum += coeffs[10] * prev[10];
				sum += coeffs[9] * prev[9];
				sum += coeffs[8] * prev[8];
				sum += coeffs[7] * prev[7];
				sum += coeffs[6] * prev[6];
				sum += coeffs[5] * prev[5];
				sum += coeffs[4] * prev[4];
				sum += coeffs[3] * prev[3];
				sum += coeffs[2] * prev[2];
				sum += coeffs[1] * prev[1];
				sum += coeffs[0] * prev[0];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = sum;
				prev[14] = prev[13];
				prev[13] = prev[12];
				prev[12] = prev[11];
				prev[11] = prev[10];
				prev[10] = prev[9];
				prev[9] = prev[8];
				prev[8] = prev[7];
				prev[7] = prev[6];
				prev[6] = prev[5];
				prev[5] = prev[4];
				prev[4] = prev[3];
				prev[3] = prev[2];
				prev[2] = prev[1];
				prev[1] = prev[0];
                prev[0] = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder16(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 16;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            Span<int> prev = stackalloc int[Order];
            output.SliceWhile(Order).CopyTo(prev);
            _ = prev[Order - 1];
            int sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[15] * prev[15];
				sum += coeffs[14] * prev[14];
				sum += coeffs[13] * prev[13];
				sum += coeffs[12] * prev[12];
				sum += coeffs[11] * prev[11];
				sum += coeffs[10] * prev[10];
				sum += coeffs[9] * prev[9];
				sum += coeffs[8] * prev[8];
				sum += coeffs[7] * prev[7];
				sum += coeffs[6] * prev[6];
				sum += coeffs[5] * prev[5];
				sum += coeffs[4] * prev[4];
				sum += coeffs[3] * prev[3];
				sum += coeffs[2] * prev[2];
				sum += coeffs[1] * prev[1];
				sum += coeffs[0] * prev[0];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = sum;
				prev[15] = prev[14];
				prev[14] = prev[13];
				prev[13] = prev[12];
				prev[12] = prev[11];
				prev[11] = prev[10];
				prev[10] = prev[9];
				prev[9] = prev[8];
				prev[8] = prev[7];
				prev[7] = prev[6];
				prev[6] = prev[5];
				prev[5] = prev[4];
				prev[4] = prev[3];
				prev[3] = prev[2];
				prev[2] = prev[1];
				prev[1] = prev[0];
                prev[0] = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder17(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 17;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            Span<int> prev = stackalloc int[Order];
            output.SliceWhile(Order).CopyTo(prev);
            _ = prev[Order - 1];
            int sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[16] * prev[16];
				sum += coeffs[15] * prev[15];
				sum += coeffs[14] * prev[14];
				sum += coeffs[13] * prev[13];
				sum += coeffs[12] * prev[12];
				sum += coeffs[11] * prev[11];
				sum += coeffs[10] * prev[10];
				sum += coeffs[9] * prev[9];
				sum += coeffs[8] * prev[8];
				sum += coeffs[7] * prev[7];
				sum += coeffs[6] * prev[6];
				sum += coeffs[5] * prev[5];
				sum += coeffs[4] * prev[4];
				sum += coeffs[3] * prev[3];
				sum += coeffs[2] * prev[2];
				sum += coeffs[1] * prev[1];
				sum += coeffs[0] * prev[0];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = sum;
				prev[16] = prev[15];
				prev[15] = prev[14];
				prev[14] = prev[13];
				prev[13] = prev[12];
				prev[12] = prev[11];
				prev[11] = prev[10];
				prev[10] = prev[9];
				prev[9] = prev[8];
				prev[8] = prev[7];
				prev[7] = prev[6];
				prev[6] = prev[5];
				prev[5] = prev[4];
				prev[4] = prev[3];
				prev[3] = prev[2];
				prev[2] = prev[1];
				prev[1] = prev[0];
                prev[0] = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder18(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 18;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            Span<int> prev = stackalloc int[Order];
            output.SliceWhile(Order).CopyTo(prev);
            _ = prev[Order - 1];
            int sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[17] * prev[17];
				sum += coeffs[16] * prev[16];
				sum += coeffs[15] * prev[15];
				sum += coeffs[14] * prev[14];
				sum += coeffs[13] * prev[13];
				sum += coeffs[12] * prev[12];
				sum += coeffs[11] * prev[11];
				sum += coeffs[10] * prev[10];
				sum += coeffs[9] * prev[9];
				sum += coeffs[8] * prev[8];
				sum += coeffs[7] * prev[7];
				sum += coeffs[6] * prev[6];
				sum += coeffs[5] * prev[5];
				sum += coeffs[4] * prev[4];
				sum += coeffs[3] * prev[3];
				sum += coeffs[2] * prev[2];
				sum += coeffs[1] * prev[1];
				sum += coeffs[0] * prev[0];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = sum;
				prev[17] = prev[16];
				prev[16] = prev[15];
				prev[15] = prev[14];
				prev[14] = prev[13];
				prev[13] = prev[12];
				prev[12] = prev[11];
				prev[11] = prev[10];
				prev[10] = prev[9];
				prev[9] = prev[8];
				prev[8] = prev[7];
				prev[7] = prev[6];
				prev[6] = prev[5];
				prev[5] = prev[4];
				prev[4] = prev[3];
				prev[3] = prev[2];
				prev[2] = prev[1];
				prev[1] = prev[0];
                prev[0] = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder19(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 19;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            Span<int> prev = stackalloc int[Order];
            output.SliceWhile(Order).CopyTo(prev);
            _ = prev[Order - 1];
            int sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[18] * prev[18];
				sum += coeffs[17] * prev[17];
				sum += coeffs[16] * prev[16];
				sum += coeffs[15] * prev[15];
				sum += coeffs[14] * prev[14];
				sum += coeffs[13] * prev[13];
				sum += coeffs[12] * prev[12];
				sum += coeffs[11] * prev[11];
				sum += coeffs[10] * prev[10];
				sum += coeffs[9] * prev[9];
				sum += coeffs[8] * prev[8];
				sum += coeffs[7] * prev[7];
				sum += coeffs[6] * prev[6];
				sum += coeffs[5] * prev[5];
				sum += coeffs[4] * prev[4];
				sum += coeffs[3] * prev[3];
				sum += coeffs[2] * prev[2];
				sum += coeffs[1] * prev[1];
				sum += coeffs[0] * prev[0];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = sum;
				prev[18] = prev[17];
				prev[17] = prev[16];
				prev[16] = prev[15];
				prev[15] = prev[14];
				prev[14] = prev[13];
				prev[13] = prev[12];
				prev[12] = prev[11];
				prev[11] = prev[10];
				prev[10] = prev[9];
				prev[9] = prev[8];
				prev[8] = prev[7];
				prev[7] = prev[6];
				prev[6] = prev[5];
				prev[5] = prev[4];
				prev[4] = prev[3];
				prev[3] = prev[2];
				prev[2] = prev[1];
				prev[1] = prev[0];
                prev[0] = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder20(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 20;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            Span<int> prev = stackalloc int[Order];
            output.SliceWhile(Order).CopyTo(prev);
            _ = prev[Order - 1];
            int sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[19] * prev[19];
				sum += coeffs[18] * prev[18];
				sum += coeffs[17] * prev[17];
				sum += coeffs[16] * prev[16];
				sum += coeffs[15] * prev[15];
				sum += coeffs[14] * prev[14];
				sum += coeffs[13] * prev[13];
				sum += coeffs[12] * prev[12];
				sum += coeffs[11] * prev[11];
				sum += coeffs[10] * prev[10];
				sum += coeffs[9] * prev[9];
				sum += coeffs[8] * prev[8];
				sum += coeffs[7] * prev[7];
				sum += coeffs[6] * prev[6];
				sum += coeffs[5] * prev[5];
				sum += coeffs[4] * prev[4];
				sum += coeffs[3] * prev[3];
				sum += coeffs[2] * prev[2];
				sum += coeffs[1] * prev[1];
				sum += coeffs[0] * prev[0];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = sum;
				prev[19] = prev[18];
				prev[18] = prev[17];
				prev[17] = prev[16];
				prev[16] = prev[15];
				prev[15] = prev[14];
				prev[14] = prev[13];
				prev[13] = prev[12];
				prev[12] = prev[11];
				prev[11] = prev[10];
				prev[10] = prev[9];
				prev[9] = prev[8];
				prev[8] = prev[7];
				prev[7] = prev[6];
				prev[6] = prev[5];
				prev[5] = prev[4];
				prev[4] = prev[3];
				prev[3] = prev[2];
				prev[2] = prev[1];
				prev[1] = prev[0];
                prev[0] = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder21(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 21;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            Span<int> prev = stackalloc int[Order];
            output.SliceWhile(Order).CopyTo(prev);
            _ = prev[Order - 1];
            int sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[20] * prev[20];
				sum += coeffs[19] * prev[19];
				sum += coeffs[18] * prev[18];
				sum += coeffs[17] * prev[17];
				sum += coeffs[16] * prev[16];
				sum += coeffs[15] * prev[15];
				sum += coeffs[14] * prev[14];
				sum += coeffs[13] * prev[13];
				sum += coeffs[12] * prev[12];
				sum += coeffs[11] * prev[11];
				sum += coeffs[10] * prev[10];
				sum += coeffs[9] * prev[9];
				sum += coeffs[8] * prev[8];
				sum += coeffs[7] * prev[7];
				sum += coeffs[6] * prev[6];
				sum += coeffs[5] * prev[5];
				sum += coeffs[4] * prev[4];
				sum += coeffs[3] * prev[3];
				sum += coeffs[2] * prev[2];
				sum += coeffs[1] * prev[1];
				sum += coeffs[0] * prev[0];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = sum;
				prev[20] = prev[19];
				prev[19] = prev[18];
				prev[18] = prev[17];
				prev[17] = prev[16];
				prev[16] = prev[15];
				prev[15] = prev[14];
				prev[14] = prev[13];
				prev[13] = prev[12];
				prev[12] = prev[11];
				prev[11] = prev[10];
				prev[10] = prev[9];
				prev[9] = prev[8];
				prev[8] = prev[7];
				prev[7] = prev[6];
				prev[6] = prev[5];
				prev[5] = prev[4];
				prev[4] = prev[3];
				prev[3] = prev[2];
				prev[2] = prev[1];
				prev[1] = prev[0];
                prev[0] = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder22(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 22;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            Span<int> prev = stackalloc int[Order];
            output.SliceWhile(Order).CopyTo(prev);
            _ = prev[Order - 1];
            int sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[21] * prev[21];
				sum += coeffs[20] * prev[20];
				sum += coeffs[19] * prev[19];
				sum += coeffs[18] * prev[18];
				sum += coeffs[17] * prev[17];
				sum += coeffs[16] * prev[16];
				sum += coeffs[15] * prev[15];
				sum += coeffs[14] * prev[14];
				sum += coeffs[13] * prev[13];
				sum += coeffs[12] * prev[12];
				sum += coeffs[11] * prev[11];
				sum += coeffs[10] * prev[10];
				sum += coeffs[9] * prev[9];
				sum += coeffs[8] * prev[8];
				sum += coeffs[7] * prev[7];
				sum += coeffs[6] * prev[6];
				sum += coeffs[5] * prev[5];
				sum += coeffs[4] * prev[4];
				sum += coeffs[3] * prev[3];
				sum += coeffs[2] * prev[2];
				sum += coeffs[1] * prev[1];
				sum += coeffs[0] * prev[0];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = sum;
				prev[21] = prev[20];
				prev[20] = prev[19];
				prev[19] = prev[18];
				prev[18] = prev[17];
				prev[17] = prev[16];
				prev[16] = prev[15];
				prev[15] = prev[14];
				prev[14] = prev[13];
				prev[13] = prev[12];
				prev[12] = prev[11];
				prev[11] = prev[10];
				prev[10] = prev[9];
				prev[9] = prev[8];
				prev[8] = prev[7];
				prev[7] = prev[6];
				prev[6] = prev[5];
				prev[5] = prev[4];
				prev[4] = prev[3];
				prev[3] = prev[2];
				prev[2] = prev[1];
				prev[1] = prev[0];
                prev[0] = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder23(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 23;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            Span<int> prev = stackalloc int[Order];
            output.SliceWhile(Order).CopyTo(prev);
            _ = prev[Order - 1];
            int sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[22] * prev[22];
				sum += coeffs[21] * prev[21];
				sum += coeffs[20] * prev[20];
				sum += coeffs[19] * prev[19];
				sum += coeffs[18] * prev[18];
				sum += coeffs[17] * prev[17];
				sum += coeffs[16] * prev[16];
				sum += coeffs[15] * prev[15];
				sum += coeffs[14] * prev[14];
				sum += coeffs[13] * prev[13];
				sum += coeffs[12] * prev[12];
				sum += coeffs[11] * prev[11];
				sum += coeffs[10] * prev[10];
				sum += coeffs[9] * prev[9];
				sum += coeffs[8] * prev[8];
				sum += coeffs[7] * prev[7];
				sum += coeffs[6] * prev[6];
				sum += coeffs[5] * prev[5];
				sum += coeffs[4] * prev[4];
				sum += coeffs[3] * prev[3];
				sum += coeffs[2] * prev[2];
				sum += coeffs[1] * prev[1];
				sum += coeffs[0] * prev[0];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = sum;
				prev[22] = prev[21];
				prev[21] = prev[20];
				prev[20] = prev[19];
				prev[19] = prev[18];
				prev[18] = prev[17];
				prev[17] = prev[16];
				prev[16] = prev[15];
				prev[15] = prev[14];
				prev[14] = prev[13];
				prev[13] = prev[12];
				prev[12] = prev[11];
				prev[11] = prev[10];
				prev[10] = prev[9];
				prev[9] = prev[8];
				prev[8] = prev[7];
				prev[7] = prev[6];
				prev[6] = prev[5];
				prev[5] = prev[4];
				prev[4] = prev[3];
				prev[3] = prev[2];
				prev[2] = prev[1];
				prev[1] = prev[0];
                prev[0] = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder24(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 24;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            Span<int> prev = stackalloc int[Order];
            output.SliceWhile(Order).CopyTo(prev);
            _ = prev[Order - 1];
            int sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[23] * prev[23];
				sum += coeffs[22] * prev[22];
				sum += coeffs[21] * prev[21];
				sum += coeffs[20] * prev[20];
				sum += coeffs[19] * prev[19];
				sum += coeffs[18] * prev[18];
				sum += coeffs[17] * prev[17];
				sum += coeffs[16] * prev[16];
				sum += coeffs[15] * prev[15];
				sum += coeffs[14] * prev[14];
				sum += coeffs[13] * prev[13];
				sum += coeffs[12] * prev[12];
				sum += coeffs[11] * prev[11];
				sum += coeffs[10] * prev[10];
				sum += coeffs[9] * prev[9];
				sum += coeffs[8] * prev[8];
				sum += coeffs[7] * prev[7];
				sum += coeffs[6] * prev[6];
				sum += coeffs[5] * prev[5];
				sum += coeffs[4] * prev[4];
				sum += coeffs[3] * prev[3];
				sum += coeffs[2] * prev[2];
				sum += coeffs[1] * prev[1];
				sum += coeffs[0] * prev[0];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = sum;
				prev[23] = prev[22];
				prev[22] = prev[21];
				prev[21] = prev[20];
				prev[20] = prev[19];
				prev[19] = prev[18];
				prev[18] = prev[17];
				prev[17] = prev[16];
				prev[16] = prev[15];
				prev[15] = prev[14];
				prev[14] = prev[13];
				prev[13] = prev[12];
				prev[12] = prev[11];
				prev[11] = prev[10];
				prev[10] = prev[9];
				prev[9] = prev[8];
				prev[8] = prev[7];
				prev[7] = prev[6];
				prev[6] = prev[5];
				prev[5] = prev[4];
				prev[4] = prev[3];
				prev[3] = prev[2];
				prev[2] = prev[1];
				prev[1] = prev[0];
                prev[0] = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder25(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 25;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            Span<int> prev = stackalloc int[Order];
            output.SliceWhile(Order).CopyTo(prev);
            _ = prev[Order - 1];
            int sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[24] * prev[24];
				sum += coeffs[23] * prev[23];
				sum += coeffs[22] * prev[22];
				sum += coeffs[21] * prev[21];
				sum += coeffs[20] * prev[20];
				sum += coeffs[19] * prev[19];
				sum += coeffs[18] * prev[18];
				sum += coeffs[17] * prev[17];
				sum += coeffs[16] * prev[16];
				sum += coeffs[15] * prev[15];
				sum += coeffs[14] * prev[14];
				sum += coeffs[13] * prev[13];
				sum += coeffs[12] * prev[12];
				sum += coeffs[11] * prev[11];
				sum += coeffs[10] * prev[10];
				sum += coeffs[9] * prev[9];
				sum += coeffs[8] * prev[8];
				sum += coeffs[7] * prev[7];
				sum += coeffs[6] * prev[6];
				sum += coeffs[5] * prev[5];
				sum += coeffs[4] * prev[4];
				sum += coeffs[3] * prev[3];
				sum += coeffs[2] * prev[2];
				sum += coeffs[1] * prev[1];
				sum += coeffs[0] * prev[0];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = sum;
				prev[24] = prev[23];
				prev[23] = prev[22];
				prev[22] = prev[21];
				prev[21] = prev[20];
				prev[20] = prev[19];
				prev[19] = prev[18];
				prev[18] = prev[17];
				prev[17] = prev[16];
				prev[16] = prev[15];
				prev[15] = prev[14];
				prev[14] = prev[13];
				prev[13] = prev[12];
				prev[12] = prev[11];
				prev[11] = prev[10];
				prev[10] = prev[9];
				prev[9] = prev[8];
				prev[8] = prev[7];
				prev[7] = prev[6];
				prev[6] = prev[5];
				prev[5] = prev[4];
				prev[4] = prev[3];
				prev[3] = prev[2];
				prev[2] = prev[1];
				prev[1] = prev[0];
                prev[0] = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder26(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 26;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            Span<int> prev = stackalloc int[Order];
            output.SliceWhile(Order).CopyTo(prev);
            _ = prev[Order - 1];
            int sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[25] * prev[25];
				sum += coeffs[24] * prev[24];
				sum += coeffs[23] * prev[23];
				sum += coeffs[22] * prev[22];
				sum += coeffs[21] * prev[21];
				sum += coeffs[20] * prev[20];
				sum += coeffs[19] * prev[19];
				sum += coeffs[18] * prev[18];
				sum += coeffs[17] * prev[17];
				sum += coeffs[16] * prev[16];
				sum += coeffs[15] * prev[15];
				sum += coeffs[14] * prev[14];
				sum += coeffs[13] * prev[13];
				sum += coeffs[12] * prev[12];
				sum += coeffs[11] * prev[11];
				sum += coeffs[10] * prev[10];
				sum += coeffs[9] * prev[9];
				sum += coeffs[8] * prev[8];
				sum += coeffs[7] * prev[7];
				sum += coeffs[6] * prev[6];
				sum += coeffs[5] * prev[5];
				sum += coeffs[4] * prev[4];
				sum += coeffs[3] * prev[3];
				sum += coeffs[2] * prev[2];
				sum += coeffs[1] * prev[1];
				sum += coeffs[0] * prev[0];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = sum;
				prev[25] = prev[24];
				prev[24] = prev[23];
				prev[23] = prev[22];
				prev[22] = prev[21];
				prev[21] = prev[20];
				prev[20] = prev[19];
				prev[19] = prev[18];
				prev[18] = prev[17];
				prev[17] = prev[16];
				prev[16] = prev[15];
				prev[15] = prev[14];
				prev[14] = prev[13];
				prev[13] = prev[12];
				prev[12] = prev[11];
				prev[11] = prev[10];
				prev[10] = prev[9];
				prev[9] = prev[8];
				prev[8] = prev[7];
				prev[7] = prev[6];
				prev[6] = prev[5];
				prev[5] = prev[4];
				prev[4] = prev[3];
				prev[3] = prev[2];
				prev[2] = prev[1];
				prev[1] = prev[0];
                prev[0] = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder27(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 27;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            Span<int> prev = stackalloc int[Order];
            output.SliceWhile(Order).CopyTo(prev);
            _ = prev[Order - 1];
            int sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[26] * prev[26];
				sum += coeffs[25] * prev[25];
				sum += coeffs[24] * prev[24];
				sum += coeffs[23] * prev[23];
				sum += coeffs[22] * prev[22];
				sum += coeffs[21] * prev[21];
				sum += coeffs[20] * prev[20];
				sum += coeffs[19] * prev[19];
				sum += coeffs[18] * prev[18];
				sum += coeffs[17] * prev[17];
				sum += coeffs[16] * prev[16];
				sum += coeffs[15] * prev[15];
				sum += coeffs[14] * prev[14];
				sum += coeffs[13] * prev[13];
				sum += coeffs[12] * prev[12];
				sum += coeffs[11] * prev[11];
				sum += coeffs[10] * prev[10];
				sum += coeffs[9] * prev[9];
				sum += coeffs[8] * prev[8];
				sum += coeffs[7] * prev[7];
				sum += coeffs[6] * prev[6];
				sum += coeffs[5] * prev[5];
				sum += coeffs[4] * prev[4];
				sum += coeffs[3] * prev[3];
				sum += coeffs[2] * prev[2];
				sum += coeffs[1] * prev[1];
				sum += coeffs[0] * prev[0];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = sum;
				prev[26] = prev[25];
				prev[25] = prev[24];
				prev[24] = prev[23];
				prev[23] = prev[22];
				prev[22] = prev[21];
				prev[21] = prev[20];
				prev[20] = prev[19];
				prev[19] = prev[18];
				prev[18] = prev[17];
				prev[17] = prev[16];
				prev[16] = prev[15];
				prev[15] = prev[14];
				prev[14] = prev[13];
				prev[13] = prev[12];
				prev[12] = prev[11];
				prev[11] = prev[10];
				prev[10] = prev[9];
				prev[9] = prev[8];
				prev[8] = prev[7];
				prev[7] = prev[6];
				prev[6] = prev[5];
				prev[5] = prev[4];
				prev[4] = prev[3];
				prev[3] = prev[2];
				prev[2] = prev[1];
				prev[1] = prev[0];
                prev[0] = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder28(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 28;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            Span<int> prev = stackalloc int[Order];
            output.SliceWhile(Order).CopyTo(prev);
            _ = prev[Order - 1];
            int sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[27] * prev[27];
				sum += coeffs[26] * prev[26];
				sum += coeffs[25] * prev[25];
				sum += coeffs[24] * prev[24];
				sum += coeffs[23] * prev[23];
				sum += coeffs[22] * prev[22];
				sum += coeffs[21] * prev[21];
				sum += coeffs[20] * prev[20];
				sum += coeffs[19] * prev[19];
				sum += coeffs[18] * prev[18];
				sum += coeffs[17] * prev[17];
				sum += coeffs[16] * prev[16];
				sum += coeffs[15] * prev[15];
				sum += coeffs[14] * prev[14];
				sum += coeffs[13] * prev[13];
				sum += coeffs[12] * prev[12];
				sum += coeffs[11] * prev[11];
				sum += coeffs[10] * prev[10];
				sum += coeffs[9] * prev[9];
				sum += coeffs[8] * prev[8];
				sum += coeffs[7] * prev[7];
				sum += coeffs[6] * prev[6];
				sum += coeffs[5] * prev[5];
				sum += coeffs[4] * prev[4];
				sum += coeffs[3] * prev[3];
				sum += coeffs[2] * prev[2];
				sum += coeffs[1] * prev[1];
				sum += coeffs[0] * prev[0];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = sum;
				prev[27] = prev[26];
				prev[26] = prev[25];
				prev[25] = prev[24];
				prev[24] = prev[23];
				prev[23] = prev[22];
				prev[22] = prev[21];
				prev[21] = prev[20];
				prev[20] = prev[19];
				prev[19] = prev[18];
				prev[18] = prev[17];
				prev[17] = prev[16];
				prev[16] = prev[15];
				prev[15] = prev[14];
				prev[14] = prev[13];
				prev[13] = prev[12];
				prev[12] = prev[11];
				prev[11] = prev[10];
				prev[10] = prev[9];
				prev[9] = prev[8];
				prev[8] = prev[7];
				prev[7] = prev[6];
				prev[6] = prev[5];
				prev[5] = prev[4];
				prev[4] = prev[3];
				prev[3] = prev[2];
				prev[2] = prev[1];
				prev[1] = prev[0];
                prev[0] = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder29(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 29;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            Span<int> prev = stackalloc int[Order];
            output.SliceWhile(Order).CopyTo(prev);
            _ = prev[Order - 1];
            int sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[28] * prev[28];
				sum += coeffs[27] * prev[27];
				sum += coeffs[26] * prev[26];
				sum += coeffs[25] * prev[25];
				sum += coeffs[24] * prev[24];
				sum += coeffs[23] * prev[23];
				sum += coeffs[22] * prev[22];
				sum += coeffs[21] * prev[21];
				sum += coeffs[20] * prev[20];
				sum += coeffs[19] * prev[19];
				sum += coeffs[18] * prev[18];
				sum += coeffs[17] * prev[17];
				sum += coeffs[16] * prev[16];
				sum += coeffs[15] * prev[15];
				sum += coeffs[14] * prev[14];
				sum += coeffs[13] * prev[13];
				sum += coeffs[12] * prev[12];
				sum += coeffs[11] * prev[11];
				sum += coeffs[10] * prev[10];
				sum += coeffs[9] * prev[9];
				sum += coeffs[8] * prev[8];
				sum += coeffs[7] * prev[7];
				sum += coeffs[6] * prev[6];
				sum += coeffs[5] * prev[5];
				sum += coeffs[4] * prev[4];
				sum += coeffs[3] * prev[3];
				sum += coeffs[2] * prev[2];
				sum += coeffs[1] * prev[1];
				sum += coeffs[0] * prev[0];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = sum;
				prev[28] = prev[27];
				prev[27] = prev[26];
				prev[26] = prev[25];
				prev[25] = prev[24];
				prev[24] = prev[23];
				prev[23] = prev[22];
				prev[22] = prev[21];
				prev[21] = prev[20];
				prev[20] = prev[19];
				prev[19] = prev[18];
				prev[18] = prev[17];
				prev[17] = prev[16];
				prev[16] = prev[15];
				prev[15] = prev[14];
				prev[14] = prev[13];
				prev[13] = prev[12];
				prev[12] = prev[11];
				prev[11] = prev[10];
				prev[10] = prev[9];
				prev[9] = prev[8];
				prev[8] = prev[7];
				prev[7] = prev[6];
				prev[6] = prev[5];
				prev[5] = prev[4];
				prev[4] = prev[3];
				prev[3] = prev[2];
				prev[2] = prev[1];
				prev[1] = prev[0];
                prev[0] = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder30(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 30;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            Span<int> prev = stackalloc int[Order];
            output.SliceWhile(Order).CopyTo(prev);
            _ = prev[Order - 1];
            int sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[29] * prev[29];
				sum += coeffs[28] * prev[28];
				sum += coeffs[27] * prev[27];
				sum += coeffs[26] * prev[26];
				sum += coeffs[25] * prev[25];
				sum += coeffs[24] * prev[24];
				sum += coeffs[23] * prev[23];
				sum += coeffs[22] * prev[22];
				sum += coeffs[21] * prev[21];
				sum += coeffs[20] * prev[20];
				sum += coeffs[19] * prev[19];
				sum += coeffs[18] * prev[18];
				sum += coeffs[17] * prev[17];
				sum += coeffs[16] * prev[16];
				sum += coeffs[15] * prev[15];
				sum += coeffs[14] * prev[14];
				sum += coeffs[13] * prev[13];
				sum += coeffs[12] * prev[12];
				sum += coeffs[11] * prev[11];
				sum += coeffs[10] * prev[10];
				sum += coeffs[9] * prev[9];
				sum += coeffs[8] * prev[8];
				sum += coeffs[7] * prev[7];
				sum += coeffs[6] * prev[6];
				sum += coeffs[5] * prev[5];
				sum += coeffs[4] * prev[4];
				sum += coeffs[3] * prev[3];
				sum += coeffs[2] * prev[2];
				sum += coeffs[1] * prev[1];
				sum += coeffs[0] * prev[0];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = sum;
				prev[29] = prev[28];
				prev[28] = prev[27];
				prev[27] = prev[26];
				prev[26] = prev[25];
				prev[25] = prev[24];
				prev[24] = prev[23];
				prev[23] = prev[22];
				prev[22] = prev[21];
				prev[21] = prev[20];
				prev[20] = prev[19];
				prev[19] = prev[18];
				prev[18] = prev[17];
				prev[17] = prev[16];
				prev[16] = prev[15];
				prev[15] = prev[14];
				prev[14] = prev[13];
				prev[13] = prev[12];
				prev[12] = prev[11];
				prev[11] = prev[10];
				prev[10] = prev[9];
				prev[9] = prev[8];
				prev[8] = prev[7];
				prev[7] = prev[6];
				prev[6] = prev[5];
				prev[5] = prev[4];
				prev[4] = prev[3];
				prev[3] = prev[2];
				prev[2] = prev[1];
				prev[1] = prev[0];
                prev[0] = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder31(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 31;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            Span<int> prev = stackalloc int[Order];
            output.SliceWhile(Order).CopyTo(prev);
            _ = prev[Order - 1];
            int sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[30] * prev[30];
				sum += coeffs[29] * prev[29];
				sum += coeffs[28] * prev[28];
				sum += coeffs[27] * prev[27];
				sum += coeffs[26] * prev[26];
				sum += coeffs[25] * prev[25];
				sum += coeffs[24] * prev[24];
				sum += coeffs[23] * prev[23];
				sum += coeffs[22] * prev[22];
				sum += coeffs[21] * prev[21];
				sum += coeffs[20] * prev[20];
				sum += coeffs[19] * prev[19];
				sum += coeffs[18] * prev[18];
				sum += coeffs[17] * prev[17];
				sum += coeffs[16] * prev[16];
				sum += coeffs[15] * prev[15];
				sum += coeffs[14] * prev[14];
				sum += coeffs[13] * prev[13];
				sum += coeffs[12] * prev[12];
				sum += coeffs[11] * prev[11];
				sum += coeffs[10] * prev[10];
				sum += coeffs[9] * prev[9];
				sum += coeffs[8] * prev[8];
				sum += coeffs[7] * prev[7];
				sum += coeffs[6] * prev[6];
				sum += coeffs[5] * prev[5];
				sum += coeffs[4] * prev[4];
				sum += coeffs[3] * prev[3];
				sum += coeffs[2] * prev[2];
				sum += coeffs[1] * prev[1];
				sum += coeffs[0] * prev[0];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = sum;
				prev[30] = prev[29];
				prev[29] = prev[28];
				prev[28] = prev[27];
				prev[27] = prev[26];
				prev[26] = prev[25];
				prev[25] = prev[24];
				prev[24] = prev[23];
				prev[23] = prev[22];
				prev[22] = prev[21];
				prev[21] = prev[20];
				prev[20] = prev[19];
				prev[19] = prev[18];
				prev[18] = prev[17];
				prev[17] = prev[16];
				prev[16] = prev[15];
				prev[15] = prev[14];
				prev[14] = prev[13];
				prev[13] = prev[12];
				prev[12] = prev[11];
				prev[11] = prev[10];
				prev[10] = prev[9];
				prev[9] = prev[8];
				prev[8] = prev[7];
				prev[7] = prev[6];
				prev[6] = prev[5];
				prev[5] = prev[4];
				prev[4] = prev[3];
				prev[3] = prev[2];
				prev[2] = prev[1];
				prev[1] = prev[0];
                prev[0] = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder32(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 32;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            Span<int> prev = stackalloc int[Order];
            output.SliceWhile(Order).CopyTo(prev);
            _ = prev[Order - 1];
            int sum = 0;
            var d = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(output)) + Order;
            int dataLength = output.Length - Order;
            var r = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(residual));
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeffs[31] * prev[31];
				sum += coeffs[30] * prev[30];
				sum += coeffs[29] * prev[29];
				sum += coeffs[28] * prev[28];
				sum += coeffs[27] * prev[27];
				sum += coeffs[26] * prev[26];
				sum += coeffs[25] * prev[25];
				sum += coeffs[24] * prev[24];
				sum += coeffs[23] * prev[23];
				sum += coeffs[22] * prev[22];
				sum += coeffs[21] * prev[21];
				sum += coeffs[20] * prev[20];
				sum += coeffs[19] * prev[19];
				sum += coeffs[18] * prev[18];
				sum += coeffs[17] * prev[17];
				sum += coeffs[16] * prev[16];
				sum += coeffs[15] * prev[15];
				sum += coeffs[14] * prev[14];
				sum += coeffs[13] * prev[13];
				sum += coeffs[12] * prev[12];
				sum += coeffs[11] * prev[11];
				sum += coeffs[10] * prev[10];
				sum += coeffs[9] * prev[9];
				sum += coeffs[8] * prev[8];
				sum += coeffs[7] * prev[7];
				sum += coeffs[6] * prev[6];
				sum += coeffs[5] * prev[5];
				sum += coeffs[4] * prev[4];
				sum += coeffs[3] * prev[3];
				sum += coeffs[2] * prev[2];
				sum += coeffs[1] * prev[1];
				sum += coeffs[0] * prev[0];
                sum >>= shiftsNeeded;
                sum += r[i];
                d[i] = sum;
				prev[31] = prev[30];
				prev[30] = prev[29];
				prev[29] = prev[28];
				prev[28] = prev[27];
				prev[27] = prev[26];
				prev[26] = prev[25];
				prev[25] = prev[24];
				prev[24] = prev[23];
				prev[23] = prev[22];
				prev[22] = prev[21];
				prev[21] = prev[20];
				prev[20] = prev[19];
				prev[19] = prev[18];
				prev[18] = prev[17];
				prev[17] = prev[16];
				prev[16] = prev[15];
				prev[15] = prev[14];
				prev[14] = prev[13];
				prev[13] = prev[12];
				prev[12] = prev[11];
				prev[11] = prev[10];
				prev[10] = prev[9];
				prev[9] = prev[8];
				prev[8] = prev[7];
				prev[7] = prev[6];
				prev[6] = prev[5];
				prev[5] = prev[4];
				prev[4] = prev[3];
				prev[3] = prev[2];
				prev[2] = prev[1];
				prev[1] = prev[0];
                prev[0] = sum;
            }
        }
    }
}
