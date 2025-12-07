using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Shamisen.Data.Binary
{

    public readonly partial struct WordBitPosition<TWord> where TWord : unmanaged, IComparable<TWord>, IEquatable<TWord>, IFormattable, IParsable<TWord>, ISpanFormattable, ISpanParsable<TWord>, IUtf8SpanFormattable, IUtf8SpanParsable<TWord>, IAdditionOperators<TWord, TWord, TWord>, IAdditiveIdentity<TWord, TWord>, IBinaryInteger<TWord>, IBinaryNumber<TWord>, IBitwiseOperators<TWord, TWord, TWord>, IComparisonOperators<TWord, TWord, bool>, IEqualityOperators<TWord, TWord, bool>, IDecrementOperators<TWord>, IDivisionOperators<TWord, TWord, TWord>, IIncrementOperators<TWord>, IModulusOperators<TWord, TWord, TWord>, IMultiplicativeIdentity<TWord, TWord>, IMultiplyOperators<TWord, TWord, TWord>, INumber<TWord>, INumberBase<TWord>, ISubtractionOperators<TWord, TWord, TWord>, IUnaryNegationOperators<TWord, TWord>, IUnaryPlusOperators<TWord, TWord>, IShiftOperators<TWord, int, TWord>, IMinMaxValue<TWord>, IUnsignedNumber<TWord>
    {
        private readonly ulong value;

        public void Deconstruct(out ulong wordIndex, out byte bitOffset)
        {
            var bitsPerWord = (ulong)Unsafe.SizeOf<TWord>() * 8;
            wordIndex = value / bitsPerWord;
            bitOffset = (byte)(value % bitsPerWord);
        }

    }
}
