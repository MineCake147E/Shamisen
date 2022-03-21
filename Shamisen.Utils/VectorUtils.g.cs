using System;
using System.Numerics;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using Shamisen.Utils.Numerics;

#if NETCOREAPP3_1_OR_GREATER
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
#endif
#if NET5_0_OR_GREATER
using System.Runtime.Intrinsics.Arm;
#endif

namespace Shamisen.Utils
{
    public static partial class VectorUtils
    {
        
        /// <inheritdoc cref="Vector.AsVectorSingle{T}(Vector{T})"/>
        [DebuggerStepThrough]
        public static Vector<Single> AsSingle<T>(this Vector<T> value) where T : struct
            => Vector.AsVectorSingle(value);
        
        /// <inheritdoc cref="Vector.AsVectorDouble{T}(Vector{T})"/>
        [DebuggerStepThrough]
        public static Vector<Double> AsDouble<T>(this Vector<T> value) where T : struct
            => Vector.AsVectorDouble(value);
        
        /// <inheritdoc cref="Vector.AsVectorByte{T}(Vector{T})"/>
        [DebuggerStepThrough]
        public static Vector<Byte> AsByte<T>(this Vector<T> value) where T : struct
            => Vector.AsVectorByte(value);
        
        /// <inheritdoc cref="Vector.AsVectorUInt16{T}(Vector{T})"/>
        [DebuggerStepThrough]
        public static Vector<UInt16> AsUInt16<T>(this Vector<T> value) where T : struct
            => Vector.AsVectorUInt16(value);
        
        /// <inheritdoc cref="Vector.AsVectorUInt32{T}(Vector{T})"/>
        [DebuggerStepThrough]
        public static Vector<UInt32> AsUInt32<T>(this Vector<T> value) where T : struct
            => Vector.AsVectorUInt32(value);
        
        /// <inheritdoc cref="Vector.AsVectorUInt64{T}(Vector{T})"/>
        [DebuggerStepThrough]
        public static Vector<UInt64> AsUInt64<T>(this Vector<T> value) where T : struct
            => Vector.AsVectorUInt64(value);
        
        /// <inheritdoc cref="Vector.AsVectorSByte{T}(Vector{T})"/>
        [DebuggerStepThrough]
        public static Vector<SByte> AsSByte<T>(this Vector<T> value) where T : struct
            => Vector.AsVectorSByte(value);
        
        /// <inheritdoc cref="Vector.AsVectorInt16{T}(Vector{T})"/>
        [DebuggerStepThrough]
        public static Vector<Int16> AsInt16<T>(this Vector<T> value) where T : struct
            => Vector.AsVectorInt16(value);
        
        /// <inheritdoc cref="Vector.AsVectorInt32{T}(Vector{T})"/>
        [DebuggerStepThrough]
        public static Vector<Int32> AsInt32<T>(this Vector<T> value) where T : struct
            => Vector.AsVectorInt32(value);
        
        /// <inheritdoc cref="Vector.AsVectorInt64{T}(Vector{T})"/>
        [DebuggerStepThrough]
        public static Vector<Int64> AsInt64<T>(this Vector<T> value) where T : struct
            => Vector.AsVectorInt64(value);
    }
}
