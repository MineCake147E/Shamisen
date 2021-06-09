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
            ref var o = ref MemoryMarshal.GetReference(output);
            ref var d = ref Unsafe.Add(ref o, Order);
            int dataLength = output.Length - Order;
            ref var r = ref MemoryMarshal.GetReference(residual);
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeff0 * prev0;
                sum >>= shiftsNeeded;
                sum += Unsafe.Add(ref r, i);
                Unsafe.Add(ref d, i) = sum;
                prev0 = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder2Standard(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 2;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
			var coeff0 = coeffs[0];
			var coeff1 = coeffs[1];
            int sum = 0;
            ref var o = ref MemoryMarshal.GetReference(output);
            ref var d = ref Unsafe.Add(ref o, Order);
            int dataLength = output.Length - Order;
            ref var r = ref MemoryMarshal.GetReference(residual);
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeff1 * Unsafe.Add(ref o, i + 0);
				sum += coeff0 * Unsafe.Add(ref o, i + 1);
                sum >>= shiftsNeeded;
                sum += Unsafe.Add(ref r, i);
                Unsafe.Add(ref d, i) = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder3(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 3;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
			var coeff0 = coeffs[0];
			var coeff1 = coeffs[1];
			var coeff2 = coeffs[2];
            int sum = 0;
            ref var o = ref MemoryMarshal.GetReference(output);
            ref var d = ref Unsafe.Add(ref o, Order);
            int dataLength = output.Length - Order;
            ref var r = ref MemoryMarshal.GetReference(residual);
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeff2 * Unsafe.Add(ref o, i + 0);
				sum += coeff1 * Unsafe.Add(ref o, i + 1);
				sum += coeff0 * Unsafe.Add(ref o, i + 2);
                sum >>= shiftsNeeded;
                sum += Unsafe.Add(ref r, i);
                Unsafe.Add(ref d, i) = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder4(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 4;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
			var coeff0 = coeffs[0];
			var coeff1 = coeffs[1];
			var coeff2 = coeffs[2];
			var coeff3 = coeffs[3];
            int sum = 0;
            ref var o = ref MemoryMarshal.GetReference(output);
            ref var d = ref Unsafe.Add(ref o, Order);
            int dataLength = output.Length - Order;
            ref var r = ref MemoryMarshal.GetReference(residual);
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeff3 * Unsafe.Add(ref o, i + 0);
				sum += coeff2 * Unsafe.Add(ref o, i + 1);
				sum += coeff1 * Unsafe.Add(ref o, i + 2);
				sum += coeff0 * Unsafe.Add(ref o, i + 3);
                sum >>= shiftsNeeded;
                sum += Unsafe.Add(ref r, i);
                Unsafe.Add(ref d, i) = sum;
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
            ref var o = ref MemoryMarshal.GetReference(output);
            ref var d = ref Unsafe.Add(ref o, Order);
            int dataLength = output.Length - Order;
            ref var r = ref MemoryMarshal.GetReference(residual);
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeff4 * Unsafe.Add(ref o, i + 0);
				sum += coeff3 * Unsafe.Add(ref o, i + 1);
				sum += coeff2 * Unsafe.Add(ref o, i + 2);
				sum += coeff1 * Unsafe.Add(ref o, i + 3);
				sum += coeff0 * Unsafe.Add(ref o, i + 4);
                sum >>= shiftsNeeded;
                sum += Unsafe.Add(ref r, i);
                Unsafe.Add(ref d, i) = sum;
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
            ref var o = ref MemoryMarshal.GetReference(output);
            ref var d = ref Unsafe.Add(ref o, Order);
            int dataLength = output.Length - Order;
            ref var r = ref MemoryMarshal.GetReference(residual);
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeff5 * Unsafe.Add(ref o, i + 0);
				sum += coeff4 * Unsafe.Add(ref o, i + 1);
				sum += coeff3 * Unsafe.Add(ref o, i + 2);
				sum += coeff2 * Unsafe.Add(ref o, i + 3);
				sum += coeff1 * Unsafe.Add(ref o, i + 4);
				sum += coeff0 * Unsafe.Add(ref o, i + 5);
                sum >>= shiftsNeeded;
                sum += Unsafe.Add(ref r, i);
                Unsafe.Add(ref d, i) = sum;
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
            ref var o = ref MemoryMarshal.GetReference(output);
            ref var d = ref Unsafe.Add(ref o, Order);
            int dataLength = output.Length - Order;
            ref var r = ref MemoryMarshal.GetReference(residual);
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeff6 * Unsafe.Add(ref o, i + 0);
				sum += coeff5 * Unsafe.Add(ref o, i + 1);
				sum += coeff4 * Unsafe.Add(ref o, i + 2);
				sum += coeff3 * Unsafe.Add(ref o, i + 3);
				sum += coeff2 * Unsafe.Add(ref o, i + 4);
				sum += coeff1 * Unsafe.Add(ref o, i + 5);
				sum += coeff0 * Unsafe.Add(ref o, i + 6);
                sum >>= shiftsNeeded;
                sum += Unsafe.Add(ref r, i);
                Unsafe.Add(ref d, i) = sum;
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
            ref var o = ref MemoryMarshal.GetReference(output);
            ref var d = ref Unsafe.Add(ref o, Order);
            int dataLength = output.Length - Order;
            ref var r = ref MemoryMarshal.GetReference(residual);
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeff7 * Unsafe.Add(ref o, i + 0);
				sum += coeff6 * Unsafe.Add(ref o, i + 1);
				sum += coeff5 * Unsafe.Add(ref o, i + 2);
				sum += coeff4 * Unsafe.Add(ref o, i + 3);
				sum += coeff3 * Unsafe.Add(ref o, i + 4);
				sum += coeff2 * Unsafe.Add(ref o, i + 5);
				sum += coeff1 * Unsafe.Add(ref o, i + 6);
				sum += coeff0 * Unsafe.Add(ref o, i + 7);
                sum >>= shiftsNeeded;
                sum += Unsafe.Add(ref r, i);
                Unsafe.Add(ref d, i) = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder9(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 9;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            ref var c = ref MemoryMarshal.GetReference(coeffs);
            int sum = 0;
            ref var o = ref MemoryMarshal.GetReference(output);
            ref var d = ref Unsafe.Add(ref o, Order);
            int dataLength = output.Length - Order;
            ref var r = ref MemoryMarshal.GetReference(residual);
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += Unsafe.Add(ref c, 8) * Unsafe.Add(ref o, i + 0);
				sum += Unsafe.Add(ref c, 7) * Unsafe.Add(ref o, i + 1);
				sum += Unsafe.Add(ref c, 6) * Unsafe.Add(ref o, i + 2);
				sum += Unsafe.Add(ref c, 5) * Unsafe.Add(ref o, i + 3);
				sum += Unsafe.Add(ref c, 4) * Unsafe.Add(ref o, i + 4);
				sum += Unsafe.Add(ref c, 3) * Unsafe.Add(ref o, i + 5);
				sum += Unsafe.Add(ref c, 2) * Unsafe.Add(ref o, i + 6);
				sum += Unsafe.Add(ref c, 1) * Unsafe.Add(ref o, i + 7);
				sum += c * Unsafe.Add(ref o, i + 8);
                sum >>= shiftsNeeded;
                sum += Unsafe.Add(ref r, i);
                Unsafe.Add(ref d, i) = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder10(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 10;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            ref var c = ref MemoryMarshal.GetReference(coeffs);
            int sum = 0;
            ref var o = ref MemoryMarshal.GetReference(output);
            ref var d = ref Unsafe.Add(ref o, Order);
            int dataLength = output.Length - Order;
            ref var r = ref MemoryMarshal.GetReference(residual);
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += Unsafe.Add(ref c, 9) * Unsafe.Add(ref o, i + 0);
				sum += Unsafe.Add(ref c, 8) * Unsafe.Add(ref o, i + 1);
				sum += Unsafe.Add(ref c, 7) * Unsafe.Add(ref o, i + 2);
				sum += Unsafe.Add(ref c, 6) * Unsafe.Add(ref o, i + 3);
				sum += Unsafe.Add(ref c, 5) * Unsafe.Add(ref o, i + 4);
				sum += Unsafe.Add(ref c, 4) * Unsafe.Add(ref o, i + 5);
				sum += Unsafe.Add(ref c, 3) * Unsafe.Add(ref o, i + 6);
				sum += Unsafe.Add(ref c, 2) * Unsafe.Add(ref o, i + 7);
				sum += Unsafe.Add(ref c, 1) * Unsafe.Add(ref o, i + 8);
				sum += c * Unsafe.Add(ref o, i + 9);
                sum >>= shiftsNeeded;
                sum += Unsafe.Add(ref r, i);
                Unsafe.Add(ref d, i) = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder11(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 11;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            ref var c = ref MemoryMarshal.GetReference(coeffs);
            int sum = 0;
            ref var o = ref MemoryMarshal.GetReference(output);
            ref var d = ref Unsafe.Add(ref o, Order);
            int dataLength = output.Length - Order;
            ref var r = ref MemoryMarshal.GetReference(residual);
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += Unsafe.Add(ref c, 10) * Unsafe.Add(ref o, i + 0);
				sum += Unsafe.Add(ref c, 9) * Unsafe.Add(ref o, i + 1);
				sum += Unsafe.Add(ref c, 8) * Unsafe.Add(ref o, i + 2);
				sum += Unsafe.Add(ref c, 7) * Unsafe.Add(ref o, i + 3);
				sum += Unsafe.Add(ref c, 6) * Unsafe.Add(ref o, i + 4);
				sum += Unsafe.Add(ref c, 5) * Unsafe.Add(ref o, i + 5);
				sum += Unsafe.Add(ref c, 4) * Unsafe.Add(ref o, i + 6);
				sum += Unsafe.Add(ref c, 3) * Unsafe.Add(ref o, i + 7);
				sum += Unsafe.Add(ref c, 2) * Unsafe.Add(ref o, i + 8);
				sum += Unsafe.Add(ref c, 1) * Unsafe.Add(ref o, i + 9);
				sum += c * Unsafe.Add(ref o, i + 10);
                sum >>= shiftsNeeded;
                sum += Unsafe.Add(ref r, i);
                Unsafe.Add(ref d, i) = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder12(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 12;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            ref var c = ref MemoryMarshal.GetReference(coeffs);
            int sum = 0;
            ref var o = ref MemoryMarshal.GetReference(output);
            ref var d = ref Unsafe.Add(ref o, Order);
            int dataLength = output.Length - Order;
            ref var r = ref MemoryMarshal.GetReference(residual);
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += Unsafe.Add(ref c, 11) * Unsafe.Add(ref o, i + 0);
				sum += Unsafe.Add(ref c, 10) * Unsafe.Add(ref o, i + 1);
				sum += Unsafe.Add(ref c, 9) * Unsafe.Add(ref o, i + 2);
				sum += Unsafe.Add(ref c, 8) * Unsafe.Add(ref o, i + 3);
				sum += Unsafe.Add(ref c, 7) * Unsafe.Add(ref o, i + 4);
				sum += Unsafe.Add(ref c, 6) * Unsafe.Add(ref o, i + 5);
				sum += Unsafe.Add(ref c, 5) * Unsafe.Add(ref o, i + 6);
				sum += Unsafe.Add(ref c, 4) * Unsafe.Add(ref o, i + 7);
				sum += Unsafe.Add(ref c, 3) * Unsafe.Add(ref o, i + 8);
				sum += Unsafe.Add(ref c, 2) * Unsafe.Add(ref o, i + 9);
				sum += Unsafe.Add(ref c, 1) * Unsafe.Add(ref o, i + 10);
				sum += c * Unsafe.Add(ref o, i + 11);
                sum >>= shiftsNeeded;
                sum += Unsafe.Add(ref r, i);
                Unsafe.Add(ref d, i) = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder13(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 13;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            ref var c = ref MemoryMarshal.GetReference(coeffs);
            int sum = 0;
            ref var o = ref MemoryMarshal.GetReference(output);
            ref var d = ref Unsafe.Add(ref o, Order);
            int dataLength = output.Length - Order;
            ref var r = ref MemoryMarshal.GetReference(residual);
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += Unsafe.Add(ref c, 12) * Unsafe.Add(ref o, i + 0);
				sum += Unsafe.Add(ref c, 11) * Unsafe.Add(ref o, i + 1);
				sum += Unsafe.Add(ref c, 10) * Unsafe.Add(ref o, i + 2);
				sum += Unsafe.Add(ref c, 9) * Unsafe.Add(ref o, i + 3);
				sum += Unsafe.Add(ref c, 8) * Unsafe.Add(ref o, i + 4);
				sum += Unsafe.Add(ref c, 7) * Unsafe.Add(ref o, i + 5);
				sum += Unsafe.Add(ref c, 6) * Unsafe.Add(ref o, i + 6);
				sum += Unsafe.Add(ref c, 5) * Unsafe.Add(ref o, i + 7);
				sum += Unsafe.Add(ref c, 4) * Unsafe.Add(ref o, i + 8);
				sum += Unsafe.Add(ref c, 3) * Unsafe.Add(ref o, i + 9);
				sum += Unsafe.Add(ref c, 2) * Unsafe.Add(ref o, i + 10);
				sum += Unsafe.Add(ref c, 1) * Unsafe.Add(ref o, i + 11);
				sum += c * Unsafe.Add(ref o, i + 12);
                sum >>= shiftsNeeded;
                sum += Unsafe.Add(ref r, i);
                Unsafe.Add(ref d, i) = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder14(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 14;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            ref var c = ref MemoryMarshal.GetReference(coeffs);
            int sum = 0;
            ref var o = ref MemoryMarshal.GetReference(output);
            ref var d = ref Unsafe.Add(ref o, Order);
            int dataLength = output.Length - Order;
            ref var r = ref MemoryMarshal.GetReference(residual);
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += Unsafe.Add(ref c, 13) * Unsafe.Add(ref o, i + 0);
				sum += Unsafe.Add(ref c, 12) * Unsafe.Add(ref o, i + 1);
				sum += Unsafe.Add(ref c, 11) * Unsafe.Add(ref o, i + 2);
				sum += Unsafe.Add(ref c, 10) * Unsafe.Add(ref o, i + 3);
				sum += Unsafe.Add(ref c, 9) * Unsafe.Add(ref o, i + 4);
				sum += Unsafe.Add(ref c, 8) * Unsafe.Add(ref o, i + 5);
				sum += Unsafe.Add(ref c, 7) * Unsafe.Add(ref o, i + 6);
				sum += Unsafe.Add(ref c, 6) * Unsafe.Add(ref o, i + 7);
				sum += Unsafe.Add(ref c, 5) * Unsafe.Add(ref o, i + 8);
				sum += Unsafe.Add(ref c, 4) * Unsafe.Add(ref o, i + 9);
				sum += Unsafe.Add(ref c, 3) * Unsafe.Add(ref o, i + 10);
				sum += Unsafe.Add(ref c, 2) * Unsafe.Add(ref o, i + 11);
				sum += Unsafe.Add(ref c, 1) * Unsafe.Add(ref o, i + 12);
				sum += c * Unsafe.Add(ref o, i + 13);
                sum >>= shiftsNeeded;
                sum += Unsafe.Add(ref r, i);
                Unsafe.Add(ref d, i) = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder15(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 15;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            ref var c = ref MemoryMarshal.GetReference(coeffs);
            int sum = 0;
            ref var o = ref MemoryMarshal.GetReference(output);
            ref var d = ref Unsafe.Add(ref o, Order);
            int dataLength = output.Length - Order;
            ref var r = ref MemoryMarshal.GetReference(residual);
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += Unsafe.Add(ref c, 14) * Unsafe.Add(ref o, i + 0);
				sum += Unsafe.Add(ref c, 13) * Unsafe.Add(ref o, i + 1);
				sum += Unsafe.Add(ref c, 12) * Unsafe.Add(ref o, i + 2);
				sum += Unsafe.Add(ref c, 11) * Unsafe.Add(ref o, i + 3);
				sum += Unsafe.Add(ref c, 10) * Unsafe.Add(ref o, i + 4);
				sum += Unsafe.Add(ref c, 9) * Unsafe.Add(ref o, i + 5);
				sum += Unsafe.Add(ref c, 8) * Unsafe.Add(ref o, i + 6);
				sum += Unsafe.Add(ref c, 7) * Unsafe.Add(ref o, i + 7);
				sum += Unsafe.Add(ref c, 6) * Unsafe.Add(ref o, i + 8);
				sum += Unsafe.Add(ref c, 5) * Unsafe.Add(ref o, i + 9);
				sum += Unsafe.Add(ref c, 4) * Unsafe.Add(ref o, i + 10);
				sum += Unsafe.Add(ref c, 3) * Unsafe.Add(ref o, i + 11);
				sum += Unsafe.Add(ref c, 2) * Unsafe.Add(ref o, i + 12);
				sum += Unsafe.Add(ref c, 1) * Unsafe.Add(ref o, i + 13);
				sum += c * Unsafe.Add(ref o, i + 14);
                sum >>= shiftsNeeded;
                sum += Unsafe.Add(ref r, i);
                Unsafe.Add(ref d, i) = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder16(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 16;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            ref var c = ref MemoryMarshal.GetReference(coeffs);
            int sum = 0;
            ref var o = ref MemoryMarshal.GetReference(output);
            ref var d = ref Unsafe.Add(ref o, Order);
            int dataLength = output.Length - Order;
            ref var r = ref MemoryMarshal.GetReference(residual);
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += Unsafe.Add(ref c, 15) * Unsafe.Add(ref o, i + 0);
				sum += Unsafe.Add(ref c, 14) * Unsafe.Add(ref o, i + 1);
				sum += Unsafe.Add(ref c, 13) * Unsafe.Add(ref o, i + 2);
				sum += Unsafe.Add(ref c, 12) * Unsafe.Add(ref o, i + 3);
				sum += Unsafe.Add(ref c, 11) * Unsafe.Add(ref o, i + 4);
				sum += Unsafe.Add(ref c, 10) * Unsafe.Add(ref o, i + 5);
				sum += Unsafe.Add(ref c, 9) * Unsafe.Add(ref o, i + 6);
				sum += Unsafe.Add(ref c, 8) * Unsafe.Add(ref o, i + 7);
				sum += Unsafe.Add(ref c, 7) * Unsafe.Add(ref o, i + 8);
				sum += Unsafe.Add(ref c, 6) * Unsafe.Add(ref o, i + 9);
				sum += Unsafe.Add(ref c, 5) * Unsafe.Add(ref o, i + 10);
				sum += Unsafe.Add(ref c, 4) * Unsafe.Add(ref o, i + 11);
				sum += Unsafe.Add(ref c, 3) * Unsafe.Add(ref o, i + 12);
				sum += Unsafe.Add(ref c, 2) * Unsafe.Add(ref o, i + 13);
				sum += Unsafe.Add(ref c, 1) * Unsafe.Add(ref o, i + 14);
				sum += c * Unsafe.Add(ref o, i + 15);
                sum >>= shiftsNeeded;
                sum += Unsafe.Add(ref r, i);
                Unsafe.Add(ref d, i) = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder17(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 17;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            ref var c = ref MemoryMarshal.GetReference(coeffs);
            int sum = 0;
            ref var o = ref MemoryMarshal.GetReference(output);
            ref var d = ref Unsafe.Add(ref o, Order);
            int dataLength = output.Length - Order;
            ref var r = ref MemoryMarshal.GetReference(residual);
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += Unsafe.Add(ref c, 16) * Unsafe.Add(ref o, i + 0);
				sum += Unsafe.Add(ref c, 15) * Unsafe.Add(ref o, i + 1);
				sum += Unsafe.Add(ref c, 14) * Unsafe.Add(ref o, i + 2);
				sum += Unsafe.Add(ref c, 13) * Unsafe.Add(ref o, i + 3);
				sum += Unsafe.Add(ref c, 12) * Unsafe.Add(ref o, i + 4);
				sum += Unsafe.Add(ref c, 11) * Unsafe.Add(ref o, i + 5);
				sum += Unsafe.Add(ref c, 10) * Unsafe.Add(ref o, i + 6);
				sum += Unsafe.Add(ref c, 9) * Unsafe.Add(ref o, i + 7);
				sum += Unsafe.Add(ref c, 8) * Unsafe.Add(ref o, i + 8);
				sum += Unsafe.Add(ref c, 7) * Unsafe.Add(ref o, i + 9);
				sum += Unsafe.Add(ref c, 6) * Unsafe.Add(ref o, i + 10);
				sum += Unsafe.Add(ref c, 5) * Unsafe.Add(ref o, i + 11);
				sum += Unsafe.Add(ref c, 4) * Unsafe.Add(ref o, i + 12);
				sum += Unsafe.Add(ref c, 3) * Unsafe.Add(ref o, i + 13);
				sum += Unsafe.Add(ref c, 2) * Unsafe.Add(ref o, i + 14);
				sum += Unsafe.Add(ref c, 1) * Unsafe.Add(ref o, i + 15);
				sum += c * Unsafe.Add(ref o, i + 16);
                sum >>= shiftsNeeded;
                sum += Unsafe.Add(ref r, i);
                Unsafe.Add(ref d, i) = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder18(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 18;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            ref var c = ref MemoryMarshal.GetReference(coeffs);
            int sum = 0;
            ref var o = ref MemoryMarshal.GetReference(output);
            ref var d = ref Unsafe.Add(ref o, Order);
            int dataLength = output.Length - Order;
            ref var r = ref MemoryMarshal.GetReference(residual);
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += Unsafe.Add(ref c, 17) * Unsafe.Add(ref o, i + 0);
				sum += Unsafe.Add(ref c, 16) * Unsafe.Add(ref o, i + 1);
				sum += Unsafe.Add(ref c, 15) * Unsafe.Add(ref o, i + 2);
				sum += Unsafe.Add(ref c, 14) * Unsafe.Add(ref o, i + 3);
				sum += Unsafe.Add(ref c, 13) * Unsafe.Add(ref o, i + 4);
				sum += Unsafe.Add(ref c, 12) * Unsafe.Add(ref o, i + 5);
				sum += Unsafe.Add(ref c, 11) * Unsafe.Add(ref o, i + 6);
				sum += Unsafe.Add(ref c, 10) * Unsafe.Add(ref o, i + 7);
				sum += Unsafe.Add(ref c, 9) * Unsafe.Add(ref o, i + 8);
				sum += Unsafe.Add(ref c, 8) * Unsafe.Add(ref o, i + 9);
				sum += Unsafe.Add(ref c, 7) * Unsafe.Add(ref o, i + 10);
				sum += Unsafe.Add(ref c, 6) * Unsafe.Add(ref o, i + 11);
				sum += Unsafe.Add(ref c, 5) * Unsafe.Add(ref o, i + 12);
				sum += Unsafe.Add(ref c, 4) * Unsafe.Add(ref o, i + 13);
				sum += Unsafe.Add(ref c, 3) * Unsafe.Add(ref o, i + 14);
				sum += Unsafe.Add(ref c, 2) * Unsafe.Add(ref o, i + 15);
				sum += Unsafe.Add(ref c, 1) * Unsafe.Add(ref o, i + 16);
				sum += c * Unsafe.Add(ref o, i + 17);
                sum >>= shiftsNeeded;
                sum += Unsafe.Add(ref r, i);
                Unsafe.Add(ref d, i) = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder19(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 19;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            ref var c = ref MemoryMarshal.GetReference(coeffs);
            int sum = 0;
            ref var o = ref MemoryMarshal.GetReference(output);
            ref var d = ref Unsafe.Add(ref o, Order);
            int dataLength = output.Length - Order;
            ref var r = ref MemoryMarshal.GetReference(residual);
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += Unsafe.Add(ref c, 18) * Unsafe.Add(ref o, i + 0);
				sum += Unsafe.Add(ref c, 17) * Unsafe.Add(ref o, i + 1);
				sum += Unsafe.Add(ref c, 16) * Unsafe.Add(ref o, i + 2);
				sum += Unsafe.Add(ref c, 15) * Unsafe.Add(ref o, i + 3);
				sum += Unsafe.Add(ref c, 14) * Unsafe.Add(ref o, i + 4);
				sum += Unsafe.Add(ref c, 13) * Unsafe.Add(ref o, i + 5);
				sum += Unsafe.Add(ref c, 12) * Unsafe.Add(ref o, i + 6);
				sum += Unsafe.Add(ref c, 11) * Unsafe.Add(ref o, i + 7);
				sum += Unsafe.Add(ref c, 10) * Unsafe.Add(ref o, i + 8);
				sum += Unsafe.Add(ref c, 9) * Unsafe.Add(ref o, i + 9);
				sum += Unsafe.Add(ref c, 8) * Unsafe.Add(ref o, i + 10);
				sum += Unsafe.Add(ref c, 7) * Unsafe.Add(ref o, i + 11);
				sum += Unsafe.Add(ref c, 6) * Unsafe.Add(ref o, i + 12);
				sum += Unsafe.Add(ref c, 5) * Unsafe.Add(ref o, i + 13);
				sum += Unsafe.Add(ref c, 4) * Unsafe.Add(ref o, i + 14);
				sum += Unsafe.Add(ref c, 3) * Unsafe.Add(ref o, i + 15);
				sum += Unsafe.Add(ref c, 2) * Unsafe.Add(ref o, i + 16);
				sum += Unsafe.Add(ref c, 1) * Unsafe.Add(ref o, i + 17);
				sum += c * Unsafe.Add(ref o, i + 18);
                sum >>= shiftsNeeded;
                sum += Unsafe.Add(ref r, i);
                Unsafe.Add(ref d, i) = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder20(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 20;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            ref var c = ref MemoryMarshal.GetReference(coeffs);
            int sum = 0;
            ref var o = ref MemoryMarshal.GetReference(output);
            ref var d = ref Unsafe.Add(ref o, Order);
            int dataLength = output.Length - Order;
            ref var r = ref MemoryMarshal.GetReference(residual);
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += Unsafe.Add(ref c, 19) * Unsafe.Add(ref o, i + 0);
				sum += Unsafe.Add(ref c, 18) * Unsafe.Add(ref o, i + 1);
				sum += Unsafe.Add(ref c, 17) * Unsafe.Add(ref o, i + 2);
				sum += Unsafe.Add(ref c, 16) * Unsafe.Add(ref o, i + 3);
				sum += Unsafe.Add(ref c, 15) * Unsafe.Add(ref o, i + 4);
				sum += Unsafe.Add(ref c, 14) * Unsafe.Add(ref o, i + 5);
				sum += Unsafe.Add(ref c, 13) * Unsafe.Add(ref o, i + 6);
				sum += Unsafe.Add(ref c, 12) * Unsafe.Add(ref o, i + 7);
				sum += Unsafe.Add(ref c, 11) * Unsafe.Add(ref o, i + 8);
				sum += Unsafe.Add(ref c, 10) * Unsafe.Add(ref o, i + 9);
				sum += Unsafe.Add(ref c, 9) * Unsafe.Add(ref o, i + 10);
				sum += Unsafe.Add(ref c, 8) * Unsafe.Add(ref o, i + 11);
				sum += Unsafe.Add(ref c, 7) * Unsafe.Add(ref o, i + 12);
				sum += Unsafe.Add(ref c, 6) * Unsafe.Add(ref o, i + 13);
				sum += Unsafe.Add(ref c, 5) * Unsafe.Add(ref o, i + 14);
				sum += Unsafe.Add(ref c, 4) * Unsafe.Add(ref o, i + 15);
				sum += Unsafe.Add(ref c, 3) * Unsafe.Add(ref o, i + 16);
				sum += Unsafe.Add(ref c, 2) * Unsafe.Add(ref o, i + 17);
				sum += Unsafe.Add(ref c, 1) * Unsafe.Add(ref o, i + 18);
				sum += c * Unsafe.Add(ref o, i + 19);
                sum >>= shiftsNeeded;
                sum += Unsafe.Add(ref r, i);
                Unsafe.Add(ref d, i) = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder21(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 21;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            ref var c = ref MemoryMarshal.GetReference(coeffs);
            int sum = 0;
            ref var o = ref MemoryMarshal.GetReference(output);
            ref var d = ref Unsafe.Add(ref o, Order);
            int dataLength = output.Length - Order;
            ref var r = ref MemoryMarshal.GetReference(residual);
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += Unsafe.Add(ref c, 20) * Unsafe.Add(ref o, i + 0);
				sum += Unsafe.Add(ref c, 19) * Unsafe.Add(ref o, i + 1);
				sum += Unsafe.Add(ref c, 18) * Unsafe.Add(ref o, i + 2);
				sum += Unsafe.Add(ref c, 17) * Unsafe.Add(ref o, i + 3);
				sum += Unsafe.Add(ref c, 16) * Unsafe.Add(ref o, i + 4);
				sum += Unsafe.Add(ref c, 15) * Unsafe.Add(ref o, i + 5);
				sum += Unsafe.Add(ref c, 14) * Unsafe.Add(ref o, i + 6);
				sum += Unsafe.Add(ref c, 13) * Unsafe.Add(ref o, i + 7);
				sum += Unsafe.Add(ref c, 12) * Unsafe.Add(ref o, i + 8);
				sum += Unsafe.Add(ref c, 11) * Unsafe.Add(ref o, i + 9);
				sum += Unsafe.Add(ref c, 10) * Unsafe.Add(ref o, i + 10);
				sum += Unsafe.Add(ref c, 9) * Unsafe.Add(ref o, i + 11);
				sum += Unsafe.Add(ref c, 8) * Unsafe.Add(ref o, i + 12);
				sum += Unsafe.Add(ref c, 7) * Unsafe.Add(ref o, i + 13);
				sum += Unsafe.Add(ref c, 6) * Unsafe.Add(ref o, i + 14);
				sum += Unsafe.Add(ref c, 5) * Unsafe.Add(ref o, i + 15);
				sum += Unsafe.Add(ref c, 4) * Unsafe.Add(ref o, i + 16);
				sum += Unsafe.Add(ref c, 3) * Unsafe.Add(ref o, i + 17);
				sum += Unsafe.Add(ref c, 2) * Unsafe.Add(ref o, i + 18);
				sum += Unsafe.Add(ref c, 1) * Unsafe.Add(ref o, i + 19);
				sum += c * Unsafe.Add(ref o, i + 20);
                sum >>= shiftsNeeded;
                sum += Unsafe.Add(ref r, i);
                Unsafe.Add(ref d, i) = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder22(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 22;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            ref var c = ref MemoryMarshal.GetReference(coeffs);
            int sum = 0;
            ref var o = ref MemoryMarshal.GetReference(output);
            ref var d = ref Unsafe.Add(ref o, Order);
            int dataLength = output.Length - Order;
            ref var r = ref MemoryMarshal.GetReference(residual);
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += Unsafe.Add(ref c, 21) * Unsafe.Add(ref o, i + 0);
				sum += Unsafe.Add(ref c, 20) * Unsafe.Add(ref o, i + 1);
				sum += Unsafe.Add(ref c, 19) * Unsafe.Add(ref o, i + 2);
				sum += Unsafe.Add(ref c, 18) * Unsafe.Add(ref o, i + 3);
				sum += Unsafe.Add(ref c, 17) * Unsafe.Add(ref o, i + 4);
				sum += Unsafe.Add(ref c, 16) * Unsafe.Add(ref o, i + 5);
				sum += Unsafe.Add(ref c, 15) * Unsafe.Add(ref o, i + 6);
				sum += Unsafe.Add(ref c, 14) * Unsafe.Add(ref o, i + 7);
				sum += Unsafe.Add(ref c, 13) * Unsafe.Add(ref o, i + 8);
				sum += Unsafe.Add(ref c, 12) * Unsafe.Add(ref o, i + 9);
				sum += Unsafe.Add(ref c, 11) * Unsafe.Add(ref o, i + 10);
				sum += Unsafe.Add(ref c, 10) * Unsafe.Add(ref o, i + 11);
				sum += Unsafe.Add(ref c, 9) * Unsafe.Add(ref o, i + 12);
				sum += Unsafe.Add(ref c, 8) * Unsafe.Add(ref o, i + 13);
				sum += Unsafe.Add(ref c, 7) * Unsafe.Add(ref o, i + 14);
				sum += Unsafe.Add(ref c, 6) * Unsafe.Add(ref o, i + 15);
				sum += Unsafe.Add(ref c, 5) * Unsafe.Add(ref o, i + 16);
				sum += Unsafe.Add(ref c, 4) * Unsafe.Add(ref o, i + 17);
				sum += Unsafe.Add(ref c, 3) * Unsafe.Add(ref o, i + 18);
				sum += Unsafe.Add(ref c, 2) * Unsafe.Add(ref o, i + 19);
				sum += Unsafe.Add(ref c, 1) * Unsafe.Add(ref o, i + 20);
				sum += c * Unsafe.Add(ref o, i + 21);
                sum >>= shiftsNeeded;
                sum += Unsafe.Add(ref r, i);
                Unsafe.Add(ref d, i) = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder23(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 23;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            ref var c = ref MemoryMarshal.GetReference(coeffs);
            int sum = 0;
            ref var o = ref MemoryMarshal.GetReference(output);
            ref var d = ref Unsafe.Add(ref o, Order);
            int dataLength = output.Length - Order;
            ref var r = ref MemoryMarshal.GetReference(residual);
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += Unsafe.Add(ref c, 22) * Unsafe.Add(ref o, i + 0);
				sum += Unsafe.Add(ref c, 21) * Unsafe.Add(ref o, i + 1);
				sum += Unsafe.Add(ref c, 20) * Unsafe.Add(ref o, i + 2);
				sum += Unsafe.Add(ref c, 19) * Unsafe.Add(ref o, i + 3);
				sum += Unsafe.Add(ref c, 18) * Unsafe.Add(ref o, i + 4);
				sum += Unsafe.Add(ref c, 17) * Unsafe.Add(ref o, i + 5);
				sum += Unsafe.Add(ref c, 16) * Unsafe.Add(ref o, i + 6);
				sum += Unsafe.Add(ref c, 15) * Unsafe.Add(ref o, i + 7);
				sum += Unsafe.Add(ref c, 14) * Unsafe.Add(ref o, i + 8);
				sum += Unsafe.Add(ref c, 13) * Unsafe.Add(ref o, i + 9);
				sum += Unsafe.Add(ref c, 12) * Unsafe.Add(ref o, i + 10);
				sum += Unsafe.Add(ref c, 11) * Unsafe.Add(ref o, i + 11);
				sum += Unsafe.Add(ref c, 10) * Unsafe.Add(ref o, i + 12);
				sum += Unsafe.Add(ref c, 9) * Unsafe.Add(ref o, i + 13);
				sum += Unsafe.Add(ref c, 8) * Unsafe.Add(ref o, i + 14);
				sum += Unsafe.Add(ref c, 7) * Unsafe.Add(ref o, i + 15);
				sum += Unsafe.Add(ref c, 6) * Unsafe.Add(ref o, i + 16);
				sum += Unsafe.Add(ref c, 5) * Unsafe.Add(ref o, i + 17);
				sum += Unsafe.Add(ref c, 4) * Unsafe.Add(ref o, i + 18);
				sum += Unsafe.Add(ref c, 3) * Unsafe.Add(ref o, i + 19);
				sum += Unsafe.Add(ref c, 2) * Unsafe.Add(ref o, i + 20);
				sum += Unsafe.Add(ref c, 1) * Unsafe.Add(ref o, i + 21);
				sum += c * Unsafe.Add(ref o, i + 22);
                sum >>= shiftsNeeded;
                sum += Unsafe.Add(ref r, i);
                Unsafe.Add(ref d, i) = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder24(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 24;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            ref var c = ref MemoryMarshal.GetReference(coeffs);
            int sum = 0;
            ref var o = ref MemoryMarshal.GetReference(output);
            ref var d = ref Unsafe.Add(ref o, Order);
            int dataLength = output.Length - Order;
            ref var r = ref MemoryMarshal.GetReference(residual);
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += Unsafe.Add(ref c, 23) * Unsafe.Add(ref o, i + 0);
				sum += Unsafe.Add(ref c, 22) * Unsafe.Add(ref o, i + 1);
				sum += Unsafe.Add(ref c, 21) * Unsafe.Add(ref o, i + 2);
				sum += Unsafe.Add(ref c, 20) * Unsafe.Add(ref o, i + 3);
				sum += Unsafe.Add(ref c, 19) * Unsafe.Add(ref o, i + 4);
				sum += Unsafe.Add(ref c, 18) * Unsafe.Add(ref o, i + 5);
				sum += Unsafe.Add(ref c, 17) * Unsafe.Add(ref o, i + 6);
				sum += Unsafe.Add(ref c, 16) * Unsafe.Add(ref o, i + 7);
				sum += Unsafe.Add(ref c, 15) * Unsafe.Add(ref o, i + 8);
				sum += Unsafe.Add(ref c, 14) * Unsafe.Add(ref o, i + 9);
				sum += Unsafe.Add(ref c, 13) * Unsafe.Add(ref o, i + 10);
				sum += Unsafe.Add(ref c, 12) * Unsafe.Add(ref o, i + 11);
				sum += Unsafe.Add(ref c, 11) * Unsafe.Add(ref o, i + 12);
				sum += Unsafe.Add(ref c, 10) * Unsafe.Add(ref o, i + 13);
				sum += Unsafe.Add(ref c, 9) * Unsafe.Add(ref o, i + 14);
				sum += Unsafe.Add(ref c, 8) * Unsafe.Add(ref o, i + 15);
				sum += Unsafe.Add(ref c, 7) * Unsafe.Add(ref o, i + 16);
				sum += Unsafe.Add(ref c, 6) * Unsafe.Add(ref o, i + 17);
				sum += Unsafe.Add(ref c, 5) * Unsafe.Add(ref o, i + 18);
				sum += Unsafe.Add(ref c, 4) * Unsafe.Add(ref o, i + 19);
				sum += Unsafe.Add(ref c, 3) * Unsafe.Add(ref o, i + 20);
				sum += Unsafe.Add(ref c, 2) * Unsafe.Add(ref o, i + 21);
				sum += Unsafe.Add(ref c, 1) * Unsafe.Add(ref o, i + 22);
				sum += c * Unsafe.Add(ref o, i + 23);
                sum >>= shiftsNeeded;
                sum += Unsafe.Add(ref r, i);
                Unsafe.Add(ref d, i) = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder25(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 25;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            ref var c = ref MemoryMarshal.GetReference(coeffs);
            int sum = 0;
            ref var o = ref MemoryMarshal.GetReference(output);
            ref var d = ref Unsafe.Add(ref o, Order);
            int dataLength = output.Length - Order;
            ref var r = ref MemoryMarshal.GetReference(residual);
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += Unsafe.Add(ref c, 24) * Unsafe.Add(ref o, i + 0);
				sum += Unsafe.Add(ref c, 23) * Unsafe.Add(ref o, i + 1);
				sum += Unsafe.Add(ref c, 22) * Unsafe.Add(ref o, i + 2);
				sum += Unsafe.Add(ref c, 21) * Unsafe.Add(ref o, i + 3);
				sum += Unsafe.Add(ref c, 20) * Unsafe.Add(ref o, i + 4);
				sum += Unsafe.Add(ref c, 19) * Unsafe.Add(ref o, i + 5);
				sum += Unsafe.Add(ref c, 18) * Unsafe.Add(ref o, i + 6);
				sum += Unsafe.Add(ref c, 17) * Unsafe.Add(ref o, i + 7);
				sum += Unsafe.Add(ref c, 16) * Unsafe.Add(ref o, i + 8);
				sum += Unsafe.Add(ref c, 15) * Unsafe.Add(ref o, i + 9);
				sum += Unsafe.Add(ref c, 14) * Unsafe.Add(ref o, i + 10);
				sum += Unsafe.Add(ref c, 13) * Unsafe.Add(ref o, i + 11);
				sum += Unsafe.Add(ref c, 12) * Unsafe.Add(ref o, i + 12);
				sum += Unsafe.Add(ref c, 11) * Unsafe.Add(ref o, i + 13);
				sum += Unsafe.Add(ref c, 10) * Unsafe.Add(ref o, i + 14);
				sum += Unsafe.Add(ref c, 9) * Unsafe.Add(ref o, i + 15);
				sum += Unsafe.Add(ref c, 8) * Unsafe.Add(ref o, i + 16);
				sum += Unsafe.Add(ref c, 7) * Unsafe.Add(ref o, i + 17);
				sum += Unsafe.Add(ref c, 6) * Unsafe.Add(ref o, i + 18);
				sum += Unsafe.Add(ref c, 5) * Unsafe.Add(ref o, i + 19);
				sum += Unsafe.Add(ref c, 4) * Unsafe.Add(ref o, i + 20);
				sum += Unsafe.Add(ref c, 3) * Unsafe.Add(ref o, i + 21);
				sum += Unsafe.Add(ref c, 2) * Unsafe.Add(ref o, i + 22);
				sum += Unsafe.Add(ref c, 1) * Unsafe.Add(ref o, i + 23);
				sum += c * Unsafe.Add(ref o, i + 24);
                sum >>= shiftsNeeded;
                sum += Unsafe.Add(ref r, i);
                Unsafe.Add(ref d, i) = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder26(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 26;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            ref var c = ref MemoryMarshal.GetReference(coeffs);
            int sum = 0;
            ref var o = ref MemoryMarshal.GetReference(output);
            ref var d = ref Unsafe.Add(ref o, Order);
            int dataLength = output.Length - Order;
            ref var r = ref MemoryMarshal.GetReference(residual);
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += Unsafe.Add(ref c, 25) * Unsafe.Add(ref o, i + 0);
				sum += Unsafe.Add(ref c, 24) * Unsafe.Add(ref o, i + 1);
				sum += Unsafe.Add(ref c, 23) * Unsafe.Add(ref o, i + 2);
				sum += Unsafe.Add(ref c, 22) * Unsafe.Add(ref o, i + 3);
				sum += Unsafe.Add(ref c, 21) * Unsafe.Add(ref o, i + 4);
				sum += Unsafe.Add(ref c, 20) * Unsafe.Add(ref o, i + 5);
				sum += Unsafe.Add(ref c, 19) * Unsafe.Add(ref o, i + 6);
				sum += Unsafe.Add(ref c, 18) * Unsafe.Add(ref o, i + 7);
				sum += Unsafe.Add(ref c, 17) * Unsafe.Add(ref o, i + 8);
				sum += Unsafe.Add(ref c, 16) * Unsafe.Add(ref o, i + 9);
				sum += Unsafe.Add(ref c, 15) * Unsafe.Add(ref o, i + 10);
				sum += Unsafe.Add(ref c, 14) * Unsafe.Add(ref o, i + 11);
				sum += Unsafe.Add(ref c, 13) * Unsafe.Add(ref o, i + 12);
				sum += Unsafe.Add(ref c, 12) * Unsafe.Add(ref o, i + 13);
				sum += Unsafe.Add(ref c, 11) * Unsafe.Add(ref o, i + 14);
				sum += Unsafe.Add(ref c, 10) * Unsafe.Add(ref o, i + 15);
				sum += Unsafe.Add(ref c, 9) * Unsafe.Add(ref o, i + 16);
				sum += Unsafe.Add(ref c, 8) * Unsafe.Add(ref o, i + 17);
				sum += Unsafe.Add(ref c, 7) * Unsafe.Add(ref o, i + 18);
				sum += Unsafe.Add(ref c, 6) * Unsafe.Add(ref o, i + 19);
				sum += Unsafe.Add(ref c, 5) * Unsafe.Add(ref o, i + 20);
				sum += Unsafe.Add(ref c, 4) * Unsafe.Add(ref o, i + 21);
				sum += Unsafe.Add(ref c, 3) * Unsafe.Add(ref o, i + 22);
				sum += Unsafe.Add(ref c, 2) * Unsafe.Add(ref o, i + 23);
				sum += Unsafe.Add(ref c, 1) * Unsafe.Add(ref o, i + 24);
				sum += c * Unsafe.Add(ref o, i + 25);
                sum >>= shiftsNeeded;
                sum += Unsafe.Add(ref r, i);
                Unsafe.Add(ref d, i) = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder27(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 27;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            ref var c = ref MemoryMarshal.GetReference(coeffs);
            int sum = 0;
            ref var o = ref MemoryMarshal.GetReference(output);
            ref var d = ref Unsafe.Add(ref o, Order);
            int dataLength = output.Length - Order;
            ref var r = ref MemoryMarshal.GetReference(residual);
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += Unsafe.Add(ref c, 26) * Unsafe.Add(ref o, i + 0);
				sum += Unsafe.Add(ref c, 25) * Unsafe.Add(ref o, i + 1);
				sum += Unsafe.Add(ref c, 24) * Unsafe.Add(ref o, i + 2);
				sum += Unsafe.Add(ref c, 23) * Unsafe.Add(ref o, i + 3);
				sum += Unsafe.Add(ref c, 22) * Unsafe.Add(ref o, i + 4);
				sum += Unsafe.Add(ref c, 21) * Unsafe.Add(ref o, i + 5);
				sum += Unsafe.Add(ref c, 20) * Unsafe.Add(ref o, i + 6);
				sum += Unsafe.Add(ref c, 19) * Unsafe.Add(ref o, i + 7);
				sum += Unsafe.Add(ref c, 18) * Unsafe.Add(ref o, i + 8);
				sum += Unsafe.Add(ref c, 17) * Unsafe.Add(ref o, i + 9);
				sum += Unsafe.Add(ref c, 16) * Unsafe.Add(ref o, i + 10);
				sum += Unsafe.Add(ref c, 15) * Unsafe.Add(ref o, i + 11);
				sum += Unsafe.Add(ref c, 14) * Unsafe.Add(ref o, i + 12);
				sum += Unsafe.Add(ref c, 13) * Unsafe.Add(ref o, i + 13);
				sum += Unsafe.Add(ref c, 12) * Unsafe.Add(ref o, i + 14);
				sum += Unsafe.Add(ref c, 11) * Unsafe.Add(ref o, i + 15);
				sum += Unsafe.Add(ref c, 10) * Unsafe.Add(ref o, i + 16);
				sum += Unsafe.Add(ref c, 9) * Unsafe.Add(ref o, i + 17);
				sum += Unsafe.Add(ref c, 8) * Unsafe.Add(ref o, i + 18);
				sum += Unsafe.Add(ref c, 7) * Unsafe.Add(ref o, i + 19);
				sum += Unsafe.Add(ref c, 6) * Unsafe.Add(ref o, i + 20);
				sum += Unsafe.Add(ref c, 5) * Unsafe.Add(ref o, i + 21);
				sum += Unsafe.Add(ref c, 4) * Unsafe.Add(ref o, i + 22);
				sum += Unsafe.Add(ref c, 3) * Unsafe.Add(ref o, i + 23);
				sum += Unsafe.Add(ref c, 2) * Unsafe.Add(ref o, i + 24);
				sum += Unsafe.Add(ref c, 1) * Unsafe.Add(ref o, i + 25);
				sum += c * Unsafe.Add(ref o, i + 26);
                sum >>= shiftsNeeded;
                sum += Unsafe.Add(ref r, i);
                Unsafe.Add(ref d, i) = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder28(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 28;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            ref var c = ref MemoryMarshal.GetReference(coeffs);
            int sum = 0;
            ref var o = ref MemoryMarshal.GetReference(output);
            ref var d = ref Unsafe.Add(ref o, Order);
            int dataLength = output.Length - Order;
            ref var r = ref MemoryMarshal.GetReference(residual);
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += Unsafe.Add(ref c, 27) * Unsafe.Add(ref o, i + 0);
				sum += Unsafe.Add(ref c, 26) * Unsafe.Add(ref o, i + 1);
				sum += Unsafe.Add(ref c, 25) * Unsafe.Add(ref o, i + 2);
				sum += Unsafe.Add(ref c, 24) * Unsafe.Add(ref o, i + 3);
				sum += Unsafe.Add(ref c, 23) * Unsafe.Add(ref o, i + 4);
				sum += Unsafe.Add(ref c, 22) * Unsafe.Add(ref o, i + 5);
				sum += Unsafe.Add(ref c, 21) * Unsafe.Add(ref o, i + 6);
				sum += Unsafe.Add(ref c, 20) * Unsafe.Add(ref o, i + 7);
				sum += Unsafe.Add(ref c, 19) * Unsafe.Add(ref o, i + 8);
				sum += Unsafe.Add(ref c, 18) * Unsafe.Add(ref o, i + 9);
				sum += Unsafe.Add(ref c, 17) * Unsafe.Add(ref o, i + 10);
				sum += Unsafe.Add(ref c, 16) * Unsafe.Add(ref o, i + 11);
				sum += Unsafe.Add(ref c, 15) * Unsafe.Add(ref o, i + 12);
				sum += Unsafe.Add(ref c, 14) * Unsafe.Add(ref o, i + 13);
				sum += Unsafe.Add(ref c, 13) * Unsafe.Add(ref o, i + 14);
				sum += Unsafe.Add(ref c, 12) * Unsafe.Add(ref o, i + 15);
				sum += Unsafe.Add(ref c, 11) * Unsafe.Add(ref o, i + 16);
				sum += Unsafe.Add(ref c, 10) * Unsafe.Add(ref o, i + 17);
				sum += Unsafe.Add(ref c, 9) * Unsafe.Add(ref o, i + 18);
				sum += Unsafe.Add(ref c, 8) * Unsafe.Add(ref o, i + 19);
				sum += Unsafe.Add(ref c, 7) * Unsafe.Add(ref o, i + 20);
				sum += Unsafe.Add(ref c, 6) * Unsafe.Add(ref o, i + 21);
				sum += Unsafe.Add(ref c, 5) * Unsafe.Add(ref o, i + 22);
				sum += Unsafe.Add(ref c, 4) * Unsafe.Add(ref o, i + 23);
				sum += Unsafe.Add(ref c, 3) * Unsafe.Add(ref o, i + 24);
				sum += Unsafe.Add(ref c, 2) * Unsafe.Add(ref o, i + 25);
				sum += Unsafe.Add(ref c, 1) * Unsafe.Add(ref o, i + 26);
				sum += c * Unsafe.Add(ref o, i + 27);
                sum >>= shiftsNeeded;
                sum += Unsafe.Add(ref r, i);
                Unsafe.Add(ref d, i) = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder29(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 29;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            ref var c = ref MemoryMarshal.GetReference(coeffs);
            int sum = 0;
            ref var o = ref MemoryMarshal.GetReference(output);
            ref var d = ref Unsafe.Add(ref o, Order);
            int dataLength = output.Length - Order;
            ref var r = ref MemoryMarshal.GetReference(residual);
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += Unsafe.Add(ref c, 28) * Unsafe.Add(ref o, i + 0);
				sum += Unsafe.Add(ref c, 27) * Unsafe.Add(ref o, i + 1);
				sum += Unsafe.Add(ref c, 26) * Unsafe.Add(ref o, i + 2);
				sum += Unsafe.Add(ref c, 25) * Unsafe.Add(ref o, i + 3);
				sum += Unsafe.Add(ref c, 24) * Unsafe.Add(ref o, i + 4);
				sum += Unsafe.Add(ref c, 23) * Unsafe.Add(ref o, i + 5);
				sum += Unsafe.Add(ref c, 22) * Unsafe.Add(ref o, i + 6);
				sum += Unsafe.Add(ref c, 21) * Unsafe.Add(ref o, i + 7);
				sum += Unsafe.Add(ref c, 20) * Unsafe.Add(ref o, i + 8);
				sum += Unsafe.Add(ref c, 19) * Unsafe.Add(ref o, i + 9);
				sum += Unsafe.Add(ref c, 18) * Unsafe.Add(ref o, i + 10);
				sum += Unsafe.Add(ref c, 17) * Unsafe.Add(ref o, i + 11);
				sum += Unsafe.Add(ref c, 16) * Unsafe.Add(ref o, i + 12);
				sum += Unsafe.Add(ref c, 15) * Unsafe.Add(ref o, i + 13);
				sum += Unsafe.Add(ref c, 14) * Unsafe.Add(ref o, i + 14);
				sum += Unsafe.Add(ref c, 13) * Unsafe.Add(ref o, i + 15);
				sum += Unsafe.Add(ref c, 12) * Unsafe.Add(ref o, i + 16);
				sum += Unsafe.Add(ref c, 11) * Unsafe.Add(ref o, i + 17);
				sum += Unsafe.Add(ref c, 10) * Unsafe.Add(ref o, i + 18);
				sum += Unsafe.Add(ref c, 9) * Unsafe.Add(ref o, i + 19);
				sum += Unsafe.Add(ref c, 8) * Unsafe.Add(ref o, i + 20);
				sum += Unsafe.Add(ref c, 7) * Unsafe.Add(ref o, i + 21);
				sum += Unsafe.Add(ref c, 6) * Unsafe.Add(ref o, i + 22);
				sum += Unsafe.Add(ref c, 5) * Unsafe.Add(ref o, i + 23);
				sum += Unsafe.Add(ref c, 4) * Unsafe.Add(ref o, i + 24);
				sum += Unsafe.Add(ref c, 3) * Unsafe.Add(ref o, i + 25);
				sum += Unsafe.Add(ref c, 2) * Unsafe.Add(ref o, i + 26);
				sum += Unsafe.Add(ref c, 1) * Unsafe.Add(ref o, i + 27);
				sum += c * Unsafe.Add(ref o, i + 28);
                sum >>= shiftsNeeded;
                sum += Unsafe.Add(ref r, i);
                Unsafe.Add(ref d, i) = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder30(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 30;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            ref var c = ref MemoryMarshal.GetReference(coeffs);
            int sum = 0;
            ref var o = ref MemoryMarshal.GetReference(output);
            ref var d = ref Unsafe.Add(ref o, Order);
            int dataLength = output.Length - Order;
            ref var r = ref MemoryMarshal.GetReference(residual);
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += Unsafe.Add(ref c, 29) * Unsafe.Add(ref o, i + 0);
				sum += Unsafe.Add(ref c, 28) * Unsafe.Add(ref o, i + 1);
				sum += Unsafe.Add(ref c, 27) * Unsafe.Add(ref o, i + 2);
				sum += Unsafe.Add(ref c, 26) * Unsafe.Add(ref o, i + 3);
				sum += Unsafe.Add(ref c, 25) * Unsafe.Add(ref o, i + 4);
				sum += Unsafe.Add(ref c, 24) * Unsafe.Add(ref o, i + 5);
				sum += Unsafe.Add(ref c, 23) * Unsafe.Add(ref o, i + 6);
				sum += Unsafe.Add(ref c, 22) * Unsafe.Add(ref o, i + 7);
				sum += Unsafe.Add(ref c, 21) * Unsafe.Add(ref o, i + 8);
				sum += Unsafe.Add(ref c, 20) * Unsafe.Add(ref o, i + 9);
				sum += Unsafe.Add(ref c, 19) * Unsafe.Add(ref o, i + 10);
				sum += Unsafe.Add(ref c, 18) * Unsafe.Add(ref o, i + 11);
				sum += Unsafe.Add(ref c, 17) * Unsafe.Add(ref o, i + 12);
				sum += Unsafe.Add(ref c, 16) * Unsafe.Add(ref o, i + 13);
				sum += Unsafe.Add(ref c, 15) * Unsafe.Add(ref o, i + 14);
				sum += Unsafe.Add(ref c, 14) * Unsafe.Add(ref o, i + 15);
				sum += Unsafe.Add(ref c, 13) * Unsafe.Add(ref o, i + 16);
				sum += Unsafe.Add(ref c, 12) * Unsafe.Add(ref o, i + 17);
				sum += Unsafe.Add(ref c, 11) * Unsafe.Add(ref o, i + 18);
				sum += Unsafe.Add(ref c, 10) * Unsafe.Add(ref o, i + 19);
				sum += Unsafe.Add(ref c, 9) * Unsafe.Add(ref o, i + 20);
				sum += Unsafe.Add(ref c, 8) * Unsafe.Add(ref o, i + 21);
				sum += Unsafe.Add(ref c, 7) * Unsafe.Add(ref o, i + 22);
				sum += Unsafe.Add(ref c, 6) * Unsafe.Add(ref o, i + 23);
				sum += Unsafe.Add(ref c, 5) * Unsafe.Add(ref o, i + 24);
				sum += Unsafe.Add(ref c, 4) * Unsafe.Add(ref o, i + 25);
				sum += Unsafe.Add(ref c, 3) * Unsafe.Add(ref o, i + 26);
				sum += Unsafe.Add(ref c, 2) * Unsafe.Add(ref o, i + 27);
				sum += Unsafe.Add(ref c, 1) * Unsafe.Add(ref o, i + 28);
				sum += c * Unsafe.Add(ref o, i + 29);
                sum >>= shiftsNeeded;
                sum += Unsafe.Add(ref r, i);
                Unsafe.Add(ref d, i) = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder31(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 31;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            ref var c = ref MemoryMarshal.GetReference(coeffs);
            int sum = 0;
            ref var o = ref MemoryMarshal.GetReference(output);
            ref var d = ref Unsafe.Add(ref o, Order);
            int dataLength = output.Length - Order;
            ref var r = ref MemoryMarshal.GetReference(residual);
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += Unsafe.Add(ref c, 30) * Unsafe.Add(ref o, i + 0);
				sum += Unsafe.Add(ref c, 29) * Unsafe.Add(ref o, i + 1);
				sum += Unsafe.Add(ref c, 28) * Unsafe.Add(ref o, i + 2);
				sum += Unsafe.Add(ref c, 27) * Unsafe.Add(ref o, i + 3);
				sum += Unsafe.Add(ref c, 26) * Unsafe.Add(ref o, i + 4);
				sum += Unsafe.Add(ref c, 25) * Unsafe.Add(ref o, i + 5);
				sum += Unsafe.Add(ref c, 24) * Unsafe.Add(ref o, i + 6);
				sum += Unsafe.Add(ref c, 23) * Unsafe.Add(ref o, i + 7);
				sum += Unsafe.Add(ref c, 22) * Unsafe.Add(ref o, i + 8);
				sum += Unsafe.Add(ref c, 21) * Unsafe.Add(ref o, i + 9);
				sum += Unsafe.Add(ref c, 20) * Unsafe.Add(ref o, i + 10);
				sum += Unsafe.Add(ref c, 19) * Unsafe.Add(ref o, i + 11);
				sum += Unsafe.Add(ref c, 18) * Unsafe.Add(ref o, i + 12);
				sum += Unsafe.Add(ref c, 17) * Unsafe.Add(ref o, i + 13);
				sum += Unsafe.Add(ref c, 16) * Unsafe.Add(ref o, i + 14);
				sum += Unsafe.Add(ref c, 15) * Unsafe.Add(ref o, i + 15);
				sum += Unsafe.Add(ref c, 14) * Unsafe.Add(ref o, i + 16);
				sum += Unsafe.Add(ref c, 13) * Unsafe.Add(ref o, i + 17);
				sum += Unsafe.Add(ref c, 12) * Unsafe.Add(ref o, i + 18);
				sum += Unsafe.Add(ref c, 11) * Unsafe.Add(ref o, i + 19);
				sum += Unsafe.Add(ref c, 10) * Unsafe.Add(ref o, i + 20);
				sum += Unsafe.Add(ref c, 9) * Unsafe.Add(ref o, i + 21);
				sum += Unsafe.Add(ref c, 8) * Unsafe.Add(ref o, i + 22);
				sum += Unsafe.Add(ref c, 7) * Unsafe.Add(ref o, i + 23);
				sum += Unsafe.Add(ref c, 6) * Unsafe.Add(ref o, i + 24);
				sum += Unsafe.Add(ref c, 5) * Unsafe.Add(ref o, i + 25);
				sum += Unsafe.Add(ref c, 4) * Unsafe.Add(ref o, i + 26);
				sum += Unsafe.Add(ref c, 3) * Unsafe.Add(ref o, i + 27);
				sum += Unsafe.Add(ref c, 2) * Unsafe.Add(ref o, i + 28);
				sum += Unsafe.Add(ref c, 1) * Unsafe.Add(ref o, i + 29);
				sum += c * Unsafe.Add(ref o, i + 30);
                sum >>= shiftsNeeded;
                sum += Unsafe.Add(ref r, i);
                Unsafe.Add(ref d, i) = sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder32(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 32;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            ref var c = ref MemoryMarshal.GetReference(coeffs);
            int sum = 0;
            ref var o = ref MemoryMarshal.GetReference(output);
            ref var d = ref Unsafe.Add(ref o, Order);
            int dataLength = output.Length - Order;
            ref var r = ref MemoryMarshal.GetReference(residual);
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += Unsafe.Add(ref c, 31) * Unsafe.Add(ref o, i + 0);
				sum += Unsafe.Add(ref c, 30) * Unsafe.Add(ref o, i + 1);
				sum += Unsafe.Add(ref c, 29) * Unsafe.Add(ref o, i + 2);
				sum += Unsafe.Add(ref c, 28) * Unsafe.Add(ref o, i + 3);
				sum += Unsafe.Add(ref c, 27) * Unsafe.Add(ref o, i + 4);
				sum += Unsafe.Add(ref c, 26) * Unsafe.Add(ref o, i + 5);
				sum += Unsafe.Add(ref c, 25) * Unsafe.Add(ref o, i + 6);
				sum += Unsafe.Add(ref c, 24) * Unsafe.Add(ref o, i + 7);
				sum += Unsafe.Add(ref c, 23) * Unsafe.Add(ref o, i + 8);
				sum += Unsafe.Add(ref c, 22) * Unsafe.Add(ref o, i + 9);
				sum += Unsafe.Add(ref c, 21) * Unsafe.Add(ref o, i + 10);
				sum += Unsafe.Add(ref c, 20) * Unsafe.Add(ref o, i + 11);
				sum += Unsafe.Add(ref c, 19) * Unsafe.Add(ref o, i + 12);
				sum += Unsafe.Add(ref c, 18) * Unsafe.Add(ref o, i + 13);
				sum += Unsafe.Add(ref c, 17) * Unsafe.Add(ref o, i + 14);
				sum += Unsafe.Add(ref c, 16) * Unsafe.Add(ref o, i + 15);
				sum += Unsafe.Add(ref c, 15) * Unsafe.Add(ref o, i + 16);
				sum += Unsafe.Add(ref c, 14) * Unsafe.Add(ref o, i + 17);
				sum += Unsafe.Add(ref c, 13) * Unsafe.Add(ref o, i + 18);
				sum += Unsafe.Add(ref c, 12) * Unsafe.Add(ref o, i + 19);
				sum += Unsafe.Add(ref c, 11) * Unsafe.Add(ref o, i + 20);
				sum += Unsafe.Add(ref c, 10) * Unsafe.Add(ref o, i + 21);
				sum += Unsafe.Add(ref c, 9) * Unsafe.Add(ref o, i + 22);
				sum += Unsafe.Add(ref c, 8) * Unsafe.Add(ref o, i + 23);
				sum += Unsafe.Add(ref c, 7) * Unsafe.Add(ref o, i + 24);
				sum += Unsafe.Add(ref c, 6) * Unsafe.Add(ref o, i + 25);
				sum += Unsafe.Add(ref c, 5) * Unsafe.Add(ref o, i + 26);
				sum += Unsafe.Add(ref c, 4) * Unsafe.Add(ref o, i + 27);
				sum += Unsafe.Add(ref c, 3) * Unsafe.Add(ref o, i + 28);
				sum += Unsafe.Add(ref c, 2) * Unsafe.Add(ref o, i + 29);
				sum += Unsafe.Add(ref c, 1) * Unsafe.Add(ref o, i + 30);
				sum += c * Unsafe.Add(ref o, i + 31);
                sum >>= shiftsNeeded;
                sum += Unsafe.Add(ref r, i);
                Unsafe.Add(ref d, i) = sum;
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
            ref var o = ref MemoryMarshal.GetReference(output);
            ref var d = ref Unsafe.Add(ref o, Order);
            int dataLength = output.Length - Order;
            ref var r = ref MemoryMarshal.GetReference(residual);
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeff0 * prev0;
                sum >>= shiftsNeeded;
                sum += Unsafe.Add(ref r, i);
                Unsafe.Add(ref d, i) = (int)sum;
                prev0 = (int)sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder2WideStandard(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 2;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
			long coeff0 = coeffs[0];
			long coeff1 = coeffs[1];
            long sum = 0;
            ref var o = ref MemoryMarshal.GetReference(output);
            ref var d = ref Unsafe.Add(ref o, Order);
            int dataLength = output.Length - Order;
            ref var r = ref MemoryMarshal.GetReference(residual);
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeff1 * Unsafe.Add(ref o, i + 0);
				sum += coeff0 * Unsafe.Add(ref o, i + 1);
                sum >>= shiftsNeeded;
                sum += Unsafe.Add(ref r, i);
                Unsafe.Add(ref d, i) = (int)sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder3Wide(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 3;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
			long coeff0 = coeffs[0];
			long coeff1 = coeffs[1];
			long coeff2 = coeffs[2];
            long sum = 0;
            ref var o = ref MemoryMarshal.GetReference(output);
            ref var d = ref Unsafe.Add(ref o, Order);
            int dataLength = output.Length - Order;
            ref var r = ref MemoryMarshal.GetReference(residual);
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeff2 * Unsafe.Add(ref o, i + 0);
				sum += coeff1 * Unsafe.Add(ref o, i + 1);
				sum += coeff0 * Unsafe.Add(ref o, i + 2);
                sum >>= shiftsNeeded;
                sum += Unsafe.Add(ref r, i);
                Unsafe.Add(ref d, i) = (int)sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder4Wide(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 4;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
			long coeff0 = coeffs[0];
			long coeff1 = coeffs[1];
			long coeff2 = coeffs[2];
			long coeff3 = coeffs[3];
            long sum = 0;
            ref var o = ref MemoryMarshal.GetReference(output);
            ref var d = ref Unsafe.Add(ref o, Order);
            int dataLength = output.Length - Order;
            ref var r = ref MemoryMarshal.GetReference(residual);
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeff3 * Unsafe.Add(ref o, i + 0);
				sum += coeff2 * Unsafe.Add(ref o, i + 1);
				sum += coeff1 * Unsafe.Add(ref o, i + 2);
				sum += coeff0 * Unsafe.Add(ref o, i + 3);
                sum >>= shiftsNeeded;
                sum += Unsafe.Add(ref r, i);
                Unsafe.Add(ref d, i) = (int)sum;
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
            ref var o = ref MemoryMarshal.GetReference(output);
            ref var d = ref Unsafe.Add(ref o, Order);
            int dataLength = output.Length - Order;
            ref var r = ref MemoryMarshal.GetReference(residual);
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeff4 * Unsafe.Add(ref o, i + 0);
				sum += coeff3 * Unsafe.Add(ref o, i + 1);
				sum += coeff2 * Unsafe.Add(ref o, i + 2);
				sum += coeff1 * Unsafe.Add(ref o, i + 3);
				sum += coeff0 * Unsafe.Add(ref o, i + 4);
                sum >>= shiftsNeeded;
                sum += Unsafe.Add(ref r, i);
                Unsafe.Add(ref d, i) = (int)sum;
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
            ref var o = ref MemoryMarshal.GetReference(output);
            ref var d = ref Unsafe.Add(ref o, Order);
            int dataLength = output.Length - Order;
            ref var r = ref MemoryMarshal.GetReference(residual);
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeff5 * Unsafe.Add(ref o, i + 0);
				sum += coeff4 * Unsafe.Add(ref o, i + 1);
				sum += coeff3 * Unsafe.Add(ref o, i + 2);
				sum += coeff2 * Unsafe.Add(ref o, i + 3);
				sum += coeff1 * Unsafe.Add(ref o, i + 4);
				sum += coeff0 * Unsafe.Add(ref o, i + 5);
                sum >>= shiftsNeeded;
                sum += Unsafe.Add(ref r, i);
                Unsafe.Add(ref d, i) = (int)sum;
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
            ref var o = ref MemoryMarshal.GetReference(output);
            ref var d = ref Unsafe.Add(ref o, Order);
            int dataLength = output.Length - Order;
            ref var r = ref MemoryMarshal.GetReference(residual);
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeff6 * Unsafe.Add(ref o, i + 0);
				sum += coeff5 * Unsafe.Add(ref o, i + 1);
				sum += coeff4 * Unsafe.Add(ref o, i + 2);
				sum += coeff3 * Unsafe.Add(ref o, i + 3);
				sum += coeff2 * Unsafe.Add(ref o, i + 4);
				sum += coeff1 * Unsafe.Add(ref o, i + 5);
				sum += coeff0 * Unsafe.Add(ref o, i + 6);
                sum >>= shiftsNeeded;
                sum += Unsafe.Add(ref r, i);
                Unsafe.Add(ref d, i) = (int)sum;
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
            ref var o = ref MemoryMarshal.GetReference(output);
            ref var d = ref Unsafe.Add(ref o, Order);
            int dataLength = output.Length - Order;
            ref var r = ref MemoryMarshal.GetReference(residual);
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += coeff7 * Unsafe.Add(ref o, i + 0);
				sum += coeff6 * Unsafe.Add(ref o, i + 1);
				sum += coeff5 * Unsafe.Add(ref o, i + 2);
				sum += coeff4 * Unsafe.Add(ref o, i + 3);
				sum += coeff3 * Unsafe.Add(ref o, i + 4);
				sum += coeff2 * Unsafe.Add(ref o, i + 5);
				sum += coeff1 * Unsafe.Add(ref o, i + 6);
				sum += coeff0 * Unsafe.Add(ref o, i + 7);
                sum >>= shiftsNeeded;
                sum += Unsafe.Add(ref r, i);
                Unsafe.Add(ref d, i) = (int)sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder9Wide(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 9;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            ref var c = ref MemoryMarshal.GetReference(coeffs);
            long sum = 0;
            ref var o = ref MemoryMarshal.GetReference(output);
            ref var d = ref Unsafe.Add(ref o, Order);
            int dataLength = output.Length - Order;
            ref var r = ref MemoryMarshal.GetReference(residual);
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += Unsafe.Add(ref c, 8) * (long)Unsafe.Add(ref o, i + 0);
				sum += Unsafe.Add(ref c, 7) * (long)Unsafe.Add(ref o, i + 1);
				sum += Unsafe.Add(ref c, 6) * (long)Unsafe.Add(ref o, i + 2);
				sum += Unsafe.Add(ref c, 5) * (long)Unsafe.Add(ref o, i + 3);
				sum += Unsafe.Add(ref c, 4) * (long)Unsafe.Add(ref o, i + 4);
				sum += Unsafe.Add(ref c, 3) * (long)Unsafe.Add(ref o, i + 5);
				sum += Unsafe.Add(ref c, 2) * (long)Unsafe.Add(ref o, i + 6);
				sum += Unsafe.Add(ref c, 1) * (long)Unsafe.Add(ref o, i + 7);
				sum += c * (long)Unsafe.Add(ref o, i + 8);
                sum >>= shiftsNeeded;
                sum += Unsafe.Add(ref r, i);
                Unsafe.Add(ref d, i) = (int)sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder10Wide(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 10;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            ref var c = ref MemoryMarshal.GetReference(coeffs);
            long sum = 0;
            ref var o = ref MemoryMarshal.GetReference(output);
            ref var d = ref Unsafe.Add(ref o, Order);
            int dataLength = output.Length - Order;
            ref var r = ref MemoryMarshal.GetReference(residual);
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += Unsafe.Add(ref c, 9) * (long)Unsafe.Add(ref o, i + 0);
				sum += Unsafe.Add(ref c, 8) * (long)Unsafe.Add(ref o, i + 1);
				sum += Unsafe.Add(ref c, 7) * (long)Unsafe.Add(ref o, i + 2);
				sum += Unsafe.Add(ref c, 6) * (long)Unsafe.Add(ref o, i + 3);
				sum += Unsafe.Add(ref c, 5) * (long)Unsafe.Add(ref o, i + 4);
				sum += Unsafe.Add(ref c, 4) * (long)Unsafe.Add(ref o, i + 5);
				sum += Unsafe.Add(ref c, 3) * (long)Unsafe.Add(ref o, i + 6);
				sum += Unsafe.Add(ref c, 2) * (long)Unsafe.Add(ref o, i + 7);
				sum += Unsafe.Add(ref c, 1) * (long)Unsafe.Add(ref o, i + 8);
				sum += c * (long)Unsafe.Add(ref o, i + 9);
                sum >>= shiftsNeeded;
                sum += Unsafe.Add(ref r, i);
                Unsafe.Add(ref d, i) = (int)sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder11Wide(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 11;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            ref var c = ref MemoryMarshal.GetReference(coeffs);
            long sum = 0;
            ref var o = ref MemoryMarshal.GetReference(output);
            ref var d = ref Unsafe.Add(ref o, Order);
            int dataLength = output.Length - Order;
            ref var r = ref MemoryMarshal.GetReference(residual);
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += Unsafe.Add(ref c, 10) * (long)Unsafe.Add(ref o, i + 0);
				sum += Unsafe.Add(ref c, 9) * (long)Unsafe.Add(ref o, i + 1);
				sum += Unsafe.Add(ref c, 8) * (long)Unsafe.Add(ref o, i + 2);
				sum += Unsafe.Add(ref c, 7) * (long)Unsafe.Add(ref o, i + 3);
				sum += Unsafe.Add(ref c, 6) * (long)Unsafe.Add(ref o, i + 4);
				sum += Unsafe.Add(ref c, 5) * (long)Unsafe.Add(ref o, i + 5);
				sum += Unsafe.Add(ref c, 4) * (long)Unsafe.Add(ref o, i + 6);
				sum += Unsafe.Add(ref c, 3) * (long)Unsafe.Add(ref o, i + 7);
				sum += Unsafe.Add(ref c, 2) * (long)Unsafe.Add(ref o, i + 8);
				sum += Unsafe.Add(ref c, 1) * (long)Unsafe.Add(ref o, i + 9);
				sum += c * (long)Unsafe.Add(ref o, i + 10);
                sum >>= shiftsNeeded;
                sum += Unsafe.Add(ref r, i);
                Unsafe.Add(ref d, i) = (int)sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder12Wide(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 12;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            ref var c = ref MemoryMarshal.GetReference(coeffs);
            long sum = 0;
            ref var o = ref MemoryMarshal.GetReference(output);
            ref var d = ref Unsafe.Add(ref o, Order);
            int dataLength = output.Length - Order;
            ref var r = ref MemoryMarshal.GetReference(residual);
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += Unsafe.Add(ref c, 11) * (long)Unsafe.Add(ref o, i + 0);
				sum += Unsafe.Add(ref c, 10) * (long)Unsafe.Add(ref o, i + 1);
				sum += Unsafe.Add(ref c, 9) * (long)Unsafe.Add(ref o, i + 2);
				sum += Unsafe.Add(ref c, 8) * (long)Unsafe.Add(ref o, i + 3);
				sum += Unsafe.Add(ref c, 7) * (long)Unsafe.Add(ref o, i + 4);
				sum += Unsafe.Add(ref c, 6) * (long)Unsafe.Add(ref o, i + 5);
				sum += Unsafe.Add(ref c, 5) * (long)Unsafe.Add(ref o, i + 6);
				sum += Unsafe.Add(ref c, 4) * (long)Unsafe.Add(ref o, i + 7);
				sum += Unsafe.Add(ref c, 3) * (long)Unsafe.Add(ref o, i + 8);
				sum += Unsafe.Add(ref c, 2) * (long)Unsafe.Add(ref o, i + 9);
				sum += Unsafe.Add(ref c, 1) * (long)Unsafe.Add(ref o, i + 10);
				sum += c * (long)Unsafe.Add(ref o, i + 11);
                sum >>= shiftsNeeded;
                sum += Unsafe.Add(ref r, i);
                Unsafe.Add(ref d, i) = (int)sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder13Wide(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 13;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            ref var c = ref MemoryMarshal.GetReference(coeffs);
            long sum = 0;
            ref var o = ref MemoryMarshal.GetReference(output);
            ref var d = ref Unsafe.Add(ref o, Order);
            int dataLength = output.Length - Order;
            ref var r = ref MemoryMarshal.GetReference(residual);
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += Unsafe.Add(ref c, 12) * (long)Unsafe.Add(ref o, i + 0);
				sum += Unsafe.Add(ref c, 11) * (long)Unsafe.Add(ref o, i + 1);
				sum += Unsafe.Add(ref c, 10) * (long)Unsafe.Add(ref o, i + 2);
				sum += Unsafe.Add(ref c, 9) * (long)Unsafe.Add(ref o, i + 3);
				sum += Unsafe.Add(ref c, 8) * (long)Unsafe.Add(ref o, i + 4);
				sum += Unsafe.Add(ref c, 7) * (long)Unsafe.Add(ref o, i + 5);
				sum += Unsafe.Add(ref c, 6) * (long)Unsafe.Add(ref o, i + 6);
				sum += Unsafe.Add(ref c, 5) * (long)Unsafe.Add(ref o, i + 7);
				sum += Unsafe.Add(ref c, 4) * (long)Unsafe.Add(ref o, i + 8);
				sum += Unsafe.Add(ref c, 3) * (long)Unsafe.Add(ref o, i + 9);
				sum += Unsafe.Add(ref c, 2) * (long)Unsafe.Add(ref o, i + 10);
				sum += Unsafe.Add(ref c, 1) * (long)Unsafe.Add(ref o, i + 11);
				sum += c * (long)Unsafe.Add(ref o, i + 12);
                sum >>= shiftsNeeded;
                sum += Unsafe.Add(ref r, i);
                Unsafe.Add(ref d, i) = (int)sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder14Wide(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 14;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            ref var c = ref MemoryMarshal.GetReference(coeffs);
            long sum = 0;
            ref var o = ref MemoryMarshal.GetReference(output);
            ref var d = ref Unsafe.Add(ref o, Order);
            int dataLength = output.Length - Order;
            ref var r = ref MemoryMarshal.GetReference(residual);
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += Unsafe.Add(ref c, 13) * (long)Unsafe.Add(ref o, i + 0);
				sum += Unsafe.Add(ref c, 12) * (long)Unsafe.Add(ref o, i + 1);
				sum += Unsafe.Add(ref c, 11) * (long)Unsafe.Add(ref o, i + 2);
				sum += Unsafe.Add(ref c, 10) * (long)Unsafe.Add(ref o, i + 3);
				sum += Unsafe.Add(ref c, 9) * (long)Unsafe.Add(ref o, i + 4);
				sum += Unsafe.Add(ref c, 8) * (long)Unsafe.Add(ref o, i + 5);
				sum += Unsafe.Add(ref c, 7) * (long)Unsafe.Add(ref o, i + 6);
				sum += Unsafe.Add(ref c, 6) * (long)Unsafe.Add(ref o, i + 7);
				sum += Unsafe.Add(ref c, 5) * (long)Unsafe.Add(ref o, i + 8);
				sum += Unsafe.Add(ref c, 4) * (long)Unsafe.Add(ref o, i + 9);
				sum += Unsafe.Add(ref c, 3) * (long)Unsafe.Add(ref o, i + 10);
				sum += Unsafe.Add(ref c, 2) * (long)Unsafe.Add(ref o, i + 11);
				sum += Unsafe.Add(ref c, 1) * (long)Unsafe.Add(ref o, i + 12);
				sum += c * (long)Unsafe.Add(ref o, i + 13);
                sum >>= shiftsNeeded;
                sum += Unsafe.Add(ref r, i);
                Unsafe.Add(ref d, i) = (int)sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder15Wide(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 15;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            ref var c = ref MemoryMarshal.GetReference(coeffs);
            long sum = 0;
            ref var o = ref MemoryMarshal.GetReference(output);
            ref var d = ref Unsafe.Add(ref o, Order);
            int dataLength = output.Length - Order;
            ref var r = ref MemoryMarshal.GetReference(residual);
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += Unsafe.Add(ref c, 14) * (long)Unsafe.Add(ref o, i + 0);
				sum += Unsafe.Add(ref c, 13) * (long)Unsafe.Add(ref o, i + 1);
				sum += Unsafe.Add(ref c, 12) * (long)Unsafe.Add(ref o, i + 2);
				sum += Unsafe.Add(ref c, 11) * (long)Unsafe.Add(ref o, i + 3);
				sum += Unsafe.Add(ref c, 10) * (long)Unsafe.Add(ref o, i + 4);
				sum += Unsafe.Add(ref c, 9) * (long)Unsafe.Add(ref o, i + 5);
				sum += Unsafe.Add(ref c, 8) * (long)Unsafe.Add(ref o, i + 6);
				sum += Unsafe.Add(ref c, 7) * (long)Unsafe.Add(ref o, i + 7);
				sum += Unsafe.Add(ref c, 6) * (long)Unsafe.Add(ref o, i + 8);
				sum += Unsafe.Add(ref c, 5) * (long)Unsafe.Add(ref o, i + 9);
				sum += Unsafe.Add(ref c, 4) * (long)Unsafe.Add(ref o, i + 10);
				sum += Unsafe.Add(ref c, 3) * (long)Unsafe.Add(ref o, i + 11);
				sum += Unsafe.Add(ref c, 2) * (long)Unsafe.Add(ref o, i + 12);
				sum += Unsafe.Add(ref c, 1) * (long)Unsafe.Add(ref o, i + 13);
				sum += c * (long)Unsafe.Add(ref o, i + 14);
                sum >>= shiftsNeeded;
                sum += Unsafe.Add(ref r, i);
                Unsafe.Add(ref d, i) = (int)sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder16Wide(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 16;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            ref var c = ref MemoryMarshal.GetReference(coeffs);
            long sum = 0;
            ref var o = ref MemoryMarshal.GetReference(output);
            ref var d = ref Unsafe.Add(ref o, Order);
            int dataLength = output.Length - Order;
            ref var r = ref MemoryMarshal.GetReference(residual);
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += Unsafe.Add(ref c, 15) * (long)Unsafe.Add(ref o, i + 0);
				sum += Unsafe.Add(ref c, 14) * (long)Unsafe.Add(ref o, i + 1);
				sum += Unsafe.Add(ref c, 13) * (long)Unsafe.Add(ref o, i + 2);
				sum += Unsafe.Add(ref c, 12) * (long)Unsafe.Add(ref o, i + 3);
				sum += Unsafe.Add(ref c, 11) * (long)Unsafe.Add(ref o, i + 4);
				sum += Unsafe.Add(ref c, 10) * (long)Unsafe.Add(ref o, i + 5);
				sum += Unsafe.Add(ref c, 9) * (long)Unsafe.Add(ref o, i + 6);
				sum += Unsafe.Add(ref c, 8) * (long)Unsafe.Add(ref o, i + 7);
				sum += Unsafe.Add(ref c, 7) * (long)Unsafe.Add(ref o, i + 8);
				sum += Unsafe.Add(ref c, 6) * (long)Unsafe.Add(ref o, i + 9);
				sum += Unsafe.Add(ref c, 5) * (long)Unsafe.Add(ref o, i + 10);
				sum += Unsafe.Add(ref c, 4) * (long)Unsafe.Add(ref o, i + 11);
				sum += Unsafe.Add(ref c, 3) * (long)Unsafe.Add(ref o, i + 12);
				sum += Unsafe.Add(ref c, 2) * (long)Unsafe.Add(ref o, i + 13);
				sum += Unsafe.Add(ref c, 1) * (long)Unsafe.Add(ref o, i + 14);
				sum += c * (long)Unsafe.Add(ref o, i + 15);
                sum >>= shiftsNeeded;
                sum += Unsafe.Add(ref r, i);
                Unsafe.Add(ref d, i) = (int)sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder17Wide(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 17;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            ref var c = ref MemoryMarshal.GetReference(coeffs);
            long sum = 0;
            ref var o = ref MemoryMarshal.GetReference(output);
            ref var d = ref Unsafe.Add(ref o, Order);
            int dataLength = output.Length - Order;
            ref var r = ref MemoryMarshal.GetReference(residual);
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += Unsafe.Add(ref c, 16) * (long)Unsafe.Add(ref o, i + 0);
				sum += Unsafe.Add(ref c, 15) * (long)Unsafe.Add(ref o, i + 1);
				sum += Unsafe.Add(ref c, 14) * (long)Unsafe.Add(ref o, i + 2);
				sum += Unsafe.Add(ref c, 13) * (long)Unsafe.Add(ref o, i + 3);
				sum += Unsafe.Add(ref c, 12) * (long)Unsafe.Add(ref o, i + 4);
				sum += Unsafe.Add(ref c, 11) * (long)Unsafe.Add(ref o, i + 5);
				sum += Unsafe.Add(ref c, 10) * (long)Unsafe.Add(ref o, i + 6);
				sum += Unsafe.Add(ref c, 9) * (long)Unsafe.Add(ref o, i + 7);
				sum += Unsafe.Add(ref c, 8) * (long)Unsafe.Add(ref o, i + 8);
				sum += Unsafe.Add(ref c, 7) * (long)Unsafe.Add(ref o, i + 9);
				sum += Unsafe.Add(ref c, 6) * (long)Unsafe.Add(ref o, i + 10);
				sum += Unsafe.Add(ref c, 5) * (long)Unsafe.Add(ref o, i + 11);
				sum += Unsafe.Add(ref c, 4) * (long)Unsafe.Add(ref o, i + 12);
				sum += Unsafe.Add(ref c, 3) * (long)Unsafe.Add(ref o, i + 13);
				sum += Unsafe.Add(ref c, 2) * (long)Unsafe.Add(ref o, i + 14);
				sum += Unsafe.Add(ref c, 1) * (long)Unsafe.Add(ref o, i + 15);
				sum += c * (long)Unsafe.Add(ref o, i + 16);
                sum >>= shiftsNeeded;
                sum += Unsafe.Add(ref r, i);
                Unsafe.Add(ref d, i) = (int)sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder18Wide(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 18;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            ref var c = ref MemoryMarshal.GetReference(coeffs);
            long sum = 0;
            ref var o = ref MemoryMarshal.GetReference(output);
            ref var d = ref Unsafe.Add(ref o, Order);
            int dataLength = output.Length - Order;
            ref var r = ref MemoryMarshal.GetReference(residual);
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += Unsafe.Add(ref c, 17) * (long)Unsafe.Add(ref o, i + 0);
				sum += Unsafe.Add(ref c, 16) * (long)Unsafe.Add(ref o, i + 1);
				sum += Unsafe.Add(ref c, 15) * (long)Unsafe.Add(ref o, i + 2);
				sum += Unsafe.Add(ref c, 14) * (long)Unsafe.Add(ref o, i + 3);
				sum += Unsafe.Add(ref c, 13) * (long)Unsafe.Add(ref o, i + 4);
				sum += Unsafe.Add(ref c, 12) * (long)Unsafe.Add(ref o, i + 5);
				sum += Unsafe.Add(ref c, 11) * (long)Unsafe.Add(ref o, i + 6);
				sum += Unsafe.Add(ref c, 10) * (long)Unsafe.Add(ref o, i + 7);
				sum += Unsafe.Add(ref c, 9) * (long)Unsafe.Add(ref o, i + 8);
				sum += Unsafe.Add(ref c, 8) * (long)Unsafe.Add(ref o, i + 9);
				sum += Unsafe.Add(ref c, 7) * (long)Unsafe.Add(ref o, i + 10);
				sum += Unsafe.Add(ref c, 6) * (long)Unsafe.Add(ref o, i + 11);
				sum += Unsafe.Add(ref c, 5) * (long)Unsafe.Add(ref o, i + 12);
				sum += Unsafe.Add(ref c, 4) * (long)Unsafe.Add(ref o, i + 13);
				sum += Unsafe.Add(ref c, 3) * (long)Unsafe.Add(ref o, i + 14);
				sum += Unsafe.Add(ref c, 2) * (long)Unsafe.Add(ref o, i + 15);
				sum += Unsafe.Add(ref c, 1) * (long)Unsafe.Add(ref o, i + 16);
				sum += c * (long)Unsafe.Add(ref o, i + 17);
                sum >>= shiftsNeeded;
                sum += Unsafe.Add(ref r, i);
                Unsafe.Add(ref d, i) = (int)sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder19Wide(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 19;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            ref var c = ref MemoryMarshal.GetReference(coeffs);
            long sum = 0;
            ref var o = ref MemoryMarshal.GetReference(output);
            ref var d = ref Unsafe.Add(ref o, Order);
            int dataLength = output.Length - Order;
            ref var r = ref MemoryMarshal.GetReference(residual);
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += Unsafe.Add(ref c, 18) * (long)Unsafe.Add(ref o, i + 0);
				sum += Unsafe.Add(ref c, 17) * (long)Unsafe.Add(ref o, i + 1);
				sum += Unsafe.Add(ref c, 16) * (long)Unsafe.Add(ref o, i + 2);
				sum += Unsafe.Add(ref c, 15) * (long)Unsafe.Add(ref o, i + 3);
				sum += Unsafe.Add(ref c, 14) * (long)Unsafe.Add(ref o, i + 4);
				sum += Unsafe.Add(ref c, 13) * (long)Unsafe.Add(ref o, i + 5);
				sum += Unsafe.Add(ref c, 12) * (long)Unsafe.Add(ref o, i + 6);
				sum += Unsafe.Add(ref c, 11) * (long)Unsafe.Add(ref o, i + 7);
				sum += Unsafe.Add(ref c, 10) * (long)Unsafe.Add(ref o, i + 8);
				sum += Unsafe.Add(ref c, 9) * (long)Unsafe.Add(ref o, i + 9);
				sum += Unsafe.Add(ref c, 8) * (long)Unsafe.Add(ref o, i + 10);
				sum += Unsafe.Add(ref c, 7) * (long)Unsafe.Add(ref o, i + 11);
				sum += Unsafe.Add(ref c, 6) * (long)Unsafe.Add(ref o, i + 12);
				sum += Unsafe.Add(ref c, 5) * (long)Unsafe.Add(ref o, i + 13);
				sum += Unsafe.Add(ref c, 4) * (long)Unsafe.Add(ref o, i + 14);
				sum += Unsafe.Add(ref c, 3) * (long)Unsafe.Add(ref o, i + 15);
				sum += Unsafe.Add(ref c, 2) * (long)Unsafe.Add(ref o, i + 16);
				sum += Unsafe.Add(ref c, 1) * (long)Unsafe.Add(ref o, i + 17);
				sum += c * (long)Unsafe.Add(ref o, i + 18);
                sum >>= shiftsNeeded;
                sum += Unsafe.Add(ref r, i);
                Unsafe.Add(ref d, i) = (int)sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder20Wide(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 20;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            ref var c = ref MemoryMarshal.GetReference(coeffs);
            long sum = 0;
            ref var o = ref MemoryMarshal.GetReference(output);
            ref var d = ref Unsafe.Add(ref o, Order);
            int dataLength = output.Length - Order;
            ref var r = ref MemoryMarshal.GetReference(residual);
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += Unsafe.Add(ref c, 19) * (long)Unsafe.Add(ref o, i + 0);
				sum += Unsafe.Add(ref c, 18) * (long)Unsafe.Add(ref o, i + 1);
				sum += Unsafe.Add(ref c, 17) * (long)Unsafe.Add(ref o, i + 2);
				sum += Unsafe.Add(ref c, 16) * (long)Unsafe.Add(ref o, i + 3);
				sum += Unsafe.Add(ref c, 15) * (long)Unsafe.Add(ref o, i + 4);
				sum += Unsafe.Add(ref c, 14) * (long)Unsafe.Add(ref o, i + 5);
				sum += Unsafe.Add(ref c, 13) * (long)Unsafe.Add(ref o, i + 6);
				sum += Unsafe.Add(ref c, 12) * (long)Unsafe.Add(ref o, i + 7);
				sum += Unsafe.Add(ref c, 11) * (long)Unsafe.Add(ref o, i + 8);
				sum += Unsafe.Add(ref c, 10) * (long)Unsafe.Add(ref o, i + 9);
				sum += Unsafe.Add(ref c, 9) * (long)Unsafe.Add(ref o, i + 10);
				sum += Unsafe.Add(ref c, 8) * (long)Unsafe.Add(ref o, i + 11);
				sum += Unsafe.Add(ref c, 7) * (long)Unsafe.Add(ref o, i + 12);
				sum += Unsafe.Add(ref c, 6) * (long)Unsafe.Add(ref o, i + 13);
				sum += Unsafe.Add(ref c, 5) * (long)Unsafe.Add(ref o, i + 14);
				sum += Unsafe.Add(ref c, 4) * (long)Unsafe.Add(ref o, i + 15);
				sum += Unsafe.Add(ref c, 3) * (long)Unsafe.Add(ref o, i + 16);
				sum += Unsafe.Add(ref c, 2) * (long)Unsafe.Add(ref o, i + 17);
				sum += Unsafe.Add(ref c, 1) * (long)Unsafe.Add(ref o, i + 18);
				sum += c * (long)Unsafe.Add(ref o, i + 19);
                sum >>= shiftsNeeded;
                sum += Unsafe.Add(ref r, i);
                Unsafe.Add(ref d, i) = (int)sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder21Wide(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 21;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            ref var c = ref MemoryMarshal.GetReference(coeffs);
            long sum = 0;
            ref var o = ref MemoryMarshal.GetReference(output);
            ref var d = ref Unsafe.Add(ref o, Order);
            int dataLength = output.Length - Order;
            ref var r = ref MemoryMarshal.GetReference(residual);
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += Unsafe.Add(ref c, 20) * (long)Unsafe.Add(ref o, i + 0);
				sum += Unsafe.Add(ref c, 19) * (long)Unsafe.Add(ref o, i + 1);
				sum += Unsafe.Add(ref c, 18) * (long)Unsafe.Add(ref o, i + 2);
				sum += Unsafe.Add(ref c, 17) * (long)Unsafe.Add(ref o, i + 3);
				sum += Unsafe.Add(ref c, 16) * (long)Unsafe.Add(ref o, i + 4);
				sum += Unsafe.Add(ref c, 15) * (long)Unsafe.Add(ref o, i + 5);
				sum += Unsafe.Add(ref c, 14) * (long)Unsafe.Add(ref o, i + 6);
				sum += Unsafe.Add(ref c, 13) * (long)Unsafe.Add(ref o, i + 7);
				sum += Unsafe.Add(ref c, 12) * (long)Unsafe.Add(ref o, i + 8);
				sum += Unsafe.Add(ref c, 11) * (long)Unsafe.Add(ref o, i + 9);
				sum += Unsafe.Add(ref c, 10) * (long)Unsafe.Add(ref o, i + 10);
				sum += Unsafe.Add(ref c, 9) * (long)Unsafe.Add(ref o, i + 11);
				sum += Unsafe.Add(ref c, 8) * (long)Unsafe.Add(ref o, i + 12);
				sum += Unsafe.Add(ref c, 7) * (long)Unsafe.Add(ref o, i + 13);
				sum += Unsafe.Add(ref c, 6) * (long)Unsafe.Add(ref o, i + 14);
				sum += Unsafe.Add(ref c, 5) * (long)Unsafe.Add(ref o, i + 15);
				sum += Unsafe.Add(ref c, 4) * (long)Unsafe.Add(ref o, i + 16);
				sum += Unsafe.Add(ref c, 3) * (long)Unsafe.Add(ref o, i + 17);
				sum += Unsafe.Add(ref c, 2) * (long)Unsafe.Add(ref o, i + 18);
				sum += Unsafe.Add(ref c, 1) * (long)Unsafe.Add(ref o, i + 19);
				sum += c * (long)Unsafe.Add(ref o, i + 20);
                sum >>= shiftsNeeded;
                sum += Unsafe.Add(ref r, i);
                Unsafe.Add(ref d, i) = (int)sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder22Wide(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 22;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            ref var c = ref MemoryMarshal.GetReference(coeffs);
            long sum = 0;
            ref var o = ref MemoryMarshal.GetReference(output);
            ref var d = ref Unsafe.Add(ref o, Order);
            int dataLength = output.Length - Order;
            ref var r = ref MemoryMarshal.GetReference(residual);
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += Unsafe.Add(ref c, 21) * (long)Unsafe.Add(ref o, i + 0);
				sum += Unsafe.Add(ref c, 20) * (long)Unsafe.Add(ref o, i + 1);
				sum += Unsafe.Add(ref c, 19) * (long)Unsafe.Add(ref o, i + 2);
				sum += Unsafe.Add(ref c, 18) * (long)Unsafe.Add(ref o, i + 3);
				sum += Unsafe.Add(ref c, 17) * (long)Unsafe.Add(ref o, i + 4);
				sum += Unsafe.Add(ref c, 16) * (long)Unsafe.Add(ref o, i + 5);
				sum += Unsafe.Add(ref c, 15) * (long)Unsafe.Add(ref o, i + 6);
				sum += Unsafe.Add(ref c, 14) * (long)Unsafe.Add(ref o, i + 7);
				sum += Unsafe.Add(ref c, 13) * (long)Unsafe.Add(ref o, i + 8);
				sum += Unsafe.Add(ref c, 12) * (long)Unsafe.Add(ref o, i + 9);
				sum += Unsafe.Add(ref c, 11) * (long)Unsafe.Add(ref o, i + 10);
				sum += Unsafe.Add(ref c, 10) * (long)Unsafe.Add(ref o, i + 11);
				sum += Unsafe.Add(ref c, 9) * (long)Unsafe.Add(ref o, i + 12);
				sum += Unsafe.Add(ref c, 8) * (long)Unsafe.Add(ref o, i + 13);
				sum += Unsafe.Add(ref c, 7) * (long)Unsafe.Add(ref o, i + 14);
				sum += Unsafe.Add(ref c, 6) * (long)Unsafe.Add(ref o, i + 15);
				sum += Unsafe.Add(ref c, 5) * (long)Unsafe.Add(ref o, i + 16);
				sum += Unsafe.Add(ref c, 4) * (long)Unsafe.Add(ref o, i + 17);
				sum += Unsafe.Add(ref c, 3) * (long)Unsafe.Add(ref o, i + 18);
				sum += Unsafe.Add(ref c, 2) * (long)Unsafe.Add(ref o, i + 19);
				sum += Unsafe.Add(ref c, 1) * (long)Unsafe.Add(ref o, i + 20);
				sum += c * (long)Unsafe.Add(ref o, i + 21);
                sum >>= shiftsNeeded;
                sum += Unsafe.Add(ref r, i);
                Unsafe.Add(ref d, i) = (int)sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder23Wide(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 23;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            ref var c = ref MemoryMarshal.GetReference(coeffs);
            long sum = 0;
            ref var o = ref MemoryMarshal.GetReference(output);
            ref var d = ref Unsafe.Add(ref o, Order);
            int dataLength = output.Length - Order;
            ref var r = ref MemoryMarshal.GetReference(residual);
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += Unsafe.Add(ref c, 22) * (long)Unsafe.Add(ref o, i + 0);
				sum += Unsafe.Add(ref c, 21) * (long)Unsafe.Add(ref o, i + 1);
				sum += Unsafe.Add(ref c, 20) * (long)Unsafe.Add(ref o, i + 2);
				sum += Unsafe.Add(ref c, 19) * (long)Unsafe.Add(ref o, i + 3);
				sum += Unsafe.Add(ref c, 18) * (long)Unsafe.Add(ref o, i + 4);
				sum += Unsafe.Add(ref c, 17) * (long)Unsafe.Add(ref o, i + 5);
				sum += Unsafe.Add(ref c, 16) * (long)Unsafe.Add(ref o, i + 6);
				sum += Unsafe.Add(ref c, 15) * (long)Unsafe.Add(ref o, i + 7);
				sum += Unsafe.Add(ref c, 14) * (long)Unsafe.Add(ref o, i + 8);
				sum += Unsafe.Add(ref c, 13) * (long)Unsafe.Add(ref o, i + 9);
				sum += Unsafe.Add(ref c, 12) * (long)Unsafe.Add(ref o, i + 10);
				sum += Unsafe.Add(ref c, 11) * (long)Unsafe.Add(ref o, i + 11);
				sum += Unsafe.Add(ref c, 10) * (long)Unsafe.Add(ref o, i + 12);
				sum += Unsafe.Add(ref c, 9) * (long)Unsafe.Add(ref o, i + 13);
				sum += Unsafe.Add(ref c, 8) * (long)Unsafe.Add(ref o, i + 14);
				sum += Unsafe.Add(ref c, 7) * (long)Unsafe.Add(ref o, i + 15);
				sum += Unsafe.Add(ref c, 6) * (long)Unsafe.Add(ref o, i + 16);
				sum += Unsafe.Add(ref c, 5) * (long)Unsafe.Add(ref o, i + 17);
				sum += Unsafe.Add(ref c, 4) * (long)Unsafe.Add(ref o, i + 18);
				sum += Unsafe.Add(ref c, 3) * (long)Unsafe.Add(ref o, i + 19);
				sum += Unsafe.Add(ref c, 2) * (long)Unsafe.Add(ref o, i + 20);
				sum += Unsafe.Add(ref c, 1) * (long)Unsafe.Add(ref o, i + 21);
				sum += c * (long)Unsafe.Add(ref o, i + 22);
                sum >>= shiftsNeeded;
                sum += Unsafe.Add(ref r, i);
                Unsafe.Add(ref d, i) = (int)sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder24Wide(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 24;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            ref var c = ref MemoryMarshal.GetReference(coeffs);
            long sum = 0;
            ref var o = ref MemoryMarshal.GetReference(output);
            ref var d = ref Unsafe.Add(ref o, Order);
            int dataLength = output.Length - Order;
            ref var r = ref MemoryMarshal.GetReference(residual);
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += Unsafe.Add(ref c, 23) * (long)Unsafe.Add(ref o, i + 0);
				sum += Unsafe.Add(ref c, 22) * (long)Unsafe.Add(ref o, i + 1);
				sum += Unsafe.Add(ref c, 21) * (long)Unsafe.Add(ref o, i + 2);
				sum += Unsafe.Add(ref c, 20) * (long)Unsafe.Add(ref o, i + 3);
				sum += Unsafe.Add(ref c, 19) * (long)Unsafe.Add(ref o, i + 4);
				sum += Unsafe.Add(ref c, 18) * (long)Unsafe.Add(ref o, i + 5);
				sum += Unsafe.Add(ref c, 17) * (long)Unsafe.Add(ref o, i + 6);
				sum += Unsafe.Add(ref c, 16) * (long)Unsafe.Add(ref o, i + 7);
				sum += Unsafe.Add(ref c, 15) * (long)Unsafe.Add(ref o, i + 8);
				sum += Unsafe.Add(ref c, 14) * (long)Unsafe.Add(ref o, i + 9);
				sum += Unsafe.Add(ref c, 13) * (long)Unsafe.Add(ref o, i + 10);
				sum += Unsafe.Add(ref c, 12) * (long)Unsafe.Add(ref o, i + 11);
				sum += Unsafe.Add(ref c, 11) * (long)Unsafe.Add(ref o, i + 12);
				sum += Unsafe.Add(ref c, 10) * (long)Unsafe.Add(ref o, i + 13);
				sum += Unsafe.Add(ref c, 9) * (long)Unsafe.Add(ref o, i + 14);
				sum += Unsafe.Add(ref c, 8) * (long)Unsafe.Add(ref o, i + 15);
				sum += Unsafe.Add(ref c, 7) * (long)Unsafe.Add(ref o, i + 16);
				sum += Unsafe.Add(ref c, 6) * (long)Unsafe.Add(ref o, i + 17);
				sum += Unsafe.Add(ref c, 5) * (long)Unsafe.Add(ref o, i + 18);
				sum += Unsafe.Add(ref c, 4) * (long)Unsafe.Add(ref o, i + 19);
				sum += Unsafe.Add(ref c, 3) * (long)Unsafe.Add(ref o, i + 20);
				sum += Unsafe.Add(ref c, 2) * (long)Unsafe.Add(ref o, i + 21);
				sum += Unsafe.Add(ref c, 1) * (long)Unsafe.Add(ref o, i + 22);
				sum += c * (long)Unsafe.Add(ref o, i + 23);
                sum >>= shiftsNeeded;
                sum += Unsafe.Add(ref r, i);
                Unsafe.Add(ref d, i) = (int)sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder25Wide(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 25;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            ref var c = ref MemoryMarshal.GetReference(coeffs);
            long sum = 0;
            ref var o = ref MemoryMarshal.GetReference(output);
            ref var d = ref Unsafe.Add(ref o, Order);
            int dataLength = output.Length - Order;
            ref var r = ref MemoryMarshal.GetReference(residual);
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += Unsafe.Add(ref c, 24) * (long)Unsafe.Add(ref o, i + 0);
				sum += Unsafe.Add(ref c, 23) * (long)Unsafe.Add(ref o, i + 1);
				sum += Unsafe.Add(ref c, 22) * (long)Unsafe.Add(ref o, i + 2);
				sum += Unsafe.Add(ref c, 21) * (long)Unsafe.Add(ref o, i + 3);
				sum += Unsafe.Add(ref c, 20) * (long)Unsafe.Add(ref o, i + 4);
				sum += Unsafe.Add(ref c, 19) * (long)Unsafe.Add(ref o, i + 5);
				sum += Unsafe.Add(ref c, 18) * (long)Unsafe.Add(ref o, i + 6);
				sum += Unsafe.Add(ref c, 17) * (long)Unsafe.Add(ref o, i + 7);
				sum += Unsafe.Add(ref c, 16) * (long)Unsafe.Add(ref o, i + 8);
				sum += Unsafe.Add(ref c, 15) * (long)Unsafe.Add(ref o, i + 9);
				sum += Unsafe.Add(ref c, 14) * (long)Unsafe.Add(ref o, i + 10);
				sum += Unsafe.Add(ref c, 13) * (long)Unsafe.Add(ref o, i + 11);
				sum += Unsafe.Add(ref c, 12) * (long)Unsafe.Add(ref o, i + 12);
				sum += Unsafe.Add(ref c, 11) * (long)Unsafe.Add(ref o, i + 13);
				sum += Unsafe.Add(ref c, 10) * (long)Unsafe.Add(ref o, i + 14);
				sum += Unsafe.Add(ref c, 9) * (long)Unsafe.Add(ref o, i + 15);
				sum += Unsafe.Add(ref c, 8) * (long)Unsafe.Add(ref o, i + 16);
				sum += Unsafe.Add(ref c, 7) * (long)Unsafe.Add(ref o, i + 17);
				sum += Unsafe.Add(ref c, 6) * (long)Unsafe.Add(ref o, i + 18);
				sum += Unsafe.Add(ref c, 5) * (long)Unsafe.Add(ref o, i + 19);
				sum += Unsafe.Add(ref c, 4) * (long)Unsafe.Add(ref o, i + 20);
				sum += Unsafe.Add(ref c, 3) * (long)Unsafe.Add(ref o, i + 21);
				sum += Unsafe.Add(ref c, 2) * (long)Unsafe.Add(ref o, i + 22);
				sum += Unsafe.Add(ref c, 1) * (long)Unsafe.Add(ref o, i + 23);
				sum += c * (long)Unsafe.Add(ref o, i + 24);
                sum >>= shiftsNeeded;
                sum += Unsafe.Add(ref r, i);
                Unsafe.Add(ref d, i) = (int)sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder26Wide(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 26;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            ref var c = ref MemoryMarshal.GetReference(coeffs);
            long sum = 0;
            ref var o = ref MemoryMarshal.GetReference(output);
            ref var d = ref Unsafe.Add(ref o, Order);
            int dataLength = output.Length - Order;
            ref var r = ref MemoryMarshal.GetReference(residual);
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += Unsafe.Add(ref c, 25) * (long)Unsafe.Add(ref o, i + 0);
				sum += Unsafe.Add(ref c, 24) * (long)Unsafe.Add(ref o, i + 1);
				sum += Unsafe.Add(ref c, 23) * (long)Unsafe.Add(ref o, i + 2);
				sum += Unsafe.Add(ref c, 22) * (long)Unsafe.Add(ref o, i + 3);
				sum += Unsafe.Add(ref c, 21) * (long)Unsafe.Add(ref o, i + 4);
				sum += Unsafe.Add(ref c, 20) * (long)Unsafe.Add(ref o, i + 5);
				sum += Unsafe.Add(ref c, 19) * (long)Unsafe.Add(ref o, i + 6);
				sum += Unsafe.Add(ref c, 18) * (long)Unsafe.Add(ref o, i + 7);
				sum += Unsafe.Add(ref c, 17) * (long)Unsafe.Add(ref o, i + 8);
				sum += Unsafe.Add(ref c, 16) * (long)Unsafe.Add(ref o, i + 9);
				sum += Unsafe.Add(ref c, 15) * (long)Unsafe.Add(ref o, i + 10);
				sum += Unsafe.Add(ref c, 14) * (long)Unsafe.Add(ref o, i + 11);
				sum += Unsafe.Add(ref c, 13) * (long)Unsafe.Add(ref o, i + 12);
				sum += Unsafe.Add(ref c, 12) * (long)Unsafe.Add(ref o, i + 13);
				sum += Unsafe.Add(ref c, 11) * (long)Unsafe.Add(ref o, i + 14);
				sum += Unsafe.Add(ref c, 10) * (long)Unsafe.Add(ref o, i + 15);
				sum += Unsafe.Add(ref c, 9) * (long)Unsafe.Add(ref o, i + 16);
				sum += Unsafe.Add(ref c, 8) * (long)Unsafe.Add(ref o, i + 17);
				sum += Unsafe.Add(ref c, 7) * (long)Unsafe.Add(ref o, i + 18);
				sum += Unsafe.Add(ref c, 6) * (long)Unsafe.Add(ref o, i + 19);
				sum += Unsafe.Add(ref c, 5) * (long)Unsafe.Add(ref o, i + 20);
				sum += Unsafe.Add(ref c, 4) * (long)Unsafe.Add(ref o, i + 21);
				sum += Unsafe.Add(ref c, 3) * (long)Unsafe.Add(ref o, i + 22);
				sum += Unsafe.Add(ref c, 2) * (long)Unsafe.Add(ref o, i + 23);
				sum += Unsafe.Add(ref c, 1) * (long)Unsafe.Add(ref o, i + 24);
				sum += c * (long)Unsafe.Add(ref o, i + 25);
                sum >>= shiftsNeeded;
                sum += Unsafe.Add(ref r, i);
                Unsafe.Add(ref d, i) = (int)sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder27Wide(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 27;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            ref var c = ref MemoryMarshal.GetReference(coeffs);
            long sum = 0;
            ref var o = ref MemoryMarshal.GetReference(output);
            ref var d = ref Unsafe.Add(ref o, Order);
            int dataLength = output.Length - Order;
            ref var r = ref MemoryMarshal.GetReference(residual);
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += Unsafe.Add(ref c, 26) * (long)Unsafe.Add(ref o, i + 0);
				sum += Unsafe.Add(ref c, 25) * (long)Unsafe.Add(ref o, i + 1);
				sum += Unsafe.Add(ref c, 24) * (long)Unsafe.Add(ref o, i + 2);
				sum += Unsafe.Add(ref c, 23) * (long)Unsafe.Add(ref o, i + 3);
				sum += Unsafe.Add(ref c, 22) * (long)Unsafe.Add(ref o, i + 4);
				sum += Unsafe.Add(ref c, 21) * (long)Unsafe.Add(ref o, i + 5);
				sum += Unsafe.Add(ref c, 20) * (long)Unsafe.Add(ref o, i + 6);
				sum += Unsafe.Add(ref c, 19) * (long)Unsafe.Add(ref o, i + 7);
				sum += Unsafe.Add(ref c, 18) * (long)Unsafe.Add(ref o, i + 8);
				sum += Unsafe.Add(ref c, 17) * (long)Unsafe.Add(ref o, i + 9);
				sum += Unsafe.Add(ref c, 16) * (long)Unsafe.Add(ref o, i + 10);
				sum += Unsafe.Add(ref c, 15) * (long)Unsafe.Add(ref o, i + 11);
				sum += Unsafe.Add(ref c, 14) * (long)Unsafe.Add(ref o, i + 12);
				sum += Unsafe.Add(ref c, 13) * (long)Unsafe.Add(ref o, i + 13);
				sum += Unsafe.Add(ref c, 12) * (long)Unsafe.Add(ref o, i + 14);
				sum += Unsafe.Add(ref c, 11) * (long)Unsafe.Add(ref o, i + 15);
				sum += Unsafe.Add(ref c, 10) * (long)Unsafe.Add(ref o, i + 16);
				sum += Unsafe.Add(ref c, 9) * (long)Unsafe.Add(ref o, i + 17);
				sum += Unsafe.Add(ref c, 8) * (long)Unsafe.Add(ref o, i + 18);
				sum += Unsafe.Add(ref c, 7) * (long)Unsafe.Add(ref o, i + 19);
				sum += Unsafe.Add(ref c, 6) * (long)Unsafe.Add(ref o, i + 20);
				sum += Unsafe.Add(ref c, 5) * (long)Unsafe.Add(ref o, i + 21);
				sum += Unsafe.Add(ref c, 4) * (long)Unsafe.Add(ref o, i + 22);
				sum += Unsafe.Add(ref c, 3) * (long)Unsafe.Add(ref o, i + 23);
				sum += Unsafe.Add(ref c, 2) * (long)Unsafe.Add(ref o, i + 24);
				sum += Unsafe.Add(ref c, 1) * (long)Unsafe.Add(ref o, i + 25);
				sum += c * (long)Unsafe.Add(ref o, i + 26);
                sum >>= shiftsNeeded;
                sum += Unsafe.Add(ref r, i);
                Unsafe.Add(ref d, i) = (int)sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder28Wide(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 28;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            ref var c = ref MemoryMarshal.GetReference(coeffs);
            long sum = 0;
            ref var o = ref MemoryMarshal.GetReference(output);
            ref var d = ref Unsafe.Add(ref o, Order);
            int dataLength = output.Length - Order;
            ref var r = ref MemoryMarshal.GetReference(residual);
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += Unsafe.Add(ref c, 27) * (long)Unsafe.Add(ref o, i + 0);
				sum += Unsafe.Add(ref c, 26) * (long)Unsafe.Add(ref o, i + 1);
				sum += Unsafe.Add(ref c, 25) * (long)Unsafe.Add(ref o, i + 2);
				sum += Unsafe.Add(ref c, 24) * (long)Unsafe.Add(ref o, i + 3);
				sum += Unsafe.Add(ref c, 23) * (long)Unsafe.Add(ref o, i + 4);
				sum += Unsafe.Add(ref c, 22) * (long)Unsafe.Add(ref o, i + 5);
				sum += Unsafe.Add(ref c, 21) * (long)Unsafe.Add(ref o, i + 6);
				sum += Unsafe.Add(ref c, 20) * (long)Unsafe.Add(ref o, i + 7);
				sum += Unsafe.Add(ref c, 19) * (long)Unsafe.Add(ref o, i + 8);
				sum += Unsafe.Add(ref c, 18) * (long)Unsafe.Add(ref o, i + 9);
				sum += Unsafe.Add(ref c, 17) * (long)Unsafe.Add(ref o, i + 10);
				sum += Unsafe.Add(ref c, 16) * (long)Unsafe.Add(ref o, i + 11);
				sum += Unsafe.Add(ref c, 15) * (long)Unsafe.Add(ref o, i + 12);
				sum += Unsafe.Add(ref c, 14) * (long)Unsafe.Add(ref o, i + 13);
				sum += Unsafe.Add(ref c, 13) * (long)Unsafe.Add(ref o, i + 14);
				sum += Unsafe.Add(ref c, 12) * (long)Unsafe.Add(ref o, i + 15);
				sum += Unsafe.Add(ref c, 11) * (long)Unsafe.Add(ref o, i + 16);
				sum += Unsafe.Add(ref c, 10) * (long)Unsafe.Add(ref o, i + 17);
				sum += Unsafe.Add(ref c, 9) * (long)Unsafe.Add(ref o, i + 18);
				sum += Unsafe.Add(ref c, 8) * (long)Unsafe.Add(ref o, i + 19);
				sum += Unsafe.Add(ref c, 7) * (long)Unsafe.Add(ref o, i + 20);
				sum += Unsafe.Add(ref c, 6) * (long)Unsafe.Add(ref o, i + 21);
				sum += Unsafe.Add(ref c, 5) * (long)Unsafe.Add(ref o, i + 22);
				sum += Unsafe.Add(ref c, 4) * (long)Unsafe.Add(ref o, i + 23);
				sum += Unsafe.Add(ref c, 3) * (long)Unsafe.Add(ref o, i + 24);
				sum += Unsafe.Add(ref c, 2) * (long)Unsafe.Add(ref o, i + 25);
				sum += Unsafe.Add(ref c, 1) * (long)Unsafe.Add(ref o, i + 26);
				sum += c * (long)Unsafe.Add(ref o, i + 27);
                sum >>= shiftsNeeded;
                sum += Unsafe.Add(ref r, i);
                Unsafe.Add(ref d, i) = (int)sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder29Wide(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 29;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            ref var c = ref MemoryMarshal.GetReference(coeffs);
            long sum = 0;
            ref var o = ref MemoryMarshal.GetReference(output);
            ref var d = ref Unsafe.Add(ref o, Order);
            int dataLength = output.Length - Order;
            ref var r = ref MemoryMarshal.GetReference(residual);
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += Unsafe.Add(ref c, 28) * (long)Unsafe.Add(ref o, i + 0);
				sum += Unsafe.Add(ref c, 27) * (long)Unsafe.Add(ref o, i + 1);
				sum += Unsafe.Add(ref c, 26) * (long)Unsafe.Add(ref o, i + 2);
				sum += Unsafe.Add(ref c, 25) * (long)Unsafe.Add(ref o, i + 3);
				sum += Unsafe.Add(ref c, 24) * (long)Unsafe.Add(ref o, i + 4);
				sum += Unsafe.Add(ref c, 23) * (long)Unsafe.Add(ref o, i + 5);
				sum += Unsafe.Add(ref c, 22) * (long)Unsafe.Add(ref o, i + 6);
				sum += Unsafe.Add(ref c, 21) * (long)Unsafe.Add(ref o, i + 7);
				sum += Unsafe.Add(ref c, 20) * (long)Unsafe.Add(ref o, i + 8);
				sum += Unsafe.Add(ref c, 19) * (long)Unsafe.Add(ref o, i + 9);
				sum += Unsafe.Add(ref c, 18) * (long)Unsafe.Add(ref o, i + 10);
				sum += Unsafe.Add(ref c, 17) * (long)Unsafe.Add(ref o, i + 11);
				sum += Unsafe.Add(ref c, 16) * (long)Unsafe.Add(ref o, i + 12);
				sum += Unsafe.Add(ref c, 15) * (long)Unsafe.Add(ref o, i + 13);
				sum += Unsafe.Add(ref c, 14) * (long)Unsafe.Add(ref o, i + 14);
				sum += Unsafe.Add(ref c, 13) * (long)Unsafe.Add(ref o, i + 15);
				sum += Unsafe.Add(ref c, 12) * (long)Unsafe.Add(ref o, i + 16);
				sum += Unsafe.Add(ref c, 11) * (long)Unsafe.Add(ref o, i + 17);
				sum += Unsafe.Add(ref c, 10) * (long)Unsafe.Add(ref o, i + 18);
				sum += Unsafe.Add(ref c, 9) * (long)Unsafe.Add(ref o, i + 19);
				sum += Unsafe.Add(ref c, 8) * (long)Unsafe.Add(ref o, i + 20);
				sum += Unsafe.Add(ref c, 7) * (long)Unsafe.Add(ref o, i + 21);
				sum += Unsafe.Add(ref c, 6) * (long)Unsafe.Add(ref o, i + 22);
				sum += Unsafe.Add(ref c, 5) * (long)Unsafe.Add(ref o, i + 23);
				sum += Unsafe.Add(ref c, 4) * (long)Unsafe.Add(ref o, i + 24);
				sum += Unsafe.Add(ref c, 3) * (long)Unsafe.Add(ref o, i + 25);
				sum += Unsafe.Add(ref c, 2) * (long)Unsafe.Add(ref o, i + 26);
				sum += Unsafe.Add(ref c, 1) * (long)Unsafe.Add(ref o, i + 27);
				sum += c * (long)Unsafe.Add(ref o, i + 28);
                sum >>= shiftsNeeded;
                sum += Unsafe.Add(ref r, i);
                Unsafe.Add(ref d, i) = (int)sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder30Wide(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 30;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            ref var c = ref MemoryMarshal.GetReference(coeffs);
            long sum = 0;
            ref var o = ref MemoryMarshal.GetReference(output);
            ref var d = ref Unsafe.Add(ref o, Order);
            int dataLength = output.Length - Order;
            ref var r = ref MemoryMarshal.GetReference(residual);
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += Unsafe.Add(ref c, 29) * (long)Unsafe.Add(ref o, i + 0);
				sum += Unsafe.Add(ref c, 28) * (long)Unsafe.Add(ref o, i + 1);
				sum += Unsafe.Add(ref c, 27) * (long)Unsafe.Add(ref o, i + 2);
				sum += Unsafe.Add(ref c, 26) * (long)Unsafe.Add(ref o, i + 3);
				sum += Unsafe.Add(ref c, 25) * (long)Unsafe.Add(ref o, i + 4);
				sum += Unsafe.Add(ref c, 24) * (long)Unsafe.Add(ref o, i + 5);
				sum += Unsafe.Add(ref c, 23) * (long)Unsafe.Add(ref o, i + 6);
				sum += Unsafe.Add(ref c, 22) * (long)Unsafe.Add(ref o, i + 7);
				sum += Unsafe.Add(ref c, 21) * (long)Unsafe.Add(ref o, i + 8);
				sum += Unsafe.Add(ref c, 20) * (long)Unsafe.Add(ref o, i + 9);
				sum += Unsafe.Add(ref c, 19) * (long)Unsafe.Add(ref o, i + 10);
				sum += Unsafe.Add(ref c, 18) * (long)Unsafe.Add(ref o, i + 11);
				sum += Unsafe.Add(ref c, 17) * (long)Unsafe.Add(ref o, i + 12);
				sum += Unsafe.Add(ref c, 16) * (long)Unsafe.Add(ref o, i + 13);
				sum += Unsafe.Add(ref c, 15) * (long)Unsafe.Add(ref o, i + 14);
				sum += Unsafe.Add(ref c, 14) * (long)Unsafe.Add(ref o, i + 15);
				sum += Unsafe.Add(ref c, 13) * (long)Unsafe.Add(ref o, i + 16);
				sum += Unsafe.Add(ref c, 12) * (long)Unsafe.Add(ref o, i + 17);
				sum += Unsafe.Add(ref c, 11) * (long)Unsafe.Add(ref o, i + 18);
				sum += Unsafe.Add(ref c, 10) * (long)Unsafe.Add(ref o, i + 19);
				sum += Unsafe.Add(ref c, 9) * (long)Unsafe.Add(ref o, i + 20);
				sum += Unsafe.Add(ref c, 8) * (long)Unsafe.Add(ref o, i + 21);
				sum += Unsafe.Add(ref c, 7) * (long)Unsafe.Add(ref o, i + 22);
				sum += Unsafe.Add(ref c, 6) * (long)Unsafe.Add(ref o, i + 23);
				sum += Unsafe.Add(ref c, 5) * (long)Unsafe.Add(ref o, i + 24);
				sum += Unsafe.Add(ref c, 4) * (long)Unsafe.Add(ref o, i + 25);
				sum += Unsafe.Add(ref c, 3) * (long)Unsafe.Add(ref o, i + 26);
				sum += Unsafe.Add(ref c, 2) * (long)Unsafe.Add(ref o, i + 27);
				sum += Unsafe.Add(ref c, 1) * (long)Unsafe.Add(ref o, i + 28);
				sum += c * (long)Unsafe.Add(ref o, i + 29);
                sum >>= shiftsNeeded;
                sum += Unsafe.Add(ref r, i);
                Unsafe.Add(ref d, i) = (int)sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder31Wide(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 31;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            ref var c = ref MemoryMarshal.GetReference(coeffs);
            long sum = 0;
            ref var o = ref MemoryMarshal.GetReference(output);
            ref var d = ref Unsafe.Add(ref o, Order);
            int dataLength = output.Length - Order;
            ref var r = ref MemoryMarshal.GetReference(residual);
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += Unsafe.Add(ref c, 30) * (long)Unsafe.Add(ref o, i + 0);
				sum += Unsafe.Add(ref c, 29) * (long)Unsafe.Add(ref o, i + 1);
				sum += Unsafe.Add(ref c, 28) * (long)Unsafe.Add(ref o, i + 2);
				sum += Unsafe.Add(ref c, 27) * (long)Unsafe.Add(ref o, i + 3);
				sum += Unsafe.Add(ref c, 26) * (long)Unsafe.Add(ref o, i + 4);
				sum += Unsafe.Add(ref c, 25) * (long)Unsafe.Add(ref o, i + 5);
				sum += Unsafe.Add(ref c, 24) * (long)Unsafe.Add(ref o, i + 6);
				sum += Unsafe.Add(ref c, 23) * (long)Unsafe.Add(ref o, i + 7);
				sum += Unsafe.Add(ref c, 22) * (long)Unsafe.Add(ref o, i + 8);
				sum += Unsafe.Add(ref c, 21) * (long)Unsafe.Add(ref o, i + 9);
				sum += Unsafe.Add(ref c, 20) * (long)Unsafe.Add(ref o, i + 10);
				sum += Unsafe.Add(ref c, 19) * (long)Unsafe.Add(ref o, i + 11);
				sum += Unsafe.Add(ref c, 18) * (long)Unsafe.Add(ref o, i + 12);
				sum += Unsafe.Add(ref c, 17) * (long)Unsafe.Add(ref o, i + 13);
				sum += Unsafe.Add(ref c, 16) * (long)Unsafe.Add(ref o, i + 14);
				sum += Unsafe.Add(ref c, 15) * (long)Unsafe.Add(ref o, i + 15);
				sum += Unsafe.Add(ref c, 14) * (long)Unsafe.Add(ref o, i + 16);
				sum += Unsafe.Add(ref c, 13) * (long)Unsafe.Add(ref o, i + 17);
				sum += Unsafe.Add(ref c, 12) * (long)Unsafe.Add(ref o, i + 18);
				sum += Unsafe.Add(ref c, 11) * (long)Unsafe.Add(ref o, i + 19);
				sum += Unsafe.Add(ref c, 10) * (long)Unsafe.Add(ref o, i + 20);
				sum += Unsafe.Add(ref c, 9) * (long)Unsafe.Add(ref o, i + 21);
				sum += Unsafe.Add(ref c, 8) * (long)Unsafe.Add(ref o, i + 22);
				sum += Unsafe.Add(ref c, 7) * (long)Unsafe.Add(ref o, i + 23);
				sum += Unsafe.Add(ref c, 6) * (long)Unsafe.Add(ref o, i + 24);
				sum += Unsafe.Add(ref c, 5) * (long)Unsafe.Add(ref o, i + 25);
				sum += Unsafe.Add(ref c, 4) * (long)Unsafe.Add(ref o, i + 26);
				sum += Unsafe.Add(ref c, 3) * (long)Unsafe.Add(ref o, i + 27);
				sum += Unsafe.Add(ref c, 2) * (long)Unsafe.Add(ref o, i + 28);
				sum += Unsafe.Add(ref c, 1) * (long)Unsafe.Add(ref o, i + 29);
				sum += c * (long)Unsafe.Add(ref o, i + 30);
                sum >>= shiftsNeeded;
                sum += Unsafe.Add(ref r, i);
                Unsafe.Add(ref d, i) = (int)sum;
            }
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static unsafe void RestoreSignalOrder32Wide(int shiftsNeeded, ReadOnlySpan<int> residual, ReadOnlySpan<int> coeffs, Span<int> output)
        {
            const int Order = 32;
            if(coeffs.Length < Order) return;
            _ = coeffs[Order - 1];
            ref var c = ref MemoryMarshal.GetReference(coeffs);
            long sum = 0;
            ref var o = ref MemoryMarshal.GetReference(output);
            ref var d = ref Unsafe.Add(ref o, Order);
            int dataLength = output.Length - Order;
            ref var r = ref MemoryMarshal.GetReference(residual);
            for(int i = 0; i < dataLength; i++)
            {
                sum = 0;
				sum += Unsafe.Add(ref c, 31) * (long)Unsafe.Add(ref o, i + 0);
				sum += Unsafe.Add(ref c, 30) * (long)Unsafe.Add(ref o, i + 1);
				sum += Unsafe.Add(ref c, 29) * (long)Unsafe.Add(ref o, i + 2);
				sum += Unsafe.Add(ref c, 28) * (long)Unsafe.Add(ref o, i + 3);
				sum += Unsafe.Add(ref c, 27) * (long)Unsafe.Add(ref o, i + 4);
				sum += Unsafe.Add(ref c, 26) * (long)Unsafe.Add(ref o, i + 5);
				sum += Unsafe.Add(ref c, 25) * (long)Unsafe.Add(ref o, i + 6);
				sum += Unsafe.Add(ref c, 24) * (long)Unsafe.Add(ref o, i + 7);
				sum += Unsafe.Add(ref c, 23) * (long)Unsafe.Add(ref o, i + 8);
				sum += Unsafe.Add(ref c, 22) * (long)Unsafe.Add(ref o, i + 9);
				sum += Unsafe.Add(ref c, 21) * (long)Unsafe.Add(ref o, i + 10);
				sum += Unsafe.Add(ref c, 20) * (long)Unsafe.Add(ref o, i + 11);
				sum += Unsafe.Add(ref c, 19) * (long)Unsafe.Add(ref o, i + 12);
				sum += Unsafe.Add(ref c, 18) * (long)Unsafe.Add(ref o, i + 13);
				sum += Unsafe.Add(ref c, 17) * (long)Unsafe.Add(ref o, i + 14);
				sum += Unsafe.Add(ref c, 16) * (long)Unsafe.Add(ref o, i + 15);
				sum += Unsafe.Add(ref c, 15) * (long)Unsafe.Add(ref o, i + 16);
				sum += Unsafe.Add(ref c, 14) * (long)Unsafe.Add(ref o, i + 17);
				sum += Unsafe.Add(ref c, 13) * (long)Unsafe.Add(ref o, i + 18);
				sum += Unsafe.Add(ref c, 12) * (long)Unsafe.Add(ref o, i + 19);
				sum += Unsafe.Add(ref c, 11) * (long)Unsafe.Add(ref o, i + 20);
				sum += Unsafe.Add(ref c, 10) * (long)Unsafe.Add(ref o, i + 21);
				sum += Unsafe.Add(ref c, 9) * (long)Unsafe.Add(ref o, i + 22);
				sum += Unsafe.Add(ref c, 8) * (long)Unsafe.Add(ref o, i + 23);
				sum += Unsafe.Add(ref c, 7) * (long)Unsafe.Add(ref o, i + 24);
				sum += Unsafe.Add(ref c, 6) * (long)Unsafe.Add(ref o, i + 25);
				sum += Unsafe.Add(ref c, 5) * (long)Unsafe.Add(ref o, i + 26);
				sum += Unsafe.Add(ref c, 4) * (long)Unsafe.Add(ref o, i + 27);
				sum += Unsafe.Add(ref c, 3) * (long)Unsafe.Add(ref o, i + 28);
				sum += Unsafe.Add(ref c, 2) * (long)Unsafe.Add(ref o, i + 29);
				sum += Unsafe.Add(ref c, 1) * (long)Unsafe.Add(ref o, i + 30);
				sum += c * (long)Unsafe.Add(ref o, i + 31);
                sum >>= shiftsNeeded;
                sum += Unsafe.Add(ref r, i);
                Unsafe.Add(ref d, i) = (int)sum;
            }
        }
    }
}
