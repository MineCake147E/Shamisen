# Shamisen.Utils - Utility features independent from [Shamisen](https://github.com/MineCake147E/Shamisen)

This library provides utility features independent from [Shamisen.Core](https://github.com/MineCake147E/Shamisen).

## Currently implemented features in Shamisen.Utils

### SIMD-Optimized Array Utilities

- Scalar operations for each elements of array
  - Scalar Multiplication(Attenuation)
    - Single
    - Double
  - DC Offset
    - Int32
  - Log2 Approximation by 5th order polynomial
    - Single
  - Log10 Approximation by 5th order polynomial
    - Single
  - NaN elimination by replacing
    - Single
- Element-wise operations for two or more arrays
  - Addition
    - Single
  - Multiplication
    - Single
    - Double
  - Mixing
    - Single
- Special operations
  - Interleave
    - Int32
  - Monaural-to-multichannel Duplication
    - Single
  - Max and Min across array
    - Single

### Fast Floating-Point Math

- Architecture-Dependent Single-Instruction Max
  - Single
  - Double
- Architecture-Dependent Single-Instruction Min
  - Single
  - Double
- `MathF.Round` polyfill
  - Single

### Fast Integer Math

- Max
  - Int32
  - UInt32
  - IntPtr
- Min
  - Int32
  - UInt32
  - IntPtr
- AndNot(~A & B)
  - Int32
  - Int64
  - UInt32
  - UInt64
  - IntPtr
  - UIntPtr
- Abstract Value(abs)
  - Int32(returns UInt32)
  - Int64(returns UInt64)
- `Math.BigMul` polyfill
  - Int64
  - UInt64
- Modular Multiplicative Inverse
  - Int32
- `BitOperations.TrailingZeroCount` polyfill
  - UInt32
  - UInt64
- Binary logarithm(returns Int32)
  - UInt32
  - UInt64
- `BitOperations.LeadingZeroCount` polyfill
  - UInt32
  - UInt64
- `BitOperations.PopCount` polyfill
  - UInt32
  - UInt64
- Extracting highest set bit
  - UInt32
  - UInt64
- Bit order reversal
  - UInt32
  - UInt64
- Bit-field extraction
  - UInt32
  - UInt64
- Zeroing higher bits than specified
  - UInt32
  - UInt64
- Zeroing specified higher bits
  - UInt32
  - UInt64
- Checking if the number is a power-of-two
  - Int32
  - UInt32

### Fixed-Point Formats

| Format | Type |
|--|--|
| Q0.63 | `Fixed64` |
| Q0.31 | `Fixed32` |
| Q0.15 | `Fixed16` |

### Vector Utilities

- Utilities for `System.Numerics.Vector4` and `System.Numerics.Vector<T>`

## License

### Shamisen.Utils itself

[Apache License 2.0](http://www.apache.org/licenses/LICENSE-2.0)
