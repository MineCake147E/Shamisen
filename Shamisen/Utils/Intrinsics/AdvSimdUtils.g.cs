﻿// <auto-generated />
#if NET5_0_OR_GREATER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Utils.Intrinsics
{
    public static partial class AdvSimdUtils
    {
        public static partial class Arm64
        {
            /// <summary>
            /// Executes <see cref="AdvSimd.Arm64.StorePair(float*, Vector128{float}, Vector128{float})" /> when in Release mode, but falls back in Debug mode.
            /// </summary>
            /// <param name="address">The target address.</param>
            /// <param name="value1">The first value to write.</param>
            /// <param name="value2">The second value to write.</param>
            public static void StorePair(ref float address, Vector128<float> value1, Vector128<float> value2)
            {
#if DEBUG
                Unsafe.As<float, Vector128<float>>(ref address) = value1;
                Unsafe.Add(ref Unsafe.As<float, Vector128<float>>(ref address), 1) = value2;
#else
                unsafe
                {
                    AdvSimd.Arm64.StorePair((float*)Unsafe.AsPointer(ref address), value1, value2);
                }
#endif
            }

            /// <summary>
            /// Executes <see cref="AdvSimd.Arm64.StorePair(float*, Vector64{float}, Vector64{float})" /> when in Release mode, but falls back in Debug mode.
            /// </summary>
            /// <param name="address">The target address.</param>
            /// <param name="value1">The first value to write.</param>
            /// <param name="value2">The second value to write.</param>
            public static void StorePair(ref float address, Vector64<float> value1, Vector64<float> value2)
            {
#if DEBUG
                Unsafe.As<float, Vector64<float>>(ref address) = value1;
                Unsafe.Add(ref Unsafe.As<float, Vector64<float>>(ref address), 1) = value2;
#else
                unsafe
                {
                    AdvSimd.Arm64.StorePair((float*)Unsafe.AsPointer(ref address), value1, value2);
                }
#endif
            }

            /// <summary>
            /// Executes <see cref="AdvSimd.Arm64.StorePair(double*, Vector128{double}, Vector128{double})" /> when in Release mode, but falls back in Debug mode.
            /// </summary>
            /// <param name="address">The target address.</param>
            /// <param name="value1">The first value to write.</param>
            /// <param name="value2">The second value to write.</param>
            public static void StorePair(ref double address, Vector128<double> value1, Vector128<double> value2)
            {
#if DEBUG
                Unsafe.As<double, Vector128<double>>(ref address) = value1;
                Unsafe.Add(ref Unsafe.As<double, Vector128<double>>(ref address), 1) = value2;
#else
                unsafe
                {
                    AdvSimd.Arm64.StorePair((double*)Unsafe.AsPointer(ref address), value1, value2);
                }
#endif
            }

            /// <summary>
            /// Executes <see cref="AdvSimd.Arm64.StorePair(double*, Vector64{double}, Vector64{double})" /> when in Release mode, but falls back in Debug mode.
            /// </summary>
            /// <param name="address">The target address.</param>
            /// <param name="value1">The first value to write.</param>
            /// <param name="value2">The second value to write.</param>
            public static void StorePair(ref double address, Vector64<double> value1, Vector64<double> value2)
            {
#if DEBUG
                Unsafe.As<double, Vector64<double>>(ref address) = value1;
                Unsafe.Add(ref Unsafe.As<double, Vector64<double>>(ref address), 1) = value2;
#else
                unsafe
                {
                    AdvSimd.Arm64.StorePair((double*)Unsafe.AsPointer(ref address), value1, value2);
                }
#endif
            }

            /// <summary>
            /// Executes <see cref="AdvSimd.Arm64.StorePair(byte*, Vector128{byte}, Vector128{byte})" /> when in Release mode, but falls back in Debug mode.
            /// </summary>
            /// <param name="address">The target address.</param>
            /// <param name="value1">The first value to write.</param>
            /// <param name="value2">The second value to write.</param>
            public static void StorePair(ref byte address, Vector128<byte> value1, Vector128<byte> value2)
            {
#if DEBUG
                Unsafe.As<byte, Vector128<byte>>(ref address) = value1;
                Unsafe.Add(ref Unsafe.As<byte, Vector128<byte>>(ref address), 1) = value2;
#else
                unsafe
                {
                    AdvSimd.Arm64.StorePair((byte*)Unsafe.AsPointer(ref address), value1, value2);
                }
#endif
            }

            /// <summary>
            /// Executes <see cref="AdvSimd.Arm64.StorePair(byte*, Vector64{byte}, Vector64{byte})" /> when in Release mode, but falls back in Debug mode.
            /// </summary>
            /// <param name="address">The target address.</param>
            /// <param name="value1">The first value to write.</param>
            /// <param name="value2">The second value to write.</param>
            public static void StorePair(ref byte address, Vector64<byte> value1, Vector64<byte> value2)
            {
#if DEBUG
                Unsafe.As<byte, Vector64<byte>>(ref address) = value1;
                Unsafe.Add(ref Unsafe.As<byte, Vector64<byte>>(ref address), 1) = value2;
#else
                unsafe
                {
                    AdvSimd.Arm64.StorePair((byte*)Unsafe.AsPointer(ref address), value1, value2);
                }
#endif
            }

            /// <summary>
            /// Executes <see cref="AdvSimd.Arm64.StorePair(ushort*, Vector128{ushort}, Vector128{ushort})" /> when in Release mode, but falls back in Debug mode.
            /// </summary>
            /// <param name="address">The target address.</param>
            /// <param name="value1">The first value to write.</param>
            /// <param name="value2">The second value to write.</param>
            public static void StorePair(ref ushort address, Vector128<ushort> value1, Vector128<ushort> value2)
            {
#if DEBUG
                Unsafe.As<ushort, Vector128<ushort>>(ref address) = value1;
                Unsafe.Add(ref Unsafe.As<ushort, Vector128<ushort>>(ref address), 1) = value2;
#else
                unsafe
                {
                    AdvSimd.Arm64.StorePair((ushort*)Unsafe.AsPointer(ref address), value1, value2);
                }
#endif
            }

            /// <summary>
            /// Executes <see cref="AdvSimd.Arm64.StorePair(ushort*, Vector64{ushort}, Vector64{ushort})" /> when in Release mode, but falls back in Debug mode.
            /// </summary>
            /// <param name="address">The target address.</param>
            /// <param name="value1">The first value to write.</param>
            /// <param name="value2">The second value to write.</param>
            public static void StorePair(ref ushort address, Vector64<ushort> value1, Vector64<ushort> value2)
            {
#if DEBUG
                Unsafe.As<ushort, Vector64<ushort>>(ref address) = value1;
                Unsafe.Add(ref Unsafe.As<ushort, Vector64<ushort>>(ref address), 1) = value2;
#else
                unsafe
                {
                    AdvSimd.Arm64.StorePair((ushort*)Unsafe.AsPointer(ref address), value1, value2);
                }
#endif
            }

            /// <summary>
            /// Executes <see cref="AdvSimd.Arm64.StorePair(uint*, Vector128{uint}, Vector128{uint})" /> when in Release mode, but falls back in Debug mode.
            /// </summary>
            /// <param name="address">The target address.</param>
            /// <param name="value1">The first value to write.</param>
            /// <param name="value2">The second value to write.</param>
            public static void StorePair(ref uint address, Vector128<uint> value1, Vector128<uint> value2)
            {
#if DEBUG
                Unsafe.As<uint, Vector128<uint>>(ref address) = value1;
                Unsafe.Add(ref Unsafe.As<uint, Vector128<uint>>(ref address), 1) = value2;
#else
                unsafe
                {
                    AdvSimd.Arm64.StorePair((uint*)Unsafe.AsPointer(ref address), value1, value2);
                }
#endif
            }

            /// <summary>
            /// Executes <see cref="AdvSimd.Arm64.StorePair(uint*, Vector64{uint}, Vector64{uint})" /> when in Release mode, but falls back in Debug mode.
            /// </summary>
            /// <param name="address">The target address.</param>
            /// <param name="value1">The first value to write.</param>
            /// <param name="value2">The second value to write.</param>
            public static void StorePair(ref uint address, Vector64<uint> value1, Vector64<uint> value2)
            {
#if DEBUG
                Unsafe.As<uint, Vector64<uint>>(ref address) = value1;
                Unsafe.Add(ref Unsafe.As<uint, Vector64<uint>>(ref address), 1) = value2;
#else
                unsafe
                {
                    AdvSimd.Arm64.StorePair((uint*)Unsafe.AsPointer(ref address), value1, value2);
                }
#endif
            }

            /// <summary>
            /// Executes <see cref="AdvSimd.Arm64.StorePair(ulong*, Vector128{ulong}, Vector128{ulong})" /> when in Release mode, but falls back in Debug mode.
            /// </summary>
            /// <param name="address">The target address.</param>
            /// <param name="value1">The first value to write.</param>
            /// <param name="value2">The second value to write.</param>
            public static void StorePair(ref ulong address, Vector128<ulong> value1, Vector128<ulong> value2)
            {
#if DEBUG
                Unsafe.As<ulong, Vector128<ulong>>(ref address) = value1;
                Unsafe.Add(ref Unsafe.As<ulong, Vector128<ulong>>(ref address), 1) = value2;
#else
                unsafe
                {
                    AdvSimd.Arm64.StorePair((ulong*)Unsafe.AsPointer(ref address), value1, value2);
                }
#endif
            }

            /// <summary>
            /// Executes <see cref="AdvSimd.Arm64.StorePair(ulong*, Vector64{ulong}, Vector64{ulong})" /> when in Release mode, but falls back in Debug mode.
            /// </summary>
            /// <param name="address">The target address.</param>
            /// <param name="value1">The first value to write.</param>
            /// <param name="value2">The second value to write.</param>
            public static void StorePair(ref ulong address, Vector64<ulong> value1, Vector64<ulong> value2)
            {
#if DEBUG
                Unsafe.As<ulong, Vector64<ulong>>(ref address) = value1;
                Unsafe.Add(ref Unsafe.As<ulong, Vector64<ulong>>(ref address), 1) = value2;
#else
                unsafe
                {
                    AdvSimd.Arm64.StorePair((ulong*)Unsafe.AsPointer(ref address), value1, value2);
                }
#endif
            }

            /// <summary>
            /// Executes <see cref="AdvSimd.Arm64.StorePair(sbyte*, Vector128{sbyte}, Vector128{sbyte})" /> when in Release mode, but falls back in Debug mode.
            /// </summary>
            /// <param name="address">The target address.</param>
            /// <param name="value1">The first value to write.</param>
            /// <param name="value2">The second value to write.</param>
            public static void StorePair(ref sbyte address, Vector128<sbyte> value1, Vector128<sbyte> value2)
            {
#if DEBUG
                Unsafe.As<sbyte, Vector128<sbyte>>(ref address) = value1;
                Unsafe.Add(ref Unsafe.As<sbyte, Vector128<sbyte>>(ref address), 1) = value2;
#else
                unsafe
                {
                    AdvSimd.Arm64.StorePair((sbyte*)Unsafe.AsPointer(ref address), value1, value2);
                }
#endif
            }

            /// <summary>
            /// Executes <see cref="AdvSimd.Arm64.StorePair(sbyte*, Vector64{sbyte}, Vector64{sbyte})" /> when in Release mode, but falls back in Debug mode.
            /// </summary>
            /// <param name="address">The target address.</param>
            /// <param name="value1">The first value to write.</param>
            /// <param name="value2">The second value to write.</param>
            public static void StorePair(ref sbyte address, Vector64<sbyte> value1, Vector64<sbyte> value2)
            {
#if DEBUG
                Unsafe.As<sbyte, Vector64<sbyte>>(ref address) = value1;
                Unsafe.Add(ref Unsafe.As<sbyte, Vector64<sbyte>>(ref address), 1) = value2;
#else
                unsafe
                {
                    AdvSimd.Arm64.StorePair((sbyte*)Unsafe.AsPointer(ref address), value1, value2);
                }
#endif
            }

            /// <summary>
            /// Executes <see cref="AdvSimd.Arm64.StorePair(short*, Vector128{short}, Vector128{short})" /> when in Release mode, but falls back in Debug mode.
            /// </summary>
            /// <param name="address">The target address.</param>
            /// <param name="value1">The first value to write.</param>
            /// <param name="value2">The second value to write.</param>
            public static void StorePair(ref short address, Vector128<short> value1, Vector128<short> value2)
            {
#if DEBUG
                Unsafe.As<short, Vector128<short>>(ref address) = value1;
                Unsafe.Add(ref Unsafe.As<short, Vector128<short>>(ref address), 1) = value2;
#else
                unsafe
                {
                    AdvSimd.Arm64.StorePair((short*)Unsafe.AsPointer(ref address), value1, value2);
                }
#endif
            }

            /// <summary>
            /// Executes <see cref="AdvSimd.Arm64.StorePair(short*, Vector64{short}, Vector64{short})" /> when in Release mode, but falls back in Debug mode.
            /// </summary>
            /// <param name="address">The target address.</param>
            /// <param name="value1">The first value to write.</param>
            /// <param name="value2">The second value to write.</param>
            public static void StorePair(ref short address, Vector64<short> value1, Vector64<short> value2)
            {
#if DEBUG
                Unsafe.As<short, Vector64<short>>(ref address) = value1;
                Unsafe.Add(ref Unsafe.As<short, Vector64<short>>(ref address), 1) = value2;
#else
                unsafe
                {
                    AdvSimd.Arm64.StorePair((short*)Unsafe.AsPointer(ref address), value1, value2);
                }
#endif
            }

            /// <summary>
            /// Executes <see cref="AdvSimd.Arm64.StorePair(int*, Vector128{int}, Vector128{int})" /> when in Release mode, but falls back in Debug mode.
            /// </summary>
            /// <param name="address">The target address.</param>
            /// <param name="value1">The first value to write.</param>
            /// <param name="value2">The second value to write.</param>
            public static void StorePair(ref int address, Vector128<int> value1, Vector128<int> value2)
            {
#if DEBUG
                Unsafe.As<int, Vector128<int>>(ref address) = value1;
                Unsafe.Add(ref Unsafe.As<int, Vector128<int>>(ref address), 1) = value2;
#else
                unsafe
                {
                    AdvSimd.Arm64.StorePair((int*)Unsafe.AsPointer(ref address), value1, value2);
                }
#endif
            }

            /// <summary>
            /// Executes <see cref="AdvSimd.Arm64.StorePair(int*, Vector64{int}, Vector64{int})" /> when in Release mode, but falls back in Debug mode.
            /// </summary>
            /// <param name="address">The target address.</param>
            /// <param name="value1">The first value to write.</param>
            /// <param name="value2">The second value to write.</param>
            public static void StorePair(ref int address, Vector64<int> value1, Vector64<int> value2)
            {
#if DEBUG
                Unsafe.As<int, Vector64<int>>(ref address) = value1;
                Unsafe.Add(ref Unsafe.As<int, Vector64<int>>(ref address), 1) = value2;
#else
                unsafe
                {
                    AdvSimd.Arm64.StorePair((int*)Unsafe.AsPointer(ref address), value1, value2);
                }
#endif
            }

            /// <summary>
            /// Executes <see cref="AdvSimd.Arm64.StorePair(long*, Vector128{long}, Vector128{long})" /> when in Release mode, but falls back in Debug mode.
            /// </summary>
            /// <param name="address">The target address.</param>
            /// <param name="value1">The first value to write.</param>
            /// <param name="value2">The second value to write.</param>
            public static void StorePair(ref long address, Vector128<long> value1, Vector128<long> value2)
            {
#if DEBUG
                Unsafe.As<long, Vector128<long>>(ref address) = value1;
                Unsafe.Add(ref Unsafe.As<long, Vector128<long>>(ref address), 1) = value2;
#else
                unsafe
                {
                    AdvSimd.Arm64.StorePair((long*)Unsafe.AsPointer(ref address), value1, value2);
                }
#endif
            }

            /// <summary>
            /// Executes <see cref="AdvSimd.Arm64.StorePair(long*, Vector64{long}, Vector64{long})" /> when in Release mode, but falls back in Debug mode.
            /// </summary>
            /// <param name="address">The target address.</param>
            /// <param name="value1">The first value to write.</param>
            /// <param name="value2">The second value to write.</param>
            public static void StorePair(ref long address, Vector64<long> value1, Vector64<long> value2)
            {
#if DEBUG
                Unsafe.As<long, Vector64<long>>(ref address) = value1;
                Unsafe.Add(ref Unsafe.As<long, Vector64<long>>(ref address), 1) = value2;
#else
                unsafe
                {
                    AdvSimd.Arm64.StorePair((long*)Unsafe.AsPointer(ref address), value1, value2);
                }
#endif
            }

        }
    }
}

#endif