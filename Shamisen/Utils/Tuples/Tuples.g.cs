using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Utils.Tuples
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly partial struct UnmanagedTupleX2<T> where T:unmanaged
    {
        private readonly T i0, i1;
        /// <summary>
        /// The item No.0.
        /// </summary>
        public T Item0
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX2<T>, T>(ref Unsafe.AsRef(in this)), 0);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX2<T>, T>(ref Unsafe.AsRef(in this)), 0) = value;
        }

        /// <summary>
        /// The item No.1.
        /// </summary>
        public T Item1
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX2<T>, T>(ref Unsafe.AsRef(in this)), 1);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX2<T>, T>(ref Unsafe.AsRef(in this)), 1) = value;
        }

        /// <summary>
        /// Initializes this <see cref="UnmanagedTupleX2{T}"/> from values.
        /// </summary>
        /// <param name="v0">The value No.0.</param>
        /// <param name="v1">The value No.1.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public UnmanagedTupleX2(T v0, T v1)
        {
            i0 = v0;
            i1 = v1;
        }

        /// <summary>
        /// Deconstructs this <see cref="UnmanagedTupleX2{T}"/> to values.
        /// </summary>
        /// <param name="v0">The output value No.0.</param>
        /// <param name="v1">The output value No.1.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public void Deconstruct(out T v0, out T v1)
        {
            v0 = Item0;
            v1 = Item1;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="UnmanagedTupleX2{T}"/> to <see cref="ValueTuple{T, T}"/>.
        /// </summary>
        /// <param name="from">The converting <see cref="UnmanagedTupleX2{T}"/>.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static implicit operator (T, T)(UnmanagedTupleX2<T> from)
            => Unsafe.As<UnmanagedTupleX2<T>, (T, T)>(ref from);

        /// <summary>
        /// Performs an implicit conversion from <see cref="ValueTuple{T, T}"/> to <see cref="UnmanagedTupleX2{T}"/>.
        /// </summary>
        /// <param name="from">The converting <see cref="ValueTuple{T0, T1}"/>.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static implicit operator UnmanagedTupleX2<T>((T, T) from)
            => Unsafe.As<(T, T), UnmanagedTupleX2<T>>(ref from);
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly partial struct UnmanagedTupleX3<T> where T:unmanaged
    {
        private readonly T i0, i1, i2;
        /// <summary>
        /// The item No.0.
        /// </summary>
        public T Item0
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX3<T>, T>(ref Unsafe.AsRef(in this)), 0);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX3<T>, T>(ref Unsafe.AsRef(in this)), 0) = value;
        }

        /// <summary>
        /// The item No.1.
        /// </summary>
        public T Item1
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX3<T>, T>(ref Unsafe.AsRef(in this)), 1);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX3<T>, T>(ref Unsafe.AsRef(in this)), 1) = value;
        }

        /// <summary>
        /// The item No.2.
        /// </summary>
        public T Item2
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX3<T>, T>(ref Unsafe.AsRef(in this)), 2);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX3<T>, T>(ref Unsafe.AsRef(in this)), 2) = value;
        }

        /// <summary>
        /// Initializes this <see cref="UnmanagedTupleX3{T}"/> from values.
        /// </summary>
        /// <param name="v0">The value No.0.</param>
        /// <param name="v1">The value No.1.</param>
        /// <param name="v2">The value No.2.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public UnmanagedTupleX3(T v0, T v1, T v2)
        {
            i0 = v0;
            i1 = v1;
            i2 = v2;
        }

        /// <summary>
        /// Deconstructs this <see cref="UnmanagedTupleX3{T}"/> to values.
        /// </summary>
        /// <param name="v0">The output value No.0.</param>
        /// <param name="v1">The output value No.1.</param>
        /// <param name="v2">The output value No.2.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public void Deconstruct(out T v0, out T v1, out T v2)
        {
            v0 = Item0;
            v1 = Item1;
            v2 = Item2;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="UnmanagedTupleX3{T}"/> to <see cref="ValueTuple{T, T}"/>.
        /// </summary>
        /// <param name="from">The converting <see cref="UnmanagedTupleX3{T}"/>.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static implicit operator (T, T, T)(UnmanagedTupleX3<T> from)
            => Unsafe.As<UnmanagedTupleX3<T>, (T, T, T)>(ref from);

        /// <summary>
        /// Performs an implicit conversion from <see cref="ValueTuple{T, T}"/> to <see cref="UnmanagedTupleX3{T}"/>.
        /// </summary>
        /// <param name="from">The converting <see cref="ValueTuple{T0, T1, T2}"/>.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static implicit operator UnmanagedTupleX3<T>((T, T, T) from)
            => Unsafe.As<(T, T, T), UnmanagedTupleX3<T>>(ref from);
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly partial struct UnmanagedTupleX4<T> where T:unmanaged
    {
        private readonly T i0, i1, i2, i3;
        /// <summary>
        /// The item No.0.
        /// </summary>
        public T Item0
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX4<T>, T>(ref Unsafe.AsRef(in this)), 0);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX4<T>, T>(ref Unsafe.AsRef(in this)), 0) = value;
        }

        /// <summary>
        /// The item No.1.
        /// </summary>
        public T Item1
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX4<T>, T>(ref Unsafe.AsRef(in this)), 1);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX4<T>, T>(ref Unsafe.AsRef(in this)), 1) = value;
        }

        /// <summary>
        /// The item No.2.
        /// </summary>
        public T Item2
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX4<T>, T>(ref Unsafe.AsRef(in this)), 2);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX4<T>, T>(ref Unsafe.AsRef(in this)), 2) = value;
        }

        /// <summary>
        /// The item No.3.
        /// </summary>
        public T Item3
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX4<T>, T>(ref Unsafe.AsRef(in this)), 3);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX4<T>, T>(ref Unsafe.AsRef(in this)), 3) = value;
        }

        /// <summary>
        /// Initializes this <see cref="UnmanagedTupleX4{T}"/> from values.
        /// </summary>
        /// <param name="v0">The value No.0.</param>
        /// <param name="v1">The value No.1.</param>
        /// <param name="v2">The value No.2.</param>
        /// <param name="v3">The value No.3.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public UnmanagedTupleX4(T v0, T v1, T v2, T v3)
        {
            i0 = v0;
            i1 = v1;
            i2 = v2;
            i3 = v3;
        }

        /// <summary>
        /// Deconstructs this <see cref="UnmanagedTupleX4{T}"/> to values.
        /// </summary>
        /// <param name="v0">The output value No.0.</param>
        /// <param name="v1">The output value No.1.</param>
        /// <param name="v2">The output value No.2.</param>
        /// <param name="v3">The output value No.3.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public void Deconstruct(out T v0, out T v1, out T v2, out T v3)
        {
            v0 = Item0;
            v1 = Item1;
            v2 = Item2;
            v3 = Item3;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="UnmanagedTupleX4{T}"/> to <see cref="ValueTuple{T, T}"/>.
        /// </summary>
        /// <param name="from">The converting <see cref="UnmanagedTupleX4{T}"/>.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static implicit operator (T, T, T, T)(UnmanagedTupleX4<T> from)
            => Unsafe.As<UnmanagedTupleX4<T>, (T, T, T, T)>(ref from);

        /// <summary>
        /// Performs an implicit conversion from <see cref="ValueTuple{T, T}"/> to <see cref="UnmanagedTupleX4{T}"/>.
        /// </summary>
        /// <param name="from">The converting <see cref="ValueTuple{T0, T1, T2, T3}"/>.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static implicit operator UnmanagedTupleX4<T>((T, T, T, T) from)
            => Unsafe.As<(T, T, T, T), UnmanagedTupleX4<T>>(ref from);
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly partial struct UnmanagedTupleX5<T> where T:unmanaged
    {
        private readonly T i0, i1, i2, i3, i4;
        /// <summary>
        /// The item No.0.
        /// </summary>
        public T Item0
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX5<T>, T>(ref Unsafe.AsRef(in this)), 0);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX5<T>, T>(ref Unsafe.AsRef(in this)), 0) = value;
        }

        /// <summary>
        /// The item No.1.
        /// </summary>
        public T Item1
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX5<T>, T>(ref Unsafe.AsRef(in this)), 1);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX5<T>, T>(ref Unsafe.AsRef(in this)), 1) = value;
        }

        /// <summary>
        /// The item No.2.
        /// </summary>
        public T Item2
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX5<T>, T>(ref Unsafe.AsRef(in this)), 2);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX5<T>, T>(ref Unsafe.AsRef(in this)), 2) = value;
        }

        /// <summary>
        /// The item No.3.
        /// </summary>
        public T Item3
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX5<T>, T>(ref Unsafe.AsRef(in this)), 3);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX5<T>, T>(ref Unsafe.AsRef(in this)), 3) = value;
        }

        /// <summary>
        /// The item No.4.
        /// </summary>
        public T Item4
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX5<T>, T>(ref Unsafe.AsRef(in this)), 4);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX5<T>, T>(ref Unsafe.AsRef(in this)), 4) = value;
        }

        /// <summary>
        /// Initializes this <see cref="UnmanagedTupleX5{T}"/> from values.
        /// </summary>
        /// <param name="v0">The value No.0.</param>
        /// <param name="v1">The value No.1.</param>
        /// <param name="v2">The value No.2.</param>
        /// <param name="v3">The value No.3.</param>
        /// <param name="v4">The value No.4.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public UnmanagedTupleX5(T v0, T v1, T v2, T v3, T v4)
        {
            i0 = v0;
            i1 = v1;
            i2 = v2;
            i3 = v3;
            i4 = v4;
        }

        /// <summary>
        /// Deconstructs this <see cref="UnmanagedTupleX5{T}"/> to values.
        /// </summary>
        /// <param name="v0">The output value No.0.</param>
        /// <param name="v1">The output value No.1.</param>
        /// <param name="v2">The output value No.2.</param>
        /// <param name="v3">The output value No.3.</param>
        /// <param name="v4">The output value No.4.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public void Deconstruct(out T v0, out T v1, out T v2, out T v3, out T v4)
        {
            v0 = Item0;
            v1 = Item1;
            v2 = Item2;
            v3 = Item3;
            v4 = Item4;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="UnmanagedTupleX5{T}"/> to <see cref="ValueTuple{T, T}"/>.
        /// </summary>
        /// <param name="from">The converting <see cref="UnmanagedTupleX5{T}"/>.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static implicit operator (T, T, T, T, T)(UnmanagedTupleX5<T> from)
            => Unsafe.As<UnmanagedTupleX5<T>, (T, T, T, T, T)>(ref from);

        /// <summary>
        /// Performs an implicit conversion from <see cref="ValueTuple{T, T}"/> to <see cref="UnmanagedTupleX5{T}"/>.
        /// </summary>
        /// <param name="from">The converting <see cref="ValueTuple{T0, T1, T2, T3, T4}"/>.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static implicit operator UnmanagedTupleX5<T>((T, T, T, T, T) from)
            => Unsafe.As<(T, T, T, T, T), UnmanagedTupleX5<T>>(ref from);
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly partial struct UnmanagedTupleX6<T> where T:unmanaged
    {
        private readonly T i0, i1, i2, i3, i4, i5;
        /// <summary>
        /// The item No.0.
        /// </summary>
        public T Item0
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX6<T>, T>(ref Unsafe.AsRef(in this)), 0);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX6<T>, T>(ref Unsafe.AsRef(in this)), 0) = value;
        }

        /// <summary>
        /// The item No.1.
        /// </summary>
        public T Item1
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX6<T>, T>(ref Unsafe.AsRef(in this)), 1);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX6<T>, T>(ref Unsafe.AsRef(in this)), 1) = value;
        }

        /// <summary>
        /// The item No.2.
        /// </summary>
        public T Item2
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX6<T>, T>(ref Unsafe.AsRef(in this)), 2);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX6<T>, T>(ref Unsafe.AsRef(in this)), 2) = value;
        }

        /// <summary>
        /// The item No.3.
        /// </summary>
        public T Item3
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX6<T>, T>(ref Unsafe.AsRef(in this)), 3);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX6<T>, T>(ref Unsafe.AsRef(in this)), 3) = value;
        }

        /// <summary>
        /// The item No.4.
        /// </summary>
        public T Item4
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX6<T>, T>(ref Unsafe.AsRef(in this)), 4);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX6<T>, T>(ref Unsafe.AsRef(in this)), 4) = value;
        }

        /// <summary>
        /// The item No.5.
        /// </summary>
        public T Item5
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX6<T>, T>(ref Unsafe.AsRef(in this)), 5);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX6<T>, T>(ref Unsafe.AsRef(in this)), 5) = value;
        }

        /// <summary>
        /// Initializes this <see cref="UnmanagedTupleX6{T}"/> from values.
        /// </summary>
        /// <param name="v0">The value No.0.</param>
        /// <param name="v1">The value No.1.</param>
        /// <param name="v2">The value No.2.</param>
        /// <param name="v3">The value No.3.</param>
        /// <param name="v4">The value No.4.</param>
        /// <param name="v5">The value No.5.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public UnmanagedTupleX6(T v0, T v1, T v2, T v3, T v4, T v5)
        {
            i0 = v0;
            i1 = v1;
            i2 = v2;
            i3 = v3;
            i4 = v4;
            i5 = v5;
        }

        /// <summary>
        /// Deconstructs this <see cref="UnmanagedTupleX6{T}"/> to values.
        /// </summary>
        /// <param name="v0">The output value No.0.</param>
        /// <param name="v1">The output value No.1.</param>
        /// <param name="v2">The output value No.2.</param>
        /// <param name="v3">The output value No.3.</param>
        /// <param name="v4">The output value No.4.</param>
        /// <param name="v5">The output value No.5.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public void Deconstruct(out T v0, out T v1, out T v2, out T v3, out T v4, out T v5)
        {
            v0 = Item0;
            v1 = Item1;
            v2 = Item2;
            v3 = Item3;
            v4 = Item4;
            v5 = Item5;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="UnmanagedTupleX6{T}"/> to <see cref="ValueTuple{T, T}"/>.
        /// </summary>
        /// <param name="from">The converting <see cref="UnmanagedTupleX6{T}"/>.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static implicit operator (T, T, T, T, T, T)(UnmanagedTupleX6<T> from)
            => Unsafe.As<UnmanagedTupleX6<T>, (T, T, T, T, T, T)>(ref from);

        /// <summary>
        /// Performs an implicit conversion from <see cref="ValueTuple{T, T}"/> to <see cref="UnmanagedTupleX6{T}"/>.
        /// </summary>
        /// <param name="from">The converting <see cref="ValueTuple{T0, T1, T2, T3, T4, T5}"/>.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static implicit operator UnmanagedTupleX6<T>((T, T, T, T, T, T) from)
            => Unsafe.As<(T, T, T, T, T, T), UnmanagedTupleX6<T>>(ref from);
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly partial struct UnmanagedTupleX7<T> where T:unmanaged
    {
        private readonly T i0, i1, i2, i3, i4, i5, i6;
        /// <summary>
        /// The item No.0.
        /// </summary>
        public T Item0
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX7<T>, T>(ref Unsafe.AsRef(in this)), 0);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX7<T>, T>(ref Unsafe.AsRef(in this)), 0) = value;
        }

        /// <summary>
        /// The item No.1.
        /// </summary>
        public T Item1
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX7<T>, T>(ref Unsafe.AsRef(in this)), 1);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX7<T>, T>(ref Unsafe.AsRef(in this)), 1) = value;
        }

        /// <summary>
        /// The item No.2.
        /// </summary>
        public T Item2
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX7<T>, T>(ref Unsafe.AsRef(in this)), 2);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX7<T>, T>(ref Unsafe.AsRef(in this)), 2) = value;
        }

        /// <summary>
        /// The item No.3.
        /// </summary>
        public T Item3
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX7<T>, T>(ref Unsafe.AsRef(in this)), 3);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX7<T>, T>(ref Unsafe.AsRef(in this)), 3) = value;
        }

        /// <summary>
        /// The item No.4.
        /// </summary>
        public T Item4
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX7<T>, T>(ref Unsafe.AsRef(in this)), 4);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX7<T>, T>(ref Unsafe.AsRef(in this)), 4) = value;
        }

        /// <summary>
        /// The item No.5.
        /// </summary>
        public T Item5
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX7<T>, T>(ref Unsafe.AsRef(in this)), 5);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX7<T>, T>(ref Unsafe.AsRef(in this)), 5) = value;
        }

        /// <summary>
        /// The item No.6.
        /// </summary>
        public T Item6
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX7<T>, T>(ref Unsafe.AsRef(in this)), 6);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX7<T>, T>(ref Unsafe.AsRef(in this)), 6) = value;
        }

        /// <summary>
        /// Initializes this <see cref="UnmanagedTupleX7{T}"/> from values.
        /// </summary>
        /// <param name="v0">The value No.0.</param>
        /// <param name="v1">The value No.1.</param>
        /// <param name="v2">The value No.2.</param>
        /// <param name="v3">The value No.3.</param>
        /// <param name="v4">The value No.4.</param>
        /// <param name="v5">The value No.5.</param>
        /// <param name="v6">The value No.6.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public UnmanagedTupleX7(T v0, T v1, T v2, T v3, T v4, T v5, T v6)
        {
            i0 = v0;
            i1 = v1;
            i2 = v2;
            i3 = v3;
            i4 = v4;
            i5 = v5;
            i6 = v6;
        }

        /// <summary>
        /// Deconstructs this <see cref="UnmanagedTupleX7{T}"/> to values.
        /// </summary>
        /// <param name="v0">The output value No.0.</param>
        /// <param name="v1">The output value No.1.</param>
        /// <param name="v2">The output value No.2.</param>
        /// <param name="v3">The output value No.3.</param>
        /// <param name="v4">The output value No.4.</param>
        /// <param name="v5">The output value No.5.</param>
        /// <param name="v6">The output value No.6.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public void Deconstruct(out T v0, out T v1, out T v2, out T v3, out T v4, out T v5, out T v6)
        {
            v0 = Item0;
            v1 = Item1;
            v2 = Item2;
            v3 = Item3;
            v4 = Item4;
            v5 = Item5;
            v6 = Item6;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="UnmanagedTupleX7{T}"/> to <see cref="ValueTuple{T, T}"/>.
        /// </summary>
        /// <param name="from">The converting <see cref="UnmanagedTupleX7{T}"/>.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static implicit operator (T, T, T, T, T, T, T)(UnmanagedTupleX7<T> from)
            => Unsafe.As<UnmanagedTupleX7<T>, (T, T, T, T, T, T, T)>(ref from);

        /// <summary>
        /// Performs an implicit conversion from <see cref="ValueTuple{T, T}"/> to <see cref="UnmanagedTupleX7{T}"/>.
        /// </summary>
        /// <param name="from">The converting <see cref="ValueTuple{T0, T1, T2, T3, T4, T5, T6}"/>.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static implicit operator UnmanagedTupleX7<T>((T, T, T, T, T, T, T) from)
            => Unsafe.As<(T, T, T, T, T, T, T), UnmanagedTupleX7<T>>(ref from);
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly partial struct UnmanagedTupleX8<T> where T:unmanaged
    {
        private readonly T i0, i1, i2, i3, i4, i5, i6, i7;
        /// <summary>
        /// The item No.0.
        /// </summary>
        public T Item0
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX8<T>, T>(ref Unsafe.AsRef(in this)), 0);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX8<T>, T>(ref Unsafe.AsRef(in this)), 0) = value;
        }

        /// <summary>
        /// The item No.1.
        /// </summary>
        public T Item1
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX8<T>, T>(ref Unsafe.AsRef(in this)), 1);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX8<T>, T>(ref Unsafe.AsRef(in this)), 1) = value;
        }

        /// <summary>
        /// The item No.2.
        /// </summary>
        public T Item2
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX8<T>, T>(ref Unsafe.AsRef(in this)), 2);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX8<T>, T>(ref Unsafe.AsRef(in this)), 2) = value;
        }

        /// <summary>
        /// The item No.3.
        /// </summary>
        public T Item3
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX8<T>, T>(ref Unsafe.AsRef(in this)), 3);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX8<T>, T>(ref Unsafe.AsRef(in this)), 3) = value;
        }

        /// <summary>
        /// The item No.4.
        /// </summary>
        public T Item4
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX8<T>, T>(ref Unsafe.AsRef(in this)), 4);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX8<T>, T>(ref Unsafe.AsRef(in this)), 4) = value;
        }

        /// <summary>
        /// The item No.5.
        /// </summary>
        public T Item5
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX8<T>, T>(ref Unsafe.AsRef(in this)), 5);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX8<T>, T>(ref Unsafe.AsRef(in this)), 5) = value;
        }

        /// <summary>
        /// The item No.6.
        /// </summary>
        public T Item6
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX8<T>, T>(ref Unsafe.AsRef(in this)), 6);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX8<T>, T>(ref Unsafe.AsRef(in this)), 6) = value;
        }

        /// <summary>
        /// The item No.7.
        /// </summary>
        public T Item7
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX8<T>, T>(ref Unsafe.AsRef(in this)), 7);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX8<T>, T>(ref Unsafe.AsRef(in this)), 7) = value;
        }

        /// <summary>
        /// Initializes this <see cref="UnmanagedTupleX8{T}"/> from values.
        /// </summary>
        /// <param name="v0">The value No.0.</param>
        /// <param name="v1">The value No.1.</param>
        /// <param name="v2">The value No.2.</param>
        /// <param name="v3">The value No.3.</param>
        /// <param name="v4">The value No.4.</param>
        /// <param name="v5">The value No.5.</param>
        /// <param name="v6">The value No.6.</param>
        /// <param name="v7">The value No.7.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public UnmanagedTupleX8(T v0, T v1, T v2, T v3, T v4, T v5, T v6, T v7)
        {
            i0 = v0;
            i1 = v1;
            i2 = v2;
            i3 = v3;
            i4 = v4;
            i5 = v5;
            i6 = v6;
            i7 = v7;
        }

        /// <summary>
        /// Deconstructs this <see cref="UnmanagedTupleX8{T}"/> to values.
        /// </summary>
        /// <param name="v0">The output value No.0.</param>
        /// <param name="v1">The output value No.1.</param>
        /// <param name="v2">The output value No.2.</param>
        /// <param name="v3">The output value No.3.</param>
        /// <param name="v4">The output value No.4.</param>
        /// <param name="v5">The output value No.5.</param>
        /// <param name="v6">The output value No.6.</param>
        /// <param name="v7">The output value No.7.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public void Deconstruct(out T v0, out T v1, out T v2, out T v3, out T v4, out T v5, out T v6, out T v7)
        {
            v0 = Item0;
            v1 = Item1;
            v2 = Item2;
            v3 = Item3;
            v4 = Item4;
            v5 = Item5;
            v6 = Item6;
            v7 = Item7;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="UnmanagedTupleX8{T}"/> to <see cref="ValueTuple{T, T}"/>.
        /// </summary>
        /// <param name="from">The converting <see cref="UnmanagedTupleX8{T}"/>.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static implicit operator (T, T, T, T, T, T, T, T)(UnmanagedTupleX8<T> from)
            => Unsafe.As<UnmanagedTupleX8<T>, (T, T, T, T, T, T, T, T)>(ref from);

        /// <summary>
        /// Performs an implicit conversion from <see cref="ValueTuple{T, T}"/> to <see cref="UnmanagedTupleX8{T}"/>.
        /// </summary>
        /// <param name="from">The converting <see cref="ValueTuple{T0, T1, T2, T3, T4, T5, T6, T7}"/>.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static implicit operator UnmanagedTupleX8<T>((T, T, T, T, T, T, T, T) from)
            => Unsafe.As<(T, T, T, T, T, T, T, T), UnmanagedTupleX8<T>>(ref from);
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly partial struct UnmanagedTupleX9<T> where T:unmanaged
    {
        private readonly T i0, i1, i2, i3, i4, i5, i6, i7, i8;
        /// <summary>
        /// The item No.0.
        /// </summary>
        public T Item0
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX9<T>, T>(ref Unsafe.AsRef(in this)), 0);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX9<T>, T>(ref Unsafe.AsRef(in this)), 0) = value;
        }

        /// <summary>
        /// The item No.1.
        /// </summary>
        public T Item1
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX9<T>, T>(ref Unsafe.AsRef(in this)), 1);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX9<T>, T>(ref Unsafe.AsRef(in this)), 1) = value;
        }

        /// <summary>
        /// The item No.2.
        /// </summary>
        public T Item2
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX9<T>, T>(ref Unsafe.AsRef(in this)), 2);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX9<T>, T>(ref Unsafe.AsRef(in this)), 2) = value;
        }

        /// <summary>
        /// The item No.3.
        /// </summary>
        public T Item3
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX9<T>, T>(ref Unsafe.AsRef(in this)), 3);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX9<T>, T>(ref Unsafe.AsRef(in this)), 3) = value;
        }

        /// <summary>
        /// The item No.4.
        /// </summary>
        public T Item4
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX9<T>, T>(ref Unsafe.AsRef(in this)), 4);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX9<T>, T>(ref Unsafe.AsRef(in this)), 4) = value;
        }

        /// <summary>
        /// The item No.5.
        /// </summary>
        public T Item5
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX9<T>, T>(ref Unsafe.AsRef(in this)), 5);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX9<T>, T>(ref Unsafe.AsRef(in this)), 5) = value;
        }

        /// <summary>
        /// The item No.6.
        /// </summary>
        public T Item6
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX9<T>, T>(ref Unsafe.AsRef(in this)), 6);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX9<T>, T>(ref Unsafe.AsRef(in this)), 6) = value;
        }

        /// <summary>
        /// The item No.7.
        /// </summary>
        public T Item7
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX9<T>, T>(ref Unsafe.AsRef(in this)), 7);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX9<T>, T>(ref Unsafe.AsRef(in this)), 7) = value;
        }

        /// <summary>
        /// The item No.8.
        /// </summary>
        public T Item8
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX9<T>, T>(ref Unsafe.AsRef(in this)), 8);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX9<T>, T>(ref Unsafe.AsRef(in this)), 8) = value;
        }

        /// <summary>
        /// Initializes this <see cref="UnmanagedTupleX9{T}"/> from values.
        /// </summary>
        /// <param name="v0">The value No.0.</param>
        /// <param name="v1">The value No.1.</param>
        /// <param name="v2">The value No.2.</param>
        /// <param name="v3">The value No.3.</param>
        /// <param name="v4">The value No.4.</param>
        /// <param name="v5">The value No.5.</param>
        /// <param name="v6">The value No.6.</param>
        /// <param name="v7">The value No.7.</param>
        /// <param name="v8">The value No.8.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public UnmanagedTupleX9(T v0, T v1, T v2, T v3, T v4, T v5, T v6, T v7, T v8)
        {
            i0 = v0;
            i1 = v1;
            i2 = v2;
            i3 = v3;
            i4 = v4;
            i5 = v5;
            i6 = v6;
            i7 = v7;
            i8 = v8;
        }

        /// <summary>
        /// Deconstructs this <see cref="UnmanagedTupleX9{T}"/> to values.
        /// </summary>
        /// <param name="v0">The output value No.0.</param>
        /// <param name="v1">The output value No.1.</param>
        /// <param name="v2">The output value No.2.</param>
        /// <param name="v3">The output value No.3.</param>
        /// <param name="v4">The output value No.4.</param>
        /// <param name="v5">The output value No.5.</param>
        /// <param name="v6">The output value No.6.</param>
        /// <param name="v7">The output value No.7.</param>
        /// <param name="v8">The output value No.8.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public void Deconstruct(out T v0, out T v1, out T v2, out T v3, out T v4, out T v5, out T v6, out T v7, out T v8)
        {
            v0 = Item0;
            v1 = Item1;
            v2 = Item2;
            v3 = Item3;
            v4 = Item4;
            v5 = Item5;
            v6 = Item6;
            v7 = Item7;
            v8 = Item8;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="UnmanagedTupleX9{T}"/> to <see cref="ValueTuple{T, T}"/>.
        /// </summary>
        /// <param name="from">The converting <see cref="UnmanagedTupleX9{T}"/>.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static implicit operator (T, T, T, T, T, T, T, T, T)(UnmanagedTupleX9<T> from)
            => Unsafe.As<UnmanagedTupleX9<T>, (T, T, T, T, T, T, T, T, T)>(ref from);

        /// <summary>
        /// Performs an implicit conversion from <see cref="ValueTuple{T, T}"/> to <see cref="UnmanagedTupleX9{T}"/>.
        /// </summary>
        /// <param name="from">The converting <see cref="ValueTuple{T0, T1, T2, T3, T4, T5, T6, T7, T8}"/>.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static implicit operator UnmanagedTupleX9<T>((T, T, T, T, T, T, T, T, T) from)
            => Unsafe.As<(T, T, T, T, T, T, T, T, T), UnmanagedTupleX9<T>>(ref from);
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly partial struct UnmanagedTupleX10<T> where T:unmanaged
    {
        private readonly T i0, i1, i2, i3, i4, i5, i6, i7, i8, i9;
        /// <summary>
        /// The item No.0.
        /// </summary>
        public T Item0
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX10<T>, T>(ref Unsafe.AsRef(in this)), 0);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX10<T>, T>(ref Unsafe.AsRef(in this)), 0) = value;
        }

        /// <summary>
        /// The item No.1.
        /// </summary>
        public T Item1
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX10<T>, T>(ref Unsafe.AsRef(in this)), 1);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX10<T>, T>(ref Unsafe.AsRef(in this)), 1) = value;
        }

        /// <summary>
        /// The item No.2.
        /// </summary>
        public T Item2
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX10<T>, T>(ref Unsafe.AsRef(in this)), 2);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX10<T>, T>(ref Unsafe.AsRef(in this)), 2) = value;
        }

        /// <summary>
        /// The item No.3.
        /// </summary>
        public T Item3
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX10<T>, T>(ref Unsafe.AsRef(in this)), 3);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX10<T>, T>(ref Unsafe.AsRef(in this)), 3) = value;
        }

        /// <summary>
        /// The item No.4.
        /// </summary>
        public T Item4
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX10<T>, T>(ref Unsafe.AsRef(in this)), 4);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX10<T>, T>(ref Unsafe.AsRef(in this)), 4) = value;
        }

        /// <summary>
        /// The item No.5.
        /// </summary>
        public T Item5
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX10<T>, T>(ref Unsafe.AsRef(in this)), 5);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX10<T>, T>(ref Unsafe.AsRef(in this)), 5) = value;
        }

        /// <summary>
        /// The item No.6.
        /// </summary>
        public T Item6
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX10<T>, T>(ref Unsafe.AsRef(in this)), 6);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX10<T>, T>(ref Unsafe.AsRef(in this)), 6) = value;
        }

        /// <summary>
        /// The item No.7.
        /// </summary>
        public T Item7
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX10<T>, T>(ref Unsafe.AsRef(in this)), 7);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX10<T>, T>(ref Unsafe.AsRef(in this)), 7) = value;
        }

        /// <summary>
        /// The item No.8.
        /// </summary>
        public T Item8
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX10<T>, T>(ref Unsafe.AsRef(in this)), 8);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX10<T>, T>(ref Unsafe.AsRef(in this)), 8) = value;
        }

        /// <summary>
        /// The item No.9.
        /// </summary>
        public T Item9
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX10<T>, T>(ref Unsafe.AsRef(in this)), 9);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX10<T>, T>(ref Unsafe.AsRef(in this)), 9) = value;
        }

        /// <summary>
        /// Initializes this <see cref="UnmanagedTupleX10{T}"/> from values.
        /// </summary>
        /// <param name="v0">The value No.0.</param>
        /// <param name="v1">The value No.1.</param>
        /// <param name="v2">The value No.2.</param>
        /// <param name="v3">The value No.3.</param>
        /// <param name="v4">The value No.4.</param>
        /// <param name="v5">The value No.5.</param>
        /// <param name="v6">The value No.6.</param>
        /// <param name="v7">The value No.7.</param>
        /// <param name="v8">The value No.8.</param>
        /// <param name="v9">The value No.9.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public UnmanagedTupleX10(T v0, T v1, T v2, T v3, T v4, T v5, T v6, T v7, T v8, T v9)
        {
            i0 = v0;
            i1 = v1;
            i2 = v2;
            i3 = v3;
            i4 = v4;
            i5 = v5;
            i6 = v6;
            i7 = v7;
            i8 = v8;
            i9 = v9;
        }

        /// <summary>
        /// Deconstructs this <see cref="UnmanagedTupleX10{T}"/> to values.
        /// </summary>
        /// <param name="v0">The output value No.0.</param>
        /// <param name="v1">The output value No.1.</param>
        /// <param name="v2">The output value No.2.</param>
        /// <param name="v3">The output value No.3.</param>
        /// <param name="v4">The output value No.4.</param>
        /// <param name="v5">The output value No.5.</param>
        /// <param name="v6">The output value No.6.</param>
        /// <param name="v7">The output value No.7.</param>
        /// <param name="v8">The output value No.8.</param>
        /// <param name="v9">The output value No.9.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public void Deconstruct(out T v0, out T v1, out T v2, out T v3, out T v4, out T v5, out T v6, out T v7, out T v8, out T v9)
        {
            v0 = Item0;
            v1 = Item1;
            v2 = Item2;
            v3 = Item3;
            v4 = Item4;
            v5 = Item5;
            v6 = Item6;
            v7 = Item7;
            v8 = Item8;
            v9 = Item9;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="UnmanagedTupleX10{T}"/> to <see cref="ValueTuple{T, T}"/>.
        /// </summary>
        /// <param name="from">The converting <see cref="UnmanagedTupleX10{T}"/>.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static implicit operator (T, T, T, T, T, T, T, T, T, T)(UnmanagedTupleX10<T> from)
            => Unsafe.As<UnmanagedTupleX10<T>, (T, T, T, T, T, T, T, T, T, T)>(ref from);

        /// <summary>
        /// Performs an implicit conversion from <see cref="ValueTuple{T, T}"/> to <see cref="UnmanagedTupleX10{T}"/>.
        /// </summary>
        /// <param name="from">The converting <see cref="ValueTuple{T0, T1, T2, T3, T4, T5, T6, T7, T8, T9}"/>.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static implicit operator UnmanagedTupleX10<T>((T, T, T, T, T, T, T, T, T, T) from)
            => Unsafe.As<(T, T, T, T, T, T, T, T, T, T), UnmanagedTupleX10<T>>(ref from);
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly partial struct UnmanagedTupleX11<T> where T:unmanaged
    {
        private readonly T i0, i1, i2, i3, i4, i5, i6, i7, i8, i9, i10;
        /// <summary>
        /// The item No.0.
        /// </summary>
        public T Item0
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX11<T>, T>(ref Unsafe.AsRef(in this)), 0);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX11<T>, T>(ref Unsafe.AsRef(in this)), 0) = value;
        }

        /// <summary>
        /// The item No.1.
        /// </summary>
        public T Item1
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX11<T>, T>(ref Unsafe.AsRef(in this)), 1);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX11<T>, T>(ref Unsafe.AsRef(in this)), 1) = value;
        }

        /// <summary>
        /// The item No.2.
        /// </summary>
        public T Item2
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX11<T>, T>(ref Unsafe.AsRef(in this)), 2);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX11<T>, T>(ref Unsafe.AsRef(in this)), 2) = value;
        }

        /// <summary>
        /// The item No.3.
        /// </summary>
        public T Item3
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX11<T>, T>(ref Unsafe.AsRef(in this)), 3);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX11<T>, T>(ref Unsafe.AsRef(in this)), 3) = value;
        }

        /// <summary>
        /// The item No.4.
        /// </summary>
        public T Item4
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX11<T>, T>(ref Unsafe.AsRef(in this)), 4);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX11<T>, T>(ref Unsafe.AsRef(in this)), 4) = value;
        }

        /// <summary>
        /// The item No.5.
        /// </summary>
        public T Item5
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX11<T>, T>(ref Unsafe.AsRef(in this)), 5);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX11<T>, T>(ref Unsafe.AsRef(in this)), 5) = value;
        }

        /// <summary>
        /// The item No.6.
        /// </summary>
        public T Item6
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX11<T>, T>(ref Unsafe.AsRef(in this)), 6);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX11<T>, T>(ref Unsafe.AsRef(in this)), 6) = value;
        }

        /// <summary>
        /// The item No.7.
        /// </summary>
        public T Item7
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX11<T>, T>(ref Unsafe.AsRef(in this)), 7);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX11<T>, T>(ref Unsafe.AsRef(in this)), 7) = value;
        }

        /// <summary>
        /// The item No.8.
        /// </summary>
        public T Item8
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX11<T>, T>(ref Unsafe.AsRef(in this)), 8);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX11<T>, T>(ref Unsafe.AsRef(in this)), 8) = value;
        }

        /// <summary>
        /// The item No.9.
        /// </summary>
        public T Item9
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX11<T>, T>(ref Unsafe.AsRef(in this)), 9);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX11<T>, T>(ref Unsafe.AsRef(in this)), 9) = value;
        }

        /// <summary>
        /// The item No.10.
        /// </summary>
        public T Item10
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX11<T>, T>(ref Unsafe.AsRef(in this)), 10);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX11<T>, T>(ref Unsafe.AsRef(in this)), 10) = value;
        }

        /// <summary>
        /// Initializes this <see cref="UnmanagedTupleX11{T}"/> from values.
        /// </summary>
        /// <param name="v0">The value No.0.</param>
        /// <param name="v1">The value No.1.</param>
        /// <param name="v2">The value No.2.</param>
        /// <param name="v3">The value No.3.</param>
        /// <param name="v4">The value No.4.</param>
        /// <param name="v5">The value No.5.</param>
        /// <param name="v6">The value No.6.</param>
        /// <param name="v7">The value No.7.</param>
        /// <param name="v8">The value No.8.</param>
        /// <param name="v9">The value No.9.</param>
        /// <param name="v10">The value No.10.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public UnmanagedTupleX11(T v0, T v1, T v2, T v3, T v4, T v5, T v6, T v7, T v8, T v9, T v10)
        {
            i0 = v0;
            i1 = v1;
            i2 = v2;
            i3 = v3;
            i4 = v4;
            i5 = v5;
            i6 = v6;
            i7 = v7;
            i8 = v8;
            i9 = v9;
            i10 = v10;
        }

        /// <summary>
        /// Deconstructs this <see cref="UnmanagedTupleX11{T}"/> to values.
        /// </summary>
        /// <param name="v0">The output value No.0.</param>
        /// <param name="v1">The output value No.1.</param>
        /// <param name="v2">The output value No.2.</param>
        /// <param name="v3">The output value No.3.</param>
        /// <param name="v4">The output value No.4.</param>
        /// <param name="v5">The output value No.5.</param>
        /// <param name="v6">The output value No.6.</param>
        /// <param name="v7">The output value No.7.</param>
        /// <param name="v8">The output value No.8.</param>
        /// <param name="v9">The output value No.9.</param>
        /// <param name="v10">The output value No.10.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public void Deconstruct(out T v0, out T v1, out T v2, out T v3, out T v4, out T v5, out T v6, out T v7, out T v8, out T v9, out T v10)
        {
            v0 = Item0;
            v1 = Item1;
            v2 = Item2;
            v3 = Item3;
            v4 = Item4;
            v5 = Item5;
            v6 = Item6;
            v7 = Item7;
            v8 = Item8;
            v9 = Item9;
            v10 = Item10;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="UnmanagedTupleX11{T}"/> to <see cref="ValueTuple{T, T}"/>.
        /// </summary>
        /// <param name="from">The converting <see cref="UnmanagedTupleX11{T}"/>.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static implicit operator (T, T, T, T, T, T, T, T, T, T, T)(UnmanagedTupleX11<T> from)
            => Unsafe.As<UnmanagedTupleX11<T>, (T, T, T, T, T, T, T, T, T, T, T)>(ref from);

        /// <summary>
        /// Performs an implicit conversion from <see cref="ValueTuple{T, T}"/> to <see cref="UnmanagedTupleX11{T}"/>.
        /// </summary>
        /// <param name="from">The converting <see cref="ValueTuple{T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10}"/>.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static implicit operator UnmanagedTupleX11<T>((T, T, T, T, T, T, T, T, T, T, T) from)
            => Unsafe.As<(T, T, T, T, T, T, T, T, T, T, T), UnmanagedTupleX11<T>>(ref from);
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly partial struct UnmanagedTupleX12<T> where T:unmanaged
    {
        private readonly T i0, i1, i2, i3, i4, i5, i6, i7, i8, i9, i10, i11;
        /// <summary>
        /// The item No.0.
        /// </summary>
        public T Item0
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX12<T>, T>(ref Unsafe.AsRef(in this)), 0);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX12<T>, T>(ref Unsafe.AsRef(in this)), 0) = value;
        }

        /// <summary>
        /// The item No.1.
        /// </summary>
        public T Item1
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX12<T>, T>(ref Unsafe.AsRef(in this)), 1);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX12<T>, T>(ref Unsafe.AsRef(in this)), 1) = value;
        }

        /// <summary>
        /// The item No.2.
        /// </summary>
        public T Item2
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX12<T>, T>(ref Unsafe.AsRef(in this)), 2);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX12<T>, T>(ref Unsafe.AsRef(in this)), 2) = value;
        }

        /// <summary>
        /// The item No.3.
        /// </summary>
        public T Item3
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX12<T>, T>(ref Unsafe.AsRef(in this)), 3);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX12<T>, T>(ref Unsafe.AsRef(in this)), 3) = value;
        }

        /// <summary>
        /// The item No.4.
        /// </summary>
        public T Item4
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX12<T>, T>(ref Unsafe.AsRef(in this)), 4);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX12<T>, T>(ref Unsafe.AsRef(in this)), 4) = value;
        }

        /// <summary>
        /// The item No.5.
        /// </summary>
        public T Item5
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX12<T>, T>(ref Unsafe.AsRef(in this)), 5);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX12<T>, T>(ref Unsafe.AsRef(in this)), 5) = value;
        }

        /// <summary>
        /// The item No.6.
        /// </summary>
        public T Item6
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX12<T>, T>(ref Unsafe.AsRef(in this)), 6);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX12<T>, T>(ref Unsafe.AsRef(in this)), 6) = value;
        }

        /// <summary>
        /// The item No.7.
        /// </summary>
        public T Item7
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX12<T>, T>(ref Unsafe.AsRef(in this)), 7);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX12<T>, T>(ref Unsafe.AsRef(in this)), 7) = value;
        }

        /// <summary>
        /// The item No.8.
        /// </summary>
        public T Item8
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX12<T>, T>(ref Unsafe.AsRef(in this)), 8);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX12<T>, T>(ref Unsafe.AsRef(in this)), 8) = value;
        }

        /// <summary>
        /// The item No.9.
        /// </summary>
        public T Item9
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX12<T>, T>(ref Unsafe.AsRef(in this)), 9);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX12<T>, T>(ref Unsafe.AsRef(in this)), 9) = value;
        }

        /// <summary>
        /// The item No.10.
        /// </summary>
        public T Item10
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX12<T>, T>(ref Unsafe.AsRef(in this)), 10);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX12<T>, T>(ref Unsafe.AsRef(in this)), 10) = value;
        }

        /// <summary>
        /// The item No.11.
        /// </summary>
        public T Item11
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX12<T>, T>(ref Unsafe.AsRef(in this)), 11);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX12<T>, T>(ref Unsafe.AsRef(in this)), 11) = value;
        }

        /// <summary>
        /// Initializes this <see cref="UnmanagedTupleX12{T}"/> from values.
        /// </summary>
        /// <param name="v0">The value No.0.</param>
        /// <param name="v1">The value No.1.</param>
        /// <param name="v2">The value No.2.</param>
        /// <param name="v3">The value No.3.</param>
        /// <param name="v4">The value No.4.</param>
        /// <param name="v5">The value No.5.</param>
        /// <param name="v6">The value No.6.</param>
        /// <param name="v7">The value No.7.</param>
        /// <param name="v8">The value No.8.</param>
        /// <param name="v9">The value No.9.</param>
        /// <param name="v10">The value No.10.</param>
        /// <param name="v11">The value No.11.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public UnmanagedTupleX12(T v0, T v1, T v2, T v3, T v4, T v5, T v6, T v7, T v8, T v9, T v10, T v11)
        {
            i0 = v0;
            i1 = v1;
            i2 = v2;
            i3 = v3;
            i4 = v4;
            i5 = v5;
            i6 = v6;
            i7 = v7;
            i8 = v8;
            i9 = v9;
            i10 = v10;
            i11 = v11;
        }

        /// <summary>
        /// Deconstructs this <see cref="UnmanagedTupleX12{T}"/> to values.
        /// </summary>
        /// <param name="v0">The output value No.0.</param>
        /// <param name="v1">The output value No.1.</param>
        /// <param name="v2">The output value No.2.</param>
        /// <param name="v3">The output value No.3.</param>
        /// <param name="v4">The output value No.4.</param>
        /// <param name="v5">The output value No.5.</param>
        /// <param name="v6">The output value No.6.</param>
        /// <param name="v7">The output value No.7.</param>
        /// <param name="v8">The output value No.8.</param>
        /// <param name="v9">The output value No.9.</param>
        /// <param name="v10">The output value No.10.</param>
        /// <param name="v11">The output value No.11.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public void Deconstruct(out T v0, out T v1, out T v2, out T v3, out T v4, out T v5, out T v6, out T v7, out T v8, out T v9, out T v10, out T v11)
        {
            v0 = Item0;
            v1 = Item1;
            v2 = Item2;
            v3 = Item3;
            v4 = Item4;
            v5 = Item5;
            v6 = Item6;
            v7 = Item7;
            v8 = Item8;
            v9 = Item9;
            v10 = Item10;
            v11 = Item11;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="UnmanagedTupleX12{T}"/> to <see cref="ValueTuple{T, T}"/>.
        /// </summary>
        /// <param name="from">The converting <see cref="UnmanagedTupleX12{T}"/>.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static implicit operator (T, T, T, T, T, T, T, T, T, T, T, T)(UnmanagedTupleX12<T> from)
            => Unsafe.As<UnmanagedTupleX12<T>, (T, T, T, T, T, T, T, T, T, T, T, T)>(ref from);

        /// <summary>
        /// Performs an implicit conversion from <see cref="ValueTuple{T, T}"/> to <see cref="UnmanagedTupleX12{T}"/>.
        /// </summary>
        /// <param name="from">The converting <see cref="ValueTuple{T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11}"/>.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static implicit operator UnmanagedTupleX12<T>((T, T, T, T, T, T, T, T, T, T, T, T) from)
            => Unsafe.As<(T, T, T, T, T, T, T, T, T, T, T, T), UnmanagedTupleX12<T>>(ref from);
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly partial struct UnmanagedTupleX13<T> where T:unmanaged
    {
        private readonly T i0, i1, i2, i3, i4, i5, i6, i7, i8, i9, i10, i11, i12;
        /// <summary>
        /// The item No.0.
        /// </summary>
        public T Item0
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX13<T>, T>(ref Unsafe.AsRef(in this)), 0);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX13<T>, T>(ref Unsafe.AsRef(in this)), 0) = value;
        }

        /// <summary>
        /// The item No.1.
        /// </summary>
        public T Item1
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX13<T>, T>(ref Unsafe.AsRef(in this)), 1);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX13<T>, T>(ref Unsafe.AsRef(in this)), 1) = value;
        }

        /// <summary>
        /// The item No.2.
        /// </summary>
        public T Item2
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX13<T>, T>(ref Unsafe.AsRef(in this)), 2);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX13<T>, T>(ref Unsafe.AsRef(in this)), 2) = value;
        }

        /// <summary>
        /// The item No.3.
        /// </summary>
        public T Item3
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX13<T>, T>(ref Unsafe.AsRef(in this)), 3);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX13<T>, T>(ref Unsafe.AsRef(in this)), 3) = value;
        }

        /// <summary>
        /// The item No.4.
        /// </summary>
        public T Item4
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX13<T>, T>(ref Unsafe.AsRef(in this)), 4);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX13<T>, T>(ref Unsafe.AsRef(in this)), 4) = value;
        }

        /// <summary>
        /// The item No.5.
        /// </summary>
        public T Item5
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX13<T>, T>(ref Unsafe.AsRef(in this)), 5);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX13<T>, T>(ref Unsafe.AsRef(in this)), 5) = value;
        }

        /// <summary>
        /// The item No.6.
        /// </summary>
        public T Item6
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX13<T>, T>(ref Unsafe.AsRef(in this)), 6);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX13<T>, T>(ref Unsafe.AsRef(in this)), 6) = value;
        }

        /// <summary>
        /// The item No.7.
        /// </summary>
        public T Item7
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX13<T>, T>(ref Unsafe.AsRef(in this)), 7);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX13<T>, T>(ref Unsafe.AsRef(in this)), 7) = value;
        }

        /// <summary>
        /// The item No.8.
        /// </summary>
        public T Item8
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX13<T>, T>(ref Unsafe.AsRef(in this)), 8);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX13<T>, T>(ref Unsafe.AsRef(in this)), 8) = value;
        }

        /// <summary>
        /// The item No.9.
        /// </summary>
        public T Item9
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX13<T>, T>(ref Unsafe.AsRef(in this)), 9);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX13<T>, T>(ref Unsafe.AsRef(in this)), 9) = value;
        }

        /// <summary>
        /// The item No.10.
        /// </summary>
        public T Item10
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX13<T>, T>(ref Unsafe.AsRef(in this)), 10);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX13<T>, T>(ref Unsafe.AsRef(in this)), 10) = value;
        }

        /// <summary>
        /// The item No.11.
        /// </summary>
        public T Item11
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX13<T>, T>(ref Unsafe.AsRef(in this)), 11);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX13<T>, T>(ref Unsafe.AsRef(in this)), 11) = value;
        }

        /// <summary>
        /// The item No.12.
        /// </summary>
        public T Item12
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX13<T>, T>(ref Unsafe.AsRef(in this)), 12);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX13<T>, T>(ref Unsafe.AsRef(in this)), 12) = value;
        }

        /// <summary>
        /// Initializes this <see cref="UnmanagedTupleX13{T}"/> from values.
        /// </summary>
        /// <param name="v0">The value No.0.</param>
        /// <param name="v1">The value No.1.</param>
        /// <param name="v2">The value No.2.</param>
        /// <param name="v3">The value No.3.</param>
        /// <param name="v4">The value No.4.</param>
        /// <param name="v5">The value No.5.</param>
        /// <param name="v6">The value No.6.</param>
        /// <param name="v7">The value No.7.</param>
        /// <param name="v8">The value No.8.</param>
        /// <param name="v9">The value No.9.</param>
        /// <param name="v10">The value No.10.</param>
        /// <param name="v11">The value No.11.</param>
        /// <param name="v12">The value No.12.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public UnmanagedTupleX13(T v0, T v1, T v2, T v3, T v4, T v5, T v6, T v7, T v8, T v9, T v10, T v11, T v12)
        {
            i0 = v0;
            i1 = v1;
            i2 = v2;
            i3 = v3;
            i4 = v4;
            i5 = v5;
            i6 = v6;
            i7 = v7;
            i8 = v8;
            i9 = v9;
            i10 = v10;
            i11 = v11;
            i12 = v12;
        }

        /// <summary>
        /// Deconstructs this <see cref="UnmanagedTupleX13{T}"/> to values.
        /// </summary>
        /// <param name="v0">The output value No.0.</param>
        /// <param name="v1">The output value No.1.</param>
        /// <param name="v2">The output value No.2.</param>
        /// <param name="v3">The output value No.3.</param>
        /// <param name="v4">The output value No.4.</param>
        /// <param name="v5">The output value No.5.</param>
        /// <param name="v6">The output value No.6.</param>
        /// <param name="v7">The output value No.7.</param>
        /// <param name="v8">The output value No.8.</param>
        /// <param name="v9">The output value No.9.</param>
        /// <param name="v10">The output value No.10.</param>
        /// <param name="v11">The output value No.11.</param>
        /// <param name="v12">The output value No.12.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public void Deconstruct(out T v0, out T v1, out T v2, out T v3, out T v4, out T v5, out T v6, out T v7, out T v8, out T v9, out T v10, out T v11, out T v12)
        {
            v0 = Item0;
            v1 = Item1;
            v2 = Item2;
            v3 = Item3;
            v4 = Item4;
            v5 = Item5;
            v6 = Item6;
            v7 = Item7;
            v8 = Item8;
            v9 = Item9;
            v10 = Item10;
            v11 = Item11;
            v12 = Item12;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="UnmanagedTupleX13{T}"/> to <see cref="ValueTuple{T, T}"/>.
        /// </summary>
        /// <param name="from">The converting <see cref="UnmanagedTupleX13{T}"/>.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static implicit operator (T, T, T, T, T, T, T, T, T, T, T, T, T)(UnmanagedTupleX13<T> from)
            => Unsafe.As<UnmanagedTupleX13<T>, (T, T, T, T, T, T, T, T, T, T, T, T, T)>(ref from);

        /// <summary>
        /// Performs an implicit conversion from <see cref="ValueTuple{T, T}"/> to <see cref="UnmanagedTupleX13{T}"/>.
        /// </summary>
        /// <param name="from">The converting <see cref="ValueTuple{T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12}"/>.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static implicit operator UnmanagedTupleX13<T>((T, T, T, T, T, T, T, T, T, T, T, T, T) from)
            => Unsafe.As<(T, T, T, T, T, T, T, T, T, T, T, T, T), UnmanagedTupleX13<T>>(ref from);
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly partial struct UnmanagedTupleX14<T> where T:unmanaged
    {
        private readonly T i0, i1, i2, i3, i4, i5, i6, i7, i8, i9, i10, i11, i12, i13;
        /// <summary>
        /// The item No.0.
        /// </summary>
        public T Item0
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX14<T>, T>(ref Unsafe.AsRef(in this)), 0);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX14<T>, T>(ref Unsafe.AsRef(in this)), 0) = value;
        }

        /// <summary>
        /// The item No.1.
        /// </summary>
        public T Item1
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX14<T>, T>(ref Unsafe.AsRef(in this)), 1);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX14<T>, T>(ref Unsafe.AsRef(in this)), 1) = value;
        }

        /// <summary>
        /// The item No.2.
        /// </summary>
        public T Item2
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX14<T>, T>(ref Unsafe.AsRef(in this)), 2);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX14<T>, T>(ref Unsafe.AsRef(in this)), 2) = value;
        }

        /// <summary>
        /// The item No.3.
        /// </summary>
        public T Item3
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX14<T>, T>(ref Unsafe.AsRef(in this)), 3);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX14<T>, T>(ref Unsafe.AsRef(in this)), 3) = value;
        }

        /// <summary>
        /// The item No.4.
        /// </summary>
        public T Item4
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX14<T>, T>(ref Unsafe.AsRef(in this)), 4);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX14<T>, T>(ref Unsafe.AsRef(in this)), 4) = value;
        }

        /// <summary>
        /// The item No.5.
        /// </summary>
        public T Item5
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX14<T>, T>(ref Unsafe.AsRef(in this)), 5);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX14<T>, T>(ref Unsafe.AsRef(in this)), 5) = value;
        }

        /// <summary>
        /// The item No.6.
        /// </summary>
        public T Item6
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX14<T>, T>(ref Unsafe.AsRef(in this)), 6);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX14<T>, T>(ref Unsafe.AsRef(in this)), 6) = value;
        }

        /// <summary>
        /// The item No.7.
        /// </summary>
        public T Item7
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX14<T>, T>(ref Unsafe.AsRef(in this)), 7);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX14<T>, T>(ref Unsafe.AsRef(in this)), 7) = value;
        }

        /// <summary>
        /// The item No.8.
        /// </summary>
        public T Item8
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX14<T>, T>(ref Unsafe.AsRef(in this)), 8);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX14<T>, T>(ref Unsafe.AsRef(in this)), 8) = value;
        }

        /// <summary>
        /// The item No.9.
        /// </summary>
        public T Item9
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX14<T>, T>(ref Unsafe.AsRef(in this)), 9);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX14<T>, T>(ref Unsafe.AsRef(in this)), 9) = value;
        }

        /// <summary>
        /// The item No.10.
        /// </summary>
        public T Item10
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX14<T>, T>(ref Unsafe.AsRef(in this)), 10);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX14<T>, T>(ref Unsafe.AsRef(in this)), 10) = value;
        }

        /// <summary>
        /// The item No.11.
        /// </summary>
        public T Item11
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX14<T>, T>(ref Unsafe.AsRef(in this)), 11);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX14<T>, T>(ref Unsafe.AsRef(in this)), 11) = value;
        }

        /// <summary>
        /// The item No.12.
        /// </summary>
        public T Item12
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX14<T>, T>(ref Unsafe.AsRef(in this)), 12);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX14<T>, T>(ref Unsafe.AsRef(in this)), 12) = value;
        }

        /// <summary>
        /// The item No.13.
        /// </summary>
        public T Item13
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX14<T>, T>(ref Unsafe.AsRef(in this)), 13);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX14<T>, T>(ref Unsafe.AsRef(in this)), 13) = value;
        }

        /// <summary>
        /// Initializes this <see cref="UnmanagedTupleX14{T}"/> from values.
        /// </summary>
        /// <param name="v0">The value No.0.</param>
        /// <param name="v1">The value No.1.</param>
        /// <param name="v2">The value No.2.</param>
        /// <param name="v3">The value No.3.</param>
        /// <param name="v4">The value No.4.</param>
        /// <param name="v5">The value No.5.</param>
        /// <param name="v6">The value No.6.</param>
        /// <param name="v7">The value No.7.</param>
        /// <param name="v8">The value No.8.</param>
        /// <param name="v9">The value No.9.</param>
        /// <param name="v10">The value No.10.</param>
        /// <param name="v11">The value No.11.</param>
        /// <param name="v12">The value No.12.</param>
        /// <param name="v13">The value No.13.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public UnmanagedTupleX14(T v0, T v1, T v2, T v3, T v4, T v5, T v6, T v7, T v8, T v9, T v10, T v11, T v12, T v13)
        {
            i0 = v0;
            i1 = v1;
            i2 = v2;
            i3 = v3;
            i4 = v4;
            i5 = v5;
            i6 = v6;
            i7 = v7;
            i8 = v8;
            i9 = v9;
            i10 = v10;
            i11 = v11;
            i12 = v12;
            i13 = v13;
        }

        /// <summary>
        /// Deconstructs this <see cref="UnmanagedTupleX14{T}"/> to values.
        /// </summary>
        /// <param name="v0">The output value No.0.</param>
        /// <param name="v1">The output value No.1.</param>
        /// <param name="v2">The output value No.2.</param>
        /// <param name="v3">The output value No.3.</param>
        /// <param name="v4">The output value No.4.</param>
        /// <param name="v5">The output value No.5.</param>
        /// <param name="v6">The output value No.6.</param>
        /// <param name="v7">The output value No.7.</param>
        /// <param name="v8">The output value No.8.</param>
        /// <param name="v9">The output value No.9.</param>
        /// <param name="v10">The output value No.10.</param>
        /// <param name="v11">The output value No.11.</param>
        /// <param name="v12">The output value No.12.</param>
        /// <param name="v13">The output value No.13.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public void Deconstruct(out T v0, out T v1, out T v2, out T v3, out T v4, out T v5, out T v6, out T v7, out T v8, out T v9, out T v10, out T v11, out T v12, out T v13)
        {
            v0 = Item0;
            v1 = Item1;
            v2 = Item2;
            v3 = Item3;
            v4 = Item4;
            v5 = Item5;
            v6 = Item6;
            v7 = Item7;
            v8 = Item8;
            v9 = Item9;
            v10 = Item10;
            v11 = Item11;
            v12 = Item12;
            v13 = Item13;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="UnmanagedTupleX14{T}"/> to <see cref="ValueTuple{T, T}"/>.
        /// </summary>
        /// <param name="from">The converting <see cref="UnmanagedTupleX14{T}"/>.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static implicit operator (T, T, T, T, T, T, T, T, T, T, T, T, T, T)(UnmanagedTupleX14<T> from)
            => Unsafe.As<UnmanagedTupleX14<T>, (T, T, T, T, T, T, T, T, T, T, T, T, T, T)>(ref from);

        /// <summary>
        /// Performs an implicit conversion from <see cref="ValueTuple{T, T}"/> to <see cref="UnmanagedTupleX14{T}"/>.
        /// </summary>
        /// <param name="from">The converting <see cref="ValueTuple{T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13}"/>.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static implicit operator UnmanagedTupleX14<T>((T, T, T, T, T, T, T, T, T, T, T, T, T, T) from)
            => Unsafe.As<(T, T, T, T, T, T, T, T, T, T, T, T, T, T), UnmanagedTupleX14<T>>(ref from);
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly partial struct UnmanagedTupleX15<T> where T:unmanaged
    {
        private readonly T i0, i1, i2, i3, i4, i5, i6, i7, i8, i9, i10, i11, i12, i13, i14;
        /// <summary>
        /// The item No.0.
        /// </summary>
        public T Item0
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX15<T>, T>(ref Unsafe.AsRef(in this)), 0);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX15<T>, T>(ref Unsafe.AsRef(in this)), 0) = value;
        }

        /// <summary>
        /// The item No.1.
        /// </summary>
        public T Item1
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX15<T>, T>(ref Unsafe.AsRef(in this)), 1);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX15<T>, T>(ref Unsafe.AsRef(in this)), 1) = value;
        }

        /// <summary>
        /// The item No.2.
        /// </summary>
        public T Item2
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX15<T>, T>(ref Unsafe.AsRef(in this)), 2);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX15<T>, T>(ref Unsafe.AsRef(in this)), 2) = value;
        }

        /// <summary>
        /// The item No.3.
        /// </summary>
        public T Item3
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX15<T>, T>(ref Unsafe.AsRef(in this)), 3);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX15<T>, T>(ref Unsafe.AsRef(in this)), 3) = value;
        }

        /// <summary>
        /// The item No.4.
        /// </summary>
        public T Item4
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX15<T>, T>(ref Unsafe.AsRef(in this)), 4);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX15<T>, T>(ref Unsafe.AsRef(in this)), 4) = value;
        }

        /// <summary>
        /// The item No.5.
        /// </summary>
        public T Item5
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX15<T>, T>(ref Unsafe.AsRef(in this)), 5);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX15<T>, T>(ref Unsafe.AsRef(in this)), 5) = value;
        }

        /// <summary>
        /// The item No.6.
        /// </summary>
        public T Item6
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX15<T>, T>(ref Unsafe.AsRef(in this)), 6);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX15<T>, T>(ref Unsafe.AsRef(in this)), 6) = value;
        }

        /// <summary>
        /// The item No.7.
        /// </summary>
        public T Item7
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX15<T>, T>(ref Unsafe.AsRef(in this)), 7);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX15<T>, T>(ref Unsafe.AsRef(in this)), 7) = value;
        }

        /// <summary>
        /// The item No.8.
        /// </summary>
        public T Item8
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX15<T>, T>(ref Unsafe.AsRef(in this)), 8);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX15<T>, T>(ref Unsafe.AsRef(in this)), 8) = value;
        }

        /// <summary>
        /// The item No.9.
        /// </summary>
        public T Item9
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX15<T>, T>(ref Unsafe.AsRef(in this)), 9);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX15<T>, T>(ref Unsafe.AsRef(in this)), 9) = value;
        }

        /// <summary>
        /// The item No.10.
        /// </summary>
        public T Item10
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX15<T>, T>(ref Unsafe.AsRef(in this)), 10);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX15<T>, T>(ref Unsafe.AsRef(in this)), 10) = value;
        }

        /// <summary>
        /// The item No.11.
        /// </summary>
        public T Item11
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX15<T>, T>(ref Unsafe.AsRef(in this)), 11);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX15<T>, T>(ref Unsafe.AsRef(in this)), 11) = value;
        }

        /// <summary>
        /// The item No.12.
        /// </summary>
        public T Item12
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX15<T>, T>(ref Unsafe.AsRef(in this)), 12);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX15<T>, T>(ref Unsafe.AsRef(in this)), 12) = value;
        }

        /// <summary>
        /// The item No.13.
        /// </summary>
        public T Item13
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX15<T>, T>(ref Unsafe.AsRef(in this)), 13);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX15<T>, T>(ref Unsafe.AsRef(in this)), 13) = value;
        }

        /// <summary>
        /// The item No.14.
        /// </summary>
        public T Item14
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX15<T>, T>(ref Unsafe.AsRef(in this)), 14);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX15<T>, T>(ref Unsafe.AsRef(in this)), 14) = value;
        }

        /// <summary>
        /// Initializes this <see cref="UnmanagedTupleX15{T}"/> from values.
        /// </summary>
        /// <param name="v0">The value No.0.</param>
        /// <param name="v1">The value No.1.</param>
        /// <param name="v2">The value No.2.</param>
        /// <param name="v3">The value No.3.</param>
        /// <param name="v4">The value No.4.</param>
        /// <param name="v5">The value No.5.</param>
        /// <param name="v6">The value No.6.</param>
        /// <param name="v7">The value No.7.</param>
        /// <param name="v8">The value No.8.</param>
        /// <param name="v9">The value No.9.</param>
        /// <param name="v10">The value No.10.</param>
        /// <param name="v11">The value No.11.</param>
        /// <param name="v12">The value No.12.</param>
        /// <param name="v13">The value No.13.</param>
        /// <param name="v14">The value No.14.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public UnmanagedTupleX15(T v0, T v1, T v2, T v3, T v4, T v5, T v6, T v7, T v8, T v9, T v10, T v11, T v12, T v13, T v14)
        {
            i0 = v0;
            i1 = v1;
            i2 = v2;
            i3 = v3;
            i4 = v4;
            i5 = v5;
            i6 = v6;
            i7 = v7;
            i8 = v8;
            i9 = v9;
            i10 = v10;
            i11 = v11;
            i12 = v12;
            i13 = v13;
            i14 = v14;
        }

        /// <summary>
        /// Deconstructs this <see cref="UnmanagedTupleX15{T}"/> to values.
        /// </summary>
        /// <param name="v0">The output value No.0.</param>
        /// <param name="v1">The output value No.1.</param>
        /// <param name="v2">The output value No.2.</param>
        /// <param name="v3">The output value No.3.</param>
        /// <param name="v4">The output value No.4.</param>
        /// <param name="v5">The output value No.5.</param>
        /// <param name="v6">The output value No.6.</param>
        /// <param name="v7">The output value No.7.</param>
        /// <param name="v8">The output value No.8.</param>
        /// <param name="v9">The output value No.9.</param>
        /// <param name="v10">The output value No.10.</param>
        /// <param name="v11">The output value No.11.</param>
        /// <param name="v12">The output value No.12.</param>
        /// <param name="v13">The output value No.13.</param>
        /// <param name="v14">The output value No.14.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public void Deconstruct(out T v0, out T v1, out T v2, out T v3, out T v4, out T v5, out T v6, out T v7, out T v8, out T v9, out T v10, out T v11, out T v12, out T v13, out T v14)
        {
            v0 = Item0;
            v1 = Item1;
            v2 = Item2;
            v3 = Item3;
            v4 = Item4;
            v5 = Item5;
            v6 = Item6;
            v7 = Item7;
            v8 = Item8;
            v9 = Item9;
            v10 = Item10;
            v11 = Item11;
            v12 = Item12;
            v13 = Item13;
            v14 = Item14;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="UnmanagedTupleX15{T}"/> to <see cref="ValueTuple{T, T}"/>.
        /// </summary>
        /// <param name="from">The converting <see cref="UnmanagedTupleX15{T}"/>.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static implicit operator (T, T, T, T, T, T, T, T, T, T, T, T, T, T, T)(UnmanagedTupleX15<T> from)
            => Unsafe.As<UnmanagedTupleX15<T>, (T, T, T, T, T, T, T, T, T, T, T, T, T, T, T)>(ref from);

        /// <summary>
        /// Performs an implicit conversion from <see cref="ValueTuple{T, T}"/> to <see cref="UnmanagedTupleX15{T}"/>.
        /// </summary>
        /// <param name="from">The converting <see cref="ValueTuple{T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14}"/>.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static implicit operator UnmanagedTupleX15<T>((T, T, T, T, T, T, T, T, T, T, T, T, T, T, T) from)
            => Unsafe.As<(T, T, T, T, T, T, T, T, T, T, T, T, T, T, T), UnmanagedTupleX15<T>>(ref from);
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly partial struct UnmanagedTupleX16<T> where T:unmanaged
    {
        private readonly T i0, i1, i2, i3, i4, i5, i6, i7, i8, i9, i10, i11, i12, i13, i14, i15;
        /// <summary>
        /// The item No.0.
        /// </summary>
        public T Item0
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX16<T>, T>(ref Unsafe.AsRef(in this)), 0);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX16<T>, T>(ref Unsafe.AsRef(in this)), 0) = value;
        }

        /// <summary>
        /// The item No.1.
        /// </summary>
        public T Item1
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX16<T>, T>(ref Unsafe.AsRef(in this)), 1);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX16<T>, T>(ref Unsafe.AsRef(in this)), 1) = value;
        }

        /// <summary>
        /// The item No.2.
        /// </summary>
        public T Item2
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX16<T>, T>(ref Unsafe.AsRef(in this)), 2);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX16<T>, T>(ref Unsafe.AsRef(in this)), 2) = value;
        }

        /// <summary>
        /// The item No.3.
        /// </summary>
        public T Item3
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX16<T>, T>(ref Unsafe.AsRef(in this)), 3);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX16<T>, T>(ref Unsafe.AsRef(in this)), 3) = value;
        }

        /// <summary>
        /// The item No.4.
        /// </summary>
        public T Item4
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX16<T>, T>(ref Unsafe.AsRef(in this)), 4);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX16<T>, T>(ref Unsafe.AsRef(in this)), 4) = value;
        }

        /// <summary>
        /// The item No.5.
        /// </summary>
        public T Item5
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX16<T>, T>(ref Unsafe.AsRef(in this)), 5);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX16<T>, T>(ref Unsafe.AsRef(in this)), 5) = value;
        }

        /// <summary>
        /// The item No.6.
        /// </summary>
        public T Item6
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX16<T>, T>(ref Unsafe.AsRef(in this)), 6);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX16<T>, T>(ref Unsafe.AsRef(in this)), 6) = value;
        }

        /// <summary>
        /// The item No.7.
        /// </summary>
        public T Item7
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX16<T>, T>(ref Unsafe.AsRef(in this)), 7);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX16<T>, T>(ref Unsafe.AsRef(in this)), 7) = value;
        }

        /// <summary>
        /// The item No.8.
        /// </summary>
        public T Item8
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX16<T>, T>(ref Unsafe.AsRef(in this)), 8);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX16<T>, T>(ref Unsafe.AsRef(in this)), 8) = value;
        }

        /// <summary>
        /// The item No.9.
        /// </summary>
        public T Item9
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX16<T>, T>(ref Unsafe.AsRef(in this)), 9);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX16<T>, T>(ref Unsafe.AsRef(in this)), 9) = value;
        }

        /// <summary>
        /// The item No.10.
        /// </summary>
        public T Item10
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX16<T>, T>(ref Unsafe.AsRef(in this)), 10);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX16<T>, T>(ref Unsafe.AsRef(in this)), 10) = value;
        }

        /// <summary>
        /// The item No.11.
        /// </summary>
        public T Item11
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX16<T>, T>(ref Unsafe.AsRef(in this)), 11);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX16<T>, T>(ref Unsafe.AsRef(in this)), 11) = value;
        }

        /// <summary>
        /// The item No.12.
        /// </summary>
        public T Item12
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX16<T>, T>(ref Unsafe.AsRef(in this)), 12);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX16<T>, T>(ref Unsafe.AsRef(in this)), 12) = value;
        }

        /// <summary>
        /// The item No.13.
        /// </summary>
        public T Item13
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX16<T>, T>(ref Unsafe.AsRef(in this)), 13);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX16<T>, T>(ref Unsafe.AsRef(in this)), 13) = value;
        }

        /// <summary>
        /// The item No.14.
        /// </summary>
        public T Item14
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX16<T>, T>(ref Unsafe.AsRef(in this)), 14);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX16<T>, T>(ref Unsafe.AsRef(in this)), 14) = value;
        }

        /// <summary>
        /// The item No.15.
        /// </summary>
        public T Item15
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX16<T>, T>(ref Unsafe.AsRef(in this)), 15);
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            set => Unsafe.Add(ref Unsafe.As<UnmanagedTupleX16<T>, T>(ref Unsafe.AsRef(in this)), 15) = value;
        }

        /// <summary>
        /// Initializes this <see cref="UnmanagedTupleX16{T}"/> from values.
        /// </summary>
        /// <param name="v0">The value No.0.</param>
        /// <param name="v1">The value No.1.</param>
        /// <param name="v2">The value No.2.</param>
        /// <param name="v3">The value No.3.</param>
        /// <param name="v4">The value No.4.</param>
        /// <param name="v5">The value No.5.</param>
        /// <param name="v6">The value No.6.</param>
        /// <param name="v7">The value No.7.</param>
        /// <param name="v8">The value No.8.</param>
        /// <param name="v9">The value No.9.</param>
        /// <param name="v10">The value No.10.</param>
        /// <param name="v11">The value No.11.</param>
        /// <param name="v12">The value No.12.</param>
        /// <param name="v13">The value No.13.</param>
        /// <param name="v14">The value No.14.</param>
        /// <param name="v15">The value No.15.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public UnmanagedTupleX16(T v0, T v1, T v2, T v3, T v4, T v5, T v6, T v7, T v8, T v9, T v10, T v11, T v12, T v13, T v14, T v15)
        {
            i0 = v0;
            i1 = v1;
            i2 = v2;
            i3 = v3;
            i4 = v4;
            i5 = v5;
            i6 = v6;
            i7 = v7;
            i8 = v8;
            i9 = v9;
            i10 = v10;
            i11 = v11;
            i12 = v12;
            i13 = v13;
            i14 = v14;
            i15 = v15;
        }

        /// <summary>
        /// Deconstructs this <see cref="UnmanagedTupleX16{T}"/> to values.
        /// </summary>
        /// <param name="v0">The output value No.0.</param>
        /// <param name="v1">The output value No.1.</param>
        /// <param name="v2">The output value No.2.</param>
        /// <param name="v3">The output value No.3.</param>
        /// <param name="v4">The output value No.4.</param>
        /// <param name="v5">The output value No.5.</param>
        /// <param name="v6">The output value No.6.</param>
        /// <param name="v7">The output value No.7.</param>
        /// <param name="v8">The output value No.8.</param>
        /// <param name="v9">The output value No.9.</param>
        /// <param name="v10">The output value No.10.</param>
        /// <param name="v11">The output value No.11.</param>
        /// <param name="v12">The output value No.12.</param>
        /// <param name="v13">The output value No.13.</param>
        /// <param name="v14">The output value No.14.</param>
        /// <param name="v15">The output value No.15.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public void Deconstruct(out T v0, out T v1, out T v2, out T v3, out T v4, out T v5, out T v6, out T v7, out T v8, out T v9, out T v10, out T v11, out T v12, out T v13, out T v14, out T v15)
        {
            v0 = Item0;
            v1 = Item1;
            v2 = Item2;
            v3 = Item3;
            v4 = Item4;
            v5 = Item5;
            v6 = Item6;
            v7 = Item7;
            v8 = Item8;
            v9 = Item9;
            v10 = Item10;
            v11 = Item11;
            v12 = Item12;
            v13 = Item13;
            v14 = Item14;
            v15 = Item15;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="UnmanagedTupleX16{T}"/> to <see cref="ValueTuple{T, T}"/>.
        /// </summary>
        /// <param name="from">The converting <see cref="UnmanagedTupleX16{T}"/>.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static implicit operator (T, T, T, T, T, T, T, T, T, T, T, T, T, T, T, T)(UnmanagedTupleX16<T> from)
            => Unsafe.As<UnmanagedTupleX16<T>, (T, T, T, T, T, T, T, T, T, T, T, T, T, T, T, T)>(ref from);

        /// <summary>
        /// Performs an implicit conversion from <see cref="ValueTuple{T, T}"/> to <see cref="UnmanagedTupleX16{T}"/>.
        /// </summary>
        /// <param name="from">The converting <see cref="ValueTuple{T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15}"/>.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static implicit operator UnmanagedTupleX16<T>((T, T, T, T, T, T, T, T, T, T, T, T, T, T, T, T) from)
            => Unsafe.As<(T, T, T, T, T, T, T, T, T, T, T, T, T, T, T, T), UnmanagedTupleX16<T>>(ref from);
    }
}
