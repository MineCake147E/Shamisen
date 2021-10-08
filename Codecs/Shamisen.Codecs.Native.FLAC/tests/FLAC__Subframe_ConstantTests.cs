using System;

using NUnit.Framework;
using System.Runtime.InteropServices;

namespace Shamisen.Codecs.Native.Flac.LibFLAC.UnitTests
{
    /// <summary>Provides validation of the <see cref="FLAC__Subframe_Constant" /> struct.</summary>
    public static unsafe class FLAC__Subframe_ConstantTests
    {
        /// <summary>Validates that the <see cref="FLAC__Subframe_Constant" /> struct is blittable.</summary>
        [Test]
        public static void IsBlittableTest()
        {
            Assert.That(Marshal.SizeOf<FLAC__Subframe_Constant>(), Is.EqualTo(sizeof(FLAC__Subframe_Constant)));
        }

        /// <summary>Validates that the <see cref="FLAC__Subframe_Constant" /> struct has the right <see cref="LayoutKind" />.</summary>
        [Test]
        public static void IsLayoutSequentialTest()
        {
            Assert.That(typeof(FLAC__Subframe_Constant).IsLayoutSequential, Is.True);
        }

        /// <summary>Validates that the <see cref="FLAC__Subframe_Constant" /> struct has the correct size.</summary>
        [Test]
        public static void SizeOfTest()
        {
            Assert.That(sizeof(FLAC__Subframe_Constant), Is.EqualTo(4));
        }
    }
}
